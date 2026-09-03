namespace ArrowSwarm.UI
{
    using System.Collections;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Game Over screen overlay displayed when the player loses all lives or fails a level.
    /// Features empty star indicators, Retry, and Main Menu navigation buttons.
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("Containers & Animation")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _dialogBox;
        [SerializeField] private float _fadeSpeed = 4f;

        [Header("Header & Title")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _levelText;

        [Header("Stars")]
        [SerializeField] private GameObject[] _stars = new GameObject[3];

        [Header("Action Buttons")]
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _mainMenuButton;

        [Header("Visual Placeholders")]
        [SerializeField] private Image _boardFrameImage;
        [SerializeField] private Image _bottomBadgeImage;

        private Coroutine _fadeCoroutine;
        private bool _isShowing;

        private void Awake()
        {
            AutoWire();
        }

        private void OnEnable()
        {
            GameManager.OnLevelLost += Show;
            SubscribeButtons();
        }

        private void OnDisable()
        {
            GameManager.OnLevelLost -= Show;
            UnsubscribeButtons();
        }

        private void Start()
        {
            if (!_isShowing)
            {
                Hide(instant: true);
            }
        }

        /// <summary>
        /// Automatically discovers and assigns missing UI references from the child hierarchy.
        /// </summary>
        public void AutoWire()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            }

            if (_dialogBox == null)
            {
                var box = transform.Find("DialogBox") ?? transform.Find("BoardFrame");
                if (box != null) _dialogBox = box.GetComponent<RectTransform>();
            }

            if (_boardFrameImage == null && _dialogBox != null)
            {
                _boardFrameImage = _dialogBox.GetComponent<Image>();
            }

            if (_titleText == null)
            {
                var title = transform.Find("DialogBox/HeaderTitle/TitleText")
                         ?? transform.Find("DialogBox/Title")
                         ?? transform.Find("Title");
                if (title != null) _titleText = title.GetComponent<TextMeshProUGUI>();
            }

            if (_levelText == null)
            {
                var lvl = transform.Find("DialogBox/HeaderTitle/LevelText")
                       ?? transform.Find("LevelFailedText");
                if (lvl != null) _levelText = lvl.GetComponent<TextMeshProUGUI>();
            }

            if (_stars == null || _stars.Length < 3 || _stars[0] == null)
            {
                _stars = new GameObject[3];
                var starsContainer = transform.Find("DialogBox/StarsContainer") ?? transform.Find("StarsContainer");
                if (starsContainer != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var star = starsContainer.Find($"Star_{i}");
                        if (star != null) _stars[i] = star.gameObject;
                    }
                }
            }

            if (_retryButton == null)
            {
                var btn = transform.Find("DialogBox/ButtonsContainer/RetryBtn")
                       ?? transform.Find("DialogBox/RestartBtn")
                       ?? transform.Find("RestartBtn");
                if (btn != null) _retryButton = btn.GetComponent<Button>();
            }

            if (_mainMenuButton == null)
            {
                var btn = transform.Find("DialogBox/ButtonsContainer/MainMenuBtn")
                       ?? transform.Find("DialogBox/MenuBtn")
                       ?? transform.Find("MenuBtn");
                if (btn != null) _mainMenuButton = btn.GetComponent<Button>();
            }

            if (_bottomBadgeImage == null)
            {
                var badge = transform.Find("DialogBox/BottomBadge");
                if (badge != null) _bottomBadgeImage = badge.GetComponent<Image>();
            }
        }

        private void SubscribeButtons()
        {
            if (_retryButton != null) _retryButton.onClick.AddListener(OnRetry);
            if (_mainMenuButton != null) _mainMenuButton.onClick.AddListener(OnMainMenu);
        }

        private void UnsubscribeButtons()
        {
            if (_retryButton != null) _retryButton.onClick.RemoveListener(OnRetry);
            if (_mainMenuButton != null) _mainMenuButton.onClick.RemoveListener(OnMainMenu);
        }

        /// <summary>
        /// Displays the game over screen.
        /// </summary>
        public void Show()
        {
            _isShowing = true;

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            if (_canvasGroup == null)
            {
                AutoWire();
            }

            int level = DataManager.Instance?.PlayerData?.currentLevel ?? 1;

            if (_titleText != null && string.IsNullOrEmpty(_titleText.text))
            {
                _titleText.text = "GAME\nOVER";
            }

            if (_levelText != null)
            {
                _levelText.text = $"Level {level} Failed";
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
                if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = StartCoroutine(FadeTo(1f));
            }
        }

        /// <summary>
        /// Hides the game over overlay.
        /// </summary>
        /// <param name="instant">If true, snaps alpha immediately to 0.</param>
        public void Hide(bool instant = false)
        {
            _isShowing = false;
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

            if (instant)
            {
                if (_canvasGroup != null) _canvasGroup.alpha = 0f;
                gameObject.SetActive(false);
            }
            else
            {
                _fadeCoroutine = StartCoroutine(FadeTo(0f));
            }
        }

        private void OnRetry()
        {
            InputManager.Instance?.BlockInput(0.35f);
            Hide(instant: true);
            LevelManager.Instance?.RetryLevel();
        }

        private void OnMainMenu()
        {
            InputManager.Instance?.BlockInput(0.35f);
            Hide(instant: true);
            GameManager.Instance?.GoToMainMenu();
        }

        private IEnumerator FadeTo(float target)
        {
            while (_canvasGroup != null && Mathf.Abs(_canvasGroup.alpha - target) > 0.01f)
            {
                _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, target, Time.unscaledDeltaTime * _fadeSpeed);
                yield return null;
            }
            if (_canvasGroup != null) _canvasGroup.alpha = target;
            _fadeCoroutine = null;

            if (target <= 0.01f)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
