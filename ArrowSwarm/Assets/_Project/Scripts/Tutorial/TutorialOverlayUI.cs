namespace ArrowSwarm.Tutorial
{
    using System;
    using System.Collections;
    using ArrowSwarm.Core;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Manages the UI overlay for the interactive tutorial:
    /// Floating instruction banner, skip button, and tutorial completion card.
    /// </summary>
    public class TutorialOverlayUI : MonoBehaviour
    {
        [Header("--- Banner UI ---")]
        [SerializeField] private CanvasGroup _bannerGroup;
        [SerializeField] private RectTransform _bannerRect;
        [SerializeField] private TextMeshProUGUI _instructionText;

        [Header("--- Skip Button ---")]
        [SerializeField] private Button _skipButton;

        [Header("--- Completion Popup ---")]
        [SerializeField] private GameObject _completeCard;
        [SerializeField] private CanvasGroup _completeGroup;
        [SerializeField] private Button _continueButton;
        [SerializeField] private TextMeshProUGUI _continueButtonText;
        [SerializeField] private TextMeshProUGUI _completeTitleText;
        [SerializeField] private TextMeshProUGUI _completeSubtitleText;

        private Coroutine _bannerFadeRoutine;
        private Vector3 _originalCardScale = Vector3.one;
        private Vector3 _originalBannerScale = Vector3.one;
        private bool _scalesCached = false;

        private void Awake()
        {
            CacheOriginalScales();

            var overlayCG = GetComponent<CanvasGroup>();
            if (overlayCG != null)
            {
                overlayCG.blocksRaycasts = false;
                overlayCG.interactable = true;
            }

            if (_skipButton != null)
            {
                _skipButton.onClick.AddListener(OnSkipClicked);
            }

            if (_continueButton != null)
            {
                _continueButton.onClick.AddListener(OnContinueClicked);
            }

            if (_completeCard != null)
            {
                _completeCard.SetActive(false);
            }
        }

        private void CacheOriginalScales()
        {
            if (_scalesCached) return;

            if (_completeCard != null)
            {
                _originalCardScale = _completeCard.transform.localScale;
            }

            if (_bannerRect != null)
            {
                _originalBannerScale = _bannerRect.localScale;
            }

            _scalesCached = true;
        }

        private void OnDestroy()
        {
            if (_skipButton != null) _skipButton.onClick.RemoveListener(OnSkipClicked);
            if (_continueButton != null) _continueButton.onClick.RemoveListener(OnContinueClicked);
        }

        /// <summary>
        /// Displays the instruction message in the tutorial banner with a soft punch animation.
        /// </summary>
        public void SetInstruction(string text)
        {
            if (_instructionText != null)
            {
                _instructionText.text = text;
            }

            if (gameObject.activeInHierarchy)
            {
                if (_bannerFadeRoutine != null) StopCoroutine(_bannerFadeRoutine);
                _bannerFadeRoutine = StartCoroutine(PunchBannerRoutine());
            }
        }

        /// <summary>
        /// Shows the tutorial overlay.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            if (_bannerGroup != null) _bannerGroup.alpha = 1f;
            if (_completeCard != null) _completeCard.SetActive(false);
        }

        /// <summary>
        /// Hides the entire tutorial overlay.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Shows the completion celebration card with victory text and localized button.
        /// </summary>
        public void ShowCompletionCard(string title, string subtitle, string buttonText = "MAIN MENU")
        {
            var overlayCG = GetComponent<CanvasGroup>();
            if (overlayCG != null)
            {
                overlayCG.blocksRaycasts = true;
                overlayCG.interactable = true;
            }

            if (_bannerGroup != null) _bannerGroup.alpha = 0f;
            if (_skipButton != null) _skipButton.gameObject.SetActive(false);

            if (_completeTitleText != null) _completeTitleText.text = title;
            if (_completeSubtitleText != null) _completeSubtitleText.text = subtitle;

            if (_completeCard != null)
            {
                _completeCard.SetActive(true);

                if (_continueButton == null)
                {
                    _continueButton = _completeCard.GetComponentInChildren<Button>(true);
                }

                if (_continueButtonText == null && _continueButton != null)
                {
                    _continueButtonText = _continueButton.GetComponentInChildren<TextMeshProUGUI>(true);
                }

                if (_continueButtonText != null && !string.IsNullOrEmpty(buttonText))
                {
                    _continueButtonText.text = buttonText;
                }

                if (_continueButton != null)
                {
                    _continueButton.onClick.RemoveAllListeners();
                    _continueButton.onClick.AddListener(OnContinueClicked);
                    _continueButton.interactable = true;
                }

                if (_completeGroup != null)
                {
                    _completeGroup.interactable = true;
                    _completeGroup.blocksRaycasts = true;
                }

                if (gameObject.activeInHierarchy)
                {
                    StartCoroutine(PopCompletionRoutine());
                }
            }
        }

        private IEnumerator PunchBannerRoutine()
        {
            if (_bannerRect == null) yield break;
            CacheOriginalScales();

            Vector3 baseScale = _originalBannerScale;
            Vector3 punchScale = baseScale * 1.08f;
            Vector3 startScale = baseScale * 0.92f;
            float elapsed = 0f;
            float duration = 0.28f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / duration;
                if (progress < 0.45f)
                {
                    float t = Mathf.SmoothStep(0f, 1f, progress / 0.45f);
                    _bannerRect.localScale = Vector3.Lerp(startScale, punchScale, t);
                }
                else
                {
                    float t = Mathf.SmoothStep(0f, 1f, (progress - 0.45f) / 0.55f);
                    _bannerRect.localScale = Vector3.Lerp(punchScale, baseScale, t);
                }
                yield return null;
            }

            _bannerRect.localScale = baseScale;
            _bannerFadeRoutine = null;
        }

        private IEnumerator PopCompletionRoutine()
        {
            CacheOriginalScales();

            if (_completeGroup != null) _completeGroup.alpha = 0f;
            RectTransform cardRect = _completeCard != null ? _completeCard.transform as RectTransform : null;
            
            Vector3 targetScale = _originalCardScale;
            Vector3 startScale = targetScale * 0.7f;

            if (cardRect != null) cardRect.localScale = startScale;

            float elapsed = 0f;
            float duration = 0.35f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                if (_completeGroup != null) _completeGroup.alpha = t;
                if (cardRect != null) cardRect.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            if (_completeGroup != null)
            {
                _completeGroup.alpha = 1f;
                _completeGroup.interactable = true;
                _completeGroup.blocksRaycasts = true;
            }
            if (cardRect != null) cardRect.localScale = targetScale;
        }

        private void OnSkipClicked()
        {
            TutorialManager.Instance?.SkipTutorial();
        }

        private void OnContinueClicked()
        {
            Debug.Log("[ArrowSwarm] TutorialOverlayUI: Continue Button Clicked! Returning to MainMenuScene...");
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.CompleteAndReturnToMenu();
            }
            else if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene("MainMenuScene");
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
            }
        }
    }
}
