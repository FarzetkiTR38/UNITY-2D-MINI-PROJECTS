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
        /// Calculates the minimum arrow weight for a given level.
        /// Always 1 for maximum playability.
        /// </summary>
        public static int GetMinWeight(int level)
        {
            return 1;
        }

        /// <summary>
        /// Calculates the maximum arrow weight for a given level based on its active map (Golden Ratio curve):
        /// - Map 1  (Index 0)  → Max Weight 5  (Weight 1–5,  2–6 points)
        /// - Map 2  (Index 1)  → Max Weight 6  (Weight 1–6,  2–7 points)
        /// - Map 3  (Index 2)  → Max Weight 7  (Weight 1–7,  2–8 points)
        /// - Map 4  (Index 3)  → Max Weight 8  (Weight 1–8,  2–9 points)
        /// - Map 5  (Index 4)  → Max Weight 10 (Weight 1–10, 2–11 points)
        /// - Map 6  (Index 5)  → Max Weight 12 (Weight 1–12, 2–13 points)
        /// - Map 7  (Index 6)  → Max Weight 15 (Weight 1–15, 2–16 points)
        /// - Map 8  (Index 7)  → Max Weight 18 (Weight 1–18, 2–19 points)
        /// - Map 9  (Index 8)  → Max Weight 22 (Weight 1–22, 2–23 points)
        /// - Map 10 (Index 9)  → Max Weight 26 (Weight 1–26, 2–27 points)
        /// - Map 11 (Index 10) → Max Weight 30 (Weight 1–30, 2–31 points)
        /// - Map 12 (Index 11) → Max Weight 35 (Weight 1–35, 2–36 points - Mega Boss Maze)
        /// </summary>
        public static int GetMaxWeight(int level)
        {
            int mapIndex = GetMapIndex(level);
            return mapIndex switch
            {
                0 => 5,   // Map 1  (Weight 1–5)
                1 => 6,   // Map 2  (Weight 1–6)
                2 => 7,   // Map 3  (Weight 1–7)
                3 => 8,   // Map 4  (Weight 1–8)
                4 => 10,  // Map 5  (Weight 1–10)
                5 => 12,  // Map 6  (Weight 1–12)
                6 => 15,  // Map 7  (Weight 1–15)
                7 => 18,  // Map 8  (Weight 1–18)
                8 => 22,  // Map 9  (Weight 1–22)
                9 => 26,  // Map 10 (Weight 1–26)
                10 => 30, // Map 11 (Weight 1–30)
                11 => 35, // Map 12 (Weight 1–35)
                _ => 5
            };
        }

        /// <summary>
        /// Calculates which map index (0 to 11 for Map 1 to Map 12) is active for the given level.
        /// Hierarchy / Override Rule (Higher-level milestones override lower rules):
        /// 1. Priority 1 (Top Override): From Level 100 onwards, every 50 levels -> Map 12 (Index 11, e.g. 100, 150, 200, 250, 300, 350...)
        /// 2. Priority 2 (Override): From Level 50 onwards, every 10 levels -> Map 11 (Index 10, e.g. 50, 60, 70, 80, 90, 110, 120, 130, 140, 160...)
        /// 3. Priority 3: Levels 1-25 -> Map 1 to Map 5 (5 levels each, Indices 0 to 4)
        /// 4. Priority 4: Levels 26+ -> 5-map rotating cycle (Map 6 to Map 10, Indices 5 to 9):
        ///   - Level % 5 == 0 → Map 6 (Index 5, e.g. 30, 35, 40, 45, 55, 65...)
        ///   - Level % 5 == 1 → Map 7 (Index 6, e.g. 26, 31, 36, 41, 46, 51, 56...)
        ///   - Level % 5 == 2 → Map 8 (Index 7, e.g. 27, 32, 37, 42, 47, 52, 57...)
        ///   - Level % 5 == 3 → Map 9 (Index 8, e.g. 28, 33, 38, 43, 48, 53, 58...)
        ///   - Level % 5 == 4 → Map 10 (Index 9, e.g. 29, 34, 39, 44, 49, 54, 59...)
        /// </summary>
        public static int GetMapIndex(int level)
        {
            if (level <= 0) return 0;

            // 1. Priority 1 (Top Override): From level 100 onwards, every 50 levels -> Map 12 (Index 11)
            // e.g. 100, 150, 200, 250, 300, 350...
            if (level >= 100 && level % 50 == 0)
            {
                return 11; // Map 12
            }

            // 2. Priority 2 (Override): From level 50 onwards, every 10 levels -> Map 11 (Index 10)
            // e.g. 50, 60, 70, 80, 90, 110, 120, 130, 140, 160...
            if (level >= 50 && level % 10 == 0)
            {
                return 10; // Map 11
            }

            // 3. Priority 3: Initial 25 levels -> Map 1 to Map 5 (5 levels each)
            if (level <= 25)
            {
                return (level - 1) / 5; // 0 to 4 (Map 1 to Map 5)
            }

            // 4. Priority 4: Rotating cycle for levels >= 26 -> Map 6 to Map 10 (Indices 5 to 9)
            int remainder = level % 5;
            return 5 + remainder;
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
