namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ArrowSwarm.Data;

    /// <summary>Uses entirely local deterministic responses, including delayed, null and throwing failures.</summary>
    public static partial class Quality5LeaderboardVerification
    {
        /// <summary>Implements cloud operations without creating authentication, SDK or network services.</summary>
        private sealed class FakeCloud : ILeaderboardCloudService
        {
            internal bool Online, ReturnNull, Throw;
            internal int FactoryCalls, Fetches, Ranks, Submissions, NameUpdates;
            internal int Rank = -1;
            internal List<LeaderboardEntry> Rows = new List<LeaderboardEntry>();
            internal TaskCompletionSource<List<LeaderboardEntry>> Gate;
            private bool _aborted;

            /// <summary>Returns the controlled connectivity state.</summary>
            public bool IsOnline() => Online && !_aborted;
            /// <summary>Initializes only the in-memory fake identity.</summary>
            public Task<bool> EnsureInitializedAsync() => Task.FromResult(IsOnline());
            /// <summary>Records unexpected profile writes without making any request.</summary>
            public Task<bool> UpdatePlayerNameAsync(string name) { NameUpdates++; return Task.FromResult(IsOnline()); }
            /// <summary>Records unexpected score writes without submitting them anywhere.</summary>
            public Task<bool> SubmitScoreAsync(int level, int stars, string name, string country)
            { Submissions++; return Task.FromResult(IsOnline()); }
            /// <summary>Returns the configured result, or waits until the fixture releases its gate.</summary>
            public Task<List<LeaderboardEntry>> FetchTopScoresAsync(int limit = 10)
            {
                Fetches++;
                if (_aborted || ReturnNull) return Task.FromResult<List<LeaderboardEntry>>(null);
                if (Throw) throw new InvalidOperationException("Expected Quality5 controlled leaderboard failure");
                return Gate != null ? Gate.Task : Task.FromResult(Rows);
            }
            /// <summary>Returns an authoritative rank or an explicitly unknown rank.</summary>
            public Task<int> FetchPlayerRankAsync() { Ranks++; return Task.FromResult(_aborted ? -1 : Rank); }

            internal void Release()
            { var gate = Gate; Gate = null; gate?.TrySetResult(_aborted ? null : Rows); }

            internal void Abort()
            { _aborted = true; Release(); }
        }

        private static LeaderboardEntry Row(string id, string name, int level, int stars, bool own = false)
        {
            return new LeaderboardEntry { PlayerId = id, PlayerName = name, HighestLevel = level,
                TotalStars = stars, CountryCode = "US", IsPlayer = own, LastUpdated = "fixture" };
        }
    }
}
