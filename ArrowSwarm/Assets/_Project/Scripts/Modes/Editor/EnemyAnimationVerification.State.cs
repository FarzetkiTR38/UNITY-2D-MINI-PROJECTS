namespace ArrowSwarm.Modes.Editor
{
    using System.Collections.Generic;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Skills;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using DebugManager = ArrowSwarm.Debug.DebugManager;

    /// <summary>Snapshots and restores save data, preview flags and temporary Defense debug settings.</summary>
    public static partial class EnemyAnimationVerification
    {
        private static DataManager _data;
        private static DebugManager _debug;
        private static string _savedData, _savedConfig;
        private static bool _savedAutoSave, _savedPreview, _savedBackground, _createdDebug;
        private static bool _debugOverride, _debugLives, _debugTips;
        private static int _debugLevel, _savedPreviewLevel;
        private static float _debugSpeed, _savedTime;
        private static GameModeId _savedPreviewMode;
        private static readonly Dictionary<string, string> Preferences = new Dictionary<string, string>();
        private static bool _pendingExists;
        private static int _pendingValue;

        private static void Snapshot()
        {
            _data = DataManager.Instance; _savedData = JsonUtility.ToJson(_data.PlayerData);
            _savedConfig = JsonUtility.ToJson(GameManager.Instance.Config); Preferences.Clear();
            foreach (string key in new[] { "ArrowSwarm_PlayerData", "ArrowSwarm_PlayerData_Backup", "ArrowSwarm_SelectedLanguage", "ArrowSwarm_CachedLeaderboard" })
                Preferences[key] = Preference(key);
            _pendingExists = PlayerPrefs.HasKey("ArrowSwarm_PendingCloudSync"); _pendingValue = PlayerPrefs.GetInt("ArrowSwarm_PendingCloudSync", 0);
            var data = new SerializedObject(_data); _savedAutoSave = data.FindProperty("_autoSaveOnChange").boolValue;
            data.FindProperty("_autoSaveOnChange").boolValue = false; data.ApplyModifiedPropertiesWithoutUndo();
            _savedPreview = PuzzleLaunch.Preview; _savedPreviewMode = PuzzleLaunch.PreviewMode;
            _savedPreviewLevel = PuzzleLaunch.PreviewLevel; _savedTime = Time.timeScale; _savedBackground = Application.runInBackground;
            _createdDebug = !DebugManager.HasInstance; _debug = DebugManager.Instance;
            var debug = new SerializedObject(_debug);
            _debugOverride = debug.FindProperty("_useDebugLevel").boolValue; _debugLevel = debug.FindProperty("_debugLevel").intValue;
            _debugLives = debug.FindProperty("_infiniteLives").boolValue; _debugTips = debug.FindProperty("_infiniteTips").boolValue;
            _debugSpeed = debug.FindProperty("_gameSpeedMultiplier").floatValue;
            debug.FindProperty("_useDebugLevel").boolValue = true; debug.FindProperty("_debugLevel").intValue = 101;
            debug.FindProperty("_infiniteLives").boolValue = false; debug.FindProperty("_infiniteTips").boolValue = false;
            debug.FindProperty("_gameSpeedMultiplier").floatValue = 1f; debug.ApplyModifiedPropertiesWithoutUndo();
            Application.runInBackground = true;
        }

        private static string Preference(string key) => (PlayerPrefs.HasKey(key) ? "present:" : "absent:") + PlayerPrefs.GetString(key, "");

        private static void CheckPreferences()
        {
            foreach (var item in Preferences) Check(Preference(item.Key) == item.Value, item.Key + " unchanged (including key existence)");
            Check(PlayerPrefs.HasKey("ArrowSwarm_PendingCloudSync") == _pendingExists &&
                PlayerPrefs.GetInt("ArrowSwarm_PendingCloudSync", 0) == _pendingValue, "Leaderboard pending-sync preference unchanged");
        }

        private static void CleanupRuntime()
        {
            if (!Application.isPlaying) return;
            if (FreezeManager.HasInstance) FreezeManager.Instance.EndFreezeSilently();
            if (LevelManager.HasInstance) LevelManager.Instance.CleanupLevel();
            if (!_exiting && GameManager.HasInstance && SceneManager.GetActiveScene().name != "MainMenuScene") GameManager.Instance.GoToMainMenu();
        }

        private static void Restore()
        {
            if (_restored) return;
            if (_data != null)
            {
                JsonUtility.FromJsonOverwrite(_savedData, _data.PlayerData);
                var data = new SerializedObject(_data); data.FindProperty("_autoSaveOnChange").boolValue = _savedAutoSave;
                data.ApplyModifiedPropertiesWithoutUndo();
            }
            if (_debug != null)
            {
                var debug = new SerializedObject(_debug);
                debug.FindProperty("_useDebugLevel").boolValue = _debugOverride; debug.FindProperty("_debugLevel").intValue = _debugLevel;
                debug.FindProperty("_infiniteLives").boolValue = _debugLives; debug.FindProperty("_infiniteTips").boolValue = _debugTips;
                debug.FindProperty("_gameSpeedMultiplier").floatValue = _debugSpeed; debug.ApplyModifiedPropertiesWithoutUndo();
                if (_createdDebug) UnityEngine.Object.Destroy(_debug.gameObject);
            }
            PuzzleLaunch.Preview = _savedPreview; PuzzleLaunch.PreviewMode = _savedPreviewMode; PuzzleLaunch.PreviewLevel = _savedPreviewLevel;
            Time.timeScale = _savedTime; Application.runInBackground = _savedBackground;
            if (ThemeManager.HasInstance) ThemeManager.Instance.ApplyCurrentTheme();
            _restored = true; Results.Add("RESTORED: PlayerData, auto-save, debug override/cheats/speed, preview, time scale, theme and background setting");
        }
    }
}
