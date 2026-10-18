namespace ArrowSwarm.Modes.Editor
{
    using System.Collections.Generic;
    using UnityEngine;

    public static partial class ShapesBalancedPaths
    {
        private static List<List<Vector2Int>> Variants(PuzzleArrowData arrow)
        {
            var result = new List<List<Vector2Int>> { arrow.Points };
            var reversed = new List<Vector2Int>(arrow.Points); reversed.Reverse(); result.Add(reversed);
            int left = int.MaxValue, bottom = int.MaxValue, right = -1, top = -1;
            foreach (var p in arrow.Points)
            { left = Mathf.Min(left, p.x); right = Mathf.Max(right, p.x); bottom = Mathf.Min(bottom, p.y); top = Mathf.Max(top, p.y); }
            if ((right - left + 1) * (top - bottom + 1) != arrow.Points.Count || right - left != 1 && top - bottom != 1) return result;
            var cycle = new List<Vector2Int>();
            for (int x = left; x <= right; x++) cycle.Add(new Vector2Int(x, top));
            for (int y = top - 1; y >= bottom; y--) cycle.Add(new Vector2Int(right, y));
            for (int x = right - 1; x >= left; x--) cycle.Add(new Vector2Int(x, bottom));
            for (int y = bottom + 1; y < top; y++) cycle.Add(new Vector2Int(left, y));
            for (int start = 0; start < cycle.Count; start++)
            {
                var p = cycle[start];
                if ((p.x != left && p.x != right) || (p.y != bottom && p.y != top)) continue;
                for (int step = -1; step <= 1; step += 2)
                {
                    var points = new List<Vector2Int>();
                    for (int i = 0; i < cycle.Count; i++) points.Add(cycle[(start + i * step + cycle.Count) % cycle.Count]);
                    result.Add(points);
                }
            }
            bool horizontal = top - bottom == 1; int span = horizontal ? right - left + 1 : top - bottom + 1;
            if (span >= 6) for (int sx = -1; sx <= 1; sx += 2) for (int sy = -1; sy <= 1; sy += 2)
            {
                var local = new List<Vector2Int> { new Vector2Int(0, 1), Vector2Int.zero, Vector2Int.right, Vector2Int.one };
                for (int x = 2; x < span; x++) local.Add(new Vector2Int(x, 1));
                for (int x = span - 1; x >= 2; x--) local.Add(new Vector2Int(x, 0));
                var points = new List<Vector2Int>();
                foreach (var p in local)
                {
                    int a = sx > 0 ? p.x : span - 1 - p.x, b = sy > 0 ? p.y : 1 - p.y;
                    points.Add(horizontal ? new Vector2Int(left + a, bottom + b) : new Vector2Int(left + b, bottom + a));
                }
                result.Add(points);
            }
            return result;
        }

        private static bool OwnRayClear(List<Vector2Int> points, int width, int height)
        {
            var direction = points[0] - points[1];
            for (var p = points[0] + direction; p.x >= 0 && p.y >= 0 && p.x < width && p.y < height; p += direction)
                if (points.Contains(p)) return false;
            return true;
        }
    }
}
