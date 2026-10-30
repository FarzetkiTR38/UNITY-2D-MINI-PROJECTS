namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Exercises actual setting callbacks and both exit controls; no external service or device is replaced.</summary>
    public static partial class UiRollout01Verification
    {
        private static IEnumerator VerifySettingsModal(MainMenuUI menu, NativeMouse mouse, ThemeMode theme)
        {
            var panel = Field<GameObject>(menu, "_settingsPanel");
            var settings = panel.GetComponent<SettingsUI>();
            Check(settings != null, "The revised Settings panel retains its original SettingsUI controller");
            Status = "Board02 Settings / " + theme;
            yield return Click(mouse, Field<Button>(menu, "_settingsButton"), true);
            var group = Field<CanvasGroup>(settings, "_canvasGroup");
            yield return (Func<bool>)(() => panel.activeInHierarchy && group.alpha >= .99f && group.interactable);
            Check(Field<GameObject>(menu, "_activeSubPanel") == panel, "Settings retains native MainMenu panel ownership");
            string[] toggles = { "_sfxToggle", "_vfxToggle", "_vibrationToggle" };
            for (int i = 0; i < toggles.Length; i++)
            {
                var toggle = Field<SettingsToggleUI>(settings, toggles[i]);
                bool original = SettingValue(i);
                Check(toggle != null && toggle.IsOn == original, "Settings switch loads its saved state / " + toggles[i]);
                yield return Click(mouse, Field<Button>(toggle, "_button"), true);
                Check(toggle.IsOn == !original && SettingValue(i) == !original, "Native switch changes its real saved setting / " + toggles[i]);
                VerifySwitch(toggle, "Changed " + toggles[i]);
                yield return Click(mouse, Field<Button>(toggle, "_button"), true);
                Check(toggle.IsOn == original && SettingValue(i) == original, "Native switch roundtrip restores its real setting / " + toggles[i]);
                VerifySwitch(toggle, "Restored " + toggles[i]);
            }
            var manager = LocalizationManager.Instance;
            string language = manager.CurrentLanguage;
            int initial = manager.GetCurrentLanguageIndex(), total = manager.AvailableLanguages.Length;
            yield return Click(mouse, Field<Button>(settings, "_nextLanguageButton"), true);
            Check(manager.GetCurrentLanguageIndex() == (initial + 1) % total, "Native language Next selects the next supported language");
            VerifyLanguageName(settings);
            yield return Click(mouse, Field<Button>(settings, "_prevLanguageButton"), true);
            Check(manager.CurrentLanguage == language, "Native language Previous returns to the original language");
            VerifyLanguageName(settings);
            var light = Field<Button>(settings, "_lightThemeButton");
            var dark = Field<Button>(settings, "_darkThemeButton");
            Check(light != null && dark != null, "Settings binds two independent direct theme choices");
            yield return Click(mouse, theme == ThemeMode.Light ? dark : light, true);
            VerifyThemeSelection(settings, theme == ThemeMode.Light ? ThemeMode.Dark : ThemeMode.Light);
            yield return Click(mouse, theme == ThemeMode.Light ? light : dark, true);
            VerifyThemeSelection(settings, theme);
            VerifyModalCopy(panel.transform, Status);
            yield return Capture("settings-" + theme.ToString().ToLowerInvariant());
            foreach (var entry in manager.AvailableLanguages)
            {
                manager.SetLanguage(entry.code, false); yield return Settled(.04f);
                VerifyLanguageName(settings);
                foreach (string field in toggles) VerifySwitch(Field<SettingsToggleUI>(settings, field), entry.code + "/" + field);
                VerifyModalCopy(panel.transform, "Settings / " + entry.code + " / " + theme);
            }
            manager.SetLanguage(language, false);
            var close = Field<Button>(settings, "_closeButton");
            yield return Click(mouse, close, true);
            yield return SettingsReturnedHome(menu, panel);
            Check(true, "Native Settings close returns through existing MainMenu wiring / " + theme);
            yield return Click(mouse, Field<Button>(menu, "_settingsButton"), true);
            yield return (Func<bool>)(() => panel.activeInHierarchy && group.alpha >= .99f && group.interactable);
            Button back = null;
            foreach (var button in panel.GetComponentsInChildren<Button>(true))
                if (button.gameObject.activeInHierarchy && button.name.IndexOf("back", StringComparison.OrdinalIgnoreCase) >= 0)
                { back = button; break; }
            Check(back != null, "The revised Settings header retains a separate Back control");
            yield return Click(mouse, back, true);
            yield return SettingsReturnedHome(menu, panel);
            Check(true, "Native Settings Back also returns through existing MainMenu wiring / " + theme);
            VerifyHomeBindings(menu);
        }

        private static Func<bool> SettingsReturnedHome(MainMenuUI menu, GameObject panel)
        {
            return () => !panel.activeInHierarchy && Field<GameObject>(menu, "_mainPanel").activeInHierarchy &&
                Field<GameObject>(menu, "_activeSubPanel") == null;
        }

        private static bool SettingValue(int index)
        {
            var data = DataManager.Instance.PlayerData;
            return index == 0 ? data.sfxEnabled : index == 1 ? data.vfxEnabled : data.vibrationEnabled;
        }

        private static void VerifySwitch(SettingsToggleUI toggle, string label)
        {
            var text = Field<TextMeshProUGUI>(toggle, "_statusText");
            string key = toggle.IsOn ? "settings_on" : "settings_off";
            Check(text != null && Visible(text) && text.text == LocalizationManager.Instance.GetText(key),
                "Switch status is correctly localized live TMP / " + label);
            var knob = Field<RectTransform>(toggle, "_knobRect");
            float expected = Field<float>(toggle, toggle.IsOn ? "_onKnobX" : "_offKnobX");
            Check(knob != null && Mathf.Abs(knob.anchoredPosition.x - expected) <= .6f,
                "Switch knob settles at the authored state position / " + label);
        }

        private static void VerifyLanguageName(SettingsUI settings)
        {
            var manager = LocalizationManager.Instance;
            var label = Field<TextMeshProUGUI>(settings, "_languageText");
            Check(label != null && label.text == manager.AvailableLanguages[manager.GetCurrentLanguageIndex()].nativeName.ToUpper(),
                "Settings displays the actual supported language's native name as TMP");
        }

        private static void VerifyThemeSelection(SettingsUI settings, ThemeMode expected)
        {
            Check(DataManager.Instance.PlayerData.theme == expected, "Native theme button applies its intended real theme / " + expected);
            var highlights = Field<GameObject[]>(settings, "_themeHighlights");
            Check(highlights != null && highlights.Length == 2, "Both theme buttons have authored selection highlights");
            int selected = expected == ThemeMode.Dark ? 1 : 0;
            for (int i = 0; i < highlights.Length; i++)
            {
                Check(highlights[i] != null && highlights[i].activeSelf == (i == selected),
                    "Only the current theme has a visible selection highlight / " + i);
                if (i == selected) Check(Visible(highlights[i].GetComponent<Image>()), "Current theme outline is actually visible");
            }
        }
    }
}
