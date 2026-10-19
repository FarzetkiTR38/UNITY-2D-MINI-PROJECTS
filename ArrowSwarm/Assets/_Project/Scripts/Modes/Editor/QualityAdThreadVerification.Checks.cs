namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using ArrowSwarm.Ads;

    /// <summary>Asserts callback FIFO, inline behavior and completion despite native cleanup exceptions.</summary>
    public static partial class QualityAdThreadVerification
    {
        private static readonly List<string> PlaybackOrder = new List<string>();
        private static bool _rewardHeld;

        private static void CheckInlineDispatch()
        {
            bool inline = false; AdUnityThread.Run(() => inline = true);
            Check(inline, "An already-main-thread callback executes inline when no native events are pending");
            var nested = new List<int>();
            AdUnityThread.Run(() =>
            {
                nested.Add(1); AdUnityThread.Run(() => nested.Add(2)); nested.Add(3);
            });
            Check(nested.Count == 3 && nested[0] == 1 && nested[1] == 2 && nested[2] == 3,
                "Nested main-thread callbacks retain their original inline order");
        }

        private static void CheckFifoBatch()
        {
            Check(_worker != null && !_worker.IsAlive && _worker.ManagedThreadId != _mainThread,
                "Callbacks were posted by a genuine separate managed thread");
            Check(_allOnMain, "Every background callback, including discarded-owner cleanup, executed on Unity's main thread");
            Check(Order.Count == 24, "All 24 queued background callbacks executed exactly once");
            for (int i = 0; i < Order.Count; i++)
                Check(Order[i] == i, "Background FIFO index " + i + " preserved");
            Check(_staticCalls == 1, "A static delegate is delivered despite the SDK executor's null-Target filter");
            Check(_liveOwner == 0 && _discarded == 1,
                "Owner destroyed after posting is skipped and its orphan cleanup runs once on Unity");
        }

        private static void StartMixedOrigin()
        {
            Status = "Checking background reward before a main-thread close";
            PlaybackOrder.Clear(); _rewardHeld = false;
            _manager.ShowRewardedAd(success => { ObserveThread(); if (success) _rewards++; });
            var playback = new AdRewardCompletion(_fake.Reward);
            PostFromWorker(() => AdUnityThread.Run(() =>
            {
                ObserveThread(); PlaybackOrder.Add("reward"); playback.MarkEarned();
                _rewardHeld = _manager.IsShowingAd && _rewards == 0;
            }));
            // This is deliberately main-thread while the earlier native reward is still waiting for Update.
            AdUnityThread.Run(() =>
            {
                ObserveThread(); PlaybackOrder.Add("close");
                AdCallbackLifecycle.Complete(() => PlaybackOrder.Add("cleanup"), playback.Complete);
            });
            Check(_rewards == 0 && _manager.IsShowingAd && PlaybackOrder.Count == 0,
                "Main-thread close queues behind an earlier background reward instead of overtaking it");
        }

        private static void CheckMixedOrigin()
        {
            Check(PlaybackOrder.Count == 3 && PlaybackOrder[0] == "reward" && PlaybackOrder[1] == "close" && PlaybackOrder[2] == "cleanup",
                "Mixed-origin reward, close and cleanup preserve their delivered FIFO order");
            Check(_rewardHeld && _rewards == 1 && !_manager.IsShowingAd,
                "Earned reward holds the fullscreen gate until close and is delivered exactly once");
            Check(_allOnMain, "Mixed-origin gameplay completion runs only on Unity's main thread");
            CheckInlineDispatch();
        }

        private static void StartMainThenWorker()
        {
            Status = "Checking already-main reward followed by a background close";
            PlaybackOrder.Clear();
            _manager.ShowRewardedAd(success => { ObserveThread(); if (success) _rewards++; });
            var playback = new AdRewardCompletion(_fake.Reward);
            AdUnityThread.Run(() => { PlaybackOrder.Add("reward"); playback.MarkEarned(); });
            Check(PlaybackOrder.Count == 1 && _rewards == 1 && _manager.IsShowingAd,
                "Already-main SDK reward records inline without an extra frame or premature reward delivery");
            PostFromWorker(() => AdUnityThread.Run(() =>
            {
                ObserveThread(); PlaybackOrder.Add("close"); playback.Complete();
            }));
        }

        private static void CheckMainThenWorker()
        {
            Check(PlaybackOrder.Count == 2 && PlaybackOrder[0] == "reward" && PlaybackOrder[1] == "close" &&
                _rewards == 2 && !_manager.IsShowingAd && _allOnMain,
                "Already-main reward and background close complete once in order on Unity");
        }

        private static void CheckCleanupFailures()
        {
            int callbacks = 0;
            _manager.ShowRewardedAd(success => { if (success) callbacks++; });
            var playback = new AdRewardCompletion(_fake.Reward); playback.MarkEarned();
            bool threw = false;
            try
            {
                AdCallbackLifecycle.Complete(() => { throw new InvalidOperationException("Expected native cleanup fixture"); }, playback.Complete);
            }
            catch (InvalidOperationException) { threw = true; }
            Check(threw && callbacks == 1 && !_manager.IsShowingAd,
                "Native reward cleanup exception cannot strand its earned result or fullscreen gate");
            playback.Complete(); Check(callbacks == 1, "Cleanup exception followed by another close still completes once");
            int closed = 0; _manager.ShowInterstitialAd(() => closed++); threw = false;
            try
            {
                AdCallbackLifecycle.Complete(() => { throw new InvalidOperationException("Expected preload fixture"); }, _fake.Closed);
            }
            catch (InvalidOperationException) { threw = true; }
            Check(threw && closed == 1 && !_manager.IsShowingAd,
                "Interstitial preload exception still releases navigation and the fullscreen gate");
        }
    }
}
