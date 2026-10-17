namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>Reverse-builds removable paths inside a sampled silhouette, giving a solution by construction.</summary>
    public static class ShapePathGenerator
    {
        private static readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        /// <summary>Generates deterministic varied paths; callers still run the independent validator.</summary>
        public static List<PuzzleArrowData> Generate(int width, int height, Func<int, int, bool> mask,
            Func<int, int, Color> color, int seed, int minLength = 4, int maxLength = 16)
        {
            var random = new System.Random(seed);
            var used = new HashSet<Vector2Int>();
            var arrows = new List<PuzzleArrowData>();
            var candidates = new List<Vector2Int>();
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) if (mask(x, y)) candidates.Add(new Vector2Int(x, y));
            for (int i = candidates.Count - 1; i > 0; i--) { int j = random.Next(i + 1); var p = candidates[i]; candidates[i] = candidates[j]; candidates[j] = p; }
            bool progress;
            do
            {
                progress = false;
                foreach (var head in candidates)
                {
                    if (used.Contains(head)) continue;
                    int start = random.Next(4);
                    for (int d = 0; d < 4; d++)
                    {
                        var direction = Directions[(start + d) % 4];
                        var tail = head - direction;
                        if (!Available(tail, width, height, mask, used) || !ClearRay(head, direction, width, height, used)) continue;
                        var path = new List<Vector2Int> { head, tail };
                        var local = new HashSet<Vector2Int>(path);
                        int target = random.Next(Mathf.Max(2, minLength), Mathf.Max(minLength + 1, maxLength + 1));
                        while (path.Count < target)
                        {
                            var last = path[path.Count - 1];
                            Vector2Int next = last; bool found = false;
                            int step = random.Next(4);
                            for (int k = 0; k < 4; k++)
                            {
                                var p = last + Directions[(step + k) % 4];
                                if (!Available(p, width, height, mask, used) || local.Contains(p)) continue;
                                var delta = p - head;
                                if (delta.x * direction.y == delta.y * direction.x && delta.x * direction.x + delta.y * direction.y > 0) continue;
                                next = p; found = true; break;
                            }
                            if (!found) break;
                            path.Add(next); local.Add(next);
                        }
                        foreach (var p in path) used.Add(p);
                        arrows.Add(new PuzzleArrowData(path, direction, color(head.x, head.y)));
                        progress = true; break;
                    }
                }
            } while (progress);
            return arrows;
        }

        private static bool Available(Vector2Int p, int w, int h, Func<int, int, bool> mask, HashSet<Vector2Int> used) =>
            p.x >= 0 && p.y >= 0 && p.x < w && p.y < h && mask(p.x, p.y) && !used.Contains(p);
        private static bool ClearRay(Vector2Int head, Vector2Int direction, int w, int h, HashSet<Vector2Int> used)
        {
            for (var p = head + direction; p.x >= 0 && p.y >= 0 && p.x < w && p.y < h; p += direction)
                if (used.Contains(p)) return false;
            return true;
        }
    }
}
