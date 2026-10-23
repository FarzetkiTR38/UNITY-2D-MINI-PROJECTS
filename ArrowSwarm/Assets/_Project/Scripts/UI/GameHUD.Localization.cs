namespace ArrowSwarm.UI
{
    /// <summary>Refreshes cached Defense label data without replaying count feedback or gameplay events.</summary>
    public partial class GameHUD
    {
        private int _displayLevel = 1, _displayFired, _displayTotal;

        private void RefreshLocalizedGameplayLabels()
        {
            if (_levelText != null)
                _levelText.text = UILocalizedLabels.Text("menu_level_short", "Lv.") + _displayLevel;
            if (_arrowCountText != null)
                _arrowCountText.text = UILocalizedLabels.Text("hud_arrows", "Arrows") + ": " + _displayFired + "/" + _displayTotal;
        }
    }
}
