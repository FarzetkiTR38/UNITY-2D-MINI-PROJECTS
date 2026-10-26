namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using ArrowSwarm.Ads;
    using ArrowSwarm.Core;
    using ArrowSwarm.UI;
    using UnityEditor;
    using UnityEngine;
    using DebugManager = ArrowSwarm.Debug.DebugManager;

    public static partial class Quality4GameplayVerification
    {
        /// <summary>Contains all temporary fixture state without invoking a save or cloud provider.</summary>
        private sealed class GameplayScope : IDisposable
        {
            private readonly QualityUIVerificationState _data = new QualityUIVerificationState();
            private readonly Dictionary<UnityEngine.Object, string> _assets = new Dictionary<UnityEngine.Object, string>();
            private readonly DebugManager _debug;
            private readonly string _debugJson;
            private readonly bool _createdDebug, _background, _preview, _levelsOnLoad;
            private readonly int _previewLevel;
            private readonly GameModeId _previewMode;
            private readonly float _time;
            private readonly UnityEngine.Random.State _random;
            private readonly AdManager _ads;
            private readonly IAdService _provider;
            private bool _disposed;

            /// <summary>Whether the original non-test state was restored in full.</summary>
            public bool Restored { get; private set; }

            /// <summary>Suppresses disk writes and captures provider, config, timing and debug ownership.</summary>
            public GameplayScope()
            {
                _ads = AdManager.Instance;
                if (_ads.IsShowingAd) { _data.Dispose(); throw new InvalidOperationException("An existing ad request is pending."); }
                _provider = Field<IAdService>(_ads, "_adService");
                _createdDebug = !DebugManager.HasInstance; _debug = DebugManager.Instance;
                _debugJson = JsonUtility.ToJson(_debug);
                _background = Application.runInBackground; _time = Time.timeScale; _random = UnityEngine.Random.state;
                _preview = PuzzleLaunch.Preview; _previewMode = PuzzleLaunch.PreviewMode; _previewLevel = PuzzleLaunch.PreviewLevel;
                _levelsOnLoad = MainMenuUI.OpenLevelsOnLoad;
                foreach (string path in new[] { "Assets/_Project/ScriptableObjects/Puzzles/ShapesPuzzleConfig.asset",
                    "Assets/_Project/ScriptableObjects/Puzzles/PuzzleConfig.asset" })
                { var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path); if (asset != null) _assets[asset] = EditorJsonUtility.ToJson(asset); }
                _assets[GameManager.Instance.Config] = EditorJsonUtility.ToJson(GameManager.Instance.Config);
                var debug = new SerializedObject(_debug);
                debug.FindProperty("_useDebugLevel").boolValue = true; debug.FindProperty("_debugLevel").intValue = 101;
                debug.FindProperty("_infiniteLives").boolValue = false; debug.FindProperty("_infiniteTips").boolValue = false;
                debug.FindProperty("_gameSpeedMultiplier").floatValue = 1; debug.ApplyModifiedPropertiesWithoutUndo();
                Application.runInBackground = true; Time.timeScale = 1; PuzzleLaunch.Preview = false;
                MainMenuUI.OpenLevelsOnLoad = false; UnityEngine.Random.InitState(426101);
            }

            /// <summary>Restores original inventory/preferences before re-enabling the user's save policy.</summary>
            public void Dispose()
            {
                if (_disposed) return; _disposed = true;
                if (_ads != null) _ads.SetAdService(_provider);
                bool assetsMatch = true;
                foreach (var item in _assets) if (item.Key == null || EditorJsonUtility.ToJson(item.Key) != item.Value) assetsMatch = false;
                if (_debug != null)
                {
                    JsonUtility.FromJsonOverwrite(_debugJson, _debug);
                    if (_createdDebug) UnityEngine.Object.Destroy(_debug.gameObject);
                }
                PuzzleLaunch.Preview = _preview; PuzzleLaunch.PreviewMode = _previewMode; PuzzleLaunch.PreviewLevel = _previewLevel;
                MainMenuUI.OpenLevelsOnLoad = _levelsOnLoad;
                Time.timeScale = _time; Application.runInBackground = _background; UnityEngine.Random.state = _random;
                _data.Dispose();
                Restored = assetsMatch && _data.IsRestored() && (_ads == null || (!_ads.IsShowingAd &&
                    ReferenceEquals(Field<IAdService>(_ads, "_adService"), _provider)));
            }
        }

        private static T Field<T>(object owner, string field) =>
            (T)owner.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
    }
}
