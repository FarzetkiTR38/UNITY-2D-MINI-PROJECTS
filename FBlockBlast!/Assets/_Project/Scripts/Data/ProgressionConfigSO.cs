using UnityEngine;

namespace NeonGalaxy.Data
{
    /// <summary>
    /// Configuration for player progression: XP conversion, level curve, and max level.
    /// Create instances via: Create → NeonGalaxy → Progression Config.
    /// </summary>
    [CreateAssetMenu(fileName = "ProgressionConfig", menuName = "NeonGalaxy/Progression Config", order = 13)]
    public class ProgressionConfigSO : ScriptableObject
    {
        [Header("XP Conversion")]
        [Tooltip("XP earned from a run = floor(runScore / scoreDivisor).")]
        public int scoreDivisor = 100;

        [Header("Level Curve")]
        [Tooltip("Base XP cost per level. Formula: baseXPPerLevel × n × (n+1) / 2 for cumulative XP at level n.")]
        public int baseXPPerLevel = 100;

        [Tooltip("Maximum achievable level in MVP.")]
        public int maxLevel = 50;

        /// <summary>
        /// Calculates the XP earned from a given run score.
        /// </summary>
        public int ScoreToXP(int runScore)
        {
            if (runScore <= 0 || scoreDivisor <= 0) return 0;
            return runScore / scoreDivisor;
        }

        /// <summary>
        /// Returns the cumulative XP required to reach the given level.
        /// Uses triangular number formula: baseXPPerLevel × n × (n+1) / 2.
        /// </summary>
        public int GetCumulativeXPForLevel(int level)
        {
            if (level <= 0) return 0;
            return baseXPPerLevel * level * (level + 1) / 2;
        }

        /// <summary>
        /// Returns the XP required to go from (level-1) to (level).
        /// </summary>
        public int GetXPForLevel(int level)
        {
            if (level <= 0) return 0;
            return baseXPPerLevel * level;
        }

        /// <summary>
        /// Calculates the current level from total accumulated XP.
        /// Inverse of triangular number: level = floor((-1 + sqrt(1 + 8*xp/base)) / 2).
        /// </summary>
        public int GetLevelFromTotalXP(int totalXP)
        {
            if (totalXP <= 0 || baseXPPerLevel <= 0) return 0;

            // Solve: baseXPPerLevel * n * (n+1) / 2 <= totalXP
            // n^2 + n - 2*totalXP/base <= 0
            float discriminant = 1f + 8f * totalXP / baseXPPerLevel;
            int level = Mathf.FloorToInt((-1f + Mathf.Sqrt(discriminant)) / 2f);
            return Mathf.Clamp(level, 0, maxLevel);
        }

        /// <summary>
        /// Returns the XP progress within the current level (0 to XPForNextLevel-1).
        /// </summary>
        public int GetXPProgressInLevel(int totalXP)
        {
            int currentLevel = GetLevelFromTotalXP(totalXP);
            int cumulativeXPForCurrentLevel = GetCumulativeXPForLevel(currentLevel);
            return totalXP - cumulativeXPForCurrentLevel;
        }

        /// <summary>
        /// Returns progress toward the next level as a 0–1 float.
        /// </summary>
        public float GetLevelProgressNormalized(int totalXP)
        {
            int currentLevel = GetLevelFromTotalXP(totalXP);
            if (currentLevel >= maxLevel) return 1f;

            int xpInLevel = GetXPProgressInLevel(totalXP);
            int xpNeeded = GetXPForLevel(currentLevel + 1);
            if (xpNeeded <= 0) return 1f;

            return Mathf.Clamp01((float)xpInLevel / xpNeeded);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (scoreDivisor < 1) scoreDivisor = 1;
            if (baseXPPerLevel < 1) baseXPPerLevel = 1;
            if (maxLevel < 1) maxLevel = 1;
        }
#endif
    }
}
