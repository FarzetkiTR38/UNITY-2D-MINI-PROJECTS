using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using NeonGalaxy.Data;

namespace NeonGalaxy.Services
{
    /// <summary>
    /// Mock leaderboard service for testing without UGS SDK.
    /// Generates fake leaderboard entries and always succeeds.
    /// Replace with UGSLeaderboardService when integrating live services.
    /// </summary>
    public class MockLeaderboardService : ILeaderboardService
    {
        private readonly SaveService _saveService;
        private CachedLeaderboard _cachedData;
        private bool _isAuthenticated;

        private static readonly string[] _fakeNames = new string[]
        {
            "NeonMaster", "GalaxyQueen", "BlockLord", "StarCrusher",
            "PixelWizard", "CosmicKing", "NovaHunter", "ZenPlayer",
            "PuzzlePro", "GridNinja", "CellBreaker", "TetrisFan",
            "MegaScorer", "ComboKing", "LineDestroyer", "BoardMaster",
            "SpacePilot", "NeonRider", "BlockSmith", "StarGazer"
        };

        public MockLeaderboardService(SaveService saveService)
        {
            _saveService = saveService;
            _isAuthenticated = false;
            GenerateFakeLeaderboard();
        }

        public bool IsAuthenticated => _isAuthenticated;
        public bool IsOnline => true; // Mock is always "online"

        public async Task<bool> AuthenticateAsync()
        {
            // Simulate network delay
            await Task.Delay(500);
            _isAuthenticated = true;
            _saveService.Data.cachedPlayerId = "mock_player_001";
            _saveService.MarkDirty();
            Debug.Log("[MockLeaderboardService] Authenticated (mock).");
            return true;
        }

        public async Task<bool> SubmitScoreAsync(int score)
        {
            // Simulate network delay
            await Task.Delay(300);

            // Update the player's entry in the fake leaderboard
            if (_cachedData.playerEntry != null)
            {
                if (score > _cachedData.playerEntry.score)
                {
                    _cachedData.playerEntry.score = score;
                    SortAndRerank();
                }
            }

            Debug.Log($"[MockLeaderboardService] Score submitted (mock): {score}");
            return true;
        }

        public async Task<CachedLeaderboard> FetchLeaderboardAsync()
        {
            // Simulate network delay
            await Task.Delay(400);
            _cachedData.lastFetchTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Debug.Log("[MockLeaderboardService] Leaderboard fetched (mock).");
            return _cachedData;
        }

        public async Task FlushPendingSubmissionsAsync()
        {
            var pending = _saveService.Data.pendingSubmissions;
            if (pending.Count == 0) return;

            foreach (var submission in new List<PendingScoreSubmission>(pending))
            {
                await SubmitScoreAsync(submission.score);
                _saveService.DequeuePendingSubmission(submission);
            }

            _saveService.Save();
            Debug.Log("[MockLeaderboardService] Flushed pending submissions (mock).");
        }

        // ── Internal ─────────────────────────────────────────────

        private void GenerateFakeLeaderboard()
        {
            _cachedData = new CachedLeaderboard();
            var rng = new System.Random(42); // Deterministic for consistency

            for (int i = 0; i < _fakeNames.Length; i++)
            {
                _cachedData.entries.Add(new LeaderboardEntry
                {
                    rank = i + 1,
                    playerName = _fakeNames[i],
                    playerId = $"mock_{i:D3}",
                    score = Mathf.Max(100, 50000 - (i * 2000) + rng.Next(-500, 500))
                });
            }

            // Add the player's entry
            var playerEntry = new LeaderboardEntry
            {
                rank = _fakeNames.Length + 1,
                playerName = _saveService.Data.playerName,
                playerId = "mock_player_001",
                score = _saveService.Data.bestScore
            };
            _cachedData.entries.Add(playerEntry);
            _cachedData.playerEntry = playerEntry;

            SortAndRerank();
            _cachedData.lastFetchTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        private void SortAndRerank()
        {
            _cachedData.entries.Sort((a, b) => b.score.CompareTo(a.score));
            for (int i = 0; i < _cachedData.entries.Count; i++)
            {
                _cachedData.entries[i].rank = i + 1;
            }
        }
    }
}
