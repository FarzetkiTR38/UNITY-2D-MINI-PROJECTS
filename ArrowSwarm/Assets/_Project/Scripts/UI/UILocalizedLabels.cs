namespace ArrowSwarm.UI
{
    using ArrowSwarm.Localization;

    /// <summary>Resolves existing dynamic UI labels without changing their authored visual styling.</summary>
    internal static class UILocalizedLabels
    {
        /// <summary>Returns an existing dictionary value or a safe startup fallback.</summary>
        public static string Text(string key, string fallback) => LocalizationManager.HasInstance
            ? LocalizationManager.Instance.GetText(key, fallback) : fallback;
    }
}
