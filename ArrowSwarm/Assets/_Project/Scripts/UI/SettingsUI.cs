namespace ArrowSwarm.UI
{
    using System.Collections.Generic;
    using ArrowSwarm.Audio;
    using ArrowSwarm.Data;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Controls the Settings panel with SFX, VFX, Vibration toggles, and Language selector.
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
        [SerializeField] private Button _languageButton; // Clicking the whole pill also cycles

        [Header("Decoration & Board")]
        [SerializeField] private Image _boardImage;
        [SerializeField] private Image _footerImage;

        [Header("Row Background Cards")]
        [SerializeField] private Image _sfxRowCard;
        [SerializeField] private Image _vfxRowCard;
        [SerializeField] private Image _vibrationRowCard;
        [SerializeField] private Image _languageRowCard;

        [Header("Row Icons")]
        [SerializeField] private Image _sfxIcon;
        [SerializeField] private Image _vfxIcon;
        [SerializeField] private Image _vibrationIcon;
        [SerializeField] private Image _languageIcon;

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

            // Auto-wire toggles
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

            // Auto-wire Language
            if (_languageText == null)
            {
                var txt = transform.Find("BoardFrame/SettingsContainer/SettingRow_Language/LanguageSelector/LanguageText");
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

            if (_sfxToggle != null)
            {
                _sfxToggle.SetIsOn(data.sfxEnabled, false);
            }

            if (_vfxToggle != null)
            {
                _vfxToggle.SetIsOn(data.vfxEnabled, false);
            }

            if (_vibrationToggle != null)
            {
                _vibrationToggle.SetIsOn(data.vibrationEnabled, false);
            }

            // Find current language index
            string lang = !string.IsNullOrEmpty(data.selectedLanguage) ? data.selectedLanguage.ToUpper() : "ENGLISH";
            _currentLanguageIndex = 0;
            for (int i = 0; i < SUPPORTED_LANGUAGES.Length; i++)
            {
                if (SUPPORTED_LANGUAGES[i] == lang)
                {
                    _currentLanguageIndex = i;
                    break;
                }
            }
            UpdateLanguageDisplay();
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
            _currentLanguageIndex--;
            if (_currentLanguageIndex < 0) _currentLanguageIndex = SUPPORTED_LANGUAGES.Length - 1;
            ApplyLanguageChange();
        }

        private void NextLanguage()
        {
            _currentLanguageIndex = (_currentLanguageIndex + 1) % SUPPORTED_LANGUAGES.Length;
            ApplyLanguageChange();
        }

        private void ApplyLanguageChange()
        {
            string newLang = SUPPORTED_LANGUAGES[_currentLanguageIndex];
            DataManager.Instance?.SetLanguage(newLang);
            UpdateLanguageDisplay();
        }

        private void UpdateLanguageDisplay()
        {
            if (_languageText != null && _currentLanguageIndex >= 0 && _currentLanguageIndex < SUPPORTED_LANGUAGES.Length)
            {
                _languageText.text = SUPPORTED_LANGUAGES[_currentLanguageIndex];
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
        }
    }
}
