namespace ArrowSwarm.Modes.Editor
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ArrowSwarm.Data;

    /// <summary>Records immutable submitted snapshots and independently controllable completion order without networking.</summary>
    public static partial class Quality5ProfileSyncVerification
    {
        /// <summary>One provider request, including the exact metadata captured at its real call time.</summary>
        private sealed class Submission
        {
            internal int Level, Stars;
            internal string Name, Country;
            internal readonly TaskCompletionSource<bool> Completion = new TaskCompletionSource<bool>();
        }

        /// <summary>Deterministic cloud adapter with immediate, delayed, failed and offline responses.</summary>
        private sealed class ProfileCloud : ILeaderboardCloudService
        {
            internal readonly List<Submission> Requests = new List<Submission>();
            internal bool Online, Delay, Accept = true;
            internal int FactoryCalls, Initializations, Names, Fetches;

            /// <inheritdoc/>
            public bool IsOnline() => Online;
            /// <inheritdoc/>
            public Task<bool> EnsureInitializedAsync() { Initializations++; return Task.FromResult(Online); }
            /// <inheritdoc/>
            public Task<bool> UpdatePlayerNameAsync(string name) { Names++; return Task.FromResult(Online); }
            /// <inheritdoc/>
            public Task<bool> SubmitScoreAsync(int level, int stars, string name, string country)
            {
                var request = new Submission { Level = level, Stars = stars, Name = name, Country = country };
                Requests.Add(request);
                if (!Delay) request.Completion.SetResult(Online && Accept);
                return request.Completion.Task;
            }
            /// <inheritdoc/>
            public Task<List<LeaderboardEntry>> FetchTopScoresAsync(int count = 10)
            { Fetches++; return Task.FromResult(new List<LeaderboardEntry>()); }
            /// <inheritdoc/>
            public Task<int> FetchPlayerRankAsync() => Task.FromResult(-1);

            internal void FinishPending(bool success)
            { foreach (var request in Requests) request.Completion.TrySetResult(success); }
        }
    }
}
