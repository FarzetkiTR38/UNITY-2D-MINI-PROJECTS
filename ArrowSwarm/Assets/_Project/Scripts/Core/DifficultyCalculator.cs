namespace ArrowSwarm.Core
{
    using UnityEngine;

    /// <summary>
    /// Static utility class containing all difficulty scaling formulas.
    /// Takes a level number and returns calculated parameters.
    /// Pure functions — no state, no side effects.
    /// Weight now represents arrow segment count (path length).
    /// </summary>
    public static class DifficultyCalculator
    {
        /// <summary>
        /// Calculates the difficulty tier from the level number.
        /// Every 5 levels, tier increases by 1.
        /// </summary>
        public static int GetDifficultyTier(int level)
        {
            return Mathf.FloorToInt((level - 1) / 5f) + 1;
        }

        /// <summary>
        /// Calculates the number of arrows for a given level.
        /// Considers point grid capacity (each arrow uses weight+1 points).
        /// </summary>
        public static int GetArrowCount(int level, int gridWidth, int gridHeight)
        {
            int totalPoints = gridWidth * gridHeight;
            int avgWeight = GetMinWeight(level) + (GetMaxWeight(level) - GetMinWeight(level)) / 2;
            int avgPointsPerArrow = avgWeight + 1;
            
            // Target fill ratio: starts at 75% for level 1, goes up to 95% at high levels
            float fillRatio = Mathf.Min(0.95f, 0.75f + (level * 0.02f));
            int maxArrows = Mathf.FloorToInt((totalPoints * fillRatio) / avgPointsPerArrow);
            
            return Mathf.Max(5, maxArrows);
        }

        /// <summary>
        /// Calculates the chance that an arrow faces "outward" (edge, clear to fire).
        /// Higher values = easier (more arrows can fire directly).
        /// </summary>
        public static float GetOutwardChance(int level)
        {
            int tier = GetDifficultyTier(level);
            float chance = 0.70f - (tier - 1) * 0.01f;
            return Mathf.Max(0.25f, chance);
        }

        /// <summary>
        /// Calculates mob HP for a given level.
        /// </summary>
        public static int GetMobHP(int level)
        {
            int tier = GetDifficultyTier(level);
            return 5 + (tier - 1) * 2 + Mathf.FloorToInt(tier / 10f) * 5;
        }

        /// <summary>
        /// Calculates mob movement speed for a given level.
        /// Capped at maxMobSpeed from GameConfig.
        /// </summary>
        public static float GetMobSpeed(int level, float maxMobSpeed)
        {
            int tier = GetDifficultyTier(level);
            float speed = 1.0f + (tier - 1) * 0.1f + Mathf.FloorToInt(tier / 20f) * 0.5f;
            return Mathf.Min(speed, maxMobSpeed);
        }

        /// <summary>
        /// Calculates mob spawn interval (seconds between spawns).
        /// Lower = harder (more frequent spawning).
        /// </summary>
        public static float GetSpawnInterval(int level, float minSpawnInterval)
        {
            int tier = GetDifficultyTier(level);
            float interval = 3.0f - (tier - 1) * 0.05f;
            return Mathf.Max(minSpawnInterval, interval);
        }

        /// <summary>
        /// Calculates total number of mobs for a given level.
        /// </summary>
        public static int GetTotalMobs(int level)
        {
            int tier = GetDifficultyTier(level);
            return Mathf.FloorToInt(5 + tier * 1.2f + Mathf.FloorToInt(tier / 10f) * 3);
        }

        /// <summary>
        /// Calculates the minimum arrow weight (segment count) for a given level.
        /// Weight 1 = arrow spans 2 points, Weight 2 = 3 points, etc.
        /// </summary>
        public static int GetMinWeight(int level)
        {
            int tier = GetDifficultyTier(level);
            // Ensure minimum weight is at least 2 in later levels, maxing out at 4.
            return Mathf.Min(4, 2 + Mathf.FloorToInt(tier / 10f));
        }

        /// <summary>
        /// Calculates the maximum arrow weight (segment count) for a given level.
        /// Starts at 2, gradually increases to allow longer, more complex arrows.
        /// </summary>
        public static int GetMaxWeight(int level)
        {
            int tier = GetDifficultyTier(level);
            // Scales up to 10. (e.g. tier 1: max 4, tier 19: max 10)
            return Mathf.Min(10, 3 + Mathf.FloorToInt(tier / 3f));
        }

        /// <summary>
        /// Calculates which map index to use for the given level (cyclic, 5 maps).
        /// </summary>
        public static int GetMapIndex(int level)
        {
            return (level - 1) % 5;
        }

        /// <summary>
        /// Returns a complete LevelParams struct with all calculated values.
        /// </summary>
        public static LevelParams CalculateAll(int level, int gridWidth, int gridHeight,
            float maxMobSpeed, float minSpawnInterval)
        {
            return new LevelParams
            {
                Level = level,
                DifficultyTier = GetDifficultyTier(level),
                MapIndex = GetMapIndex(level),
                ArrowCount = GetArrowCount(level, gridWidth, gridHeight),
                OutwardChance = GetOutwardChance(level),
                MobHP = GetMobHP(level),
                MobSpeed = GetMobSpeed(level, maxMobSpeed),
                SpawnInterval = GetSpawnInterval(level, minSpawnInterval),
                TotalMobs = GetTotalMobs(level),
                MinWeight = GetMinWeight(level),
                MaxWeight = GetMaxWeight(level)
            };
        }
    }

    /// <summary>
    /// Holds all calculated parameters for a level.
    /// </summary>
    [System.Serializable]
    public struct LevelParams
    {
        /// <summary>Level number.</summary>
        public int Level;

        /// <summary>Difficulty tier (increases every 5 levels).</summary>
        public int DifficultyTier;

        /// <summary>Map index (0-4, cyclic).</summary>
        public int MapIndex;

        /// <summary>Number of arrows to place on the grid.</summary>
        public int ArrowCount;

        /// <summary>Probability that an arrow faces outward (easier).</summary>
        public float OutwardChance;

        /// <summary>Hit points for each mob.</summary>
        public int MobHP;

        /// <summary>Mob movement speed in units/second.</summary>
        public float MobSpeed;

        /// <summary>Seconds between mob spawns.</summary>
        public float SpawnInterval;

        /// <summary>Total number of mobs to spawn.</summary>
        public int TotalMobs;

        /// <summary>Minimum arrow weight (segment count).</summary>
        public int MinWeight;

        /// <summary>Maximum arrow weight (segment count).</summary>
        public int MaxWeight;

        /// <summary>
        /// Formatted string representation for debug logging.
        /// </summary>
        public override string ToString()
        {
            return $"Level={Level}, Tier={DifficultyTier}, Map={MapIndex}, " +
                   $"Arrows={ArrowCount}, Outward={OutwardChance:P0}, " +
                   $"MobHP={MobHP}, MobSpd={MobSpeed:F1}, " +
                   $"SpawnInt={SpawnInterval:F2}s, Mobs={TotalMobs}, " +
                   $"Weight={MinWeight}-{MaxWeight}";
        }
    }
}
