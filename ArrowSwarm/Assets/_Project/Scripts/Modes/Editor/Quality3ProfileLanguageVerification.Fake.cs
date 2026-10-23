namespace ArrowSwarm.Modes.Editor
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ArrowSwarm.Data;

    /// <summary>Offline fake provider makes the real profile Save path deterministic without auth or network traffic.</summary>
    public static partial class Quality3ProfileLanguageVerification
    {
        /// <summary>Reports offline status so production profile synchronization cannot start any external operation.</summary>
        private sealed class OfflineCloud : ILeaderboardCloudService
        {
            internal int FactoryCalls, NetworkOperations;
            /// <inheritdoc/>
            public bool IsOnline() => false;
            /// <inheritdoc/>
            public Task<bool> EnsureInitializedAsync() { NetworkOperations++; return Task.FromResult(false); }
            /// <inheritdoc/>
            public Task<bool> UpdatePlayerNameAsync(string name) { NetworkOperations++; return Task.FromResult(false); }
            /// <inheritdoc/>
            public Task<bool> SubmitScoreAsync(int level, int stars, string name, string country)
            { NetworkOperations++; return Task.FromResult(false); }
            /// <inheritdoc/>
            public Task<List<LeaderboardEntry>> FetchTopScoresAsync(int limit = 10)
            { NetworkOperations++; return Task.FromResult(new List<LeaderboardEntry>()); }
            /// <inheritdoc/>
            public Task<int> FetchPlayerRankAsync() { NetworkOperations++; return Task.FromResult(-1); }
        }
    }
}
