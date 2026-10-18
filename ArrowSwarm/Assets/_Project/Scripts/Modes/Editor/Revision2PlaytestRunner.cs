namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Data;
    using ArrowSwarm.Puzzle;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Exercises real preview scenes with isolated cloned tuning and no save or ad calls.</summary>
    public static partial class Revision2PlaytestRunner
    {
        private const string Report = "Captures/Revision2/interaction-verification.txt";
        private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
        private static readonly List<string> Results = new List<string>();
        private static DataManager _data;
        private static string _savedData, _savedPrefs, _savedBackup, _savedLanguagePrefs;
        private static bool _savedAutoSave, _savedPreview, _running;
        private static GameModeId _savedPreviewMode;
        private static int _savedPreviewLevel;
        private static float _savedTime;
        private static double _until, _deadline;
        private static Func<bool> _condition;
        private static PuzzleSession _session;
        private static PuzzleConfig _originalConfig, _testConfig;
        public static string Status { get; private set; } = "Idle";

        /// <summary>Runs isolated Rocket, frame, gesture, recovery and production timing checks.</summary>
        [MenuItem("Arrow Swarm/Puzzles/Verify Revision 2 Interactions (Play Mode Preview)")]
        public static void Begin()
        {
            if (_running) throw new InvalidOperationException("Revision 2 verification is already running.");
            if (!Application.isPlaying || PuzzleSession.Active == null || !PuzzleSession.Active.Preview)
                throw new InvalidOperationException("Start Play Mode in a puzzle preview first.");
            _data = DataManager.Instance; _savedData = JsonUtility.ToJson(_data.PlayerData);
            _savedPrefs = PlayerPrefs.GetString("ArrowSwarm_PlayerData", "");
            _savedBackup = PlayerPrefs.GetString("ArrowSwarm_PlayerData_Backup", "");
            _savedLanguagePrefs = PlayerPrefs.GetString("ArrowSwarm_SelectedLanguage", "");
            var serialized = new SerializedObject(_data);
            _savedAutoSave = serialized.FindProperty("_autoSaveOnChange").boolValue;
            serialized.FindProperty("_autoSaveOnChange").boolValue = false; serialized.ApplyModifiedPropertiesWithoutUndo();
            _savedPreview = PuzzleLaunch.Preview; _savedPreviewMode = PuzzleLaunch.PreviewMode;
            _savedPreviewLevel = PuzzleLaunch.PreviewLevel; _savedTime = Time.timeScale;
            _running = true; _condition = null; _until = 0; _deadline = EditorApplication.timeSinceStartup + 300;
            Directory.CreateDirectory("Captures/Revision2"); Results.Clear(); Steps.Clear(); Steps.Push(Run());
            Status = "Starting Revision 2 preview verification";
            EditorApplication.update += Tick; EditorApplication.playModeStateChanged += ChangedPlayMode;
        }

        private static IEnumerator Run()
        {
            foreach (var mode in new[] { GameModeId.Shapes, GameModeId.ColorShift })
            {
                Status = "Loading " + mode;
                PuzzleLaunch.Preview = true; PuzzleLaunch.PreviewMode = mode; PuzzleLaunch.PreviewLevel = 1;
                SceneManager.LoadScene(mode == GameModeId.Shapes ? "ShapesScene" : "ColorShiftScene");
                yield return (Func<bool>)(() => PuzzleSession.Active != null && PuzzleSession.Active.Level != null && PuzzleSession.Active.Mode == mode);
                yield return .15f; _session = PuzzleSession.Active;
                Check(_session.Preview, mode + ": all checks remain in preview");
                Check(_session.RocketCharges == _session.Config.RocketCharges, mode + ": scene uses configured initial rocket charges");
                _originalConfig = _session.Config; _testConfig = UnityEngine.Object.Instantiate(_originalConfig);
                _testConfig.hideFlags = HideFlags.DontSave;
                Tune("_rocketCharges", 2); Tune("_rocketDuration", .6f); Tune("_rotationDuration", 1f);
                BindConfig(_session, _testConfig); _session.Retry();
                yield return RocketChecks(_session);
                if (mode == GameModeId.ColorShift) yield return FrameChecks(_session);
                yield return RecoveryAndFlightChecks(_session);
                if (mode == GameModeId.Shapes) yield return RapidFireChecks(_session);
                yield return LastArrowRocket(_session);
                Check(JsonUtility.ToJson(_data.PlayerData) == _savedData, mode + ": no campaign or inventory writes");
                RestoreConfig();
            }
            Check(PlayerPrefs.GetString("ArrowSwarm_PlayerData", "") == _savedPrefs &&
                PlayerPrefs.GetString("ArrowSwarm_PlayerData_Backup", "") == _savedBackup &&
                PlayerPrefs.GetString("ArrowSwarm_SelectedLanguage", "") == _savedLanguagePrefs,
                "Campaign save, backup and language preferences unchanged");
        }

        private static void Tick()
        {
            if (!_running) return;
            try
            {
                if (!Application.isPlaying) { Finish("ABORT: Play Mode ended"); return; }
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
                Finish("PASS: Rocket, flowing rounded frame and real gesture interactions in both preview modes");
            }
            catch (Exception exception) { Finish("FAIL: " + exception.Message); Debug.LogException(exception); }
        }

        private static void ChangedPlayMode(PlayModeStateChange state)
        { if (_running && state == PlayModeStateChange.ExitingPlayMode) Finish("ABORT: Play Mode exiting"); }

        private static void Finish(string outcome)
        {
            _running = false; EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= ChangedPlayMode;
            try
            {
                while (Steps.Count > 0) (Steps.Pop() as IDisposable)?.Dispose();
                ReleasePointer(); RestoreConfig();
                if (_data != null)
                {
                    JsonUtility.FromJsonOverwrite(_savedData, _data.PlayerData);
                    var serialized = new SerializedObject(_data);
                    serialized.FindProperty("_autoSaveOnChange").boolValue = _savedAutoSave; serialized.ApplyModifiedPropertiesWithoutUndo();
                }
                PuzzleLaunch.Preview = _savedPreview; PuzzleLaunch.PreviewMode = _savedPreviewMode;
                PuzzleLaunch.PreviewLevel = _savedPreviewLevel; Time.timeScale = _savedTime;
                Results.Add("RESTORED: original tuning assets, player data, auto-save, preview fields and time scale");
            }
            catch (Exception exception) { outcome += " / RESTORE FAILED: " + exception.Message; }
            Steps.Clear(); _condition = null; Status = outcome;
            File.WriteAllText(Report, outcome + "\n" + string.Join("\n", Results) + "\n");
            Debug.Log("[ArrowSwarm] " + outcome + " (" + Report + ")");
        }
        private static void Check(bool condition, string text)
        { if (!condition) throw new Exception(text); Results.Add("PASS: " + text); }
    }
}
