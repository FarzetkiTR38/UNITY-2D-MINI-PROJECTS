namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Reflection;
    using ArrowSwarm.Ads;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Modes.Editor;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.SceneManagement;
    using DebugManager = ArrowSwarm.Debug.DebugManager;

    /// <summary>Scopes cloned progress, five preferences, providers, debug, language and transient scene state.</summary>
    public static partial class UiRollout01Verification
    {
        private sealed partial class Scope : IDisposable
        {
            private static readonly FieldInfo Factory = typeof(LeaderboardManager).GetField("_editorCloudFactory", BindingFlags.Static | BindingFlags.NonPublic);
            internal readonly OfflineCloud Cloud = new OfflineCloud();
            internal readonly NoAds Ads = new NoAds();
            private readonly QualityDataSnapshot _data;
            private readonly object _factory;
            private readonly string _player, _language, _debugJson, _config, _selectionPath;
            private readonly bool _background, _createdDebug, _preview, _openLevels;
            private readonly int _previewLevel, _lives;
            private readonly GameModeId _previewMode;
            private readonly float _time;
            private readonly UnityEngine.Random.State _random;
            private readonly DebugManager _debug;
            private readonly AdManager _adManager;
            private readonly IAdService _adProvider;
            private bool _stateCaptured, _factoryInstalled, _adSwapped;
            private bool _disposed;

            internal Scope()
            {
                var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
                var profile = UnityEngine.Object.FindFirstObjectByType<ProfileSetupModalUI>(FindObjectsInactive.Include);
                if (menu == null || Field<GameObject>(menu, "_activeSubPanel") != null ||
                    (profile != null && profile.gameObject.activeInHierarchy && Field<CanvasGroup>(profile, "_canvasGroup").alpha > 0))
                    throw new InvalidOperationException("Close profile/subpanels before the phase 01 check.");
                if (Factory == null || (AdManager.HasInstance && AdManager.Instance.IsShowingAd))
                    throw new InvalidOperationException("Editor provider seam must be available and no ad may be pending.");
                _factory = Factory.GetValue(null); _player = JsonUtility.ToJson(DataManager.Instance.PlayerData);
                _language = LocalizationManager.Instance.CurrentLanguage; _random = UnityEngine.Random.state;
                _selectionPath = SelectionPath(EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null);
                _background = Application.runInBackground; _time = Time.timeScale;
                _config = EditorJsonUtility.ToJson(GameManager.Instance.Config); _lives = GameManager.Instance.CurrentLives;
                _preview = PuzzleLaunch.Preview; _previewMode = PuzzleLaunch.PreviewMode; _previewLevel = PuzzleLaunch.PreviewLevel;
                _openLevels = MainMenuUI.OpenLevelsOnLoad; _stateCaptured = true;
                try
                {
                    _data = new QualityDataSnapshot(DataManager.Instance);
                    var clone = JsonUtility.FromJson<PlayerData>(_player); clone.isTutorialCompleted = clone.isProfileSetupCompleted = true;
                    _data.SetData(clone); Factory.SetValue(null, new Func<ILeaderboardCloudService>(() => Cloud)); _factoryInstalled = true;
                    _createdDebug = !DebugManager.HasInstance; _debug = DebugManager.Instance; _debugJson = JsonUtility.ToJson(_debug);
                    var debug = new SerializedObject(_debug); debug.FindProperty("_useDebugLevel").boolValue = true;
                    debug.FindProperty("_debugLevel").intValue = 101; debug.FindProperty("_infiniteLives").boolValue = true;
                    debug.FindProperty("_gameSpeedMultiplier").floatValue = 1; debug.ApplyModifiedPropertiesWithoutUndo();
                    if (AdManager.HasInstance)
                    {
                        _adManager = AdManager.Instance; _adProvider = Field<IAdService>(_adManager, "_adService");
                        _adSwapped = true; _adManager.SetAdService(Ads);
                    }
                    Application.runInBackground = true; Time.timeScale = 1; MainMenuUI.OpenLevelsOnLoad = false;
                    PuzzleLaunch.Preview = false; UnityEngine.Random.InitState(270101);
                }
                catch
                {
                    var failures = new System.Collections.Generic.List<Exception>();
                    RestoreState(failures, false);
                    foreach (var failure in failures) Results.Add(failure.ToString());
                    throw;
                }
            }

            public void Dispose() => Dispose(true);

            internal void Dispose(bool returnToMenu)
            {
                if (_disposed) return; _disposed = true;
                var failures = new System.Collections.Generic.List<Exception>();
                try
                {
                    if (returnToMenu) RestoreAttempt(failures, ReturnToMenu, "return to the existing main menu");
                }
                finally
                {
                    RestoreState(failures, false);
                }
                if (failures.Count > 0) throw new AggregateException("Verification cleanup could not restore every original state.", failures);
            }

            internal void CompleteRestore(bool verifyNativeMenu)
            {
                var failures = new System.Collections.Generic.List<Exception>();
                RestoreState(failures, verifyNativeMenu);
                if (failures.Count > 0) throw new AggregateException("Final native restoration could not restore every original state.", failures);
                if (verifyNativeMenu) VerifyRestored();
                else _data?.VerifyRestored(Results);
            }

            private static string SelectionPath(GameObject selected)
            {
                if (selected == null) return string.Empty;
                string path = selected.name;
                for (var parent = selected.transform.parent; parent != null; parent = parent.parent) path = parent.name + "/" + path;
                return path;
            }
        }
    }
}
