namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ArrowSwarm.Ads;
    using ArrowSwarm.Data;

    /// <summary>Isolates later-panel navigation from real authentication, cloud writes and ad SDK calls.</summary>
    public static partial class UiRollout01Verification
    {
        private sealed class OfflineCloud : ILeaderboardCloudService
        {
            internal int Operations;
            public bool IsOnline() => false;
            public Task<bool> EnsureInitializedAsync() { Operations++; return Task.FromResult(false); }
            public Task<bool> UpdatePlayerNameAsync(string name) { Operations++; return Task.FromResult(false); }
            public Task<bool> SubmitScoreAsync(int level, int stars, string name, string country)
            { Operations++; return Task.FromResult(false); }
            public Task<List<LeaderboardEntry>> FetchTopScoresAsync(int limit = 10)
            { Operations++; return Task.FromResult<List<LeaderboardEntry>>(null); }
            public Task<int> FetchPlayerRankAsync() { Operations++; return Task.FromResult(-1); }
        }

        private sealed class NoAds : IAdService
        {
            internal int Calls;
            public bool IsAdReady() => false;
            public bool IsInterstitialAdReady() => false;
            public void ShowRewardedAd(Action<bool> completed) { Calls++; completed?.Invoke(false); }
            public void ShowInterstitialAd(Action completed) { Calls++; completed?.Invoke(); }
        }
    }
}
