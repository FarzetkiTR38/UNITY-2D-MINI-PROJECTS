using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using NeonGalaxy.Utility;

#pragma warning disable CS0618 // Disable obsolete warnings for IAP v4 API

namespace NeonGalaxy.Services
{
    /// <summary>
    /// Real implementation of IIAPService using Unity In-App Purchasing.
    /// Handles product catalog initialization, purchasing, and receipt validation.
    /// </summary>
    public class UnityIAPService : IIAPService, IDetailedStoreListener
    {
        private IStoreController _storeController;
        private IExtensionProvider _extensionProvider;

        private Action<bool> _onPurchaseComplete;
        private string _pendingProductId;

        public bool IsInitialized => _storeController != null && _extensionProvider != null;

        public UnityIAPService()
        {
            InitializePurchasing();
        }

        private void InitializePurchasing()
        {
            if (IsInitialized) return;

            Debug.Log("[UnityIAPService] Initializing Unity IAP...");
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            // Add non-consumables
            builder.AddProduct(Constants.IAP_REMOVE_ADS, ProductType.NonConsumable);
            builder.AddProduct(Constants.IAP_STARTER_PACK, ProductType.NonConsumable);

            // Add consumables (Coins)
            builder.AddProduct(Constants.IAP_COINS_500, ProductType.Consumable);
            builder.AddProduct(Constants.IAP_COINS_1500, ProductType.Consumable);
            builder.AddProduct(Constants.IAP_COINS_5000, ProductType.Consumable);

            // Add consumables (Gems)
            builder.AddProduct(Constants.IAP_GEMS_100, ProductType.Consumable);
            builder.AddProduct(Constants.IAP_GEMS_500, ProductType.Consumable);
            builder.AddProduct(Constants.IAP_GEMS_1500, ProductType.Consumable);

            UnityPurchasing.Initialize(this, builder);
        }

        public string GetLocalizedPrice(string productId)
        {
            if (!IsInitialized) return "---";

            var product = _storeController.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                return product.metadata.localizedPriceString;
            }
            return "N/A";
        }

        public void PurchaseProduct(string productId, Action<bool> onComplete)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[UnityIAPService] Not initialized.");
                onComplete?.Invoke(false);
                return;
            }

            var product = _storeController.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                _pendingProductId = productId;
                _onPurchaseComplete = onComplete;
                _storeController.InitiatePurchase(product);
            }
            else
            {
                Debug.LogWarning($"[UnityIAPService] Product {productId} is not available.");
                onComplete?.Invoke(false);
            }
        }

        public bool IsProductOwned(string productId)
        {
            if (!IsInitialized) return false;

            var product = _storeController.products.WithID(productId);
            return product != null && product.hasReceipt;
        }

        public void RestorePurchases(Action<bool> onComplete)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[UnityIAPService] Not initialized.");
                onComplete?.Invoke(false);
                return;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                var apple = _extensionProvider.GetExtension<IAppleExtensions>();
                apple.RestoreTransactions((result, error) =>
                {
                    Debug.Log($"[UnityIAPService] Restore completed with result: {result}. Error: {error}");
                    onComplete?.Invoke(result);
                });
            }
            else
            {
                Debug.LogWarning("[UnityIAPService] RestorePurchases is not required/supported on this platform.");
                onComplete?.Invoke(true);
            }
        }

        // ── IStoreListener / IDetailedStoreListener ────────────────

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
            _extensionProvider = extensions;
            Debug.Log("[UnityIAPService] Initialization complete.");
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"[UnityIAPService] Initialization failed: {error}");
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"[UnityIAPService] Initialization failed: {error} - {message}");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            var product = purchaseEvent.purchasedProduct;
            Debug.Log($"[UnityIAPService] Purchase successful: {product.definition.id}");

            if (_pendingProductId == product.definition.id)
            {
                _onPurchaseComplete?.Invoke(true);
                _onPurchaseComplete = null;
                _pendingProductId = null;
            }
            else
            {
                // Edge case: A purchase was completed (or restored) but wasn't initiated in this session
                Debug.Log($"[UnityIAPService] Unsolicited purchase processed: {product.definition.id}");
                // Typically you'd send an event here so the UI can update if necessary
            }

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.LogError($"[UnityIAPService] Purchase failed ({product.definition.id}): {failureReason}");

            if (_pendingProductId == product.definition.id)
            {
                _onPurchaseComplete?.Invoke(false);
                _onPurchaseComplete = null;
                _pendingProductId = null;
            }
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Debug.LogError($"[UnityIAPService] Purchase failed ({product.definition.id}): {failureDescription.message}");

            if (_pendingProductId == product.definition.id)
            {
                _onPurchaseComplete?.Invoke(false);
                _onPurchaseComplete = null;
                _pendingProductId = null;
            }
        }
    }
}
#pragma warning restore CS0618
