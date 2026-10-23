namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.UI;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Exercises original Defense gameplay and UI in Play Mode without changing saved progress or tuning assets.</summary>
    public static partial class DefenseRevisionVerification
    {
        private static string _folder = "Captures/Revision2";
        private static string Report => Path.Combine(_folder, "defense-verification.txt");
        private static string Capture => Path.Combine(_folder, "defense-101.png");
        private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
        private static readonly List<string> Results = new List<string>();
        private static bool _running, _restored, _exiting;
        private static double _until, _deadline;
        private static Func<bool> _condition;
        public static string Status { get; private set; } = "Idle";

        /// <summary>Starts a real level101 smoke test with temporary inventory and an in-memory debug override.</summary>
        [MenuItem("Arrow Swarm/Puzzles/Verify Original Defense 101 (Play Mode)")]
        public static void Begin() => BeginAt("Captures/Revision2");

        /// <summary>Writes a new verification run into its own folder without replacing earlier evidence.</summary>
        public static void BeginAt(string folder)
        {
            if (_running) throw new InvalidOperationException("Defense verification is already running.");
            if (!Application.isPlaying || !DataManager.HasInstance || !GameManager.HasInstance)
                throw new InvalidOperationException("Enter Play Mode with the existing game managers before verifying Defense.");
            if (string.IsNullOrWhiteSpace(folder)) throw new ArgumentException("A report folder is required.", nameof(folder));
            _folder = folder;
            Snapshot(); _running = true; _restored = _exiting = false; _condition = null; _until = 0;
            _deadline = EditorApplication.timeSinceStartup + 180; Results.Clear(); Steps.Clear(); Steps.Push(Run());
            Directory.CreateDirectory(_folder); if (File.Exists(Capture)) File.Delete(Capture);
            Status = "Loading original Defense level101";
            EditorApplication.update += Tick; EditorApplication.playModeStateChanged += ChangedPlayMode;
            AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
        }

        /// <summary>Cancels testing and restores all temporary player and debug settings.</summary>
        [MenuItem("Arrow Swarm/Puzzles/Cancel Defense Verification")]
        public static void Cancel() { if (_running) Finish("ABORT: Defense verification cancelled"); }

        private static IEnumerator Run()
        {
            PuzzleLaunch.Preview = false; Time.timeScale = 1f;
            _data.PlayerData.tipCount = 2; _data.PlayerData.freezeCount = 2;
            SceneManager.LoadScene("GameScene");
            yield return (Func<bool>)DefenseReady;
            yield return .5f;
            var hud = UnityEngine.Object.FindFirstObjectByType<GameHUD>();
            CheckOriginalLayout(hud);
            yield return (Func<bool>)(() => ArrowSwarm.Mob.MobSpawner.Instance.SpawnedCount >= 2 &&
                ArrowSwarm.Mob.MobSpawner.Instance.ActiveMobs.Count > 0);
            CheckOriginalBoard();
            yield return .15f;
            int frame = Time.frameCount; ScreenCapture.CaptureScreenshot(Capture);
            yield return (Func<bool>)(() => Time.frameCount > frame && File.Exists(Capture) && new FileInfo(Capture).Length > 0);
            Check(true, "Captured original Defense101 with populated arrows, portal, enemies and original HUD");
            yield return CheckSkillsAndPause(hud);
            yield return CheckArrowsAndRetry(hud);
            CheckPreferences();
            CheckInventoryIsolation();
            Check(JsonUtility.ToJson(_config) == _savedConfig, "Original GameConfig asset remained unchanged");
            Status = "Returning to menu and restoring original settings";
            CleanupRuntime();
            yield return (Func<bool>)(() => SceneManager.GetActiveScene().name == "MainMenuScene" &&
                (!SceneTransitionManager.HasInstance || !SceneTransitionManager.Instance.IsTransitioning));
            Restore();
            Check(JsonUtility.ToJson(_data.PlayerData) == _savedData, "Complete original PlayerData restored");
            CheckPreferences();
            Check(GameManager.Instance.CurrentState == GameState.Menu, "Original game managers returned safely to menu");
        }

        private static bool DefenseReady()
        {
            return SceneManager.GetActiveScene().name == "GameScene" && LevelManager.HasInstance &&
                LevelManager.Instance.CurrentLevelData.IsValid && LevelManager.Instance.CurrentParams.Level == 101 &&
                GameManager.Instance.CurrentState == GameState.Playing &&
                UnityEngine.Object.FindFirstObjectByType<GameHUD>() != null &&
                (!SceneTransitionManager.HasInstance || !SceneTransitionManager.Instance.IsTransitioning);
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
                Finish("PASS: original Defense101 UI, arrows, bounce damage, hint, freeze, pause and retry");
            }
            catch (Exception exception) { Finish("FAIL: " + exception.Message); Debug.LogException(exception); }
        }

        private static void ChangedPlayMode(PlayModeStateChange state)
        { if (_running && state == PlayModeStateChange.ExitingPlayMode) { _exiting = true; Finish("ABORT: Play Mode exiting"); } }
        private static void BeforeReload() { if (_running) Finish("ABORT: script reload"); }

        private static void Finish(string outcome)
        {
            _running = false; EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= ChangedPlayMode;
            AssemblyReloadEvents.beforeAssemblyReload -= BeforeReload;
            try
            {
                while (Steps.Count > 0) (Steps.Pop() as IDisposable)?.Dispose();
                if (!_restored) CleanupRuntime();
            }
            catch (Exception exception) { outcome += " / CLEANUP FAILED: " + exception.Message; }
            finally
            {
                try { Restore(); }
                catch (Exception exception) { outcome += " / RESTORE FAILED: " + exception.Message; }
            }
            try { CheckPreferences(); }
            catch (Exception exception) { outcome += " / PREFERENCES CHANGED: " + exception.Message; }
            Steps.Clear(); _condition = null; Status = outcome;
            Directory.CreateDirectory(_folder);
            File.WriteAllText(Report, outcome + "\n" + string.Join("\n", Results) + "\n");
            Debug.Log("[ArrowSwarm] " + outcome + " (" + Report + ")");
        }

        private static void Check(bool condition, string message)
        { if (!condition) throw new InvalidOperationException(message); Results.Add("PASS: " + message); }
    }
}
