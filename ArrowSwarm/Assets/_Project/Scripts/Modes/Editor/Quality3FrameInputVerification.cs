namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Puzzle;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Verifies Color Shift UI touch ownership through actual Input System events without saving progress.</summary>
    public static partial class Quality3FrameInputVerification
    {
        private const string Report = "Captures/QualityPass3/frame-input-verification.txt";
        private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
        private static readonly List<string> Results = new List<string>();
        private static bool _running, _restored, _exiting;
        private static double _deadline;
        private static Func<bool> _condition;
        private static int _failures;

        /// <summary>Current real-input fixture stage or final PASS/FAIL result.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Starts from the real main menu, opens Color Shift preview, verifies gestures and returns to that menu.</summary>
        public static void Begin()
        {
            if (_running) throw new InvalidOperationException("Frame input verification is already running.");
            if (!Application.isPlaying || !DataManager.HasInstance || !GameManager.HasInstance ||
                SceneManager.GetActiveScene().name != "MainMenuScene")
                throw new InvalidOperationException("Start in MainMenuScene Play Mode with the normal managers first.");
            Snapshot(); _running = true; _restored = _exiting = false; _condition = null; _failures = 0;
            _deadline = EditorApplication.timeSinceStartup + 240; Results.Clear(); Steps.Clear(); Steps.Push(Run());
            Status = "Loading isolated Color Shift preview"; EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += ModeChanged; AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
        }

        /// <summary>Cancels queued UI events and restores devices, player settings and preferences.</summary>
        public static void Cancel() { if (_running) Finish("ABORT: frame input verification cancelled"); }

        private static IEnumerator Run()
        {
            PuzzleLaunch.Preview = true; PuzzleLaunch.PreviewMode = GameModeId.ColorShift; PuzzleLaunch.PreviewLevel = 1;
            Time.timeScale = 1f; SceneManager.LoadScene("ColorShiftScene");
            yield return (Func<bool>)(() => PuzzleSession.Active != null && PuzzleSession.Active.Level != null &&
                PuzzleSession.Active.Preview && PuzzleSession.Active.Ready &&
                SceneManager.GetActiveScene().name == "ColorShiftScene" &&
                (!SceneTransitionManager.HasInstance || !SceneTransitionManager.Instance.IsTransitioning));
            float readyAt = Time.unscaledTime + .4f;
            yield return (Func<bool>)(() => Time.unscaledTime > readyAt);
            yield return GestureChecks(PuzzleSession.Active);
            Check(JsonUtility.ToJson(GameManager.Instance.Config) == _savedConfig, "Original GameConfig unchanged");
            Check(JsonUtility.ToJson(_data.PlayerData) == _savedData, "Player data and all saved progress unchanged");
            CheckPreferences(); CleanupRuntime();
            yield return (Func<bool>)(() => SceneManager.GetActiveScene().name == "MainMenuScene" &&
                (!SceneTransitionManager.HasInstance || !SceneTransitionManager.Instance.IsTransitioning));
            Restore(); CheckPreferences();
            Check(JsonUtility.ToJson(_data.PlayerData) == _savedData && GameManager.Instance.CurrentState == GameState.Menu,
                "Original main-menu scene, player data and normal game state restored");
        }

        private static void Tick()
        {
            if (!_running) return;
            try
            {
                if (!Application.isPlaying) { _exiting = true; Finish("ABORT: Play Mode ended"); return; }
                if (EditorApplication.timeSinceStartup > _deadline) throw new Exception("Timeout: " + Status);
                if (_condition != null) { if (!_condition()) return; _condition = null; }
                while (Steps.Count > 0)
                {
                    var iterator = Steps.Peek();
                    if (!iterator.MoveNext()) { Steps.Pop(); continue; }
                    if (iterator.Current is IEnumerator nested) { Steps.Push(nested); continue; }
                    if (iterator.Current is Func<bool> condition) _condition = condition;
                    return;
                }
                Finish((_failures == 0 ? "PASS: " : "FAIL: " + _failures + " gesture invariants failed; ") +
                    "real Color Shift cancel, paused/retried ownership and fresh tap/swipe checks");
            }
            catch (Exception exception) { Finish("FAIL: fixture prerequisite / " + exception.Message); Debug.LogException(exception); }
        }

        private static void ModeChanged(PlayModeStateChange state)
        { if (_running && state == PlayModeStateChange.ExitingPlayMode) { _exiting = true; Finish("ABORT: Play Mode exiting"); } }
        private static void BeforeReload() { if (_running) Finish("ABORT: script reload"); }

        private static void Finish(string outcome)
        {
            _running = false; EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= ModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= BeforeReload;
            try { while (Steps.Count > 0) (Steps.Pop() as IDisposable)?.Dispose(); if (!_restored) CleanupRuntime(); }
            catch (Exception exception) { outcome += " / CLEANUP FAILED: " + exception.Message; }
            finally { try { Restore(); } catch (Exception exception) { outcome += " / RESTORE FAILED: " + exception.Message; } }
            try { CheckPreferences(); } catch (Exception exception) { outcome += " / PREFERENCES CHANGED: " + exception.Message; }
            Steps.Clear(); _condition = null; Status = outcome;
            Directory.CreateDirectory("Captures/QualityPass3"); File.WriteAllText(Report, outcome + "\n" + string.Join("\n", Results) + "\n");
            Debug.Log("[ArrowSwarm] " + outcome + " (" + Report + ")");
        }

        private static void Check(bool condition, string message)
        { if (!condition) throw new InvalidOperationException(message); Results.Add("PASS: " + message); }

        private static void Verify(bool condition, string message)
        { if (!condition) _failures++; Results.Add((condition ? "PASS: " : "FAIL: ") + message); }
    }
}
