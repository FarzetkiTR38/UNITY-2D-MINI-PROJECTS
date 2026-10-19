namespace ArrowSwarm.Modes.Editor
{
    using System.Linq;
    using ArrowSwarm.Data;
    using ArrowSwarm.Puzzle;
    using UnityEditor;
    using UnityEngine;

    public static partial class PuzzlePlaytestRunner
    {
        private static DataManager _data;
        private static PuzzleConfig _testConfig, _originalConfig;
        private static string _savedData, _savedPrefs, _savedBackup;
        private static bool _savedAutoSave, _savedPreview, _savedBackground;
        private static GameModeId _savedPreviewMode;
        private static int _savedPreviewLevel;
        private static float _savedTimeScale;

        private static void Snapshot()
        {
            _data = DataManager.Instance; _savedData = JsonUtility.ToJson(_data.PlayerData);
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
            ReleaseConfig(); _originalConfig = session.Config;
            _testConfig = Object.Instantiate(_originalConfig); _testConfig.hideFlags = HideFlags.HideAndDontSave;
            var config = new SerializedObject(_testConfig);
            config.FindProperty("_speed").floatValue = 1000;
            config.FindProperty("_rotationDuration").floatValue = .01f; config.ApplyModifiedPropertiesWithoutUndo();
            var serialized = new SerializedObject(session);
            serialized.FindProperty("_config").objectReferenceValue = _testConfig; serialized.ApplyModifiedPropertiesWithoutUndo();
            Time.timeScale = 10; Application.runInBackground = true;
        }

        private static void VerifyNoSaveWrites()
        {
            Require(_data != null && JsonUtility.ToJson(_data.PlayerData) == _savedData, "preview changed player progress");
            Require(PlayerPrefs.GetString("ArrowSwarm_PlayerData", "") == _savedPrefs, "preview wrote saved campaign JSON");
            Require(PlayerPrefs.GetString("ArrowSwarm_PlayerData_Backup", "") == _savedBackup, "preview wrote backup campaign JSON");
        }

        private static void VerifyPresentation(PuzzleSession session)
        {
            VerifyFrameParity(session);
            var roots = session.gameObject.scene.GetRootGameObjects();
            var surfaces = roots.SelectMany(root => root.GetComponentsInChildren<PuzzleBoardSurface>(true)).ToArray();
            Require(surfaces.Length == 1, "expected exactly one puzzle grid surface");
            var cards = surfaces[0].GetComponentsInChildren<SpriteRenderer>(true);
            Require(cards.Length == 1 && cards[0].name == "Layer3_InnerGrid" && cards[0].enabled && cards[0].sprite != null,
                "puzzle surface must render only its inner grid card");
            Require(!roots.SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Any(item => item.name.Contains("OuterCard") || item.name == "Cream Backdrop"),
                "puzzle scene contains a Defense outer card or full-board backdrop");
        }

        private static void VerifyFrameParity(PuzzleSession session)
        {
            if (session.Mode != GameModeId.ColorShift) return;
            var frame = Field<PuzzleFrame>(session, "_frame");
            Require(frame != null && frame.gameObject.activeInHierarchy && !frame.IsAnimating &&
                frame.VisualOrientation == (session.Orientation & 1), "frame visual/logical orientation differs while idle");
        }

        private static void ReleaseConfig()
        {
            var session = PuzzleSession.Active;
            if (session != null && _testConfig != null && session.Config == _testConfig && _originalConfig != null)
            {
                var serialized = new SerializedObject(session);
                serialized.FindProperty("_config").objectReferenceValue = _originalConfig; serialized.ApplyModifiedPropertiesWithoutUndo();
                RestoreRuntimeConfig(Field<PuzzleFrame>(session, "_frame"));
                var root = Field<Transform>(session, "_arrowRoot");
                if (root != null) foreach (var view in root.GetComponentsInChildren<PuzzleArrowView>(true)) RestoreRuntimeConfig(view);
            }
            if (_testConfig != null) Object.DestroyImmediate(_testConfig);
            _testConfig = null; _originalConfig = null;
        }

        private static void RestoreRuntimeConfig(Object target)
        {
            if (target == null) return;
            var field = target.GetType().GetField("_config", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field != null && ReferenceEquals(field.GetValue(target), _testConfig)) field.SetValue(target, _originalConfig);
        }

        private static void Restore()
        {
            ReleaseConfig();
            if (_data != null)
            {
                if (JsonUtility.ToJson(_data.PlayerData) != _savedData) JsonUtility.FromJsonOverwrite(_savedData, _data.PlayerData);
                var serialized = new SerializedObject(_data);
                serialized.FindProperty("_autoSaveOnChange").boolValue = _savedAutoSave; serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            PuzzleLaunch.Preview = _savedPreview; PuzzleLaunch.PreviewMode = _savedPreviewMode;
            PuzzleLaunch.PreviewLevel = _savedPreviewLevel;
            Time.timeScale = _savedTimeScale; Application.runInBackground = _savedBackground;
        }

        private static T Field<T>(Object source, string name) where T : Object
        { return new SerializedObject(source).FindProperty(name).objectReferenceValue as T; }
    }
}
