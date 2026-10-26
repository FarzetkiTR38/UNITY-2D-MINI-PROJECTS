namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Runs isolated real-player-loop regressions for terminal Defense flights and delayed puzzle rewards.</summary>
    public static partial class Quality4GameplayVerification
    {
        private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
        private static readonly List<string> Results = new List<string>();
        private static GameplayScope _scope;
        private static bool _running;
        private static int _failures;
        private static double _deadline;
        private static Func<bool> _condition;
        private static string _report;

        /// <summary>Latest progress or completed result; report files retain all assertions.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Checks an actual mob's final-life exit while the last pooled Defense arrows remain in flight.</summary>
        public static void BeginDefense(string reportName = "defense-terminal-before.txt") => Begin(Defense(), reportName);

        /// <summary>Checks real Shapes and Color Shift hint requests through a delayed, network-free ad provider.</summary>
        public static void BeginPuzzleRewards(string reportName = "puzzle-reward-before.txt") => Begin(PuzzleRewards(), reportName);

        /// <summary>Cancels any current run and restores every captured user setting.</summary>
        public static void Cancel() { if (_running) Finish("ABORT: cancelled"); }

        private static void Begin(IEnumerator run, string reportName)
        {
            if (_running) throw new InvalidOperationException("A gameplay verification is already running.");
            if (!Application.isPlaying || !DataManager.HasInstance || !GameManager.HasInstance)
                throw new InvalidOperationException("Enter Play Mode with the booted game managers first.");
            if (Path.GetFileName(reportName) != reportName) throw new ArgumentException("Supply a report filename only.");
            _scope = new GameplayScope(); _running = true; _failures = 0; _condition = null;
            _report = Path.Combine("Captures/QualityPass4", reportName); Results.Clear(); Steps.Clear(); Steps.Push(run);
            _deadline = EditorApplication.timeSinceStartup + 180; Status = "Starting " + reportName;
            EditorApplication.update += Tick; EditorApplication.playModeStateChanged += ChangedPlayMode;
            AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
        }

        private static void Tick()
        {
            if (!_running) return;
            try
            {
                if (!Application.isPlaying) { Finish("ABORT: Play Mode ended"); return; }
                if (EditorApplication.timeSinceStartup > _deadline) throw new TimeoutException(Status);
                if (_condition != null) { if (!_condition()) return; _condition = null; }
                while (Steps.Count > 0)
                {
                    var iterator = Steps.Peek();
                    if (!iterator.MoveNext()) { (Steps.Pop() as IDisposable)?.Dispose(); continue; }
                    if (iterator.Current is IEnumerator nested) { Steps.Push(nested); continue; }
                    if (iterator.Current is Func<bool> condition) _condition = condition;
                    return;
                }
                Finish(_failures == 0 ? "PASS" : "FAIL: " + _failures + " regression assertions");
            }
            catch (Exception error) { Results.Add(error.ToString()); Finish("ERROR: " + error.Message); }
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
                while (Steps.Count > 0) (Steps.Pop() as IDisposable)?.Dispose();
                if (Application.isPlaying)
                {
                    if (LevelManager.HasInstance) LevelManager.Instance.CleanupLevel();
                    Time.timeScale = 1; GameManager.Instance.SetState(GameState.Menu);
                    if (SceneManager.GetActiveScene().name != "MainMenuScene") SceneManager.LoadScene("MainMenuScene");
                }
            }
            catch (Exception error) { outcome += " / CLEANUP: " + error.Message; }
            finally
            {
                try { _scope?.Dispose(); Check(_scope != null && _scope.Restored, "Original data, preferences, provider, debug and config restored"); }
                catch (Exception error) { outcome += " / RESTORE: " + error.Message; }
                _scope = null;
            }
            if (_failures > 0 && outcome == "PASS") outcome = "FAIL: " + _failures + " regression assertions";
            Status = outcome; _condition = null;
            Directory.CreateDirectory(Path.GetDirectoryName(_report));
            File.WriteAllText(_report, outcome + "\n" + string.Join("\n", Results) + "\n");
            Debug.Log("[ArrowSwarm] Quality4 gameplay " + outcome + " (" + _report + ")");
        }

        private static void Require(bool condition, string label)
        { if (!condition) throw new InvalidOperationException(label); Results.Add("SETUP: " + label); }

        private static void Check(bool condition, string label)
        { Results.Add((condition ? "PASS: " : "FAIL: ") + label); if (!condition) _failures++; }

        private static Func<bool> NextFrames(int count = 1)
        { int frame = Time.frameCount; return () => Time.frameCount >= frame + count; }

        private static bool MenuReady() => SceneManager.GetActiveScene().name == "MainMenuScene" &&
            (!SceneTransitionManager.HasInstance || !SceneTransitionManager.Instance.IsTransitioning);
    }
}
