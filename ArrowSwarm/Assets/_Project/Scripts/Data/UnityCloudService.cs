namespace ArrowSwarm.Data
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Unity.Services.Authentication;
    using Unity.Services.Core;
    using Unity.Services.Leaderboards;
    using Unity.Services.Leaderboards.Models;
    using UnityEngine;

    /// <summary>
    /// Implementation of ICloudService using Unity Gaming Services (UGS).
    /// Handles anonymous authentication, leaderboard score uploads with metadata,
    /// fetching global rankings, and graceful offline fallback.
    /// </summary>
    public class UnityCloudService : ICloudService
    {
        public const string LEADERBOARD_ID = "arrow_swarm_leaderboard";

        private bool _isInitialized;
        private bool _isInitializing;

        /// <summary>
        /// Metadata stored alongside each leaderboard score entry.
        /// </summary>
        [Serializable]
        public struct ScoreMetadata
        {
            public int level;
            public int stars;
            public string country;
            public string name;
        }

        /// <summary>
        /// Checks if the device has an active network reachability.
        /// </summary>
        public bool IsOnline()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        /// <summary>
        /// Initializes Unity Services and signs in anonymously if online.
        /// </summary>
        public async Task<bool> EnsureInitializedAsync()
        {
            if (_isInitialized && AuthenticationService.Instance.IsSignedIn)
            {
                return true;
            }

            if (!IsOnline())
            {
                LogDebug("Device is offline. Skipping UGS initialization.");
                return false;
            }

            if (_isInitializing)
            {
                while (_isInitializing)
                {
                    await Task.Delay(100);
                }
                return _isInitialized && AuthenticationService.Instance.IsSignedIn;
            }

            _isInitializing = true;

            try
            {
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    await UnityServices.InitializeAsync();
                }

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }

                _isInitialized = true;
                LogDebug($"UGS Initialized. PlayerID: {AuthenticationService.Instance.PlayerId}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ArrowSwarm] UGS Initialization failed (offline or network error): {ex.Message}");
                _isInitialized = false;
                return false;
            }
            finally
            {
                _isInitializing = false;
            }
        }

        /// <summary>
        /// Updates the player's display name in UGS Authentication.
        /// </summary>
        public async Task<bool> UpdatePlayerNameAsync(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName)) return false;

            bool isReady = await EnsureInitializedAsync();
            if (!isReady) return false;

            try
            {
                // UGS requires player names to be sanitized
                string sanitizedName = playerName.Trim();
                if (sanitizedName.Length > 30) sanitizedName = sanitizedName.Substring(0, 30);

                await AuthenticationService.Instance.UpdatePlayerNameAsync(sanitizedName);
                LogDebug($"Player name updated on cloud: {sanitizedName}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ArrowSwarm] Failed to update player name on cloud: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Submits the player's score (highest level & total stars) to UGS Leaderboard.
        /// </summary>
        public async Task<bool> SubmitScoreAsync(int highestLevel, int totalStars, string playerName, string countryCode)
        {
            bool isReady = await EnsureInitializedAsync();
            if (!isReady) return false;

            try
            {
                // Primary sorting: Level (multiplied by 10,000), Secondary sorting: Stars
                double score = (double)highestLevel * 10000.0 + totalStars;

                var metadata = new ScoreMetadata
                {
                    level = highestLevel,
                    stars = totalStars,
                    country = string.IsNullOrEmpty(countryCode) ? "TR" : countryCode,
                    name = string.IsNullOrEmpty(playerName) ? "Player" : playerName
                };

                var options = new AddPlayerScoreOptions
                {
                    Metadata = metadata
                };

                await LeaderboardsService.Instance.AddPlayerScoreAsync(LEADERBOARD_ID, score, options);
                LogDebug($"Score submitted to cloud: Level={highestLevel}, Stars={totalStars}, Score={score}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ArrowSwarm] Failed to submit score to cloud: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Fetches top scores from the UGS Leaderboard.
        /// </summary>
        public async Task<List<LeaderboardEntry>> FetchTopScoresAsync(int limit = 10)
        {
            var result = new List<LeaderboardEntry>();

            bool isReady = await EnsureInitializedAsync();
            if (!isReady) return result;

            try
            {
                var options = new GetScoresOptions
                {
                    Limit = Mathf.Clamp(limit, 1, 50),
                    IncludeMetadata = true
                };

                LeaderboardScoresPage page = await LeaderboardsService.Instance.GetScoresAsync(LEADERBOARD_ID, options);
                string currentPlayerId = AuthenticationService.Instance.PlayerId;

                foreach (Unity.Services.Leaderboards.Models.LeaderboardEntry entry in page.Results)
                {
                    int level = 1;
                    int stars = 0;
                    string country = "TR";
                    string name = entry.PlayerName;

                    // Parse metadata string if present
                    if (!string.IsNullOrEmpty(entry.Metadata))
                    {
                        try
                        {
                            var meta = JsonUtility.FromJson<ScoreMetadata>(entry.Metadata);
                            level = meta.level;
                            stars = meta.stars;
                            country = meta.country;
                            if (!string.IsNullOrEmpty(meta.name)) name = meta.name;
                        }
                        catch
                        {
                            level = (int)(entry.Score / 10000.0);
                            stars = (int)(entry.Score % 10000.0);
                        }
                    }
                    else
                    {
                        level = (int)(entry.Score / 10000.0);
                        stars = (int)(entry.Score % 10000.0);
                    }

                    if (string.IsNullOrEmpty(name))
                    {
                        name = $"Player #{entry.Rank + 1}";
                    }

                    result.Add(new LeaderboardEntry
                    {
                        PlayerId = entry.PlayerId,
                        PlayerName = name,
                        HighestLevel = Mathf.Max(1, level),
                        TotalStars = Mathf.Max(0, stars),
                        IsPlayer = (entry.PlayerId == currentPlayerId),
                        CountryCode = country
                    });
                }

                LogDebug($"Fetched {result.Count} real entries from UGS Leaderboard.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ArrowSwarm] Failed to fetch leaderboard from cloud: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Gets the current player's rank and score entry from the cloud.
        /// </summary>
        public async Task<int> FetchPlayerRankAsync()
        {
            bool isReady = await EnsureInitializedAsync();
            if (!isReady) return -1;

            try
            {
                Unity.Services.Leaderboards.Models.LeaderboardEntry playerEntry = 
                    await LeaderboardsService.Instance.GetPlayerScoreAsync(LEADERBOARD_ID);

                if (playerEntry != null)
                {
                    return playerEntry.Rank + 1;
                }
            }
            catch
            {
                // Player might not have a score submitted yet
            }

            return -1;
        }

        #region ICloudService Implementation

        public void SavePlayerData(PlayerData data, Action<bool> onComplete)
        {
            if (data == null)
            {
                onComplete?.Invoke(false);
                return;
            }

            _ = Task.Run(async () =>
            {
                bool success = await SubmitScoreAsync(
                    data.highestLevel,
                    data.GetTotalStars(),
                    data.playerName,
                    data.playerCountry
                );

                if (!string.IsNullOrEmpty(data.playerName))
                {
                    await UpdatePlayerNameAsync(data.playerName);
                }

                onComplete?.Invoke(success);
            });
        }

        public void LoadLeaderboard(Action<List<LeaderboardEntry>> onComplete)
        {
            _ = Task.Run(async () =>
            {
                List<LeaderboardEntry> entries = await FetchTopScoresAsync(10);
                onComplete?.Invoke(entries);
            });
        }

        #endregion

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] UnityCloudService: {message}");
        }
    }
}
