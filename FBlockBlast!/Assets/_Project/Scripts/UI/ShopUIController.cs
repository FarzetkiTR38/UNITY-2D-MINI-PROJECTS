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
        [SerializeField] private ShopProductSO[] currencyProducts;
        [SerializeField] private ShopProductSO[] blockSkinProducts;

        [Header("UI Elements")]
        [SerializeField] private Transform currencyProductListParent;
        [SerializeField] private Transform blockSkinProductListParent;
        [SerializeField] private GameObject productCardPrefab;
        [UnityEngine.Serialization.FormerlySerializedAs("coinBalanceText")]
        [SerializeField] private TextMeshProUGUI coinText;
        [UnityEngine.Serialization.FormerlySerializedAs("gemBalanceText")]
        [SerializeField] private TextMeshProUGUI gemText;

        [Header("Categories")]
        [SerializeField] private GameObject goldGemPanel;
        [SerializeField] private GameObject blockSkinsPanel;
        [SerializeField] private Button goldGemTabButton;
        [SerializeField] private Button blockSkinsTabButton;

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

            if (goldGemTabButton != null)
            {
                goldGemTabButton.onClick.AddListener(ShowGoldGemCategory);
            }
            if (blockSkinsTabButton != null)
            {
                blockSkinsTabButton.onClick.AddListener(ShowBlockSkinsCategory);
            }
        }

        private void OnEnable()
        {
            ShowGoldGemCategory();
            RefreshUI();
            GameEvents.OnCoinBalanceChanged += HandleCoinBalanceChanged;
            GameEvents.OnGemBalanceChanged += HandleGemBalanceChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnCoinBalanceChanged -= HandleCoinBalanceChanged;
            GameEvents.OnGemBalanceChanged -= HandleGemBalanceChanged;
        }

        // ── Category Navigation ──────────────────────────────────
        
        public void ShowGoldGemCategory()
        {
            if (goldGemPanel != null) goldGemPanel.SetActive(true);
            if (blockSkinsPanel != null) blockSkinsPanel.SetActive(false);
            
            if (goldGemTabButton != null) goldGemTabButton.interactable = false;
            if (blockSkinsTabButton != null) blockSkinsTabButton.interactable = true;
        }

        public void ShowBlockSkinsCategory()
        {
            if (goldGemPanel != null) goldGemPanel.SetActive(false);
            if (blockSkinsPanel != null) blockSkinsPanel.SetActive(true);
            
            if (goldGemTabButton != null) goldGemTabButton.interactable = true;
            if (blockSkinsTabButton != null) blockSkinsTabButton.interactable = false;
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
            var iapService = ServiceLocator.Get<IIAPService>();

            // Setup Limited Offer Price if available
            if (limitedOfferProduct != null && limitedOfferPriceText != null)
            {
                limitedOfferPriceText.text = GetPriceString(limitedOfferProduct, iapService);
            }

            // Populate Currency Products
            if (currencyProductListParent != null && productCardPrefab != null && currencyProducts != null)
            {
                foreach (Transform child in currencyProductListParent) Destroy(child.gameObject);
                
                var sortedCurrency = new List<ShopProductSO>(currencyProducts);
                sortedCurrency.Sort((a, b) => a.sortOrder.CompareTo(b.sortOrder));
                
                foreach (var product in sortedCurrency)
                {
                    if (product != null && product.productType != ShopProductType.StarterPack)
                    {
                        CreateProductCard(product, iapService, currencyProductListParent);
                    }
                }
            }

            // Populate Block Skin Products
            if (blockSkinProductListParent != null && productCardPrefab != null && blockSkinProducts != null)
            {
                foreach (Transform child in blockSkinProductListParent) Destroy(child.gameObject);
                
                var sortedSkins = new List<ShopProductSO>(blockSkinProducts);
                sortedSkins.Sort((a, b) => a.sortOrder.CompareTo(b.sortOrder));
                
                foreach (var product in sortedSkins)
                {
                    if (product != null && product.productType != ShopProductType.StarterPack)
                    {
                        CreateProductCard(product, iapService, blockSkinProductListParent);
                    }
                }
            }
        }

        private void CreateProductCard(ShopProductSO product, IIAPService iapService, Transform parent)
        {
            var cardGO = Instantiate(productCardPrefab, parent);
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
            var cosmeticManager = ServiceLocator.Get<CosmeticManager>();
            var saveService = ServiceLocator.Get<SaveService>();
            bool inPurchasedList = saveService != null && saveService.Data.purchasedProductIds.Contains(product.productId);

            if (product.productType == ShopProductType.RemoveAds)
            {
                isOwned = (iapService != null && iapService.IsProductOwned(product.iapProductId)) || inPurchasedList;
            }
            else if (product.productType == ShopProductType.CosmeticPack || product.productType == ShopProductType.StarterPack)
            {
                if (product.productType == ShopProductType.CosmeticPack && product.includedCosmetics != null && product.includedCosmetics.Length > 0)
                {
                    isOwned = cosmeticManager != null && cosmeticManager.IsUnlocked(product.includedCosmetics[0].itemId);
                }
                else
                {
                    isOwned = inPurchasedList;
                }
            }

            if (isOwned)
            {
                if (product.productType == ShopProductType.CosmeticPack && product.includedCosmetics != null && product.includedCosmetics.Length > 0)
                {
                    string cosmeticId = product.includedCosmetics[0].itemId;
                    var category = product.includedCosmetics[0].category;
                    bool isEquipped = cosmeticManager != null && cosmeticManager.GetEquipped(category) == cosmeticId;

                    if (isEquipped)
                    {
                        if (texts.Length >= 3) texts[2].text = "Selected";
                        if (button != null) button.interactable = false;
                    }
                    else
                    {
                        if (texts.Length >= 3) texts[2].text = "Equip";
                        if (button != null)
                        {
                            button.interactable = true;
                            button.onClick.AddListener(() =>
                            {
                                cosmeticManager?.Equip(category, cosmeticId);
                                
                                // Refresh game pieces if playing
                                var gameManager = FindObjectOfType<GameManager>();
                                if (gameManager != null)
                                {
                                    gameManager.ApplyEquippedSkin();
                                }

                                RefreshUI();
                            });
                        }
                    }
                }
                else
                {
                    if (texts.Length >= 3) texts[2].text = "Owned";
                    if (button != null) button.interactable = false;
                }
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
            if (product.gemCost > 0) return $"{product.gemCost} GEM";
            if (product.coinCost > 0) return $"{product.coinCost} GOLD";

            if (iapService != null && !string.IsNullOrEmpty(product.iapProductId))
            {
                string localizedPrice = iapService.GetLocalizedPrice(product.iapProductId);
                // Eğer fiyatın içinde ₺ işareti (veya bilinmeyen sembol) varsa onu silip sonuna TL ekler
                return localizedPrice.Replace("₺", "").Trim() + " TL";
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
