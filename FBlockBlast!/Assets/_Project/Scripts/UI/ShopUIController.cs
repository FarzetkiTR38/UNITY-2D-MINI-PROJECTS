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
    /// Displays purchasable products organized by category tabs,
    /// handles IAP purchases and coin purchases.
    /// </summary>
    public class ShopUIController : MonoBehaviour
    {
        [Header("Product Registry")]
        [SerializeField] private ShopProductSO[] products;

        [Header("UI Elements")]
        [SerializeField] private Transform productListParent;
        [SerializeField] private GameObject productCardPrefab;
        [SerializeField] private TextMeshProUGUI coinBalanceText;

        [Header("Category Tabs")]
        [SerializeField] private Button tabRemoveAds;
        [SerializeField] private Button tabCoinPacks;
        [SerializeField] private Button tabCosmetics;

        [Header("Navigation")]
        [SerializeField] private Button closeButton;

        public event System.Action OnCloseClicked;

        private ShopProductType _activeTab = ShopProductType.CoinPack;

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

            SetupTabs();
        }

        private void OnEnable()
        {
            RefreshUI();
            GameEvents.OnCoinBalanceChanged += HandleCoinBalanceChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnCoinBalanceChanged -= HandleCoinBalanceChanged;
        }

        // ── Tab Setup ────────────────────────────────────────────

        private void SetupTabs()
        {
            if (tabRemoveAds != null)
                tabRemoveAds.onClick.AddListener(() => ShowTab(ShopProductType.RemoveAds));

            if (tabCoinPacks != null)
                tabCoinPacks.onClick.AddListener(() => ShowTab(ShopProductType.CoinPack));

            if (tabCosmetics != null)
                tabCosmetics.onClick.AddListener(() => ShowTab(ShopProductType.CosmeticPack));
        }

        private void ShowTab(ShopProductType type)
        {
            _activeTab = type;
            PopulateProductList();
        }

        // ── UI Refresh ───────────────────────────────────────────

        private void RefreshUI()
        {
            RefreshCoinBalance();
            PopulateProductList();
        }

        private void RefreshCoinBalance()
        {
            var currencyManager = ServiceLocator.Get<CurrencyManager>();
            if (coinBalanceText != null && currencyManager != null)
            {
                coinBalanceText.text = $"🪙 {currencyManager.GetBalance():N0}";
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

            // Filter and sort products
            var filtered = GetProductsByType(_activeTab);
            filtered.Sort((a, b) => a.sortOrder.CompareTo(b.sortOrder));

            var iapService = ServiceLocator.Get<IIAPService>();

            foreach (var product in filtered)
            {
                CreateProductCard(product, iapService);
            }
        }

        private void CreateProductCard(ShopProductSO product, IIAPService iapService)
        {
            var cardGO = Instantiate(productCardPrefab, productListParent);
            cardGO.SetActive(true);

            // Find text components
            var texts = cardGO.GetComponentsInChildren<TextMeshProUGUI>();
            var button = cardGO.GetComponentInChildren<Button>();

            if (texts.Length >= 3)
            {
                texts[0].text = product.displayName;
                texts[1].text = product.description;

                // Price display
                string priceStr = GetPriceString(product, iapService);
                texts[2].text = priceStr;
            }

            // Check if already owned (non-consumable)
            bool isOwned = false;
            if (iapService != null && product.productType == ShopProductType.RemoveAds)
            {
                isOwned = iapService.IsProductOwned(product.iapProductId);
            }

            if (isOwned)
            {
                if (texts.Length >= 3)
                    texts[2].text = "✅ Purchased";

                if (button != null)
                    button.interactable = false;
            }
            else if (button != null)
            {
                // Capture for closure
                var capturedProduct = product;
                button.onClick.AddListener(() => OnProductPurchaseClicked(capturedProduct));
            }

            // Badge
            if (!string.IsNullOrEmpty(product.badgeText) && texts.Length >= 4)
            {
                texts[3].text = product.badgeText;
                texts[3].gameObject.SetActive(true);
            }
        }

        // ── Purchase Flow ────────────────────────────────────────

        private void OnProductPurchaseClicked(ShopProductSO product)
        {
            // Coin purchase path
            if (product.coinCost > 0 && product.productType == ShopProductType.CosmeticPack)
            {
                var currencyManager = ServiceLocator.Get<CurrencyManager>();
                if (currencyManager != null && currencyManager.SpendCoins(product.coinCost))
                {
                    GrantProduct(product);
                    RefreshUI();
                    return;
                }
                else
                {
                    Debug.Log($"[Shop] Not enough coins for {product.displayName}");
                    return;
                }
            }

            // IAP purchase path
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
                        saveService.MarkDirty();
                        saveService.Save();
                    }
                    Debug.Log("[Shop] Remove Ads purchased!");
                    break;

                case ShopProductType.CoinPack:
                    currencyManager?.AddCoins(product.coinAmount);
                    Debug.Log($"[Shop] Granted {product.coinAmount} coins.");
                    break;

                case ShopProductType.CosmeticPack:
                case ShopProductType.StarterPack:
                    // Unlock included cosmetics
                    if (product.includedCosmetics != null && cosmeticManager != null)
                    {
                        foreach (var cosmetic in product.includedCosmetics)
                        {
                            if (cosmetic != null)
                                cosmeticManager.TryUnlock(cosmetic.itemId);
                        }
                    }

                    // Grant coins if starter pack includes them
                    if (product.coinAmount > 0)
                        currencyManager?.AddCoins(product.coinAmount);

                    saveService?.Save();
                    Debug.Log($"[Shop] Pack granted: {product.displayName}");
                    break;
            }

            // Track purchase
            if (saveService != null && !saveService.Data.purchasedProductIds.Contains(product.productId))
            {
                saveService.Data.purchasedProductIds.Add(product.productId);
                saveService.MarkDirty();
                saveService.Save();
            }
        }

        // ── Helpers ──────────────────────────────────────────────

        private List<ShopProductSO> GetProductsByType(ShopProductType type)
        {
            var result = new List<ShopProductSO>();
            if (products == null) return result;

            foreach (var p in products)
            {
                if (p != null && p.productType == type)
                    result.Add(p);
            }

            // Also include StarterPack in RemoveAds tab
            if (type == ShopProductType.RemoveAds)
            {
                foreach (var p in products)
                {
                    if (p != null && p.productType == ShopProductType.StarterPack)
                        result.Add(p);
                }
            }

            return result;
        }

        private string GetPriceString(ShopProductSO product, IIAPService iapService)
        {
            if (product.coinCost > 0 && product.productType == ShopProductType.CosmeticPack)
            {
                return $"🪙 {product.coinCost}";
            }

            if (iapService != null)
            {
                return iapService.GetLocalizedPrice(product.iapProductId);
            }

            return "—";
        }

        private void HandleCoinBalanceChanged(int newBalance)
        {
            RefreshCoinBalance();
        }
    }
}
