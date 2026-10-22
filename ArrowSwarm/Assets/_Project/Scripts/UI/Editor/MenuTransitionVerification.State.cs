namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Reflection;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Modes.Editor;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using DebugManager = ArrowSwarm.Debug.DebugManager;
    using Random = UnityEngine.Random;

    /// <summary>Restores exact player/preferences, debug, menu and material state after real scene navigation.</summary>
    public static partial class MenuTransitionVerification
    {
        /// <summary>Owns only temporary runtime changes; no config or material asset is modified.</summary>
        private sealed class Scope : IDisposable
        {
            internal readonly Material Source, Runtime;
            internal readonly string SourceJson, ConfigJson;
            internal readonly PlayerData Player;
            private readonly QualityDataSnapshot _data;
            private readonly DebugManager _debug;
            private readonly string _debugJson, _playerJson;
            private readonly bool _createdDebug, _background, _preview, _levelsOnLoad;
            private readonly int _previewLevel, _lives;
            private readonly GameModeId _previewMode;
            private readonly GameState _state;
            private readonly float _time;
            private readonly Random.State _random;
            private bool _disposed;

            internal Scope()
            {
                if (!GameManager.HasInstance || !DataManager.HasInstance)
                    throw new InvalidOperationException("The real main menu and initialized core managers are required.");
                var manager = SceneTransitionManager.Instance;
                Source = Field<Material>(manager, "_customMaterialOverride");
                Runtime = Field<Image>(manager, "_overlayImage").material;
                if (Source == null || Runtime == null) throw new InvalidOperationException("Authored custom material and runtime overlay are required.");
                SourceJson = EditorJsonUtility.ToJson(Source); ConfigJson = JsonUtility.ToJson(GameManager.Instance.Config);
                _playerJson = JsonUtility.ToJson(DataManager.Instance.PlayerData);
                Player = JsonUtility.FromJson<PlayerData>(_playerJson);
                _background = Application.runInBackground; _time = Time.timeScale; _random = Random.state;
                _state = GameManager.Instance.CurrentState; _lives = GameManager.Instance.CurrentLives;
                _preview = PuzzleLaunch.Preview; _previewMode = PuzzleLaunch.PreviewMode; _previewLevel = PuzzleLaunch.PreviewLevel;
                _levelsOnLoad = MainMenuUI.OpenLevelsOnLoad;
                _createdDebug = !DebugManager.HasInstance; _debug = DebugManager.Instance;
                _debugJson = JsonUtility.ToJson(_debug);
                _data = new QualityDataSnapshot(DataManager.Instance);
                try
                {
                    _data.SetData(Player); Player.isTutorialCompleted = Player.isProfileSetupCompleted = true;
                    Player.shapesProgress = new ModeProgress(); Player.colorShiftProgress = new ModeProgress();
                    var serialized = new SerializedObject(_debug);
                    serialized.FindProperty("_useDebugLevel").boolValue = true;
                    serialized.FindProperty("_debugLevel").intValue = 101;
                    serialized.FindProperty("_infiniteLives").boolValue = true;
                    serialized.FindProperty("_infiniteTips").boolValue = false;
                    serialized.FindProperty("_gameSpeedMultiplier").floatValue = 1f;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    MainMenuUI.OpenLevelsOnLoad = false; PuzzleLaunch.Preview = false;
                    Application.runInBackground = true;
                }
                catch { Dispose(); throw; }
            }

            /// <summary>Restores the original context on success, failed assertions, cancellation or script reload.</summary>
            public void Dispose()
            {
                if (_disposed) return; _disposed = true;
                try
                {
                    if (Application.isPlaying)
                    {
                        if (SceneTransitionManager.HasInstance && SceneTransitionManager.Instance.IsTransitioning)
                            SceneTransitionManager.Instance.StopAllCoroutines();
                        if (LevelManager.HasInstance) LevelManager.Instance.CleanupLevel();
                        if (SceneManager.GetActiveScene().name != "MainMenuScene") SceneManager.LoadScene("MainMenuScene");
                    }
                }
                finally
                {
                    _data.Dispose(); _data.VerifyRestored(Results);
                    Check(JsonUtility.ToJson(DataManager.Instance.PlayerData) == _playerJson, "Original player JSON, progress and inventory restored exactly");
                    if (GameManager.HasInstance)
                    {
                        GameManager.Instance.SetState(_state);
                        typeof(GameManager).GetField("_currentLives", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(GameManager.Instance, _lives);
                    }
                    if (_debug != null)
                    {
                        JsonUtility.FromJsonOverwrite(_debugJson, _debug);
                        if (_createdDebug) UnityEngine.Object.DestroyImmediate(_debug.gameObject);
                    }
                    PuzzleLaunch.Preview = _preview; PuzzleLaunch.PreviewMode = _previewMode; PuzzleLaunch.PreviewLevel = _previewLevel;
                    MainMenuUI.OpenLevelsOnLoad = _levelsOnLoad;
                    Time.timeScale = _time; Application.runInBackground = _background; Random.state = _random;
                    var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
                    if (menu != null) typeof(MainMenuUI).GetMethod("SetupUI", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(menu, null);
                    Check(Source != null && EditorJsonUtility.ToJson(Source) == SourceJson, "Authored custom material remains unchanged after cleanup");
                    Check(!Application.isPlaying || SceneManager.GetActiveScene().name == "MainMenuScene", "Initial MainMenu scene restored");
                }
            }
        }
    }
}
