namespace ArrowSwarm.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>Builds detached display rows without treating mutable display names as account identities.</summary>
    public partial class LeaderboardManager
    {
        private List<LeaderboardEntry> BuildDisplayEntries(PlayerData data, int count)
        {
            var player = new LeaderboardEntry
            {
                PlayerId = data?.playerId,
                PlayerName = data?.playerName ?? "Player",
                HighestLevel = data?.highestLevel ?? 1,
                TotalStars = data?.GetTotalStars() ?? 0,
                IsPlayer = true,
                CountryCode = data?.playerCountry ?? "TR"
            };
            var entries = new List<LeaderboardEntry>();
            bool playerFound = false;
            if (_cachedEntries != null)
            {
                foreach (var cached in _cachedEntries)
                {
                    if (cached == null) continue;
                    // Cloud services determine this flag by authenticated player ID, never by nickname.
                    if (cached.IsPlayer)
                    {
                        if (playerFound) continue;
                        if (!string.IsNullOrEmpty(cached.PlayerId)) player.PlayerId = cached.PlayerId;
                        player.LastUpdated = cached.LastUpdated;
                        player.HighestLevel = Mathf.Max(cached.HighestLevel, player.HighestLevel);
                        player.TotalStars = Mathf.Max(cached.TotalStars, player.TotalStars);
                        entries.Add(player); playerFound = true;
                    }
                    else entries.Add(CopyDisplayEntry(cached));
                }
            }
            if (!playerFound) entries.Add(player);
            return entries.OrderByDescending(entry => entry.HighestLevel)
                .ThenByDescending(entry => entry.TotalStars).Take(count).ToList();
        }

        private static LeaderboardEntry CopyDisplayEntry(LeaderboardEntry entry)
        {
            return new LeaderboardEntry
            {
                PlayerId = entry.PlayerId,
                PlayerName = entry.PlayerName,
                HighestLevel = entry.HighestLevel,
                TotalStars = entry.TotalStars,
                IsPlayer = false,
                CountryCode = entry.CountryCode,
                LastUpdated = entry.LastUpdated
            };
        }
    }
}
