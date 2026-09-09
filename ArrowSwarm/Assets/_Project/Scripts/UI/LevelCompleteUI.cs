namespace ArrowSwarm.UI
{
    using System.Collections;
    using ArrowSwarm.Audio;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Effects;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Win screen overlay displayed when the player completes a level.
    /// Features sequential star popping, celebratory confetti, and navigation buttons.
    /// </summary>
    public class LevelCompleteUI : MonoBehaviour
    {
        [Header("Containers & Animation")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _dialogBox;

        [Header("Header & Title")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _levelText;

        [Header("Stars")]
        [SerializeField] private GameObject[] _stars = new GameObject[3];

        [Header("Action Buttons")]
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _levelsButton;

        private Coroutine _fadeCoroutine;
        private Coroutine _starsCoroutine;
        private bool _isShowing;

        private void Awake() => AutoWire();

        private void OnEnable()
        {
            GameManager.OnLevelWon += Show;
            SetButtonsListening(true);
        }

        private void OnDisable()
        {
            GameManager.OnLevelWon -= Show;
            SetButtonsListening(false);
        }

        private void Start()
        {
            if (!_isShowing) Hide(instant: true);
        }

        /// <summary>
        /// Automatically discovers and assigns missing UI references from child hierarchy.
        /// </summary>
        public void AutoWire()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            if (_dialogBox == null) _dialogBox = (transform.Find("DialogBox") ?? transform.Find("BoardFrame")) as RectTransform ?? GetComponentInChildren<RectTransform>();
            if (_titleText == null) _titleText = transform.Find("DialogBox/HeaderTitle/TitleText")?.GetComponent<TextMeshProUGUI>() ?? GetComponentInChildren<TextMeshProUGUI>();
            if (_levelText == null) _levelText = transform.Find("DialogBox/HeaderTitle/LevelText")?.GetComponent<TextMeshProUGUI>();

            if (_stars == null || _stars.Length < 3 || _stars[0] == null)
            {
                _stars = new GameObject[3];
                var c = transform.Find("DialogBox/StarsContainer") ?? transform.Find("StarsContainer");
                for (int i = 0; c != null && i < 3; i++) _stars[i] = c.Find($"Star_{i}")?.gameObject;
            }

            if (_nextLevelButton == null) _nextLevelButton = transform.Find("DialogBox/ButtonsContainer/NextLevelBtn")?.GetComponent<Button>();
            if (_mainMenuButton == null) _mainMenuButton = transform.Find("DialogBox/ButtonsContainer/MainMenuBtn")?.GetComponent<Button>();
            if (_levelsButton == null) _levelsButton = transform.Find("DialogBox/ButtonsContainer/LevelsBtn")?.GetComponent<Button>();
        }

        private void SetButtonsListening(bool listen)
        {
            if (_nextLevelButton != null) { if (listen) _nextLevelButton.onClick.AddListener(OnNextLevel); else _nextLevelButton.onClick.RemoveListener(OnNextLevel); }
            if (_mainMenuButton != null) { if (listen) _mainMenuButton.onClick.AddListener(OnMainMenu); else _mainMenuButton.onClick.RemoveListener(OnMainMenu); }
            if (_levelsButton != null) { if (listen) _levelsButton.onClick.AddListener(OnLevels); else _levelsButton.onClick.RemoveListener(OnLevels); }
        }

        /// <summary>
        /// Displays the level complete screen, awards stars, and launches celebrations.
        /// </summary>
        public void Show()
        {
            if (_isShowing) return;
            if (ArrowSwarm.Tutorial.TutorialManager.Instance != null && ArrowSwarm.Tutorial.TutorialManager.Instance.IsTutorialActive) return;

            _isShowing = true;
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            if (_canvasGroup == null) AutoWire();

            int level = LevelManager.Instance != null && LevelManager.Instance.CurrentParams.Level > 0
                ? LevelManager.Instance.CurrentParams.Level
                : (DataManager.Instance?.PlayerData?.currentLevel ?? 1);

            if (_titleText != null && string.IsNullOrEmpty(_titleText.text)) _titleText.text = "LEVEL\nCOMPLETED";
            if (_levelText != null) _levelText.text = $"Level {level} Cleared";

            int currentLives = GameManager.Instance != null ? GameManager.Instance.CurrentLives : 3;
            int starsEarned = Mathf.Clamp(currentLives, 0, 3);

            DataManager.Instance?.SetLevelStars(level, starsEarned);
            DataManager.Instance?.UnlockNextLevel(level);

            // Trigger celebratory confetti shower
            ParticleManager.Instance?.SpawnConfetti();

            // Animate popup open and sequence stars pop-in
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
                if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = StartCoroutine(UIPopupAnimator.AnimateOpen(_dialogBox, _canvasGroup));
            }

            if (_starsCoroutine != null) StopCoroutine(_starsCoroutine);
            _starsCoroutine = StartCoroutine(AnimateStarsRoutine(starsEarned));
        }

        private IEnumerator AnimateStarsRoutine(int starsEarned)
        {
            for (int i = 0; i < _stars.Length; i++)
            {
                if (_stars[i] != null)
                {
                    _stars[i].transform.localScale = Vector3.zero;
                    _stars[i].SetActive(false);
                }
            }

            yield return new WaitForSecondsRealtime(0.20f);

            for (int i = 0; i < starsEarned && i < _stars.Length; i++)
            {
                if (_stars[i] != null)
                {
                    _stars[i].SetActive(true);
                    AudioManager.Instance?.PlayStarEarn(i);

                    float elapsed = 0f;
                    float duration = 0.22f;
                    while (elapsed < duration)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        float t = Mathf.Clamp01(elapsed / duration);
                        float c1 = 1.70158f;
                        float scale = 1f + (c1 + 1f) * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
                        _stars[i].transform.localScale = Vector3.one * Mathf.Max(0f, scale);
                        yield return null;
                    }
                    _stars[i].transform.localScale = Vector3.one;
                }
                yield return new WaitForSecondsRealtime(0.12f);
            }
            _starsCoroutine = null;
        }

        /// <summary>
        /// Hides the level complete overlay with elastic pop-out.
        /// </summary>
        public void Hide(bool instant = false)
        {
            _isShowing = false;
            if (_canvasGroup != null) _canvasGroup.interactable = false;
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            if (_starsCoroutine != null) StopCoroutine(_starsCoroutine);

            if (instant)
            {
                if (_canvasGroup != null) { _canvasGroup.alpha = 0f; _canvasGroup.blocksRaycasts = false; }
                if (_dialogBox != null) _dialogBox.localScale = Vector3.one;
                gameObject.SetActive(false);
            }
            else
            {
                _fadeCoroutine = StartCoroutine(UIPopupAnimator.AnimateClose(_dialogBox, _canvasGroup, onComplete: () => gameObject.SetActive(false)));
            }
        }

        private void OnNextLevel()
        {
            InputManager.Instance?.BlockInput(0.35f);
            Hide(instant: true);
            int currentLevel = DataManager.Instance?.PlayerData != null ? DataManager.Instance.PlayerData.currentLevel : 1;
            if (ArrowSwarm.Ads.AdManager.Instance != null)
            {
                ArrowSwarm.Ads.AdManager.Instance.ShowInterstitialWithPacing(currentLevel, () => LevelManager.Instance?.NextLevel());
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
    }
}
