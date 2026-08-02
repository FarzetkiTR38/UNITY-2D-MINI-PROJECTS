namespace ArrowSwarm.Data
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface for cloud data services (leaderboard, player sync).
    /// Allows swapping between mock and Firebase implementations.
    /// </summary>
    public interface ICloudService
    {
        /// <summary>
        /// Saves player data to the cloud.
        /// </summary>
        void SavePlayerData(PlayerData data, Action<bool> onComplete);

        /// <summary>
        /// Loads the leaderboard from the cloud.
        /// </summary>
        void LoadLeaderboard(Action<List<LeaderboardEntry>> onComplete);

        /// <summary>
        /// Returns true if the device has internet connectivity.
        /// </summary>
        bool IsOnline();
    }

    /// <summary>
    /// Represents a single leaderboard entry.
    /// </summary>
    [System.Serializable]
    public class LeaderboardEntry
    {
        public string PlayerId;
        public string PlayerName;
        public int HighestLevel;
        public string LastUpdated;
    }
}
