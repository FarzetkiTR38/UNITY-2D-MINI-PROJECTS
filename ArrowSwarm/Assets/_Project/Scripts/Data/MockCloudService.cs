namespace ArrowSwarm.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Mock implementation of ICloudService for development/testing.
    /// Returns fake leaderboard data and simulates save operations.
    /// </summary>
    public class MockCloudService : MonoBehaviour, ICloudService
    {
        [SerializeField] private float _simulatedLatency = 0.5f;

        private static MockCloudService _instance;

        /// <summary>Singleton-like access for the mock service.</summary>
        public static MockCloudService Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        /// <inheritdoc/>
        public void SavePlayerData(PlayerData data, Action<bool> onComplete)
        {
            StartCoroutine(SimulateSave(data, onComplete));
        }

        /// <inheritdoc/>
        public void LoadLeaderboard(Action<List<LeaderboardEntry>> onComplete)
        {
            StartCoroutine(SimulateLoadLeaderboard(onComplete));
        }

        /// <inheritdoc/>
        public bool IsOnline() => Application.internetReachability != NetworkReachability.NotReachable;

        private IEnumerator SimulateSave(PlayerData data, Action<bool> onComplete)
        {
            yield return new WaitForSecondsRealtime(_simulatedLatency);
            LogDebug($"Mock save: {data.playerName} - Level {data.highestLevel}");
            onComplete?.Invoke(true);
        }

        private IEnumerator SimulateLoadLeaderboard(Action<List<LeaderboardEntry>> onComplete)
        {
            yield return new WaitForSecondsRealtime(_simulatedLatency);

            var entries = new List<LeaderboardEntry>
            {
                new LeaderboardEntry { PlayerName = "ProPlayer", HighestLevel = 892 },
                new LeaderboardEntry { PlayerName = "ArrowKing", HighestLevel = 756 },
                new LeaderboardEntry { PlayerName = "SwarmMaster", HighestLevel = 643 },
                new LeaderboardEntry { PlayerName = "PathFinder", HighestLevel = 521 },
                new LeaderboardEntry { PlayerName = "GridLord", HighestLevel = 478 },
                new LeaderboardEntry { PlayerName = "MobSlayer", HighestLevel = 412 },
                new LeaderboardEntry { PlayerName = "QuickShot", HighestLevel = 367 },
                new LeaderboardEntry { PlayerName = "TipMaster", HighestLevel = 298 },
                new LeaderboardEntry { PlayerName = "LevelCrusher", HighestLevel = 245 },
                new LeaderboardEntry { PlayerName = "BugHunter", HighestLevel = 189 },
            };

            LogDebug($"Mock leaderboard loaded: {entries.Count} entries");
            onComplete?.Invoke(entries);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] MockCloudService: {message}");
        }
    }
}
