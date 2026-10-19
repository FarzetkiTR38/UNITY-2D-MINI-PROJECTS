namespace ArrowSwarm.Modes.Editor
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>Peels medium color packets, joins orthogonal pieces and chooses acyclic inward-facing heads.</summary>
    public static partial class ShapesBalancedPaths
    {
        /// <summary>Partitions complete 2x2 tiles without changing occupancy or their published color key.</summary>
        public static List<PuzzleArrowData> Generate(Color[,] tiles, int seed, int index)
        {
            int tw = tiles.GetLength(0), th = tiles.GetLength(1), remaining = 0;
            var free = new bool[tw, th]; var arrows = new List<PuzzleArrowData>(); var random = new System.Random(seed);
            for (int y = 0; y < th; y++) for (int x = 0; x < tw; x++)
                if (tiles[x, y].a > .5f) { free[x, y] = true; remaining++; }
            float cx = (tw - 1) * .5f, cy = (th - 1) * .5f;
            float wx = .8f + (float)random.NextDouble() * .4f, wy = .8f + (float)random.NextDouble() * .4f;
            while (remaining > 0)
            {
                var tile = Vector2Int.zero; float rank = float.NegativeInfinity;
                for (int y = 0; y < th; y++) for (int x = 0; x < tw; x++)
                {
                    if (!free[x, y]) continue;
                    float score = Mathf.Abs(x - cx) * wx + Mathf.Abs(y - cy) * wy;
                    if (score > rank) { rank = score; tile = new Vector2Int(x, y); }
                }
                int sx = tile.x >= cx ? 1 : -1, sy = tile.y >= cy ? 1 : -1, maximum = random.Next(3, 8);
                int horizontal = Packet(free, tiles, tile, new Vector2Int(-sx, 0), maximum);
                int vertical = Packet(free, tiles, tile, new Vector2Int(0, -sy), maximum);
                bool alongX = horizontal + random.NextDouble() * 3 > vertical + random.NextDouble() * 3;
                int count = alongX ? horizontal : vertical;
                var step = alongX ? new Vector2Int(-sx, 0) : new Vector2Int(0, -sy);
                var head = tile * 2 + new Vector2Int(sx > 0 ? 1 : 0, sy > 0 ? 1 : 0);
                var forward = alongX ? new Vector2Int(-sx, 0) : new Vector2Int(0, -sy);
                var across = alongX ? new Vector2Int(0, -sy) : new Vector2Int(-sx, 0);
                var points = new List<Vector2Int>();
                for (int i = 0; i < count * 2; i++) points.Add(head + forward * i);
                for (int i = count * 2 - 1; i >= 0; i--) points.Add(head + forward * i + across);
                for (int i = 0; i < count; i++) { var p = tile + step * i; free[p.x, p.y] = false; remaining--; }
                arrows.Add(new PuzzleArrowData(points, points[0] - points[1], tiles[tile.x, tile.y]));
            }
            JoinPackets(arrows, tw * 2, th * 2, random);
            Orient(arrows, tw * 2, th * 2, random, index);
            return arrows;
        }

        private static int Packet(bool[,] free, Color[,] tiles, Vector2Int start, Vector2Int step, int maximum)
        {
            int count = 0;
            for (var p = start; count < maximum; p += step)
            {
                if (p.x < 0 || p.y < 0 || p.x >= free.GetLength(0) || p.y >= free.GetLength(1) || !free[p.x, p.y] ||
                    !ShapesBalancedSource.SameInk(tiles[start.x, start.y], tiles[p.x, p.y])) break;
                count++;
            }
            return count;
        }

        private static int Bends(List<Vector2Int> points)
        {
            int result = 0;
            for (int i = 2; i < points.Count; i++) if (points[i] - points[i - 1] != points[i - 1] - points[i - 2]) result++;
            return result;
        }
        private static bool Adjacent(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
    }
}
