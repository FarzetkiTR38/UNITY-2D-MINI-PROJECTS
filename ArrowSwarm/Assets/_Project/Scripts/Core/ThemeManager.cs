namespace ArrowSwarm.Core
{
    using ArrowSwarm.Data;
    using ArrowSwarm.Utils;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Broadcasts saved theme changes and applies the Inspector-authored camera palette.</summary>
    public class ThemeManager : Singleton<ThemeManager>
    {
        [Header("Camera Palette")]
        [SerializeField] private Color _lightCameraColor = new Color(0.96f, 0.94f, 0.90f, 1f);
        [SerializeField] private Color _darkCameraColor = new Color(0.055f, 0.078f, 0.13f, 1f);
        private ThemeMode? _appliedTheme;

        /// <summary>Fired when the theme changes or a new scene needs its theme applied.</summary>
        public static event System.Action<ThemeMode> OnThemeChanged;
        /// <summary>Saved mode, also available before the manager's Start callback.</summary>
        public static ThemeMode CurrentTheme => DataManager.Instance?.PlayerData?.theme ?? ThemeMode.Light;
        /// <summary>Whether the saved visual theme is dark.</summary>
        public static bool IsDark => CurrentTheme == ThemeMode.Dark;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
            DataManager.OnPlayerDataChanged += HandlePlayerDataChanged;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            DataManager.OnPlayerDataChanged -= HandlePlayerDataChanged;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize() => Instance?.ApplyCurrentTheme();

        private void Start() => ApplyCurrentTheme();
        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode) => ApplyCurrentTheme();

        private void HandlePlayerDataChanged(PlayerData data)
        {
            // Star/level/audio saves must not repaint every Canvas or reset visual state.
            if (data != null && _appliedTheme != data.theme) ApplyTheme(data.theme);
        }

        /// <summary>Applies the saved theme after scene loading.</summary>
        public void ApplyCurrentTheme() => ApplyTheme(CurrentTheme);

        /// <summary>Notifies scene and prefab bindings; no name searches or per-frame work.</summary>
        public void ApplyTheme(ThemeMode theme)
        {
            _appliedTheme = theme;
            var camera = Camera.main ?? FindFirstObjectByType<Camera>();
            if (camera != null)
                camera.backgroundColor = theme == ThemeMode.Dark ? _darkCameraColor : _lightCameraColor;
            OnThemeChanged?.Invoke(theme);
        }
    }
}
