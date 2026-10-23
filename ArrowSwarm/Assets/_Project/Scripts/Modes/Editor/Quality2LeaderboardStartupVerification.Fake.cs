namespace ArrowSwarm.Modes.Editor
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ArrowSwarm.Data;

    /// <summary>Controlled provider whose initialization, rows and submissions never call UGS or authentication.</summary>
    public static partial class Quality2LeaderboardStartupVerification
    {
        private sealed class FakeCloud : ILeaderboardCloudService
        {
            internal readonly TaskCompletionSource<bool> Initialization = new TaskCompletionSource<bool>();
            internal readonly TaskCompletionSource<List<LeaderboardEntry>> Scores = new TaskCompletionSource<List<LeaderboardEntry>>();
            internal readonly List<TaskCompletionSource<bool>> Submissions = new List<TaskCompletionSource<bool>>();
            internal int FactoryCalls, Fetches, Names, Level, Stars;
            internal string Name, Country;
            internal bool Online = true;

            public bool IsOnline() => Online;
            public Task<bool> EnsureInitializedAsync() => Initialization.Task;
            public Task<bool> UpdatePlayerNameAsync(string name)
            { Names++; Name = name; return Task.FromResult(true); }
            public Task<bool> SubmitScoreAsync(int level, int stars, string name, string country)
            {
                Level = level; Stars = stars; Name = name; Country = country;
                var request = new TaskCompletionSource<bool>(); Submissions.Add(request); return request.Task;
            }
            public async Task<List<LeaderboardEntry>> FetchTopScoresAsync(int count = 10)
            {
                Fetches++;
                if (!await Initialization.Task) return new List<LeaderboardEntry>();
                return await Scores.Task;
            }
            public Task<int> FetchPlayerRankAsync() => Task.FromResult(7);

            internal void Finish()
            {
                Initialization.TrySetResult(false); Scores.TrySetResult(new List<LeaderboardEntry>());
                foreach (var request in Submissions) request.TrySetResult(false);
            }
        }
    }
}
