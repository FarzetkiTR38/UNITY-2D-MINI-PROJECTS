namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using ArrowSwarm.Ads;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Puzzle;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>Exercises exhausted Rockets in both real puzzle campaigns using scoped native input and fake ads.</summary>
    public static partial class UiRollout01Verification
    {
        private static bool _nextRocketStage, _rocketStage;

        /// <summary>Runs from fresh Boot-loaded MainMenu; captures the current Game View size and restores private state.</summary>
        public static void RocketAt(string folder)
        {
            if (!AdManager.HasInstance) throw new InvalidOperationException("Resolve the Boot AdManager before starting the scoped Rocket fixture.");
            _nextRocketStage = true;
            try { Start(folder, false); } finally { _nextRocketStage = false; }
        }

        private static IEnumerator VerifyRocketStage()
        {
            using (var ads = new RocketAdsScope())
            {
            foreach (var mode in new[] { GameModeId.Shapes, GameModeId.ColorShift })
            {
                DataManager.Instance.SelectMode(mode); DataManager.Instance.SetCurrentLevel(mode, 1);
                LocalizationManager.Instance.SetLanguage("tr", false); yield return Settled();
                var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
                using (var pointer = new NativeMouse()) yield return Click(pointer, Field<Button>(menu, "_playButton"), true);
                string scene = mode == GameModeId.Shapes ? "ShapesScene" : "ColorShiftScene";
                yield return (Func<bool>)(() => SceneManager.GetActiveScene().name == scene &&
                    !SceneTransitionManager.Instance.IsTransitioning && PuzzleSession.Active != null && PuzzleSession.Active.Idle);
                var session = PuzzleSession.Active;
                var hud = UnityEngine.Object.FindFirstObjectByType<PuzzleHUD>();
                var popup = UnityEngine.Object.FindFirstObjectByType<PuzzleRocketPopupUI>(FindObjectsInactive.Include);
                Check(!session.Preview && session.LevelNumber == 1 && session.Mode == mode && popup != null,
                    "Actual Play reaches the chosen campaign and its authored Rocket modal / " + mode);
                using (var pointer = new NativeMouse())
                {
                    foreach (var theme in new[] { ThemeMode.Light, ThemeMode.Dark })
                    {
                        DataManager.Instance.SetTheme(theme); LocalizationManager.Instance.SetLanguage("tr", false);
                        yield return Settled();
                        Status = "Rocket / " + mode + " / " + theme;
                        if (theme == ThemeMode.Light) yield return SpendLastRocket(pointer, session, hud);
                        else EmptyRocket(session);
                        yield return ClickRocket(pointer, Field<Button>(hud, "_rocket"));
                        yield return Settled(.4f);
                        Check(session.RocketPromptOpen && !session.Ready && !session.Idle &&
                            Field<CanvasGroup>(popup, "_canvasGroup").blocksRaycasts,
                            "Native zero-charge HUD input opens the modal and blocks gameplay / " + mode);
                        Check(!Field<Button>(hud, "_hint").interactable && !Field<Button>(hud, "_rocket").interactable &&
                            !Field<Button>(hud, "_pause").interactable,
                            "Open Rocket prompt disables Hint, Rocket and Pause HUD controls");
                        VerifyRocketCopy(popup);
                        yield return Capture("rocket-" + mode.ToString().ToLowerInvariant() + "-" + theme.ToString().ToLowerInvariant());
                        foreach (var language in LocalizationManager.Instance.AvailableLanguages)
                        {
                            LocalizationManager.Instance.SetLanguage(language.code, false); yield return Settled(.04f);
                            VerifyRocketCopy(popup);
                        }
                        LocalizationManager.Instance.SetLanguage("tr", false); yield return Settled(.08f);
                        int requests = ads.Service.Calls;
                        yield return ClickRocket(pointer, Field<Button>(popup, "_closeButton"));
                        Check(!session.RocketPromptOpen && session.RocketCharges == 0 && ads.Service.Calls == requests,
                            "Native Close dismisses the exhausted prompt without charge or ad cost");
                        yield return VerifyRocketRewards(pointer, session, hud, popup, ads.Service);
                    }
                    yield return VerifyRocketMenuCallback(pointer, session, hud, popup, ads.Service);
                }
            }
            Check(ads.Service.Calls == 14 && ads.Service.Interstitials == 0,
                "Exactly fourteen owned fake rewarded requests and no interstitials were used across both modes");
            }
        }

        private static IEnumerator SpendLastRocket(NativeMouse pointer, PuzzleSession session, PuzzleHUD hud)
        {
            Check(session.RocketCharges == 1 && session.Remaining > 1, "Campaign starts with one real Rocket and a playable board / " + session.Mode);
            var removed = Field<bool[]>(session, "_removed");
            int index = -1;
            for (int candidate = 0; candidate < removed.Length; candidate++)
            {
                if (!PuzzleRules.CanRemove(session.Level, removed, candidate, session.Orientation)) continue;
                index = candidate;
                break;
            }
            Check(index >= 0, "Campaign has a legal arrow for the final Rocket / " + session.Mode);
            int arrows = session.Remaining;
            int lives = session.Lives;
            int turns = session.Turns;
            yield return ClickRocket(pointer, Field<Button>(hud, "_rocket"));
            Check(session.RocketArmed && session.RocketCharges == 1, "Native HUD arms the available Rocket without spending it");
            session.FireRocket(index);
            yield return (Func<bool>)(() => session.Idle || session.Won || session.Lost);
            Check(session.Idle && session.RocketCharges == 0 && !session.RocketArmed && removed[index] &&
                session.Remaining == arrows - 1 && session.Lives == lives && session.Turns == turns,
                "Actual Rocket extraction exhausts the charge and removes one legal arrow / " + session.Mode);
        }

        private static void EmptyRocket(PuzzleSession session)
        {
            typeof(PuzzleSession).GetProperty("RocketCharges").SetValue(session, 0, null);
            typeof(PuzzleSession).GetMethod("Notify", System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.NonPublic).Invoke(null, null);
            Results.Add("FIXTURE: only the current cloned attempt's Rocket count was exhausted; no config or saved inventory edited");
        }

        private sealed class RocketAdsScope : IDisposable
        {
            internal readonly DeferredRocketAds Service = new DeferredRocketAds();
            private readonly AdManager _manager;
            private readonly IAdService _previous;
            internal RocketAdsScope()
            {
                _manager = AdManager.Instance; _previous = Field<IAdService>(_manager, "_adService");
                Check(ReferenceEquals(_previous, _scope.Ads), "Rocket fixture starts on the outer scope's isolated ad provider");
                _manager.SetAdService(Service);
            }
            public void Dispose()
            {
                if (_manager != null) _manager.SetAdService(_previous);
                Check(_manager == null || ReferenceEquals(Field<IAdService>(_manager, "_adService"), _previous),
                    "Owned deferred ad provider restores the exact outer scoped provider before final cleanup");
            }
        }

        private sealed class DeferredRocketAds : IAdService
        {
            internal int Calls, Interstitials;
            internal Action<bool> Pending;
            public bool IsAdReady() => true;
            public bool IsInterstitialAdReady() => false;
            public void ShowRewardedAd(Action<bool> callback) { Calls++; Pending = callback; }
            public void ShowInterstitialAd(Action callback) { Interstitials++; throw new InvalidOperationException("Unexpected Rocket fixture interstitial"); }
        }
    }
}
