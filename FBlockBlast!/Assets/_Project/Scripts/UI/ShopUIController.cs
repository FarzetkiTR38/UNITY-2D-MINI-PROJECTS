using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonGalaxy.Boot;
using NeonGalaxy.Services;
using NeonGalaxy.Data;
using NeonGalaxy.Meta;
using NeonGalaxy.Core;

namespace NeonGalaxy.UI
{
    /// <summary>
    /// Controls the Shop panel UI.
    /// Displays purchasable products, handles IAP purchases, coin, and gem purchases.
    /// Updated for unified grid display (no tabs).
    /// </summary>
    public class ShopUIController : MonoBehaviour
    {
        [Header("Product Registry")]
        [SerializeField] private ShopProductSO[] products;

        [Header("UI Elements")]
        [SerializeField] private Transform productListParent;
        [SerializeField] private GameObject productCardPrefab;
        [UnityEngine.Serialization.FormerlySerializedAs("coinBalanceText")]
        [SerializeField] private TextMeshProUGUI coinText;
        [UnityEngine.Serialization.FormerlySerializedAs("gemBalanceText")]
        [SerializeField] private TextMeshProUGUI gemText;

        [Header("Limited Offer (Starter Pack)")]
        [SerializeField] private ShopProductSO limitedOfferProduct;
        [SerializeField] private Button limitedOfferButton;
        [SerializeField] private TextMeshProUGUI limitedOfferPriceText;

        [Header("Navigation")]
        [SerializeField] private Button closeButton;

        public event System.Action OnCloseClicked;

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() =>
                {
                    OnCloseClicked?.Invoke();
                    gameObject.SetActive(false);
                });
            }

            if (limitedOfferButton != null && limitedOfferProduct != null)
            {
                limitedOfferButton.onClick.AddListener(() => OnProductPurchaseClicked(limitedOfferProduct));
            }
        }

        private void OnEnable()
        {
            RefreshUI();
            GameEvents.OnCoinBalanceChanged += HandleCoinBalanceChanged;
            GameEvents.OnGemBalanceChanged += HandleGemBalanceChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnCoinBalanceChanged -= HandleCoinBalanceChanged;
            GameEvents.OnGemBalanceChanged -= HandleGemBalanceChanged;
        }

        // ── UI Refresh ───────────────────────────────────────────

        private void RefreshUI()
        {
            RefreshBalances();
            PopulateProductList();
        }

        private void RefreshBalances()
        {
            var currencyManager = ServiceLocator.Get<CurrencyManager>();
            if (currencyManager != null)
            {
                if (coinText != null)
                    coinText.text = $"{currencyManager.GetBalance():N0}";
                
                if (gemText != null)
                    gemText.text = $"{currencyManager.GetGemBalance():N0}";
            }
        }

        private void PopulateProductList()
        {
            if (productListParent == null || productCardPrefab == null) return;

            // Clear existing cards
            foreach (Transform child in productListParent)
            {
                Destroy(child.gameObject);
            }

            // Sort products by order
            var sortedProducts = new List<ShopProductSO>(products);
            sortedProducts.Sort((a, b) => a.sortOrder.CompareTo(b.sortOrder));

            var iapService = ServiceLocator.Get<IIAPService>();

            // Setup Limited Offer Price if available
            if (limitedOfferProduct != null && limitedOfferPriceText != null)
            {
                limitedOfferPriceText.text = GetPriceString(limitedOfferProduct, iapService);
            }

            foreach (var product in sortedProducts)
            {
                if (product != null && product.productType != ShopProductType.StarterPack)
                {
                    CreateProductCard(product, iapService);
                }
            }
        }

        private void CreateProductCard(ShopProductSO product, IIAPService iapService)
        {
            var cardGO = Instantiate(productCardPrefab, productListParent);
            cardGO.SetActive(true);

            // Find common UI components in the prefab (Order: Name, Description/Amount, Price, Badge)
            var texts = cardGO.GetComponentsInChildren<TextMeshProUGUI>(true);
            var button = cardGO.GetComponentInChildren<Button>(true);
            var images = cardGO.GetComponentsInChildren<Image>(true); // To set the icon

            // Set Texts
            if (texts.Length >= 3)
            {
                texts[0].text = product.displayName;
                texts[1].text = string.IsNullOrEmpty(product.description) ? GetAmountString(product) : product.description;
                texts[2].text = GetPriceString(product, iapService);
            }

            // Optional Icon Setup
            if (product.icon != null && images.Length > 1)
            {
                // Assuming images[0] is background, images[1] is the item icon. Adjust in prefab as needed.
                images[1].sprite = product.icon;
            }

            // Handle Ownership (Non-consumable)
            bool isOwned = false;
            if (iapService != null && product.productType == ShopProductType.RemoveAds)
            {
                isOwned = iapService.IsProductOwned(product.iapProductId);
            }

            if (isOwned)
            {
                if (texts.Length >= 3) texts[2].text = "✅ Owned";
                if (button != null) button.interactable = false;
            }
            else if (button != null)
            {
                var capturedProduct = product;
                button.onClick.AddListener(() => OnProductPurchaseClicked(capturedProduct));
            }

            // Badge Setup
            if (!string.IsNullOrEmpty(product.badgeText) && texts.Length >= 4)
            {
                texts[3].text = product.badgeText;
                texts[3].transform.parent.gameObject.SetActive(true); // Assuming text has a background parent
            }
        }

        // ── Purchase Flow ────────────────────────────────────────

        private void OnProductPurchaseClicked(ShopProductSO product)
        {
            var currencyManager = ServiceLocator.Get<CurrencyManager>();

            // Soft Currency Purchase Path (Gems first, then Coins)
            if (product.gemCost > 0)
            {
                if (currencyManager != null && currencyManager.SpendGems(product.gemCost))
                {
                    GrantProduct(product);
                    RefreshUI();
                    return;
                }
                Debug.Log($"[Shop] Not enough gems for {product.displayName}");
                return;
            }
            
            if (product.coinCost > 0)
            {
                if (currencyManager != null && currencyManager.SpendCoins(product.coinCost))
                {
                    GrantProduct(product);
                    RefreshUI();
                    return;
                }
                Debug.Log($"[Shop] Not enough coins for {product.displayName}");
                return;
            }

            // Real Money IAP Purchase Path
            var iapService = ServiceLocator.Get<IIAPService>();
            if (iapService == null) return;

            iapService.PurchaseProduct(product.iapProductId, (success) =>
            {
                if (success)
                {
                    GrantProduct(product);
                    RefreshUI();
                }
                else
                {
                    Debug.Log($"[Shop] Purchase failed or cancelled: {product.displayName}");
                }
            });
        }

        private void GrantProduct(ShopProductSO product)
        {
            var saveService = ServiceLocator.Get<SaveService>();
            var currencyManager = ServiceLocator.Get<CurrencyManager>();
            var cosmeticManager = ServiceLocator.Get<CosmeticManager>();

            switch (product.productType)
            {
                case ShopProductType.RemoveAds:
                    if (saveService != null)
                    {
                        saveService.Data.removeAdsPurchased = true;
                    }
                    Debug.Log("[Shop] Remove Ads purchased!");
                    break;

                case ShopProductType.CoinPack:
                    currencyManager?.AddCoins(product.coinAmount);
                    break;

                case ShopProductType.GemPack:
                    currencyManager?.AddGems(product.gemAmount);
                    break;

                case ShopProductType.StarterPack:
                case ShopProductType.CosmeticPack:
                    // Unlock cosmetics
                    if (product.includedCosmetics != null && cosmeticManager != null)
                    {
                        foreach (var cosmetic in product.includedCosmetics)
                        {
                            if (cosmetic != null) cosmeticManager.TryUnlock(cosmetic.itemId);
                        }
                    }
                    // Grant currencies
                    if (product.coinAmount > 0) currencyManager?.AddCoins(product.coinAmount);
                    if (product.gemAmount > 0) currencyManager?.AddGems(product.gemAmount);
                    break;
            }

            // Track purchase
            if (saveService != null && !saveService.Data.purchasedProductIds.Contains(product.productId))
            {
                saveService.Data.purchasedProductIds.Add(product.productId);
                saveService.MarkDirty();
                saveService.Save();
            }
            else
            {
                saveService?.MarkDirty();
                saveService?.Save();
            }
        }

        // ── Helpers ──────────────────────────────────────────────

        private string GetAmountString(ShopProductSO product)
        {
            if (product.coinAmount > 0 && product.gemAmount > 0)
                return $"{product.coinAmount:N0} Coins & {product.gemAmount:N0} Gems";
            if (product.coinAmount > 0)
                return $"{product.coinAmount:N0}";
            if (product.gemAmount > 0)
                return $"{product.gemAmount:N0}";
            
            return "Item";
        }

        private string GetPriceString(ShopProductSO product, IIAPService iapService)
        {
            if (product.gemCost > 0) return $"💎 {product.gemCost}";
            if (product.coinCost > 0) return $"🪙 {product.coinCost}";

            if (iapService != null && !string.IsNullOrEmpty(product.iapProductId))
            {
                return iapService.GetLocalizedPrice(product.iapProductId);
            }

            return "$1.99"; // Fallback preview
        }

        private void HandleCoinBalanceChanged(int newBalance)
        {
            RefreshBalances();
        }

        private void HandleGemBalanceChanged(int newBalance)
        {
            RefreshBalances();
        }
    }
}
