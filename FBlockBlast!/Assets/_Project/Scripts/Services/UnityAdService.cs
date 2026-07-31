using System;
using UnityEngine;
using UnityEngine.Advertisements;

namespace NeonGalaxy.Services
{
    /// <summary>
    /// Real implementation of IAdService using Unity Ads.
    /// Manages initialization, loading, and showing of Interstitial and Rewarded ads.
    /// </summary>
    public class UnityAdService : IAdService, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
    {
#if UNITY_IOS
        private const string GAME_ID = "6166175"; // Replace with your iOS Game ID
        private const string REWARDED_AD_UNIT_ID = "Rewarded_iOS";
        private const string INTERSTITIAL_AD_UNIT_ID = "Interstitial_iOS";
#else
        private const string GAME_ID = "6166174"; // Replace with your Android Game ID
        private const string REWARDED_AD_UNIT_ID = "Rewarded_Android";
        private const string INTERSTITIAL_AD_UNIT_ID = "Interstitial_Android";
#endif

        private const bool TEST_MODE = false; // Set to false for production

        private bool _isRewardedReady;
        private bool _isInterstitialReady;

        private Action<bool> _onRewardedComplete;
        private Action _onInterstitialComplete;

        public bool IsRewardedAdReady => _isRewardedReady;
        public bool IsInterstitialReady => _isInterstitialReady;

        public UnityAdService()
        {
            if (!Advertisement.isSupported)
            {
                Debug.LogWarning("[UnityAdService] Platform not supported.");
                return;
            }

            if (!Advertisement.isInitialized && !Advertisement.isShowing)
            {
                Debug.Log($"[UnityAdService] Initializing with GameID: {GAME_ID}");
                Advertisement.Initialize(GAME_ID, TEST_MODE, this);
            }
            else
            {
                // Already initialized (e.g. domain reload disabled in editor)
                PreloadAds();
            }
        }

        public void PreloadAds()
        {
            if (!Advertisement.isInitialized) return;

            Debug.Log("[UnityAdService] Preloading ads...");
            Advertisement.Load(REWARDED_AD_UNIT_ID, this);
            Advertisement.Load(INTERSTITIAL_AD_UNIT_ID, this);
        }

        public void ShowRewardedAd(Action<bool> onComplete)
        {
            _onRewardedComplete = onComplete;

            if (_isRewardedReady)
            {
                _isRewardedReady = false; // Reset state
                Advertisement.Show(REWARDED_AD_UNIT_ID, this);
            }
            else
            {
                Debug.LogWarning("[UnityAdService] Rewarded ad not ready. Attempting to reload...");
                _onRewardedComplete?.Invoke(false);
                Advertisement.Load(REWARDED_AD_UNIT_ID, this);
            }
        }

        public void ShowInterstitial(Action onComplete)
        {
            _onInterstitialComplete = onComplete;

            if (_isInterstitialReady)
            {
                _isInterstitialReady = false; // Reset state
                Advertisement.Show(INTERSTITIAL_AD_UNIT_ID, this);
            }
            else
            {
                Debug.LogWarning("[UnityAdService] Interstitial ad not ready. Attempting to reload...");
                _onInterstitialComplete?.Invoke();
                Advertisement.Load(INTERSTITIAL_AD_UNIT_ID, this);
            }
        }

        // ── IUnityAdsInitializationListener ─────────────────────

        public void OnInitializationComplete()
        {
            Debug.Log("[UnityAdService] Initialization complete.");
            PreloadAds();
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.LogError($"[UnityAdService] Initialization failed: {error} - {message}");
        }

        // ── IUnityAdsLoadListener ───────────────────────────────

        public void OnUnityAdsAdLoaded(string placementId)
        {
            Debug.Log($"[UnityAdService] Ad loaded: {placementId}");
            if (placementId == REWARDED_AD_UNIT_ID)
                _isRewardedReady = true;
            else if (placementId == INTERSTITIAL_AD_UNIT_ID)
                _isInterstitialReady = true;
        }

        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
        {
            Debug.LogWarning($"[UnityAdService] Ad failed to load ({placementId}): {error} - {message}");
            if (placementId == REWARDED_AD_UNIT_ID)
                _isRewardedReady = false;
            else if (placementId == INTERSTITIAL_AD_UNIT_ID)
                _isInterstitialReady = false;
        }

        // ── IUnityAdsShowListener ───────────────────────────────

        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            Debug.LogError($"[UnityAdService] Error showing ad ({placementId}): {error} - {message}");

            if (placementId == REWARDED_AD_UNIT_ID)
            {
                _onRewardedComplete?.Invoke(false);
                Advertisement.Load(REWARDED_AD_UNIT_ID, this); // Reload
            }
            else if (placementId == INTERSTITIAL_AD_UNIT_ID)
            {
                _onInterstitialComplete?.Invoke();
                Advertisement.Load(INTERSTITIAL_AD_UNIT_ID, this); // Reload
            }
        }

        public void OnUnityAdsShowStart(string placementId) { }
        public void OnUnityAdsShowClick(string placementId) { }

        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            Debug.Log($"[UnityAdService] Ad show complete ({placementId}): {showCompletionState}");

            if (placementId == REWARDED_AD_UNIT_ID)
            {
                bool success = showCompletionState == UnityAdsShowCompletionState.COMPLETED;
                _onRewardedComplete?.Invoke(success);
                Advertisement.Load(REWARDED_AD_UNIT_ID, this); // Preload next
            }
            else if (placementId == INTERSTITIAL_AD_UNIT_ID)
            {
                _onInterstitialComplete?.Invoke();
                Advertisement.Load(INTERSTITIAL_AD_UNIT_ID, this); // Preload next
            }
        }
    }
}
