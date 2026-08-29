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

        private IAdService _adService;

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

            _adService = GetComponent<IAdService>() ?? MockAdService.Instance;
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
    }
}
