namespace ArrowSwarm.Ads
{
    using System;

    /// <summary>
    /// Interface for ad services. Allows swapping between mock and real ad SDKs.
    /// </summary>
    public interface IAdService
    {
        /// <summary>
        /// Shows a rewarded ad. Callback receives true if reward was granted.
        /// </summary>
        void ShowRewardedAd(Action<bool> onRewardGranted);

        /// <summary>
        /// Returns true if a rewarded ad is loaded and ready to show.
        /// </summary>
        bool IsAdReady();

        /// <summary>
        /// Shows an interstitial ad. Callback executes when ad is closed or fails.
        /// </summary>
        void ShowInterstitialAd(Action onClosed);

        /// <summary>
        /// Returns true if an interstitial ad is loaded and ready to show.
        /// </summary>
        bool IsInterstitialAdReady();
    }
}

