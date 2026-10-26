namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Reflection;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.UI;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using DebugManager = ArrowSwarm.Debug.DebugManager;

    /// <summary>Contains title sweeps inside cloned player data and restores every persistent or runtime setting it owns.</summary>
    public static partial class Quality4PauseTitleVerification
    {
        private static void ConfigurePreview(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "MapTestScene") return;
            var controller = UnityEngine.Object.FindFirstObjectByType<MapSceneController>();
            if (controller == null) return;
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("_previewMode").enumValueIndex = (int)GameModeId.Defense;
            serialized.FindProperty("_enableTutorialTest").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo(); controller.DefaultLevel = 101;
        }

        private static void ReturnToMenu()
        {
            if (ArrowSwarm.Skills.FreezeManager.HasInstance) ArrowSwarm.Skills.FreezeManager.Instance.EndFreezeSilently();
            if (LevelManager.HasInstance) LevelManager.Instance.CleanupLevel();
            Time.timeScale = 1f; GameManager.Instance.SetState(GameState.Menu);
            if (SceneManager.GetActiveScene().name != "MainMenuScene") SceneManager.LoadScene("MainMenuScene");
        }

        private sealed class Scope : IDisposable
        {
            private static readonly FieldInfo Factory = typeof(LeaderboardManager).GetField("_editorCloudFactory", BindingFlags.NonPublic | BindingFlags.Static);
            private readonly QualityDataSnapshot _data;
            private readonly object _factory;
            private readonly string _playerJson, _debugJson, _configJson, _language;
            private readonly DebugManager _debug;
            private readonly bool _createdDebug, _background, _preview, _levelsOnLoad;
            private readonly GameModeId _previewMode;
            private readonly int _previewLevel;
            private readonly float _time;
            private readonly UnityEngine.Random.State _random;
            private bool _disposed;
            internal int FactoryCalls;

            internal Scope()
            {
                if (Factory == null) throw new InvalidOperationException("The Editor-only cloud seam is required.");
                _factory = Factory.GetValue(null); _language = LocalizationManager.Instance.CurrentLanguage;
                _playerJson = JsonUtility.ToJson(DataManager.Instance.PlayerData); _data = new QualityDataSnapshot(DataManager.Instance);
                var player = JsonUtility.FromJson<PlayerData>(_playerJson); player.isTutorialCompleted = player.isProfileSetupCompleted = true;
                _data.SetData(player);
                _background = Application.runInBackground; _time = Time.timeScale; _random = UnityEngine.Random.state;
                _preview = PuzzleLaunch.Preview; _previewMode = PuzzleLaunch.PreviewMode; _previewLevel = PuzzleLaunch.PreviewLevel;
                _levelsOnLoad = MainMenuUI.OpenLevelsOnLoad; _createdDebug = !DebugManager.HasInstance;
                _debug = DebugManager.Instance; _debugJson = JsonUtility.ToJson(_debug); _configJson = EditorJsonUtility.ToJson(GameManager.Instance.Config);
                var debug = new SerializedObject(_debug); debug.FindProperty("_useDebugLevel").boolValue = true;
                debug.FindProperty("_debugLevel").intValue = 101; debug.FindProperty("_infiniteLives").boolValue = true;
                debug.FindProperty("_gameSpeedMultiplier").floatValue = 1; debug.ApplyModifiedPropertiesWithoutUndo();
                Factory.SetValue(null, new Func<ILeaderboardCloudService>(() =>
                { FactoryCalls++; throw new InvalidOperationException("Title-only fixture forbids cloud-manager creation."); }));
                Application.runInBackground = true; Time.timeScale = 1f; MainMenuUI.OpenLevelsOnLoad = false;
                UnityEngine.Random.InitState(426102);
            }

            public void Dispose()
            {
                if (_disposed) return; _disposed = true;
                try
                {
                    if (LeaderboardManager.HasInstance) UnityEngine.Object.DestroyImmediate(LeaderboardManager.Instance.gameObject);
                    if (_debug != null)
                    {
                        JsonUtility.FromJsonOverwrite(_debugJson, _debug);
                        if (_createdDebug) UnityEngine.Object.DestroyImmediate(_debug.gameObject);
                    }
                    PuzzleLaunch.Preview = _preview; PuzzleLaunch.PreviewMode = _previewMode; PuzzleLaunch.PreviewLevel = _previewLevel;
                    MainMenuUI.OpenLevelsOnLoad = _levelsOnLoad;
                }
                finally
                {
                    Factory.SetValue(null, _factory); _data.Dispose();
                    if (LocalizationManager.HasInstance) LocalizationManager.Instance.SetLanguage(_language, false);
                    if (ThemeManager.HasInstance) ThemeManager.Instance.ApplyCurrentTheme();
                    var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
                    if (menu != null) typeof(MainMenuUI).GetMethod("SetupUI", Private).Invoke(menu, null);
                    var modes = UnityEngine.Object.FindFirstObjectByType<GameModeSelectUI>(FindObjectsInactive.Include);
                    if (modes != null) typeof(GameModeSelectUI).GetMethod("Refresh", Private).Invoke(modes, null);
                    Time.timeScale = _time; Application.runInBackground = _background; UnityEngine.Random.state = _random;
                    _data.VerifyRestored(Results);
                    Check(JsonUtility.ToJson(DataManager.Instance.PlayerData) == _playerJson, "Exact original player object and all progress restored");
                    Check(LocalizationManager.Instance.CurrentLanguage == _language, "Original live language restored");
                    Check(EditorJsonUtility.ToJson(GameManager.Instance.Config) == _configJson, "Authored GameConfig unchanged");
                    Check(!LeaderboardManager.HasInstance && ReferenceEquals(Factory.GetValue(null), _factory), "Exact original lazy cloud provider restored");
                    Check(SceneManager.GetActiveScene().name == "MainMenuScene" && GameManager.Instance.CurrentState == GameState.Menu,
                        "Actual main menu and normal menu state restored");
                }
            }
        }
    }
}
