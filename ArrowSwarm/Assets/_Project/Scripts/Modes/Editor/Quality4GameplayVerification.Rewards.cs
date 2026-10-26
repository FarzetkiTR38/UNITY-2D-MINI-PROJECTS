namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using ArrowSwarm.Ads;
    using ArrowSwarm.Data;
    using ArrowSwarm.Puzzle;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public static partial class Quality4GameplayVerification
    {
        private static IEnumerator PuzzleRewards()
        {
            var fake = new DelayedPuzzleAds(); AdManager.Instance.SetAdService(fake);
            foreach (var mode in new[] { GameModeId.Shapes, GameModeId.ColorShift })
            {
                yield return OpenPuzzle(mode);
                var session = PuzzleSession.Active;
                Status = mode + ": live reward, duplicates and failure controls";
                Action<bool> callback = RequestHint(session, fake);
                int changes = 0; Action changed = () => changes++;
                PuzzleSession.OnChanged += changed;
                try { callback(true); callback(true); }
                finally { PuzzleSession.OnChanged -= changed; }
                Check(DataManager.Instance.PlayerData.tipCount == 1 && changes == 1 && !AdManager.Instance.IsShowingAd,
                    mode + ": live successful reward adds exactly one Hint and notifies once despite duplicate provider completion");
                callback = RequestHint(session, fake); string message = session.Message;
                callback(false); callback(true);
                Check(DataManager.Instance.PlayerData.tipCount == 0 && session.Message == message && !AdManager.Instance.IsShowingAd,
                    mode + ": failed request grants nothing and obsolete success is rejected");

                Status = mode + ": earned reward after actual Retry";
                callback = RequestHint(session, fake); int version = session.AttemptVersion;
                session.Retry(); message = session.Message; changes = 0;
                Require(session.AttemptVersion != version && session.Ready, mode + ": real Retry started a fresh attempt");
                PuzzleSession.OnChanged += changed;
                try { callback(true); callback(true); }
                finally { PuzzleSession.OnChanged -= changed; }
                Check(DataManager.Instance.PlayerData.tipCount == 1,
                    mode + ": earned reward after Retry remains persistent inventory exactly once");
                Check(changes == 0 && session.Message == message && session.Ready,
                    mode + ": delayed previous-attempt reward cannot message or alter the retried board");

                Status = mode + ": earned reward after actual Menu before scene destruction";
                callback = RequestHint(session, fake); version = session.AttemptVersion;
                session.Menu(); message = session.Message; changes = 0;
                Require(session != null && session.AttemptVersion != version, mode + ": real Menu invalidates its still-live session");
                PuzzleSession.OnChanged += changed;
                try { callback(true); callback(true); }
                finally { PuzzleSession.OnChanged -= changed; }
                Check(DataManager.Instance.PlayerData.tipCount == 1,
                    mode + ": earned reward during Menu transition remains inventory exactly once");
                Check(changes == 0 && session.Message == message,
                    mode + ": departing session does not notify or display delayed reward text");
                yield return (Func<bool>)MenuReady;

                yield return OpenPuzzle(mode); session = PuzzleSession.Active;
                Status = mode + ": earned reward after real scene-owned session destruction";
                callback = RequestHint(session, fake);
                SceneManager.LoadScene("MainMenuScene"); yield return NextFrames(2);
                Require(session == null && PuzzleSession.Active == null, mode + ": unloading the actual scene destroys its session");
                changes = 0; PuzzleSession.OnChanged += changed;
                try { callback(true); callback(true); }
                finally { PuzzleSession.OnChanged -= changed; }
                Check(DataManager.Instance.PlayerData.tipCount == 1 && changes == 0 && !AdManager.Instance.IsShowingAd,
                    mode + ": destroyed session keeps earned Hint exactly once without touching old UI");
            }
        }

        private static IEnumerator OpenPuzzle(GameModeId mode)
        {
            PuzzleLaunch.Preview = false;
            DataManager.Instance.SetCurrentLevel(mode, 1);
            SceneManager.LoadScene(mode == GameModeId.Shapes ? "ShapesScene" : "ColorShiftScene");
            yield return (Func<bool>)(() => PuzzleSession.Active != null && PuzzleSession.Active.Mode == mode && PuzzleSession.Active.Ready);
            Require(!PuzzleSession.Active.Preview && PuzzleSession.Active.LevelNumber == 1,
                mode + ": authored scene, normal session and actual level-one board are ready");
        }

        private static Action<bool> RequestHint(PuzzleSession session, DelayedPuzzleAds fake)
        {
            DataManager.Instance.PlayerData.tipCount = 0;
            int requests = fake.Requests; session.Hint();
            Require(session.HintPromptOpen, session.Mode + ": actual zero-inventory Hint opens the authored popup state");
            session.WatchHintAd();
            Require(!session.HintPromptOpen && AdManager.Instance.IsShowingAd && fake.Requests == requests + 1 && fake.Pending != null,
                session.Mode + ": actual WatchHintAd owns one delayed fake-provider request");
            return fake.Pending;
        }

        /// <summary>Accepts real AdManager requests while exposing deterministic completion without ads or network use.</summary>
        private sealed class DelayedPuzzleAds : IAdService
        {
            /// <summary>Completion supplied by the real manager for the latest request.</summary>
            public Action<bool> Pending { get; private set; }
            /// <summary>Number of accepted reward requests.</summary>
            public int Requests { get; private set; }
            /// <inheritdoc/>
            public bool IsAdReady() => true;
            /// <inheritdoc/>
            public bool IsInterstitialAdReady() => false;
            /// <inheritdoc/>
            public void ShowRewardedAd(Action<bool> callback) { Requests++; Pending = callback; }
            /// <inheritdoc/>
            public void ShowInterstitialAd(Action callback) => callback();
        }
    }
}
