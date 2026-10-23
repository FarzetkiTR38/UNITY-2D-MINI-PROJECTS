namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using ArrowSwarm.Data;
    using UnityEngine;

    /// <summary>Checks production leaderboard identity and display isolation without network requests.</summary>
    public static class QualityLeaderboardVerification
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo CacheField = typeof(LeaderboardManager).GetField("_cachedEntries", Flags);
        private static readonly FieldInfo RankField = typeof(LeaderboardManager).GetField("_cachedPlayerRank", Flags);
        private static readonly List<string> Results = new List<string>();

        /// <summary>Runs in the main menu's Play Mode and restores real cache, data and preferences in finally.</summary>
        public static string Run()
        {
            if (!Application.isPlaying || !DataManager.HasInstance)
                throw new InvalidOperationException("Open the main menu in Play Mode before verifying leaderboard rows.");
            Results.Clear(); bool hadSingleton = LeaderboardManager.HasInstance;
            GameObject temporary = null; LeaderboardManager manager;
            if (hadSingleton) manager = LeaderboardManager.Instance;
            else
            {
                // Inactive components never run Awake, so this fixture cannot initialize UGS or become the singleton.
                temporary = new GameObject("Isolated leaderboard verification"); temporary.SetActive(false);
                manager = temporary.AddComponent<LeaderboardManager>();
            }
            var snapshot = new QualityDataSnapshot(DataManager.Instance);
            object originalCache = CacheField.GetValue(manager), originalRank = RankField.GetValue(manager);
            bool passed = false; string path = "Captures/QualityPass/leaderboard-verification.txt";
            try
            {
                var player = PlayerData.CreateDefault(); player.playerId = "local-save-id";
                player.playerName = "Player"; player.playerCountry = "TR"; player.highestLevel = 20;
                player.levelStars.Add(new LevelStarData { level = 1, stars = 3 });
                player.shapesProgress.Complete(1, 2, 50); player.colorShiftProgress.Complete(1, 3, 50);
                snapshot.SetData(player);
                VerifyNamesAndCopies(manager, player);
                VerifySorting(manager, player);
                VerifyEmptyCaches(manager);
                passed = true;
            }
            catch (Exception error) { Results.Add("FAIL: " + error.Message); throw; }
            finally
            {
                try
                {
                    CacheField.SetValue(manager, originalCache); RankField.SetValue(manager, originalRank);
                    snapshot.Dispose(); snapshot.VerifyRestored(Results);
                    Check(ReferenceEquals(CacheField.GetValue(manager), originalCache) &&
                        Equals(RankField.GetValue(manager), originalRank), "original leaderboard cache object and rank restored");
                }
                finally { if (temporary != null) UnityEngine.Object.DestroyImmediate(temporary); }
                Check(LeaderboardManager.HasInstance == hadSingleton, "lazy production singleton existence remains unchanged");
                Results.Insert(0, passed ? "PASS: production leaderboard identity, copy and ordering checks" : "FAIL: leaderboard verification");
                Directory.CreateDirectory("Captures/QualityPass"); File.WriteAllLines(path, Results);
            }
            return Path.GetFullPath(path);
        }

        private static LeaderboardEntry Row(string id, string name, int level, int stars, bool own = false)
        {
            return new LeaderboardEntry { PlayerId = id, PlayerName = name, HighestLevel = level,
                TotalStars = stars, IsPlayer = own, CountryCode = "US", LastUpdated = "fixture-time" };
        }

        private static void VerifyNamesAndCopies(LeaderboardManager manager, PlayerData player)
        {
            var rival = Row("rival-a", "Player", 2, 1);
            var secondRival = Row("rival-b", "Player", 3, 2);
            var own = Row("cloud-local", "Old Name", 5, 2, true);
            var cache = new List<LeaderboardEntry> { rival, null, secondRival, own };
            string beforeRival = JsonUtility.ToJson(rival), beforeOwn = JsonUtility.ToJson(own);
            CacheField.SetValue(manager, cache);
            var rows = manager.GetTopPlayers(10);
            Check(rows.Count == 3, "null cached rows are skipped without hiding real players");
            Check(rows.FindAll(row => row.IsPlayer).Count == 1, "duplicate display names never create extra local accounts");
            Check(!rows.Find(row => row.PlayerId == "rival-a").IsPlayer &&
                !rows.Find(row => row.PlayerId == "rival-b").IsPlayer, "same-name rivals retain their account identity");
            Check(rows.Find(row => row.PlayerId == "rival-a").HighestLevel == 2 &&
                rows.Find(row => row.PlayerId == "rival-a").TotalStars == 1, "rival scores are never replaced by local progress");
            var local = rows.Find(row => row.IsPlayer);
            Check(local.PlayerId == "cloud-local", "authenticated cloud player ID is preserved");
            Check(local.PlayerName == "Player" && local.CountryCode == "TR", "local nickname and country replace stale cloud metadata");
            Check(local.HighestLevel == 20 && local.TotalStars == 8, "all three campaigns supply the local score snapshot");
            Check(local.LastUpdated == "fixture-time", "cloud row timestamp remains available to the display");
            Check(!ReferenceEquals(rows.Find(row => row.PlayerId == "rival-a"), rival) &&
                !ReferenceEquals(local, own), "both rival and local display rows are detached copies");
            Check(JsonUtility.ToJson(rival) == beforeRival && JsonUtility.ToJson(own) == beforeOwn,
                "building the display never changes cached metadata or scores");

            rows.Find(row => row.PlayerId == "rival-a").TotalStars = 999; local.PlayerName = "Changed display only";
            var again = manager.GetTopPlayers(10);
            Check(again.Find(row => row.PlayerId == "rival-a").TotalStars == 1 &&
                again.Find(row => row.IsPlayer).PlayerName == "Player", "mutating returned rows cannot poison later refreshes");
            player.playerName = "Renamed"; player.playerCountry = "JP"; player.highestLevel = 22;
            player.levelStars.Add(new LevelStarData { level = 2, stars = 3 });
            local = manager.GetTopPlayers(10).Find(row => row.IsPlayer);
            Check(local.PlayerName == "Renamed" && local.CountryCode == "JP" && local.HighestLevel == 22 && local.TotalStars == 11,
                "a local profile and score change immediately replaces stale cached player metadata");
            Check(JsonUtility.ToJson(rival) == beforeRival && JsonUtility.ToJson(own) == beforeOwn,
                "repeated refreshes and profile changes leave the original cached rows byte identical");

            own.HighestLevel = 30; own.TotalStars = 40;
            local = manager.GetTopPlayers(10).Find(row => row.IsPlayer);
            Check(local.HighestLevel == 30 && local.TotalStars == 40 && local.PlayerName == "Renamed" && local.CountryCode == "JP",
                "cloud personal bests ahead of offline progress retain the original maximum-score policy");

            CacheField.SetValue(manager, new List<LeaderboardEntry> { rival, secondRival });
            rows = manager.GetTopPlayers(10);
            Check(rows.Count == 3 && rows.FindAll(row => row.IsPlayer).Count == 1,
                "missing local cloud entry inserts one local row despite duplicate rival names");
            Check(rows.Find(row => row.IsPlayer).PlayerId == "local-save-id", "offline inserted local row retains the saved player ID");
            player.playerName = "Player"; player.playerCountry = "TR"; player.highestLevel = 20;
            player.levelStars.RemoveAt(1);
        }

        private static void VerifySorting(LeaderboardManager manager, PlayerData player)
        {
            var own = Row("cloud-local", "Previous", 1, 0, true);
            CacheField.SetValue(manager, new List<LeaderboardEntry> { Row("lower", "Lower", 19, 999),
                own, Row("tie-high", "Tie", 20, 9), Row("top", "Top", 21, 0), Row("tie-low", "Tie", 20, 1) });
            var rows = manager.GetTopPlayers(4);
            Check(rows.Count == 4, "top-count limit remains enforced");
            Check(rows[0].PlayerId == "top" && rows[1].PlayerId == "tie-high" &&
                rows[2].PlayerId == "cloud-local" && rows[3].PlayerId == "tie-low",
                "ordering remains highest level first, then aggregate stars");
            RankField.SetValue(manager, -1);
            Check(manager.GetPlayerRank() == 3, "offline rank identifies the actual local row after sorting");
            RankField.SetValue(manager, 27);
            Check(manager.GetPlayerRank() == 27, "an authoritative cloud rank retains priority");
            CacheField.SetValue(manager, new List<LeaderboardEntry> { Row("first-tie", "A", 30, 7), Row("second-tie", "B", 30, 7) });
            rows = manager.GetTopPlayers(2);
            Check(rows[0].PlayerId == "first-tie" && rows[1].PlayerId == "second-tie", "equal-score cache ordering remains stable");
        }

        private static void VerifyEmptyCaches(LeaderboardManager manager)
        {
            CacheField.SetValue(manager, null);
            Check(manager.GetTopPlayers(10).Count == 1 && manager.GetTopPlayers(10)[0].IsPlayer,
                "a missing cache still displays the local player");
            CacheField.SetValue(manager, new List<LeaderboardEntry> { null, null });
            Check(manager.GetTopPlayers(10).Count == 1, "an all-null cache safely displays the local player");
            CacheField.SetValue(manager, new List<LeaderboardEntry>());
            Check(manager.GetTopPlayers(10).Count == 1, "an empty cache safely displays the local player");
            Check(manager.GetTopPlayers(0).Count == 0 && manager.GetTopPlayers(-1).Count == 0,
                "nonpositive display limits return no rows");
        }

        private static void Check(bool value, string message)
        {
            if (!value) throw new InvalidOperationException(message);
            Results.Add("PASS: " + message);
        }
    }
}
