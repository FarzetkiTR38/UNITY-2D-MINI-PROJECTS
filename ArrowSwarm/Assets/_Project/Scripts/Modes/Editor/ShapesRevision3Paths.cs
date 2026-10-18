namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>Peels long, color-coherent two-row ribbons with two bends instead of a curl per tile.</summary>
    public static partial class ShapesRevision3Paths
    {
        /// <summary>Partitions the complete color mask into solvable, region-coherent ribbon paths.</summary>
        public static List<PuzzleArrowData> Generate(Color[,] tiles, ShapesRevision3Profile profile, int seed)
        {
            int tw = tiles.GetLength(0), th = tiles.GetLength(1), remaining = 0;
            var free = new bool[tw, th]; var result = new List<PuzzleArrowData>();
            for (int y = 0; y < th; y++) for (int x = 0; x < tw; x++)
                if (tiles[x, y].a > .5f) { free[x, y] = true; remaining++; }
            var random = new System.Random(seed); var directionCounts = new int[4];
            float cx = (tw - 1) * .5f, cy = (th - 1) * .5f;
            float wx = .9f + (float)random.NextDouble() * .2f, wy = .9f + (float)random.NextDouble() * .2f;
            while (remaining > 0)
            {
                var tile = OuterTile(free, cx, cy, wx, wy, random);
                int sx = tile.x > cx ? 1 : tile.x < cx ? -1 : random.Next(2) * 2 - 1;
                int sy = tile.y > cy ? 1 : tile.y < cy ? -1 : random.Next(2) * 2 - 1;
                int horizontal = Run(free, tiles, tile, new Vector2Int(-sx, 0), profile);
                int vertical = Run(free, tiles, tile, new Vector2Int(0, -sy), profile);
                int hd = sx > 0 ? 1 : 3, vd = sy > 0 ? 0 : 2;
                float hScore = horizontal + (directionCounts[hd] == 0 ? 3f : 0f) + (float)random.NextDouble() * 1.4f;
                float vScore = vertical + (directionCounts[vd] == 0 ? 3f : 0f) + (float)random.NextDouble() * 1.4f;
                bool alongX = hScore >= vScore; int count = alongX ? horizontal : vertical;
                var step = alongX ? new Vector2Int(-sx, 0) : new Vector2Int(0, -sy);
                var points = Ribbon(tile, sx, sy, alongX, count);
                for (int i = 0; i < count; i++) { var p = tile + step * i; free[p.x, p.y] = false; remaining--; }
                directionCounts[alongX ? hd : vd]++;
                result.Add(new PuzzleArrowData(points, points[0] - points[1], tiles[tile.x, tile.y]));
            }
            AddHeadVariety(result, tw * 2, th * 2, random);
            Compact(result, tw * 2, th * 2, profile);
            return result;
        }

        private static Vector2Int OuterTile(bool[,] free, float cx, float cy, float wx, float wy, System.Random random)
        {
            Vector2Int selected = Vector2Int.zero; float best = float.NegativeInfinity; int ties = 0;
            for (int y = 0; y < free.GetLength(1); y++) for (int x = 0; x < free.GetLength(0); x++)
            {
                if (!free[x, y]) continue;
                float rank = Mathf.Abs(x - cx) * wx + Mathf.Abs(y - cy) * wy;
                if (rank > best + .00001f) { best = rank; selected = new Vector2Int(x, y); ties = 1; }
                else if (Mathf.Abs(rank - best) < .00001f && random.Next(++ties) == 0) selected = new Vector2Int(x, y);
            }
            return selected;
        }

        private static int Run(bool[,] free, Color[,] tiles, Vector2Int start, Vector2Int step, ShapesRevision3Profile profile)
        {
            int count = 0; var ink = tiles[start.x, start.y];
            for (var p = start; count < profile.MaximumCells / 4; p += step)
            {
                if (p.x < 0 || p.y < 0 || p.x >= free.GetLength(0) || p.y >= free.GetLength(1) || !free[p.x, p.y]) break;
                var c = tiles[p.x, p.y];
                if (Mathf.Abs(ink.r - c.r) + Mathf.Abs(ink.g - c.g) + Mathf.Abs(ink.b - c.b) > profile.RegionTolerance) break;
                count++;
            }
            return count;
        }

        private static List<Vector2Int> Ribbon(Vector2Int tile, int sx, int sy, bool alongX, int count)
        {
            var head = tile * 2 + new Vector2Int(sx > 0 ? 1 : 0, sy > 0 ? 1 : 0);
            var forward = alongX ? new Vector2Int(-sx, 0) : new Vector2Int(0, -sy);
            var across = alongX ? new Vector2Int(0, -sy) : new Vector2Int(-sx, 0);
            int length = count * 2; var points = new List<Vector2Int>(length * 2);
            for (int i = 0; i < length; i++) points.Add(head + forward * i);
            for (int i = length - 1; i >= 0; i--) points.Add(head + forward * i + across);
            return points;
        }
    }
}
