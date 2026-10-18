namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>Creates fully occupied silhouette puzzles; partial covers are never accepted.</summary>
    public static class ShapePathGenerator
    {
        /// <summary>Partitions an even grid of complete 2x2 sampled tiles into solvable arrow paths.</summary>
        public static List<PuzzleArrowData> Generate(int width, int height, Func<int, int, bool> mask,
            Func<int, int, Color> color, int seed, int minLength = 4, int maxLength = 16)
        {
            if (width < 2 || height < 2 || mask == null || color == null) throw new ArgumentException("Invalid silhouette grid or sampler.");
            var occupied = new bool[width, height]; int cells = 0;
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
                if (mask(x, y)) { occupied[x, y] = true; cells++; }
            if (cells == 0) throw new InvalidOperationException("The sampled silhouette is empty.");
            if (!DenseTilePaths.Supports(width, height, occupied))
                throw new InvalidOperationException("Exact silhouette coverage requires even dimensions and complete 2x2 sampled cells. Sample the image once per 2x2 block.");
            var arrows = DenseTilePaths.Generate(width, height, occupied, color, seed,
                Mathf.Max(4, minLength), Mathf.Max(Mathf.Max(4, minLength), maxLength));
            DenseCampaignVerification.AssertCoverage(arrows, width, height, occupied);
            if (!DenseDependencyGraph.Analyze(arrows, width, height).Solvable)
                throw new InvalidOperationException("Dense path dependency validation failed.");
            return arrows;
        }
    }
}
