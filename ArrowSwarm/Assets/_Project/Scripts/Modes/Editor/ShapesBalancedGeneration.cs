namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>Selects medium curved Shapes partitions by real opening and midgame choice pressure.</summary>
    public static class ShapesBalancedGeneration
    {
        /// <summary>Rebuilds geometry against a frozen color mask; no asset is written by this factory.</summary>
        public static PuzzleLevel Create(int index, Color[,] fallback)
        {
            var tiles = ShapesBalancedSource.Read(index, fallback, out bool published);
            int width = tiles.GetLength(0) * 2, height = tiles.GetLength(1) * 2;
            List<PuzzleArrowData> best = null; float bestScore = float.NegativeInfinity;
            for (int attempt = 0; attempt < 6; attempt++)
            {
                int seed = 31415 + index * 7919 + attempt * 104729;
                var paths = published ? ShapesBalancedPaths.Generate(tiles, seed, index) :
                    ShapesRevision3Paths.Generate(tiles, ShapesRevision3Profile.For(index), seed);
                var metrics = ShapesDifficultyMetrics.Analyze(paths, width, height);
                if (!metrics.Solvable || metrics.DirectionCount != 4) continue;
                var graph = DenseDependencyGraph.Analyze(paths, width, height);
                if (graph.InwardHeads == 0 || graph.MaximumDepth < 2) continue;
                float score = ShapesBalancedScoring.Final(metrics, index);
                if (score > bestScore) { bestScore = score; best = paths; }
            }
            if (best == null) throw new InvalidOperationException("Shapes " + (index + 1) + ": balanced partition failed.");
            var mask = new bool[width, height];
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) mask[x, y] = tiles[x / 2, y / 2].a > .5f;
            DenseCampaignVerification.AssertCoverage(best, width, height, mask);
            foreach (var arrow in best) foreach (var p in arrow.Points)
                if (published && !ShapesBalancedSource.SameInk(arrow.Color, tiles[p.x / 2, p.y / 2]))
                    throw new InvalidOperationException("Balanced path crossed a published color boundary.");
            var level = ScriptableObject.CreateInstance<PuzzleLevel>();
            level.Author(GameModeId.Shapes, DenseShapeLibrary.Name(index), width, height, best, null, 0);
            var solution = PuzzleSolver.Solve(level);
            if (!solution.Solvable) throw new InvalidOperationException(level.Title + ": " + solution.Error);
            return level;
        }
    }
}
