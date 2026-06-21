using UnityEngine;

namespace NeonGalaxy.Data
{
    /// <summary>
    /// Configuration for the combo system: growth rates per batch quality
    /// and Nova Cross bonus. Create instances via: Create → NeonGalaxy → Combo Config.
    /// </summary>
    [CreateAssetMenu(fileName = "ComboConfig", menuName = "NeonGalaxy/Combo Config", order = 12)]
    public class ComboConfigSO : ScriptableObject
    {
        [Header("Combo Growth Per Batch")]
        [Tooltip("Combo increment when batch clears 1–2 lines total.")]
        public int comboIncrementFor1to2Lines = 1;

        [Tooltip("Combo increment when batch clears exactly 3 lines total.")]
        public int comboIncrementFor3Lines = 2;

        [Tooltip("Combo increment when batch clears 4+ lines total.")]
        public int comboIncrementFor4PlusLines = 3;

        [Header("Nova Cross Bonus")]
        [Tooltip("Additional combo increment when any placement in the batch triggers a Nova Cross.")]
        public int novaCrossBonusComboIncrement = 1;

        [Header("Combo Reset")]
        [Tooltip("If true, combo resets to 0 when a full batch produces zero line clears.")]
        public bool resetOnZeroClearBatch = true;

        /// <summary>
        /// Returns the base combo increment for a batch based on total lines cleared.
        /// Does NOT include Nova Cross bonus.
        /// </summary>
        public int GetComboIncrement(int totalLinesCleared)
        {
            if (totalLinesCleared <= 0) return 0;
            if (totalLinesCleared <= 2) return comboIncrementFor1to2Lines;
            if (totalLinesCleared == 3) return comboIncrementFor3Lines;
            return comboIncrementFor4PlusLines;
        }
    }
}
