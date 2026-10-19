namespace ArrowSwarm.Modes.Editor
{
    using System.Collections.Generic;
    using UnityEngine;

    public static partial class ShapesBalancedPaths
    {
        private static void JoinPackets(List<PuzzleArrowData> arrows, int width, int height, System.Random random)
        {
            int cells = 0; foreach (var a in arrows) cells += a.Points.Count;
            int target = Mathf.Clamp(cells / 25, 55, 105);
            bool changed = true;
            while (changed && arrows.Count > target)
            {
                changed = false;
                for (int n = 0, offset = random.Next(arrows.Count); n < arrows.Count && arrows.Count > target; n++)
                {
                    int i = (n + offset) % arrows.Count;
                    for (int j = i + 1; j < arrows.Count && arrows.Count > target; j++)
                    {
                        var a = arrows[i]; var b = arrows[j];
                        if (a.Points.Count + b.Points.Count > 88 || !ShapesBalancedSource.SameInk(a.Color, b.Color)) continue;
                        if (!Touches(a.Points, b.Points)) continue;
                        var options = new List<List<Vector2Int>>();
                        foreach (var ap in Variants(a)) foreach (var bp in Variants(b))
                        {
                            if (!Adjacent(ap[ap.Count - 1], bp[0])) continue;
                            var points = new List<Vector2Int>(ap); points.AddRange(bp);
                            int bends = Bends(points);
                            if (bends < 4 || bends > 10 || !OwnRayClear(points, width, height)) continue;
                            options.Add(points);
                        }
                        options.Sort((x, y) => Mathf.Abs(Bends(x) - 6).CompareTo(Mathf.Abs(Bends(y) - 6)));
                        bool joined = false;
                        for (int k = 0; k < Mathf.Min(6, options.Count); k++)
                        {
                            var points = options[k]; arrows[i] = new PuzzleArrowData(points, points[0] - points[1], a.Color); arrows.RemoveAt(j);
                            if (DenseDependencyGraph.Analyze(arrows, width, height).Solvable) { joined = changed = true; break; }
                            arrows[i] = a; arrows.Insert(j, b);
                        }
                        if (joined) j--;
                    }
                }
            }
        }

        private static bool Touches(List<Vector2Int> a, List<Vector2Int> b)
        {
            foreach (var x in a) foreach (var y in b) if (Adjacent(x, y)) return true;
            return false;
        }
    }
}
