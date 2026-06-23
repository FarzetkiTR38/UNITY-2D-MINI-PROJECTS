using System;

namespace NeonGalaxy.Services
{
    /// <summary>
    /// Abstraction for ad operations (rewarded and interstitial).
    /// Implementations can target Unity Ads, AdMob, or mock data.
    /// </summary>
    public interface IAdService
    {
        /// <summary>
        /// Returns true if a rewarded ad is loaded and ready to show.
        /// </summary>
        bool IsRewardedAdReady { get; }

        /// <summary>
        /// Returns true if an interstitial ad is loaded and ready to show.
        /// </summary>
        bool IsInterstitialReady { get; }

        /// <summary>
        /// Shows a rewarded ad. Callback receives true if the reward was earned,
        /// false if the user skipped or an error occurred.
        /// </summary>
        void ShowRewardedAd(Action<bool> onComplete);

        /// <summary>
        /// Shows an interstitial ad. Callback is called when the ad is dismissed.
        /// </summary>
        void ShowInterstitial(Action onComplete);

        /// <summary>
        /// Requests ad preloading. Call early to ensure ads are ready.
        /// </summary>
        void PreloadAds();
    }
}
