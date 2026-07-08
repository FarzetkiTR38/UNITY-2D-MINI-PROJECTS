using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NeonGalaxy.UI
{
    /// <summary>
    /// Manages the game-over dialog popup.
    /// Animates score values and handles retry/home button signals.
    /// </summary>
    public class GameOverPopupController : MonoBehaviour
    {
        [Header("UI Fields")]
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI bestScoreText;
        [SerializeField] private TextMeshProUGUI xpEarnedText;
        [SerializeField] private TextMeshProUGUI goldEarnedText;
        [SerializeField] private GameObject newBestBadge;

        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button homeButton;

        [Header("Animation Settings")]
        [SerializeField] private float countUpDuration = 1.0f;

        public event Action OnRetryClicked;
        public event Action OnHomeClicked;

        private void Awake()
        {
            if (retryButton != null) retryButton.onClick.AddListener(() => OnRetryClicked?.Invoke());
            if (homeButton != null) homeButton.onClick.AddListener(() => OnHomeClicked?.Invoke());
        }

        /// <summary>
        /// Activates and populates the popup data.
        /// Starts a visual count-up score animation.
        /// </summary>
        public void Show(int finalScore, int bestScore, bool isNewBest, int xpEarned, int goldEarned)
        {
            gameObject.SetActive(true);
            transform.localScale = Vector3.one; // FIX: Prevent invisible popup on second open

            if (newBestBadge != null)
            {
                newBestBadge.SetActive(isNewBest);
            }

            if (bestScoreText != null)
            {
                bestScoreText.text = bestScore.ToString("N0");
            }

            if (xpEarnedText != null)
            {
                xpEarnedText.text = xpEarned.ToString("N0");
            }

            if (goldEarnedText != null)
            {
                goldEarnedText.text = goldEarned.ToString("N0");
            }

            if (finalScoreText != null)
            {
                StartCoroutine(CountUpScoreRoutine(finalScore));
            }

            StartCoroutine(NeonGalaxy.VFX.UIAnimator.BounceIn(transform, 0.4f));
        }

        /// <summary>
        /// Deactivates the popup.
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

        private IEnumerator CountUpScoreRoutine(int targetScore)
        {
            float elapsed = 0f;
            while (elapsed < countUpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / countUpDuration;
                // Cubic ease-out count up
                float ease = 1f - Mathf.Pow(1f - t, 3);
                int currentVal = Mathf.RoundToInt(Mathf.Lerp(0, targetScore, ease));
                finalScoreText.text = currentVal.ToString("N0");
                yield return null;
            }
            finalScoreText.text = targetScore.ToString("N0");
        }
    }
}
