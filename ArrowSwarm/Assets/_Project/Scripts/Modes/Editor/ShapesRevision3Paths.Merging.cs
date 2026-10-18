namespace ArrowSwarm.Modes.Editor
{
    using System.Collections.Generic;
    using UnityEngine;

    public static partial class ShapesRevision3Paths
    {
        /// <summary>Joins touching same-ink ribbons into longer serpentines without changing a single mask cell.</summary>
        private static void Compact(List<PuzzleArrowData> arrows, int width, int height, ShapesRevision3Profile profile)
        {
            int cells = 0; foreach (var arrow in arrows) cells += arrow.Points.Count;
            int target = Mathf.Clamp(cells / 32, 45, 95);
            var graph = DenseDependencyGraph.Analyze(arrows, width, height);
            bool changed = true;
            while (changed && arrows.Count > target)
            {
                changed = false;
                for (int i = 0; i < arrows.Count && arrows.Count > target; i++)
                    for (int j = i + 1; j < arrows.Count && arrows.Count > target; j++)
                    {
                        var a = arrows[i]; var b = arrows[j];
                        if (a.Points.Count + b.Points.Count > profile.MaximumCells ||
                            Mathf.Abs(a.Color.r - b.Color.r) + Mathf.Abs(a.Color.g - b.Color.g) + Mathf.Abs(a.Color.b - b.Color.b) > .002f) continue;
                        var points = Join(a.Points, b.Points);
                        if (points == null || Bends(points) > 8) continue;
                        arrows[i] = new PuzzleArrowData(points, a.Direction, a.Color); arrows.RemoveAt(j);
                        var candidate = DenseDependencyGraph.Analyze(arrows, width, height);
                        if (candidate.Solvable && candidate.DirectionCount == 4 && candidate.MaximumDepth > 1 &&
                            candidate.InwardHeads >= Mathf.Min(3, graph.InwardHeads))
                        { graph = candidate; changed = true; j--; }
                        else { arrows[i] = a; arrows.Insert(j, b); }
                    }
            }
        }

        private static List<Vector2Int> Join(List<Vector2Int> a, List<Vector2Int> b)
        {
            var tail = a[a.Count - 1]; bool reverse = false;
            if (!Adjacent(tail, b[0])) { if (!Adjacent(tail, b[b.Count - 1])) return null; reverse = true; }
            var joined = new List<Vector2Int>(a); joined.Capacity = a.Count + b.Count;
            if (reverse) for (int i = b.Count - 1; i >= 0; i--) joined.Add(b[i]);
            else joined.AddRange(b);
            return joined;
        }

        private static bool Adjacent(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
        private static int Bends(List<Vector2Int> points)
        {
            int bends = 0;
            for (int i = 2; i < points.Count; i++) if (points[i] - points[i - 1] != points[i - 1] - points[i - 2]) bends++;
            return bends;
        }
    }
}
