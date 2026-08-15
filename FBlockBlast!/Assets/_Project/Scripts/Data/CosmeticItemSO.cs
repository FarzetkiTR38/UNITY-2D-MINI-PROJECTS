using UnityEngine;

namespace NeonGalaxy.Data
{
    /// <summary>
    /// Type of in-game currency used to acquire items.
    /// </summary>
    public enum CurrencyType
    {
        Coin = 0,
        Gem = 1
    }

    /// <summary>
    /// Defines a single cosmetic item (board skin, block skin, frame, or title).
    /// Create instances via: Create → NeonGalaxy → Cosmetic Item.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCosmetic", menuName = "NeonGalaxy/Cosmetic Item", order = 20)]
    public class CosmeticItemSO : ScriptableObject
    {
        [Tooltip("Unique identifier for save/load reference.")]
        public string itemId;

        [Tooltip("Display name in UI.")]
        public string displayName;

        [Tooltip("Short description for UI.")]
        [TextArea(1, 3)]
        public string description;

        [Tooltip("Category of this cosmetic.")]
        public CosmeticCategory category;

        [Tooltip("Icon shown in shop/profile.")]
        public Sprite icon;

        [Tooltip("Larger preview image for equip screen.")]
        public Sprite previewImage;

        [Tooltip("How this item is unlocked.")]
        public UnlockCondition unlockCondition;

        [Tooltip("Parameter for unlock condition (e.g., level number, or unused for IAP).")]
        public int unlockParam;

        [Tooltip("If true, only available via IAP purchase.")]
        public bool isPremium;

        [Header("Currency Pricing")]
        [Tooltip("Select whether this cosmetic is purchased using Coin or Gem.")]
        public CurrencyType costCurrencyType = CurrencyType.Coin;

        [Tooltip("Coin cost if purchased with in-game coins. 0 = not purchasable with coins.")]
        public int coinCost;

        [Tooltip("Gem cost if purchased with in-game gems. 0 = not purchasable with gems.")]
        public int gemCost;

        /// <summary>
        /// Returns the active price depending on costCurrencyType (Coin or Gem).
        /// </summary>
        public int Price => costCurrencyType == CurrencyType.Coin ? coinCost : gemCost;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(itemId))
                itemId = name;
        }
#endif
    }
}
