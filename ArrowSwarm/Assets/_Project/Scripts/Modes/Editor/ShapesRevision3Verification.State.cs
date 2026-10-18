namespace ArrowSwarm.Modes.Editor
{
    using System.Reflection;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Puzzle;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Restores temporary verification changes without touching authored assets or campaign saves.</summary>
    public static partial class ShapesRevision3Verification
    {
        private static DataManager _data;
        private static PuzzleConfig _testConfig, _originalConfig;
        private static string _savedData, _savedPrefs, _savedBackup;
        private static bool _savedAutoSave, _savedPreview, _savedBackground;
        private static GameModeId _savedPreviewMode;
        private static int _savedPreviewLevel;
        private static float _savedTimeScale;
        private static ThemeMode _savedTheme;

        private static void Snapshot()
        {
            _data = DataManager.Instance; _savedData = JsonUtility.ToJson(_data.PlayerData);
            _savedTheme = _data.PlayerData.theme;
            _savedPrefs = PlayerPrefs.GetString("ArrowSwarm_PlayerData", "");
            _savedBackup = PlayerPrefs.GetString("ArrowSwarm_PlayerData_Backup", "");
            var serialized = new SerializedObject(_data);
            _savedAutoSave = serialized.FindProperty("_autoSaveOnChange").boolValue;
            serialized.FindProperty("_autoSaveOnChange").boolValue = false; serialized.ApplyModifiedPropertiesWithoutUndo();
            _savedPreview = PuzzleLaunch.Preview; _savedPreviewMode = PuzzleLaunch.PreviewMode;
            _savedPreviewLevel = PuzzleLaunch.PreviewLevel; _savedTimeScale = Time.timeScale;
            _savedBackground = Application.runInBackground;
        }

        private static void Accelerate(PuzzleSession session)
        {
            _originalConfig = session.Config;
            _testConfig = Object.Instantiate(_originalConfig); _testConfig.hideFlags = HideFlags.HideAndDontSave;
            var config = new SerializedObject(_testConfig);
            // Keep flights visible for several runtime frames while rapidly exercising whole campaigns.
            config.FindProperty("_speed").floatValue = 200;
            config.FindProperty("_maximumFlightDuration").floatValue = .2f;
            config.ApplyModifiedPropertiesWithoutUndo();
            var serialized = new SerializedObject(session);
            serialized.FindProperty("_config").objectReferenceValue = _testConfig; serialized.ApplyModifiedPropertiesWithoutUndo();
            Time.timeScale = 1; Application.runInBackground = true;
        }

        private static void VerifyNoSaveWrites()
        {
            var theme = _data.PlayerData.theme; _data.PlayerData.theme = _savedTheme;
            string current = JsonUtility.ToJson(_data.PlayerData); _data.PlayerData.theme = theme;
            Require(current == _savedData, "preview changed player progress or inventory");
            Require(PlayerPrefs.GetString("ArrowSwarm_PlayerData", "") == _savedPrefs, "preview wrote campaign JSON");
            Require(PlayerPrefs.GetString("ArrowSwarm_PlayerData_Backup", "") == _savedBackup, "preview wrote backup JSON");
        }

        private static void SetTheme(ThemeMode theme)
        {
            _data.PlayerData.theme = theme; ThemeManager.Instance.ApplyTheme(theme); Canvas.ForceUpdateCanvases();
        }

        private static void Restore()
        {
            var session = PuzzleSession.Active;
            if (session != null && _testConfig != null && session.Config == _testConfig)
            {
                var serialized = new SerializedObject(session);
                serialized.FindProperty("_config").objectReferenceValue = _originalConfig; serialized.ApplyModifiedPropertiesWithoutUndo();
                RestoreRuntimeConfig(Field<PuzzleFrame>(session, "_frame"));
                var root = Field<Transform>(session, "_arrowRoot");
                if (root != null) foreach (var view in root.GetComponentsInChildren<PuzzleArrowView>(true)) RestoreRuntimeConfig(view);
            }
            if (_testConfig != null) Object.DestroyImmediate(_testConfig);
            _testConfig = null; _originalConfig = null;
            if (_data != null)
            {
                JsonUtility.FromJsonOverwrite(_savedData, _data.PlayerData);
                var serialized = new SerializedObject(_data);
                serialized.FindProperty("_autoSaveOnChange").boolValue = _savedAutoSave; serialized.ApplyModifiedPropertiesWithoutUndo();
                ThemeManager.Instance.ApplyTheme(_savedTheme);
            }
            PuzzleLaunch.Preview = _savedPreview; PuzzleLaunch.PreviewMode = _savedPreviewMode;
            PuzzleLaunch.PreviewLevel = _savedPreviewLevel;
            Time.timeScale = _savedTimeScale; Application.runInBackground = _savedBackground;
        }

        private static void RestoreRuntimeConfig(Object source)
        {
            if (source == null) return;
            var field = source.GetType().GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null && ReferenceEquals(field.GetValue(source), _testConfig)) field.SetValue(source, _originalConfig);
        }

        private static T Field<T>(Object source, string name) where T : Object =>
            new SerializedObject(source).FindProperty(name).objectReferenceValue as T;
        private static int ActiveShotCount(PuzzleSession session) => (int)typeof(PuzzleSession)
            .GetField("_activeShots", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(session);
    }
}
