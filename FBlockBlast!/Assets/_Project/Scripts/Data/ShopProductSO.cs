using UnityEngine;

namespace NeonGalaxy.Data
{
    /// <summary>
    /// Type of shop product for categorization and purchase flow.
    /// </summary>
    public enum ShopProductType
    {
        RemoveAds,
        StarterPack,
        CoinPack,
        GemPack,
        CosmeticPack
    }

    /// <summary>
    /// Defines a single purchasable product in the shop.
    /// Supports both real-money IAP products and coin-purchasable items.
    /// Create instances via: Create → NeonGalaxy → Shop Product.
    /// </summary>
    [CreateAssetMenu(fileName = "NewShopProduct", menuName = "NeonGalaxy/Shop Product", order = 22)]
    public class ShopProductSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique product identifier matching Constants IAP ID.")]
        public string productId;

        [Tooltip("Display name in shop UI.")]
        public string displayName;

        [Tooltip("Short description for the product card.")]
        [TextArea(1, 3)]
        public string description;

        [Tooltip("Product icon for the shop card.")]
        public Sprite icon;

        [Header("Type")]
        [Tooltip("Category of this product.")]
        public ShopProductType productType;

        [Header("Currency Pack Details")]
        [Tooltip("Number of coins granted if this pack provides coins.")]
        public int coinAmount;

        [Tooltip("Number of gems granted if this pack provides gems.")]
        public int gemAmount;

        [Header("Cosmetic Pack Details")]
        [Tooltip("Cosmetics included if this is a CosmeticPack.")]
        public CosmeticItemSO[] includedCosmetics;

        [Header("Pricing")]
        [Tooltip("IAP product ID for real-money purchase. Maps to Constants.IAP_* IDs.")]
        public string iapProductId;

        [Tooltip("Select whether this item is purchased with Coin or Gem.")]
        public CurrencyType costCurrencyType = CurrencyType.Coin;

        [Tooltip("If > 0, this item can also be purchased with in-game coins.")]
        public int coinCost;

        [Tooltip("If > 0, this item can also be purchased with in-game gems.")]
        public int gemCost;

        [Header("Display")]
        [Tooltip("Badge text for promotional items (e.g., 'BEST VALUE', '2X COINS'). Leave empty for no badge.")]
        public string badgeText;

        [Tooltip("Sort order in the shop (lower = displayed first).")]
        public int sortOrder;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(productId))
                productId = name;
            if (string.IsNullOrEmpty(iapProductId))
                iapProductId = productId;
        }
#endif
    }
}
