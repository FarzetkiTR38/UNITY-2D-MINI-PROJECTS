namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using ArrowSwarm.Ads;
    using UnityEngine;

    /// <summary>Exercises fullscreen request ownership with faulty delayed providers and no SDK network traffic.</summary>
    public static partial class QualityAdsVerification
    {
        private static readonly List<string> Results = new List<string>();

        /// <summary>Runs synchronous callback regressions in Play Mode and restores the installed provider.</summary>
        public static string Run()
        {
            if (!Application.isPlaying || !AdManager.HasInstance) throw new InvalidOperationException("Play Mode managers required.");
            var manager = AdManager.Instance;
            if (manager.IsShowingAd) throw new InvalidOperationException("An ad is already pending.");
            var field = typeof(AdManager).GetField("_adService", BindingFlags.NonPublic | BindingFlags.Instance);
            var pacing = typeof(AdManager).GetField("_lastInterstitialTime", BindingFlags.NonPublic | BindingFlags.Instance);
            var original = (IAdService)field.GetValue(manager); var savedPacing = pacing.GetValue(manager);
            Results.Clear();
            try
            {
                var fake = new DelayedProvider(); manager.SetAdService(fake);
                CheckRewardRequests(manager, fake);
                CheckInterstitialRequests(manager, fake);
                CheckSdkCompletion(manager, fake);
                manager.SetAdService(original); pacing.SetValue(manager, savedPacing);
                Check(ReferenceEquals(field.GetValue(manager), original) && !manager.IsShowingAd,
                    "Original provider and idle request state restored");
                Check(Equals(pacing.GetValue(manager), savedPacing), "Original interstitial pacing timestamp restored");
                return Save("PASS");
            }
            catch (Exception error) { Save("FAIL: " + error.Message); throw; }
            finally { manager.SetAdService(original); pacing.SetValue(manager, savedPacing); }
        }

        private static void CheckRewardRequests(AdManager manager, DelayedProvider fake)
        {
            int accepted = 0, rejected = 0;
            manager.ShowRewardedAd(success => { if (success) accepted++; });
            var first = fake.Reward;
            Check(fake.Requests == 1 && manager.IsShowingAd, "First rewarded request is pending");
            Check(!manager.IsRewardedAdReady() && !manager.IsInterstitialAdReady(), "Readiness excludes an occupied fullscreen request");
            manager.ShowRewardedAd(success => { if (!success) rejected++; });
            Check(fake.Requests == 1 && rejected == 1, "Repeated reward tap is rejected without replacing its owner");
            first(true); first(true); first(false);
            Check(accepted == 1 && !manager.IsShowingAd, "Faulty duplicate reward/failure callbacks complete once");
            int second = 0;
            manager.ShowRewardedAd(success => { if (success) second++; });
            var pending = fake.Reward; first(true);
            Check(manager.IsShowingAd && second == 0, "Old reward callback cannot complete a later request");
            pending(true); Check(second == 1 && !manager.IsShowingAd, "Later reward keeps its own callback");
            int failures = 0;
            manager.ShowRewardedAd(success => { if (!success) failures++; });
            fake.Reward(false); fake.Reward(false);
            Check(failures == 1 && !manager.IsShowingAd, "Failure result settles once and unlocks another request");
            manager.ShowRewardedAd(null); fake.Reward(true);
            Check(!manager.IsShowingAd, "A null caller callback cannot leave the manager busy");
            fake.Throw = true; manager.ShowRewardedAd(success => { if (!success) failures++; });
            Check(failures == 2 && !manager.IsShowingAd, "A throwing provider settles failure and unlocks requests");
            fake.Throw = false;
            manager.ShowRewardedAd(success => { if (!success) failures++; }); var cancelled = fake.Reward;
            manager.SetAdService(fake); cancelled(true);
            Check(failures == 3 && !manager.IsShowingAd, "Replacing a service cancels its owner and ignores late success");
        }

        private static void CheckInterstitialRequests(AdManager manager, DelayedProvider fake)
        {
            int closed = 0, rejected = 0;
            manager.ShowInterstitialAd(() => closed++); var first = fake.Closed;
            manager.ShowRewardedAd(success => { if (!success) rejected++; });
            Check(manager.IsShowingAd && rejected == 1, "Interstitial playback excludes a simultaneous rewarded request");
            first(); first();
            Check(closed == 1 && !manager.IsShowingAd, "Duplicate interstitial close completes once");
            manager.ShowInterstitialAd(() => closed++); var second = fake.Closed; first();
            Check(closed == 1 && manager.IsShowingAd, "Old interstitial event cannot close a later playback");
            second(); Check(closed == 2 && !manager.IsShowingAd, "Later interstitial closes its own request");
            fake.Throw = true; manager.ShowInterstitialAd(() => closed++); fake.Throw = false;
            Check(closed == 3 && !manager.IsShowingAd, "Throwing interstitial provider still releases navigation");
        }

        private static void CheckSdkCompletion(AdManager manager, DelayedProvider fake)
        {
            int rewards = 0, failures = 0;
            var earned = new AdRewardCompletion(success => { if (success) rewards++; else failures++; });
            earned.MarkEarned(); earned.MarkEarned();
            Check(rewards == 0 && failures == 0, "SDK reward records intent without applying effects during fullscreen playback");
            earned.Complete(); earned.Complete(); earned.MarkEarned();
            Check(rewards == 1 && failures == 0, "SDK earned reward is delivered once only after close");
            var skipped = new AdRewardCompletion(success => { if (success) rewards++; else failures++; });
            skipped.Complete(); skipped.Complete(); skipped.MarkEarned();
            Check(rewards == 1 && failures == 1, "SDK unrewarded close settles failure once");
            manager.ShowRewardedAd(success => { if (success) rewards++; });
            var playback = new AdRewardCompletion(fake.Reward); playback.MarkEarned();
            Check(manager.IsShowingAd && !manager.IsInterstitialAdReady(), "Recorded SDK reward keeps the fullscreen gate until close");
            playback.Complete();
            Check(!manager.IsShowingAd && rewards == 2, "Fullscreen close releases the manager and delivers its earned reward");
        }

        private static void Check(bool condition, string label)
        { Results.Add((condition ? "PASS: " : "FAIL: ") + label); if (!condition) throw new InvalidOperationException(label); }

        private static string Save(string status)
        {
            string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Captures/QualityPass"));
            Directory.CreateDirectory(directory);
            var lines = new List<string> { status }; lines.AddRange(Results);
            File.WriteAllLines(Path.Combine(directory, "ads-verification.txt"), lines);
            return status + ": " + Results.Count + " assertions";
        }

        /// <summary>Retains callbacks after completion to simulate duplicated and obsolete SDK deliveries.</summary>
        private sealed class DelayedProvider : IAdService
        {
            /// <summary>Last rewarded provider callback.</summary>
            public Action<bool> Reward { get; private set; }
            /// <summary>Last interstitial provider callback.</summary>
            public Action Closed { get; private set; }
            /// <summary>Number of forwarded rewarded requests.</summary>
            public int Requests { get; private set; }
            /// <summary>Whether the next show deliberately throws before completion.</summary>
            public bool Throw { get; set; }
            /// <inheritdoc/>
            public bool IsAdReady() => true;
            /// <inheritdoc/>
            public bool IsInterstitialAdReady() => true;
            /// <inheritdoc/>
            public void ShowRewardedAd(Action<bool> callback)
            { if (Throw) throw new InvalidOperationException("Expected faulty test provider"); Requests++; Reward = callback; }
            /// <inheritdoc/>
            public void ShowInterstitialAd(Action callback)
            { if (Throw) throw new InvalidOperationException("Expected faulty test provider"); Closed = callback; }
        }
    }
}
