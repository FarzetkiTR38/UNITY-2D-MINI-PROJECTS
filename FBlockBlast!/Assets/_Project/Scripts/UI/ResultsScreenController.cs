using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonGalaxy.Boot;
using NeonGalaxy.Services;
using NeonGalaxy.Meta;
using NeonGalaxy.Core;
using NeonGalaxy.Utility;

namespace NeonGalaxy.UI
{
    /// <summary>
    /// Full results screen shown after game over.
    /// Replaces the simple GameOverPopup with XP progression,
    /// achievement unlocks, ad rewards, and navigation.
    /// </summary>
    public class ResultsScreenController : MonoBehaviour
    {
        [Header("Score Display")]
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI bestScoreText;
        [SerializeField] private GameObject newBestBadge;

        [Header("XP Display")]
        [SerializeField] private TextMeshProUGUI xpEarnedText;
        [SerializeField] private Image xpBarFill;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private GameObject levelUpBanner;

        [Header("Achievements")]
        [SerializeField] private Transform achievementListParent;
        [SerializeField] private TextMeshProUGUI achievementEntryPrefabText;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI linesText;
        [SerializeField] private TextMeshProUGUI comboText;

        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button doubleXPButton;
        [SerializeField] private TextMeshProUGUI doubleXPButtonText;

        [Header("Animation")]
        [SerializeField] private float countUpDuration = 1.0f;
        [SerializeField] private float xpBarAnimDuration = 0.8f;

        public event System.Action OnRetryClicked;
        public event System.Action OnHomeClicked;

        private RunProgressionResult _progressionResult;
        private int _finalScore;
        private bool _xpDoubled;

        private void Awake()
        {
            if (retryButton != null)
                retryButton.onClick.AddListener(() => OnRetryClicked?.Invoke());

            if (homeButton != null)
                homeButton.onClick.AddListener(() => OnHomeClicked?.Invoke());

            if (doubleXPButton != null)
                doubleXPButton.onClick.AddListener(OnDoubleXPClicked);
        }

        // ── Public API ───────────────────────────────────────────

        /// <summary>
        /// Shows the results screen with full progression data.
        /// Call after game over with the final score.
        /// </summary>
        public void Show(int finalScore, int bestScore, bool isNewBest,
                         int linesCleared, int bestCombo)
        {
            gameObject.SetActive(true);
            transform.localScale = Vector3.one; // FIX: Prevent invisible popup on second open
            
            _finalScore = finalScore;
            _xpDoubled = false;

            // Process progression
            var progressionManager = ServiceLocator.Get<ProgressionManager>();
            var achievementManager = ServiceLocator.Get<AchievementManager>();

            if (progressionManager != null)
            {
                _progressionResult = progressionManager.ProcessRunResult(finalScore);
            }

            // Check achievements
            List<string> newAchievements = null;
            if (achievementManager != null)
            {
                newAchievements = achievementManager.CheckAllAchievements();
            }

            // Submit score to leaderboard
            var leaderboardService = ServiceLocator.Get<ILeaderboardService>();
            if (leaderboardService != null)
            {
                _ = leaderboardService.SubmitScoreAsync(finalScore);
            }

            // Populate UI
            PopulateScoreSection(finalScore, bestScore, isNewBest);
            PopulateStatsSection(linesCleared, bestCombo);
            PopulateXPSection();
            PopulateAchievementSection(newAchievements);
            SetupDoubleXPButton();

            // Start animations
            StartCoroutine(AnimateResults());
            StartCoroutine(NeonGalaxy.VFX.UIAnimator.BounceIn(transform, 0.4f));
        }

        /// <summary>
        /// Hides the results screen.
        /// </summary>
        public void Hide()
        {
            if (!gameObject.activeSelf) return;
            StartCoroutine(HideRoutine());
        }

        private IEnumerator HideRoutine()
        {
            yield return StartCoroutine(NeonGalaxy.VFX.UIAnimator.ScaleOut(transform, 0.2f));
            gameObject.SetActive(false);
        }

        // ── UI Population ────────────────────────────────────────

        private void PopulateScoreSection(int finalScore, int bestScore, bool isNewBest)
        {
            if (bestScoreText != null)
                bestScoreText.text = $"BEST: {bestScore:N0}";

            if (newBestBadge != null)
                newBestBadge.SetActive(isNewBest);
        }

        private void PopulateStatsSection(int linesCleared, int bestCombo)
        {
            if (linesText != null)
                linesText.text = linesCleared.ToString();

            if (comboText != null)
                comboText.text = $"{bestCombo}x";
        }

        private void PopulateXPSection()
        {
            if (_progressionResult == null) return;

            if (xpEarnedText != null)
                xpEarnedText.text = $"+{_progressionResult.XPEarned} XP";

            if (levelText != null)
                levelText.text = $"LV {_progressionResult.NewLevel}";

            if (levelUpBanner != null)
                levelUpBanner.SetActive(_progressionResult.DidLevelUp);
        }

        private void PopulateAchievementSection(List<string> newAchievements)
        {
            if (achievementListParent == null) return;

            // Clear previous entries
            foreach (Transform child in achievementListParent)
            {
                Destroy(child.gameObject);
            }

            if (newAchievements == null || newAchievements.Count == 0) return;

            var achievementManager = ServiceLocator.Get<AchievementManager>();
            if (achievementManager == null) return;

            foreach (var id in newAchievements)
            {
                var def = achievementManager.GetDefinition(id);
                if (def == null) continue;

                if (achievementEntryPrefabText != null)
                {
                    var entry = Instantiate(achievementEntryPrefabText, achievementListParent);
                    entry.text = $"🏆 {def.displayName}";
                    entry.gameObject.SetActive(true);
                }
            }
        }

        private void SetupDoubleXPButton()
        {
            if (doubleXPButton == null) return;

            var adService = ServiceLocator.Get<IAdService>();
            bool adReady = adService != null && adService.IsRewardedAdReady;

            doubleXPButton.gameObject.SetActive(adReady);
            doubleXPButton.interactable = true;

            if (doubleXPButtonText != null)
                doubleXPButtonText.text = "📺 Watch Ad — 2X XP";
        }

        // ── Animations ───────────────────────────────────────────

        private IEnumerator AnimateResults()
        {
            // Animate score count-up
            if (finalScoreText != null)
            {
                float elapsed = 0f;
                while (elapsed < countUpDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / countUpDuration;
                    float ease = 1f - Mathf.Pow(1f - t, 3f);
                    int val = Mathf.RoundToInt(Mathf.Lerp(0, _finalScore, ease));
                    finalScoreText.text = val.ToString("N0");
                    yield return null;
                }
                finalScoreText.text = _finalScore.ToString("N0");
            }

            // Animate XP bar
            if (xpBarFill != null && _progressionResult != null)
            {
                xpBarFill.type = Image.Type.Filled;
                xpBarFill.fillMethod = Image.FillMethod.Horizontal;
                xpBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;

                float startFill = _progressionResult.DidLevelUp ? 0f :
                    Mathf.Clamp01((_progressionResult.XPProgressNormalized * _progressionResult.XPNeededForNextLevel - _progressionResult.XPEarned)
                    / Mathf.Max(1, _progressionResult.XPNeededForNextLevel));
                float endFill = _progressionResult.XPProgressNormalized;

                if (startFill < 0f) startFill = 0f;

                float elapsed = 0f;
                while (elapsed < xpBarAnimDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / xpBarAnimDuration);
                    float ease = 1f - Mathf.Pow(1f - t, 2f);
                    xpBarFill.fillAmount = Mathf.Lerp(startFill, endFill, ease);
                    yield return null;
                }
                xpBarFill.fillAmount = endFill;
            }
        }

        // ── Button Handlers ──────────────────────────────────────

        private void OnDoubleXPClicked()
        {
            if (_xpDoubled) return;

            var adService = ServiceLocator.Get<IAdService>();
            if (adService == null) return;

            doubleXPButton.interactable = false;

            adService.ShowRewardedAd((success) =>
            {
                if (success && _progressionResult != null)
                {
                    _xpDoubled = true;

                    // Grant bonus XP equal to what was earned
                    var saveService = ServiceLocator.Get<SaveService>();

                    if (saveService != null)
                    {
                        saveService.Data.totalXP += _progressionResult.XPEarned;
                        saveService.MarkDirty();
                        saveService.Save();
                    }

                    // Update UI
                    if (xpEarnedText != null)
                        xpEarnedText.text = $"+{_progressionResult.XPEarned * 2} XP (2X!)";

                    if (doubleXPButtonText != null)
                        doubleXPButtonText.text = "✅ XP Doubled!";

                    // Notify ad policy
                    var adPolicyManager = ServiceLocator.Get<AdPolicyManager>();
                    adPolicyManager?.OnRewardedAdWatched();

                    Debug.Log($"[ResultsScreen] XP doubled via rewarded ad: +{_progressionResult.XPEarned}");
                }
                else
                {
                    doubleXPButton.interactable = true;
                }
            });
        }
    }
}
