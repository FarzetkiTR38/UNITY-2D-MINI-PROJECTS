namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using ArrowSwarm.Ads;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Effects;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using DebugManager = ArrowSwarm.Debug.DebugManager;

    /// <summary>Verifies the real confetti prefab and its native ParticleManager pool without ads, saves or backend calls.</summary>
    public static class Quality4ConfettiVerification
    {
        private const string PrefabPath = "Assets/_Project/Prefabs/Effects/ConfettiParticle.prefab";
        private static readonly List<string> Results = new List<string>();
        private static QualityUIVerificationState _data;
        private static AdManager _ads;
        private static IAdService _provider;
        private static NoAds _fake;
        private static DebugManager _debug;
        private static bool _debugEnabled, _background, _running;
        private static float _time;
        private static UnityEngine.Random.State _random;
        private static ParticleSystem _first;
        private static IEnumerator _run;
        private static Func<bool> _condition;
        private static double _deadline;
        private static string _report, _source;

        /// <summary>Current native-player-loop progress or completed outcome.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Starts from a freshly booted MainMenu before any victory has populated the confetti pool.</summary>
        public static void Begin(string reportName = "confetti-verification.txt")
        {
            if (_running) throw new InvalidOperationException("Confetti verification is already running.");
            if (!Application.isPlaying || SceneManager.GetActiveScene().name != "MainMenuScene" ||
                !DataManager.HasInstance || !GameManager.HasInstance || GameManager.Instance.CurrentState != GameState.Menu)
                throw new InvalidOperationException("A freshly booted playing MainMenu is required.");
            if (Path.GetFileName(reportName) != reportName) throw new ArgumentException("Supply a report filename only.");
            _ads = AdManager.Instance;
            if (_ads.IsShowingAd) throw new InvalidOperationException("An existing ad is pending.");
            if (ConfettiClones(ParticleManager.Instance).Count > 0)
                throw new InvalidOperationException("Restart Play Mode: a previous confetti clone already exists.");
            _data = new QualityUIVerificationState();
            _provider = (IAdService)typeof(AdManager).GetField("_adService", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_ads);
            _ads.SetAdService(_fake = new NoAds());
            _time = Time.timeScale; _background = Application.runInBackground; _random = UnityEngine.Random.state;
            _debug = DebugManager.HasInstance ? DebugManager.Instance : null;
            if (_debug != null) { _debugEnabled = _debug.enabled; _debug.enabled = false; }
            Time.timeScale = 1; Application.runInBackground = true; UnityEngine.Random.InitState(426707);
            DataManager.Instance.PlayerData.vfxEnabled = true;
            _source = File.ReadAllText(PrefabPath); _report = Path.Combine("Captures/QualityPass4", reportName);
            Results.Clear(); _first = null; _condition = null; _run = Run(); _running = true;
            _deadline = EditorApplication.timeSinceStartup + 60; Status = "Verifying native confetti prefab and pool";
            EditorApplication.update += Tick; EditorApplication.playModeStateChanged += ChangedPlayMode;
            AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
        }

        /// <summary>Stops verification and restores original preferences, data, provider and timing.</summary>
        public static void Cancel() { if (_running) Finish("ABORT: cancelled"); }

        private static IEnumerator Run()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Check(prefab != null && Missing(prefab) == 0, "Loaded authored prefab and its full hierarchy have zero missing scripts");
            var source = prefab.GetComponent<ParticleSystem>(); var main = source.main;
            Check(prefab.GetComponentsInChildren<ParticleSystem>(true).Length == 1 &&
                prefab.GetComponentsInChildren<ParticleSystemRenderer>(true).Length == 1 &&
                main.duration == 5 && main.startLifetime.constant == 2 && main.stopAction == ParticleSystemStopAction.None,
                "Original one-system/one-renderer particle design, five-second duration, two-second lifetime and pooling-compatible stop action preserved");
            var manager = ParticleManager.Instance; manager.SpawnConfetti(Vector3.zero);
            var clones = ConfettiClones(manager);
            Check(clones.Count == 1, "Actual ParticleManager.SpawnConfetti creates one native pooled confetti effect");
            _first = clones[0];
            Check(Missing(_first.gameObject) == 0 && _first.gameObject.activeInHierarchy && _first.isPlaying,
                "Actual native clone has zero missing scripts and starts playing");
            yield return (Func<bool>)(() => _first.particleCount > 0);
            Check(_first.IsAlive(true), "First native player-loop emission produces live particles");
            Status = "Waiting for actual first confetti pool return";
            yield return (Func<bool>)(() => !_first.gameObject.activeSelf);
            Check(!_first.IsAlive(true) && _first.transform.parent == manager.transform,
                "Existing timed ParticleManager coroutine clears and deactivates the completed effect under its pool owner");
            manager.SpawnConfetti(Vector3.zero); clones = ConfettiClones(manager);
            Check(clones.Count == 1 && ReferenceEquals(clones[0], _first) && _first.gameObject.activeInHierarchy,
                "Second real SpawnConfetti reuses exactly the same native instance instead of allocating another clone");
            yield return (Func<bool>)(() => _first.particleCount > 0);
            Check(_first.isPlaying && _first.IsAlive(true) && Missing(_first.gameObject) == 0,
                "Reused native system emits living particles with zero missing scripts");
            Status = "Waiting for reused confetti pool return";
            yield return (Func<bool>)(() => !_first.gameObject.activeSelf);
            Check(!_first.IsAlive(true), "Reused effect returns cleared to the same existing pool");
            Check(File.ReadAllText(PrefabPath) == _source, "Runtime verification does not rewrite authored particle settings");
        }

        private static int Missing(GameObject root)
        {
            int count = 0;
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
                count += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject);
            return count;
        }

        private static List<ParticleSystem> ConfettiClones(ParticleManager manager)
        {
            var clones = new List<ParticleSystem>();
            foreach (var particle in manager.GetComponentsInChildren<ParticleSystem>(true))
                if (particle.name.StartsWith("ConfettiParticle", StringComparison.Ordinal)) clones.Add(particle);
            return clones;
        }

        private static void Tick()
        {
            if (!_running) return;
            try
            {
                if (!Application.isPlaying) { Finish("ABORT: Play Mode ended"); return; }
                if (EditorApplication.timeSinceStartup > _deadline) throw new TimeoutException(Status);
                if (_condition != null) { if (!_condition()) return; _condition = null; }
                if (!_run.MoveNext()) { Finish("PASS"); return; }
                _condition = _run.Current as Func<bool>;
            }
            catch (Exception error) { Results.Add(error.ToString()); Finish("FAIL: " + error.Message); }
        }

        private static void ChangedPlayMode(PlayModeStateChange state)
        { if (_running && state == PlayModeStateChange.ExitingPlayMode) Finish("ABORT: Play Mode exiting"); }
        private static void BeforeReload() { if (_running) Finish("ABORT: assembly reload"); }

        private static void Finish(string outcome)
        {
            _running = false; EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= ChangedPlayMode;
            AssemblyReloadEvents.beforeAssemblyReload -= BeforeReload;
            try
            {
                (_run as IDisposable)?.Dispose();
                if (_first != null && _first.gameObject.activeSelf)
                { _first.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); _first.gameObject.SetActive(false); }
                if (_ads != null) _ads.SetAdService(_provider);
                if (_debug != null) _debug.enabled = _debugEnabled;
                Time.timeScale = _time; Application.runInBackground = _background; UnityEngine.Random.state = _random;
                _data.Dispose(); Check(_data.IsRestored(), "Original player/VFX/inventory/preferences/save policy restored");
                Check(_fake.Calls == 0, "No ad provider request occurred during real particle and pool verification");
                Check(_ads == null || (!_ads.IsShowingAd && ReferenceEquals(typeof(AdManager).GetField("_adService",
                    BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_ads), _provider)), "Original ad provider restored with no requests pending");
            }
            catch (Exception error) { outcome += " / RESTORE: " + error.Message; }
            Status = outcome; _condition = null; _run = null; _data = null;
            Directory.CreateDirectory(Path.GetDirectoryName(_report));
            File.WriteAllText(_report, outcome + "\n" + string.Join("\n", Results) + "\n");
            Debug.Log("[ArrowSwarm] Confetti verification " + outcome + " (" + _report + ")");
        }

        private static void Check(bool condition, string label)
        { if (!condition) throw new InvalidOperationException(label); Results.Add("PASS: " + label); }

        /// <summary>Prevents accidental ad-network use throughout the isolated visual-effect check.</summary>
        private sealed class NoAds : IAdService
        {
            /// <summary>Number of intercepted accidental fullscreen requests.</summary>
            public int Calls { get; private set; }
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
