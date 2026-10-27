namespace ArrowSwarm.UI.Editor
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ArrowSwarm.Data;
    using UnityEngine;

    /// <summary>Produces visibly fictional, realistic-length sample rivals without a network-backed adapter.</summary>
    public static partial class LeaderboardSamplePreview
    {
        private static List<LeaderboardEntry> BuildRows(PlayerData player, bool playerOnBoard)
        {
            string[] names = { "NovaRider", "ArrowFox", "PixelMage", "LunaQuest", "StarPilot",
                "NeonTiger", "MapleArcher", "CloudDancer", "BlueComet", "SunnyWings" };
            string[] countries = { "TR", "US", "GB", "DE", "FR", "BR", "JP", "KR", "CA", "AU" };
            int[] gaps = playerOnBoard ? new[] { 160, 130, 95, 64, 40, 20, 0, -10, -20, -30 }
                : new[] { 220, 200, 180, 160, 140, 120, 100, 80, 60, 40 };
            var rows = new List<LeaderboardEntry>();
            for (int i = 0; i < 10; i++)
            {
                bool current = playerOnBoard && i == 6;
                rows.Add(new LeaderboardEntry
                {
                    PlayerId = "editor-sample-" + i,
                    PlayerName = current ? player.playerName : names[i],
                    HighestLevel = Mathf.Max(1, player.highestLevel + gaps[i]),
                    TotalStars = Mathf.Max(0, player.GetTotalStars() + gaps[i] * 3),
                    CountryCode = current ? player.playerCountry : countries[i],
                    IsPlayer = current
                });
            }
            return rows;
        }

        /// <summary>Always offline: a sample row can never become a real fetch, auth, score or profile request.</summary>
        private sealed class OfflineCloud : ILeaderboardCloudService
        {
            internal int ExternalOperations;
            /// <inheritdoc/>
            public bool IsOnline() => false;
            /// <inheritdoc/>
            public Task<bool> EnsureInitializedAsync() { ExternalOperations++; return Task.FromResult(false); }
            /// <inheritdoc/>
            public Task<bool> UpdatePlayerNameAsync(string name) { ExternalOperations++; return Task.FromResult(false); }
            /// <inheritdoc/>
            public Task<bool> SubmitScoreAsync(int level, int stars, string name, string country)
            { ExternalOperations++; return Task.FromResult(false); }
            /// <inheritdoc/>
            public Task<List<LeaderboardEntry>> FetchTopScoresAsync(int count = 10)
            { ExternalOperations++; return Task.FromResult<List<LeaderboardEntry>>(null); }
            /// <inheritdoc/>
            public Task<int> FetchPlayerRankAsync() { ExternalOperations++; return Task.FromResult(-1); }
        }
    }
}
