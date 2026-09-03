namespace ArrowSwarm.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Manages the game leaderboard using Unity Gaming Services (UGS) with offline caching.
    /// Saves player progress locally when offline and synchronizes with cloud when online.
    /// </summary>
    public class LeaderboardManager : Singleton<LeaderboardManager>
    {
        private readonly UnityCloudService _cloudService = new UnityCloudService();
        private List<LeaderboardEntry> _cachedEntries = new List<LeaderboardEntry>();
        private int _cachedPlayerRank = -1;
        private bool _isRefreshing;

        private const string CACHE_KEY = "ArrowSwarm_CachedLeaderboard";
        private const string PENDING_SYNC_KEY = "ArrowSwarm_PendingCloudSync";

        /// <summary>Fired when leaderboard data has been refreshed.</summary>
        public static event Action OnLeaderboardUpdated;

        protected override void OnSingletonAwake()
        {
            LoadCachedLeaderboard();
            _ = InitializeAndSyncAsync();
        }

        private async Task InitializeAndSyncAsync()
        {
            if (_cloudService.IsOnline())
            {
                bool initialized = await _cloudService.EnsureInitializedAsync();
                if (initialized)
                {
                    if (PlayerPrefs.GetInt(PENDING_SYNC_KEY, 0) == 1)
                    {
                        await SyncCurrentPlayerDataAsync();
                    }
                    await RefreshFromCloudAsync();
                }
            }
        }

        private void LoadCachedLeaderboard()
        {
            if (PlayerPrefs.HasKey(CACHE_KEY))
            {
                try
                {
                    string json = PlayerPrefs.GetString(CACHE_KEY);
                    var wrapper = JsonUtility.FromJson<LeaderboardWrapper>(json);
                    if (wrapper != null && wrapper.Entries != null)
                    {
                        _cachedEntries = wrapper.Entries;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[ArrowSwarm] Failed to parse cached leaderboard: {ex.Message}");
                    _cachedEntries = new List<LeaderboardEntry>();
                }
            }
        }

        private void SaveCachedLeaderboard()
        {
            try
            {
                var wrapper = new LeaderboardWrapper { Entries = _cachedEntries };
                string json = JsonUtility.ToJson(wrapper);
                PlayerPrefs.SetString(CACHE_KEY, json);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ArrowSwarm] Failed to save cached leaderboard: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously refreshes the leaderboard from UGS Cloud.
        /// </summary>
        public async Task<List<LeaderboardEntry>> RefreshFromCloudAsync(int count = 10)
        {
            if (_isRefreshing) return GetTopPlayers(count);
            _isRefreshing = true;

            try
            {
                if (_cloudService.IsOnline())
                {
                    List<LeaderboardEntry> cloudScores = await _cloudService.FetchTopScoresAsync(count);
                    if (cloudScores != null && cloudScores.Count > 0)
                    {
                        _cachedEntries = cloudScores;
                        SaveCachedLeaderboard();
                    }

                    _cachedPlayerRank = await _cloudService.FetchPlayerRankAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ArrowSwarm] Leaderboard cloud refresh failed: {ex.Message}");
            }
            finally
            {
                _isRefreshing = false;
                OnLeaderboardUpdated?.Invoke();
            }

            return GetTopPlayers(count);
        }

        /// <summary>
        /// Submits the current player's highest level and stars to UGS.
        /// If offline, marks pending sync for when network becomes available.
        /// </summary>
        public async Task<bool> SubmitScoreAsync(int highestLevel, int totalStars)
        {
            var playerData = DataManager.Instance?.PlayerData;
            string playerName = playerData?.playerName ?? "Player";
            string country = playerData?.playerCountry ?? "TR";

            if (!_cloudService.IsOnline())
            {
                PlayerPrefs.SetInt(PENDING_SYNC_KEY, 1);
                PlayerPrefs.Save();
                LogDebug("Saved score locally (Offline). Will sync when online.");
                return false;
            }

            bool success = await _cloudService.SubmitScoreAsync(highestLevel, totalStars, playerName, country);
            if (success)
            {
                PlayerPrefs.SetInt(PENDING_SYNC_KEY, 0);
                PlayerPrefs.Save();
            }
            else
            {
                PlayerPrefs.SetInt(PENDING_SYNC_KEY, 1);
                PlayerPrefs.Save();
            }

            return success;
        }

        /// <summary>
        /// Updates the current player's nickname and submits to cloud.
        /// </summary>
        public void SetPlayerName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) return;
            DataManager.Instance?.SetPlayerName(newName);
            _ = SyncCurrentPlayerDataAsync();
        }

        /// <summary>
        /// Synchronizes player name, country, level, and stars to cloud.
        /// </summary>
        public async Task SyncCurrentPlayerDataAsync()
        {
            var playerData = DataManager.Instance?.PlayerData;
            if (playerData == null) return;

            if (_cloudService.IsOnline())
            {
                await _cloudService.UpdatePlayerNameAsync(playerData.playerName);
                await SubmitScoreAsync(playerData.highestLevel, playerData.GetTotalStars());
                await RefreshFromCloudAsync();
            }
            else
            {
                PlayerPrefs.SetInt(PENDING_SYNC_KEY, 1);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Gets the top players for display. Always injects local player data for instant responsiveness.
        /// </summary>
        public List<LeaderboardEntry> GetTopPlayers(int count = 10)
        {
            var currentData = DataManager.Instance?.PlayerData;
            int playerLevel = currentData?.highestLevel ?? 1;
            int playerStars = currentData?.GetTotalStars() ?? 0;
            string playerName = currentData?.playerName ?? "Player";
            string country = currentData?.playerCountry ?? "TR";

            var playerEntry = new LeaderboardEntry
            {
                PlayerName = playerName,
                HighestLevel = playerLevel,
                TotalStars = playerStars,
                IsPlayer = true,
                CountryCode = country
            };

            var list = new List<LeaderboardEntry>();

            if (_cachedEntries != null && _cachedEntries.Count > 0)
            {
                bool playerFoundInList = false;
                for (int i = 0; i < _cachedEntries.Count; i++)
                {
                    var entry = _cachedEntries[i];
                    if (entry.IsPlayer || entry.PlayerName == playerName)
                    {
                        entry.IsPlayer = true;
                        entry.HighestLevel = Mathf.Max(entry.HighestLevel, playerLevel);
                        entry.TotalStars = Mathf.Max(entry.TotalStars, playerStars);
                        entry.CountryCode = country;
                        playerFoundInList = true;
                    }
                    list.Add(entry);
                }

                if (!playerFoundInList)
                {
                    list.Add(playerEntry);
                }
            }
            else
            {
                list.Add(playerEntry);
            }

            return list.OrderByDescending(e => e.HighestLevel)
                       .ThenByDescending(e => e.TotalStars)
                       .Take(count)
                       .ToList();
        }

        /// <summary>
        /// Returns the player's current leaderboard rank.
        /// </summary>
        public int GetPlayerRank()
        {
            if (_cachedPlayerRank > 0) return _cachedPlayerRank;

            var all = GetTopPlayers(100);
            int idx = all.FindIndex(e => e.IsPlayer);
            return idx >= 0 ? idx + 1 : 1;
        }

        [Serializable]
        private class LeaderboardWrapper
        {
            public List<LeaderboardEntry> Entries;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] LeaderboardManager: {message}");
        }
    }
}
