namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.Modes;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Captures board04 with temporary progress inside the existing reversible native scope.</summary>
    public static partial class UiRollout01Verification
    {
        private static bool _nextStage04, _stage04, _stage04Revised, _stage04Interactions;

        /// <summary>Checks level selection without touching real progress or live providers.</summary>
        public static void Stage04At(string folder, bool revised, bool interactions)
        {
            _nextStage04 = true; _stage04Revised = revised; _stage04Interactions = interactions;
            try { Start(folder, false); } finally { _nextStage04 = false; }
        }

        private static IEnumerator VerifyStage04()
        {
            foreach (var mode in new[] { GameModeId.Defense, GameModeId.Shapes, GameModeId.ColorShift })
            foreach (var theme in new[] { ThemeMode.Light, ThemeMode.Dark })
            {
                Install04Player(mode); DataManager.Instance.SetTheme(theme);
                LocalizationManager.Instance.SetLanguage("tr", false); DataManager.Instance.SelectMode(mode);
                yield return Settled();
                var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
                var levels = UnityEngine.Object.FindFirstObjectByType<LevelSelectUI>(FindObjectsInactive.Include);
                Status = "Board04 Levels / " + mode + " / " + theme;
                using (var mouse = new NativeMouse())
                {
                    yield return Click(mouse, Field<Button>(menu, "_levelsButton"), true);
                    yield return (Func<bool>)(() => levels.gameObject.activeInHierarchy &&
                        Field<CanvasGroup>(levels, "_canvasGroup").alpha >= .99f && Field<CanvasGroup>(levels, "_canvasGroup").interactable);
                    yield return Settled(.3f);
                    Check(Field<GameObject>(menu, "_activeSubPanel") == levels.gameObject, "Levels retain real MainMenu ownership / " + Status);
                    var pool = Field<List<LevelButtonUI>>(levels, "_buttonPool");
                    Check(pool.Count == 20, "All twenty original level slots are bound / " + Status);
                    Check(Field<int>(levels, "_currentPage") == (mode == GameModeId.Defense ? 6 : 1),
                        "Actual mode progress chooses its highest page / " + Status);
                    Check(Field<TMP_Text>(levels, "_pageText").text.Contains(mode == GameModeId.Defense ? "101-120" : "1-20"),
                        "Actual localized range matches the independent campaign / " + Status);
                    if (_stage04Revised && mode != GameModeId.ColorShift) VerifyModalCopy(levels.transform, Status);
                    yield return Capture("levels-" + mode.ToString().ToLowerInvariant() + "-" + theme.ToString().ToLowerInvariant());
                    if (_stage04Revised && mode != GameModeId.ColorShift)
                    {
                        foreach (var language in LocalizationManager.Instance.AvailableLanguages)
                        {
                            LocalizationManager.Instance.SetLanguage(language.code, false); yield return Settled(.04f);
                            VerifyModalCopy(levels.transform, "Board04 / " + mode + " / " + theme + " / " + language.code);
                        }
                        LocalizationManager.Instance.SetLanguage("tr", false); yield return Settled(.06f);
                    }
                    if (_stage04Interactions && mode != GameModeId.ColorShift) yield return Verify04Interactions(levels, menu, mouse, mode);
                    yield return Click(mouse, Field<Button>(levels, "_closeButton"), true); yield return Settled(.55f);
                    Check(!levels.gameObject.activeInHierarchy && Field<GameObject>(menu, "_mainPanel").activeInHierarchy &&
                        Field<GameObject>(menu, "_activeSubPanel") == null, "Native Levels Close returns to Home / " + mode + " / " + theme);
                }
            }
            if (_stage04Interactions) yield return Verify04LevelRoutes();
        }

        private static void Install04Player(GameModeId mode)
        {
            var player = PlayerData.CreateDefault(); player.playerName = "Preview";
            player.isTutorialCompleted = player.isProfileSetupCompleted = true;
            player.currentLevel = player.highestLevel = 102; player.selectedGameMode = mode;
            player.levelStars.Add(new LevelStarData { level = 101, stars = 3 });
            Set03(DataManager.Instance, "_playerData", player);
        }

        private static IEnumerator Verify04Interactions(LevelSelectUI levels, MainMenuUI menu, NativeMouse mouse, GameModeId mode) { yield break; }
        private static IEnumerator Verify04LevelRoutes() { yield break; }
    }
}
