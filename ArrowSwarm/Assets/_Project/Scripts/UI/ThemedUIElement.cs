namespace ArrowSwarm.UI
{
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Semantic roles retained for existing scene and prefab bindings.</summary>
    public enum ThemeRole { BoardFrame, CardRow, SubCard, ContrastText, Background, Custom }

    /// <summary>Applies Inspector-authored theme variants without changing layout or interaction state.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    public class ThemedUIElement : MonoBehaviour
    {
        [Header("Inspector Palette")]
        [SerializeField] private ThemeRole _role = ThemeRole.Custom;
        [SerializeField] private Color _customLightColor = Color.white;
        [SerializeField] private Color _customDarkColor = Color.white;
        [Tooltip("Leave dynamic state colors to the owner (locked levels, toggles, effects).")]
        [SerializeField] private bool _materialOnly;
        [Tooltip("Keep fades, hidden graphics and earned-star opacity intact when switching themes.")]
        [SerializeField] private bool _preserveAlpha = true;

        [Header("Optional Sprite Variants")]
        [SerializeField] private Sprite _lightSprite;
        [SerializeField] private Sprite _darkSprite;

        [Header("Shared Dark Material")]
        [Tooltip("Uses the existing sprite texture. The original light material is restored exactly.")]
        [SerializeField] private Material _darkMaterial;

        private Graphic _graphic;
        private Image _image;
        private Material _originalMaterial;
        private Sprite _originalSprite;
        private bool _initialized;

        /// <summary>Semantic role shown in the Inspector.</summary>
        public ThemeRole Role { get => _role; set => _role = value; }

        private void Awake() => EnsureInitialized();

        private void OnEnable()
        {
            EnsureInitialized();
            ThemeManager.OnThemeChanged += ApplyTheme;
            ApplyCurrentTheme();
        }

        private void OnDisable() => ThemeManager.OnThemeChanged -= ApplyTheme;

        private void EnsureInitialized()
        {
            if (_initialized) return;
            _graphic = GetComponent<Graphic>();
            _image = _graphic as Image;
            if (_graphic != null) _originalMaterial = _graphic.material;
            if (_image != null) _originalSprite = _image.sprite;
            _initialized = true;
        }

        /// <summary>Refreshes newly enabled or pooled graphics from the saved theme.</summary>
        public void ApplyCurrentTheme() => ApplyTheme(ThemeManager.CurrentTheme);

        /// <summary>Switches authored variants, preserving dynamic opacity and sprite state.</summary>
        public void ApplyTheme(ThemeMode mode)
        {
            EnsureInitialized();
            if (_graphic == null) return;
            bool dark = mode == ThemeMode.Dark;
            if (_darkMaterial != null)
                _graphic.material = dark ? _darkMaterial : _originalMaterial;
            if (_image != null && _darkSprite != null)
                _image.sprite = dark ? _darkSprite : (_lightSprite != null ? _lightSprite : _originalSprite);
            if (_materialOnly) return;
            Color color = dark ? _customDarkColor : _customLightColor;
            if (_preserveAlpha) color.a = _graphic.color.a;
            _graphic.color = color;
        }
    }
}
