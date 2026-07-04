using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonGalaxy.Boot;
using NeonGalaxy.Services;
using NeonGalaxy.Meta;
using NeonGalaxy.Core;

namespace NeonGalaxy.UI
{
    /// <summary>
    /// Reusable widget that displays player profile information:
    /// avatar, name, level badge, XP progress bar, and best score.
    /// Used on Home Screen and Results Screen.
    /// </summary>
    public class ProfileDisplayWidget : MonoBehaviour
    {
        [Header("Avatar")]
        [SerializeField] private Image profileAvatarImage;

        [Header("Text Fields")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI xpText;
        [SerializeField] private TextMeshProUGUI bestScoreText;

        [Header("Progress Bar")]
        [SerializeField] private Image xpFillImage;

        // ── Lifecycle ────────────────────────────────────────────

        private void OnEnable()
        {
            GameEvents.OnProfileUpdated += Refresh;
        }

        private void OnDisable()
        {
            GameEvents.OnProfileUpdated -= Refresh;
        }

        // ── Public API ───────────────────────────────────────────

        /// <summary>
        /// Refreshes all profile fields from current save/progression data.
        /// Call this when the widget becomes visible or data changes.
        /// </summary>
        public void Refresh()
        {
            var saveService = ServiceLocator.Get<SaveService>();
            var progressionManager = ServiceLocator.Get<ProgressionManager>();

            if (saveService == null || progressionManager == null) return;

            var data = saveService.Data;

            // Avatar
            if (profileAvatarImage != null)
            {
                var profileManager = ServiceLocator.Get<ProfileManager>();
                if (profileManager != null)
                {
                    var sprite = profileManager.GetCurrentAvatarSprite();
                    if (sprite != null) profileAvatarImage.sprite = sprite;
                }
            }

            if (playerNameText != null)
                playerNameText.text = data.playerName;

            if (levelText != null)
                levelText.text = $"LV {progressionManager.GetCurrentLevel()}";

            if (bestScoreText != null)
                bestScoreText.text = $"BEST: {data.bestScore:N0}";

            RefreshXPBar(progressionManager);
        }

        /// <summary>
        /// Updates only the XP bar with animation-friendly normalized value.
        /// </summary>
        public void SetXPProgress(float normalized)
        {
            if (xpFillImage != null)
                xpFillImage.fillAmount = Mathf.Clamp01(normalized);
        }

        /// <summary>
        /// Updates the XP text display.
        /// </summary>
        public void SetXPText(int current, int needed)
        {
            if (xpText != null)
            {
                if (needed <= 0)
                    xpText.text = "MAX LEVEL";
                else
                    xpText.text = $"{current} / {needed} XP";
            }
        }

        /// <summary>
        /// Returns the avatar Image component for external button wiring.
        /// </summary>
        public Image GetAvatarImage() => profileAvatarImage;

        // ── Internal ─────────────────────────────────────────────

        private void RefreshXPBar(ProgressionManager progressionManager)
        {
            float normalized = progressionManager.GetXPProgressNormalized();
            int xpInLevel = progressionManager.GetXPProgressInLevel();
            int xpNeeded = progressionManager.GetXPNeededForNextLevel();

            SetXPProgress(normalized);
            SetXPText(xpInLevel, xpNeeded);
        }
    }
}
