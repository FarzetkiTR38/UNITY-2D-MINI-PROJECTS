namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Core;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Puzzle;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>Exercises the real shared scene transition without advancing any campaign.</summary>
    public static partial class MenuTransitionVerification
    {
        private const string Output = "Captures/UIVisualRevision";
        private static readonly List<string> Results = new List<string>();
        private static IEnumerator _steps;
        private static Func<bool> _wait;
        private static Scope _scope;
        private static double _deadline;
        private static int _failures, _midpoints, _sceneLoads;
        private static string _expectedScene;
        private static bool _running;
        /// <summary>Current result or asynchronous verification stage.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Starts in an idle MainMenuScene in Play Mode and uses production mode navigation.</summary>
        public static void Begin()
        {
            if (!Application.isPlaying || _running || SceneManager.GetActiveScene().name != "MainMenuScene" ||
                !SceneTransitionManager.HasInstance || SceneTransitionManager.Instance.IsTransitioning)
                throw new InvalidOperationException("Start once in idle MainMenuScene Play Mode.");
            Results.Clear(); Captures.Clear(); _failures = _midpoints = _sceneLoads = 0;
            _scope = null; _expectedScene = null; _capturing = false; _captureError = null;
            _running = true; _deadline = EditorApplication.timeSinceStartup + 180;
            Directory.CreateDirectory(Output); _steps = Run(); Status = "Starting transition verification";
            EditorApplication.update += Tick; EditorApplication.playModeStateChanged += ModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += Cancel;
            SceneManager.sceneLoaded += SceneLoaded; Canvas.willRenderCanvases += ObserveRender;
        }

        /// <summary>Cancels the fixture and restores its isolated player, debug and rendering context.</summary>
        public static void Cancel() { if (_running) Finish("ABORT: transition verification interrupted"); }

        private static IEnumerator Run()
        {
            _scope = new Scope();
            try
            {
                var manager = SceneTransitionManager.Instance;
                var image = Field<Image>(manager, "_overlayImage");
                Check(_scope.Source != null && image.material != _scope.Source, "Authored custom material has a separate runtime clone");
                Check(image.material.shader.name == "UI/SwarmIrisWipe", "Actual overlay uses the authored Swarm iris shader");
                Check(RuntimeCount() == 1, "Exactly one runtime custom material instance exists");
                Status = "Paused real-time iris, ten rendered transition captures";
                _captureManager = manager; _closingCapture = _openingCapture = 0; _lastCaptureFrame = -1;
                _capturing = true; _midpointReached = false; Time.timeScale = 0;
                manager.PlayTransition(ActionMidpoint, 6f, new Vector2(.5f, .5f));
                Check(manager.IsTransitioning && image.gameObject.activeInHierarchy && image.raycastTarget,
                    "Paused transition activates its full-screen input blocker immediately");
                yield return (Func<bool>)(() => !manager.IsTransitioning);
                _capturing = false;
                Check(_captureError == null, "Render capture callback did not throw: " + (_captureError ?? "none"));
                Check(_midpoints == 1 && Time.timeScale == 0, "Action executes once and finishes using unscaled time while paused");
                VerifyComplete("Paused action"); Time.timeScale = 1;
                yield return (Func<bool>)(() => CaptureFilesReady());
                Check(Captures.Count == 10, "Ten distinct actual transition frames were captured");
                foreach (var capture in Captures)
                    Results.Add("CAPTURE: " + capture.Path + "; progress=" + capture.Progress.ToString("F3") +
                        "; frame=" + capture.Frame + "; bytes=" + new FileInfo(capture.Path).Length);

                foreach (var mode in new[] { GameModeId.Shapes, GameModeId.ColorShift, GameModeId.Defense })
                {
                    string target = mode == GameModeId.Defense ? "GameScene" : mode == GameModeId.Shapes ? "ShapesScene" : "ColorShiftScene";
                    Status = "Real menu Play route to " + target; _scope.Player.selectedGameMode = mode;
                    _expectedScene = target; GameModeSelection.PlaySelected();
                    Check(manager.IsTransitioning, mode + " production Play route starts the shared transition");
                    yield return (Func<bool>)(() => SceneManager.GetActiveScene().name == target && !manager.IsTransitioning && BoardReady(mode));
                    Check(GameManager.Instance.CurrentState == GameState.Playing, mode + " reaches its real Playing state");
                    Check(mode == GameModeId.Defense ? LevelManager.Instance.TotalArrows > 0 : PuzzleSession.Active.Mode == mode,
                        mode + " actual map is loaded through its normal route");
                    VerifyComplete(mode + " entry"); VerifyMaterial(mode + " entry");
                    Status = "Real " + mode + " menu return"; _expectedScene = "MainMenuScene";
                    if (mode == GameModeId.Defense) GameManager.Instance.GoToMainMenu(); else PuzzleSession.Active.Menu();
                    yield return (Func<bool>)(() => SceneManager.GetActiveScene().name == "MainMenuScene" &&
                        !manager.IsTransitioning && UnityEngine.Object.FindFirstObjectByType<MainMenuUI>() != null);
                    Check(GameManager.Instance.CurrentState == GameState.Menu, mode + " returns to actual Menu state");
                    VerifyComplete(mode + " return"); VerifyMaterial(mode + " return");
                }
                Check(_sceneLoads == 6, "All six actual scene loads occurred behind the fully closed overlay");
                Check(JsonUtility.ToJson(GameManager.Instance.Config) == _scope.ConfigJson, "Authored game configuration remains unchanged");
            }
            finally { _scope.Dispose(); }
        }

        private static bool BoardReady(GameModeId mode) => mode == GameModeId.Defense ?
            LevelManager.HasInstance && LevelManager.Instance.TotalArrows > 0 && GameManager.Instance.CurrentState == GameState.Playing :
            PuzzleSession.Active != null && PuzzleSession.Active.Level != null && PuzzleSession.Active.Ready;

        private static void ActionMidpoint()
        {
            _midpoints++; _midpointReached = true;
            VerifyCovered("Paused callback midpoint");
            Capture("closed-midpoint", 1f);
        }

        private static void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!_running || _expectedScene == null) return;
            _sceneLoads++; Check(scene.name == _expectedScene, "Expected scene loaded: " + _expectedScene);
            VerifyCovered(scene.name + " scene-loaded midpoint");
        }

        private static void Tick()
        {
            if (!_running) return;
            try
            {
                if (!Application.isPlaying || EditorApplication.timeSinceStartup > _deadline)
                    throw new TimeoutException(Status);
                EditorApplication.QueuePlayerLoopUpdate();
                if (_wait != null) { if (!_wait()) return; _wait = null; }
                if (!_steps.MoveNext()) { Finish((_failures == 0 ? "PASS" : "FAIL") + ": real shared transitions; failures=" + _failures); return; }
                _wait = _steps.Current as Func<bool>;
            }
            catch (Exception error) { Finish("FAIL: " + error.Message); }
        }

        private static void ModeChanged(PlayModeStateChange change)
        { if (_running && change == PlayModeStateChange.ExitingPlayMode) Finish("ABORT: Play Mode exiting"); }

        private static void Finish(string result)
        {
            _running = false; _capturing = false; _expectedScene = null;
            EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= ModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= Cancel;
            SceneManager.sceneLoaded -= SceneLoaded; Canvas.willRenderCanvases -= ObserveRender;
            try { (_steps as IDisposable)?.Dispose(); _scope?.Dispose(); }
            catch (Exception error) { result += " / RESTORE FAILED: " + error.Message; }
            _steps = null; _wait = null; _scope = null; _captureManager = null;
            if (_failures > 0 && result.StartsWith("PASS")) result = "FAIL: restoration checks; failures=" + _failures;
            Status = result; File.WriteAllText(Output + "/transition-verification.txt", result + "\n" + string.Join("\n", Results) + "\n");
            Debug.Log("[ArrowSwarm] " + result);
        }

        private static void Check(bool condition, string label)
        { Results.Add((condition ? "PASS: " : "FAIL: ") + label); if (!condition) _failures++; }
    }
}
