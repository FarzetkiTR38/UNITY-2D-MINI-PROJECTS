namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Exercises only the Boot/Home rollout through the native player loop and authored UI controls.</summary>
    public static partial class UiRollout01Verification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
        private static readonly List<string> Results = new List<string>();
        private static Scope _scope;
        private static UiRollout01NativeFrameRunner _frameRunner;
        private static Func<bool> _wait;
        private static string _folder;
        private static double _deadline;
        private static bool _navigation;
        private static Sprite _cloudFooter;
        private static bool _finishing;
        private static string _finishOutcome;
        private static List<Exception> _finishFailures;
        private static double _finishDeadline;
        private static int _finishReadyFrame;

        /// <summary>Current native stage or final outcome, for nonblocking Unity MCP inspection.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Starts the complete check from a fresh, idle MainMenu reached through Boot.</summary>
        public static void Begin() => Start("Captures/UIRollout/Phase01/Checks", true);

        /// <summary>Repeats paired captures and typography checks at the caller's current Game View size.</summary>
        public static void CaptureAt(string folder) => Start(folder, false);

        private static void Start(string folder, bool navigation)
        {
            if (_scope != null || !Application.isPlaying || SceneManager.GetActiveScene().name != "MainMenuScene" ||
                !DataManager.HasInstance || !GameManager.HasInstance || LeaderboardManager.HasInstance ||
                !SceneTransitionManager.HasInstance || SceneTransitionManager.Instance.IsTransitioning)
                throw new InvalidOperationException("Use idle MainMenu Play reached through Boot, before opening the real leaderboard.");
            _folder = folder; _navigation = navigation; Directory.CreateDirectory(folder);
            _modalScreens = _nextModalScreens;
            Results.Clear(); Steps.Clear(); _wait = null; _cloudFooter = null; _textControlChecked = false;
            try { _scope = new Scope(); Steps.Push(Run()); _frameRunner = UiRollout01NativeFrameRunner.Begin(Tick); }
            catch { _scope?.Dispose(); _scope = null; throw; }
            _deadline = EditorApplication.timeSinceStartup + 300; Status = "Checking real Boot and Home";
            EditorApplication.update += Watchdog; AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
            EditorApplication.playModeStateChanged += ModeChanged;
        }

        private static IEnumerator Run()
        {
            yield return VerifyBoot();
            yield return VerifyHome();
            if (_modalScreens) yield return VerifyModals();
            if (_navigation) { yield return VerifyNavigation(); yield return VerifyPlayRoutes(); }
            Check(_scope.Cloud.Operations == 0 && _scope.Ads.Calls == 0, "No auth, score, profile, fetch or advertisement operation occurred");
        }

        /// <summary>Cancels safely before Play ends or scripts are reloaded.</summary>
        public static void Cancel() { if (_scope != null) Finish("ABORT: Boot/Home check cancelled", Application.isPlaying); }

        private static void BeforeReload()
        { if (_scope != null) Finish("ABORT: Boot/Home script reload", false); }

        // Screen/safe-area and Canvas assertions belong to the native player frame. In an Editor
        // update Unity can temporarily report a docked editor viewport instead of the Game View.
        private static void Watchdog()
        {
            if (_scope != null && !_finishing && (!Application.isPlaying || _frameRunner == null || EditorApplication.timeSinceStartup > _deadline))
                Finish("FAIL: Native verification stopped or timed out / " + Status, Application.isPlaying);
        }

        private static void Tick()
        {
            try
            {
                if (_scope == null) return;
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
                Finish("PASS: native Boot/Home, paired themes, TMP layout and scoped navigation");
            }
            catch (Exception error) { Results.Add(error.ToString()); Finish("FAIL: " + error.Message); }
        }

        private static void ModeChanged(PlayModeStateChange state)
        { if (state == PlayModeStateChange.ExitingPlayMode && _scope != null) Finish("ABORT: Boot/Home Play Mode exiting", false); }

        private static void Finish(string result, bool allowDeferred = true)
        {
            if (_finishing) { CompleteFinish(result, false); return; }
            EditorApplication.update -= Watchdog; AssemblyReloadEvents.beforeAssemblyReload -= BeforeReload;
            if (_frameRunner != null) { _frameRunner.End(!allowDeferred); _frameRunner = null; }
            EditorApplication.playModeStateChanged -= ModeChanged;
            var failures = new List<Exception>();
            try
            {
                while (Steps.Count > 0)
                {
                    var step = Steps.Pop() as IDisposable;
                    RestoreAttempt(failures, () => step?.Dispose(), "active verification iterator");
                }
            }
            finally
            {
                RestoreAttempt(failures, () => _scope?.Dispose(allowDeferred && Application.isPlaying), "original project/player scope");
                _wait = null;
            }
            _finishOutcome = result; _finishFailures = failures;
            if (allowDeferred && Application.isPlaying)
                BeginFinishWait();
            else CompleteFinish(result, false);
        }

        private static Func<bool> Settled(float seconds = .35f)
        { float until = Time.unscaledTime + seconds; int frame = Time.frameCount; return () => Time.unscaledTime > until && Time.frameCount > frame + 2; }

        private static T Field<T>(object owner, string name) => (T)owner.GetType().GetField(name, Private).GetValue(owner);
        private static void Check(bool value, string label)
        { Results.Add((value ? "PASS: " : "FAIL: ") + label); if (!value) throw new InvalidOperationException(label); }
    }
}
