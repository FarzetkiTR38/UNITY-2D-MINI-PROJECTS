namespace ArrowSwarm.Marketing.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using ArrowSwarm.Data;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Private preference backup and pre-bootstrap offline cloud isolation.</summary>
    public static partial class MarketingCapture
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo Factory = typeof(LeaderboardManager).GetField("_editorCloudFactory", BindingFlags.Static | BindingFlags.NonPublic);
        private const string BackupKey = "ArrowSwarm.Marketing.Backup";
        private static bool _offlineInstalled;
        private static object _originalFactory;
        [Serializable] private sealed class Pref { public string key, value; public bool exists; }
        [Serializable] private sealed class Prefs { public List<Pref> items = new List<Pref>(); public bool pendingExists; public int pending; }

        private static void SavePreferences()
        {
            var data = new Prefs();
            foreach (var key in new[] { "ArrowSwarm_PlayerData", "ArrowSwarm_PlayerData_Backup", "ArrowSwarm_SelectedLanguage", "ArrowSwarm_CachedLeaderboard" })
                data.items.Add(new Pref { key = key, value = PlayerPrefs.GetString(key), exists = PlayerPrefs.HasKey(key) });
            data.pendingExists = PlayerPrefs.HasKey("ArrowSwarm_PendingCloudSync");
            data.pending = PlayerPrefs.GetInt("ArrowSwarm_PendingCloudSync", 0);
            string path = Path.Combine(Path.GetTempPath(), "ArrowSwarm-CapturePrivate-" + Guid.NewGuid() + ".json");
            File.WriteAllText(path, JsonUtility.ToJson(data)); SessionState.SetString(BackupKey, path);
        }
        private static void RestorePreferences()
        {
            string path = SessionState.GetString(BackupKey, "");
            if (!File.Exists(path)) throw new InvalidOperationException("Private preference backup is missing; preserve Armed recovery state.");
            var data = JsonUtility.FromJson<Prefs>(File.ReadAllText(path));
            foreach (var item in data.items)
                if (item.exists) PlayerPrefs.SetString(item.key, item.value); else PlayerPrefs.DeleteKey(item.key);
            if (data.pendingExists) PlayerPrefs.SetInt("ArrowSwarm_PendingCloudSync", data.pending); else PlayerPrefs.DeleteKey("ArrowSwarm_PendingCloudSync");
            PlayerPrefs.Save();
            foreach (var item in data.items)
                if (PlayerPrefs.HasKey(item.key) != item.exists || (item.exists && PlayerPrefs.GetString(item.key) != item.value))
                    throw new InvalidOperationException("Preference restoration verification failed.");
            if (PlayerPrefs.HasKey("ArrowSwarm_PendingCloudSync") != data.pendingExists ||
                (data.pendingExists && PlayerPrefs.GetInt("ArrowSwarm_PendingCloudSync") != data.pending))
                throw new InvalidOperationException("Pending cloud-sync restoration verification failed.");
            Directory.CreateDirectory("ArrowSwarm_Marketing/01_Game_Audit");
            File.WriteAllText("ArrowSwarm_Marketing/01_Game_Audit/player_state_restoration.txt", "PASS: all four string preferences and pending-sync key restored after leaving Play. Private values excluded.");
        }
        private static void InstallOffline()
        {
            if (_offlineInstalled) return;
            _originalFactory = Factory.GetValue(null);
            Factory.SetValue(null, new Func<ILeaderboardCloudService>(() => new Offline()));
            _offlineInstalled = true;
        }
        private static void RestoreOffline()
        {
            if (!_offlineInstalled) return;
            Factory.SetValue(null, _originalFactory); _originalFactory = null; _offlineInstalled = false;
        }
        private sealed class Offline : ILeaderboardCloudService
        {
            public bool IsOnline() => false;
            public Task<bool> EnsureInitializedAsync() => Task.FromResult(false);
            public Task<bool> UpdatePlayerNameAsync(string name) => Task.FromResult(false);
            public Task<bool> SubmitScoreAsync(int level, int stars, string name, string country) => Task.FromResult(false);
            public Task<List<LeaderboardEntry>> FetchTopScoresAsync(int limit = 10) => Task.FromResult<List<LeaderboardEntry>>(null);
            public Task<int> FetchPlayerRankAsync() => Task.FromResult(-1);
        }
    }
}
