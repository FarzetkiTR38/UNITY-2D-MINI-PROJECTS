namespace ArrowSwarm.Modes.Editor
{
    using System.Collections.Generic;
    using UnityEngine;

    public static partial class ShapesRevision3Paths
    {
        private static readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        private static void AddHeadVariety(List<PuzzleArrowData> arrows, int width, int height, System.Random random)
        {
            foreach (var wanted in Directions)
            {
                if (arrows.Exists(a => a.Direction == wanted)) continue;
                int existingDirections = DenseDependencyGraph.Analyze(arrows, width, height).DirectionCount;
                for (int i = 0; i < arrows.Count; i++)
                {
                    var original = arrows[i]; var points = Alternate(original, wanted);
                    if (points == null) continue;
                    arrows[i] = new PuzzleArrowData(points, wanted, original.Color);
                    var candidate = DenseDependencyGraph.Analyze(arrows, width, height);
                    if (candidate.Solvable && candidate.DirectionCount > existingDirections) break;
                    arrows[i] = original;
                }
            }
            int wantedInward = Mathf.Clamp(arrows.Count / 8, 3, 12);
            Vector2 center = new Vector2((width - 1) * .5f, (height - 1) * .5f);
            var graph = DenseDependencyGraph.Analyze(arrows, width, height);
            for (int pass = 0; pass < 2 && graph.InwardHeads < wantedInward; pass++)
                for (int n = 0, offset = random.Next(arrows.Count); n < arrows.Count && graph.InwardHeads < wantedInward; n++)
                {
                    int i = (n + offset) % arrows.Count; var original = arrows[i];
                    var wanted = -original.Direction; var points = Alternate(original, wanted);
                    if (points == null || Vector2.Dot((Vector2)points[0] - center, wanted) >= 0f) continue;
                    arrows[i] = new PuzzleArrowData(points, wanted, original.Color);
                    var candidate = DenseDependencyGraph.Analyze(arrows, width, height);
                    if (candidate.Solvable && candidate.DirectionCount == 4 && candidate.InwardHeads > graph.InwardHeads) graph = candidate;
                    else arrows[i] = original;
                }
        }

        private static List<Vector2Int> Alternate(PuzzleArrowData arrow, Vector2Int wanted)
        {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = -1, maxY = -1;
            foreach (var p in arrow.Points)
            { minX = Mathf.Min(minX, p.x); minY = Mathf.Min(minY, p.y); maxX = Mathf.Max(maxX, p.x); maxY = Mathf.Max(maxY, p.y); }
            bool horizontal = maxY - minY == 1, vertical = maxX - minX == 1;
            if (wanted.x != 0 && !horizontal || wanted.y != 0 && !vertical) return null;
            var points = new List<Vector2Int>(arrow.Points.Count);
            if (wanted.x != 0)
            {
                int start = wanted.x > 0 ? maxX : minX, step = -wanted.x;
                for (int x = start; x >= minX && x <= maxX; x += step) points.Add(new Vector2Int(x, maxY));
                for (int x = start + step * (maxX - minX); x >= minX && x <= maxX; x -= step) points.Add(new Vector2Int(x, minY));
            }
            else
            {
                int start = wanted.y > 0 ? maxY : minY, step = -wanted.y;
                for (int y = start; y >= minY && y <= maxY; y += step) points.Add(new Vector2Int(maxX, y));
                for (int y = start + step * (maxY - minY); y >= minY && y <= maxY; y -= step) points.Add(new Vector2Int(minX, y));
            }
            return points;
        }
    }
}
