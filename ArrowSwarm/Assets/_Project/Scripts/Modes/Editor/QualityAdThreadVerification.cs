namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using ArrowSwarm.Ads;
    using GoogleMobileAds.Common;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Posts genuine background-thread callbacks through the installed executor without SDK initialization.</summary>
    public static partial class QualityAdThreadVerification
    {
        private static readonly List<string> Results = new List<string>();
        private static readonly List<int> Order = new List<int>();
        private static bool _running, _background, _allOnMain;
        private static int _mainThread, _stage, _staticCalls, _discarded, _liveOwner, _rewards;
        private static double _deadline;
        private static Thread _worker;
        private static Exception _workerFailure;
        private static AdManager _manager;
        private static IAdService _original;
        private static ThreadTestAds _fake;
        private static object _pacing;
        private static GameObject _owner;

        /// <summary>Current asynchronous check progress or the final result.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Runs real queued/main-thread ordering and owner lifetime checks in Play Mode with no native ad calls.</summary>
        public static void Begin()
        {
            if (_running || !Application.isPlaying || !AdManager.HasInstance)
                throw new InvalidOperationException("Idle Play Mode managers required.");
            _manager = AdManager.Instance;
            if (_manager.IsShowingAd) throw new InvalidOperationException("An ad is already pending.");
            _original = (IAdService)Field("_adService").GetValue(_manager);
            _pacing = Field("_lastInterstitialTime").GetValue(_manager);
            _background = Application.runInBackground; Application.runInBackground = true;
            _fake = new ThreadTestAds(); _manager.SetAdService(_fake);
            Results.Clear(); Order.Clear(); _stage = _staticCalls = _discarded = _liveOwner = _rewards = 0;
            _running = _allOnMain = true; _mainThread = Thread.CurrentThread.ManagedThreadId;
            _deadline = EditorApplication.timeSinceStartup + 20;
            EditorApplication.update += Tick; EditorApplication.playModeStateChanged += ModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
            try
            {
                AdUnityThread.Initialize();
                Check(AdUnityThread.IsMainThread && MobileAdsEventExecutor.IsActive(), "SDK executor initialized on Unity before any native initialization");
                CheckInlineDispatch(); StartFifoBatch();
                Status = "Waiting for real background-thread FIFO callbacks";
            }
            catch (Exception error) { Finish("FAIL: " + error.Message); throw; }
        }

        /// <summary>Cancels the fixture and restores its original provider and background setting.</summary>
        public static void Cancel() { if (_running) Finish("ABORT: cancelled"); }

        private static void StartFifoBatch()
        {
            _owner = new GameObject("QualityAdThreadOwner"); var owner = _owner;
            PostFromWorker(() =>
            {
                for (int i = 0; i < 24; i++)
                {
                    int index = i;
                    AdUnityThread.Run(() => { if (_running) { ObserveThread(); Order.Add(index); } });
                }
                AdUnityThread.Run(StaticPosted);
                AdUnityThread.Run(owner, () => { if (_running) _liveOwner++; },
                    () => { if (_running) { ObserveThread(); _discarded++; } });
            });
            // No Unity Update can run until this main-thread call returns, so destruction precedes delivery.
            UnityEngine.Object.DestroyImmediate(_owner); _owner = null;
        }

        private static void PostFromWorker(Action post)
        {
            _workerFailure = null;
            _worker = new Thread(() => { try { post(); } catch (Exception error) { _workerFailure = error; } })
                { IsBackground = true, Name = "ArrowSwarm Ad Callback Verification" };
            _worker.Start();
            if (!_worker.Join(1000)) throw new InvalidOperationException("Background posting exceeded one second.");
            if (_workerFailure != null) throw new InvalidOperationException("Background posting failed.", _workerFailure);
        }

        private static void Tick()
        {
            if (!_running) return;
            try
            {
                if (!Application.isPlaying) { Finish("ABORT: Play Mode ended"); return; }
                if (EditorApplication.timeSinceStartup > _deadline) throw new InvalidOperationException("Timeout: " + Status);
                if (_stage == 0)
                {
                    if (Order.Count < 24 || _staticCalls < 1 || _discarded < 1) return;
                    CheckFifoBatch(); StartMixedOrigin(); _stage = 1; return;
                }
                if (_stage == 1)
                {
                    if (_rewards < 1) return;
                    CheckMixedOrigin(); StartMainThenWorker(); _stage = 2; return;
                }
                if (_rewards < 2) return;
                CheckMainThenWorker(); CheckCleanupFailures();
                Check(ReferenceEquals(Field("_adService").GetValue(_manager), _fake), "Only the deterministic provider was used; no SDK initialization or ad load invoked by fixture");
                Finish("PASS: background callback dispatch, mixed-origin ordering, lifetime and cleanup completion");
            }
            catch (Exception error) { Finish("FAIL: " + error.Message); Debug.LogException(error); }
        }

        private static void ObserveThread() => _allOnMain &= Thread.CurrentThread.ManagedThreadId == _mainThread;
        private static void StaticPosted() { if (_running) { ObserveThread(); _staticCalls++; } }
        private static FieldInfo Field(string name) => typeof(AdManager).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        private static void ModeChanged(PlayModeStateChange change) { if (_running && change == PlayModeStateChange.ExitingPlayMode) Finish("ABORT: Play Mode exiting"); }
        private static void BeforeReload() { if (_running) Finish("ABORT: assembly reload"); }

        private static void Finish(string outcome)
        {
            _running = false; EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= ModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= BeforeReload;
            if (_owner != null) UnityEngine.Object.DestroyImmediate(_owner);
            if (_manager != null)
            {
                _manager.SetAdService(_original); Field("_lastInterstitialTime").SetValue(_manager, _pacing);
                Check(ReferenceEquals(Field("_adService").GetValue(_manager), _original) && !_manager.IsShowingAd,
                    "Original provider restored with no fullscreen request pending");
            }
            Application.runInBackground = _background;
            string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Captures/QualityPass"));
            Directory.CreateDirectory(directory); var lines = new List<string> { outcome }; lines.AddRange(Results);
            File.WriteAllLines(Path.Combine(directory, "ad-thread-verification.txt"), lines);
            Status = outcome + " / " + Results.Count + " assertions";
            Debug.Log("[ArrowSwarm] " + Status);
        }

        private static void Check(bool condition, string label)
        { Results.Add((condition ? "PASS: " : "FAIL: ") + label); if (!condition) throw new InvalidOperationException(label); }

        /// <summary>Retains callbacks for real-thread delivery without invoking any SDK API.</summary>
        private sealed class ThreadTestAds : IAdService
        {
            /// <summary>Current reward completion.</summary>
            public Action<bool> Reward { get; private set; }
            /// <summary>Current interstitial completion.</summary>
            public Action Closed { get; private set; }
            /// <inheritdoc/>
            public bool IsAdReady() => true;
            /// <inheritdoc/>
            public bool IsInterstitialAdReady() => true;
            /// <inheritdoc/>
            public void ShowRewardedAd(Action<bool> callback) => Reward = callback;
            /// <inheritdoc/>
            public void ShowInterstitialAd(Action callback) => Closed = callback;
        }
    }
}
