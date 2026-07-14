using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using NeonGalaxy.Data;
using NeonGalaxy.Utility;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;

namespace NeonGalaxy.Services
{
    /// <summary>
    /// Real implementation of ILeaderboardService using Unity Gaming Services (UGS) Leaderboards.
    /// 
    /// Features:
    /// - Submits scores to UGS Leaderboard
    /// - Fetches top scores and player's own rank
    /// - Offline queue: if internet is unavailable, scores are saved locally (via SaveService)
    ///   and automatically flushed when connectivity is restored
    /// - Caches last fetched leaderboard data for offline display
    /// </summary>
    public class UGSLeaderboardService : ILeaderboardService
    {
        private readonly SaveService _saveService;
        private CachedLeaderboard _cachedData;

        public UGSLeaderboardService(SaveService saveService)
        {
            _saveService = saveService;
            _cachedData = new CachedLeaderboard();
        }

        // ── Properties ──────────────────────────────────────────

        public bool IsAuthenticated =>
            UnityServices.State == ServicesInitializationState.Initialized
            && AuthenticationService.Instance.IsSignedIn;

        public bool IsOnline
        {
            get
            {
                // Check both UGS initialization and network reachability
                if (Application.internetReachability == NetworkReachability.NotReachable)
                    return false;
                return IsAuthenticated;
            }
        }

        // ── Authenticate ────────────────────────────────────────

        /// <summary>
        /// For UGS Leaderboards, authentication is handled by UGSAuthService at boot.
        /// This method simply validates that UGS Auth is already signed in.
        /// </summary>
        public async Task<bool> AuthenticateAsync()
        {
            // Small delay to match interface expectation
            await Task.Yield();

            if (!IsAuthenticated)
            {
                Debug.LogWarning("[UGSLeaderboardService] Not authenticated. UGS Auth must be initialized first.");
                return false;
            }

            string playerId = AuthenticationService.Instance.PlayerId;
            _saveService.Data.cachedPlayerId = playerId;
            _saveService.MarkDirty();

            Debug.Log($"[UGSLeaderboardService] Authenticated. Player ID: {playerId}");
            return true;
        }

        // ── Submit Score ────────────────────────────────────────

        /// <summary>
        /// Submits a score to the UGS leaderboard.
        /// If offline, queues the score locally for later submission.
        /// UGS automatically keeps only the player's best score.
        /// </summary>
        public async Task<bool> SubmitScoreAsync(int score)
        {
            if (!IsOnline)
            {
                // Queue for later — offline fallback
                Debug.Log($"[UGSLeaderboardService] Offline. Queuing score {score} for later submission.");
                _saveService.EnqueueScoreSubmission(score);
                _saveService.Save();
                return true; // Queued successfully
            }

            try
            {
                var result = await LeaderboardsService.Instance.AddPlayerScoreAsync(
                    Constants.LEADERBOARD_ID,
                    score
                );

                Debug.Log($"[UGSLeaderboardService] Score submitted: {score}. " +
                          $"Server rank: {result.Rank}, Server score: {result.Score}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UGSLeaderboardService] SubmitScoreAsync failed: {ex.Message}");

                // Network error at runtime — queue it so it's not lost
                _saveService.EnqueueScoreSubmission(score);
                _saveService.Save();
                Debug.Log($"[UGSLeaderboardService] Score {score} queued as fallback after error.");
                return true; // Queued successfully
            }
        }

        // ── Fetch Leaderboard ───────────────────────────────────

        /// <summary>
        /// Fetches the leaderboard from UGS.
        /// Returns cached data if offline.
        /// </summary>
        public async Task<CachedLeaderboard> FetchLeaderboardAsync()
        {
            if (!IsOnline)
            {
                Debug.Log("[UGSLeaderboardService] Offline. Returning cached leaderboard data.");
                return _cachedData;
            }

            try
            {
                // Fetch top scores
                var options = new GetScoresOptions
                {
                    Limit = Constants.LEADERBOARD_FETCH_COUNT
                };

                var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(
                    Constants.LEADERBOARD_ID,
                    options
                );

                // Build cached leaderboard from response
                var newCachedData = new CachedLeaderboard();
                newCachedData.lastFetchTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                string myPlayerId = AuthenticationService.Instance.PlayerId;

                if (scoresResponse?.Results != null)
                {
                    foreach (var entry in scoresResponse.Results)
                    {
                        var leaderboardEntry = new LeaderboardEntry
                        {
                            rank = entry.Rank + 1, // UGS is 0-indexed, we display 1-indexed
                            playerName = !string.IsNullOrEmpty(entry.PlayerName)
                                ? entry.PlayerName
                                : $"Player_{entry.PlayerId[..6]}",
                            playerId = entry.PlayerId,
                            score = (int)entry.Score
                        };

                        newCachedData.entries.Add(leaderboardEntry);

                        // Identify the current player's entry
                        if (entry.PlayerId == myPlayerId)
                        {
                            newCachedData.playerEntry = leaderboardEntry;
                        }
                    }
                }

                // If the player wasn't in the top list, fetch their score separately
                if (newCachedData.playerEntry == null)
                {
                    try
                    {
                        var playerScore = await LeaderboardsService.Instance.GetPlayerScoreAsync(
                            Constants.LEADERBOARD_ID
                        );

                        if (playerScore != null)
                        {
                            newCachedData.playerEntry = new LeaderboardEntry
                            {
                                rank = playerScore.Rank + 1,
                                playerName = !string.IsNullOrEmpty(playerScore.PlayerName)
                                    ? playerScore.PlayerName
                                    : _saveService.Data.playerName,
                                playerId = playerScore.PlayerId,
                                score = (int)playerScore.Score
                            };
                        }
                    }
                    catch (Exception)
                    {
                        // Player might not have submitted a score yet — that's OK
                        Debug.Log("[UGSLeaderboardService] Player has no score on leaderboard yet.");
                    }
                }

                _cachedData = newCachedData;

                Debug.Log($"[UGSLeaderboardService] Leaderboard fetched. {_cachedData.entries.Count} entries loaded.");
                return _cachedData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UGSLeaderboardService] FetchLeaderboardAsync failed: {ex.Message}");
                // Return whatever we have cached
                return _cachedData;
            }
        }

        // ── Flush Pending Submissions ───────────────────────────

        /// <summary>
        /// Attempts to submit any scores that were queued while the player was offline.
        /// Called at boot and whenever connectivity is restored.
        /// </summary>
        public async Task FlushPendingSubmissionsAsync()
        {
            var pending = _saveService.Data.pendingSubmissions;
            if (pending == null || pending.Count == 0)
            {
                return;
            }

            if (!IsOnline)
            {
                Debug.Log("[UGSLeaderboardService] Still offline. Cannot flush pending submissions.");
                return;
            }

            Debug.Log($"[UGSLeaderboardService] Flushing {pending.Count} pending score submission(s)...");

            // Find the best pending score — UGS only keeps the best anyway,
            // so we only need to submit the highest one
            int bestPendingScore = 0;
            foreach (var submission in pending)
            {
                if (submission.score > bestPendingScore)
                    bestPendingScore = submission.score;
            }

            try
            {
                await LeaderboardsService.Instance.AddPlayerScoreAsync(
                    Constants.LEADERBOARD_ID,
                    bestPendingScore
                );

                Debug.Log($"[UGSLeaderboardService] Flushed best pending score: {bestPendingScore}");

                // Clear all pending submissions
                pending.Clear();
                _saveService.MarkDirty();
                _saveService.Save();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UGSLeaderboardService] Flush failed: {ex.Message}. Will retry later.");
                // Don't clear pending — they'll be retried next time
            }
        }
    }
}
