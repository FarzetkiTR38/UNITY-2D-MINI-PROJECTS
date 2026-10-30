namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using ArrowSwarm.Localization;
    using ArrowSwarm.Modes;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Checks every mode through real card clicks, authored highlights and translated status labels.</summary>
    public static partial class UiRollout01Verification
    {
        private static IEnumerator VerifyModeModal(MainMenuUI menu, GameModeSelectUI selector, NativeMouse mouse, ArrowSwarm.Data.ThemeMode theme)
        {
            Status = "Board02 modes / " + theme;
            yield return Click(mouse, Field<Button>(selector, "_open"), true);
            var group = Field<CanvasGroup>(selector, "_group");
            yield return (Func<bool>)(() => group.alpha >= .99f && group.interactable && group.blocksRaycasts);
            yield return Settled(.08f);
            var choices = Field<Button[]>(selector, "_choices");
            var labels = Field<TMP_Text[]>(selector, "_choiceLabels");
            var highlights = Field<GameObject[]>(selector, "_selectionHighlights");
            Check(choices != null && choices.Length == 3 && labels != null && labels.Length == 3 &&
                highlights != null && highlights.Length == 3, "Board02 binds all three real mode cards and selection states");
            for (int mode = 0; mode < 3; mode++)
            {
                Status = "Board02 mode pointer / " + theme + " / " + (GameModeId)mode;
                LocalizationManager.Instance.SetLanguage("tr", false);
                yield return Click(mouse, choices[mode], true);
                Check(GameModeSelection.Selected == (GameModeId)mode, "Native card click selects the intended campaign / " + Status);
                VerifyModeSelection(selector, highlights, labels, mode, theme);
                VerifyModalCopy(group.transform, Status);
                if (mode == 0) yield return Capture("mode-selection-" + theme.ToString().ToLowerInvariant());
                foreach (var language in LocalizationManager.Instance.AvailableLanguages)
                {
                    LocalizationManager.Instance.SetLanguage(language.code, false);
                    yield return Settled(.04f);
                    VerifyModeSelection(selector, highlights, labels, mode, theme);
                    VerifyModalCopy(group.transform, "Modes / " + language.code + " / " + theme + " / " + (GameModeId)mode);
                }
            }
            LocalizationManager.Instance.SetLanguage("tr", false);
            yield return Click(mouse, Field<Button>(selector, "_close"), true);
            yield return (Func<bool>)(() => group.alpha <= .01f && !group.blocksRaycasts);
            Check(Field<GameObject>(menu, "_mainPanel").activeInHierarchy && Field<GameObject>(menu, "_activeSubPanel") == null,
                "Mode close returns to the same active Home without starting gameplay / " + theme);
            VerifyHomeBindings(menu);
        }

        private static void VerifyModeSelection(GameModeSelectUI selector, GameObject[] highlights, TMP_Text[] labels,
            int selected, ArrowSwarm.Data.ThemeMode theme)
        {
            int active = 0;
            for (int i = 0; i < 3; i++)
            {
                Check(highlights[i] != null && labels[i] != null, "Each mode has authored selection chrome and a TMP action label");
                if (highlights[i].activeSelf) active++;
                Check(highlights[i].activeSelf == (i == selected), "Only the selected mode's authored highlight is active / " + i);
                if (i == selected) Check(Visible(highlights[i].GetComponentInChildren<Image>()), "Selected mode outline is actually visible");
                string key = i == selected ? "modes_selected" : "modes_select";
                Check(labels[i].text == LocalizationManager.Instance.GetText(key), "Mode action state remains correctly localized TMP / " + i);
            }
            Check(active == 1, "Exactly one mode is visibly selected");
            VerifyModeIcon(selector, selected, theme);
        }
    }
}
