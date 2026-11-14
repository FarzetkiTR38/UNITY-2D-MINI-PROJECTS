namespace ArrowSwarm.Marketing.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Puzzle;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Editor-only recorder of actual rendered gameplay and production actions.</summary>
    [InitializeOnLoad]
    public static partial class MarketingCapture
    {
        private const string Root = "ArrowSwarm_Marketing/04_Raw_Gameplay";
        private const string Armed = "ArrowSwarm.Marketing.Armed";
        private static readonly List<string> Events = new List<string>();
        private static readonly Stack<IEnumerator> Routines = new Stack<IEnumerator>();
        private static string _folder;
        private static int _frame;
        private static bool _recording;
        private static CaptureScope _scope;
        private static int _firstShotHoldFrames;
        private static string _captureLanguage;
        /// <summary>Current recording progress, without private save contents.</summary>
        public static string Status { get; private set; } = "Idle";

        static MarketingCapture()
        {
            EditorApplication.playModeStateChanged += OnPlayState;
            AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
            EditorApplication.quitting += BeforeReload;
        }

        /// <summary>Backs up private preferences before Play; never silently saves an unsaved scene.</summary>
        public static void Arm()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Arm in Edit Mode with no unsaved scene.");
            if (SessionState.GetBool(Armed, false))
                throw new InvalidOperationException("Already armed; call Recover after any interrupted capture first.");
            SavePreferences();
            SessionState.SetInt(Armed + ".Fps", Time.captureFramerate);
            SessionState.SetBool(Armed + ".Background", Application.runInBackground);
            SessionState.SetBool(Armed, true);
            Status = "Armed";
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void BeforeBootstrap()
        {
            if (SessionState.GetBool(Armed, false)) InstallOffline();
        }

        /// <summary>Starts a 30fps authored-puzzle capture after Arm and initialized Play startup.</summary>
        public static void Begin(int level, string mode = "Shapes", int seconds = 20,
            int firstShotHoldFrames = 0, string language = "en")
        {
            if (!Application.isPlaying || !SessionState.GetBool(Armed, false) || !DataManager.HasInstance)
                throw new InvalidOperationException("Arm before Play, then wait for initialized MainMenu.");
            if (_recording) throw new InvalidOperationException("Already recording.");
            if (mode != "Shapes" && mode != "ColorShift") throw new ArgumentException("Unsupported puzzle mode.", nameof(mode));
            if (level < 1) throw new ArgumentOutOfRangeException(nameof(level));
            if (seconds < 1 || seconds > 40) throw new ArgumentOutOfRangeException(nameof(seconds));
            if (firstShotHoldFrames < 0 || (firstShotHoldFrames > 0 && firstShotHoldFrames > seconds * 30 - 180))
                throw new ArgumentOutOfRangeException(nameof(firstShotHoldFrames));
            if (firstShotHoldFrames > 0 && mode != "Shapes")
                throw new ArgumentException("Isolated-first-shot capture currently requires Shapes.", nameof(mode));
            if (string.IsNullOrEmpty(language)) throw new ArgumentException("Language is required.", nameof(language));
            _firstShotHoldFrames = firstShotHoldFrames; _captureLanguage = language;
            _folder = Root + "/" + mode + "_L" + level + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
            Directory.CreateDirectory(_folder); Events.Clear(); Events.Add("frame,time_seconds,event,arrow,remaining,lives");
            _recording = true; Status = "Loading " + _folder;
            ArrowSwarm.Audio.AudioManager.Instance.StartCoroutine(Drive(Record(level, mode, seconds)));
        }

        private static IEnumerator Drive(IEnumerator routine)
        {
            Routines.Push(routine);
            try
            {
                while (Routines.Count > 0)
                {
                    bool moved = false; object current = null; Exception failure = null;
                    try { moved = Routines.Peek().MoveNext(); if (moved) current = Routines.Peek().Current; }
                    catch (Exception error) { failure = error; }
                    if (failure != null)
                    {
                        Status = "FAIL: " + failure.Message;
                        File.WriteAllText(_folder + "/failure.txt", failure.ToString());
                        yield break;
                    }
                    if (!moved) { (Routines.Pop() as IDisposable)?.Dispose(); continue; }
                    if (current is IEnumerator child) Routines.Push(child); else yield return current;
                }
            }
            finally { CleanupRecording(); }
        }

        private static IEnumerator Record(int level, string mode, int seconds)
        {
            _scope = new CaptureScope();
            ArrowSwarm.Localization.LocalizationManager.Instance.SetLanguage(_captureLanguage, false);
            var modeId = mode == "ColorShift" ? GameModeId.ColorShift : GameModeId.Shapes;
            PuzzleLaunch.Preview = true; PuzzleLaunch.PreviewMode = modeId; PuzzleLaunch.PreviewLevel = level;
            SceneManager.LoadScene(modeId == GameModeId.Shapes ? "ShapesScene" : "ColorShiftScene");
            yield return null; yield return null;
            var session = PuzzleSession.Active;
            if (session == null || session.Level == null) throw new InvalidOperationException("Puzzle not ready.");
            yield return new WaitForSecondsRealtime(1);
            if (Screen.width != 1080 || Screen.height != 1920) throw new InvalidOperationException("Set native Game View to1080x1920.");
            var solution = PuzzleSolver.Solve(session.Level);
            if (!solution.Solvable) throw new InvalidOperationException("No legal witness.");
            ExportGeometry(session, solution);
            int step = 0, nextShot = 75, firstSettled = -1;
            bool winLogged = false;
            int cadence = Mathf.Clamp((seconds * 30 - 120 - _firstShotHoldFrames) / Mathf.Max(1, solution.Steps.Count), 5, 22);
            Time.captureFramerate = 30;
            var endFrame = new WaitForEndOfFrame();
            for (_frame = 0; _frame < seconds * 30; _frame++)
            {
                if (session.Won && !winLogged)
                {
                    winLogged = true;
                    Events.Add(_frame + "," + (_frame / 30f).ToString("F3", System.Globalization.CultureInfo.InvariantCulture) +
                        ",won,-1," + session.Remaining + "," + session.Lives);
                }
                if (_firstShotHoldFrames > 0 && step == 1 && firstSettled < 0 && session.Idle)
                {
                    if (session.Remaining != session.Level.Arrows.Count - 1 || session.Lives != session.Config.Lives)
                        throw new InvalidOperationException("The isolated first shot did not complete cleanly.");
                    firstSettled = _frame;
                    Events.Add(_frame + "," + (_frame / 30f).ToString("F3", System.Globalization.CultureInfo.InvariantCulture) +
                        ",first_settled," + solution.Steps[0] + "," + session.Remaining + "," + session.Lives);
                    nextShot = Mathf.Max(nextShot, firstSettled + 30);
                }
                bool firstShotFinished = _firstShotHoldFrames == 0 || step != 1 || (firstSettled >= 0 && session.Idle);
                if (_frame >= nextShot && session.Ready && firstShotFinished && step < solution.Steps.Count)
                {
                    int action = solution.Steps[step++];
                    if (action < 0) session.Rotate(1); else session.Fire(action);
                    Events.Add(_frame + "," + (_frame / 30f).ToString("F3", System.Globalization.CultureInfo.InvariantCulture) + "," +
                        (action < 0 ? "rotate" : "fire") + "," + action + "," + session.Remaining + "," + session.Lives);
                    nextShot = _frame + (step == 1 && _firstShotHoldFrames > 0 ? _firstShotHoldFrames : cadence);
                }
                yield return endFrame;
                var texture = ScreenCapture.CaptureScreenshotAsTexture();
                try
                {
                    if (texture == null || texture.width != 1080 || texture.height != 1920)
                        throw new InvalidOperationException("Rendered frame dimensions do not match the configured Game View.");
                    File.WriteAllBytes(_folder + "/frame_" + _frame.ToString("D5") + ".jpg", texture.EncodeToJPG(96));
                    if (_frame == 0) File.WriteAllBytes(_folder + "/source.png", texture.EncodeToPNG());
                }
                finally { if (texture != null) UnityEngine.Object.Destroy(texture); }
                if (_frame % 30 == 0) Status = "Recording " + _frame + "/" + seconds * 30;
            }
            File.WriteAllLines(_folder + "/events.csv", Events);
            File.WriteAllText(_folder + "/provenance.json", JsonUtility.ToJson(new Provenance
            { scene = SceneManager.GetActiveScene().path, level = level, mode = mode, frames = _frame,
                width = Screen.width, height = Screen.height, fps = 30, arrows = session.Level.Arrows.Count,
                remaining = session.Remaining, won = session.Won, levelAsset = AssetDatabase.GetAssetPath(session.Level),
                language = _captureLanguage, firstShotHoldFrames = _firstShotHoldFrames, firstShotSettledFrame = firstSettled }, true));
            // Unload the temporary board before restoring transient launch state and user data.
            SceneManager.LoadScene("MainMenuScene"); yield return null;
            Status = "PASS: " + _folder;
        }

        [Serializable] private sealed class Provenance
        {
            public string scene, mode, levelAsset, language;
            public int level, frames, width, height, fps, arrows, remaining, firstShotHoldFrames, firstShotSettledFrame;
            public bool won;
        }
    }
}
