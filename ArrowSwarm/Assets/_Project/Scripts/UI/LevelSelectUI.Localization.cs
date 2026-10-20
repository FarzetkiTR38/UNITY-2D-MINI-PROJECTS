namespace ArrowSwarm.UI
{
    /// <summary>Translates only the current range so a language switch never changes the selected page.</summary>
    public partial class LevelSelectUI
    {
        private void OnDisable() => ArrowSwarm.Localization.LocalizationManager.OnLanguageChanged -= RefreshPageLabel;

        private void RefreshPageLabel()
        {
            if (_pageText == null) return;
            int start = (_currentPage - 1) * LEVELS_PER_PAGE + 1;
            int end = UnityEngine.Mathf.Min(_currentPage * LEVELS_PER_PAGE, CampaignCount);
            _pageText.text = UILocalizedLabels.Text("menu_levels", "LEVELS") + " " + start + "-" + end;
        }
    }
}
