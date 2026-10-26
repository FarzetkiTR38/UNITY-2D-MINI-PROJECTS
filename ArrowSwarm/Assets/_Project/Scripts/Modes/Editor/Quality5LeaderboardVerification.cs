namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Exercises the authored leaderboard through real menu and Retry callbacks with an isolated provider.</summary>
    public static partial class Quality5LeaderboardVerification
    {
        private const string Output = "Captures/QualityPass5/Leaderboard";
        private static readonly List<string> Results = new List<string>();
        private static IEnumerator _steps;
        private static Func<bool> _wait;
        private static double _deadline;
        private static int _failures;

        /// <summary>Current state for a nonblocking Unity MCP caller.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Starts once from the original Boot-loaded main menu before the lazy leaderboard is opened.</summary>
        public static void Begin()
        {
            if (_steps != null) throw new InvalidOperationException("Leaderboard verification is already running.");
            Results.Clear(); _failures = 0; Directory.CreateDirectory(Output);
            _deadline = EditorApplication.timeSinceStartup + 240;
            _steps = Run(); Status = "Running isolated leaderboard states";
            EditorApplication.update += Tick; AssemblyReloadEvents.beforeAssemblyReload += Cancel;
            EditorApplication.playModeStateChanged += ModeChanged;
        }

        /// <summary>Cancels the fixture and restores its data, service, language and presentation ownership.</summary>
        public static void Cancel() { if (_steps != null) Finish("ABORT: leaderboard verification interrupted"); }

        private static IEnumerator Run()
        {
            using (var scope = new Scope())
            {
                yield return OfflineAndLoading(scope);
                yield return ResultsAndFailures(scope);
                yield return CachedAndEmpty(scope);
                yield return LanguagesAndThemes(scope);
                Check(scope.Cloud.Submissions == 0 && scope.Cloud.NameUpdates == 0,
                    "all fixture views and Retry actions performed zero score/name writes and zero real network operations");
            }
        }

        private static void Tick()
        {
            try
            {
                if (!Application.isPlaying || EditorApplication.timeSinceStartup > _deadline)
                    throw new TimeoutException("Timeout or Play Mode ended: " + Status);
                EditorApplication.QueuePlayerLoopUpdate();
                if (_wait != null) { if (!_wait()) return; _wait = null; }
                if (!MoveNext()) Finish((_failures == 0 ? "PASS" : "FAIL") + ": leaderboard states, failures=" + _failures);
            }
            catch (Exception error) { Check(false, error.GetType().Name + ": " + error.Message); Finish("FAIL: " + error.Message); }
        }

        private static readonly Stack<IEnumerator> Nested = new Stack<IEnumerator>();
        private static bool MoveNext()
        {
            if (Nested.Count == 0) Nested.Push(_steps);
            while (Nested.Count > 0)
            {
                var current = Nested.Peek();
                if (!current.MoveNext()) { (current as IDisposable)?.Dispose(); Nested.Pop(); continue; }
                if (current.Current is IEnumerator child) { Nested.Push(child); continue; }
                _wait = current.Current as Func<bool>; return true;
            }
            return false;
        }

        private static void ModeChanged(PlayModeStateChange state)
        { if (state == PlayModeStateChange.ExitingPlayMode) Cancel(); }

        private static void Finish(string result)
        {
            EditorApplication.update -= Tick; AssemblyReloadEvents.beforeAssemblyReload -= Cancel;
            EditorApplication.playModeStateChanged -= ModeChanged;
            try
            {
                while (Nested.Count > 0) (Nested.Pop() as IDisposable)?.Dispose();
                (_steps as IDisposable)?.Dispose();
            }
            catch (Exception error) { Check(false, "RESTORE FAILED: " + error.Message); result = "FAIL: restoration"; }
            _steps = null; _wait = null; Status = result;
            Results.Insert(0, result); File.WriteAllLines(Output + "/verification.txt", Results);
        }

        private static void Check(bool value, string message)
        { Results.Add((value ? "PASS: " : "FAIL: ") + message); if (!value) _failures++; }

        private static Func<bool> Settled(float seconds = .35f)
        { float end = Time.unscaledTime + seconds; int frame = Time.frameCount; return () => Time.unscaledTime > end && Time.frameCount > frame + 2; }

        private static IEnumerator Capture(string name)
        {
            string path = Output + "/" + name + ".png"; int frame = Time.frameCount;
            if (File.Exists(path)) File.Delete(path);
            ScreenCapture.CaptureScreenshot(path);
            yield return (Func<bool>)(() => Time.frameCount > frame + 2 && File.Exists(path) && new FileInfo(path).Length > 0);
            Results.Add("CAPTURE: " + path);
        }
    }
}
