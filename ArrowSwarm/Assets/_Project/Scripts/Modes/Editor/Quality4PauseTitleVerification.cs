namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.Puzzle;
    using ArrowSwarm.UI;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Verifies real pause-title presentation across four gameplay scenes, ten languages and both themes.</summary>
    public static partial class Quality4PauseTitleVerification
    {
        private const string Folder = "Captures/QualityPass4/PauseTitle";
        private static readonly List<string> Results = new List<string>();
        private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
        private static Scope _scope;
        private static Func<bool> _condition;
        private static bool _running;
        private static double _deadline;
        private static int _failures, _combinations;

        /// <summary>Current actual-scene stage or final result for MCP polling.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Starts from fresh Boot-to-MainMenu Play Mode without opening the live leaderboard.</summary>
        public static void Begin()
        {
            if (_running) throw new InvalidOperationException("Pause-title verification is already running.");
            if (!Application.isPlaying || SceneManager.GetActiveScene().name != "MainMenuScene" ||
                !DataManager.HasInstance || !GameManager.HasInstance || !LocalizationManager.HasInstance || LeaderboardManager.HasInstance)
                throw new InvalidOperationException("Use a fresh booted MainMenu Play session before opening leaderboard.");
            Results.Clear(); Steps.Clear(); _condition = null; _failures = _combinations = 0;
            try
            {
                _scope = new Scope(); _running = true; _deadline = EditorApplication.timeSinceStartup + 300;
                Directory.CreateDirectory(Folder); Steps.Push(Run()); Status = "Loading actual gameplay pause titles";
                EditorApplication.update += Tick; EditorApplication.playModeStateChanged += ModeChanged;
                AssemblyReloadEvents.beforeAssemblyReload += BeforeReload; SceneManager.sceneLoaded += ConfigurePreview;
            }
            catch (Exception error) { Finish("FAIL: " + error.Message); throw; }
        }

        /// <summary>Cancels pending captures and restores player, preferences, debug, preview, language and theme context.</summary>
        public static void Cancel() { if (_running) Finish("ABORT: pause-title verification cancelled"); }

        private static IEnumerator Run()
        {
            foreach (string scene in new[] { "GameScene", "ShapesScene", "ColorShiftScene", "MapTestScene" })
            {
                Status = "Loading " + scene; Time.timeScale = 1f;
                bool puzzle = scene == "ShapesScene" || scene == "ColorShiftScene";
                PuzzleLaunch.Preview = puzzle; PuzzleLaunch.PreviewLevel = 15;
                PuzzleLaunch.PreviewMode = scene == "ShapesScene" ? GameModeId.Shapes : GameModeId.ColorShift;
                SceneManager.LoadScene(scene);
                yield return (Func<bool>)(() => puzzle ? PuzzleSession.Active != null && PuzzleSession.Active.Level != null &&
                    PuzzleSession.Active.Ready : DefenseReady());
                if (ArrowSwarm.Mob.MobSpawner.HasInstance) ArrowSwarm.Mob.MobSpawner.Instance.StopSpawning();
                if (puzzle) PuzzleSession.Active.Pause(true);
                else if (scene == "MapTestScene") { Time.timeScale = 0f; GameManager.Instance.SetState(GameState.Paused); }
                else GameManager.Instance.PauseGame();
                var title = FindPauseTitle(); var group = PauseGroup(title);
                yield return (Func<bool>)(() => group.gameObject.activeInHierarchy && group.alpha >= .99f && group.interactable);
                yield return Settled(.1f);
                Check(Field<bool>(title, "_fitToRect"), scene + ": authored pause title opts into fitting");
                foreach (var language in LocalizationManager.Instance.AvailableLanguages)
                    foreach (ThemeMode theme in new[] { ThemeMode.Light, ThemeMode.Dark })
                    {
                        string label = scene + " / " + language.code + " / " + theme; Status = label;
                        DataManager.Instance.SetTheme(theme); LocalizationManager.Instance.SetLanguage(language.code, false);
                        yield return Settled(.12f); title.ApplyAll(); yield return Settled(.05f);
                        yield return VerifyTitle(title, label, language.code);
                        if (language.code == "tr") yield return Capture(scene + "-tr-" + theme.ToString().ToLowerInvariant());
                        _combinations++;
                    }
            }
            Check(_combinations == 80, "All four actual pause scenes × ten languages × two themes were visited");
            Check(!LeaderboardManager.HasInstance && _scope.FactoryCalls == 0,
                "No leaderboard instance or real cloud request was started");
            ReturnToMenu(); yield return Settled(.15f);
        }

        private static bool DefenseReady()
        {
            if (!ArrowSwarm.Arrow.ArrowSpawner.HasInstance || ArrowSwarm.Arrow.ArrowSpawner.Instance.ActiveArrows.Count == 0) return false;
            foreach (var arrow in ArrowSwarm.Arrow.ArrowSpawner.Instance.ActiveArrows)
                if (arrow != null && arrow.GetComponent<ArrowSwarm.Arrow.ArrowVisuals>().IsSpawning) return false;
            return UnityEngine.Object.FindFirstObjectByType<OverlayManager>(FindObjectsInactive.Include) != null;
        }

        private static Func<bool> Settled(float seconds)
        { float until = Time.unscaledTime + seconds; int frame = Time.frameCount; return () => Time.unscaledTime > until && Time.frameCount > frame + 2; }

        private static IEnumerator Capture(string label)
        {
            string path = Path.Combine(Folder, label + "-" + DateTime.UtcNow.Ticks + ".png"); int frame = Time.frameCount;
            Canvas.ForceUpdateCanvases(); ScreenCapture.CaptureScreenshot(path);
            yield return (Func<bool>)(() => Time.frameCount > frame + 2 && File.Exists(path) && new FileInfo(path).Length > 0);
            Results.Add("CAPTURE: " + path);
        }

        private static void Tick()
        {
            if (!_running) return;
            try
            {
                if (!Application.isPlaying || EditorApplication.timeSinceStartup > _deadline) throw new TimeoutException(Status);
                EditorApplication.QueuePlayerLoopUpdate();
                if (_condition != null) { if (!_condition()) return; _condition = null; }
                while (Steps.Count > 0)
                {
                    var step = Steps.Peek();
                    if (!step.MoveNext()) { (Steps.Pop() as IDisposable)?.Dispose(); continue; }
                    if (step.Current is IEnumerator nested) { Steps.Push(nested); continue; }
                    _condition = step.Current as Func<bool>; return;
                }
                Finish((_failures == 0 ? "PASS" : "FAIL: " + _failures + " title assertions") + ": " + _combinations + " actual pause combinations");
            }
            catch (Exception error) { Results.Add(error.ToString()); Finish("FAIL: " + error.Message); }
        }

        private static void ModeChanged(PlayModeStateChange state)
        { if (_running && state == PlayModeStateChange.ExitingPlayMode) Finish("ABORT: Play Mode exiting"); }
        private static void BeforeReload() { if (_running) Finish("ABORT: assembly reload"); }
        private static void Finish(string outcome)
        {
            _running = false; EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= ModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= BeforeReload; SceneManager.sceneLoaded -= ConfigurePreview;
            try { while (Steps.Count > 0) (Steps.Pop() as IDisposable)?.Dispose(); if (Application.isPlaying) ReturnToMenu(); }
            catch (Exception error) { outcome += " / MENU CLEANUP: " + error.Message; }
            finally { try { _scope?.Dispose(); } catch (Exception error) { outcome += " / RESTORE FAILED: " + error.Message; } }
            _scope = null; _condition = null; Status = outcome; Directory.CreateDirectory("Captures/QualityPass4");
            File.WriteAllText("Captures/QualityPass4/pause-title-verification.txt", outcome + "\n" + string.Join("\n", Results) + "\n");
            Debug.Log("[ArrowSwarm] " + outcome);
        }

        private static void Check(bool value, string label)
        { Results.Add((value ? "PASS: " : "FAIL: ") + label); if (!value) throw new InvalidOperationException(label); }
        private static void Verify(bool value, string label)
        { if (!value) _failures++; Results.Add((value ? "PASS: " : "FAIL: ") + label); }
    }
}
