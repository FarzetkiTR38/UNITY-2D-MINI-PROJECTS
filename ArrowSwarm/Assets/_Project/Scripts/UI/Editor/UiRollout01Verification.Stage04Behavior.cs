namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.Modes;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Exercises real paging and locking without leaving the existing reversible data scope.</summary>
    public static partial class UiRollout01Verification
    {
        private static IEnumerator Verify04Interactions(LevelSelectUI levels, MainMenuUI menu, NativeMouse mouse, GameModeId mode)
        {
            Check(GameModeSelection.Selected == mode, "Levels interaction context uses its actual selected mode / " + mode);
            var player = DataManager.Instance.PlayerData;
            string original = JsonUtility.ToJson(player), language = LocalizationManager.Instance.CurrentLanguage;
            int highest = mode == GameModeId.Defense ? 102 : 50;
            try
            {
                if (mode == GameModeId.Shapes)
                {
                    Verify04Page(levels, mode);
                    var firstPool = Field<List<LevelButtonUI>>(levels, "_buttonPool");
                    yield return Press04Disabled(mouse, Field<Button>(firstPool[1], "_button"), "Locked Shapes level2");
                }
                Apply04Progress(player, mode, highest, highest);
                levels.Show(); yield return Settled(.65f);
                int last = (highest - 1) / LevelSelectUI.LEVELS_PER_PAGE + 1;
                var pool = Field<List<LevelButtonUI>>(levels, "_buttonPool");
                var identity = pool.ToArray();
                Check(identity.Length == 20 && identity.All(item => item != null), "Levels reuse exactly twenty authored pooled buttons / " + mode);
                Check(Field<int>(levels, "_currentPage") == last, "Opening starts on the reached level page / " + mode);
                string prepared = JsonUtility.ToJson(player);
                Verify04Page(levels, mode);
                yield return Press04Disabled(mouse, Field<Button>(levels, "_nextPageButton"), "Highest-page Next");
                while (Field<int>(levels, "_currentPage") > 1)
                {
                    int before = Field<int>(levels, "_currentPage");
                    yield return Click(mouse, Field<Button>(levels, "_prevPageButton"), true);
                    Check(Field<int>(levels, "_currentPage") == before - 1, "Actual Previous moves exactly one page / " + mode);
                    Verify04Page(levels, mode);
                }
                yield return Press04Disabled(mouse, Field<Button>(levels, "_prevPageButton"), "First-page Previous");
                foreach (var item in LocalizationManager.Instance.AvailableLanguages)
                {
                    int before = Field<int>(levels, "_currentPage");
                    LocalizationManager.Instance.SetLanguage(item.code, false); yield return Settled(.04f);
                    Check(Field<int>(levels, "_currentPage") == before, "Live language retains the browsed page / " + mode + " / " + item.code);
                    Verify04Page(levels, mode); VerifyModalCopy(levels.transform, "Levels / " + mode + " / " + item.code);
                }
                LocalizationManager.Instance.SetLanguage(language, false); yield return Settled(.06f);
                while (Field<int>(levels, "_currentPage") < last)
                {
                    int before = Field<int>(levels, "_currentPage");
                    yield return Click(mouse, Field<Button>(levels, "_nextPageButton"), true);
                    Check(Field<int>(levels, "_currentPage") == before + 1, "Actual Next moves exactly one page / " + mode);
                    Verify04Page(levels, mode);
                }
                if (mode == GameModeId.Defense)
                    yield return Press04Disabled(mouse, Field<Button>(pool[2], "_button"), "Locked level103");
                else
                {
                    Check(Field<TMP_Text>(levels, "_pageText").text.EndsWith("41-50"), "Finite Shapes range ends at50");
                    Check(pool.Count(item => item.gameObject.activeSelf) == 10 && pool.Skip(10).All(item => !item.gameObject.activeSelf),
                        "Shapes final page shows41 through50 and hides51 through60");
                }
                Check(identity.SequenceEqual(pool), "Paging and language changes reuse the same twenty object identities / " + mode);
                Check(JsonUtility.ToJson(player) == prepared, "Paging, locked input and languages preserve all prepared progress and inventory / " + mode);
            }
            finally
            {
                JsonUtility.FromJsonOverwrite(original, player);
                LocalizationManager.Instance.SetLanguage(language, false);
                if (levels != null && levels.gameObject.activeInHierarchy) levels.Show();
            }
            yield return Settled(.65f);
            Check(ReferenceEquals(player, DataManager.Instance.PlayerData) && JsonUtility.ToJson(player) == original,
                "Levels behavior restores the exact outer cloned player / " + mode);
        }

        private static void Apply04Progress(PlayerData player, GameModeId mode, int highest, int current)
        {
            var stars = new List<LevelStarData> { new LevelStarData { level = 1, stars = 3 },
                new LevelStarData { level = 2, stars = 2 }, new LevelStarData { level = 3, stars = 1 } };
            if (mode == GameModeId.Defense)
            {
                stars.Add(new LevelStarData { level = 101, stars = 3 });
                player.highestLevel = highest; player.currentLevel = current; player.levelStars = stars;
            }
            else
            {
                stars.RemoveAll(item => item.level >= highest);
                var progress = player.GetModeProgress(mode);
                Set03(progress, "_highestLevel", highest); Set03(progress, "_currentLevel", current); Set03(progress, "_stars", stars);
            }
        }

        private static void Verify04Page(LevelSelectUI levels, GameModeId mode)
        {
            var pool = Field<List<LevelButtonUI>>(levels, "_buttonPool");
            int page = Field<int>(levels, "_currentPage"), first = (page - 1) * 20 + 1;
            int count = mode == GameModeId.Defense ? int.MaxValue : Field<PuzzleCatalog>(levels, "_shapesCatalog").Count;
            int highest = DataManager.Instance.GetHighestLevel(mode);
            Check(Field<TMP_Text>(levels, "_pageText").text == LocalizationManager.Instance.GetText("menu_levels") + " " + first + "-" + Mathf.Min(page * 20, count),
                "Range label is localized and matches the current bounded page / " + mode);
            for (int i = 0; i < pool.Count; i++)
            {
                var tile = pool[i]; int level = first + i; bool valid = level <= count, unlocked = level <= highest;
                Check(tile.gameObject.activeSelf == valid && Field<int>(tile, "_level") == level, "Pooled slot binds the exact level and campaign bound / " + mode + " / " + level);
                if (!valid) continue;
                var number = Field<TMP_Text>(tile, unlocked ? "_levelText" : "_lockedLevelText");
                Check(number != null && Visible(number) && number.text == level.ToString(), "Both available and locked levels retain a live visible number / " + level);
                Check(Field<Button>(tile, "_button").interactable == unlocked && Field<GameObject>(tile, "_lockIcon").activeInHierarchy == !unlocked,
                    "Button interactivity and lock match mode progress / " + level);
                Check(Field<GameObject>(tile, "_unlockedState").activeSelf == unlocked && Field<GameObject>(tile, "_lockedState").activeSelf == !unlocked,
                    "Only the matching authored tile state is active / " + level);
                Check(Field<GameObject>(tile, "_starsContainer").activeInHierarchy == unlocked, "Star slots appear only on available levels / " + level);
                if (!unlocked) continue;
                int earned = DataManager.Instance.GetLevelStars(mode, level);
                var gold = Field<GameObject[]>(tile, "_earnedStarStates"); var empty = Field<GameObject[]>(tile, "_emptyStarStates");
                Check(gold.Length == 3 && empty.Length == 3, "Available level owns exactly three authored star pairs / " + level);
                for (int star = 0; star < 3; star++)
                    Check(gold[star].activeInHierarchy == (star < earned) && empty[star].activeInHierarchy == (star >= earned),
                        "Only the correct earned or empty star state renders / " + level + " / " + star);
            }
        }
    }
}
