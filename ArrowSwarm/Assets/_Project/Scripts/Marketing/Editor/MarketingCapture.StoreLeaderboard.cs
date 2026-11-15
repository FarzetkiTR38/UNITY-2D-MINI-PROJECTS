namespace ArrowSwarm.Marketing.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ArrowSwarm.Data;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Fictional in-process leaderboard data shown by the real English runtime UI, never submitted externally.</summary>
    public static partial class MarketingCapture
    {
        private static IEnumerator StoreLeaderboard()
        {
            StoreEnglish(); SceneManager.LoadScene("MainMenuScene"); yield return StoreWait(60); StoreEnglish();
            _storeDemo = new StoreDemoCloud();
            Factory.SetValue(null, new Func<ILeaderboardCloudService>(() => _storeDemo));
            var manager = LeaderboardManager.Instance;
            typeof(LeaderboardManager).GetField("_cloudService", Private).SetValue(manager, _storeDemo);
            var menu = UnityEngine.Object.FindFirstObjectByType<ArrowSwarm.UI.MainMenuUI>();
            menu.OpenLeaderboard(); yield return StoreWait(2);
            var board = UnityEngine.Object.FindFirstObjectByType<ArrowSwarm.UI.LeaderboardUI>();
            if (board == null) throw new InvalidOperationException("Leaderboard did not open.");
            typeof(ArrowSwarm.UI.LeaderboardUI).GetField("_showSampleRivalsInEditor", Private).SetValue(board, false);
            var refresh = manager.RefreshFromCloudAsync();
            while (!refresh.IsCompleted) yield return null;
            if (refresh.IsFaulted || manager.RefreshState != LeaderboardRefreshState.Ready)
                throw new InvalidOperationException("In-process demonstration board did not refresh.");
            board.RefreshLeaderboardData(); yield return StoreWait(45);
            yield return StoreSave("08_Leaderboard_EN", "Leaderboard", 20,
                "Fictional Nova and demonstration rivals through real provider/UI refresh. No live players, external requests or verified global rank claims.");
        }

        private sealed class StoreDemoCloud : ILeaderboardCloudService
        {
            internal int Calls { get; private set; }
            /// <summary>Reports this local demonstration provider as available.</summary>
            public bool IsOnline() { Calls++; return true; }
            /// <summary>Completes local initialization without authentication or network.</summary>
            public Task<bool> EnsureInitializedAsync() { Calls++; return Task.FromResult(true); }
            /// <summary>Accepts only an in-memory demonstration name.</summary>
            public Task<bool> UpdatePlayerNameAsync(string value) { Calls++; return Task.FromResult(true); }
            /// <summary>Accepts only an in-memory demonstration score; never uploads.</summary>
            public Task<bool> SubmitScoreAsync(int level, int stars, string name, string country) { Calls++; return Task.FromResult(true); }
            /// <summary>Returns fictional, ordered demonstration entries to the unmodified leaderboard renderer.</summary>
            public Task<List<LeaderboardEntry>> FetchTopScoresAsync(int limit = 10)
            {
                Calls++;
                string[] names = { "SkyRunner", "PixelNova", "LunaArrow", "NeonWing", "MapleFox", "Nova", "BlueComet", "CoralCloud", "SunnyOrbit", "JadeSpark" };
                string[] countries = { "GB", "CA", "JP", "DE", "FR", "US", "AU", "BR", "KR", "TR" };
                int[] scores = { 180, 168, 156, 144, 132, 108, 96, 84, 72, 60 };
                var list = new List<LeaderboardEntry>();
                for (int i = 0; i < Math.Min(limit, names.Length); i++) list.Add(new LeaderboardEntry
                {
                    PlayerId = i == 5 ? "store-demo-nova" : "store-demo-" + i,
                    PlayerName = names[i], CountryCode = countries[i], HighestLevel = 30 - i * 2,
                    TotalStars = scores[i], IsPlayer = i == 5
                });
                return Task.FromResult(list);
            }
            /// <summary>Returns only the fictional local player's position in the demonstration data.</summary>
            public Task<int> FetchPlayerRankAsync() { Calls++; return Task.FromResult(6); }
        }
    }
}
