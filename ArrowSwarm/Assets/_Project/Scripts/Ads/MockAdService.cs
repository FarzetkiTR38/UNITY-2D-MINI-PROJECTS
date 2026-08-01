namespace ArrowSwarm.Ads
{
    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Mock implementation of IAdService for development/testing.
    /// Simulates a 2-second ad loading screen, then grants the reward.
    /// </summary>
    public class MockAdService : MonoBehaviour, IAdService
    {
        [SerializeField] private float _fakeAdDuration = 2f;
        [SerializeField] private GameObject _fakeAdPanel; // Optional UI panel

        private static MockAdService _instance;

        /// <summary>Singleton-like access for the mock service.</summary>
        public static MockAdService Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        /// <inheritdoc/>
        public void ShowRewardedAd(Action<bool> onRewardGranted)
        {
            StartCoroutine(SimulateAd(onRewardGranted));
            LogDebug("Showing mock rewarded ad...");
        }

        /// <inheritdoc/>
        public bool IsAdReady() => true;

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
