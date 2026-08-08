namespace ArrowSwarm.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ArrowSwarm.Utils;
    using UnityEngine;

    public class LeaderboardManager : Singleton<LeaderboardManager>
    {
        [Serializable]
        public class LeaderboardEntry
        {
            public string PlayerName;
            public int HighestLevel;
            public int TotalStars;
            public bool IsPlayer;
        }

        private List<LeaderboardEntry> _cachedEntries;
        private const string SAVE_KEY = "ArrowSwarm_FakeLeaderboard";

        protected override void OnSingletonAwake()
        {
            GenerateFakeLeaderboardIfNeeded();
        }

        private void GenerateFakeLeaderboardIfNeeded()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                var wrapper = JsonUtility.FromJson<LeaderboardWrapper>(json);
                _cachedEntries = wrapper.Entries;
            }
            else
            {
                _cachedEntries = new List<LeaderboardEntry>();
                string[] fakeNames = { "ArrowGod", "Sniper99", "NoobSlayer", "RobinHood", "Legolas", "Hawkeye", "BowMaster", "SwiftArrow", "EagleEye", "ShadowArcher" };
                
                System.Random rnd = new System.Random();
                foreach (string name in fakeNames)
                {
                    int fakeLevel = rnd.Next(5, 30);
                    int fakeStars = fakeLevel * rnd.Next(1, 4); // Random stars
                    _cachedEntries.Add(new LeaderboardEntry { PlayerName = name, HighestLevel = fakeLevel, TotalStars = fakeStars, IsPlayer = false });
                }

                SaveLeaderboard();
            }
        }

        private void SaveLeaderboard()
        {
            var wrapper = new LeaderboardWrapper { Entries = _cachedEntries };
            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        public List<LeaderboardEntry> GetTopPlayers(int count)
        {
            // Always inject current player data dynamically so it's fresh
            var currentData = DataManager.Instance.PlayerData;
            
            // Calculate total stars for player
            int playerStars = 0;
            if (currentData.levelStars != null)
            {
                foreach (var starData in currentData.levelStars)
                {
                    playerStars += starData.stars;
                }
            }

            var playerEntry = new LeaderboardEntry 
            { 
                PlayerName = currentData.playerName, 
                HighestLevel = currentData.highestLevel, 
                TotalStars = playerStars,
                IsPlayer = true
            };

            var allEntries = new List<LeaderboardEntry>(_cachedEntries);
            allEntries.Add(playerEntry);

            // Sort by level first, then stars
            var sorted = allEntries.OrderByDescending(e => e.HighestLevel)
                                   .ThenByDescending(e => e.TotalStars)
                                   .Take(count)
                                   .ToList();
            return sorted;
        }

        public int GetPlayerRank()
        {
            var currentData = DataManager.Instance.PlayerData;
            int playerStars = 0;
            if (currentData.levelStars != null)
            {
                foreach (var starData in currentData.levelStars)
                {
                    playerStars += starData.stars;
                }
            }

            var allEntries = new List<LeaderboardEntry>(_cachedEntries);
            allEntries.Add(new LeaderboardEntry 
            { 
                PlayerName = currentData.playerName, 
                HighestLevel = currentData.highestLevel, 
                TotalStars = playerStars,
                IsPlayer = true 
            });

            var sorted = allEntries.OrderByDescending(e => e.HighestLevel)
                                   .ThenByDescending(e => e.TotalStars)
                                   .ToList();

            return sorted.FindIndex(e => e.IsPlayer) + 1;
        }

        [Serializable]
        private class LeaderboardWrapper
        {
            public List<LeaderboardEntry> Entries;
        }
    }
}
