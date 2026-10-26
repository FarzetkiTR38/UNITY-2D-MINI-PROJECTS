namespace ArrowSwarm.Data
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>Retries one pending snapshot per online refresh without recursive refreshes or duplicate active synchronization.</summary>
    public partial class LeaderboardManager
    {
        private Task _currentSynchronization;

        private async Task<List<LeaderboardEntry>> RefreshCoreAsync(int count, bool retryPending)
        {
            if (_isRefreshing) return GetTopPlayers(count);
            _isRefreshing = true;
            RefreshState = LeaderboardRefreshState.Loading;
            OnLeaderboardUpdated?.Invoke();
            try
            {
                if (_cloudService.IsOnline())
                {
                    if (retryPending && PlayerPrefs.GetInt(PENDING_SYNC_KEY, 0) == 1)
                        await SynchronizeCurrentWithoutRefreshAsync();
                    var scores = await _cloudService.FetchTopScoresAsync(count);
                    if (scores != null)
                    {
                        int rank = await _cloudService.FetchPlayerRankAsync();
                        _cachedEntries = scores;
                        _cachedPlayerRank = rank;
                        _lastSuccessfulRefreshUtc = DateTime.UtcNow;
                        SaveCachedLeaderboard();
                        RefreshState = LeaderboardRefreshState.Ready;
                    }
                    else RefreshState = _cloudService.IsOnline() ? LeaderboardRefreshState.Failed : LeaderboardRefreshState.Offline;
                }
                else RefreshState = LeaderboardRefreshState.Offline;
            }
            catch (Exception error)
            {
                RefreshState = LeaderboardRefreshState.Failed;
                Debug.LogWarning("[ArrowSwarm] Leaderboard cloud refresh failed: " + error.Message);
            }
            finally
            {
                _isRefreshing = false;
                OnLeaderboardUpdated?.Invoke();
            }
            return GetTopPlayers(count);
        }

        private Task SynchronizeCurrentWithoutRefreshAsync()
        {
            if (_currentSynchronization == null || _currentSynchronization.IsCompleted)
                _currentSynchronization = SynchronizeCurrentSnapshotAsync();
            return _currentSynchronization;
        }

        private async Task SynchronizeCurrentSnapshotAsync()
        {
            if (!_cloudService.IsOnline() || !await _cloudService.EnsureInitializedAsync())
            {
                PlayerPrefs.SetInt(PENDING_SYNC_KEY, 1); PlayerPrefs.Save();
                return;
            }
            var player = DataManager.Instance?.PlayerData;
            if (player == null) return;
            await _cloudService.UpdatePlayerNameAsync(player.playerName);
            await SubmitScoreAsync(player.highestLevel, player.GetTotalStars());
        }
    }
}
