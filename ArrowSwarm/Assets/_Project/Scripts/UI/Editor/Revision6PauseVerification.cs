namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Puzzle;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Verifies original Pause artwork and real pause/resume input across all four gameplay scenes.</summary>
    public static partial class Revision6PauseVerification
    {
        private static string Folder = "Captures/Revision6/Checks/Pause";
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly List<string> Results = new List<string>();
        private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
        private static Func<bool> _wait;
        private static Scope _scope;
        private static double _deadline;
        private static bool _running;

        /// <summary>Current stage or final result for nonblocking Unity MCP inspection.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Starts from a fresh Boot-loaded idle main menu, without a live leaderboard manager.</summary>
        public static void Begin()
        {
            if (_running || !Application.isPlaying || SceneManager.GetActiveScene().name != "MainMenuScene" ||
                !DataManager.HasInstance || !GameManager.HasInstance || LeaderboardManager.HasInstance)
                throw new InvalidOperationException("Use fresh MainMenu Play Mode before opening the leaderboard.");
            Results.Clear(); Steps.Clear(); _wait = null; Directory.CreateDirectory(Folder);
            try
            {
                _scope = new Scope(); _deadline = EditorApplication.timeSinceStartup + 240;
                _running = true; Status = "Verifying original Pause sprite GPU output"; Steps.Push(Run());
                EditorApplication.update += Tick; AssemblyReloadEvents.beforeAssemblyReload += Cancel;
                EditorApplication.playModeStateChanged += ModeChanged; SceneManager.sceneLoaded += ConfigurePreview;
            }
            catch (Exception error) { Finish("FAIL: " + error.Message); throw; }
        }

        /// <summary>Cancels and restores only fixture-owned data, preferences, debug and input context.</summary>
        public static void Cancel() { if (_running) Finish("ABORT: Pause verification cancelled"); }

        private static IEnumerator Run()
        {
            if (_rollout0608) { yield return Run0608(); yield break; }
            VerifyPixels();
            foreach (string scene in new[] { "GameScene", "ShapesScene", "ColorShiftScene", "MapTestScene" })
                yield return VerifyScene(scene);
            Check(!LeaderboardManager.HasInstance && _scope.FactoryCalls == 0, "No cloud manager or provider operation was started");
            ReturnToMenu(); yield return Settled(.2f);
        }

        private static void Tick()
        {
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
                Finish(_rollout0608 ? "PASS: Defense pause and skill modal native checks" : "PASS: original Pause artwork, native input and resume in all four gameplay scenes");
            }
            catch (Exception error) { Results.Add(error.ToString()); Finish("FAIL: " + error.Message); }
        }

        private static void ModeChanged(PlayModeStateChange state)
        { if (_running && state == PlayModeStateChange.ExitingPlayMode) Finish("ABORT: Play Mode exiting"); }

        private static void Finish(string outcome)
        {
            _running = false; EditorApplication.update -= Tick; AssemblyReloadEvents.beforeAssemblyReload -= Cancel;
            EditorApplication.playModeStateChanged -= ModeChanged; SceneManager.sceneLoaded -= ConfigurePreview;
            try { while (Steps.Count > 0) (Steps.Pop() as IDisposable)?.Dispose(); if (Application.isPlaying) ReturnToMenu(); }
            catch (Exception error) { outcome += " / CLEANUP FAILED: " + error.Message; }
            finally { try { _scope?.Dispose(); } catch (Exception error) { outcome += " / RESTORE FAILED: " + error.Message; } }
            _scope = null; _wait = null; Status = outcome;
            File.WriteAllText(Folder + "/verification.txt", outcome + "\n" + string.Join("\n", Results) + "\n");
            UnityEngine.Debug.Log("[ArrowSwarm] " + outcome);
        }

        private static Func<bool> Settled(float seconds = .4f)
        { float end = Time.unscaledTime + seconds; int frame = Time.frameCount; return () => Time.unscaledTime > end && Time.frameCount > frame + 2; }

        private static IEnumerator Capture(string name)
        {
            string path = Folder + "/" + name + ".png"; int frame = Time.frameCount;
            if (File.Exists(path)) File.Delete(path);
            Canvas.ForceUpdateCanvases(); ScreenCapture.CaptureScreenshot(path);
            yield return (Func<bool>)(() => Time.frameCount > frame + 2 && File.Exists(path) && new FileInfo(path).Length > 0);
            Results.Add("CAPTURE: " + path);
        }

        private static T Field<T>(object owner, string name) => (T)owner.GetType().GetField(name, Private).GetValue(owner);
        private static void Check(bool value, string label)
        { Results.Add((value ? "PASS: " : "FAIL: ") + label); if (!value) throw new InvalidOperationException(label); }
    }
}
