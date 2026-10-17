namespace ArrowSwarm.Data
{
    using ArrowSwarm.Modes;

    /// <summary>Additive fields preserve the legacy Defense save format and stars.</summary>
    public partial class PlayerData
    {
        public GameModeId selectedGameMode = GameModeId.Defense;
        public ModeProgress shapesProgress = new ModeProgress();
        public ModeProgress colorShiftProgress = new ModeProgress();

        /// <summary>Gets a new campaign record, including initialization of older JSON saves.</summary>
        public ModeProgress GetModeProgress(GameModeId mode)
        {
            if (mode == GameModeId.Shapes) return shapesProgress ?? (shapesProgress = new ModeProgress());
            if (mode == GameModeId.ColorShift) return colorShiftProgress ?? (colorShiftProgress = new ModeProgress());
            throw new System.ArgumentException("Defense uses the original progress fields.", nameof(mode));
        }
    }
}
