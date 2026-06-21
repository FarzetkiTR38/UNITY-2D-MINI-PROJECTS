using System;
using System.Collections.Generic;
using UnityEngine;
using NeonGalaxy.Utility;

namespace NeonGalaxy.Services
{
    /// <summary>
    /// Mock IAP service for testing without a real store.
    /// All purchases succeed after a simulated delay.
    /// Prices are hardcoded mock strings.
    /// </summary>
    public class MockIAPService : IIAPService
    {
        private readonly HashSet<string> _ownedProducts = new HashSet<string>();
        private readonly Dictionary<string, string> _mockPrices;

        public MockIAPService()
        {
            _mockPrices = new Dictionary<string, string>
            {
                { Constants.IAP_REMOVE_ADS,   "$2.99" },
                { Constants.IAP_STARTER_PACK, "$4.99" },
                { Constants.IAP_COINS_500,    "$0.99" },
                { Constants.IAP_COINS_1500,   "$2.99" },
                { Constants.IAP_COINS_5000,   "$7.99" },
            };

            Debug.Log("[MockIAPService] Initialized with mock prices.");
        }

        public bool IsInitialized => true;

        public string GetLocalizedPrice(string productId)
        {
            if (_mockPrices.TryGetValue(productId, out var price))
                return price;

            return "$?.??";
        }

        public void PurchaseProduct(string productId, Action<bool> onComplete)
        {
            Debug.Log($"[MockIAPService] Mock purchase started for: {productId}");

            // Simulate async purchase with a short delay
            // In a real implementation, this would go through Unity IAP
            SimulatePurchase(productId, onComplete);
        }

        public bool IsProductOwned(string productId)
        {
            return _ownedProducts.Contains(productId);
        }

        public void RestorePurchases(Action<bool> onComplete)
        {
            Debug.Log("[MockIAPService] Mock restore purchases — nothing to restore.");
            onComplete?.Invoke(true);
        }

        // ── Internal ─────────────────────────────────────────────

        private async void SimulatePurchase(string productId, Action<bool> onComplete)
        {
            // Simulate store confirmation delay
            await System.Threading.Tasks.Task.Delay(800);

            // Mark non-consumable as owned
            if (productId == Constants.IAP_REMOVE_ADS || productId == Constants.IAP_STARTER_PACK)
            {
                _ownedProducts.Add(productId);
            }

            Debug.Log($"[MockIAPService] Mock purchase completed for: {productId}");
            onComplete?.Invoke(true);
        }
    }
}
