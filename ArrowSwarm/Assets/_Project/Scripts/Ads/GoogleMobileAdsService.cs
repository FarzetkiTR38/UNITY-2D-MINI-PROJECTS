namespace ArrowSwarm.Ads
{
    using System;
    using GoogleMobileAds.Api;
    using UnityEngine;

    /// <summary>
    /// Production implementation of IAdService using Google Mobile Ads (AdMob) SDK.
    /// Manages loading, caching, and showing Rewarded and Interstitial ads with mediation support.
    /// </summary>
    [AddComponentMenu("ArrowSwarm/Ads/Google Mobile Ads Service")]
    public class GoogleMobileAdsService : MonoBehaviour, IAdService
    {
        private const string TestRewardedId = "ca-app-pub-3940256099942544/5224354917";
        private const string TestInterstitialId = "ca-app-pub-3940256099942544/1033173712";

        [Header("Test Mode")]
        [Tooltip("When enabled, uses official Google test ad units to prevent invalid traffic bans.")]
        [SerializeField] private bool _useTestAds = true;

        [Header("Production Ad Unit IDs (Android)")]
        [SerializeField] private string _rewardedAdUnitId = "";
        [SerializeField] private string _interstitialAdUnitId = "";

        private RewardedAd _rewardedAd;
        private InterstitialAd _interstitialAd;
        private bool _isInitialized;
        private bool _isLoadingRewarded;
        private bool _isLoadingInterstitial;

        private Action<bool> _currentRewardCallback;
        private Action _currentInterstitialCallback;

        private string ActiveRewardedId => _useTestAds || string.IsNullOrEmpty(_rewardedAdUnitId)
            ? TestRewardedId
            : _rewardedAdUnitId;

        private string ActiveInterstitialId => _useTestAds || string.IsNullOrEmpty(_interstitialAdUnitId)
            ? TestInterstitialId
            : _interstitialAdUnitId;

        private void Start()
        {
            InitializeSdk();
        }

        private void InitializeSdk()
        {
            MobileAds.Initialize(initStatus =>
            {
                _isInitialized = true;
                Debug.Log("[ArrowSwarm] Google Mobile Ads initialized successfully.");
                LoadRewardedAd();
                LoadInterstitialAd();
            });
        }

        #region Rewarded Ads

        /// <summary>
        /// Pre-loads a rewarded ad if not already cached.
        /// </summary>
        public void LoadRewardedAd()
        {
            if (!_isInitialized || _isLoadingRewarded || IsAdReady()) return;

            _isLoadingRewarded = true;
            CleanUpRewardedAd();

            AdRequest request = new AdRequest();
            RewardedAd.Load(ActiveRewardedId, request, (RewardedAd ad, LoadAdError error) =>
            {
                _isLoadingRewarded = false;
                if (error != null || ad == null)
                {
                    Debug.LogWarning($"[ArrowSwarm] Rewarded ad failed to load: {error?.GetMessage()}");
                    return;
                }

                _rewardedAd = ad;
                RegisterRewardedEvents(ad);
                Debug.Log("[ArrowSwarm] Rewarded ad loaded and ready.");
            });
        }

        private void RegisterRewardedEvents(RewardedAd ad)
        {
            ad.OnAdFullScreenContentClosed += () =>
            {
                CleanUpRewardedAd();
                LoadRewardedAd();
            };

            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogWarning($"[ArrowSwarm] Rewarded ad failed to show: {error.GetMessage()}");
                _currentRewardCallback?.Invoke(false);
                _currentRewardCallback = null;
                CleanUpRewardedAd();
                LoadRewardedAd();
            };
        }

        /// <inheritdoc/>
        public void ShowRewardedAd(Action<bool> onRewardGranted)
        {
            if (!IsAdReady())
            {
                Debug.LogWarning("[ArrowSwarm] Rewarded ad not ready. Attempting to reload.");
                onRewardGranted?.Invoke(false);
                LoadRewardedAd();
                return;
            }

            _currentRewardCallback = onRewardGranted;

            _rewardedAd.Show((Reward reward) =>
            {
                _currentRewardCallback?.Invoke(true);
                _currentRewardCallback = null;
            });
        }

        /// <inheritdoc/>
        public bool IsAdReady() => _rewardedAd != null && _rewardedAd.CanShowAd();

        private void CleanUpRewardedAd()
        {
            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }
        }

        #endregion

        #region Interstitial Ads

        /// <summary>
        /// Pre-loads an interstitial ad if not already cached.
        /// </summary>
        public void LoadInterstitialAd()
        {
            if (!_isInitialized || _isLoadingInterstitial || IsInterstitialAdReady()) return;

            _isLoadingInterstitial = true;
            CleanUpInterstitialAd();

            AdRequest request = new AdRequest();
            InterstitialAd.Load(ActiveInterstitialId, request, (InterstitialAd ad, LoadAdError error) =>
            {
                _isLoadingInterstitial = false;
                if (error != null || ad == null)
                {
                    Debug.LogWarning($"[ArrowSwarm] Interstitial ad failed to load: {error?.GetMessage()}");
                    return;
                }

                _interstitialAd = ad;
                RegisterInterstitialEvents(ad);
                Debug.Log("[ArrowSwarm] Interstitial ad loaded and ready.");
            });
        }

        private void RegisterInterstitialEvents(InterstitialAd ad)
        {
            ad.OnAdFullScreenContentClosed += () =>
            {
                _currentInterstitialCallback?.Invoke();
                _currentInterstitialCallback = null;
                CleanUpInterstitialAd();
                LoadInterstitialAd();
            };

            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogWarning($"[ArrowSwarm] Interstitial ad failed to show: {error.GetMessage()}");
                _currentInterstitialCallback?.Invoke();
                _currentInterstitialCallback = null;
                CleanUpInterstitialAd();
                LoadInterstitialAd();
            };
        }

        /// <inheritdoc/>
        public void ShowInterstitialAd(Action onClosed)
        {
            if (!IsInterstitialAdReady())
            {
                onClosed?.Invoke();
                LoadInterstitialAd();
                return;
            }

            _currentInterstitialCallback = onClosed;
            _interstitialAd.Show();
        }

        /// <inheritdoc/>
        public bool IsInterstitialAdReady() => _interstitialAd != null && _interstitialAd.CanShowAd();

        private void CleanUpInterstitialAd()
        {
            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }
        }

        #endregion

        private void OnDestroy()
        {
            CleanUpRewardedAd();
            CleanUpInterstitialAd();
        }
    }
}
