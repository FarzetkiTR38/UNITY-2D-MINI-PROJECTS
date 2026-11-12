namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Reflection;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Modes.Editor;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using DebugManager = ArrowSwarm.Debug.DebugManager;

    /// <summary>Owns cloned player/debug state; native input is separately scoped around each scene.</summary>
    public static partial class Revision6PauseVerification
    {
        private static void ConfigurePreview(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "MapTestScene") return;
            var controller = UnityEngine.Object.FindFirstObjectByType<MapSceneController>();
            var data = new SerializedObject(controller); data.FindProperty("_previewMode").enumValueIndex = (int)GameModeId.Defense;
            data.FindProperty("_enableTutorialTest").boolValue = false; data.ApplyModifiedPropertiesWithoutUndo();
            controller.DefaultLevel = 101;
        }

        private static void ReturnToMenu()
        {
            if (ArrowSwarm.Skills.FreezeManager.HasInstance) ArrowSwarm.Skills.FreezeManager.Instance.EndFreezeSilently();
            if (LevelManager.HasInstance) LevelManager.Instance.CleanupLevel();
            Time.timeScale = 1; GameManager.Instance.SetState(GameState.Menu);
            if (SceneManager.GetActiveScene().name != "MainMenuScene") SceneManager.LoadScene("MainMenuScene");
        }

        /// <summary>Restores exact original data, preferences, tuning, preview, language and random state.</summary>
        private sealed class Scope : IDisposable
        {
            private static readonly FieldInfo Factory = typeof(LeaderboardManager).GetField("_editorCloudFactory", BindingFlags.Static | BindingFlags.NonPublic);
            private readonly QualityDataSnapshot _data;
            private readonly object _factory;
            private readonly string _player, _debugJson, _config, _language;
            private readonly DebugManager _debug;
            private readonly bool _createdDebug, _background, _preview, _levelsOnLoad;
            private readonly float _time;
            private readonly int _previewLevel, _lives;
            private readonly GameModeId _previewMode;
            private readonly UnityEngine.Random.State _random;
            private bool _disposed;
            internal int FactoryCalls;

            internal Scope()
            {
                _factory = Factory.GetValue(null); _player = JsonUtility.ToJson(DataManager.Instance.PlayerData);
                _data = new QualityDataSnapshot(DataManager.Instance);
                var clone = JsonUtility.FromJson<PlayerData>(_player); clone.isTutorialCompleted = clone.isProfileSetupCompleted = true;
                _data.SetData(clone); _language = LocalizationManager.Instance.CurrentLanguage;
                _config = EditorJsonUtility.ToJson(GameManager.Instance.Config); _lives = GameManager.Instance.CurrentLives;
                _background = Application.runInBackground; _time = Time.timeScale; _random = UnityEngine.Random.state;
                _preview = PuzzleLaunch.Preview; _previewMode = PuzzleLaunch.PreviewMode; _previewLevel = PuzzleLaunch.PreviewLevel;
                _levelsOnLoad = MainMenuUI.OpenLevelsOnLoad; _createdDebug = !DebugManager.HasInstance;
                _debug = DebugManager.Instance; _debugJson = JsonUtility.ToJson(_debug);
                var debug = new SerializedObject(_debug); debug.FindProperty("_useDebugLevel").boolValue = true;
                debug.FindProperty("_debugLevel").intValue = 101; debug.FindProperty("_infiniteLives").boolValue = true;
                debug.FindProperty("_gameSpeedMultiplier").floatValue = 1; debug.ApplyModifiedPropertiesWithoutUndo();
                Factory.SetValue(null, new Func<ILeaderboardCloudService>(() =>
                { FactoryCalls++; throw new InvalidOperationException("Pause verification forbids external leaderboard creation."); }));
                Application.runInBackground = true; Time.timeScale = 1; MainMenuUI.OpenLevelsOnLoad = false;
                UnityEngine.Random.InitState(626101);
            }

            /// <summary>Restores even on timeout, cancelled input or script reload.</summary>
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
                    LocalizationManager.Instance.SetLanguage(_language, false); ThemeManager.Instance.ApplyCurrentTheme();
                    var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
                    if (menu != null) typeof(MainMenuUI).GetMethod("SetupUI", Private).Invoke(menu, null);
                    typeof(GameManager).GetField("_currentLives", Private).SetValue(GameManager.Instance, _lives);
                    Time.timeScale = _time; Application.runInBackground = _background; UnityEngine.Random.state = _random;
                    _data.VerifyRestored(Results);
                    Check(JsonUtility.ToJson(DataManager.Instance.PlayerData) == _player, "Exact original player data and preferences restored");
                    Check(EditorJsonUtility.ToJson(GameManager.Instance.Config) == _config, "Original GameConfig remains unchanged");
                    Check(!LeaderboardManager.HasInstance && ReferenceEquals(Factory.GetValue(null), _factory), "Original lazy provider restored with no fake manager retained");
                    Check(LocalizationManager.Instance.CurrentLanguage == _language && UnityEngine.Random.state.Equals(_random),
                        "Original language and random state restored");
                    Check(GameManager.Instance.CurrentLives == _lives && SceneManager.GetActiveScene().name == "MainMenuScene",
                        "Original lives and actual main menu restored");
                }
            }
        }
    }
}
