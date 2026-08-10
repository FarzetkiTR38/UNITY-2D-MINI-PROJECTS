using System;
using System.Collections.Generic;

namespace NeonGalaxy.Data
{
    /// <summary>
    /// Local representation of a single leaderboard entry.
    /// </summary>
    [Serializable]
    public class LeaderboardEntry
    {
        public int rank;
        public string playerName;
        public string playerId;
        public int score;
        /// <summary>
        /// URL or local path for the player's profile picture.
        /// Empty string means default avatar should be used.
        /// Phase-2: Will be populated from server for other players.
        /// </summary>
        public string avatarUrl = "";
    }

    /// <summary>
    /// Cached leaderboard data for offline display.
    /// </summary>
    [Serializable]
    public class CachedLeaderboard
    {
        public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
        public LeaderboardEntry playerEntry;
        public long lastFetchTimestamp; // Unix timestamp in seconds
    }
}
