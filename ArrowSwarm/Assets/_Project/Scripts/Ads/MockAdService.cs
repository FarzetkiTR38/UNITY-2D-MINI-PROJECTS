namespace ArrowSwarm.Ads
{
    using System;
    using System.Collections;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Mock implementation of IAdService for development/testing.
    /// Simulates a 2-second ad loading screen, then grants the reward.
    /// </summary>
    public class MockAdService : Singleton<MockAdService>, IAdService
    {
        [SerializeField] private float _fakeAdDuration = 2f;
        [SerializeField] private GameObject _fakeAdPanel; // Optional UI panel

        /// <inheritdoc/>
        public void ShowRewardedAd(Action<bool> onRewardGranted)
        {
            StartCoroutine(SimulateAd(onRewardGranted));
            LogDebug("Showing mock rewarded ad...");
        }

        /// <inheritdoc/>
        public bool IsAdReady() => true;

        /// <inheritdoc/>
        public void ShowInterstitialAd(Action onClosed)
        {
            LogDebug("Showing mock interstitial ad...");
            StartCoroutine(SimulateInterstitial(onClosed));
        }

        /// <inheritdoc/>
        public bool IsInterstitialAdReady() => true;

        private IEnumerator SimulateInterstitial(Action onClosed)
        {
            _fakeAdPanel?.SetActive(true);
            yield return new WaitForSecondsRealtime(1f);
            _fakeAdPanel?.SetActive(false);
            onClosed?.Invoke();
            LogDebug("Mock interstitial closed.");
        }

        private IEnumerator SimulateAd(Action<bool> onRewardGranted)
        {
            // Show fake ad panel
            _fakeAdPanel?.SetActive(true);

            yield return new WaitForSecondsRealtime(_fakeAdDuration);

            // Hide fake ad panel
            _fakeAdPanel?.SetActive(false);

            // Grant reward
            onRewardGranted?.Invoke(true);
            LogDebug("Mock ad completed. Reward granted.");
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] MockAdService: {message}");
        }
    }
}
