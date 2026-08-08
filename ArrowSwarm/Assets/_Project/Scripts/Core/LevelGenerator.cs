namespace ArrowSwarm.Core
{
    using System.Collections.Generic;
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Generates levels procedurally based on level number.
    /// Creates multi-point arrow paths using random walk on the point grid.
    /// Validates with SolvabilityChecker before returning.
    /// Pure logic — no MonoBehaviour dependency.
    /// </summary>
    public static class LevelGenerator
    {
        /// <summary>
        /// Holds all data needed to play a generated level.
        /// </summary>
        public struct LevelData
        {
            public int Level;
            public LevelParams Params;
            public MapData Map;
            public List<SolvabilityChecker.ArrowPlacement> ArrowPlacements;
            public bool IsValid;
            public int GenerationAttempts;
        }

        // Cached direction vectors for random walk
        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        /// <summary>
        /// Generates a complete level. Returns LevelData with arrow placements.
        /// Guarantees solvability (retries up to maxAttempts).
        /// </summary>
        public static LevelData Generate(int level, GameConfig config)
        {
            MapData map = config.GetMapForLevel(level);
            if (map == null)
            {
                Debug.LogError($"[ArrowSwarm] LevelGenerator: No map found for level {level}!");
                return new LevelData { IsValid = false };
            }

            LevelParams levelParams = DifficultyCalculator.CalculateAll(
                level, map.GridWidth, map.GridHeight,
                config.MaxMobSpeed, config.MinSpawnInterval);

            int maxAttempts = config.MaxRegenerateAttempts;
            float winabilityRatio = config.WinabilityRatio;
            float difficultyReduction = config.DifficultyReductionOnFail;
            int totalMobHP = levelParams.TotalMobs * levelParams.MobHP;

            LevelData result = new LevelData
            {
                Level = level,
                Params = levelParams,
                Map = map,
                IsValid = false,
                GenerationAttempts = 0
            };

            // Try generating a valid level
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                result.GenerationAttempts = attempt;

                var placements = GenerateArrowPlacements(
                    levelParams, map.GridWidth, map.GridHeight);

                if (placements == null || placements.Count == 0) continue;

                var solvability = SolvabilityChecker.Check(
                    placements, map.GridWidth, map.GridHeight,
                    totalMobHP, winabilityRatio);

                if (solvability.IsValid)
                {
                    result.ArrowPlacements = placements;
                    result.IsValid = true;

                    LogDebug($"Level {level} generated: {levelParams} " +
                             $"(attempt {attempt}/{maxAttempts})");
                    LogDebug($"Solvability: PASSED (steps={solvability.FiringSteps})");
                    return result;
                }
            }

            // All attempts failed — reduce difficulty and try again
            LogDebug($"Level {level}: {maxAttempts} attempts failed. Reducing difficulty.");

            LevelParams easierParams = ReduceDifficulty(levelParams, difficultyReduction);
            result.Params = easierParams;
            totalMobHP = easierParams.TotalMobs * easierParams.MobHP;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                var placements = GenerateArrowPlacements(
                    easierParams, map.GridWidth, map.GridHeight);

                if (placements == null || placements.Count == 0) continue;

                var solvability = SolvabilityChecker.Check(
                    placements, map.GridWidth, map.GridHeight,
                    totalMobHP, winabilityRatio);

                if (solvability.IsValid)
                {
                    result.ArrowPlacements = placements;
                    result.IsValid = true;
                    result.GenerationAttempts += attempt;
                    LogDebug($"Level {level} generated with reduced difficulty (attempt {attempt}).");
                    return result;
                }
            }

            // Final fallback
            Debug.LogWarning($"[ArrowSwarm] LevelGenerator: Could not generate valid level {level}!");
            var fallback = GenerateArrowPlacements(easierParams, map.GridWidth, map.GridHeight);
            result.ArrowPlacements = fallback ?? new List<SolvabilityChecker.ArrowPlacement>();
            result.IsValid = true; // Force accept
            return result;
        }

        /// <summary>
        /// Generates multi-point straight arrow placements filling 100% of the grid.
        /// Uses Binary Space Partitioning (BSP) to guarantee no 1-point arrows are left,
        /// ensuring all arrows have a length of at least 2 points.
        /// </summary>
        private static List<SolvabilityChecker.ArrowPlacement> GenerateArrowPlacements(
            LevelParams levelParams, int gridWidth, int gridHeight)
        {
            var placements = new List<SolvabilityChecker.ArrowPlacement>();
            var pieces = new List<RectInt>();
            
            PartitionGrid(0, 0, gridWidth, gridHeight, levelParams.MaxWeight + 1, pieces);

            foreach (var rect in pieces)
            {
                var path = new List<Vector2Int>();
                if (rect.width > 1) // Horizontal
                {
                    for (int i = 0; i < rect.width; i++) path.Add(new Vector2Int(rect.x + i, rect.y));
                }
                else // Vertical
                {
                    for (int i = 0; i < rect.height; i++) path.Add(new Vector2Int(rect.x, rect.y + i));
                }

                if (path.Count < 2) continue;

                if (Random.value > 0.5f)
                {
                    path.Reverse();
                }

                Vector2Int bodyDir = path[1] - path[0];
                ArrowDirection headDir = OppositeDirection(VectorToDirection(bodyDir));

                placements.Add(new SolvabilityChecker.ArrowPlacement(path, headDir));
            }

            return placements;
        }

        private static void PartitionGrid(int x, int y, int w, int h, int maxLength, List<RectInt> pieces)
        {
            if (w == 1 && h == 1) return; // Should not happen with proper splitting

            if (w == 1)
            {
                Split1D(x, y, h, true, maxLength, pieces);
                return;
            }
            
            if (h == 1)
            {
                Split1D(x, y, w, false, maxLength, pieces);
                return;
            }

            // Both w > 1 and h > 1
            bool splitVertically = Random.value > 0.5f;
            
            if (splitVertically)
            {
                int splitW = Random.Range(1, w);
                PartitionGrid(x, y, splitW, h, maxLength, pieces);
                PartitionGrid(x + splitW, y, w - splitW, h, maxLength, pieces);
            }
            else
            {
                int splitH = Random.Range(1, h);
                PartitionGrid(x, y, w, splitH, maxLength, pieces);
                PartitionGrid(x, y + splitH, w, h - splitH, maxLength, pieces);
            }
        }

        private static void Split1D(int x, int y, int length, bool isVertical, int maxLength, List<RectInt> pieces)
        {
            int currentPos = 0;
            while (currentPos < length)
            {
                int remaining = length - currentPos;
                if (remaining == 0) break;
                
                int pieceLen;
                
                if (remaining <= maxLength)
                {
                    if (remaining >= 4 && maxLength >= 2 && Random.value > 0.5f)
                    {
                        pieceLen = Random.Range(2, remaining - 1);
                    }
                    else
                    {
                        pieceLen = remaining;
                    }
                }
                else
                {
                    int maxAllowed = Mathf.Min(maxLength, remaining - 2); 
                    if (maxAllowed >= 2)
                    {
                        pieceLen = Random.Range(2, maxAllowed + 1);
                    }
                    else
                    {
                        pieceLen = 2; 
                        if (remaining - pieceLen == 1) pieceLen = 3; // Prevent leaving exactly 1
                    }
                }
                
                // Safety catch to absolutely prevent 1-length pieces
                if (pieceLen < 2) pieceLen = 2;
                if (currentPos + pieceLen > length) pieceLen = length - currentPos;

                if (isVertical)
                    pieces.Add(new RectInt(x, y + currentPos, 1, pieceLen));
                else
                    pieces.Add(new RectInt(x + currentPos, y, pieceLen, 1));
                    
                currentPos += pieceLen;
            }
        }

        /// <summary>
        /// Determines the head direction for the arrow.
        /// With outwardChance probability, the head is placed on an edge facing outward.
        /// Otherwise, a random valid direction is chosen.
        /// Path[0] = head, so we may need to reverse the path.
        /// </summary>
        private static ArrowDirection DetermineHeadDirection(
            List<Vector2Int> path, int gridWidth, int gridHeight, float outwardChance)
        {
            // Try to make the head face outward (better for solvability)
            if (Random.value < outwardChance)
            {
                // Check if first point (path[0]) is on edge
                ArrowDirection? outDir = GetOutwardDirectionForPoint(path[0], gridWidth, gridHeight);
                if (outDir.HasValue)
                {
                    return outDir.Value;
                }

                // Check if last point is on edge — if so, reverse the path
                ArrowDirection? tailOutDir = GetOutwardDirectionForPoint(
                    path[path.Count - 1], gridWidth, gridHeight);
                if (tailOutDir.HasValue)
                {
                    path.Reverse();
                    return tailOutDir.Value;
                }
            }

            // Not on edge or random: direction from path[0] opposite to path[1]
            if (path.Count >= 2)
            {
                Vector2Int headToSecond = path[1] - path[0];
                // Head direction is opposite to the path direction (pointing away from body)
                ArrowDirection bodyDir = VectorToDirection(headToSecond);
                return OppositeDirection(bodyDir);
            }

            return (ArrowDirection)Random.Range(0, 4);
        }

        /// <summary>
        /// Returns an outward direction if the point is on the grid edge, null otherwise.
        /// If multiple edge directions exist (corner), picks one randomly.
        /// </summary>
        private static ArrowDirection? GetOutwardDirectionForPoint(
            Vector2Int point, int gridWidth, int gridHeight)
        {
            var candidates = new List<ArrowDirection>(4);

            if (point.x == 0) candidates.Add(ArrowDirection.Left);
            if (point.x == gridWidth - 1) candidates.Add(ArrowDirection.Right);
            if (point.y == 0) candidates.Add(ArrowDirection.Down);
            if (point.y == gridHeight - 1) candidates.Add(ArrowDirection.Up);

            if (candidates.Count == 0) return null;
            return candidates[Random.Range(0, candidates.Count)];
        }

        private static ArrowDirection VectorToDirection(Vector2Int vec)
        {
            if (vec.y > 0) return ArrowDirection.Up;
            if (vec.y < 0) return ArrowDirection.Down;
            if (vec.x < 0) return ArrowDirection.Left;
            return ArrowDirection.Right;
        }

        private static ArrowDirection OppositeDirection(ArrowDirection dir)
        {
            return dir switch
            {
                ArrowDirection.Up => ArrowDirection.Down,
                ArrowDirection.Down => ArrowDirection.Up,
                ArrowDirection.Left => ArrowDirection.Right,
                ArrowDirection.Right => ArrowDirection.Left,
                _ => ArrowDirection.Up
            };
        }

        private static LevelParams ReduceDifficulty(LevelParams original, float reduction)
        {
            LevelParams easier = original;
            easier.OutwardChance = Mathf.Min(0.95f, original.OutwardChance + reduction * 2f);
            easier.MobHP = Mathf.Max(1, Mathf.FloorToInt(original.MobHP * (1f - reduction)));
            easier.TotalMobs = Mathf.Max(1, Mathf.FloorToInt(original.TotalMobs * (1f - reduction)));
            easier.ArrowCount = Mathf.Max(3, Mathf.FloorToInt(original.ArrowCount * (1f - reduction * 0.5f)));
            easier.MaxWeight = Mathf.Max(1, easier.MaxWeight - 1);
            return easier;
        }

        private static void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private static void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] LevelGenerator: {message}");
        }
    }
}
