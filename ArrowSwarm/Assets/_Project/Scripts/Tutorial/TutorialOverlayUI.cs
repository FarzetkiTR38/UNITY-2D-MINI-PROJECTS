namespace ArrowSwarm.Tutorial
{
    using System.Collections;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Manages the UI overlay for the interactive tutorial:
    /// Floating instruction banner, skip button, and tutorial completion card.
    /// </summary>
    public partial class TutorialOverlayUI : MonoBehaviour
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
        [SerializeField] private CanvasGroup _completionBackdrop;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _mainMenuButton;

        private Coroutine _bannerFadeRoutine;
        private Vector3 _originalCardScale = Vector3.one;
        private Vector3 _originalBannerScale = Vector3.one;
        private bool _scalesCached = false;

        private void Awake()
        {
            CacheOriginalScales();
            SetCompletionBackdrop(false);

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
            if (_mainMenuButton != null) _mainMenuButton.onClick.AddListener(OnMainMenuClicked);

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
            if (_mainMenuButton != null) _mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
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
            SetCompletionBackdrop(false);
            var overlayGroup = GetComponent<CanvasGroup>();
            if (overlayGroup != null) overlayGroup.blocksRaycasts = false;
            if (_skipButton != null) _skipButton.gameObject.SetActive(true);
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
        /// Shows the localized completion card and its navigation choices.
        /// </summary>
        public void ShowCompletionCard()
        {
            SetCompletionBackdrop(true);
            var overlayCG = GetComponent<CanvasGroup>();
            if (overlayCG != null)
            {
                overlayCG.blocksRaycasts = true;
                overlayCG.interactable = true;
            }

            if (_bannerGroup != null) _bannerGroup.alpha = 0f;
            if (_skipButton != null) _skipButton.gameObject.SetActive(false);

            if (_completeCard != null)
            {
                _completeCard.SetActive(true);
                if (_completeGroup != null)
                {
                    _completeGroup.interactable = false;
                    _completeGroup.blocksRaycasts = true;
                }

                if (gameObject.activeInHierarchy)
                {
                    StartCoroutine(PopCompletionRoutine());
                }
            }
        }

    }
}
