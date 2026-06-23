using System;

namespace NeonGalaxy.Services
{
    /// <summary>
    /// Abstraction for in-app purchase operations.
    /// Implementations can target Unity IAP, mock data, or custom backends.
    /// </summary>
    public interface IIAPService
    {
        /// <summary>
        /// Returns true if the IAP service has been initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Returns the localized price string for a product.
        /// </summary>
        string GetLocalizedPrice(string productId);

        /// <summary>
        /// Initiates a purchase flow for the given product.
        /// Callback receives true on success, false on failure/cancellation.
        /// </summary>
        void PurchaseProduct(string productId, Action<bool> onComplete);

        /// <summary>
        /// Returns true if the product is a non-consumable that has been purchased.
        /// </summary>
        bool IsProductOwned(string productId);

        /// <summary>
        /// Restores previously purchased non-consumable products (iOS requirement).
        /// Callback receives true on success.
        /// </summary>
        void RestorePurchases(Action<bool> onComplete);
    }
}
