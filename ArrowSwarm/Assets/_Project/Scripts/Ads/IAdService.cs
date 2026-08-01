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
    }
}
