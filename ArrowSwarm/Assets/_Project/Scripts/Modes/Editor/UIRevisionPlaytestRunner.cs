namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.Puzzle;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Focused real-UI checks in non-saving preview sessions; no ads or settings writes.</summary>
    public static partial class UIRevisionPlaytestRunner
    {
        private const string ReportPath = "Captures/UIRevision/ui-verification.txt";
        private static readonly List<string> Results = new List<string>();
        private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
        private static DataManager _data;
        private static LocalizationManager _localization;
        private static string _savedData, _savedLanguage, _savedPrefs, _savedLanguagePrefs;
        private static bool _savedAutoSave, _savedPreview, _running;
        private static GameModeId _savedPreviewMode;
        private static int _savedPreviewLevel;
        private static float _savedTimeScale;
        private static double _until, _deadline;
        private static Func<bool> _condition;
        public static string Status { get; private set; } = "Idle";

        [MenuItem("Arrow Swarm/Puzzles/Verify Revised UI (Play Mode Preview)")]
        public static void Begin()
        {
            if (_running) throw new InvalidOperationException("A UI verification is already running.");
            if (!Application.isPlaying || PuzzleSession.Active == null || !PuzzleSession.Active.Preview)
                throw new InvalidOperationException("Enter Play Mode in a puzzle preview before verifying UI.");
            _data = DataManager.Instance; _localization = LocalizationManager.Instance;
            _savedData = JsonUtility.ToJson(_data.PlayerData); _savedLanguage = _localization.CurrentLanguage;
            _savedPrefs = PlayerPrefs.GetString("ArrowSwarm_PlayerData", "");
            _savedLanguagePrefs = PlayerPrefs.GetString("ArrowSwarm_SelectedLanguage", "");
            var serialized = new SerializedObject(_data);
            _savedAutoSave = serialized.FindProperty("_autoSaveOnChange").boolValue;
            serialized.FindProperty("_autoSaveOnChange").boolValue = false; serialized.ApplyModifiedPropertiesWithoutUndo();
            _savedPreview = PuzzleLaunch.Preview; _savedPreviewMode = PuzzleLaunch.PreviewMode;
            _savedPreviewLevel = PuzzleLaunch.PreviewLevel; _savedTimeScale = Time.timeScale;
            _running = true; _until = 0; _condition = null; _deadline = EditorApplication.timeSinceStartup + 180;
            Results.Clear(); Steps.Clear(); Steps.Push(Run()); Directory.CreateDirectory("Captures/UIRevision");
            Status = "Running revised UI checks";
            EditorApplication.update += Tick; EditorApplication.playModeStateChanged += PlayModeChanged;
        }

        private static IEnumerator Run()
        {
            foreach (var mode in new[] { GameModeId.Shapes, GameModeId.ColorShift })
            {
                Status = "Loading " + mode + " preview";
                PuzzleLaunch.Preview = true; PuzzleLaunch.PreviewMode = mode; PuzzleLaunch.PreviewLevel = 1;
                SceneManager.LoadScene(mode == GameModeId.Shapes ? "ShapesScene" : "ColorShiftScene");
                yield return (Func<bool>)(() => PuzzleSession.Active != null && PuzzleSession.Active.Level != null && PuzzleSession.Active.Mode == mode);
                yield return .15f;
                var session = PuzzleSession.Active;
                Check(session.Preview, mode + ": non-saving preview");
                yield return CheckMode(session);
            }
            Check(JsonUtility.ToJson(_data.PlayerData) == _savedData, "PlayerData unchanged throughout preview tests");
            Check(PlayerPrefs.GetString("ArrowSwarm_PlayerData", "") == _savedPrefs, "Saved campaign JSON unchanged");
            Check(PlayerPrefs.GetString("ArrowSwarm_SelectedLanguage", "") == _savedLanguagePrefs, "Saved language preference unchanged");
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
                    object value = iterator.Current;
                    if (value is IEnumerator nested) { Steps.Push(nested); continue; }
                    if (value is float delay) _until = EditorApplication.timeSinceStartup + delay;
                    else if (value is Func<bool> condition) _condition = condition;
                    return;
                }
                Finish("PASS: revised UI interaction and isolation checks in both puzzle modes");
            }
            catch (Exception exception) { Finish("FAIL: " + exception.Message); Debug.LogException(exception); }
        }

        private static void PlayModeChanged(PlayModeStateChange state)
        { if (state == PlayModeStateChange.ExitingPlayMode && _running) Finish("ABORT: Play Mode exiting"); }

        private static void Finish(string outcome)
        {
            _running = false; EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= PlayModeChanged;
            try
            {
                while (Steps.Count > 0) (Steps.Pop() as IDisposable)?.Dispose();
                ReleasePointer();
                if (_localization != null) _localization.SetLanguage(_savedLanguage, saveToPrefs: false);
                if (_data != null)
                {
                    JsonUtility.FromJsonOverwrite(_savedData, _data.PlayerData);
                    var serialized = new SerializedObject(_data);
                    serialized.FindProperty("_autoSaveOnChange").boolValue = _savedAutoSave;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
                PuzzleLaunch.Preview = _savedPreview; PuzzleLaunch.PreviewMode = _savedPreviewMode;
                PuzzleLaunch.PreviewLevel = _savedPreviewLevel; Time.timeScale = _savedTimeScale;
                Results.Add("RESTORED: PlayerData, auto-save policy, language, time scale and preview launch fields");
            }
            catch (Exception exception) { outcome += " / RESTORE FAILED: " + exception.Message; }
            Steps.Clear(); _condition = null; Status = outcome;
            File.WriteAllText(ReportPath, outcome + "\n" + string.Join("\n", Results) + "\n");
            Debug.Log("[ArrowSwarm] " + outcome + " (" + ReportPath + ")");
        }

        private static void Check(bool value, string message)
        { if (!value) throw new Exception(message); Results.Add("PASS: " + message); }
    }
}
