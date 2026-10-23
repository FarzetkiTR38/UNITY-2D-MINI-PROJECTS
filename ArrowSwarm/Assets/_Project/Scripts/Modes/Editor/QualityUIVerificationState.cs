namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Contains test-only inventory and preference changes inside one synchronous UI verification.</summary>
    internal sealed class QualityUIVerificationState : IDisposable
    {
        private readonly DataManager _data;
        private readonly string _json, _language;
        private readonly bool _autoSave, _pendingExists;
        private readonly int _pending;
        private readonly Dictionary<string, string> _preferences = new Dictionary<string, string>();

        /// <summary>Snapshots original data and suppresses automatic disk saves for the test.</summary>
        public QualityUIVerificationState()
        {
            _data = DataManager.Instance; _json = JsonUtility.ToJson(_data.PlayerData);
            _language = LocalizationManager.Instance.CurrentLanguage;
            foreach (string key in new[] { "ArrowSwarm_PlayerData", "ArrowSwarm_PlayerData_Backup", "ArrowSwarm_SelectedLanguage", "ArrowSwarm_CachedLeaderboard" })
                _preferences[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : null;
            _pendingExists = PlayerPrefs.HasKey("ArrowSwarm_PendingCloudSync");
            _pending = PlayerPrefs.GetInt("ArrowSwarm_PendingCloudSync");
            var serialized = new SerializedObject(_data);
            _autoSave = serialized.FindProperty("_autoSaveOnChange").boolValue;
            serialized.FindProperty("_autoSaveOnChange").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Restores original player data, selected language, save policy and preference existence.</summary>
        public void Dispose()
        {
            if (_data != null)
            {
                JsonUtility.FromJsonOverwrite(_json, _data.PlayerData);
                var serialized = new SerializedObject(_data);
                serialized.FindProperty("_autoSaveOnChange").boolValue = _autoSave;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            if (LocalizationManager.HasInstance && LocalizationManager.Instance.CurrentLanguage != _language)
                LocalizationManager.Instance.SetLanguage(_language, false);
            foreach (var item in _preferences)
            {
                if (item.Value == null) { if (PlayerPrefs.HasKey(item.Key)) PlayerPrefs.DeleteKey(item.Key); }
                else if (!PlayerPrefs.HasKey(item.Key) || PlayerPrefs.GetString(item.Key) != item.Value)
                    PlayerPrefs.SetString(item.Key, item.Value);
            }
            if (!_pendingExists) PlayerPrefs.DeleteKey("ArrowSwarm_PendingCloudSync");
            else if (PlayerPrefs.GetInt("ArrowSwarm_PendingCloudSync") != _pending)
                PlayerPrefs.SetInt("ArrowSwarm_PendingCloudSync", _pending);
        }

        /// <summary>Whether inventory, progress and preference values match the original snapshot.</summary>
        public bool IsRestored()
        {
            if (_data == null || JsonUtility.ToJson(_data.PlayerData) != _json) return false;
            foreach (var item in _preferences)
                if (PlayerPrefs.HasKey(item.Key) != (item.Value != null) ||
                    (item.Value != null && PlayerPrefs.GetString(item.Key) != item.Value)) return false;
            return PlayerPrefs.HasKey("ArrowSwarm_PendingCloudSync") == _pendingExists &&
                PlayerPrefs.GetInt("ArrowSwarm_PendingCloudSync") == _pending;
        }
    }
}
