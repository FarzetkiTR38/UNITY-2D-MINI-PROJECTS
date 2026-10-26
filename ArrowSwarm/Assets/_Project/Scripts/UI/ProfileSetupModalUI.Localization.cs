namespace ArrowSwarm.UI
{
    using ArrowSwarm.Localization;
    using TMPro;
    using UnityEngine.UI;

    /// <summary>Refreshes translated actions without reinitializing a draft name, country or modal animation.</summary>
    public partial class ProfileSetupModalUI
    {
        private void ApplyLocalization()
        {
            RefreshAction(_saveButton, "profile_save_play", "SAVE");
            RefreshAction(_skipButton, "profile_skip", "SKIP");
        }

        private static void RefreshAction(Button button, string key, string fallback)
        {
            var label = button != null ? button.GetComponentInChildren<TextMeshProUGUI>(true) : null;
            if (label == null) return;
            var localized = label.GetComponent<LocalizedText>();
            if (localized != null) localized.RefreshText();
            else label.text = UILocalizedLabels.Text(key, fallback);
        }
    }
}
