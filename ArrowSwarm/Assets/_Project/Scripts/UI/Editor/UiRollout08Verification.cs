namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using ArrowSwarm.Ads;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Checks only Defense exhausted-skill modal presentation and original actions through native input.</summary>
    public static partial class UiRollout08Verification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly List<string> Results = new List<string>();
        private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
        private static string _folder;
        private static bool _running, _revised;
        private static Func<bool> _wait;
        private static Scope _scope;
        private static UiRollout01NativeFrameRunner _frameRunner;
        private static double _deadline;

        /// <summary>Current native verification stage or final outcome.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Starts from fresh Boot-loaded MainMenu; data, provider and preferences are scoped and restored.</summary>
        public static void Begin(string folder, bool revised)
        {
            if (_running || _finishing || _scope != null || !Application.isPlaying || SceneManager.GetActiveScene().name != "MainMenuScene" ||
                !DataManager.HasInstance || !GameManager.HasInstance || !AdManager.HasInstance || LeaderboardManager.HasInstance)
                throw new InvalidOperationException("Use fresh Boot-loaded idle MainMenu Play Mode before leaderboard access.");
            if (string.IsNullOrWhiteSpace(folder)) throw new ArgumentException("Provide an evidence folder.", nameof(folder));
            Results.Clear(); Steps.Clear(); _wait = null; _folder = folder; _revised = revised;
            Directory.CreateDirectory(_folder);
            try
            {
                _scope = new Scope(); _deadline = EditorApplication.timeSinceStartup + 300;
                _running = true; Status = "Loading Defense for board08 checks"; Steps.Push(Run());
                _frameRunner = UiRollout01NativeFrameRunner.Begin(Tick);
                EditorApplication.update += Watchdog; AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
                EditorApplication.playModeStateChanged += ModeChanged;
            }
            catch (Exception error) { Finish("FAIL: " + error.Message, false); throw; }
        }

        /// <summary>Stops the owned fixture and restores original state even during a pending input step.</summary>
        public static void Cancel() { if (_running || _finishing) Finish("ABORT: board08 verification cancelled"); }

        private static void BeforeReload()
        { if (_scope != null) Finish("ABORT: script reload", false); }

        private static void Watchdog()
        {
            if (!_running && !_finishing) return;
            EditorApplication.QueuePlayerLoopUpdate();
            if (_finishing)
            {
                if (!Application.isPlaying || _frameRunner == null || EditorApplication.timeSinceStartup > _finishDeadline)
                    CompleteFinish(_finishOutcome + " / CLEANUP: native MainMenu wait ended", false);
                return;
            }
            if (!Application.isPlaying || _frameRunner == null || EditorApplication.timeSinceStartup > _deadline)
                Finish("FAIL: Native frame stopped or timed out / " + Status);
        }

        private static void Tick()
        {
            if (_finishing) { TickFinish(); return; }
            if (!_running) return;
            try
            {
                if (!Application.isPlaying || EditorApplication.timeSinceStartup > _deadline) throw new TimeoutException(Status);
                EditorApplication.QueuePlayerLoopUpdate();
                if (_wait != null) { if (!_wait()) return; _wait = null; }
                while (Steps.Count > 0)
                {
                    var step = Steps.Peek();
                    if (!step.MoveNext()) { (Steps.Pop() as IDisposable)?.Dispose(); continue; }
                    if (step.Current is IEnumerator nested) { Steps.Push(nested); continue; }
                    _wait = step.Current as Func<bool>; return;
                }
                Finish("PASS: Defense hint/freeze modal visuals, localization, native controls and unchanged rewards");
            }
            catch (Exception error) { Results.Add(error.ToString()); Finish("FAIL: " + error.Message); }
        }

        private static void ModeChanged(PlayModeStateChange state)
        { if ((_running || _finishing) && state == PlayModeStateChange.ExitingPlayMode) Finish("ABORT: Play Mode exiting", false); }

        private static Func<bool> Settled(float seconds = .4f)
        { float end = Time.unscaledTime + seconds; int frame = Time.frameCount; return () => Time.unscaledTime > end && Time.frameCount > frame + 2; }

        private static IEnumerator Capture(string name)
        {
            string path = _folder + "/" + name + ".png"; int frame = Time.frameCount;
            if (File.Exists(path)) File.Delete(path);
            Canvas.ForceUpdateCanvases(); ScreenCapture.CaptureScreenshot(path);
            yield return (Func<bool>)(() => Time.frameCount > frame + 2 && File.Exists(path) && new FileInfo(path).Length > 0);
            Results.Add("CAPTURE: " + path + "; native=" + Screen.width + "x" + Screen.height);
        }

        private static T Field<T>(object owner, string name) => (T)owner.GetType().GetField(name, Private).GetValue(owner);
        private static void Check(bool value, string label)
        { Results.Add((value ? "PASS: " : "FAIL: ") + label); if (!value) throw new InvalidOperationException(label); }

        private static void ReturnToMenu()
        {
            if (ArrowSwarm.Skills.FreezeManager.HasInstance) ArrowSwarm.Skills.FreezeManager.Instance.EndFreezeSilently();
            if (LevelManager.HasInstance) LevelManager.Instance.CleanupLevel();
            Time.timeScale = 1;
            if (GameManager.HasInstance) GameManager.Instance.SetState(GameState.Menu);
            if (SceneManager.GetActiveScene().name != "MainMenuScene") SceneManager.LoadScene("MainMenuScene");
        }
    }
}
