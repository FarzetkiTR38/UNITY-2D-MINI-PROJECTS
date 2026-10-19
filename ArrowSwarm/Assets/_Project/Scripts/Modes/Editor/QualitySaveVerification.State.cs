namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using ArrowSwarm.Data;
    using UnityEngine;

    /// <summary>Restores the original data object and every preference used by production data services.</summary>
    internal sealed class QualityDataSnapshot : IDisposable
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        private const string Pending = "ArrowSwarm_PendingCloudSync";
        private static readonly FieldInfo DataField = typeof(DataManager).GetField("_playerData", Flags);
        private static readonly FieldInfo AutoSaveField = typeof(DataManager).GetField("_autoSaveOnChange", Flags);
        private readonly DataManager _manager;
        private readonly object _originalData;
        private readonly bool _autoSave, _pendingExists;
        private readonly int _pending;
        private readonly Dictionary<string, string> _strings = new Dictionary<string, string>();

        /// <summary>Snapshots key existence as well as values without reading private data into reports.</summary>
        internal QualityDataSnapshot(DataManager manager)
        {
            _manager = manager; _originalData = DataField.GetValue(manager);
            _autoSave = (bool)AutoSaveField.GetValue(manager);
            foreach (string key in new[] { "ArrowSwarm_PlayerData", "ArrowSwarm_PlayerData_Backup",
                "ArrowSwarm_SelectedLanguage", "ArrowSwarm_CachedLeaderboard" })
                _strings.Add(key, PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : null);
            _pendingExists = PlayerPrefs.HasKey(Pending); _pending = PlayerPrefs.GetInt(Pending, 0);
            AutoSaveField.SetValue(manager, false);
        }

        /// <summary>Changes only the temporary in-memory data reference; no notifications or cloud requests.</summary>
        internal void SetData(PlayerData data) => DataField.SetValue(_manager, data);

        /// <summary>Restores the original object reference, settings and all preference values.</summary>
        public void Dispose()
        {
            DataField.SetValue(_manager, _originalData); AutoSaveField.SetValue(_manager, _autoSave);
            foreach (var pair in _strings)
                if (pair.Value == null) PlayerPrefs.DeleteKey(pair.Key); else PlayerPrefs.SetString(pair.Key, pair.Value);
            if (_pendingExists) PlayerPrefs.SetInt(Pending, _pending); else PlayerPrefs.DeleteKey(Pending);
            PlayerPrefs.Save();
        }

        /// <summary>Checks that successful and failing test runs both leave the user's progress untouched.</summary>
        internal void VerifyRestored(List<string> results)
        {
            if (!ReferenceEquals(DataField.GetValue(_manager), _originalData) ||
                (bool)AutoSaveField.GetValue(_manager) != _autoSave)
                throw new InvalidOperationException("Original data reference or auto-save setting was not restored.");
            foreach (var pair in _strings)
                if (PlayerPrefs.HasKey(pair.Key) != (pair.Value != null) ||
                    (pair.Value != null && PlayerPrefs.GetString(pair.Key) != pair.Value))
                    throw new InvalidOperationException("A data-service string preference was not restored.");
            if (PlayerPrefs.HasKey(Pending) != _pendingExists || PlayerPrefs.GetInt(Pending, 0) != _pending)
                throw new InvalidOperationException("The pending cloud-sync preference was not restored.");
            results.Add("PASS: original data object, auto-save and all five data-service preferences restored");
        }
    }
}
