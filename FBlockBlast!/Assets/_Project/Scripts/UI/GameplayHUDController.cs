using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonGalaxy.Core;
using NeonGalaxy.Data;

namespace NeonGalaxy.UI
{
    /// <summary>
    /// Updates the main gameplay HUD, including current score, highest score, 
    /// combo counters, top bar (back/pause/currency), and text punch animations.
    /// </summary>
    public class GameplayHUDController : MonoBehaviour
    {
        [Header("Text Fields")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI bestScoreText;
        [SerializeField] private TextMeshProUGUI comboText;

        [Header("Containers")]
        [SerializeField] private GameObject comboContainer;
        [SerializeField] private GameObject newBestBadge;

        [Header("Top Bar")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button addCurrencyButton;
        [SerializeField] private TextMeshProUGUI currencyText;

        [Header("Text Animations")]
        [SerializeField] private float punchScale = 1.2f;
        [SerializeField] private float punchDuration = 0.15f;

        private Coroutine _scorePunchRoutine;
        private Coroutine _comboPunchRoutine;
        private Vector3 _originalScoreScale;
        private Vector3 _originalComboScale;

        private void Awake()
        {
            if (scoreText != null) _originalScoreScale = scoreText.transform.localScale;
            if (comboText != null) _originalComboScale = comboText.transform.localScale;

            if (newBestBadge != null) newBestBadge.SetActive(false);
            if (comboContainer != null) comboContainer.SetActive(false);

            // Wire up top bar buttons
            if (backButton != null) backButton.onClick.AddListener(HandleBackPressed);
            if (pauseButton != null) pauseButton.onClick.AddListener(HandlePausePressed);
            if (addCurrencyButton != null) addCurrencyButton.onClick.AddListener(HandleAddCurrencyPressed);
        }

        private void OnEnable()
        {
            GameEvents.OnScoreChanged += HandleScoreChanged;
            GameEvents.OnComboUpdated += HandleComboUpdated;
            GameEvents.OnNewBestScore += HandleNewBestScore;
            GameEvents.OnCoinBalanceChanged += HandleCoinBalanceChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnScoreChanged -= HandleScoreChanged;
            GameEvents.OnComboUpdated -= HandleComboUpdated;
            GameEvents.OnNewBestScore -= HandleNewBestScore;
            GameEvents.OnCoinBalanceChanged -= HandleCoinBalanceChanged;
        }

        /// <summary>
        /// Updates the high score text explicitly.
        /// </summary>
        public void UpdateBestScore(int bestScore)
        {
            if (bestScoreText != null)
            {
                bestScoreText.text = $"BEST: {bestScore}";
            }
        }

        private void HandleScoreChanged(int currentScore)
        {
            if (scoreText != null)
            {
                scoreText.text = currentScore.ToString();
                
                // Trigger score punch animation
                if (_scorePunchRoutine != null) StopCoroutine(_scorePunchRoutine);
                _scorePunchRoutine = StartCoroutine(PunchTextRoutine(scoreText.transform, _originalScoreScale));
            }
        }

        private void HandleComboUpdated(int currentCombo)
        {
            if (comboContainer != null)
            {
                // Show combo container only if combo is greater than zero
                bool hasCombo = currentCombo > 0;
                comboContainer.SetActive(hasCombo);
            }

            if (comboText != null && currentCombo > 0)
            {
                comboText.text = $"{currentCombo}x COMBO";

                // Trigger combo punch animation
                if (_comboPunchRoutine != null) StopCoroutine(_comboPunchRoutine);
                _comboPunchRoutine = StartCoroutine(PunchTextRoutine(comboText.transform, _originalComboScale));
            }
        }

        private void HandleNewBestScore(int newBestScore)
        {
            if (newBestBadge != null)
            {
                newBestBadge.SetActive(true);
            }
            UpdateBestScore(newBestScore);
        }

        private void HandleCoinBalanceChanged(int newBalance)
        {
            UpdateCurrency(newBalance);
        }

        /// <summary>
        /// Updates the currency display text.
        /// </summary>
        public void UpdateCurrency(int amount)
        {
            if (currencyText != null)
            {
                currencyText.text = amount.ToString("N0");
            }
        }

        private void HandleBackPressed()
        {
            // Navigate back to Home scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("Home");
        }

        private void HandlePausePressed()
        {
            GameEvents.InvokeGameStateChanged(GameState.Paused);
        }

        private void HandleAddCurrencyPressed()
        {
            // Placeholder: open shop or trigger rewarded ad
            Debug.Log("[HUD] Add currency pressed — hook up to shop or rewarded ad.");
        }

        private IEnumerator PunchTextRoutine(Transform targetTransform, Vector3 originalScale)
        {
            float elapsed = 0f;
            Vector3 peakScale = originalScale * punchScale;

            while (elapsed < punchDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / punchDuration;
                // Simple sine wave ease-in-out punch
                float scaleOffset = Mathf.Sin(t * Mathf.PI);
                targetTransform.localScale = Vector3.Lerp(originalScale, peakScale, scaleOffset);
                yield return null;
            }

            targetTransform.localScale = originalScale;
        }
    }
}
