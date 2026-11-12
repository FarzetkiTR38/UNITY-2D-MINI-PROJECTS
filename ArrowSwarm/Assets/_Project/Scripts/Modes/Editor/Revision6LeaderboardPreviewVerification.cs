namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.UI;
    using ArrowSwarm.UI.Editor;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Checks the discoverable Editor sample preview through real authored rows and reversible lifecycle events.</summary>
    public static partial class Revision6LeaderboardPreviewVerification
    {
        private const string Folder = "Captures/Revision6/Leaderboard";
        private static readonly List<string> Results = new List<string>();
        private static readonly Stack<IEnumerator> Nested = new Stack<IEnumerator>();
        private static IEnumerator _steps;
        private static Func<bool> _wait;
        private static Snapshot _snapshot;
        private static double _deadline;

        /// <summary>Current native fixture stage or final result.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Runs from a fresh Boot-loaded main menu, before opening the real leaderboard.</summary>
        public static void Begin()
        {
            if (_steps != null || LeaderboardSamplePreview.IsActive) throw new InvalidOperationException("A preview check is already running.");
            if (!Application.isPlaying || LeaderboardManager.HasInstance) throw new InvalidOperationException("Fresh MainMenu Play without a leaderboard manager is required.");
            _snapshot = new Snapshot(); Results.Clear(); Directory.CreateDirectory(Folder);
            Application.runInBackground = true; _steps = Run(); _deadline = EditorApplication.timeSinceStartup + 90;
            Status = "Running sample preview checks"; EditorApplication.update += Tick;
            AssemblyReloadEvents.beforeAssemblyReload += Cancel; EditorApplication.playModeStateChanged += ModeChanged;
        }

        /// <summary>Restores the preview and reports a cancelled check when compilation or Play exit interrupts the runner.</summary>
        public static void Cancel() { if (_steps != null) Finish("ABORT: sample preview verification interrupted"); }

        private static IEnumerator Run()
        {
            LeaderboardSamplePreview.Begin(true); yield return Settled(); Verify(true);
            _snapshot.VerifyPreferences("opening samples does not write any of the five original preferences");
            LeaderboardSamplePreview.SetTheme(ThemeMode.Light); yield return Settled();
            yield return Capture("sample-ten-light");
            LeaderboardSamplePreview.SetTheme(ThemeMode.Dark); yield return Settled();
            yield return Capture("sample-ten-dark");
            var board = UnityEngine.Object.FindFirstObjectByType<LeaderboardUI>();
            Field<Button>(board, "_retryButton").onClick.Invoke(); yield return Settled(); Verify(true);
            LocalizationManager.Instance.SetLanguage(_snapshot.Language == "tr" ? "en" : "tr", false);
            yield return Settled(); Verify(true);
            _snapshot.VerifyPreferences("theme, language and actual Retry keep sample rows out of persistent data/cache");
            LeaderboardSamplePreview.SetScenario(false); yield return Settled(); Verify(false);
            yield return Capture("sample-outside-top-ten-dark");
            var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>(); menu.BackToMain();
            yield return (Func<bool>)(() => !LeaderboardSamplePreview.IsActive);
            _snapshot.VerifyRestored("closing the actual leaderboard ends sample ownership");
            LeaderboardSamplePreview.Begin(); yield return Settled();
            var existing = Resources.FindObjectsOfTypeAll<LeaderboardSamplePreviewWindow>();
            Check(existing.Length == 0, "native window-close test owns a newly created preview window");
            var window = EditorWindow.GetWindow<LeaderboardSamplePreviewWindow>("Leaderboard Samples");
            window.Close(); yield return Settled();
            Check(!LeaderboardSamplePreview.IsActive, "closing the actual Editor window ends the sample session");
            _snapshot.VerifyRestored("window close restores original state");
        }

        private static void Verify(bool currentOnBoard)
        {
            Check(LeaderboardSamplePreview.IsActive, "explicit sample session is active");
            var manager = LeaderboardManager.Instance; var rows = manager.GetRankedPlayers();
            Check(rows.Count == 10 && rows.TrueForAll(x => x.PlayerId.StartsWith("editor-sample-")), "ten visibly sample-owned rows stay entirely in memory");
            Check(rows.FindAll(x => x.IsPlayer).Count == (currentOnBoard ? 1 : 0) &&
                manager.GetVerifiedPlayerRank() == (currentOnBoard ? 7 : 27), "sample local rank follows inside-seven / outside-twenty-seven scenario");
            var board = UnityEngine.Object.FindFirstObjectByType<LeaderboardUI>(); int visible = 0;
            foreach (var row in Field<LeaderboardEntryUI[]>(board, "_entryRows"))
                if (row != null && row.gameObject.activeInHierarchy) visible++;
            Check(visible == 10, "real authored panel displays all ten rows");
            Check(Field<TMP_Text>(board, "_statusText").text.Contains("SAMPLE") &&
                Field<TMP_Text>(board, "_updatedText").text.Contains("kaydedilmez"), "actual TMP status explicitly identifies fictional unsynchronized preview data");
            Check(Field<TMP_Text>(board, "_playerSummaryText").text.Contains(currentOnBoard ? "#7" : "#27"), "actual local footer displays the selected example rank");
            foreach (var text in board.GetComponentsInChildren<TMP_Text>())
            {
                if (!text.enabled || text.GetComponentInParent<Casual3DTitle>() != null) continue;
                text.ForceMeshUpdate();
                if (text.name == "RankText") VerifyRankBadge(text);
                else Check(!text.isTextOverflowing, "visible authored text fits: " + text.name);
            }
        }

        private static Func<bool> Settled()
        { float until = Time.unscaledTime + .4f; int frame = Time.frameCount; return () => Time.unscaledTime > until && Time.frameCount > frame + 2; }

        private static IEnumerator Capture(string label)
        {
            string path = Folder + "/" + label + ".png"; int frame = Time.frameCount;
            if (File.Exists(path)) File.Delete(path);
            ScreenCapture.CaptureScreenshot(path);
            yield return (Func<bool>)(() => Time.frameCount > frame + 2 && File.Exists(path) && new FileInfo(path).Length > 0);
            Results.Add("CAPTURE: " + path);
        }

        private static void Tick()
        {
            try
            {
                if (!Application.isPlaying || EditorApplication.timeSinceStartup > _deadline) throw new TimeoutException(Status);
                EditorApplication.QueuePlayerLoopUpdate();
                if (_wait != null) { if (!_wait()) return; _wait = null; }
                if (Nested.Count == 0) Nested.Push(_steps);
                while (Nested.Count > 0)
                {
                    var current = Nested.Peek();
                    if (!current.MoveNext()) { (current as IDisposable)?.Dispose(); Nested.Pop(); continue; }
                    if (current.Current is IEnumerator child) { Nested.Push(child); continue; }
                    _wait = current.Current as Func<bool>; return;
                }
                Finish("PASS: real sample leaderboard and lifecycle preservation");
            }
            catch (Exception error) { Finish("FAIL: " + error.Message); }
        }

        private static void ModeChanged(PlayModeStateChange state)
        { if (state == PlayModeStateChange.ExitingPlayMode) Cancel(); }

        private static void Finish(string result)
        {
            EditorApplication.update -= Tick; AssemblyReloadEvents.beforeAssemblyReload -= Cancel;
            EditorApplication.playModeStateChanged -= ModeChanged;
            try { LeaderboardSamplePreview.End(); _snapshot?.VerifyRestored("final cleanup"); }
            catch (Exception error) { result += " / RESTORE FAILED: " + error.Message; }
            if (_snapshot != null) Application.runInBackground = _snapshot.Background;
            while (Nested.Count > 0) (Nested.Pop() as IDisposable)?.Dispose();
            (_steps as IDisposable)?.Dispose(); _steps = null; _wait = null; _snapshot = null;
            Status = result; Directory.CreateDirectory(Folder);
            Results.Insert(0, result); File.WriteAllLines(Folder + "/preview-verification.txt", Results);
        }

        private static void Check(bool value, string label)
        { Results.Add((value ? "PASS: " : "FAIL: ") + label); if (!value) throw new InvalidOperationException(label); }
    }
}
