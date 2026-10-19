namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>Partitions complete 2x2 silhouette tiles into varied, fully covering Hamiltonian paths.</summary>
    public static class DenseTilePaths
    {
        private static readonly Vector2Int[] Corners = { Vector2Int.zero, Vector2Int.right, Vector2Int.one, Vector2Int.up };
        private static readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        /// <summary>Creates an exact cover, then safely reverses paths to add inward heads and dependencies.</summary>
        public static List<PuzzleArrowData> Generate(int width, int height, bool[,] mask, Func<int, int, Color> color,
            int seed, int minLength, int maxLength)
        {
            int tw = width / 2, th = height / 2;
            var free = new bool[tw, th]; int remaining = 0;
            for (int y = 0; y < th; y++) for (int x = 0; x < tw; x++)
                if (mask[x * 2, y * 2]) { free[x, y] = true; remaining++; }
            var random = new System.Random(seed);
            var result = new List<PuzzleArrowData>();
            float cx = (tw - 1) * .5f, cy = (th - 1) * .5f;
            float wx = .85f + (float)random.NextDouble() * .3f, wy = .85f + (float)random.NextDouble() * .3f;
            while (remaining > 0)
            {
                Vector2Int tile = Vector2Int.zero; float rank = float.NegativeInfinity; int ties = 0;
                for (int y = 0; y < th; y++) for (int x = 0; x < tw; x++)
                {
                    if (!free[x, y]) continue;
                    float r = Mathf.Abs(x - cx) * wx + Mathf.Abs(y - cy) * wy;
                    if (r > rank + .00001f) { rank = r; tile = new Vector2Int(x, y); ties = 1; }
                    else if (Mathf.Abs(r - rank) < .00001f && random.Next(++ties) == 0) tile = new Vector2Int(x, y);
                }
                int sx = tile.x > cx ? 1 : tile.x < cx ? -1 : random.Next(2) * 2 - 1;
                int sy = tile.y > cy ? 1 : tile.y < cy ? -1 : random.Next(2) * 2 - 1;
                Vector2Int local = new Vector2Int(sx > 0 ? 1 : 0, sy > 0 ? 1 : 0);
                Vector2Int direction = random.Next(2) == 0 ? new Vector2Int(sx, 0) : new Vector2Int(0, sy);
                int corner = Array.IndexOf(Corners, local);
                int step = Corners[(corner + 1) % 4] == local - direction ? 1 : -1;
                var path = new List<Vector2Int>();
                Append(path, tile, corner, step); free[tile.x, tile.y] = false; remaining--;
                Color ink = color(path[0].x, path[0].y);
                int target = random.Next(Mathf.Max(1, minLength / 4), Mathf.Max(2, maxLength / 4 + 1));
                for (int count = 1; count < target; count++)
                {
                    Vector2Int end = path[path.Count - 1], nextTile = Vector2Int.zero;
                    int nextCorner = -1, nextStep = 1; float best = float.NegativeInfinity;
                    for (int d = 0; d < 4; d++)
                    {
                        Vector2Int p = end + Directions[d];
                        if (p.x < 0 || p.y < 0 || p.x >= width || p.y >= height) continue;
                        Vector2Int t = new Vector2Int(p.x / 2, p.y / 2);
                        if (!free[t.x, t.y]) continue;
                        Color sample = color(p.x, p.y);
                        float difference = Mathf.Abs(ink.r - sample.r) + Mathf.Abs(ink.g - sample.g) + Mathf.Abs(ink.b - sample.b);
                        if (difference > .65f) continue;
                        int entry = Array.IndexOf(Corners, p - t * 2);
                        for (int rotation = -1; rotation <= 1; rotation += 2)
                        {
                            Vector2Int exit = t * 2 + Corners[(entry + rotation * 3 + 12) % 4];
                            float score = (float)random.NextDouble() - difference;
                            for (int n = 0; n < 4; n++)
                            {
                                Vector2Int q = exit + Directions[n];
                                if (q.x < 0 || q.y < 0 || q.x >= width || q.y >= height) continue;
                                var neighbor = new Vector2Int(q.x / 2, q.y / 2);
                                if (neighbor != t && free[neighbor.x, neighbor.y]) score += .15f;
                            }
                            if (score <= best) continue;
                            best = score; nextTile = t; nextCorner = entry; nextStep = rotation;
                        }
                    }
                    if (nextCorner < 0) break;
                    Append(path, nextTile, nextCorner, nextStep); free[nextTile.x, nextTile.y] = false; remaining--;
                }
                result.Add(new PuzzleArrowData(path, direction, ink));
            }
            // Each accepted reversal is independently checked against the complete dependency graph.
            for (int pass = 0; pass < 2; pass++)
                for (int i = result.Count - 1; i >= 0; i--)
                    if (random.NextDouble() < .48) DenseDependencyGraph.TryReverse(result, i, width, height);
            for (int pass = 0; pass < 3; pass++)
                for (int i = 0; i < result.Count; i++) DenseDependencyGraph.TryTurnHead(result, i, width, height);
            return result;
        }

        /// <summary>Checks whether every populated atomic tile contains all four intended cells.</summary>
        public static bool Supports(int width, int height, bool[,] mask)
        {
            if ((width & 1) != 0 || (height & 1) != 0) return false;
            for (int y = 0; y < height; y += 2) for (int x = 0; x < width; x += 2)
                if (mask[x, y] != mask[x + 1, y] || mask[x, y] != mask[x, y + 1] || mask[x, y] != mask[x + 1, y + 1]) return false;
            return true;
        }

        private static void Append(List<Vector2Int> path, Vector2Int tile, int corner, int step)
        {
            for (int i = 0; i < 4; i++) path.Add(tile * 2 + Corners[(corner + i * step + 12) % 4]);
        }
    }
}
