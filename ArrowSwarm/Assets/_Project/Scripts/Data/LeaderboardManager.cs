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
                _cachedEntries = wrapper?.Entries ?? new List<LeaderboardEntry>();
            }
            else
            {
                _cachedEntries = new List<LeaderboardEntry>();
                string[] fakeNames = { "ArrowGod", "Sniper99", "NoobSlayer", "RobinHood", "Legolas", "Hawkeye", "BowMaster", "SwiftArrow", "EagleEye", "ShadowArcher" };
                int[] baseLevels = { 123, 118, 112, 108, 101, 96, 89, 84, 79, 74 };
                int[] baseStars = { 320, 309, 294, 281, 268, 251, 233, 220, 208, 195 };
                
                for (int i = 0; i < fakeNames.Length; i++)
                {
                    int fakeLevel = i < baseLevels.Length ? baseLevels[i] : (70 - i * 5);
                    int fakeStars = i < baseStars.Length ? baseStars[i] : (fakeLevel * 2 + 10);
                    _cachedEntries.Add(new LeaderboardEntry { PlayerName = fakeNames[i], HighestLevel = fakeLevel, TotalStars = fakeStars, IsPlayer = false });
                }

                SaveLeaderboard();
            }
        }

        /// <summary>
        /// Updates the current player's username.
        /// </summary>
        public void SetPlayerName(string newName)
        {
            DataManager.Instance?.SetPlayerName(newName);
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
            var currentData = DataManager.Instance?.PlayerData;
            int playerLevel = currentData?.highestLevel ?? 1;
            int playerStars = currentData?.GetTotalStars() ?? 0;
            string playerName = currentData?.playerName ?? "Player";

            var playerEntry = new LeaderboardEntry 
            { 
                PlayerName = playerName, 
                HighestLevel = playerLevel, 
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
            var currentData = DataManager.Instance?.PlayerData;
            int playerLevel = currentData?.highestLevel ?? 1;
            int playerStars = currentData?.GetTotalStars() ?? 0;
            string playerName = currentData?.playerName ?? "Player";

            var allEntries = new List<LeaderboardEntry>(_cachedEntries);
            allEntries.Add(new LeaderboardEntry 
            { 
                PlayerName = playerName, 
                HighestLevel = playerLevel, 
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
