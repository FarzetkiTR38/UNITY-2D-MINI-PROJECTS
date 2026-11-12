namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.Modes;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Checks real Home navigation and existing mode modal callbacks without redesigning later panels.</summary>
    public static partial class UiRollout01Verification
    {
        private static IEnumerator VerifyNavigation()
        {
            foreach (var theme in new[] { ThemeMode.Light, ThemeMode.Dark })
            {
                DataManager.Instance.SetTheme(theme); yield return Settled();
                var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
                var selector = UnityEngine.Object.FindFirstObjectByType<GameModeSelectUI>(FindObjectsInactive.Include);
                using (var mouse = new NativeMouse())
                {
                    for (int mode = 0; mode < 3; mode++)
                    {
                        Status = "Native mode card / " + theme + " / " + (GameModeId)mode;
                        yield return Click(mouse, Field<Button>(selector, "_open"), true);
                        var group = Field<CanvasGroup>(selector, "_group");
                        yield return (Func<bool>)(() => group.alpha >= .99f && group.interactable && group.blocksRaycasts);
                        yield return Click(mouse, Field<Button[]>(selector, "_choices")[mode]);
                        Check(GameModeSelection.Selected == (GameModeId)mode, "Actual mode-choice pointer callback selects " + Status);
                        string key = mode == 0 ? "mode_defense" : mode == 1 ? "mode_shapes" : "mode_color_shift";
                        Check(Field<TMP_Text>(selector, "_modeLabel").text == LocalizationManager.Instance.GetText(key), "Home mode label remains live localized TMP");
                        VerifyModeIcon(selector, mode, theme);
                        yield return Click(mouse, Field<Button>(selector, "_close"));
                        yield return (Func<bool>)(() => group.alpha <= .01f && !group.blocksRaycasts);
                        VerifyHomeBindings(menu);
                    }
                    foreach (string entry in new[] { "Levels", "Leaderboard", "Settings" })
                    {
                        Status = "Native Home navigation / " + theme + " / " + entry;
                        string prefix = "_" + char.ToLowerInvariant(entry[0]) + entry.Substring(1);
                        var panel = Field<GameObject>(menu, prefix + "Panel");
                        yield return Click(mouse, Field<Button>(menu, prefix + "Button"), true);
                        var group = panel.GetComponent<CanvasGroup>();
                        yield return (Func<bool>)(() => panel.activeInHierarchy && group.alpha >= .99f && group.interactable);
                        Check(Field<GameObject>(menu, "_activeSubPanel") == panel, "Existing subpanel ownership is preserved: " + entry);
                        Button close = null;
                        foreach (var candidate in panel.GetComponentsInChildren<Button>(true))
                            if (candidate.gameObject.activeInHierarchy && candidate.name.IndexOf("close", StringComparison.OrdinalIgnoreCase) >= 0) { close = candidate; break; }
                        yield return Click(mouse, close);
                        yield return (Func<bool>)(() => Field<GameObject>(menu, "_mainPanel").activeInHierarchy && Field<GameObject>(menu, "_activeSubPanel") == null);
                    }
                    Button profileButton = null;
                    foreach (var button in Field<GameObject>(menu, "_mainPanel").GetComponentsInChildren<Button>(true))
                        if (button.gameObject.activeInHierarchy && button.name.IndexOf("profile", StringComparison.OrdinalIgnoreCase) >= 0) { profileButton = button; break; }
                    yield return Click(mouse, profileButton, true);
                    var profile = UnityEngine.Object.FindFirstObjectByType<ProfileSetupModalUI>();
                    yield return (Func<bool>)(() => profile != null && Field<CanvasGroup>(profile, "_canvasGroup").alpha >= .99f);
                    Check(profile.gameObject.activeInHierarchy, "New Home profile control opens the existing profile dialog");
                    // The preserved profile panel uses its existing Skip action instead of an X.
                    var dismiss = Field<Button>(profile, "_closeButton") ?? Field<Button>(profile, "_skipButton");
                    yield return Click(mouse, dismiss, legacyTextAction: dismiss != null && dismiss.targetGraphic == null);
                    yield return (Func<bool>)(() => !profile.gameObject.activeInHierarchy);
                }
            }
        }

        private static void VerifyModeIcon(GameModeSelectUI selector, int mode, ThemeMode theme)
        {
            var open = Field<Button>(selector, "_open");
            foreach (var visual in open.GetComponentsInChildren<ModeButtonVisual>(true))
            {
                if (!visual.isActiveAndEnabled) continue;
                var target = Field<Image>(visual, "_target");
                var variants = Field<Sprite[]>(visual, theme == ThemeMode.Dark ? "_darkModes" : "_lightModes");
                Check(Visible(target) && variants.Length == 3 && target.sprite == variants[mode], "Selected mode uses its authored reusable icon");
                return;
            }
            var icons = Field<Image[]>(selector, "_menuModeIcons"); int shown = 0;
            for (int i = 0; icons != null && i < icons.Length; i++)
                if (Visible(icons[i])) { shown++; Check(i == mode, "Only the chosen authored mode icon is visible"); }
            Check(shown == 1, "The Home card displays exactly one selected mode icon");
        }
    }
}
