namespace ArrowSwarm.Data
{
    using System;
    using System.Collections.Generic;

    /// <summary>Separates a successful empty board from unrequested, pending or unavailable results.</summary>
    public enum LeaderboardRefreshState { NotRequested, Loading, Ready, Offline, Failed }

    /// <summary>Exposes the provenance of global results independently of optimistic local progress.</summary>
    public partial class LeaderboardManager
    {
        private DateTime? _lastSuccessfulRefreshUtc;

        /// <summary>State of the latest global board request, including its in-flight loading state.</summary>
        public LeaderboardRefreshState RefreshState { get; private set; }

        /// <summary>UTC time of the last accepted global snapshot, if known from this or a previous session.</summary>
        public DateTime? LastSuccessfulRefreshUtc => _lastSuccessfulRefreshUtc;

        /// <summary>Whether actual provider rows are available, even while offline or refreshing.</summary>
        public bool HasCachedResults => _cachedEntries != null && _cachedEntries.Count > 0;

        /// <summary>Returns detached global snapshot rows in provider order; never invents or reorders a local account.</summary>
        public List<LeaderboardEntry> GetRankedPlayers(int count = 10)
        {
            var rows = new List<LeaderboardEntry>();
            if (_cachedEntries == null || count <= 0) return rows;
            foreach (var entry in _cachedEntries)
            {
                if (entry == null) continue;
                var copy = CopyDisplayEntry(entry);
                copy.IsPlayer = entry.IsPlayer;
                rows.Add(copy);
                if (rows.Count >= count) break;
            }
            return rows;
        }

        /// <summary>Returns only a rank verified by the latest successful refresh; a negative value means unknown.</summary>
        public int GetVerifiedPlayerRank() => RefreshState == LeaderboardRefreshState.Ready ? _cachedPlayerRank : -1;
    }
}
