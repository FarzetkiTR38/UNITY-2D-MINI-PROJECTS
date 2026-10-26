namespace ArrowSwarm.Modes.Editor
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ArrowSwarm.Data;

    /// <summary>Supplies deterministic immediate responses without constructing Unity services or authentication.</summary>
    public static partial class Quality4SettingsSyncVerification
    {
        /// <summary>Records provider usage and the latest submitted payload while simulating online/offline changes.</summary>
        private sealed class SettingsCloud : ILeaderboardCloudService
        {
            internal bool Online;
            internal int FactoryCalls, Initializations, Submissions, Level, Stars;
            internal string Name, Country;

            /// <summary>Returns only the locally controlled connection state.</summary>
            public bool IsOnline() => Online;
            /// <summary>Records initialization without any network traffic.</summary>
            public Task<bool> EnsureInitializedAsync() { Initializations++; return Task.FromResult(Online); }
            /// <summary>Accepts a profile update entirely in memory.</summary>
            public Task<bool> UpdatePlayerNameAsync(string name) => Task.FromResult(Online);
            /// <summary>Records one score snapshot and immediately accepts it while online.</summary>
            public Task<bool> SubmitScoreAsync(int level, int stars, string name, string country)
            {
                Submissions++; Level = level; Stars = stars; Name = name; Country = country;
                return Task.FromResult(Online);
            }
            /// <summary>Returns no synthetic leaderboard rows.</summary>
            public Task<List<LeaderboardEntry>> FetchTopScoresAsync(int limit = 10) => Task.FromResult(new List<LeaderboardEntry>());
            /// <summary>Returns an explicitly unavailable global rank.</summary>
            public Task<int> FetchPlayerRankAsync() => Task.FromResult(-1);
        }
    }
}
