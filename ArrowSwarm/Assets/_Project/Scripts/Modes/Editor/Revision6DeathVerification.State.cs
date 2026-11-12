namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using ArrowSwarm.Ads;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using DebugManager = ArrowSwarm.Debug.DebugManager;

    /// <summary>Contains all mutable verification state and restores the caller's exact player/provider ownership.</summary>
    public static partial class Revision6DeathVerification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

        /// <summary>Protects data, debug/config, RNG, original ad service and pooled particle seeds.</summary>
        private sealed class Scope : IDisposable
        {
            internal readonly Dictionary<ParticleSystem, Seed> Seeds = new Dictionary<ParticleSystem, Seed>();
            private readonly QualityDataSnapshot _data;
            private readonly DebugManager _debug;
            private readonly bool _createdDebug, _background;
            private readonly string _debugJson, _configJson, _originalJson;
            private readonly float _time;
            private readonly UnityEngine.Random.State _random;
            private readonly AdManager _ads;
            private readonly IAdService _provider;
            private readonly NoAds _fake = new NoAds();
            private readonly bool _preview;
            private readonly GameModeId _previewMode;
            private readonly int _previewLevel;
            private bool _disposed;

            internal Scope()
            {
                if (!Application.isPlaying || SceneManager.GetActiveScene().name != "MainMenuScene" ||
                    !DataManager.HasInstance || !GameManager.HasInstance || LeaderboardManager.HasInstance ||
                    GameManager.Instance.CurrentState != GameState.Menu)
                    throw new InvalidOperationException("Idle Boot-loaded MainMenu without an existing leaderboard is required.");
                if (AdManager.HasInstance && AdManager.Instance.IsShowingAd)
                    throw new InvalidOperationException("An ad is already pending.");
                _originalJson = JsonUtility.ToJson(DataManager.Instance.PlayerData);
                _configJson = JsonUtility.ToJson(GameManager.Instance.Config);
                _data = new QualityDataSnapshot(DataManager.Instance);
                var clone = JsonUtility.FromJson<PlayerData>(_originalJson);
                clone.isTutorialCompleted = clone.isProfileSetupCompleted = clone.vfxEnabled = true;
                clone.selectedGameMode = GameModeId.Defense; _data.SetData(clone);
                _createdDebug = !DebugManager.HasInstance; _debug = DebugManager.Instance;
                _debugJson = JsonUtility.ToJson(_debug);
                var debug = new SerializedObject(_debug);
                debug.FindProperty("_useDebugLevel").boolValue = true; debug.FindProperty("_debugLevel").intValue = 101;
                debug.FindProperty("_infiniteLives").boolValue = true;
                debug.FindProperty("_gameSpeedMultiplier").floatValue = 1; debug.ApplyModifiedPropertiesWithoutUndo();
                _time = Time.timeScale; _background = Application.runInBackground; _random = UnityEngine.Random.state;
                _preview = PuzzleLaunch.Preview; _previewMode = PuzzleLaunch.PreviewMode; _previewLevel = PuzzleLaunch.PreviewLevel;
                if (AdManager.HasInstance)
                {
                    _ads = AdManager.Instance;
                    _provider = (IAdService)typeof(AdManager).GetField("_adService", Private).GetValue(_ads);
                    _ads.SetAdService(_fake);
                }
                Time.timeScale = 1; Application.runInBackground = true; PuzzleLaunch.Preview = false;
                UnityEngine.Random.InitState(620271);
            }

            internal void Remember(ParticleSystem particle)
            { if (!Seeds.ContainsKey(particle)) Seeds.Add(particle, new Seed { Auto = particle.useAutoRandomSeed, Value = particle.randomSeed }); }

            /// <summary>Releases only fixture-owned mutations and returns to the original home scene.</summary>
            public void Dispose()
            {
                if (_disposed) return; _disposed = true;
                foreach (var item in Seeds)
                    if (item.Key != null)
                    {
                        item.Key.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                        item.Key.randomSeed = item.Value.Value; item.Key.useAutoRandomSeed = item.Value.Auto;
                    }
                if (Application.isPlaying && SceneManager.GetActiveScene().name != "MainMenuScene")
                {
                    if (LevelManager.HasInstance) LevelManager.Instance.CleanupLevel();
                    GameManager.Instance.SetState(GameState.Menu); SceneManager.LoadScene("MainMenuScene");
                }
                _data.Dispose(); _data.VerifyRestored(Results);
                if (_debug != null) { JsonUtility.FromJsonOverwrite(_debugJson, _debug); if (_createdDebug) UnityEngine.Object.DestroyImmediate(_debug.gameObject); }
                if (_ads != null) _ads.SetAdService(_provider);
                PuzzleLaunch.Preview = _preview; PuzzleLaunch.PreviewMode = _previewMode; PuzzleLaunch.PreviewLevel = _previewLevel;
                ThemeManager.Instance.ApplyCurrentTheme(); Time.timeScale = _time;
                Application.runInBackground = _background; UnityEngine.Random.state = _random;
                Check(JsonUtility.ToJson(DataManager.Instance.PlayerData) == _originalJson, "Exact original player object contents restored");
                Check(JsonUtility.ToJson(GameManager.Instance.Config) == _configJson, "GameConfig tuning asset unchanged");
                Check(!LeaderboardManager.HasInstance && _fake.Calls == 0, "No leaderboard was created and no ad request occurred");
                Check(_ads == null || ReferenceEquals(typeof(AdManager).GetField("_adService", Private).GetValue(_ads), _provider),
                    "Exact original ad provider restored");
            }
        }

        /// <summary>Original per-instance random configuration restored after deterministic native captures.</summary>
        internal struct Seed { internal bool Auto; internal uint Value; }

        /// <summary>Intercepts accidental fullscreen requests without contacting a network.</summary>
        private sealed class NoAds : IAdService
        {
            internal int Calls;
            /// <inheritdoc/>
            public bool IsAdReady() => false;
            /// <inheritdoc/>
            public bool IsInterstitialAdReady() => false;
            /// <inheritdoc/>
            public void ShowRewardedAd(Action<bool> callback) { Calls++; callback(false); }
            /// <inheritdoc/>
            public void ShowInterstitialAd(Action callback) { Calls++; callback(); }
        }
    }
}
