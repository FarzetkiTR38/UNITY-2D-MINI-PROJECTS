namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.UI.Editor;
    using UnityEngine;
    using UnityEngine.EventSystems;

    /// <summary>Checks exact original objects, preferences, provider ownership, selection and random state.</summary>
    public static partial class Revision6LeaderboardPreviewVerification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo Factory = typeof(LeaderboardManager).GetField("_editorCloudFactory", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>Reads private verification state without writing or logging any player payload.</summary>
        private sealed class Snapshot
        {
            internal readonly string Language;
            internal readonly bool Background;
            private readonly PlayerData _originalPlayer;
            private readonly DataManager _manager;
            private readonly string _json, _random;
            private readonly object _factory;
            private readonly Dictionary<string, string> _preferences = new Dictionary<string, string>();
            private readonly bool _pendingExists;
            private readonly int _pending;
            private readonly GameObject _selection;

            internal Snapshot()
            {
                if (!DataManager.HasInstance || !LocalizationManager.HasInstance || LeaderboardManager.HasInstance)
                    throw new InvalidOperationException("Fresh Boot-to-MainMenu managers are required.");
                _manager = DataManager.Instance; _originalPlayer = _manager.PlayerData; _json = JsonUtility.ToJson(_originalPlayer);
                _factory = Factory.GetValue(null); Language = LocalizationManager.Instance.CurrentLanguage;
                Background = Application.runInBackground; _random = JsonUtility.ToJson(UnityEngine.Random.state);
                _selection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
                foreach (string key in new[] { "ArrowSwarm_PlayerData", "ArrowSwarm_PlayerData_Backup", "ArrowSwarm_SelectedLanguage", "ArrowSwarm_CachedLeaderboard" })
                    _preferences.Add(key, PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : null);
                _pendingExists = PlayerPrefs.HasKey("ArrowSwarm_PendingCloudSync");
                _pending = PlayerPrefs.GetInt("ArrowSwarm_PendingCloudSync", 0);
            }

            internal void VerifyPreferences(string label)
            {
                bool same = PlayerPrefs.HasKey("ArrowSwarm_PendingCloudSync") == _pendingExists &&
                    PlayerPrefs.GetInt("ArrowSwarm_PendingCloudSync", 0) == _pending;
                foreach (var pair in _preferences)
                    same &= PlayerPrefs.HasKey(pair.Key) == (pair.Value != null) &&
                        (pair.Value == null || PlayerPrefs.GetString(pair.Key) == pair.Value);
                Check(same, label);
            }

            internal void VerifyRestored(string context)
            {
                VerifyPreferences(context + ": all five original preferences match exactly");
                Check(!LeaderboardManager.HasInstance && ReferenceEquals(Factory.GetValue(null), _factory) &&
                    LeaderboardSamplePreview.LastExternalOperations == 0,
                    context + ": fake manager removed, exact provider factory restored, zero auth/fetch/upload/profile calls");
                Check(ReferenceEquals(typeof(DataManager).GetField("_playerData", Private).GetValue(_manager), _originalPlayer) &&
                    JsonUtility.ToJson(_originalPlayer) == _json,
                    context + ": original player object and all progress/settings restored");
                Check(LocalizationManager.Instance.CurrentLanguage == Language &&
                    JsonUtility.ToJson(UnityEngine.Random.state) == _random,
                    context + ": original language and random state restored");
                Check(EventSystem.current == null || EventSystem.current.currentSelectedGameObject == _selection,
                    context + ": original EventSystem selection restored");
            }
        }

        private static T Field<T>(object owner, string name) => (T)owner.GetType().GetField(name, Private).GetValue(owner);
    }
}
