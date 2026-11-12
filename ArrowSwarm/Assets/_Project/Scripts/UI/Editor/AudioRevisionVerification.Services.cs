namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using ArrowSwarm.Ads;
    using ArrowSwarm.Data;

    /// <summary>Prevents native audio checks from using live leaderboard or ad providers.</summary>
    public static partial class AudioRevisionVerification
    {
        private sealed class Services : IDisposable
        {
            private readonly FieldInfo _factory = typeof(LeaderboardManager).GetField("_editorCloudFactory", BindingFlags.Static | BindingFlags.NonPublic);
            private readonly object _originalFactory;
            private readonly ILeaderboardCloudService _originalCloud;
            private readonly IAdService _originalAds;
            private readonly bool _hadLeaderboard;
            private readonly Offline _offline = new Offline();
            internal Services()
            {
                _originalFactory = _factory.GetValue(null); _hadLeaderboard = LeaderboardManager.HasInstance;
                _factory.SetValue(null, new Func<ILeaderboardCloudService>(() => _offline));
                if (_hadLeaderboard)
                {
                    _originalCloud = Field<ILeaderboardCloudService>(LeaderboardManager.Instance, "_cloudService");
                    typeof(LeaderboardManager).GetField("_cloudService", Private).SetValue(LeaderboardManager.Instance, _offline);
                }
                _originalAds = Field<IAdService>(AdManager.Instance, "_adService");
                AdManager.Instance.SetAdService(new NoAds());
            }
            public void Dispose()
            {
                AdManager.Instance.SetAdService(_originalAds);
                _factory.SetValue(null, _originalFactory);
                if (!_hadLeaderboard && LeaderboardManager.HasInstance) UnityEngine.Object.DestroyImmediate(LeaderboardManager.Instance.gameObject);
                else if (_hadLeaderboard && LeaderboardManager.HasInstance)
                    typeof(LeaderboardManager).GetField("_cloudService", Private).SetValue(LeaderboardManager.Instance, _originalCloud);
                Check(_offline.OnlineRequests == 0, "No live leaderboard operation was requested");
            }
        }
        private sealed class NoAds : IAdService
        {
            public bool IsAdReady() => false;
            public bool IsInterstitialAdReady() => false;
            public void ShowRewardedAd(Action<bool> completed) => completed?.Invoke(false);
            public void ShowInterstitialAd(Action completed) => completed?.Invoke();
        }
        private sealed class Offline : ILeaderboardCloudService
        {
            internal int OnlineRequests;
            public bool IsOnline() => false;
            public Task<bool> EnsureInitializedAsync() { OnlineRequests++; return Task.FromResult(false); }
            public Task<bool> UpdatePlayerNameAsync(string name) { OnlineRequests++; return Task.FromResult(false); }
            public Task<bool> SubmitScoreAsync(int level, int stars, string name, string country) { OnlineRequests++; return Task.FromResult(false); }
            public Task<List<LeaderboardEntry>> FetchTopScoresAsync(int limit = 10) { OnlineRequests++; return Task.FromResult<List<LeaderboardEntry>>(null); }
            public Task<int> FetchPlayerRankAsync() { OnlineRequests++; return Task.FromResult(-1); }
        }
    }
}
