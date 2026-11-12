namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>Builds compact versions of the existing Shapes levels using the same silhouettes and rules.</summary>
    public static class ShapesCompactGeneration
    {
        public const int MinimumArrows = 40;
        public const int MaximumArrows = 50;
        public const int MaximumSide = 34;
        private const int Attempts = 12;

        /// <summary>Returns the expected authored color for one cell of the compact silhouette.</summary>
        public static Color Sample(int index, int x, int y, int width, int height)
        {
            var tiles = ShapesCompactLayout.Get(index);
            if (width != tiles.GetLength(0) * 2 || height != tiles.GetLength(1) * 2)
                throw new ArgumentException("Unexpected compact Shapes dimensions.");
            return tiles[x / 2, y / 2];
        }

        /// <summary>Creates one validated in-memory candidate without editing the original level.</summary>
        public static PuzzleLevel Create(int index, PuzzleLevel original)
        {
            var tiles = ShapesCompactLayout.Get(index);
            int width = tiles.GetLength(0) * 2, height = tiles.GetLength(1) * 2;
            var mask = new bool[width, height];
            int filled = 0;
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                mask[x, y] = tiles[x / 2, y / 2].a > .5f;
                if (mask[x, y]) filled++;
            }

            var arrows = SelectArrows(index, tiles, width, height, filled / 4);
            DenseCampaignVerification.AssertCoverage(arrows, width, height, mask);
            foreach (var arrow in arrows)
            foreach (var point in arrow.Points)
                if (!ShapesBalancedSource.SameInk(arrow.Color, tiles[point.x / 2, point.y / 2]))
                    throw new InvalidOperationException("Compact path crossed a painted color boundary.");

            var level = ScriptableObject.CreateInstance<PuzzleLevel>();
            try
            {
                level.Author(GameModeId.Shapes, original != null ? original.Title : DenseShapeLibrary.Name(index),
                    width, height, arrows, original != null ? original.WallColors : null,
                    original != null ? original.TurnBudget : 0, original != null ? original.SourceImage : null);
                var solution = PuzzleSolver.Solve(level);
                if (!solution.Solvable) throw new InvalidOperationException(level.Title + ": " + solution.Error);
                return level;
            }
            catch
            {
                UnityEngine.Object.DestroyImmediate(level);
                throw;
            }
        }

        private static List<PuzzleArrowData> SelectArrows(int index, Color[,] tiles, int width, int height, int filled)
        {
            int target = Mathf.Clamp(Mathf.Min(42 + index / 8, filled * 2 / 5), MinimumArrows, MaximumArrows);
            int packetLimit = Mathf.Clamp(Mathf.CeilToInt(filled * 2f / target), 2, 8);
            List<PuzzleArrowData> best = null;
            float bestScore = float.NegativeInfinity;
            for (int attempt = 0; attempt < Attempts; attempt++)
            {
                int seed = 49999 + index * 7919 + attempt * 104729;
                int variedLimit = Mathf.Clamp(packetLimit + attempt % 3 - 1, 2, 8);
                var candidate = ShapesBalancedPaths.Generate(tiles, seed, index, target, variedLimit);
                var metrics = ShapesDifficultyMetrics.Analyze(candidate, width, height);
                if (!metrics.Solvable || metrics.DirectionCount != 4 || metrics.DependencyLayers < 3 ||
                    metrics.InwardRatio < .2f || metrics.InitialLegalRatio > .4f ||
                    candidate.Count < MinimumArrows || candidate.Count > MaximumArrows) continue;
                float score = ShapesBalancedScoring.Final(metrics, index) -
                    Mathf.Abs(candidate.Count - target) * 5f - metrics.ShortArrowRatio * 24f;
                if (score <= bestScore) continue;
                bestScore = score;
                best = candidate;
            }
            if (best == null)
                throw new InvalidOperationException("Shapes " + (index + 1) + " has no valid compact path layout.");
            return best;
        }
    }
}
