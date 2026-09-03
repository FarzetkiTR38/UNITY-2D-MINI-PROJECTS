namespace ArrowSwarm.UI
{
    using System.Collections.Generic;
    using ArrowSwarm.Audio;
    using ArrowSwarm.Data;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Controls the Settings panel with SFX, VFX, Vibration toggles, Language selector, and Theme selector.
    /// Matches the visual mockup layout and integrates directly with DataManager, AudioManager, and ParticleManager.
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TextMeshProUGUI _titleText;

        [Header("Toggles")]
        [SerializeField] private SettingsToggleUI _sfxToggle;
        [SerializeField] private SettingsToggleUI _vfxToggle;
        [SerializeField] private SettingsToggleUI _vibrationToggle;

        [Header("Language Selector")]
        [SerializeField] private TextMeshProUGUI _languageText;
        [SerializeField] private Button _prevLanguageButton;
        [SerializeField] private Button _nextLanguageButton;
        [SerializeField] private Button _languageButton;

        [Header("Theme Selector")]
        [Tooltip("Target Image component to swap theme sprites (e.g. ThemeSelector)")]
        [SerializeField] private Image _themeSelectorImage;
        [Tooltip("Button to toggle theme on click (optional)")]
        [SerializeField] private Button _themeToggleButton;
        [Tooltip("Sprite displayed when Light theme is selected")]
        [SerializeField] private Sprite _lightThemeSprite;
        [Tooltip("Sprite displayed when Dark theme is selected")]
        [SerializeField] private Sprite _darkThemeSprite;
        [Tooltip("Direct button to select Light theme (optional)")]
        [SerializeField] private Button _lightThemeButton;
        [Tooltip("Direct button to select Dark theme (optional)")]
        [SerializeField] private Button _darkThemeButton;

        [Header("Decoration & Board")]
        [SerializeField] private Image _boardImage;
        [SerializeField] private Image _footerImage;

        [Header("Row Background Cards")]
        [SerializeField] private Image _sfxRowCard;
        [SerializeField] private Image _vfxRowCard;
        [SerializeField] private Image _vibrationRowCard;
        [SerializeField] private Image _languageRowCard;
        [SerializeField] private Image _themeRowCard;

        [Header("Row Icons")]
        [SerializeField] private Image _sfxIcon;
        [SerializeField] private Image _vfxIcon;
        [SerializeField] private Image _vibrationIcon;
        [SerializeField] private Image _languageIcon;
        [SerializeField] private Image _themeIcon;

        [Header("Animation")]
        [SerializeField] private float _fadeSpeed = 5f;

        private static readonly string[] SUPPORTED_LANGUAGES = { "ENGLISH", "TURKISH", "GERMAN", "SPANISH", "FRENCH" };
        private int _currentLanguageIndex = 0;

        private void Awake()
        {
            AutoWire();
        }

        private void Start()
        {
            _closeButton?.onClick.AddListener(Hide);

            if (_sfxToggle != null)
                _sfxToggle.OnValueChanged += OnSFXChanged;

            if (_vfxToggle != null)
                _vfxToggle.OnValueChanged += OnVFXChanged;

            if (_vibrationToggle != null)
                _vibrationToggle.OnValueChanged += OnVibrationChanged;

            _prevLanguageButton?.onClick.AddListener(PrevLanguage);
            _nextLanguageButton?.onClick.AddListener(NextLanguage);
            _languageButton?.onClick.AddListener(NextLanguage);

            _themeToggleButton?.onClick.AddListener(ToggleTheme);
            _lightThemeButton?.onClick.AddListener(OnSelectLightTheme);
            _darkThemeButton?.onClick.AddListener(OnSelectDarkTheme);

            if (_titleText != null && string.IsNullOrEmpty(_titleText.text))
                _titleText.text = "SETTINGS";

            LoadSettings();
        }

        /// <summary>
        /// Automatically discovers and links child references.
        /// </summary>
        public void AutoWire()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

            if (_closeButton == null)
            {
                var btn = transform.Find("BoardFrame/Header/CloseButton") ?? transform.Find("CloseBtn") ?? transform.Find("CloseButton");
                if (btn != null) _closeButton = btn.GetComponent<Button>();
            }

            if (_titleText == null)
            {
                var txt = transform.Find("BoardFrame/Header/TitleText") ?? transform.Find("TitleText");
                if (txt != null) _titleText = txt.GetComponent<TextMeshProUGUI>();
            }

            if (_boardImage == null)
            {
                var b = transform.Find("BoardFrame");
                if (b != null) _boardImage = b.GetComponent<Image>();
            }

            if (_footerImage == null)
            {
                var f = transform.Find("BoardFrame/Footer") ?? transform.Find("Footer");
                if (f != null) _footerImage = f.GetComponent<Image>();
            }

            AutoWireToggles();
            AutoWireLanguage();
            AutoWireTheme();
        }

        private void AutoWireToggles()
        {
            if (_sfxToggle == null)
            {
                var row = transform.Find("BoardFrame/SettingsContainer/SettingRow_SFX/Toggle");
                if (row != null) _sfxToggle = row.GetComponent<SettingsToggleUI>();
            }

            if (_vfxToggle == null)
            {
                var row = transform.Find("BoardFrame/SettingsContainer/SettingRow_VFX/Toggle");
                if (row != null) _vfxToggle = row.GetComponent<SettingsToggleUI>();
            }

            if (_vibrationToggle == null)
            {
                var row = transform.Find("BoardFrame/SettingsContainer/SettingRow_Vibration/Toggle");
                if (row != null) _vibrationToggle = row.GetComponent<SettingsToggleUI>();
            }
        }

        private void AutoWireLanguage()
        {
            if (_languageText == null)
            {
                var txt = transform.Find("BoardFrame/SettingsContainer/SettingRow_Language/LanguageSelector/LanguageText")
                       ?? transform.Find("BoardFrame/SettingsContainer/SettingRow_Language/LanguageSelector/Text");
                if (txt != null) _languageText = txt.GetComponent<TextMeshProUGUI>();
            }

            if (_prevLanguageButton == null)
            {
                var btn = transform.Find("BoardFrame/SettingsContainer/SettingRow_Language/LanguageSelector/PrevButton");
                if (btn != null) _prevLanguageButton = btn.GetComponent<Button>();
            }

            if (_nextLanguageButton == null)
            {
                var btn = transform.Find("BoardFrame/SettingsContainer/SettingRow_Language/LanguageSelector/NextButton");
                if (btn != null) _nextLanguageButton = btn.GetComponent<Button>();
            }

            if (_languageButton == null)
            {
                var btn = transform.Find("BoardFrame/SettingsContainer/SettingRow_Language/LanguageSelector");
                if (btn != null) _languageButton = btn.GetComponent<Button>();
            }
        }

        private void AutoWireTheme()
        {
            if (_themeSelectorImage == null)
            {
                var selector = transform.Find("BoardFrame/SettingsContainer/SettingRow_Theme/ThemeSelector")
                            ?? transform.Find("BoardFrame/SettingsContainer/SettingRow_Theme/ThemeToggle")
                            ?? transform.Find("BoardFrame/SettingsContainer/SettingRow_Theme");
                if (selector != null) _themeSelectorImage = selector.GetComponent<Image>();
            }

            if (_themeToggleButton == null && _themeSelectorImage != null)
            {
                _themeToggleButton = _themeSelectorImage.GetComponent<Button>();
            }

            if (_lightThemeButton == null)
            {
                var btn = transform.Find("BoardFrame/SettingsContainer/SettingRow_Theme/ThemeSelector/LightButton")
                       ?? transform.Find("BoardFrame/SettingsContainer/SettingRow_Theme/LightButton");
                if (btn != null) _lightThemeButton = btn.GetComponent<Button>();
            }

            if (_darkThemeButton == null)
            {
                var btn = transform.Find("BoardFrame/SettingsContainer/SettingRow_Theme/ThemeSelector/DarkButton")
                       ?? transform.Find("BoardFrame/SettingsContainer/SettingRow_Theme/DarkButton");
                if (btn != null) _darkThemeButton = btn.GetComponent<Button>();
            }
        }

        /// <summary>Shows the settings panel with smooth fade in.</summary>
        public void Show()
        {
            gameObject.SetActive(true);
            AutoWire();
            LoadSettings();

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
                StopAllCoroutines();
                StartCoroutine(FadeTo(1f));
            }
        }

        /// <summary>Hides the settings panel with smooth fade out.</summary>
        public void Hide()
        {
            if (!gameObject.activeInHierarchy) return;

            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
                StopAllCoroutines();
                StartCoroutine(FadeTo(0f, true));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void LoadSettings()
        {
            PlayerData data = DataManager.Instance?.PlayerData;
            if (data == null) return;

            if (_sfxToggle != null) _sfxToggle.SetIsOn(data.sfxEnabled, false);
            if (_vfxToggle != null) _vfxToggle.SetIsOn(data.vfxEnabled, false);
            if (_vibrationToggle != null) _vibrationToggle.SetIsOn(data.vibrationEnabled, false);
            UpdateLanguageDisplay();
            UpdateThemeDisplay(data.theme);
        }

        private void OnSFXChanged(bool isEnabled)
        {
            DataManager.Instance?.SetSFXEnabled(isEnabled);
            if (AudioManager.Instance != null)
            {
                DataManager.Instance?.SetVolumes(
                    DataManager.Instance.PlayerData.musicVolume,
                    isEnabled ? 1f : 0f
                );
            }
        }

        private void OnVFXChanged(bool isEnabled)
        {
            DataManager.Instance?.SetVFXEnabled(isEnabled);
        }

        private void OnVibrationChanged(bool isEnabled)
        {
            DataManager.Instance?.SetVibrationEnabled(isEnabled);
        }

        private void PrevLanguage()
        {
            if (Localization.LocalizationManager.HasInstance)
            {
                Localization.LocalizationManager.Instance.PrevLanguage();
            }
            UpdateLanguageDisplay();
        }

        private void NextLanguage()
        {
            if (Localization.LocalizationManager.HasInstance)
            {
                Localization.LocalizationManager.Instance.NextLanguage();
            }
            UpdateLanguageDisplay();
        }

        private void UpdateLanguageDisplay()
        {
            if (_languageText == null) return;

            if (Localization.LocalizationManager.HasInstance)
            {
                var mgr = Localization.LocalizationManager.Instance;
                int idx = mgr.GetCurrentLanguageIndex();
                var defs = mgr.AvailableLanguages;
                if (defs != null && idx >= 0 && idx < defs.Length)
                {
                    _languageText.text = defs[idx].nativeName.ToUpper();
                    return;
                }
            }

            _languageText.text = "ENGLISH";
        }

        /// <summary>
        /// Toggles theme between Light and Dark mode.
        /// </summary>
        public void ToggleTheme()
        {
            var currentTheme = DataManager.Instance?.PlayerData?.theme ?? ThemeMode.Light;
            var nextTheme = currentTheme == ThemeMode.Light ? ThemeMode.Dark : ThemeMode.Light;
            SetTheme(nextTheme);
        }

        /// <summary>
        /// Sets a specific theme mode and saves to DataManager.
        /// </summary>
        public void SetTheme(ThemeMode theme)
        {
            DataManager.Instance?.SetTheme(theme);
            UpdateThemeDisplay(theme);
        }

        private void OnSelectLightTheme() => SetTheme(ThemeMode.Light);
        private void OnSelectDarkTheme() => SetTheme(ThemeMode.Dark);

        private void UpdateThemeDisplay(ThemeMode theme)
        {
            if (_themeSelectorImage != null)
            {
                Sprite targetSprite = theme == ThemeMode.Light ? _lightThemeSprite : _darkThemeSprite;
                if (targetSprite != null)
                {
                    _themeSelectorImage.sprite = targetSprite;
                    _themeSelectorImage.color = Color.white;
                }
            }
        }

        private System.Collections.IEnumerator FadeTo(float target, bool disableOnComplete = false)
        {
            while (Mathf.Abs(_canvasGroup.alpha - target) > 0.01f)
            {
                _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, target, Time.unscaledDeltaTime * _fadeSpeed);
                yield return null;
            }
            _canvasGroup.alpha = target;

            if (disableOnComplete)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            _closeButton?.onClick.RemoveListener(Hide);

            if (_sfxToggle != null) _sfxToggle.OnValueChanged -= OnSFXChanged;
            if (_vfxToggle != null) _vfxToggle.OnValueChanged -= OnVFXChanged;
            if (_vibrationToggle != null) _vibrationToggle.OnValueChanged -= OnVibrationChanged;

            _prevLanguageButton?.onClick.RemoveListener(PrevLanguage);
            _nextLanguageButton?.onClick.RemoveListener(NextLanguage);
            _languageButton?.onClick.RemoveListener(NextLanguage);

            _themeToggleButton?.onClick.RemoveListener(ToggleTheme);
            _lightThemeButton?.onClick.RemoveAllListeners();
            _darkThemeButton?.onClick.RemoveAllListeners();
        }
    }
}
