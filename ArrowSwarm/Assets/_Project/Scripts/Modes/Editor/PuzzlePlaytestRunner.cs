namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Puzzle;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Plays every catalog level through real pooled animations in non-saving preview mode.</summary>
    public static partial class PuzzlePlaytestRunner
    {
        private const string CaptureFolder = "Captures/Revision2/Playthroughs";
        private static int _modeIndex, _level, _step, _stage, _catalogCount, _captureFrame;
        private static double _deadline, _pauseUntil, _captureUntil;
        private static bool _initializing, _captured, _running;
        private static PuzzleSolution _solution;
        private static readonly List<string> Results = new List<string>();
        public static string Status { get; private set; } = "Idle";
        private static GameModeId Mode => _modeIndex == 0 ? GameModeId.Shapes : GameModeId.ColorShift;

        /// <summary>Starts only in Play Mode and temporarily disables automatic campaign saves.</summary>
        [MenuItem("Arrow Swarm/Puzzles/Playtest All Levels (Play Mode)")]
        public static void Begin()
        {
            if (_running) throw new InvalidOperationException("A campaign playtest is already running.");
            if (!Application.isPlaying) throw new InvalidOperationException("Enter Play Mode first.");
            Snapshot(); _running = true; Results.Clear(); _modeIndex = 0; _level = 1; _stage = 0;
            Directory.CreateDirectory(CaptureFolder);
            EditorApplication.update += Tick; EditorApplication.playModeStateChanged += PlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
            try { LoadCampaign(); }
            catch (Exception exception) { Finish("FAIL: " + exception.Message); throw; }
        }

        /// <summary>Cancels the current witness and restores all temporary runner settings.</summary>
        [MenuItem("Arrow Swarm/Puzzles/Cancel Campaign Playtest")]
        public static void Cancel() { if (_running) Finish("ABORT: cancelled after " + Results.Count + " completed levels"); }

        private static void LoadCampaign()
        {
            PuzzleLaunch.Preview = true; PuzzleLaunch.PreviewMode = Mode; PuzzleLaunch.PreviewLevel = 1;
            _initializing = true; _solution = null; _captured = false;
            _deadline = EditorApplication.timeSinceStartup + 180;
            Status = "Loading " + Mode + " campaign";
            SceneManager.LoadScene(Mode == GameModeId.Shapes ? "ShapesScene" : "ColorShiftScene");
        }

        private static void Tick()
        {
            if (!_running) return;
            try
            {
                if (!Application.isPlaying) { Finish("ABORT: Play Mode ended"); return; }
                double now = EditorApplication.timeSinceStartup;
                if (now > _deadline) throw new Exception("Playtest timed out: " + Status);
                var session = PuzzleSession.Active;
                if (session == null || session.Level == null || session.Mode != Mode) return;
                Require(session.Preview, "playthrough must never award progress");
                if (_initializing)
                {
                    _initializing = false; Accelerate(session);
                    var catalog = Field<PuzzleCatalog>(session, "_catalog");
                    Require(catalog != null && catalog.Count > 0, "bound campaign is empty");
                    _catalogCount = catalog.Count;
                    Require(session.LevelNumber == 1 && session.Lives == 3, "fresh campaign state");
                }
                if (_stage == 1)
                {
                    if (now < _pauseUntil) return;
                    Require(session.Paused && Time.timeScale == 0, "pause stops time");
                    session.Pause(false); Time.timeScale = 10; _stage = 2; return;
                }
                if (session.Lost) throw new Exception("Unexpected loss in solution witness");
                if (session.Won)
                {
                    Require(session.Lives == 3 && session.Remaining == 0, "perfect witness clears all arrows");
                    Require(session.RocketCharges == 1, "normal witness unexpectedly spent the rescue rocket");
                    VerifyPresentation(session); VerifyNoSaveWrites();
                    Results.Add(Mode + " " + _level + "/" + _catalogCount + " (" + session.Level.Title +
                        "): PASS " + session.Level.Arrows.Count + " arrows, " + _step + " actual actions, 3 lives, " +
                        session.Turns + " turns left, frame parity, grid-only card, no save writes; " + CapturePath());
                    if (_level < _catalogCount)
                    {
                        _level++; session.LoadLevel(_level); _solution = null; _captured = false;
                        _deadline = now + 180; return;
                    }
                    if (_modeIndex == 0) { _modeIndex = 1; _level = 1; _stage = 0; LoadCampaign(); return; }
                    Finish("PASS all " + Results.Count + " runtime playthroughs"); return;
                }
                if (!session.Ready) return;
                Time.timeScale = 10;
                if (_solution == null)
                {
                    _solution = PuzzleSolver.Solve(session.Level); Require(_solution.Solvable, "level witness");
                    _step = 0; _captureUntil = now + .15; VerifyPresentation(session);
                    Status = Mode + " " + _level + "/" + _catalogCount + ": settling camera before capture"; return;
                }
                if (!_captured)
                {
                    if (now < _captureUntil) return;
                    VerifyPresentation(session); ScreenCapture.CaptureScreenshot(CapturePath());
                    _captured = true; _captureFrame = Time.frameCount; return;
                }
                if (Time.frameCount <= _captureFrame) return;
                VerifyFrameParity(session);
                Status = Mode + " " + _level + "/" + _catalogCount + " action " + _step + "/" + _solution.Steps.Count;
                Require(_step < _solution.Steps.Count, "witness exhausted before runtime win");
                int action = _solution.Steps[_step++];
                if (action < 0) session.Rotate(1); else session.Fire(action);
                if (_stage == 0) { _stage = 1; session.Pause(true); _pauseUntil = now + .2; }
            }
            catch (Exception exception) { Finish("FAIL: " + exception.Message); Debug.LogException(exception); }
        }

        private static string CapturePath() => CaptureFolder + "/" + Mode.ToString().ToLowerInvariant() + "-" + _level.ToString("D2") + ".png";
        private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
        private static void PlayModeChanged(PlayModeStateChange state)
        { if (_running && state == PlayModeStateChange.ExitingPlayMode) Finish("ABORT: Play Mode exiting"); }
        private static void BeforeReload() { if (_running) Finish("ABORT: script reload"); }

        private static void Finish(string outcome)
        {
            _running = false; EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= PlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= BeforeReload;
            try { Restore(); }
            catch (Exception exception) { outcome += " / RESTORE FAILED: " + exception.Message; }
            Status = outcome; _solution = null;
            Directory.CreateDirectory(CaptureFolder);
            File.WriteAllText(CaptureFolder + "/runtime-verification.txt", outcome + "\n" + string.Join("\n", Results) + "\n");
            Debug.Log("[ArrowSwarm] " + outcome + " (" + CaptureFolder + ")");
        }
    }
}
