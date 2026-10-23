namespace ArrowSwarm.Modes.Editor
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ArrowSwarm.Data;

    /// <summary>Supplies completed deterministic responses without constructing authentication or a network client.</summary>
    public static partial class Quality3LeaderboardReconnectVerification
    {
        /// <summary>Simulates connectivity changes while recording the exact submitted score snapshot.</summary>
        private sealed class ReconnectCloud : ILeaderboardCloudService
        {
            internal bool Online;
            internal bool AcceptSubmissions = true;
            internal TaskCompletionSource<bool> SubmissionGate;
            internal int FactoryCalls, Initializations, Fetches, Submissions, Level, Stars;
            internal string Name, Country;

            /// <summary>Returns the fixture's current connection state.</summary>
            public bool IsOnline() => Online;
            /// <summary>Records initialization without any external service calls.</summary>
            public Task<bool> EnsureInitializedAsync() { Initializations++; return Task.FromResult(Online); }
            /// <summary>Accepts a profile update locally.</summary>
            public Task<bool> UpdatePlayerNameAsync(string name) { Name = name; return Task.FromResult(Online); }
            /// <summary>Records a submitted snapshot and accepts it only while the fixture is online.</summary>
            public Task<bool> SubmitScoreAsync(int level, int stars, string name, string country)
            {
                Submissions++; Level = level; Stars = stars; Name = name; Country = country;
                return SubmissionGate != null ? SubmissionGate.Task : Task.FromResult(Online && AcceptSubmissions);
            }
            /// <summary>Returns an identifiable global row for the real UI and cache path.</summary>
            public Task<List<LeaderboardEntry>> FetchTopScoresAsync(int limit = 10)
            {
                Fetches++;
                return Task.FromResult(new List<LeaderboardEntry> { new LeaderboardEntry
                    { PlayerId = "reconnect-peer", PlayerName = "Reconnect Peer", HighestLevel = 30, TotalStars = 20, CountryCode = "US" } });
            }
            /// <summary>Returns a controlled authenticated rank.</summary>
            public Task<int> FetchPlayerRankAsync() => Task.FromResult(2);
        }
    }
}
