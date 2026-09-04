namespace ArrowSwarm.Localization
{
    using TMPro;
    using UnityEngine;

    /// <summary>
    /// Component that binds a TextMeshProUGUI element to a localization key.
    /// Automatically updates the text when the language changes.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    [DisallowMultipleComponent]
    public class LocalizedText : MonoBehaviour
    {
        [Tooltip("The localization key defined in language JSON files (e.g. 'menu_play').")]
        [SerializeField] private string _localizationKey;

        [Tooltip("Optional prefix text before localized string.")]
        [SerializeField] private string _prefix = string.Empty;

        [Tooltip("Optional suffix text after localized string.")]
        [SerializeField] private string _suffix = string.Empty;

        private TextMeshProUGUI _text;

        /// <summary>Gets or sets the current localization key and immediately refreshes text.</summary>
        public string LocalizationKey
        {
            get => _localizationKey;
            set
            {
                _localizationKey = value;
                RefreshText();
            }
        }

        private void Awake()
        {
            EnsureTextComponent();
        }

        private void OnEnable()
        {
            EnsureTextComponent();
            LocalizationManager.OnLanguageChanged += RefreshText;
            RefreshText();
        }

        private void OnDisable()
        {
            LocalizationManager.OnLanguageChanged -= RefreshText;
        }

        /// <summary>Sets a new localization key and refreshes text display.</summary>
        public void SetKey(string key)
        {
            _localizationKey = key;
            RefreshText();
        }

        /// <summary>Refreshes the displayed text using the current language in LocalizationManager.</summary>
        public void RefreshText()
        {
            if (string.IsNullOrEmpty(_localizationKey)) return;
            EnsureTextComponent();
            if (_text == null) return;

            if (LocalizationManager.Instance != null)
            {
                string localized = LocalizationManager.Instance.GetText(_localizationKey, _text.text);
                _text.text = $"{_prefix}{localized}{_suffix}";
            }
        }

        private void EnsureTextComponent()
        {
            if (_text == null) _text = GetComponent<TextMeshProUGUI>();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying && _text != null && !string.IsNullOrEmpty(_localizationKey))
            {
                // Visual reminder in Inspector if empty
            }
        }
#endif
    }
}
