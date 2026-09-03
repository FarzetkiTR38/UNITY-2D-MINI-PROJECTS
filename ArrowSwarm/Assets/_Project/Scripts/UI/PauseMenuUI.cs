namespace ArrowSwarm.UI
{
    using System.Collections;
    using ArrowSwarm.Audio;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Pause menu overlay with Sound and Vibration toggles, Continue, Retry, Levels, and Main Menu actions.
    /// Uses CanvasGroup for fade animation and supports automatic wiring of scene references.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("Containers & Animation")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _dialogBox;
        [SerializeField] private float _fadeSpeed = 5f;

        [Header("Header & Title")]
        [SerializeField] private TextMeshProUGUI _titleText;

        [Header("Toggles")]
        [SerializeField] private SettingsToggleUI _soundToggle;
        [SerializeField] private SettingsToggleUI _vibrationToggle;

        [Header("Action Buttons")]
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _levelsButton;
        [SerializeField] private Button _mainMenuButton;

        [Header("Visual Placeholders")]
        [SerializeField] private Image _boardFrameImage;
        [SerializeField] private Image _bottomBadgeImage;
        [SerializeField] private Image _soundIcon;
        [SerializeField] private Image _vibrationIcon;

        private bool _isShowing;
        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            AutoWire();
        }

        private void OnEnable()
        {
            GameManager.OnGameStateChanged += HandleStateChanged;
            SubscribeEvents();
        }

        private void OnDisable()
        {
            GameManager.OnGameStateChanged -= HandleStateChanged;
            UnsubscribeEvents();
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

            if (_soundToggle == null)
            {
                var t = transform.Find("DialogBox/ContentContainer/SettingRow_Sound/SoundToggle")
                     ?? transform.Find("DialogBox/SettingRow_Sound/SoundToggle");
                if (t != null) _soundToggle = t.GetComponent<SettingsToggleUI>();
            }

            if (_vibrationToggle == null)
            {
                var t = transform.Find("DialogBox/ContentContainer/SettingRow_Vibration/VibrationToggle")
                     ?? transform.Find("DialogBox/SettingRow_Vibration/VibrationToggle");
                if (t != null) _vibrationToggle = t.GetComponent<SettingsToggleUI>();
            }

            if (_soundIcon == null)
            {
                var icon = transform.Find("DialogBox/ContentContainer/SettingRow_Sound/SoundIcon");
                if (icon != null) _soundIcon = icon.GetComponent<Image>();
            }

            if (_vibrationIcon == null)
            {
                var icon = transform.Find("DialogBox/ContentContainer/SettingRow_Vibration/VibrationIcon");
                if (icon != null) _vibrationIcon = icon.GetComponent<Image>();
            }

            if (_bottomBadgeImage == null)
            {
                var badge = transform.Find("DialogBox/BottomBadge");
                if (badge != null) _bottomBadgeImage = badge.GetComponent<Image>();
            }

            if (_continueButton == null)
            {
                var btn = transform.Find("DialogBox/ContentContainer/ContinueBtn")
                       ?? transform.Find("DialogBox/ContinueBtn")
                       ?? transform.Find("ResumeBtn");
                if (btn != null) _continueButton = btn.GetComponent<Button>();
            }

            if (_retryButton == null)
            {
                var btn = transform.Find("DialogBox/ContentContainer/RetryBtn")
                       ?? transform.Find("DialogBox/RetryBtn")
                       ?? transform.Find("RestartBtn");
                if (btn != null) _retryButton = btn.GetComponent<Button>();
            }

            if (_levelsButton == null)
            {
                var btn = transform.Find("DialogBox/ContentContainer/LevelsBtn")
                       ?? transform.Find("DialogBox/LevelsBtn");
                if (btn != null) _levelsButton = btn.GetComponent<Button>();
            }

            if (_mainMenuButton == null)
            {
                var btn = transform.Find("DialogBox/ContentContainer/MainMenuBtn")
                       ?? transform.Find("DialogBox/MainMenuBtn")
                       ?? transform.Find("MenuBtn");
                if (btn != null) _mainMenuButton = btn.GetComponent<Button>();
            }
        }

        private void SubscribeEvents()
        {
            if (_continueButton != null) _continueButton.onClick.AddListener(OnContinueClicked);
            if (_retryButton != null) _retryButton.onClick.AddListener(OnRetryClicked);
            if (_levelsButton != null) _levelsButton.onClick.AddListener(OnLevelsClicked);
            if (_mainMenuButton != null) _mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            if (_soundToggle != null) _soundToggle.OnValueChanged += OnSoundToggleChanged;
            if (_vibrationToggle != null) _vibrationToggle.OnValueChanged += OnVibrationToggleChanged;
        }

        private void UnsubscribeEvents()
        {
            if (_continueButton != null) _continueButton.onClick.RemoveListener(OnContinueClicked);
            if (_retryButton != null) _retryButton.onClick.RemoveListener(OnRetryClicked);
            if (_levelsButton != null) _levelsButton.onClick.RemoveListener(OnLevelsClicked);
            if (_mainMenuButton != null) _mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);

            if (_soundToggle != null) _soundToggle.OnValueChanged -= OnSoundToggleChanged;
            if (_vibrationToggle != null) _vibrationToggle.OnValueChanged -= OnVibrationToggleChanged;
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.Paused)
            {
                Show();
            }
            else if (_isShowing)
            {
                Hide();
            }
        }

        /// <summary>
        /// Displays the pause menu overlay and syncs toggles with saved player data.
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

            RefreshToggles();

            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
                if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = StartCoroutine(FadeTo(1f));
            }
        }

        /// <summary>
        /// Hides the pause menu overlay.
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

        private void RefreshToggles()
        {
            var data = DataManager.Instance?.PlayerData;
            if (data == null) return;

            if (_soundToggle != null)
            {
                _soundToggle.SetIsOn(data.sfxEnabled, false);
            }

            if (_vibrationToggle != null)
            {
                _vibrationToggle.SetIsOn(data.vibrationEnabled, false);
            }
        }

        private void OnSoundToggleChanged(bool isOn)
        {
            DataManager.Instance?.SetSFXEnabled(isOn);
            if (AudioManager.Instance != null && DataManager.Instance?.PlayerData != null)
            {
                DataManager.Instance.SetVolumes(
                    DataManager.Instance.PlayerData.musicVolume,
                    isOn ? 1f : 0f
                );
            }
        }

        private void OnVibrationToggleChanged(bool isOn)
        {
            DataManager.Instance?.SetVibrationEnabled(isOn);
        }

        private void OnContinueClicked()
        {
            InputManager.Instance?.BlockInput(0.35f);
            Hide(instant: true);
            GameManager.Instance?.ResumeGame();
        }

        private void OnRetryClicked()
        {
            InputManager.Instance?.BlockInput(0.35f);
            Hide(instant: true);
            LevelManager.Instance?.RetryLevel();
        }

        private void OnLevelsClicked()
        {
            InputManager.Instance?.BlockInput(0.35f);
            Hide(instant: true);
            MainMenuUI.OpenLevelsOnLoad = true;
            GameManager.Instance?.GoToMainMenu();
        }

        private void OnMainMenuClicked()
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
