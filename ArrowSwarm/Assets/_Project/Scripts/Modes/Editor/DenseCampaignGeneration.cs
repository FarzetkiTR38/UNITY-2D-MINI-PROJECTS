namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>Deterministic dense puzzle factories; asset persistence is handled separately.</summary>
    public static class DenseCampaignGeneration
    {
        /// <summary>Samples the cropped Shapes-only layout once for each complete 2x2 tile.</summary>
        public static Color SampleShape(int index, int x, int y, int width, int height)
            => ShapesRevision3Generation.Sample(index, x, y, width, height);

        /// <summary>Creates one fully covered Shapes level, including varied inward-facing heads.</summary>
        public static PuzzleLevel CreateShape(int index) => ShapesRevision3Generation.Create(index);

        /// <summary>Creates a unique full board whose color phases follow an exact dependency order.</summary>
        public static PuzzleLevel CreateColorShift(int index)
        {
            int size = 10 + index / 3 * 2;
            int wantedTurns = 1 + index / 5;
            var paths = BestPaths(size, size, (x, y) => true, (x, y) => Color.white,
                27182 + index * 6151, 8, 16 + index / 4 * 4, wantedTurns + 1);
            var graph = DenseDependencyGraph.Analyze(paths, size, size);
            int phases = Mathf.Min(wantedTurns + 1, graph.MaximumDepth);
            var palette = Palette(index); var arrows = new List<PuzzleArrowData>();
            for (int i = 0; i < paths.Count; i++)
            {
                int phase = (graph.Depths[i] - 1) * phases / Mathf.Max(1, graph.MaximumDepth);
                int id = PuzzleRules.WallId(paths[i].Direction, phase & 1);
                arrows.Add(new PuzzleArrowData(paths[i].Points, paths[i].Direction, palette[id], id));
            }
            var level = ScriptableObject.CreateInstance<PuzzleLevel>();
            string[] motifs = { "Aqua Circuit", "Coral Weave", "Golden Switch", "Violet Relay", "Prism Lock",
                "Lagoon Labyrinth", "Sunset Matrix", "Jade Current", "Aurora Vault", "Spectrum Citadel" };
            level.Author(GameModeId.ColorShift, motifs[index % motifs.Length] + " " + (index + 1).ToString("D2"),
                size, size, arrows, palette, 1000);
            var solved = PuzzleSolver.Solve(level);
            if (!solved.Solvable) throw new InvalidOperationException("Color Shift " + (index + 1) + ": " + solved.Error);
            level.Author(GameModeId.ColorShift, level.Title, size, size, arrows, palette,
                solved.RequiredTurns + (index < 5 ? 1 : 0));
            DenseCampaignVerification.AssertLevel(level, index);
            return level;
        }

        private static List<PuzzleArrowData> BestPaths(int width, int height, Func<int, int, bool> mask,
            Func<int, int, Color> color, int seed, int min, int max, int desiredDepth)
        {
            List<PuzzleArrowData> best = null; float bestScore = float.NegativeInfinity;
            for (int attempt = 0; attempt < 5; attempt++)
            {
                var paths = ShapePathGenerator.Generate(width, height, mask, color, seed + attempt * 104729, min, max);
                var graph = DenseDependencyGraph.Analyze(paths, width, height);
                if (!graph.Solvable) continue;
                float score = Mathf.Min(desiredDepth, graph.MaximumDepth) * 30f + graph.DirectionCount * 15f
                    + graph.InwardHeads * 8f + graph.BlockedArrows * .8f;
                if (graph.InwardHeads == 0) score -= 1000f;
                if (score > bestScore) { bestScore = score; best = paths; }
            }
            if (best == null) throw new InvalidOperationException("Could not partition dense silhouette.");
            return best;
        }

        private static Color[] Palette(int index)
        {
            float hue = Mathf.Repeat(.48f + index * .13750776f, 1f);
            return new[] { Color.HSVToRGB(hue, .68f, .87f), Color.HSVToRGB(Mathf.Repeat(hue + .37f, 1f), .63f, .96f) };
        }
    }
}
