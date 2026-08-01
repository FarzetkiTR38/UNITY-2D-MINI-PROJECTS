namespace ArrowSwarm.Core
{
    using System.Collections.Generic;
    using ArrowSwarm.Arrow;
    using UnityEngine;

    /// <summary>
    /// Static utility class that validates whether a generated level is solvable.
    /// Runs a simulation: iteratively finds and "fires" all arrows that have clear paths.
    /// Also checks winability (total arrow damage vs total mob HP).
    /// </summary>
    public static class SolvabilityChecker
    {
        /// <summary>
        /// Data structure representing an arrow's placement for simulation.
        /// </summary>
        public struct ArrowPlacement
        {
            public Vector2Int Position;
            public ArrowDirection Direction;
            public int Weight;

            public ArrowPlacement(Vector2Int pos, ArrowDirection dir, int weight)
            {
                Position = pos;
                Direction = dir;
                Weight = weight;
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
            public int FiringSteps; // How many iterations it took

            public bool IsValid => IsSolvable && IsWinnable;
        }

        /// <summary>
        /// Checks if the given arrow configuration is solvable.
        /// All arrows must be fireable through iterative clearing.
        /// </summary>
        /// <param name="arrows">List of arrow placements to validate.</param>
        /// <param name="gridWidth">Grid width.</param>
        /// <param name="gridHeight">Grid height.</param>
        /// <param name="totalMobHP">Total HP of all mobs in the level.</param>
        /// <param name="winabilityRatio">Minimum ratio of arrow damage to mob HP.</param>
        /// <returns>SolvabilityResult with details.</returns>
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

            // Build grid simulation
            bool[,] occupied = new bool[gridWidth, gridHeight];
            List<ArrowPlacement> remaining = new List<ArrowPlacement>(arrows);

            for (int i = 0; i < remaining.Count; i++)
            {
                var a = remaining[i];
                if (a.Position.x >= 0 && a.Position.x < gridWidth &&
                    a.Position.y >= 0 && a.Position.y < gridHeight)
                {
                    occupied[a.Position.x, a.Position.y] = true;
                }
            }

            // Iteratively fire arrows with clear paths
            int steps = 0;
            bool progress = true;

            while (progress && remaining.Count > 0)
            {
                progress = false;
                steps++;

                for (int i = remaining.Count - 1; i >= 0; i--)
                {
                    ArrowPlacement arrow = remaining[i];
                    if (IsPathClearSim(arrow.Position, arrow.Direction, occupied, gridWidth, gridHeight))
                    {
                        // Fire this arrow — remove from grid
                        occupied[arrow.Position.x, arrow.Position.y] = false;
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
        /// Checks if the path from a position in the given direction
        /// is clear in the simulation grid.
        /// </summary>
        private static bool IsPathClearSim(
            Vector2Int from, ArrowDirection direction,
            bool[,] occupied, int gridWidth, int gridHeight)
        {
            Vector2Int step = DirectionToStep(direction);
            Vector2Int current = from + step;

            while (current.x >= 0 && current.x < gridWidth &&
                   current.y >= 0 && current.y < gridHeight)
            {
                if (occupied[current.x, current.y])
                {
                    return false;
                }
                current += step;
            }

            return true;
        }

        private static Vector2Int DirectionToStep(ArrowDirection direction)
        {
            return direction switch
            {
                ArrowDirection.Up => Vector2Int.up,
                ArrowDirection.Down => Vector2Int.down,
                ArrowDirection.Left => Vector2Int.left,
                ArrowDirection.Right => Vector2Int.right,
                _ => Vector2Int.zero
            };
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private static void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] SolvabilityChecker: {message}");
        }
    }
}
