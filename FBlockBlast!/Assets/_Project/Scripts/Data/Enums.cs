namespace NeonGalaxy.Data
{
    /// <summary>
    /// Size category for piece definitions. Affects spawn weight heuristics
    /// and batch generator balancing.
    /// </summary>
    public enum PieceCategory
    {
        Tiny,   // 1 cell
        Small,  // 2-3 cells
        Medium, // 4 cells
        Large,  // 5 cells
        XL      // 6-9 cells
    }

    /// <summary>
    /// Category of cosmetic items for the profile/customization system.
    /// </summary>
    public enum CosmeticCategory
    {
        BoardSkin,
        BlockSkin,
        ProfileFrame,
        PlayerTitle
    }

    /// <summary>
    /// How a cosmetic item is unlocked.
    /// </summary>
    public enum UnlockCondition
    {
        Default,     // Available from the start
        Level,       // Unlocked at a specific player level
        Achievement, // Unlocked by earning an achievement
        IAP          // Purchased via in-app purchase
    }

    /// <summary>
    /// Gameplay state machine states for the core game loop.
    /// </summary>
    public enum GameState
    {
        WaitingForBatch,
        PieceSelection,
        PieceDragging,
        ValidPlacement,
        ClearAnimation,
        CheckGameOver,
        BatchComplete,
        GameOver,
        Paused,
        Reviving
    }
}
