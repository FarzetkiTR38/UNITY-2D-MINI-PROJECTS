using System;
using UnityEngine;
using NeonGalaxy.Data;
using NeonGalaxy.Services;

namespace NeonGalaxy.Meta
{
    /// <summary>
    /// Controls interstitial ad display policy based on configurable rules.
    /// Checks game count, time intervals, and suppression conditions
    /// before allowing an interstitial to show.
    /// 
    /// Registered in ServiceLocator at boot time.
    /// </summary>
    public class AdPolicyManager
    {
        private readonly SaveService _saveService;
        private readonly IAdService _adService;
        private readonly AdPolicyConfigSO _config;
        private bool _suppressNextInterstitial;

        public AdPolicyManager(SaveService saveService, IAdService adService, AdPolicyConfigSO config)
        {
            _saveService = saveService;
            _adService = adService;
            _config = config;
        }

        // ── Public API ───────────────────────────────────────────

        /// <summary>
        /// Call this when a game-over occurs. Evaluates the interstitial policy
        /// and shows an ad if conditions are met.
        /// </summary>
        /// <param name="onAdDismissed">Called when the ad is dismissed (or skipped).</param>
        public void OnGameOverTriggered(Action onAdDismissed = null)
        {
            var data = _saveService.Data;
            data.gamesPlayedSinceLastInterstitial++;
            _saveService.MarkDirty();

            if (ShouldShowInterstitial())
            {
                ShowInterstitial(onAdDismissed);
            }
            else
            {
                onAdDismissed?.Invoke();
            }
        }

        /// <summary>
        /// Call after a rewarded ad is watched to suppress the next interstitial.
        /// </summary>
        public void OnRewardedAdWatched()
        {
            if (_config.suppressAfterRewardedAd)
            {
                _suppressNextInterstitial = true;
            }
        }

        /// <summary>
        /// Evaluates whether an interstitial should be shown right now.
        /// </summary>
        public bool ShouldShowInterstitial()
        {
            var data = _saveService.Data;

            // Suppress if Remove Ads is purchased
            if (_config.suppressForRemoveAdsPurchase && data.removeAdsPurchased)
            {
                return false;
            }

            // Suppress if just watched a rewarded ad
            if (_suppressNextInterstitial)
            {
                _suppressNextInterstitial = false;
                return false;
            }

            // Check minimum games threshold
            int totalGames = data.gamesPlayedSinceLastInterstitial;
            if (data.lastInterstitialTimestamp == 0)
            {
                // First time — use gamesBeforeFirstInterstitial
                if (totalGames < _config.gamesBeforeFirstInterstitial)
                    return false;
            }
            else
            {
                // Subsequent — use gamesBetweenInterstitials
                if (totalGames < _config.gamesBetweenInterstitials)
                    return false;
            }

            // Check minimum time between interstitials
            if (data.lastInterstitialTimestamp > 0)
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                float elapsed = now - data.lastInterstitialTimestamp;
                if (elapsed < _config.minSecondsBetweenInterstitials)
                    return false;
            }

            // Check if ad is loaded
            if (!_adService.IsInterstitialReady)
                return false;

            return true;
        }

        // ── Internal ─────────────────────────────────────────────

        private void ShowInterstitial(Action onDismissed)
        {
            _adService.ShowInterstitial(() =>
            {
                var data = _saveService.Data;
                data.gamesPlayedSinceLastInterstitial = 0;
                data.lastInterstitialTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                _saveService.MarkDirty();
                _saveService.Save();

                Debug.Log("[AdPolicyManager] Interstitial shown and policy state updated.");
                onDismissed?.Invoke();
            });
        }
    }
}
