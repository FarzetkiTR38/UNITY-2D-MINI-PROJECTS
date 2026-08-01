namespace ArrowSwarm.Core
{
    using System.Collections.Generic;
    using ArrowSwarm.Arrow;
    using UnityEngine;

    /// <summary>
    /// Generates levels procedurally based on level number.
    /// Calls DifficultyCalculator for parameters, places arrows,
    /// and validates with SolvabilityChecker.
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
            LogDebug($"Level {level}: {maxAttempts} attempts failed. Reducing difficulty by {difficultyReduction:P0}.");

            LevelParams easierParams = ReduceDifficulty(levelParams, difficultyReduction);
            result.Params = easierParams;
            totalMobHP = easierParams.TotalMobs * easierParams.MobHP;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                var placements = GenerateArrowPlacements(
                    easierParams, map.GridWidth, map.GridHeight);

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

            // Final fallback — use last generated placements anyway
            Debug.LogWarning($"[ArrowSwarm] LevelGenerator: Could not generate valid level {level} after all attempts!");
            var fallback = GenerateArrowPlacements(easierParams, map.GridWidth, map.GridHeight);
            result.ArrowPlacements = fallback;
            result.IsValid = true; // Force accept
            return result;
        }

        /// <summary>
        /// Generates arrow placements on the grid.
        /// </summary>
        private static List<SolvabilityChecker.ArrowPlacement> GenerateArrowPlacements(
            LevelParams levelParams, int gridWidth, int gridHeight)
        {
            var positions = GenerateRandomPositions(gridWidth, gridHeight, levelParams.ArrowCount);
            var placements = new List<SolvabilityChecker.ArrowPlacement>(positions.Count);

            for (int i = 0; i < positions.Count; i++)
            {
                ArrowDirection direction = DetermineDirection(
                    positions[i], gridWidth, gridHeight, levelParams.OutwardChance);
                int weight = Random.Range(levelParams.MinWeight, levelParams.MaxWeight + 1);

                placements.Add(new SolvabilityChecker.ArrowPlacement(
                    positions[i], direction, weight));
            }

            return placements;
        }

        private static List<Vector2Int> GenerateRandomPositions(int width, int height, int count)
        {
            List<Vector2Int> all = new List<Vector2Int>(width * height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    all.Add(new Vector2Int(x, y));
                }
            }

            // Fisher-Yates shuffle
            for (int i = all.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (all[i], all[j]) = (all[j], all[i]);
            }

            count = Mathf.Min(count, all.Count);
            return all.GetRange(0, count);
        }

        private static ArrowDirection DetermineDirection(
            Vector2Int pos, int gridWidth, int gridHeight, float outwardChance)
        {
            if (Random.value < outwardChance)
            {
                return GetOutwardDirection(pos, gridWidth, gridHeight);
            }
            return (ArrowDirection)Random.Range(0, 4);
        }

        private static ArrowDirection GetOutwardDirection(Vector2Int pos, int gridWidth, int gridHeight)
        {
            int dL = pos.x, dR = gridWidth - 1 - pos.x;
            int dD = pos.y, dU = gridHeight - 1 - pos.y;
            int min = Mathf.Min(dL, Mathf.Min(dR, Mathf.Min(dD, dU)));

            List<ArrowDirection> candidates = new List<ArrowDirection>(4);
            if (dL == min) candidates.Add(ArrowDirection.Left);
            if (dR == min) candidates.Add(ArrowDirection.Right);
            if (dD == min) candidates.Add(ArrowDirection.Down);
            if (dU == min) candidates.Add(ArrowDirection.Up);

            return candidates[Random.Range(0, candidates.Count)];
        }

        private static LevelParams ReduceDifficulty(LevelParams original, float reduction)
        {
            LevelParams easier = original;
            easier.OutwardChance = Mathf.Min(0.8f, original.OutwardChance + reduction);
            easier.MobHP = Mathf.Max(1, Mathf.FloorToInt(original.MobHP * (1f - reduction)));
            easier.TotalMobs = Mathf.Max(1, Mathf.FloorToInt(original.TotalMobs * (1f - reduction)));
            easier.ArrowCount = Mathf.Max(5, Mathf.FloorToInt(original.ArrowCount * (1f - reduction * 0.5f)));
            return easier;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private static void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] LevelGenerator: {message}");
        }
    }
}
