namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Captures real pooled Defense mob deaths in both themes without progress or service writes.</summary>
    public static partial class Revision6DeathVerification
    {
        private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
        private static readonly List<string> Results = new List<string>();
        private static Scope _scope;
        private static Func<bool> _wait;
        private static double _deadline;
        private static string _folder;

        /// <summary>Native-player-loop verification status for Unity MCP.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Starts a before or after run from the idle Boot-loaded MainMenu.</summary>
        public static void Begin(string label = "before")
        {
            if (_scope != null) throw new InvalidOperationException("Death verification already running.");
            _folder = "Captures/Revision6/Death/" + (label == "before" ? "before" : "after");
            Directory.CreateDirectory(_folder); Results.Clear(); Steps.Clear(); _wait = null;
            try { _scope = new Scope(); Steps.Push(Run()); }
            catch { _scope?.Dispose(); _scope = null; throw; }
            _deadline = EditorApplication.timeSinceStartup + 150; Status = "Loading real Defense101";
            EditorApplication.update += Tick; EditorApplication.playModeStateChanged += ModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += Cancel;
        }

        /// <summary>Cancels a fixture and restores its temporary player, services and timing.</summary>
        public static void Cancel() { if (_scope != null) Finish("ABORT: death verification cancelled"); }

        private static void Tick()
        {
            try
            {
                if (_scope == null) return;
                if (!Application.isPlaying || EditorApplication.timeSinceStartup > _deadline)
                    throw new TimeoutException(Status);
                EditorApplication.QueuePlayerLoopUpdate();
                if (_wait != null) { if (!_wait()) return; _wait = null; }
                while (Steps.Count > 0)
                {
                    var iterator = Steps.Peek();
                    if (!iterator.MoveNext()) { (iterator as IDisposable)?.Dispose(); Steps.Pop(); continue; }
                    if (iterator.Current is IEnumerator child) { Steps.Push(child); continue; }
                    _wait = iterator.Current as Func<bool>; return;
                }
                Finish("PASS: actual Defense deaths, paired particle captures, VFX gate and pool reuse");
            }
            catch (Exception error) { Finish("FAIL: " + error.Message); }
        }

        private static void ModeChanged(PlayModeStateChange state)
        { if (state == PlayModeStateChange.ExitingPlayMode) Cancel(); }

        private static void Finish(string outcome)
        {
            EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= ModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= Cancel;
            try
            {
                while (Steps.Count > 0) (Steps.Pop() as IDisposable)?.Dispose();
                _scope?.Dispose();
            }
            catch (Exception error) { outcome += " / RESTORE FAILED: " + error.Message; }
            _scope = null; _wait = null; Status = outcome;
            File.WriteAllText(_folder + "/gameplay-verification.txt", outcome + "\n" + string.Join("\n", Results) + "\n");
        }

        private static void Check(bool value, string text)
        { Results.Add((value ? "PASS: " : "FAIL: ") + text); if (!value) throw new InvalidOperationException(text); }

        private static Func<bool> Settled(float seconds = .2f)
        { float until = Time.unscaledTime + seconds; int frame = Time.frameCount; return () => Time.unscaledTime > until && Time.frameCount > frame + 2; }
    }
}
