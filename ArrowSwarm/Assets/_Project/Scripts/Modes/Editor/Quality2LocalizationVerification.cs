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

    /// <summary>Checks real Defense and menu labels across all configured languages without saved progress changes.</summary>
    public static partial class Quality2LocalizationVerification
    {
        private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
        private static readonly List<string> Results = new List<string>();
        private static bool _running, _exiting;
        private static double _until, _deadline;
        private static Func<bool> _condition;

        /// <summary>Current asynchronous verification stage or final result.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Loads real scenes, uses authored controls and verifies localization without ads or cloud submissions.</summary>
        public static void Begin()
        {
            if (_running || !Application.isPlaying || !DataManager.HasInstance || !GameManager.HasInstance)
                throw new InvalidOperationException("Idle Play Mode managers are required.");
            Snapshot(); Results.Clear(); Steps.Clear(); _running = true; _exiting = false;
            Directory.CreateDirectory("Captures/QualityPass2");
            _condition = null; _until = 0; _deadline = EditorApplication.timeSinceStartup + 120;
            Steps.Push(Run()); Status = "Loading real Defense101 for localized HUD checks";
            EditorApplication.update += Tick; EditorApplication.playModeStateChanged += ModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
        }

        /// <summary>Cancels verification and restores saved data, language, debug settings and preferences.</summary>
        public static void Cancel() { if (_running) Finish("ABORT: localization verification cancelled"); }

        private static IEnumerator Run()
        {
            PuzzleLaunch.Preview = false; Time.timeScale = 1f; SceneManager.LoadScene("GameScene");
            yield return (Func<bool>)(() => SceneManager.GetActiveScene().name == "GameScene" &&
                LevelManager.HasInstance && LevelManager.Instance.CurrentParams.Level == 101 &&
                GameManager.Instance.CurrentState == GameState.Playing);
            yield return .6f;
            Time.timeScale = 0f;
            yield return CheckDefenseHud();
            Time.timeScale = 1f; GameManager.Instance.GoToMainMenu();
            yield return (Func<bool>)(() => SceneManager.GetActiveScene().name == "MainMenuScene" &&
                (!SceneTransitionManager.HasInstance || !SceneTransitionManager.Instance.IsTransitioning));
            yield return .25f;
            yield return CheckProfile();
            yield return CheckLevels();
            Restore();
            Check(_state.IsRestored(), "Original player data, inventory, settings and preference existence restored");
            Check(JsonUtility.ToJson(GameManager.Instance.Config) == _configJson, "Original GameConfig remained unchanged");
            Check(GameManager.Instance.CurrentState == GameState.Menu, "Real managers returned safely to the menu");
        }

        private static void Tick()
        {
            if (!_running) return;
            try
            {
                if (!Application.isPlaying) { _exiting = true; Finish("ABORT: Play Mode ended"); return; }
                if (EditorApplication.timeSinceStartup > _deadline) throw new InvalidOperationException("Timeout: " + Status);
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
                Finish("PASS: real Defense HUD, profile reopening and mode-aware level ranges across ten languages");
            }
            catch (Exception error) { Finish("FAIL: " + error.Message); Debug.LogException(error); }
        }

        private static void ModeChanged(PlayModeStateChange state)
        { if (_running && state == PlayModeStateChange.ExitingPlayMode) { _exiting = true; Finish("ABORT: Play Mode exiting"); } }
        private static void BeforeReload() { if (_running) Finish("ABORT: script reload"); }

        private static void Finish(string outcome)
        {
            _running = false; EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= ModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= BeforeReload;
            try { while (Steps.Count > 0) (Steps.Pop() as IDisposable)?.Dispose(); }
            finally { Restore(); }
            if (!_exiting && Application.isPlaying && SceneManager.GetActiveScene().name != "MainMenuScene")
                GameManager.Instance.GoToMainMenu();
            Status = outcome; Directory.CreateDirectory("Captures/QualityPass2");
            File.WriteAllText("Captures/QualityPass2/localization-verification.txt", outcome + "\n" + string.Join("\n", Results) + "\n");
            Debug.Log("[ArrowSwarm] " + outcome + " (" + Results.Count + " checks and fit observations)");
        }

        private static void Check(bool value, string label)
        { Results.Add((value ? "PASS: " : "FAIL: ") + label); if (!value) throw new InvalidOperationException(label); }
    }
}
