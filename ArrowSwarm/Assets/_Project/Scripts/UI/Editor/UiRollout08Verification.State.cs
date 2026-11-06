namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using ArrowSwarm.Ads;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Modes.Editor;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using DebugManager = ArrowSwarm.Debug.DebugManager;

    /// <summary>Owns only cloned data, original preferences/provider/debug and deterministic preview context.</summary>
    public static partial class UiRollout08Verification
    {
        private sealed class Scope : IDisposable
        {
            private static readonly FieldInfo Factory = typeof(LeaderboardManager).GetField("_editorCloudFactory", BindingFlags.Static | BindingFlags.NonPublic);
            internal readonly ModalAds Ads = new ModalAds();
            internal int FactoryCalls;
            private readonly QualityDataSnapshot _data;
            private readonly object _factory;
            private readonly string _player, _language, _config, _debugJson;
            private readonly bool _background, _createdDebug, _preview, _levelsOnLoad;
            private readonly int _previewLevel, _lives;
            private readonly GameModeId _previewMode;
            private readonly float _time;
            private readonly UnityEngine.Random.State _random;
            private readonly DebugManager _debug;
            private readonly AdManager _ads;
            private readonly IAdService _provider;
            private bool _disposed, _factoryInstalled, _adSwapped;

            internal Scope()
            {
                if (Factory == null || AdManager.Instance.IsShowingAd)
                    throw new InvalidOperationException("No pending ad and an available Editor cloud seam are required.");
                _factory = Factory.GetValue(null); _player = JsonUtility.ToJson(DataManager.Instance.PlayerData);
                _language = LocalizationManager.Instance.CurrentLanguage; _random = UnityEngine.Random.state;
                _config = EditorJsonUtility.ToJson(GameManager.Instance.Config); _lives = GameManager.Instance.CurrentLives;
                _background = Application.runInBackground; _time = Time.timeScale;
                _preview = PuzzleLaunch.Preview; _previewMode = PuzzleLaunch.PreviewMode; _previewLevel = PuzzleLaunch.PreviewLevel;
                _levelsOnLoad = MainMenuUI.OpenLevelsOnLoad;
                try
                {
                    _data = new QualityDataSnapshot(DataManager.Instance);
                    var clone = JsonUtility.FromJson<PlayerData>(_player); clone.isTutorialCompleted = clone.isProfileSetupCompleted = true;
                    _data.SetData(clone);
                    Factory.SetValue(null, new Func<ILeaderboardCloudService>(() =>
                    { FactoryCalls++; throw new InvalidOperationException("Board08 verification forbids cloud provider creation."); }));
                    _factoryInstalled = true;
                    _createdDebug = !DebugManager.HasInstance; _debug = DebugManager.Instance; _debugJson = JsonUtility.ToJson(_debug);
                    var debug = new SerializedObject(_debug); debug.FindProperty("_useDebugLevel").boolValue = true;
                    debug.FindProperty("_debugLevel").intValue = 101; debug.FindProperty("_infiniteLives").boolValue = true;
                    debug.FindProperty("_gameSpeedMultiplier").floatValue = 1; debug.ApplyModifiedPropertiesWithoutUndo();
                    _ads = AdManager.Instance; _provider = Field<IAdService>(_ads, "_adService");
                    _ads.SetAdService(Ads); _adSwapped = true;
                    Application.runInBackground = true; Time.timeScale = 1; MainMenuUI.OpenLevelsOnLoad = false;
                    PuzzleLaunch.Preview = false; UnityEngine.Random.InitState(808101);
                }
                catch { Dispose(false); throw; }
            }

            /// <summary>Attempts every restoration even if an earlier cleanup action fails.</summary>
            public void Dispose() => Dispose(true);

            internal void Dispose(bool verifyNativeMenu)
            {
                if (_disposed) return; _disposed = true;
                var failures = new List<Exception>();
                Restore(failures, () =>
                {
                    if (_debug == null) return;
                    if (!string.IsNullOrEmpty(_debugJson)) JsonUtility.FromJsonOverwrite(_debugJson, _debug);
                    if (_createdDebug) UnityEngine.Object.DestroyImmediate(_debug.gameObject);
                });
                Restore(failures, () => { if (_adSwapped && _ads != null) _ads.SetAdService(_provider); });
                Restore(failures, () => { if (_factoryInstalled) Factory.SetValue(null, _factory); });
                Restore(failures, () => _data?.Dispose());
                Restore(failures, () =>
                {
                    PuzzleLaunch.Preview = _preview; PuzzleLaunch.PreviewMode = _previewMode; PuzzleLaunch.PreviewLevel = _previewLevel;
                    MainMenuUI.OpenLevelsOnLoad = _levelsOnLoad;
                });
                Restore(failures, () =>
                {
                    if (LocalizationManager.HasInstance) LocalizationManager.Instance.SetLanguage(_language, false);
                    if (ThemeManager.HasInstance) ThemeManager.Instance.ApplyCurrentTheme();
                    var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
                    if (menu != null) typeof(MainMenuUI).GetMethod("SetupUI", Private).Invoke(menu, null);
                });
                Restore(failures, () =>
                {
                    if (GameManager.HasInstance) typeof(GameManager).GetField("_currentLives", Private).SetValue(GameManager.Instance, _lives);
                    Time.timeScale = _time; Application.runInBackground = _background; UnityEngine.Random.state = _random;
                });
                if (failures.Count > 0) throw new AggregateException("Board08 restoration failed", failures);
                _data?.VerifyRestored(Results);
                Check(JsonUtility.ToJson(DataManager.Instance.PlayerData) == _player, "Exact original player content restored");
                Check(EditorJsonUtility.ToJson(GameManager.Instance.Config) == _config, "Original GameConfig remains unchanged");
                Check(!LeaderboardManager.HasInstance && ReferenceEquals(Factory.GetValue(null), _factory),
                    "Original lazy cloud provider restored; no leaderboard manager created");
                Check(_ads == null || ReferenceEquals(Field<IAdService>(_ads, "_adService"), _provider), "Original ad provider restored");
                Check(LocalizationManager.Instance.CurrentLanguage == _language && UnityEngine.Random.state.Equals(_random),
                    "Original language and random state restored");
                if (verifyNativeMenu) Check(SceneManager.GetActiveScene().name == "MainMenuScene" && GameManager.Instance.CurrentLives == _lives,
                    "MainMenu and original lives restored after native scene activation");
            }

            private static void Restore(List<Exception> failures, Action action)
            { try { action(); } catch (Exception error) { failures.Add(error); } }
        }
    }
}
