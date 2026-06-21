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
        public void Show(int finalScore, int bestScore, bool isNewBest)
        {
            gameObject.SetActive(true);

            if (newBestBadge != null)
            {
                newBestBadge.SetActive(isNewBest);
            }

            if (bestScoreText != null)
            {
                bestScoreText.text = $"BEST SCORE: {bestScore}";
            }

            if (finalScoreText != null)
            {
                StartCoroutine(CountUpScoreRoutine(finalScore));
            }
        }

        /// <summary>
        /// Deactivates the popup.
        /// </summary>
        public void Hide()
        {
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
                finalScoreText.text = currentVal.ToString();
                yield return null;
            }
            finalScoreText.text = targetScore.ToString();
        }
    }
}
