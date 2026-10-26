namespace ArrowSwarm.UI
{
    using ArrowSwarm.Data;

    /// <summary>Displays real global rows while keeping current unsynchronized progress separate.</summary>
    public partial class LeaderboardUI
    {
        /// <summary>Refreshes data and localized status without changing authored fonts, colors or geometry.</summary>
        public void RefreshLeaderboardData()
        {
            AutoWire();
            var manager = LeaderboardManager.HasInstance ? LeaderboardManager.Instance : null;
            int count = _entryRows != null ? _entryRows.Length : 0;
            var entries = manager?.GetRankedPlayers(count);
            if (_entryRows != null)
                for (int i = 0; i < _entryRows.Length; i++)
                {
                    var row = _entryRows[i];
                    if (row == null) continue;
                    bool present = entries != null && i < entries.Count;
                    row.gameObject.SetActive(present);
                    if (!present) continue;
                    var entry = entries[i];
                    row.Setup(i + 1, entry.PlayerName, entry.HighestLevel, entry.TotalStars, entry.IsPlayer, entry.CountryCode);
                }

            var data = DataManager.HasInstance ? DataManager.Instance.PlayerData : null;
            if (data != null)
            {
                string tag = GetCountryTag(data.playerCountry);
                string name = string.IsNullOrEmpty(tag) ? data.playerName : $"{tag} {data.playerName}";
                int rank = manager?.GetVerifiedPlayerRank() ?? -1;
                string rankText = rank > 0 ? $"#{rank}" : "—";
                if (_currentPlayerNameText != null) _currentPlayerNameText.text = name;
                if (_currentPlayerRankText != null) _currentPlayerRankText.text = rankText;
                if (_currentPlayerLevelText != null) _currentPlayerLevelText.text = Text("menu_level_short", "Lv.") + data.highestLevel;
                if (_currentPlayerStarsText != null) _currentPlayerStarsText.text = data.GetTotalStars().ToString();
                if (_nameInputField != null && !_nameInputField.isFocused) _nameInputField.SetTextWithoutNotify(data.playerName);
                if (_playerSummaryText != null)
                    _playerSummaryText.text = Format("leaderboard_player_summary", "{0}  •  Lv.{1}  •  {2} stars\nWorld rank: {3}",
                        name, data.highestLevel, data.GetTotalStars(), rankText);
            }
            RefreshStatus(manager, entries != null && entries.Count > 0);
        }
    }
}
