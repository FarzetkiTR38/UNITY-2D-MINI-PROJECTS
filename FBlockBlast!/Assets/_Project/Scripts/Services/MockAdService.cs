using System;
using UnityEngine;
using NeonGalaxy.Core;

namespace NeonGalaxy.Services
{
    /// <summary>
    /// Mock ad service for testing without a real ad SDK.
    /// Rewarded ads always succeed after a simulated delay.
    /// Interstitials always dismiss immediately.
    /// </summary>
    public class MockAdService : IAdService
    {
        public bool IsRewardedAdReady => true;
        public bool IsInterstitialReady => true;

        public MockAdService()
        {
            Debug.Log("[MockAdService] Initialized (mock). Ads are always ready.");
        }

        public void ShowRewardedAd(Action<bool> onComplete)
        {
            Debug.Log("[MockAdService] Showing mock rewarded ad...");
            SimulateRewardedAd(onComplete);
        }

        public void ShowInterstitial(Action onComplete)
        {
            Debug.Log("[MockAdService] Showing mock interstitial...");
            SimulateInterstitial(onComplete);
        }

        public void PreloadAds()
        {
            Debug.Log("[MockAdService] PreloadAds called (mock — no-op).");
        }

        // ── Internal ─────────────────────────────────────────────

        private async void SimulateRewardedAd(Action<bool> onComplete)
        {
            // Simulate the player watching a full rewarded ad
            await System.Threading.Tasks.Task.Delay(1000);

            Debug.Log("[MockAdService] Mock rewarded ad completed — reward granted.");
            GameEvents.InvokeAdRewardReceived(true);
            onComplete?.Invoke(true);
        }

        private async void SimulateInterstitial(Action onComplete)
        {
            // Simulate interstitial display time
            await System.Threading.Tasks.Task.Delay(500);

            Debug.Log("[MockAdService] Mock interstitial dismissed.");
            onComplete?.Invoke();
        }
    }
}
