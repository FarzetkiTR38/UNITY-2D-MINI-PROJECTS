namespace ArrowSwarm.Ads
{
    using System;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Central manager for in-game advertisements (Rewarded Ads, Interstitials).
    /// Wraps IAdService allowing seamless plug-and-play switching between
    /// MockAdService (editor/testing) and real SDKs (Unity Ads, AdMob, LevelPlay).
    /// </summary>
    public class AdManager : Singleton<AdManager>
    {
        [SerializeField] private MonoBehaviour _adServiceProvider;

        [Header("Interstitial Pacing")]
        [SerializeField] private float _interstitialCooldownSeconds = 90f;
        [SerializeField] private int _minLevelForInterstitials = 4;

        private IAdService _adService;
        private float _lastInterstitialTime = -999f;

        protected override void OnSingletonAwake()
        {
            ResolveService();
        }

        private void ResolveService()
        {
            if (_adService != null) return;

            if (_adServiceProvider is IAdService provider)
            {
                _adService = provider;
                return;
            }

            _adService = GetComponent<IAdService>();
            if (_adService != null) return;

            var googleService = FindFirstObjectByType<GoogleMobileAdsService>(FindObjectsInactive.Include);
            if (googleService != null)
            {
                _adService = googleService;
                return;
            }

#if !UNITY_EDITOR
            _adService = gameObject.AddComponent<GoogleMobileAdsService>();
            return;
#endif

            _adService = MockAdService.Instance;
        }

        /// <summary>
        /// Registers or swaps the active Ad Service provider (e.g. UnityAdsService or GoogleMobileAdsService).
        /// </summary>
        public void SetAdService(IAdService service)
        {
            _adService = service;
        }

        /// <summary>
        /// Shows a rewarded ad and invokes the callback upon completion.
        /// </summary>
        /// <param name="onRewardGranted">Callback receiving true if user watched full ad and earned reward.</param>
        public void ShowRewardedAd(Action<bool> onRewardGranted)
        {
            ResolveService();

            if (_adService != null)
            {
                _adService.ShowRewardedAd(onRewardGranted);
            }
            else
            {
                Debug.LogWarning("[ArrowSwarm] AdManager: No ad service available! Simulating direct reward.");
                onRewardGranted?.Invoke(true);
            }
        }

        /// <summary>
        /// Returns true if a rewarded ad is currently cached and ready to play.
        /// </summary>
        public bool IsRewardedAdReady()
        {
            ResolveService();
            return _adService != null && _adService.IsAdReady();
        }

        /// <summary>
        /// Shows an interstitial ad and invokes callback when closed or failed.
        /// </summary>
        public void ShowInterstitialAd(Action onClosed)
        {
            ResolveService();

            if (_adService != null && _adService.IsInterstitialAdReady())
            {
                _lastInterstitialTime = Time.realtimeSinceStartup;
                _adService.ShowInterstitialAd(onClosed);
            }
            else
            {
                onClosed?.Invoke();
            }
        }

        /// <summary>
        /// Shows an interstitial ad only if pacing rules (min level, cooldown timer) are met.
        /// </summary>
        public void ShowInterstitialWithPacing(int currentLevel, Action onClosed)
        {
            if (currentLevel < _minLevelForInterstitials)
            {
                onClosed?.Invoke();
                return;
            }

            float elapsed = Time.realtimeSinceStartup - _lastInterstitialTime;
            if (elapsed < _interstitialCooldownSeconds)
            {
                onClosed?.Invoke();
                return;
            }

            ShowInterstitialAd(onClosed);
        }

        /// <summary>
        /// Returns true if an interstitial ad is currently cached and ready to play.
        /// </summary>
        public bool IsInterstitialAdReady()
        {
            ResolveService();
            return _adService != null && _adService.IsInterstitialAdReady();
        }
    }
}
