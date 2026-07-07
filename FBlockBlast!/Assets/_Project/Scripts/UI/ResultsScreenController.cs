using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NeonGalaxy.UI
{
    /// <summary>
    /// "Continue Your Run?" popup shown when the player runs out of valid placements.
    /// Displays a countdown timer with radial fill, score/best score info,
    /// and three options: Watch Ad, Spend Gems, or No Thanks.
    /// When the countdown expires or the player declines, the game proceeds to Game Over.
    /// </summary>
    public class ResultsScreenController : MonoBehaviour
    {
        [Header("Score Display")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI bestScoreText;

        [Header("Countdown")]
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private TextMeshProUGUI countdownInfoText;
        [SerializeField] private Image countdownFillImage;
        [SerializeField] private float countdownDuration = 5f;

        [Header("Buttons")]
        [SerializeField] private Button watchAdButton;
        [SerializeField] private Button spendGemsButton;
        [SerializeField] private Button noThanksButton;

        [Header("Gem Cost")]
        [SerializeField] private TextMeshProUGUI gemCostText;
        [SerializeField] private int gemCost = 50;

        // ── Events ──────────────────────────────────────────────
        /// <summary>Fired when the player chooses to watch an ad to continue.</summary>
        public event Action OnContinueWithAd;

        /// <summary>Fired when the player chooses to spend gems to continue.</summary>
        public event Action OnContinueWithGems;

        /// <summary>Fired when the player declines or the countdown expires.</summary>
        public event Action OnDeclined;

        private Coroutine _countdownCoroutine;
        private bool _choiceMade;

        private void Awake()
        {
            if (watchAdButton != null)
                watchAdButton.onClick.AddListener(OnWatchAdClicked);

            if (spendGemsButton != null)
                spendGemsButton.onClick.AddListener(OnSpendGemsClicked);

            if (noThanksButton != null)
                noThanksButton.onClick.AddListener(OnNoThanksClicked);
        }

        // ── Public API ──────────────────────────────────────────

        /// <summary>
        /// Shows the continue popup with score info and starts the countdown timer.
        /// </summary>
        public void Show(int finalScore, int bestScore)
        {
            gameObject.SetActive(true);
            transform.localScale = Vector3.one;
            _choiceMade = false;

            // Populate score texts
            if (scoreText != null)
                scoreText.text = finalScore.ToString("N0");

            if (bestScoreText != null)
                bestScoreText.text = bestScore.ToString("N0");

            // Populate gem cost text
            if (gemCostText != null)
                gemCostText.text = gemCost.ToString();

            // Initialize countdown visuals
            int fullSeconds = Mathf.CeilToInt(countdownDuration);
            if (countdownText != null)
                countdownText.text = fullSeconds.ToString();

            if (countdownInfoText != null)
                countdownInfoText.text = $"Auto game over in {fullSeconds}s";

            if (countdownFillImage != null)
            {
                countdownFillImage.type = Image.Type.Filled;
                countdownFillImage.fillMethod = Image.FillMethod.Radial360;
                countdownFillImage.fillOrigin = (int)Image.Origin360.Top;
                countdownFillImage.fillClockwise = true;
                countdownFillImage.fillAmount = 1f;
            }

            // Enable buttons
            SetButtonsInteractable(true);

            // Start countdown
            if (_countdownCoroutine != null)
                StopCoroutine(_countdownCoroutine);
            _countdownCoroutine = StartCoroutine(CountdownRoutine());

            // Bounce in animation
            StartCoroutine(NeonGalaxy.VFX.UIAnimator.BounceIn(transform, 0.4f));
        }

        /// <summary>
        /// Hides the continue popup with scale-out animation.
        /// </summary>
        public void Hide()
        {
            if (_countdownCoroutine != null)
            {
                StopCoroutine(_countdownCoroutine);
                _countdownCoroutine = null;
            }

            if (!gameObject.activeSelf) return;
            StartCoroutine(HideRoutine());
        }

        /// <summary>
        /// Force-hides without animation (used when immediately transitioning).
        /// </summary>
        public void HideImmediate()
        {
            if (_countdownCoroutine != null)
            {
                StopCoroutine(_countdownCoroutine);
                _countdownCoroutine = null;
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Returns the configured gem cost for continue.
        /// Used by GameManager to know how many gems to spend.
        /// </summary>
        public int GetGemCost() => gemCost;

        // ── Countdown ───────────────────────────────────────────

        private IEnumerator CountdownRoutine()
        {
            float remaining = countdownDuration;

            while (remaining > 0f)
            {
                remaining -= Time.deltaTime;
                if (remaining < 0f) remaining = 0f;

                float normalizedTime = remaining / countdownDuration;
                int displaySeconds = Mathf.CeilToInt(remaining);

                // Update countdown number text
                if (countdownText != null)
                    countdownText.text = displaySeconds.ToString();

                // Update info text
                if (countdownInfoText != null)
                    countdownInfoText.text = $"Auto game over in {displaySeconds}s";

                // Update radial fill (1 → 0 as time passes)
                if (countdownFillImage != null)
                    countdownFillImage.fillAmount = normalizedTime;

                yield return null;
            }

            // Countdown expired — treat as decline
            _countdownCoroutine = null;

            if (!_choiceMade)
            {
                _choiceMade = true;
                SetButtonsInteractable(false);
                Debug.Log("[ResultsScreen] Countdown expired. Proceeding to Game Over.");
                OnDeclined?.Invoke();
            }
        }

        // ── Button Handlers ─────────────────────────────────────

        private void OnWatchAdClicked()
        {
            if (_choiceMade) return;
            _choiceMade = true;

            StopCountdown();
            SetButtonsInteractable(false);

            Debug.Log("[ResultsScreen] Player chose: Watch Ad");
            OnContinueWithAd?.Invoke();
        }

        private void OnSpendGemsClicked()
        {
            if (_choiceMade) return;
            _choiceMade = true;

            StopCountdown();
            SetButtonsInteractable(false);

            Debug.Log($"[ResultsScreen] Player chose: Spend {gemCost} Gems");
            OnContinueWithGems?.Invoke();
        }

        private void OnNoThanksClicked()
        {
            if (_choiceMade) return;
            _choiceMade = true;

            StopCountdown();
            SetButtonsInteractable(false);

            Debug.Log("[ResultsScreen] Player chose: No Thanks");
            OnDeclined?.Invoke();
        }

        // ── Helpers ─────────────────────────────────────────────

        private void StopCountdown()
        {
            if (_countdownCoroutine != null)
            {
                StopCoroutine(_countdownCoroutine);
                _countdownCoroutine = null;
            }
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (watchAdButton != null) watchAdButton.interactable = interactable;
            if (spendGemsButton != null) spendGemsButton.interactable = interactable;
            if (noThanksButton != null) noThanksButton.interactable = interactable;
        }

        private IEnumerator HideRoutine()
        {
            yield return StartCoroutine(NeonGalaxy.VFX.UIAnimator.ScaleOut(transform, 0.2f));
            gameObject.SetActive(false);
        }
    }
}
