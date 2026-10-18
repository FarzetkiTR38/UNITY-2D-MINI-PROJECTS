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

    /// <summary>Checks animated pooled enemies in the real Defense scene without saving player progress.</summary>
    public static partial class EnemyAnimationVerification
    {
        private const string Folder = "Captures/EnemyAnimationIntegration";
        private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
        private static readonly List<string> Results = new List<string>();
        private static bool _running, _restored, _exiting;
        private static double _until, _deadline;
        private static Func<bool> _condition;

        /// <summary>Current verification stage or final PASS/FAIL result.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Loads Defense101, checks animation/lifecycle behavior and captures real gameplay frames.</summary>
        [MenuItem("Arrow Swarm/Enemies/Verify Animated Enemies (Play Mode)")]
        public static void Begin()
        {
            if (_running) throw new InvalidOperationException("Enemy verification is already running.");
            if (!Application.isPlaying || !DataManager.HasInstance || !GameManager.HasInstance)
                throw new InvalidOperationException("Enter Play Mode with the normal game managers first.");
            Snapshot(); _running = true; _restored = _exiting = false; _condition = null; _until = 0;
            _deadline = EditorApplication.timeSinceStartup + 180; Results.Clear(); Steps.Clear(); Steps.Push(Run());
            Directory.CreateDirectory(Folder); Status = "Loading real GameScene / Defense101";
            EditorApplication.update += Tick; EditorApplication.playModeStateChanged += ModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
        }

        /// <summary>Cancels testing and restores temporary settings and player data.</summary>
        [MenuItem("Arrow Swarm/Enemies/Cancel Animated Enemy Verification")]
        public static void Cancel() { if (_running) Finish("ABORT: enemy verification cancelled"); }

        private static IEnumerator Run()
        {
            PuzzleLaunch.Preview = false; Time.timeScale = 1f;
            SceneManager.LoadScene("GameScene");
            yield return (Func<bool>)(() => SceneManager.GetActiveScene().name == "GameScene" &&
                LevelManager.HasInstance && LevelManager.Instance.CurrentLevelData.IsValid &&
                LevelManager.Instance.CurrentParams.Level == 101 && GameManager.Instance.CurrentState == GameState.Playing &&
                (!SceneTransitionManager.HasInstance || !SceneTransitionManager.Instance.IsTransitioning));
            yield return (Func<bool>)(() => ArrowSwarm.Mob.MobSpawner.Instance.ActiveMobs.Count >= 2);
            yield return CheckFlightAndCapture();
            yield return CheckFreezePauseAndDamage();
            yield return CheckPoolAndFinish();
            Check(JsonUtility.ToJson(GameManager.Instance.Config) == _savedConfig, "Original GameConfig remained unchanged");
            Check(JsonUtility.ToJson(_data.PlayerData) == _savedData, "Player inventory, progress, stars, theme and settings remained unchanged");
            CheckPreferences(); Status = "Returning to menu and restoring test context";
            CleanupRuntime();
            yield return (Func<bool>)(() => SceneManager.GetActiveScene().name == "MainMenuScene" &&
                (!SceneTransitionManager.HasInstance || !SceneTransitionManager.Instance.IsTransitioning));
            Restore();
            Check(JsonUtility.ToJson(_data.PlayerData) == _savedData, "Complete original PlayerData restored");
            CheckPreferences();
            Check(GameManager.Instance.CurrentState == GameState.Menu, "Normal game managers returned safely to menu");
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
                Finish("PASS: real enemy animation, movement, HP, freeze, pause, hit, death, pool reuse and finish");
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
            Steps.Clear(); _condition = null; Status = outcome; Directory.CreateDirectory(Folder);
            File.WriteAllText(Folder + "/runtime-verification.txt", outcome + "\n" + string.Join("\n", Results) + "\n");
            Debug.Log("[ArrowSwarm] " + outcome + " (" + Folder + "/runtime-verification.txt)");
        }

        private static void Check(bool condition, string message)
        { if (!condition) throw new InvalidOperationException(message); Results.Add("PASS: " + message); }
    }
}
