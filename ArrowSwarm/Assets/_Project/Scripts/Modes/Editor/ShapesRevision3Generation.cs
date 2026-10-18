namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>Uniform-pitch sampling, exact tile crop and smooth Shapes-only ribbon authoring.</summary>
    public static class ShapesRevision3Generation
    {
        private sealed class Layout
        {
            public Color[,] Tiles;
            public int Width => Tiles.GetLength(0) * 2;
            public int Height => Tiles.GetLength(1) * 2;
        }
        private static readonly Dictionary<int, Layout> Layouts = new Dictionary<int, Layout>();

        /// <summary>Returns the identical cropped mask/color used by generation and independent verification.</summary>
        public static Color Sample(int index, int x, int y, int width, int height)
        {
            var layout = GetLayout(index);
            int tx = Mathf.Clamp(x * layout.Tiles.GetLength(0) / width, 0, layout.Tiles.GetLength(0) - 1);
            int ty = Mathf.Clamp(y * layout.Tiles.GetLength(1) / height, 0, layout.Tiles.GetLength(1) - 1);
            return layout.Tiles[tx, ty];
        }

        /// <summary>Creates a fully covered silhouette with no repeated per-tile Hamiltonian curls.</summary>
        public static PuzzleLevel Create(int index)
        {
            var layout = GetLayout(index); var profile = ShapesRevision3Profile.For(index);
            List<PuzzleArrowData> best = null; float bestScore = float.NegativeInfinity;
            for (int attempt = 0; attempt < 4; attempt++)
            {
                var paths = ShapesRevision3Paths.Generate(layout.Tiles, profile, 31415 + index * 7919 + attempt * 104729);
                var graph = DenseDependencyGraph.Analyze(paths, layout.Width, layout.Height);
                if (!graph.Solvable || graph.DirectionCount != 4 || graph.InwardHeads == 0 || graph.MaximumDepth < 2) continue;
                int bends = 0;
                foreach (var arrow in paths) for (int p = 2; p < arrow.Points.Count; p++)
                    if (arrow.Points[p] - arrow.Points[p - 1] != arrow.Points[p - 1] - arrow.Points[p - 2]) bends++;
                float score = Mathf.Min(8, graph.MaximumDepth) * 4f + Mathf.Min(12, graph.InwardHeads) * 2f - paths.Count * 2f - bends * .1f;
                if (score > bestScore) { bestScore = score; best = paths; }
            }
            if (best == null) throw new InvalidOperationException("Shapes " + (index + 1) + ": no four-direction acyclic ribbon partition.");
            var level = ScriptableObject.CreateInstance<PuzzleLevel>();
            level.Author(GameModeId.Shapes, DenseShapeLibrary.Name(index), layout.Width, layout.Height, best, null, 0);
            DenseCampaignVerification.AssertLevel(level, index);
            return level;
        }

        private static Layout GetLayout(int index)
        {
            if (Layouts.TryGetValue(index, out var cached)) return cached;
            const int probe = 192;
            float minX = 1f, minY = 1f, maxX = -1f, maxY = -1f;
            for (int y = 0; y <= probe; y++) for (int x = 0; x <= probe; x++)
            {
                float u = x * 2f / probe - 1f, v = y * 2f / probe - 1f;
                if (DenseShapeLibrary.Sample(index, u, v).a <= .5f) continue;
                minX = Mathf.Min(minX, u); maxX = Mathf.Max(maxX, u);
                minY = Mathf.Min(minY, v); maxY = Mathf.Max(maxY, v);
            }
            if (minX > maxX) throw new InvalidOperationException("Empty shape reference " + index);
            float pitch = Mathf.Max(maxX - minX, maxY - minY) / (ShapesRevision3Profile.For(index).LongSide / 2f);
            int tw = Mathf.CeilToInt((maxX - minX) / pitch) + 2, th = Mathf.CeilToInt((maxY - minY) / pitch) + 2;
            float centerX = (minX + maxX) * .5f, centerY = (minY + maxY) * .5f;
            var sampled = new Color[tw, th]; int left = tw, right = -1, bottom = th, top = -1;
            for (int y = 0; y < th; y++) for (int x = 0; x < tw; x++)
            {
                sampled[x, y] = DenseShapeLibrary.Sample(index, centerX + (x + .5f - tw * .5f) * pitch,
                    centerY + (y + .5f - th * .5f) * pitch);
                if (sampled[x, y].a <= .5f) continue;
                left = Mathf.Min(left, x); right = Mathf.Max(right, x); bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y);
            }
            if (right < left) throw new InvalidOperationException("Shape resolution lost its silhouette " + index);
            var tiles = new Color[right - left + 1, top - bottom + 1];
            for (int y = bottom; y <= top; y++) for (int x = left; x <= right; x++) tiles[x - left, y - bottom] = sampled[x, y];
            var result = new Layout { Tiles = tiles }; Layouts.Add(index, result); return result;
        }
    }
}
