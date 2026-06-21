using UnityEngine;

namespace NeonGalaxy.Data
{
    /// <summary>
    /// Policy configuration for interstitial ad frequency.
    /// Controls when and how often interstitials are shown.
    /// Create instances via: Create → NeonGalaxy → Ad Policy Config.
    /// </summary>
    [CreateAssetMenu(fileName = "AdPolicyConfig", menuName = "NeonGalaxy/Ad Policy Config", order = 14)]
    public class AdPolicyConfigSO : ScriptableObject
    {
        [Header("Interstitial Frequency")]
        [Tooltip("Number of game-overs before the first interstitial is shown.")]
        public int gamesBeforeFirstInterstitial = 3;

        [Tooltip("Minimum games between interstitials after the first one.")]
        public int gamesBetweenInterstitials = 2;

        [Tooltip("Minimum real-time seconds between interstitials.")]
        public float minSecondsBetweenInterstitials = 120f;

        [Header("Suppression Rules")]
        [Tooltip("If true, skip interstitial if the player just watched a rewarded ad.")]
        public bool suppressAfterRewardedAd = true;

        [Tooltip("If true, skip all interstitials if 'Remove Ads' has been purchased.")]
        public bool suppressForRemoveAdsPurchase = true;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (gamesBeforeFirstInterstitial < 0) gamesBeforeFirstInterstitial = 0;
            if (gamesBetweenInterstitials < 1) gamesBetweenInterstitials = 1;
            if (minSecondsBetweenInterstitials < 0f) minSecondsBetweenInterstitials = 0f;
        }
#endif
    }
}
