namespace ArrowSwarm.Modes.Editor
{
    using System.Collections;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Exercises real pagination controls and finite campaign edges in every selected language.</summary>
    public static partial class Quality2LocalizationVerification
    {
        private static IEnumerator CheckLevels()
        {
            var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
            var levels = UnityEngine.Object.FindFirstObjectByType<LevelSelectUI>(FindObjectsInactive.Include);
            var data = DataManager.Instance; data.PlayerData.highestLevel = 45;
            foreach (var mode in new[] { GameModeId.Shapes, GameModeId.ColorShift })
                JsonUtility.FromJsonOverwrite("{\"_highestLevel\":50,\"_currentLevel\":50}", data.PlayerData.GetModeProgress(mode));
            var localization = LocalizationManager.Instance;
            foreach (var mode in new[] { GameModeId.Defense, GameModeId.Shapes, GameModeId.ColorShift })
            {
                data.SelectMode(mode); menu.OpenLevels(); yield return .25f;
                var label = Field<TextMeshProUGUI>(levels, "_pageText"); var style = new TextStyle(label);
                if (mode == GameModeId.Defense) CheckTextStyleNegativeControls(label);
                var previous = Field<Button>(levels, "_prevPageButton"); var next = Field<Button>(levels, "_nextPageButton");
                Check(Field<int>(levels, "_currentPage") == 3, mode + ": authored Levels opens on the independent highest page");
                foreach (var language in localization.AvailableLanguages)
                {
                    Status = "Level ranges: " + mode + "/" + language.code;
                    previous.onClick.Invoke();
                    Check(Field<int>(levels, "_currentPage") == 2, mode + "/" + language.code + ": actual Previous button moves to page two");
                    localization.SetLanguage(language.code, false);
                    Check(Field<int>(levels, "_currentPage") == 2 && label.text == localization.GetText("menu_levels") + " 21-40",
                        mode + "/" + language.code + ": language change translates the range without resetting pagination");
                    next.onClick.Invoke();
                    int last = mode == GameModeId.Defense ? 60 : 50;
                    Check(Field<int>(levels, "_currentPage") == 3 && label.text == localization.GetText("menu_levels") + " 41-" + last,
                        mode + "/" + language.code + ": actual Next button shows the correct independent campaign edge");
                    Check(previous.interactable && !next.interactable, mode + "/" + language.code + ": last-page navigation stays correct");
                    var grid = Field<Transform>(levels, "_levelGridContainer"); int active = 0;
                    foreach (var button in grid.GetComponentsInChildren<LevelButtonUI>(true)) if (button.gameObject.activeSelf) active++;
                    Check(active == (mode == GameModeId.Defense ? 20 : 10),
                        mode + "/" + language.code + ": catalog ends keep the correct number of authored level controls visible");
                    Check(style.Matches(label), mode + "/" + language.code + ": localized range retains Inspector-owned text styling");
                    RecordFit(label, mode + "/" + language.code + "/Levels/LastPage");
                    if (mode == GameModeId.Defense && language.code == "tr")
                    {
                        yield return CaptureVisual("levels-tr");
                    }
                    yield return .03f;
                }
                menu.OpenMainPanel(); yield return .25f;
            }
        }
    }
}
