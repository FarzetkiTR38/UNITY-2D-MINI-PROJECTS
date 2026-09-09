namespace ArrowSwarm.UI
{
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// UI theme role preset for automatic color and style mapping.
    /// </summary>
    public enum ThemeRole
    {
        BoardFrame,
        CardRow,
        SubCard,
        ContrastText,
        Background,
        Custom
    }

    /// <summary>
    /// Binds an Image or TextMeshProUGUI to the ThemeManager.
    /// Supports automatic color tinting by role and optional sprite swapping.
    /// </summary>
    [DisallowMultipleComponent]
    public class ThemedUIElement : MonoBehaviour
    {
        [Header("Role & Palette")]
        [SerializeField] private ThemeRole _role = ThemeRole.BoardFrame;
        [SerializeField] private Color _customLightColor = Color.white;
        [SerializeField] private Color _customDarkColor = Color.white;

        [Header("Optional Dedicated Sprites")]
        [SerializeField] private Sprite _lightSprite;
        [SerializeField] private Sprite _darkSprite;

        private Image _image;
        private TextMeshProUGUI _text;
        private bool _isInitialized;
        private Color _originalLightColor;
        private Sprite _originalLightSprite;

        // Preset Colors
        public static readonly Color BoardFrameLight = new Color(0.92f, 0.96f, 1.0f, 1.0f);   // #EBF5FF
        public static readonly Color BoardFrameDark = new Color(0.06f, 0.08f, 0.14f, 0.98f);  // #101524 deep navy
        public static readonly Color CardRowLight = new Color(0.61f, 0.81f, 1.0f, 1.0f);      // #9BCFFF
        public static readonly Color CardRowDark = new Color(0.08f, 0.11f, 0.19f, 0.95f);    // #141C30
        public static readonly Color SubCardLight = Color.white;
        public static readonly Color SubCardDark = new Color(0.10f, 0.13f, 0.22f, 0.95f);     // #1A2138
        public static readonly Color ContrastTextLight = new Color(0.10f, 0.29f, 0.75f, 1.0f);// #1A49C0
        public static readonly Color ContrastTextDark = new Color(0.94f, 0.96f, 1.0f, 1.0f);  // #F0F5FF crisp white
        public static readonly Color BackgroundLight = Color.white;
        public static readonly Color BackgroundDark = new Color(0.16f, 0.18f, 0.28f, 1.0f);   // #292E47

        public ThemeRole Role { get => _role; set => _role = value; }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void OnEnable()
        {
            EnsureInitialized();
            ThemeManager.OnThemeChanged += HandleThemeChanged;
            ApplyCurrentTheme();
        }

        private void OnDisable()
        {
            ThemeManager.OnThemeChanged -= HandleThemeChanged;
        }

        private void EnsureInitialized()
        {
            if (_isInitialized) return;
            _image = GetComponent<Image>();
            _text = GetComponent<TextMeshProUGUI>();

            if (_image != null)
            {
                _originalLightColor = _image.color;
                if (_lightSprite == null) _lightSprite = _image.sprite;
                _originalLightSprite = _image.sprite;
            }
            else if (_text != null)
            {
                _originalLightColor = _text.color;
            }
            _isInitialized = true;
        }

        /// <summary>
        /// Applies theme based on the current theme mode in DataManager or ThemeManager.
        /// </summary>
        public void ApplyCurrentTheme()
        {
            ThemeMode mode = DataManager.Instance?.PlayerData?.theme ?? ThemeMode.Light;
            ApplyTheme(mode);
        }

        /// <summary>
        /// Applies the specified theme mode (Light or Dark) to this graphic.
        /// </summary>
        public void ApplyTheme(ThemeMode mode)
        {
            EnsureInitialized();
            bool isDark = mode == ThemeMode.Dark;

            if (_image != null)
            {
                if (_darkSprite != null && _lightSprite != null)
                {
                    _image.sprite = isDark ? _darkSprite : _lightSprite;
                }

                _image.color = GetTargetColor(isDark);
            }
            else if (_text != null)
            {
                _text.color = GetTargetColor(isDark);
            }
        }

        private Color GetTargetColor(bool isDark)
        {
            switch (_role)
            {
                case ThemeRole.BoardFrame: return isDark ? BoardFrameDark : (_isInitialized && _originalLightColor != Color.clear ? _originalLightColor : BoardFrameLight);
                case ThemeRole.CardRow: return isDark ? CardRowDark : (_isInitialized && _originalLightColor != Color.clear ? _originalLightColor : CardRowLight);
                case ThemeRole.SubCard: return isDark ? SubCardDark : (_isInitialized && _originalLightColor != Color.clear ? _originalLightColor : SubCardLight);
                case ThemeRole.ContrastText: return isDark ? ContrastTextDark : (_isInitialized && _originalLightColor != Color.clear ? _originalLightColor : ContrastTextLight);
                case ThemeRole.Background: return isDark ? BackgroundDark : BackgroundLight;
                case ThemeRole.Custom: return isDark ? _customDarkColor : _customLightColor;
                default: return isDark ? Color.gray : Color.white;
            }
        }

        private void HandleThemeChanged(ThemeMode mode) => ApplyTheme(mode);
    }
}
