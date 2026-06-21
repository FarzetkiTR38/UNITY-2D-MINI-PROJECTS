using UnityEngine;

namespace NeonGalaxy.Data
{
    /// <summary>
    /// Defines a single achievement and its unlock condition.
    /// Create instances via: Create → NeonGalaxy → Achievement Definition.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAchievement", menuName = "NeonGalaxy/Achievement Definition", order = 21)]
    public class AchievementDefinitionSO : ScriptableObject
    {
        [Tooltip("Unique identifier for save/load reference.")]
        public string achievementId;

        [Tooltip("Display name in UI.")]
        public string displayName;

        [Tooltip("Description of how to earn this achievement.")]
        [TextArea(1, 3)]
        public string description;

        [Tooltip("Icon for the achievement badge.")]
        public Sprite icon;

        [Tooltip("Stat key to check (e.g., 'bestCombo', 'totalNovaCrosses', 'bestScore', 'totalRuns').")]
        public string statKey;

        [Tooltip("Threshold value. Achievement unlocks when stat >= this value.")]
        public int threshold;

        [Tooltip("Cosmetic reward granted on unlock. Can be null.")]
        public CosmeticItemSO rewardCosmetic;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(achievementId))
                achievementId = name;
        }
#endif
    }
}
