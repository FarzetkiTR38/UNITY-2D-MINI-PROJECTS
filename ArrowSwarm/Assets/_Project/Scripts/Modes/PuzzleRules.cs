namespace ArrowSwarm.Modes
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>Pure point/edge collision and wall rules; contains no game services or scene state.</summary>
    public static class PuzzleRules
    {
        /// <summary>Reports the first occupied point on a head ray, including self obstruction.</summary>
        public static float ObstacleDistance(PuzzleLevel level, bool[] removed, int index)
        {
            var arrow = level.Arrows[index];
            var point = arrow.Head + arrow.Direction;
            int distance = 1;
            while (Inside(level, point))
            {
                for (int j = 0; j < level.Arrows.Count; j++)
                {
                    if (removed[j]) continue;
                    var path = level.Arrows[j].Points;
                    for (int k = j == index ? 1 : 0; k < path.Count; k++)
                        if (path[k] == point) return distance;
                }
                point += arrow.Direction; distance++;
            }
            return float.PositiveInfinity;
        }

        public static bool Inside(PuzzleLevel level, Vector2Int p) => p.x >= 0 && p.y >= 0 && p.x < level.Width && p.y < level.Height;
        public static int WallId(Vector2Int direction, int orientation) => ((direction.x == 0 ? 0 : 1) + orientation) & 1;
        public static bool MatchesWall(PuzzleLevel level, int index, int orientation) => level.Mode != GameModeId.ColorShift ||
            level.Arrows[index].ColorId == WallId(level.Arrows[index].Direction, orientation);

        /// <summary>A removable arrow has an unobstructed ray and the matching wall.</summary>
        public static bool CanRemove(PuzzleLevel level, bool[] removed, int index, int orientation) =>
            !removed[index] && MatchesWall(level, index, orientation) && float.IsPositiveInfinity(ObstacleDistance(level, removed, index));

        /// <summary>Defense's natural-neck rule: one unit edge directly behind the head continues its firing direction.</summary>
        public static bool HasNaturalNeck(PuzzleArrowData arrow) => arrow != null && arrow.Points != null &&
            arrow.Points.Count >= 2 && Mathf.Abs(arrow.Direction.x) + Mathf.Abs(arrow.Direction.y) == 1 &&
            arrow.Head - arrow.Points[1] == arrow.Direction;

        /// <summary>Rejects malformed paths before they reach the runtime or solver.</summary>
        public static string Validate(PuzzleLevel level)
        {
            if (level == null) return "Missing level.";
            if (level.Mode == GameModeId.Defense || level.Width < 2 || level.Height < 2 || level.Arrows == null || level.Arrows.Count == 0) return "Invalid mode, bounds or empty puzzle.";
            if (level.Mode == GameModeId.ColorShift && (level.WallColors == null || level.WallColors.Length != 2 || level.WallColors[0] == level.WallColors[1])) return "Two distinct wall colors required.";
            var occupied = new HashSet<Vector2Int>();
            foreach (var arrow in level.Arrows)
            {
                if (arrow == null || arrow.Points == null || arrow.Points.Count < 2 || Mathf.Abs(arrow.Direction.x) + Mathf.Abs(arrow.Direction.y) != 1) return "Invalid arrow path/direction.";
                if (!HasNaturalNeck(arrow)) return "Head direction must continue one unit of straight neck behind the head.";
                if (level.Mode == GameModeId.ColorShift && (arrow.ColorId < 0 || arrow.ColorId > 1)) return "Invalid wall color key.";
                for (int i = 0; i < arrow.Points.Count; i++)
                {
                    var p = arrow.Points[i];
                    if (!Inside(level, p) || !occupied.Add(p)) return "Off-board or overlapping path point.";
                    if (i > 0) { var delta = p - arrow.Points[i - 1]; if (Mathf.Abs(delta.x) + Mathf.Abs(delta.y) != 1) return "Paths must use unit orthogonal segments."; }
                }
            }
            return null;
        }
    }
}
