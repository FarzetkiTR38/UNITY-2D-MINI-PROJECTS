namespace ArrowSwarm.UI
{
    using System.Collections;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Win screen overlay displayed when the player completes a level.
    /// Features a 3-star rating display, Next Level, Main Menu, and Levels navigation buttons.
    /// </summary>
    public class LevelCompleteUI : MonoBehaviour
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
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _levelsButton;

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
            GameManager.OnLevelWon += Show;
            SubscribeButtons();
        }

        private void OnDisable()
        {
            GameManager.OnLevelWon -= Show;
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
        /// Automatically discovers and assigns missing UI references from child hierarchy.
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
                       ?? transform.Find("LevelClearedText");
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

            if (_nextLevelButton == null)
            {
                var btn = transform.Find("DialogBox/ButtonsContainer/NextLevelBtn")
                       ?? transform.Find("DialogBox/NextBtn")
                       ?? transform.Find("NextBtn");
                if (btn != null) _nextLevelButton = btn.GetComponent<Button>();
            }

            if (_mainMenuButton == null)
            {
                var btn = transform.Find("DialogBox/ButtonsContainer/MainMenuBtn")
                       ?? transform.Find("DialogBox/MenuBtn")
                       ?? transform.Find("MenuBtn");
                if (btn != null) _mainMenuButton = btn.GetComponent<Button>();
            }

            if (_levelsButton == null)
            {
                var btn = transform.Find("DialogBox/ButtonsContainer/LevelsBtn")
                       ?? transform.Find("DialogBox/LevelsBtn");
                if (btn != null) _levelsButton = btn.GetComponent<Button>();
            }

            if (_bottomBadgeImage == null)
            {
                var badge = transform.Find("DialogBox/BottomBadge");
                if (badge != null) _bottomBadgeImage = badge.GetComponent<Image>();
            }
        }

        private void SubscribeButtons()
        {
            if (_nextLevelButton != null) _nextLevelButton.onClick.AddListener(OnNextLevel);
            if (_mainMenuButton != null) _mainMenuButton.onClick.AddListener(OnMainMenu);
            if (_levelsButton != null) _levelsButton.onClick.AddListener(OnLevels);
        }

        private void UnsubscribeButtons()
        {
            if (_nextLevelButton != null) _nextLevelButton.onClick.RemoveListener(OnNextLevel);
            if (_mainMenuButton != null) _mainMenuButton.onClick.RemoveListener(OnMainMenu);
            if (_levelsButton != null) _levelsButton.onClick.RemoveListener(OnLevels);
        }

        /// <summary>
        /// Displays the level complete screen, awards stars, and persists progress.
        /// </summary>
        public void Show()
        {
            if (_isShowing) return;
            if (ArrowSwarm.Tutorial.TutorialManager.Instance != null && ArrowSwarm.Tutorial.TutorialManager.Instance.IsTutorialActive)
            {
                return;
            }

            _isShowing = true;

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            if (_canvasGroup == null)
            {
                AutoWire();
            }

            int level = LevelManager.Instance != null && LevelManager.Instance.CurrentParams.Level > 0
                ? LevelManager.Instance.CurrentParams.Level
                : (DataManager.Instance?.PlayerData?.currentLevel ?? 1);

            if (_titleText != null && string.IsNullOrEmpty(_titleText.text))
            {
                _titleText.text = "LEVEL\nCOMPLETED";
            }

            if (_levelText != null)
            {
                _levelText.text = $"Level {level} Cleared";
            }

            int currentLives = GameManager.Instance != null ? GameManager.Instance.CurrentLives : 3;
            int starsEarned = Mathf.Clamp(currentLives, 0, 3);

            DataManager.Instance?.SetLevelStars(level, starsEarned);
            DataManager.Instance?.UnlockNextLevel(level);

            for (int i = 0; i < _stars.Length; i++)
            {
                if (_stars[i] != null)
                {
                    _stars[i].SetActive(i < starsEarned);
                }
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
        /// Hides the level complete overlay.
        /// </summary>
        /// <param name="instant">If true, snaps alpha immediately to 0.</param>
        public void Hide(bool instant = false)
        {
            _isShowing = false;
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
            }

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

            if (instant)
            {
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 0f;
                    _canvasGroup.blocksRaycasts = false;
                }
                gameObject.SetActive(false);
            }
            else
            {
                _fadeCoroutine = StartCoroutine(FadeTo(0f));
            }
        }

        private void OnNextLevel()
        {
            InputManager.Instance?.BlockInput(0.35f);
            Hide(instant: true);

            int currentLevel = DataManager.Instance?.PlayerData != null ? DataManager.Instance.PlayerData.currentLevel : 1;
            if (ArrowSwarm.Ads.AdManager.Instance != null)
            {
                ArrowSwarm.Ads.AdManager.Instance.ShowInterstitialWithPacing(currentLevel, () =>
                {
                    LevelManager.Instance?.NextLevel();
                });
            }
            else
            {
                LevelManager.Instance?.NextLevel();
            }
        }

        private void OnMainMenu()
        {
            InputManager.Instance?.BlockInput(0.35f);
            Hide(instant: true);
            GameManager.Instance?.GoToMainMenu();
        }

        private void OnLevels()
        {
            InputManager.Instance?.BlockInput(0.35f);
            Hide(instant: true);
            MainMenuUI.OpenLevelsOnLoad = true;
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
                if (_canvasGroup != null) _canvasGroup.blocksRaycasts = false;
                gameObject.SetActive(false);
            }
        }
    }
}
