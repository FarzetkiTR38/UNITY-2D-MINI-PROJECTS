#if UNITY_EDITOR
namespace ArrowSwarm.UI
{
    using System;
    using System.Collections.Generic;
    using ArrowSwarm.Data;
    using UnityEngine;

    /// <summary>Display-only rivals mixed with fetched rows and fresh local progress; never cached or submitted.</summary>
    public partial class LeaderboardUI
    {
        [Header("Editor preview only — excluded from player builds")]
        [SerializeField] private bool _showSampleRivalsInEditor = true;

        private List<LeaderboardEntry> MixedEditorRows(List<LeaderboardEntry> live)
        {
            if (!_showSampleRivalsInEditor) return live;
            var rows = new List<LeaderboardEntry>();
            if (live != null) foreach (var entry in live) if (!entry.IsPlayer) rows.Add(entry);
            string[] names = { "NovaRider", "ArrowFox", "PixelMage", "LunaQuest", "StarPilot",
                "NeonTiger", "MapleArcher", "CloudDancer", "BlueComet", "SunnyWings",
                "MoonArrow", "CoralWing", "JadeRunner", "AmberStar", "VelvetSky" };
            string[] countries = { "TR", "US", "GB", "DE", "FR", "BR", "JP", "KR", "CA", "AU" };
            for (int i = 0; i < names.Length; i++) rows.Add(new LeaderboardEntry
            {
                PlayerId = "ui-only-sample-" + i, PlayerName = names[i] + " (sample)",
                CountryCode = countries[i % countries.Length], HighestLevel = 5 + i * 7,
                TotalStars = i * 9
            });
            var player = DataManager.HasInstance ? DataManager.Instance.PlayerData : null;
            if (player != null) rows.Add(new LeaderboardEntry
            {
                PlayerId = "ui-only-current-local", PlayerName = player.playerName,
                CountryCode = player.playerCountry, HighestLevel = player.highestLevel,
                TotalStars = player.GetTotalStars(), IsPlayer = true
            });
            rows.Sort((a, b) => {
                int score = b.TotalStars.CompareTo(a.TotalStars);
                if (score != 0) return score;
                int level = b.HighestLevel.CompareTo(a.HighestLevel);
                return level != 0 ? level : string.Compare(a.PlayerId, b.PlayerId, StringComparison.Ordinal);
            });
            return rows;
        }
    }
}
#endif
