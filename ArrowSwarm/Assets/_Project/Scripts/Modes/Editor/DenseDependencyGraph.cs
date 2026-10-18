namespace ArrowSwarm.Modes.Editor
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>Geometry-only dependency analysis used while authoring dense paths.</summary>
    public sealed class DenseDependencyMetrics
    {
        public bool Solvable { get; internal set; }
        public int BlockedArrows { get; internal set; }
        public int MaximumDepth { get; internal set; }
        public int InwardHeads { get; internal set; }
        public int DirectionCount { get; internal set; }
        public int[] Depths { get; internal set; }
    }

    /// <summary>Constructs exact head-ray dependencies without rescanning every path for every cell.</summary>
    public static class DenseDependencyGraph
    {
        /// <summary>Rejects self-blocking/cycles and reports dependency depth and direction variety.</summary>
        public static DenseDependencyMetrics Analyze(IReadOnlyList<PuzzleArrowData> arrows, int width, int height)
        {
            var result = new DenseDependencyMetrics { Depths = new int[arrows.Count] };
            var owner = new int[width, height];
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) owner[x, y] = -1;
            for (int i = 0; i < arrows.Count; i++) foreach (var p in arrows[i].Points)
            {
                if (p.x < 0 || p.y < 0 || p.x >= width || p.y >= height || owner[p.x, p.y] >= 0) return result;
                owner[p.x, p.y] = i;
            }
            var blockers = new List<int>[arrows.Count]; var directions = new HashSet<Vector2Int>();
            Vector2 center = new Vector2((width - 1) * .5f, (height - 1) * .5f);
            for (int i = 0; i < arrows.Count; i++)
            {
                blockers[i] = new List<int>(); var arrow = arrows[i]; directions.Add(arrow.Direction);
                if (Vector2.Dot((Vector2)arrow.Head - center, arrow.Direction) < 0f) result.InwardHeads++;
                for (var p = arrow.Head + arrow.Direction; p.x >= 0 && p.y >= 0 && p.x < width && p.y < height; p += arrow.Direction)
                {
                    int other = owner[p.x, p.y]; if (other < 0) continue;
                    if (other == i) return result;
                    if (!blockers[i].Contains(other)) blockers[i].Add(other);
                }
                if (blockers[i].Count > 0) result.BlockedArrows++;
            }
            var state = new byte[arrows.Count];
            for (int i = 0; i < arrows.Count; i++)
            {
                if (!Visit(i, blockers, state, result.Depths)) return result;
                result.MaximumDepth = Mathf.Max(result.MaximumDepth, result.Depths[i]);
            }
            result.DirectionCount = directions.Count; result.Solvable = true; return result;
        }

        /// <summary>Reverses a path only when the resulting complete puzzle remains acyclic.</summary>
        public static bool TryReverse(List<PuzzleArrowData> arrows, int index, int width, int height)
        {
            var original = arrows[index]; var points = new List<Vector2Int>(original.Points); points.Reverse();
            arrows[index] = new PuzzleArrowData(points, points[0] - points[1], original.Color, original.ColorId);
            if (Analyze(arrows, width, height).Solvable) return true;
            arrows[index] = original; return false;
        }

        /// <summary>Turns a Hamiltonian tile head inward without changing its occupied cells or tail connection.</summary>
        public static bool TryTurnHead(List<PuzzleArrowData> arrows, int index, int width, int height)
        {
            var original = arrows[index];
            if (original.Points.Count < 4) return false;
            Vector2 center = new Vector2((width - 1) * .5f, (height - 1) * .5f);
            if (Vector2.Dot((Vector2)original.Head - center, original.Direction) < 0f) return false;
            var points = new List<Vector2Int>(original.Points);
            var swap = points[0]; points[0] = points[2]; points[2] = swap;
            Vector2Int direction = points[0] - points[1];
            if (Vector2.Dot((Vector2)points[0] - center, direction) >= 0f) return false;
            arrows[index] = new PuzzleArrowData(points, direction, original.Color, original.ColorId);
            if (Analyze(arrows, width, height).Solvable) return true;
            arrows[index] = original; return false;
        }

        private static bool Visit(int index, List<int>[] blockers, byte[] state, int[] depth)
        {
            if (state[index] == 1) return false;
            if (state[index] == 2) return true;
            state[index] = 1; depth[index] = 1;
            foreach (int other in blockers[index])
            {
                if (!Visit(other, blockers, state, depth)) return false;
                depth[index] = Mathf.Max(depth[index], depth[other] + 1);
            }
            state[index] = 2; return true;
        }
    }
}
