namespace ArrowSwarm.UI
{
    using System;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using TMPro;
    using UnityEngine.UI;

    /// <summary>
    /// Refreshes translated actions and keeps the Profile country/language selector
    /// synchronized with LocalizationManager and SettingsUI without resetting the draft name or modal animation.
    /// </summary>
    public partial class ProfileSetupModalUI
    {
        private void ApplyLocalization()
        {
            SyncCountryWithActiveLanguage();
            RefreshAction(_saveButton, "profile_save_play", "SAVE");
            RefreshAction(_skipButton, "profile_skip", "SKIP");
        }

        private void SyncCountryWithActiveLanguage()
        {
            if (!LocalizationManager.HasInstance) return;
            string activeLang = LocalizationManager.NormalizeLanguageCode(LocalizationManager.Instance.CurrentLanguage);
            for (int i = 0; i < COUNTRIES.Length; i++)
            {
                if (COUNTRIES[i].LanguageCode.Equals(activeLang, StringComparison.OrdinalIgnoreCase))
                {
                    _currentCountryIndex = i;
                    UpdateCountryDisplay();
                    return;
                }
            }
        }

        private void ApplySelectedLanguage()
        {
            if (_currentCountryIndex < 0 || _currentCountryIndex >= COUNTRIES.Length) return;
            string langCode = COUNTRIES[_currentCountryIndex].LanguageCode;
            if (LocalizationManager.HasInstance)
            {
                LocalizationManager.Instance.SetLanguage(langCode, saveToPrefs: true);
            }
            else if (DataManager.HasInstance)
            {
                DataManager.Instance.SetLanguage(langCode);
            }
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
