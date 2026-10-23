namespace ArrowSwarm.Ads
{
    using System;
    using GoogleMobileAds.Api;
    using UnityEngine;

    /// <summary>Keeps rewarded callbacks attached to their own playback until reward or close.</summary>
    public partial class GoogleMobileAdsService
    {
        /// <summary>Preloads one rewarded ad when there is no cached or active playback.</summary>
        public void LoadRewardedAd()
        {
            if (!_isInitialized || _isLoadingRewarded || _rewardPlayback != null || IsAdReady()) return;
            _isLoadingRewarded = true;
            CleanUpRewardedAd();
            RewardedAd.Load(ActiveRewardedId, new AdRequest(), (ad, error) => AdUnityThread.Run(this, () =>
            {
                _isLoadingRewarded = false;
                if (error != null || ad == null)
                {
                    Debug.LogWarning("[ArrowSwarm] Rewarded ad failed to load: " + error?.GetMessage());
                    return;
                }
                _rewardedAd = ad;
                RegisterRewardedEvents(ad);
            }, () => ad?.Destroy()));
        }

        private void RegisterRewardedEvents(RewardedAd ad)
        {
            ad.OnAdFullScreenContentClosed += () => AdUnityThread.Run(this, () => FinishRewardedPlayback(ad));
            ad.OnAdFullScreenContentFailed += error => AdUnityThread.Run(this, () =>
            {
                if (!ReferenceEquals(ad, _rewardedAd)) return;
                Debug.LogWarning("[ArrowSwarm] Rewarded ad failed to show: " + error.GetMessage());
                FinishRewardedPlayback(ad);
            });
        }

        private void FinishRewardedPlayback(RewardedAd ad)
        {
            if (this == null || !ReferenceEquals(ad, _rewardedAd)) return;
            var playback = _rewardPlayback;
            _rewardPlayback = null;
            AdCallbackLifecycle.Complete(() =>
            {
                CleanUpRewardedAd();
                LoadRewardedAd();
            }, () => playback?.Complete());
        }

        /// <inheritdoc/>
        public void ShowRewardedAd(Action<bool> onRewardGranted)
        {
            if (!IsAdReady())
            {
                onRewardGranted?.Invoke(false);
                LoadRewardedAd();
                return;
            }
            var ad = _rewardedAd;
            var playback = new AdRewardCompletion(onRewardGranted);
            _rewardPlayback = playback;
            try
            {
                ad.Show(reward => AdUnityThread.Run(this, () =>
                {
                    if (ReferenceEquals(ad, _rewardedAd) && ReferenceEquals(playback, _rewardPlayback))
                        playback.MarkEarned();
                }));
            }
            catch
            {
                FinishRewardedPlayback(ad);
                throw;
            }
        }

        /// <inheritdoc/>
        public bool IsAdReady() => _rewardPlayback == null && _rewardedAd != null && _rewardedAd.CanShowAd();

        private void CleanUpRewardedAd()
        {
            var ad = _rewardedAd;
            _rewardedAd = null;
            ad?.Destroy();
        }
    }
}
