namespace ArrowSwarm.Core
{
    using ArrowSwarm.Data;
    using ArrowSwarm.Utils;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Manages visual theme mode (Light / Dark) across all scenes,
    /// updating camera background clear color and backdrop images.
    /// </summary>
    public class ThemeManager : Singleton<ThemeManager>
    {
        public static readonly Color LightCamColor = new Color(0.96f, 0.94f, 0.90f, 1f);
        public static readonly Color DarkCamColor = new Color(0.10f, 0.10f, 0.18f, 1f);

        public static readonly Color LightBgImageColor = Color.white;
        public static readonly Color DarkBgImageColor = new Color(0.18f, 0.20f, 0.28f, 1f);

        private void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += HandleSceneLoaded;
            DataManager.OnPlayerDataChanged += HandlePlayerDataChanged;
        }

        private void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= HandleSceneLoaded;
            DataManager.OnPlayerDataChanged -= HandlePlayerDataChanged;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            if (Instance != null) Instance.ApplyCurrentTheme();
        }

        private void Start() => ApplyCurrentTheme();

        private void HandleSceneLoaded(UnityEngine.SceneManagement.Scene s, UnityEngine.SceneManagement.LoadSceneMode m) => ApplyCurrentTheme();

        private void HandlePlayerDataChanged(PlayerData data)
        {
            if (data != null) ApplyTheme(data.theme);
        }

        /// <summary>Applies the theme mode stored in DataManager.</summary>
        public void ApplyCurrentTheme()
        {
            ThemeMode theme = DataManager.Instance?.PlayerData?.theme ?? ThemeMode.Light;
            ApplyTheme(theme);
        }

        /// <summary>
        /// Applies the specified theme to cameras and background images in the active scene.
        /// </summary>
        public void ApplyTheme(ThemeMode theme)
        {
            bool isDark = theme == ThemeMode.Dark;
            var cam = Camera.main ?? FindFirstObjectByType<Camera>();
            if (cam != null)
            {
                cam.backgroundColor = isDark ? DarkCamColor : LightCamColor;
            }

            var bgObj = GameObject.Find("BG") ?? GameObject.Find("Background");
            if (bgObj != null && bgObj.TryGetComponent<Image>(out var img))
            {
                img.color = isDark ? DarkBgImageColor : LightBgImageColor;
            }
        }
    }
}
