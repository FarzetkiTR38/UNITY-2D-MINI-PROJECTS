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
    public partial class GoogleMobileAdsService : MonoBehaviour, IAdService
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
        private bool _isShowingInterstitial;

        private AdRewardCompletion _rewardPlayback;
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
                if (this == null) return;
                _isInitialized = true;
                Debug.Log("[ArrowSwarm] Google Mobile Ads initialized successfully.");
                LoadRewardedAd();
                LoadInterstitialAd();
            });
        }

        #region Interstitial Ads

        /// <summary>
        /// Pre-loads an interstitial ad if not already cached.
        /// </summary>
        public void LoadInterstitialAd()
        {
            if (!_isInitialized || _isLoadingInterstitial || _isShowingInterstitial || IsInterstitialAdReady()) return;

            _isLoadingInterstitial = true;
            CleanUpInterstitialAd();

            AdRequest request = new AdRequest();
            InterstitialAd.Load(ActiveInterstitialId, request, (InterstitialAd ad, LoadAdError error) =>
            {
                if (this == null) { ad?.Destroy(); return; }
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
                FinishInterstitial(ad);
            };

            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                if (this == null || !ReferenceEquals(ad, _interstitialAd)) return;
                Debug.LogWarning($"[ArrowSwarm] Interstitial ad failed to show: {error.GetMessage()}");
                FinishInterstitial(ad);
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
            _isShowingInterstitial = true;
            try { _interstitialAd.Show(); }
            catch { FinishInterstitial(_interstitialAd); throw; }
        }

        /// <inheritdoc/>
        public bool IsInterstitialAdReady() => !_isShowingInterstitial && _interstitialAd != null && _interstitialAd.CanShowAd();

        private void FinishInterstitial(InterstitialAd ad)
        {
            if (this == null || !ReferenceEquals(ad, _interstitialAd)) return;
            var callback = _currentInterstitialCallback;
            _currentInterstitialCallback = null;
            _isShowingInterstitial = false;
            CleanUpInterstitialAd();
            LoadInterstitialAd();
            callback?.Invoke();
        }

        private void CleanUpInterstitialAd()
        {
            var ad = _interstitialAd;
            _interstitialAd = null;
            ad?.Destroy();
        }

        #endregion

        private void OnDestroy()
        {
            var reward = _rewardPlayback;
            var interstitial = _currentInterstitialCallback;
            _rewardPlayback = null;
            _currentInterstitialCallback = null;
            CleanUpRewardedAd();
            CleanUpInterstitialAd();
            reward?.Complete();
            interstitial?.Invoke();
        }
    }
}
