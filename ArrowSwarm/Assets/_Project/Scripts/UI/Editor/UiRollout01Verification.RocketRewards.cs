namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using ArrowSwarm.Ads;
    using ArrowSwarm.Data;
    using ArrowSwarm.Puzzle;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>Checks retryable failure, exactly-once rewards and obsolete attempt/scene callbacks without live ads.</summary>
    public static partial class UiRollout01Verification
    {
        private static IEnumerator VerifyRocketRewards(NativeMouse pointer, PuzzleSession session, PuzzleHUD hud,
            PuzzleRocketPopupUI popup, DeferredRocketAds ads)
        {
            int lives = session.Lives, turns = session.Turns, remaining = session.Remaining;
            int hints = DataManager.Instance.PlayerData.tipCount, requests = ads.Calls;
            var level = session.Level;
            var watch = Field<Button>(popup, "_watchAdButton"); var close = Field<Button>(popup, "_closeButton");
            yield return ClickRocket(pointer, Field<Button>(hud, "_rocket"));
            yield return ClickRocket(pointer, watch);
            var failed = ads.Pending;
            Check(session.RocketAdPending && !watch.interactable && !close.interactable && ads.Calls == requests + 1,
                "Watch Ad requests exactly one fake ad and disables repeated input until completion");
            session.WatchRocketAd(); session.CloseRocketPrompt();
            Check(ads.Calls == requests + 1 && session.RocketPromptOpen && session.RocketAdPending,
                "Repeated public Watch/Close calls cannot duplicate or discard an owned pending request");
            failed(false); yield return Settled(.12f);
            Check(session.RocketPromptOpen && !session.RocketAdPending && session.RocketCharges == 0 &&
                watch.interactable && close.interactable, "Failed ad leaves the same zero-charge modal open and retryable");
            failed(true);
            Check(session.RocketCharges == 0, "Duplicate failure-to-success callback cannot grant a charge");
            yield return ClickRocket(pointer, watch);
            var earned = ads.Pending;
            failed(true);
            Check(session.RocketAdPending && session.RocketCharges == 0 && ads.Calls == requests + 2,
                "An older provider callback cannot complete the newer pending Rocket request");
            earned(true); yield return Settled(.3f); earned(true);
            Check(session.RocketCharges == 1 && !session.RocketAdPending && !session.RocketPromptOpen && !session.RocketArmed &&
                !Field<CanvasGroup>(popup, "_canvasGroup").blocksRaycasts,
                "Earned ad grants exactly one Rocket, closes the modal and releases its input blocking");
            Check(session.Lives == lives && session.Turns == turns && session.Remaining == remaining &&
                ReferenceEquals(session.Level, level) && DataManager.Instance.PlayerData.tipCount == hints,
                "Popup actions preserve arrows, lives, turns, level data and saved hint inventory");

            EmptyRocket(session);
            yield return ClickRocket(pointer, Field<Button>(hud, "_rocket"));
            yield return ClickRocket(pointer, watch);
            var stale = ads.Pending; int version = session.AttemptVersion;
            session.Retry(); yield return (Func<bool>)(() => session.Idle && session.AttemptVersion != version);
            int resetCharges = session.RocketCharges;
            stale(true); yield return Settled(.15f); stale(true);
            Check(session.RocketCharges == resetCharges && !session.RocketPromptOpen && !session.RocketAdPending &&
                !AdManager.Instance.IsShowingAd && ads.Calls == requests + 3,
                "Retry invalidates the old request; delayed and duplicate success cannot alter the new attempt");
        }

        private static IEnumerator VerifyRocketMenuCallback(NativeMouse pointer, PuzzleSession session, PuzzleHUD hud,
            PuzzleRocketPopupUI popup, DeferredRocketAds ads)
        {
            EmptyRocket(session);
            yield return ClickRocket(pointer, Field<Button>(hud, "_rocket"));
            yield return ClickRocket(pointer, Field<Button>(popup, "_watchAdButton"));
            var stale = ads.Pending; string player = JsonUtility.ToJson(DataManager.Instance.PlayerData);
            session.Menu();
            yield return (Func<bool>)(() => SceneManager.GetActiveScene().name == "MainMenuScene" &&
                !ArrowSwarm.Core.SceneTransitionManager.Instance.IsTransitioning &&
                UnityEngine.Object.FindFirstObjectByType<MainMenuUI>() != null);
            stale(true); yield return Settled(.12f); stale(true);
            Check(PuzzleSession.Active == null && !AdManager.Instance.IsShowingAd &&
                JsonUtility.ToJson(DataManager.Instance.PlayerData) == player,
                "Menu invalidates the old scene's Rocket request without touching current player data");
        }
    }
}
