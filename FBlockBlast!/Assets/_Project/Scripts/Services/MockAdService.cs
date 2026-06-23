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
            Debug.Log("[MockAdService] Showing mock rewarded ad (immediate main-thread).");
            GameEvents.InvokeAdRewardReceived(true);
            onComplete?.Invoke(true);
        }

        public void ShowInterstitial(Action onComplete)
        {
            Debug.Log("[MockAdService] Showing mock interstitial (immediate main-thread).");
            onComplete?.Invoke();
        }

        public void PreloadAds()
        {
            Debug.Log("[MockAdService] PreloadAds called (mock — no-op).");
        }
    }
}
