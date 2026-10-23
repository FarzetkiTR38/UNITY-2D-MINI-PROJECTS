namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Core;
    using ArrowSwarm.Mob;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Reproduces overlapping Defense retreats with real pooled enemies and restores the test context.</summary>
    public static partial class QualityGapCloseVerification
    {
        private const string Report = "Captures/QualityPass/gap-close-verification.txt";
        private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
        private static readonly List<string> Results = new List<string>();
        private static bool _running, _restored, _exiting;
        private static double _until, _deadline;
        private static Func<bool> _condition;
        private static Action _extraCheck;

        /// <summary>Current real-scene verification progress or its final result.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Loads Defense101 without save writes and checks retreat replacement, freeze and pooled reuse.</summary>
        [MenuItem("Arrow Swarm/Quality/Verify Defense Gap Closing (Play Mode)")]
        public static void Begin() => BeginWithCheck(null);

        /// <summary>Runs an additional synchronous check in the safely isolated Defense scene before returning to menu.</summary>
        public static void BeginWithCheck(Action check)
        {
            if (_running) throw new InvalidOperationException("Gap-close verification is already running.");
            if (!Application.isPlaying || !ArrowSwarm.Data.DataManager.HasInstance || !GameManager.HasInstance)
                throw new InvalidOperationException("Enter Play Mode with the normal game managers first.");
            Snapshot(); _running = true; _restored = _exiting = false; _condition = null; _until = 0; _extraCheck = check;
            _deadline = EditorApplication.timeSinceStartup + 120; Results.Clear(); Steps.Clear(); Steps.Push(Run());
            Status = "Loading real Defense101"; EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += ModeChanged; AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
        }

        /// <summary>Cancels the check and restores temporary player, debug and background settings.</summary>
        public static void Cancel() { if (_running) Finish("ABORT: gap-close verification cancelled"); }

        private static IEnumerator Run()
        {
            PuzzleLaunch.Preview = false; Time.timeScale = 1f; SceneManager.LoadScene("GameScene");
            yield return (Func<bool>)(() => SceneManager.GetActiveScene().name == "GameScene" &&
                LevelManager.HasInstance && LevelManager.Instance.CurrentLevelData.IsValid &&
                LevelManager.Instance.CurrentParams.Level == 101 && GameManager.Instance.CurrentState == GameState.Playing &&
                (!SceneTransitionManager.HasInstance || !SceneTransitionManager.Instance.IsTransitioning));
            yield return .5f;
            yield return CheckReplacement();
            yield return CheckFreezeAndStop();
            yield return CheckPooledIdentity();
            Status = "Checking Defense body uploads and restoring menu";
            _extraCheck?.Invoke();
            Check(JsonUtility.ToJson(GameManager.Instance.Config) == _savedConfig, "Original GameConfig remained unchanged");
            Check(JsonUtility.ToJson(_data.PlayerData) == _savedData, "Player inventory, progress and settings remained unchanged");
            CheckPreferences(); CleanupRuntime();
            yield return (Func<bool>)(() => SceneManager.GetActiveScene().name == "MainMenuScene" &&
                (!SceneTransitionManager.HasInstance || !SceneTransitionManager.Instance.IsTransitioning));
            Restore(); CheckPreferences();
            Check(JsonUtility.ToJson(_data.PlayerData) == _savedData, "Original PlayerData fully restored");
            Check(GameManager.Instance.CurrentState == GameState.Menu, "Normal game managers safely returned to menu");
        }

        private static void Tick()
        {
            if (!_running) return;
            try
            {
                if (!Application.isPlaying) { _exiting = true; Finish("ABORT: Play Mode ended"); return; }
                if (EditorApplication.timeSinceStartup > _deadline) throw new Exception("Timeout: " + Status);
                if (_until > EditorApplication.timeSinceStartup) return;
                if (_condition != null) { if (!_condition()) return; _condition = null; }
                while (Steps.Count > 0)
                {
                    var iterator = Steps.Peek();
                    if (!iterator.MoveNext()) { Steps.Pop(); continue; }
                    if (iterator.Current is IEnumerator nested) { Steps.Push(nested); continue; }
                    if (iterator.Current is float delay) _until = EditorApplication.timeSinceStartup + delay;
                    else if (iterator.Current is Func<bool> condition) _condition = condition;
                    return;
                }
                Finish("PASS: real Defense retreat replacement, expiry, stop, freeze and pooled spawn identity");
            }
            catch (Exception exception) { Finish("FAIL: " + exception.Message); Debug.LogException(exception); }
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
            Steps.Clear(); _condition = null; _extraCheck = null; Status = outcome; Directory.CreateDirectory(Path.GetDirectoryName(Report));
            File.WriteAllText(Report, outcome + "\n" + string.Join("\n", Results) + "\n");
            Debug.Log("[ArrowSwarm] " + outcome + " (" + Report + ")");
        }

        private static void Check(bool condition, string message)
        { if (!condition) throw new InvalidOperationException(message); Results.Add("PASS: " + message); }
    }
}
