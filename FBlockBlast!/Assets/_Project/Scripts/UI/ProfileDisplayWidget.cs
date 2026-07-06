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
        [SerializeField] private Sprite defaultAvatarSprite; // Varsayılan profil fotoğrafı için eklendi

        [Header("Text Fields")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI levelText;       // "Level 12 -> 13" bar altındaki text
        [SerializeField] private TextMeshProUGUI levelBadgeText;  // Profil fotoğrafı üstündeki level badge (sadece "12")
        [SerializeField] private TextMeshProUGUI xpText;
        [SerializeField] private TextMeshProUGUI bestScoreText;

        [Header("Progress Bar")]
        [SerializeField] private Image xpFillImage;

        // ── Lifecycle ────────────────────────────────────────────

        private void Awake()
        {
            // "Slide yok aslında görsel var" dediğiniz için görselin Fill özelliklerini garantiye alıyoruz
            if (xpFillImage != null)
            {
                // Unity UI Image requires a Sprite for 'Filled' type to work. 
                // If it's null, the image is drawn as a solid quad (always full).
                if (xpFillImage.sprite == null)
                {
                    Texture2D tex = new Texture2D(1, 1);
                    tex.SetPixel(0, 0, Color.white);
                    tex.Apply();
                    xpFillImage.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
                    Debug.LogWarning("[ProfileDisplayWidget] xpFillImage had no sprite! A default white sprite was created so FillAmount works. Please assign a UI Sprite in the inspector.");
                }

                xpFillImage.type = Image.Type.Filled;
                xpFillImage.fillMethod = Image.FillMethod.Horizontal;
                xpFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            }
        }

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
                Sprite spriteToUse = defaultAvatarSprite; // Varsayılanı ata
                var profileManager = ServiceLocator.Get<ProfileManager>();
                if (profileManager != null)
                {
                    var fetchedSprite = profileManager.GetCurrentAvatarSprite();
                    if (fetchedSprite != null) spriteToUse = fetchedSprite;
                }
                
                if (spriteToUse != null)
                    profileAvatarImage.sprite = spriteToUse;
            }

            if (playerNameText != null)
                playerNameText.text = data.playerName;

            int currentLevel = progressionManager.GetCurrentLevel();

            // Bar altındaki level progression text: "Level 12 -> 13"
            if (levelText != null)
                levelText.text = $"Level {currentLevel} -> {currentLevel + 1}";

            // Profil fotoğrafı üstündeki badge: sadece level numarası
            if (levelBadgeText != null)
                levelBadgeText.text = currentLevel.ToString();

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
