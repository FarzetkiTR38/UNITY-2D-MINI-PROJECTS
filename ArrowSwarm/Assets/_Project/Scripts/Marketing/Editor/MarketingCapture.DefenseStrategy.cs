namespace ArrowSwarm.Marketing.Editor
{
    using System;
    using System.Collections.Generic;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Path;
    using UnityEngine;
    using Arrow = ArrowSwarm.Arrow.Arrow;
    using Spawner = ArrowSwarm.Arrow.ArrowSpawner;

    /// <summary>Reproducible native procedural roll and legal last-shot selection, confined to capture scope.</summary>
    public static partial class MarketingCapture
    {
        private static void SeedDefenseLevel(int level)
        {
            if (_defenseSeed >= 0) UnityEngine.Random.InitState(_defenseSeed);
        }

        private static bool DefenseFlightsMoving()
        {
            foreach (var arrow in Spawner.Instance.ActiveArrows)
                if (arrow != null && arrow.GetComponent<ArrowSwarm.Arrow.ArrowMovement>().IsMoving) return true;
            return false;
        }

        private static Vector2 DefenseScreenHead(Arrow arrow)
        {
            var grid = GridManager.Instance;
            var world = grid.Origin + (Vector2)arrow.HeadPoint * grid.PointSpacing;
            var screen = UnityEngine.Camera.main.WorldToScreenPoint(world);
            return new Vector2(screen.x, Screen.height - screen.y);
        }

        private static int ChooseDefenseLastArrow()
        {
            var arrows = Spawner.Instance.ActiveArrows;
            int best = -1; float longest = -1;
            for (int index = 0; index < arrows.Count; index++)
            {
                if (!CanReserveDefenseLast(index)) continue;
                var arrow = arrows[index];
                var route = PathManager.Instance.GetArrowPathWaypoints(GridManager.Instance.GetGridExitPoint(arrow.HeadPoint, arrow.HeadDirection));
                float length = 0;
                for (int i = 1; i < route.Count; i++) length += Vector2.Distance(route[i - 1], route[i]);
                if (length > longest) { best = index; longest = length; }
            }
            if (best < 0) throw new InvalidOperationException("No legal reserved Defense final arrow.");
            return best;
        }

        private static bool CanReserveDefenseLast(int reserved)
        {
            var arrows = Spawner.Instance.ActiveArrows;
            var remaining = new HashSet<int>();
            for (int i = 0; i < arrows.Count; i++) remaining.Add(i);
            bool changed = true;
            while (changed && remaining.Count > 1)
            {
                changed = false;
                for (int i = 0; i < arrows.Count; i++)
                {
                    if (i == reserved || !remaining.Contains(i) || !ClearDefenseRay(arrows[i], remaining)) continue;
                    remaining.Remove(i); changed = true;
                }
            }
            return remaining.Count == 1;
        }

        private static bool ClearDefenseRay(Arrow arrow, HashSet<int> remaining)
        {
            var occupied = new HashSet<Vector2Int>();
            foreach (int i in remaining)
                foreach (var point in Spawner.Instance.ActiveArrows[i].PathPoints) occupied.Add(point);
            Vector2Int direction;
            switch (arrow.HeadDirection)
            {
                case ArrowSwarm.Arrow.ArrowDirection.Up: direction = Vector2Int.up; break;
                case ArrowSwarm.Arrow.ArrowDirection.Down: direction = Vector2Int.down; break;
                case ArrowSwarm.Arrow.ArrowDirection.Left: direction = Vector2Int.left; break;
                default: direction = Vector2Int.right; break;
            }
            var grid = GridManager.Instance;
            var position = arrow.HeadPoint + direction;
            while (position.x >= 0 && position.y >= 0 && position.x < grid.Width && position.y < grid.Height)
            {
                if (occupied.Contains(position)) return false;
                position += direction;
            }
            return true;
        }
    }
}
