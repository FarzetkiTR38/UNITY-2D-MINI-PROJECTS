namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Localization;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEngine;

    /// <summary>Checks actual HUD renderers after real legal shots and language-only refreshes.</summary>
    public static partial class Quality2LocalizationVerification
    {
        private static IEnumerator CheckDefenseHud()
        {
            var hud = UnityEngine.Object.FindFirstObjectByType<GameHUD>();
            Check(hud != null, "Actual GameScene HUD is present");
            var level = Field<TextMeshProUGUI>(hud, "_levelText");
            var counter = Field<TextMeshProUGUI>(hud, "_arrowCountText");
            var levelStyle = new TextStyle(level); var counterStyle = new TextStyle(counter);
            var feedback = Field<HUDFeedbackEffects>(hud, "_feedbackEffects");
            int lives = GameManager.Instance.CurrentLives, stars = DataManager.Instance.GetTotalStars();
            var localization = LocalizationManager.Instance;
            Check(localization.AvailableLanguages.Length == 10, "All ten configured languages are available");
            foreach (var language in localization.AvailableLanguages)
            {
                Status = "Defense HUD: " + language.code;
                int fired = LevelManager.Instance.ArrowsFired, total = LevelManager.Instance.TotalArrows;
                var punch = Field<Coroutine>(feedback, "_arrowPunchCoroutine");
                localization.SetLanguage(language.code, false);
                Check(ReferenceEquals(punch, Field<Coroutine>(feedback, "_arrowPunchCoroutine")),
                    language.code + ": language-only HUD refresh does not restart count feedback");
                Check(level.text == localization.GetText("menu_level_short") + 101,
                    language.code + ": actual Defense level label uses the existing translated prefix");
                Check(counter.text == localization.GetText("hud_arrows") + ": " + fired + "/" + total,
                    language.code + ": actual Defense arrow count translates without resetting its values");
                var arrow = FindLegalArrow(); Check(arrow != null, language.code + ": a real legal arrow remains available");
                arrow.OnPlayerClick();
                Check(arrow.IsFired && LevelManager.Instance.ArrowsFired == fired + 1,
                    language.code + ": real gameplay shot updates the level counter once");
                Check(counter.text == localization.GetText("hud_arrows") + ": " + (fired + 1) + "/" + total,
                    language.code + ": gameplay count update preserves the selected language");
                Check(levelStyle.Matches(level) && counterStyle.Matches(counter),
                    language.code + ": HUD fonts, colors, materials, size and authored bounds stay unchanged");
                RecordFit(level, language.code + "/Defense/Level");
                RecordFit(counter, language.code + "/Defense/Arrows");
                if (language.code == "ja")
                {
                    yield return CaptureVisual("defense-ja");
                }
                yield return .03f;
            }
            Check(GameManager.Instance.CurrentLives == lives && DataManager.Instance.GetTotalStars() == stars,
                "Language changes and legal-shot fixture do not alter lives or saved stars");
        }

        private static Arrow FindLegalArrow()
        {
            foreach (var arrow in ArrowSpawner.Instance.ActiveArrows)
                if (arrow != null && !arrow.IsFired && GridManager.Instance.IsPathClear(arrow.HeadPoint, arrow.HeadDirection))
                    return arrow;
            return null;
        }
    }
}
