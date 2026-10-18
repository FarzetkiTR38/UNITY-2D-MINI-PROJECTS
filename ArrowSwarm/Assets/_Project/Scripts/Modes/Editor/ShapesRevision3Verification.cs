namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Data;
    using ArrowSwarm.Puzzle;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Captures and plays all redesigned Shapes through real rapid taps without saving progress.</summary>
    public static partial class ShapesRevision3Verification
    {
        private static string Folder = "Captures/Revision3/Playthroughs";
        private static readonly List<string> Results = new List<string>();
        private static readonly HashSet<string> Signatures = new HashSet<string>();
        private static bool _running, _initializing;
        private static int _level, _stage, _step, _captureFrame, _peakShots, _nextActionFrame;
        private static double _deadline, _settleUntil;
        private static PuzzleSolution _solution;
        private static MetricRow _metrics;
        private static string _captureRun;
        /// <summary>Current level/action or final verification result for MCP polling.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Starts a Preview-only fifty-level capture and rapid normal-arrow playthrough.</summary>
        [MenuItem("Arrow Swarm/Puzzles/Revision3 Capture And Verify Shapes (Play Mode)")]
        public static void Begin() => BeginAt("Captures/Revision3/Playthroughs");

        /// <summary>Verifies the balanced catalog without overwriting the previous visual review.</summary>
        [MenuItem("Arrow Swarm/Puzzles/Revision4 Capture And Verify Shapes (Play Mode)")]
        public static void BeginRevision4() => BeginAt("Captures/Revision4/Playthroughs");

        private static void BeginAt(string folder)
        {
            if (_running || !Application.isPlaying) throw new InvalidOperationException("Enter Play Mode without another running verification.");
            Folder = folder;
            Snapshot(); _running = true; _initializing = true; _level = 1; _stage = 0;
            Results.Clear(); Rows.Clear(); Signatures.Clear(); Directory.CreateDirectory(Folder);
            _captureRun = "run-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            Directory.CreateDirectory(Folder + "/" + _captureRun);
            EditorApplication.update += Tick; EditorApplication.playModeStateChanged += PlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
            _deadline = EditorApplication.timeSinceStartup + 180;
            Status = "Loading Shapes Preview";
            PuzzleLaunch.Preview = true; PuzzleLaunch.PreviewMode = GameModeId.Shapes; PuzzleLaunch.PreviewLevel = 1;
            try { SceneManager.LoadScene("ShapesScene"); }
            catch (Exception error) { Finish("FAIL: " + error.Message); throw; }
        }

        /// <summary>Stops verification and restores player data, theme, config and runtime settings.</summary>
        [MenuItem("Arrow Swarm/Puzzles/Cancel Revision3 Verification")]
        public static void Cancel() { if (_running) Finish("ABORT: cancelled after " + Results.Count + " levels"); }

        private static void Tick()
        {
            if (!_running) return;
            try
            {
                Require(Application.isPlaying, "Play Mode ended unexpectedly");
                double now = EditorApplication.timeSinceStartup;
                Require(now <= _deadline, "timeout: " + Status);
                var session = PuzzleSession.Active;
                if (session == null || session.Level == null || session.Mode != GameModeId.Shapes) return;
                Require(session.Preview, "capture session must never award progress");
                if (_initializing)
                {
                    _initializing = false; Accelerate(session);
                    Require(Field<PuzzleCatalog>(session, "_catalog").Count == 50, "expected fifty Shapes");
                }
                Require(!session.Lost, "solution witness lost a life");
                if (session.Won) { CompleteLevel(session, now); return; }
                if (_stage < 5) { CaptureInitial(session, now); return; }
                if (Time.frameCount < _nextActionFrame || !session.Ready) return;
                if (_step >= _solution.Steps.Count)
                {
                    Require(session.HasActiveShots, "witness exhausted before terminal state"); return;
                }
                // Real taps exercise touch hit testing while previously launched arrows remain in flight.
                for (int batch = 0; batch < 4 && _step < _solution.Steps.Count; batch++)
                {
                    int action = _solution.Steps[_step++]; Require(action >= 0, "Shapes witness requested a rotation");
                    var p = session.Level.Arrows[action].Head;
                    var world = new Vector2(p.x - (session.Level.Width - 1) * .5f, p.y - (session.Level.Height - 1) * .5f);
                    int before = session.Remaining; session.Tap(world);
                    Require(session.Remaining == before - 1, "tap did not immediately accept witness arrow " + action);
                    _peakShots = Mathf.Max(_peakShots, ActiveShotCount(session));
                }
                _nextActionFrame = Time.frameCount + 1;
                Status = "Shapes " + _level + "/50: " + _step + "/" + _solution.Steps.Count + " rapid taps";
            }
            catch (Exception error) { Finish("FAIL: " + error.Message); Debug.LogException(error); }
        }

        private static void CaptureInitial(PuzzleSession session, double now)
        {
            if (_stage == 0)
            {
                SetTheme(ThemeMode.Light); _metrics = Analyze(session.Level, _level);
                _solution = PuzzleSolver.Solve(session.Level); Require(_solution.Solvable, _solution.Error);
                Require(Signatures.Add(DenseCampaignVerification.GeometrySignature(session.Level)), "duplicate geometry");
                _step = _peakShots = 0; _settleUntil = now + .2; _stage = 1; return;
            }
            if (_stage == 1)
            {
                if (now < _settleUntil) return;
                Require(session.Remaining == session.Level.Arrows.Count && session.Lives == 3, "capture must show untouched initial board");
                _metrics.SetDisplay(session); Capture("light"); _stage = 2; return;
            }
            if (_stage == 2)
            {
                if (Time.frameCount <= _captureFrame) return;
                Require(File.Exists(CapturePath("light")), "initial screenshot was not written");
                if (!CaptureDark(_level)) { _stage = 5; return; }
                SetTheme(ThemeMode.Dark); _settleUntil = now + .15; _stage = 3; return;
            }
            if (_stage == 3)
            {
                if (now < _settleUntil) return;
                Capture("dark"); _stage = 4; return;
            }
            if (Time.frameCount <= _captureFrame) return;
            Require(File.Exists(CapturePath("dark")), "dark screenshot was not written");
            SetTheme(ThemeMode.Light); _stage = 5;
        }

        private static void CompleteLevel(PuzzleSession session, double now)
        {
            Require(session.Lives == 3 && session.Remaining == 0 && !session.HasActiveShots, "perfect witness must settle all flights");
            Require(session.RocketCharges == session.Config.RocketCharges, "normal witness spent rocket inventory");
            Require(_peakShots >= 2, "rapid taps never overlapped arrow flights"); VerifyNoSaveWrites();
            _metrics.SetResult(_peakShots, _step); Rows.Add(_metrics);
            Results.Add("Shapes " + _level.ToString("D2") + " " + session.Level.Title + ": PASS, " + _step +
                " rapid taps, peak " + _peakShots + " simultaneous flights, 3 lives, exact coverage, no save writes.");
            WriteMetrics();
            if (_level == 50) { Finish("PASS all 50 Shapes runtime playthroughs and initial captures"); return; }
            _level++; session.LoadLevel(_level); Time.timeScale = 1; _stage = 0; _deadline = now + 180;
        }

        private static bool CaptureDark(int level) => level == 2 || level == 3 || level == 15 || level == 20 || level == 28 || level == 34 || level == 50;
        private static string CaptureRelative(int level, string theme) => _captureRun + "/shapes-" + level.ToString("D2") + "-" + theme + ".png";
        private static string CapturePath(string theme) => Folder + "/" + CaptureRelative(_level, theme);
        private static void Capture(string theme) { ScreenCapture.CaptureScreenshot(CapturePath(theme)); _captureFrame = Time.frameCount; }
        private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException("Revision3: " + message); }
        private static void PlayModeChanged(PlayModeStateChange state)
        { if (_running && state == PlayModeStateChange.ExitingPlayMode) Finish("ABORT: Play Mode exiting"); }
        private static void BeforeReload() { if (_running) Finish("ABORT: script reload"); }
        private static void Finish(string outcome)
        {
            _running = false; EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= PlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= BeforeReload;
            try { Restore(); } catch (Exception error) { outcome += " / RESTORE FAILED: " + error.Message; }
            Status = outcome; WriteMetrics(); Directory.CreateDirectory(Folder);
            File.WriteAllText(Folder + "/runtime-verification.txt", outcome + "\n" + string.Join("\n", Results) + "\n");
            Debug.Log("[ArrowSwarm] " + outcome + " (" + Folder + ")");
        }
    }
}
