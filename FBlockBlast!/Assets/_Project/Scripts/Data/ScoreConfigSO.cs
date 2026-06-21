using UnityEngine;

namespace NeonGalaxy.Data
{
    /// <summary>
    /// Scoring formula configuration. All tuning values for placement score,
    /// line clear score, combo multiplier, and batch quality bonus.
    /// Create instances via: Create → NeonGalaxy → Score Config.
    /// </summary>
    [CreateAssetMenu(fileName = "ScoreConfig", menuName = "NeonGalaxy/Score Config", order = 11)]
    public class ScoreConfigSO : ScriptableObject
    {
        [Header("Placement Score")]
        [Tooltip("Points awarded per cell when a piece is placed (10 × cellCount).")]
        public int pointsPerCell = 10;

        [Header("Line Clear Score")]
        [Tooltip("Base points for line clear. Formula: baseLineClearPoints × lines × lines.")]
        public int baseLineClearPoints = 100;

        [Header("Nova Cross")]
        [Tooltip("Flat bonus points awarded when a single placement clears both a row and a column.")]
        public int novaCrossBonus = 500;

        [Header("Combo Multiplier")]
        [Tooltip("Multiplier values indexed by combo count (0, 1, 2, ...). Index beyond array uses overflow formula.")]
        public float[] comboMultiplierTable = new float[]
        {
            1.0f, // combo 0
            1.0f, // combo 1
            1.2f, // combo 2
            1.4f, // combo 3
            1.6f, // combo 4
            1.8f, // combo 5
            2.0f, // combo 6
            2.2f, // combo 7
            2.5f, // combo 8
            2.8f, // combo 9
            3.0f  // combo 10
        };

        [Tooltip("Base multiplier for combos beyond the table.")]
        public float comboOverflowBase = 3.0f;

        [Tooltip("Additional multiplier per combo level beyond the table.")]
        public float comboOverflowStep = 0.1f;

        [Tooltip("Maximum combo multiplier cap.")]
        public float comboMultiplierCap = 5.0f;

        [Header("Batch Quality Bonus")]
        [Tooltip("Bonus points indexed by total lines cleared in the batch. Index beyond array uses overflow formula.")]
        public int[] batchBonusTable = new int[]
        {
            0,    // 0 lines
            0,    // 1 line
            50,   // 2 lines
            150,  // 3 lines
            300,  // 4 lines
        };

        [Tooltip("Base bonus for batch clears beyond the table.")]
        public int batchBonusOverflowBase = 300;

        [Tooltip("Additional bonus per line beyond the table range.")]
        public int batchBonusOverflowStep = 100;

        /// <summary>
        /// Returns the combo multiplier for the given combo count.
        /// Uses the lookup table for low values, linear formula for high values, capped.
        /// </summary>
        public float GetComboMultiplier(int combo)
        {
            if (combo < 0) return 1.0f;

            if (combo < comboMultiplierTable.Length)
                return comboMultiplierTable[combo];

            int overflow = combo - (comboMultiplierTable.Length - 1);
            float multiplier = comboOverflowBase + overflow * comboOverflowStep;
            return Mathf.Min(multiplier, comboMultiplierCap);
        }

        /// <summary>
        /// Returns the batch quality bonus for the given number of total lines cleared in one batch.
        /// </summary>
        public int GetBatchBonus(int linesCleared)
        {
            if (linesCleared < 0) return 0;

            if (linesCleared < batchBonusTable.Length)
                return batchBonusTable[linesCleared];

            int overflow = linesCleared - (batchBonusTable.Length - 1);
            return batchBonusOverflowBase + overflow * batchBonusOverflowStep;
        }

        /// <summary>
        /// Calculates the placement score for a piece with the given cell count.
        /// </summary>
        public int CalculatePlacementScore(int cellCount)
        {
            return pointsPerCell * cellCount;
        }

        /// <summary>
        /// Calculates the base line clear score (before combo multiplier).
        /// Uses quadratic scaling: baseLineClearPoints × lines².
        /// </summary>
        public int CalculateLineClearBase(int linesCleared)
        {
            return baseLineClearPoints * linesCleared * linesCleared;
        }
    }
}
