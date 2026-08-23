namespace ArrowSwarm.Core
{
    using System.Collections.Generic;
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Static utility class that validates whether a generated level is solvable.
    /// Runs a simulation: iteratively finds and "fires" all arrows whose heads
    /// are on the grid edge AND facing outward.
    /// Also checks winability (total arrow damage vs total mob HP).
    /// </summary>
    public static class SolvabilityChecker
    {
        /// <summary>
        /// Data structure representing a multi-point arrow placement for simulation.
        /// </summary>
        public struct ArrowPlacement
        {
            /// <summary>All grid points this arrow occupies. First = head, last = tail.</summary>
            public List<Vector2Int> PathPoints;

            /// <summary>Direction the arrow head faces.</summary>
            public ArrowDirection HeadDirection;

            /// <summary>Weight = number of segments = PathPoints.Count - 1.</summary>
            public int Weight => Mathf.Max(1, PathPoints.Count - 1);

            /// <summary>The head (tip) point of the arrow.</summary>
            public Vector2Int HeadPoint => PathPoints[0];

            public ArrowPlacement(List<Vector2Int> pathPoints, ArrowDirection headDir)
            {
                PathPoints = pathPoints;
                HeadDirection = headDir;
            }
        }

        /// <summary>
        /// Result of a solvability check.
        /// </summary>
        public struct SolvabilityResult
        {
            public bool IsSolvable;
            public bool IsWinnable;
            public int TotalArrowDamage;
            public int TotalMobHP;
            public int FiringSteps;

            public bool IsValid => IsSolvable && IsWinnable;
        }

        /// <summary>
        /// Checks if the given arrow configuration is solvable.
        /// An arrow can fire if its head is on the grid edge AND facing outward.
        /// Firing removes all its occupied points from the grid.
        /// </summary>
        public static SolvabilityResult Check(
            List<ArrowPlacement> arrows,
            int gridWidth, int gridHeight,
            int totalMobHP, float winabilityRatio)
        {
            SolvabilityResult result = new SolvabilityResult();

            // Calculate total arrow damage
            int totalDamage = 0;
            for (int i = 0; i < arrows.Count; i++)
            {
                totalDamage += arrows[i].Weight;
            }
            result.TotalArrowDamage = totalDamage;
            result.TotalMobHP = totalMobHP;

            // Build occupied point set for simulation
            HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
            List<ArrowPlacement> remaining = new List<ArrowPlacement>(arrows);

            for (int i = 0; i < remaining.Count; i++)
            {
                var points = remaining[i].PathPoints;
                for (int j = 0; j < points.Count; j++)
                {
                    occupied.Add(points[j]);
                }
            }

            // Iteratively fire arrows whose heads face outward from the edge
            int steps = 0;
            bool progress = true;

            while (progress && remaining.Count > 0)
            {
                progress = false;
                steps++;

                for (int i = remaining.Count - 1; i >= 0; i--)
                {
                    ArrowPlacement arrow = remaining[i];
                    if (CanFireSim(arrow, gridWidth, gridHeight, occupied))
                    {
                        // Fire: remove all points from occupied
                        var points = arrow.PathPoints;
                        for (int j = 0; j < points.Count; j++)
                        {
                            occupied.Remove(points[j]);
                        }
                        remaining.RemoveAt(i);
                        progress = true;
                    }
                }
            }

            result.IsSolvable = remaining.Count == 0;
            result.FiringSteps = steps;

            // Winability check
            result.IsWinnable = totalDamage >= Mathf.FloorToInt(totalMobHP * winabilityRatio);

            LogDebug($"Solvability: {(result.IsSolvable ? "PASS" : "FAIL")} " +
                     $"(remaining={remaining.Count}, steps={steps}), " +
                     $"Winnable: {(result.IsWinnable ? "PASS" : "FAIL")} " +
                     $"(damage={totalDamage}, mobHP={totalMobHP}, ratio={winabilityRatio})");

            return result;
        }

        /// <summary>
        /// Checks if an arrow can be fired in the simulation.
        /// Condition: Path must be clear to the edge of the grid.
        /// </summary>
        private static bool CanFireSim(
            ArrowPlacement arrow, int gridWidth, int gridHeight, HashSet<Vector2Int> occupied)
        {
            Vector2Int current = arrow.HeadPoint;
            Vector2Int step = ArrowSwarm.Grid.GridManager.DirectionToVector(arrow.HeadDirection);
            if (step == Vector2Int.zero) return false;
            
            current += step;
            while (current.IsInBounds(gridWidth, gridHeight))
            {
                if (occupied.Contains(current))
                {
                    return false;
                }
                current += step;
            }
            return true;
        }

        /// <summary>
        /// Validates that no two placements share the same grid coordinate.
        /// Returns true if all arrow placements have strictly disjoint grid cells.
        /// </summary>
        public static bool ValidateNoOverlaps(List<ArrowPlacement> placements)
        {
            if (placements == null) return true;
            var seen = new HashSet<Vector2Int>();
            for (int i = 0; i < placements.Count; i++)
            {
                var points = placements[i].PathPoints;
                if (points == null) continue;
                for (int j = 0; j < points.Count; j++)
                {
                    if (seen.Contains(points[j]))
                    {
                        Debug.LogError($"[ArrowSwarm] SolvabilityChecker OVERLAP DETECTED at {points[j]} in placement {i}!");
                        return false;
                    }
                    seen.Add(points[j]);
                }
            }
            return true;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private static void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] SolvabilityChecker: {message}");
        }
    }
}
