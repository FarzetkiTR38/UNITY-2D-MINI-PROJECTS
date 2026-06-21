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
