namespace ArrowSwarm.Data
{
    using ArrowSwarm.Modes;

    /// <summary>Mode-aware operations leave all original Defense entry points intact.</summary>
    public partial class DataManager
    {
        /// <summary>Persists menu selection; invalid legacy values default to Defense.</summary>
        public void SelectMode(GameModeId mode)
        {
            if (!System.Enum.IsDefined(typeof(GameModeId), mode)) mode = GameModeId.Defense;
            PlayerData.selectedGameMode = mode;
            NotifyAndSave();
        }

        /// <summary>Returns independent unlock progress for the selected campaign.</summary>
        public int GetHighestLevel(GameModeId mode) => mode == GameModeId.Defense
            ? PlayerData.highestLevel : PlayerData.GetModeProgress(mode).HighestLevel;

        /// <summary>Returns independent current level for the selected campaign.</summary>
        public int GetCurrentLevel(GameModeId mode) => mode == GameModeId.Defense
            ? PlayerData.currentLevel : PlayerData.GetModeProgress(mode).CurrentLevel;

        /// <summary>Returns the best star record from the specified campaign.</summary>
        public int GetLevelStars(GameModeId mode, int level) => mode == GameModeId.Defense
            ? GetLevelStars(level) : PlayerData.GetModeProgress(mode).GetStars(level);

        /// <summary>Selects an unlocked puzzle level, preserving legacy Defense selection semantics.</summary>
        public void SetCurrentLevel(GameModeId mode, int level)
        {
            if (mode == GameModeId.Defense) { SetCurrentLevel(level); return; }
            PlayerData.GetModeProgress(mode).Select(level);
            NotifyAndSave();
        }

        /// <summary>Stores a valid puzzle result and refreshes the existing aggregate leaderboard.</summary>
        public void CompletePuzzleLevel(GameModeId mode, int level, int lives, int levelCount)
        {
            if (mode == GameModeId.Defense) return;
            PlayerData.GetModeProgress(mode).Complete(level, lives, levelCount);
            NotifyAndSave();
        }
    }
}
