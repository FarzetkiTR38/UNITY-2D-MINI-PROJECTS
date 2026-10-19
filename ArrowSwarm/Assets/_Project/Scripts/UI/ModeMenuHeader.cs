namespace ArrowSwarm.UI
{
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.Modes;

    /// <summary>Provides one mode-aware localized value for both menu header refresh paths.</summary>
    internal static class ModeMenuHeader
    {
        /// <summary>Returns the selected campaign's highest unlocked level with the current language prefix.</summary>
        public static string LevelText()
        {
            string prefix = LocalizationManager.HasInstance
                ? LocalizationManager.Instance.GetText("menu_level_short", "Lv.") : "Lv.";
            int level = DataManager.HasInstance ? DataManager.Instance.GetHighestLevel(GameModeSelection.Selected) : 1;
            return prefix + level;
        }
    }
}
