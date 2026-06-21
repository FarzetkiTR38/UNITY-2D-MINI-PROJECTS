using System.Collections.Generic;
using UnityEngine;
using NeonGalaxy.Data;
using NeonGalaxy.Services;
using NeonGalaxy.Core;

namespace NeonGalaxy.Meta
{
    /// <summary>
    /// Manages cosmetic item unlocks, equipping, and availability checks.
    /// Listens to level-up events to auto-unlock level-gated cosmetics.
    /// 
    /// Registered in ServiceLocator at boot time.
    /// </summary>
    public class CosmeticManager
    {
        private readonly SaveService _saveService;
        private readonly CosmeticItemSO[] _allCosmetics;
        private readonly Dictionary<string, CosmeticItemSO> _cosmeticLookup;

        public CosmeticManager(SaveService saveService, CosmeticItemSO[] allCosmetics)
        {
            _saveService = saveService;
            _allCosmetics = allCosmetics;

            // Build fast lookup dictionary
            _cosmeticLookup = new Dictionary<string, CosmeticItemSO>();
            foreach (var item in _allCosmetics)
            {
                if (item != null && !string.IsNullOrEmpty(item.itemId))
                {
                    _cosmeticLookup[item.itemId] = item;
                }
            }

            // Auto-unlock all "Default" condition items
            EnsureDefaultItemsUnlocked();

            // Subscribe to level-up events for level-gated unlocks
            GameEvents.OnLevelUp += HandleLevelUp;
        }

        ~CosmeticManager()
        {
            GameEvents.OnLevelUp -= HandleLevelUp;
        }

        // ── Public API ───────────────────────────────────────────

        /// <summary>
        /// Returns true if the cosmetic with the given ID is unlocked.
        /// </summary>
        public bool IsUnlocked(string itemId)
        {
            return _saveService.Data.unlockedCosmeticIds.Contains(itemId);
        }

        /// <summary>
        /// Attempts to unlock a cosmetic. Returns true if newly unlocked.
        /// </summary>
        public bool TryUnlock(string itemId)
        {
            if (IsUnlocked(itemId)) return false;
            if (!_cosmeticLookup.ContainsKey(itemId)) return false;

            _saveService.Data.unlockedCosmeticIds.Add(itemId);
            _saveService.MarkDirty();

            Debug.Log($"[CosmeticManager] Unlocked cosmetic: {itemId}");
            return true;
        }

        /// <summary>
        /// Equips a cosmetic of the given category. Must be unlocked first.
        /// </summary>
        public bool Equip(CosmeticCategory category, string itemId)
        {
            if (!IsUnlocked(itemId))
            {
                Debug.LogWarning($"[CosmeticManager] Cannot equip locked cosmetic: {itemId}");
                return false;
            }

            var data = _saveService.Data;
            switch (category)
            {
                case CosmeticCategory.BoardSkin:
                    data.equippedBoardSkin = itemId;
                    break;
                case CosmeticCategory.BlockSkin:
                    data.equippedBlockSkin = itemId;
                    break;
                case CosmeticCategory.ProfileFrame:
                    data.equippedFrame = itemId;
                    break;
                case CosmeticCategory.PlayerTitle:
                    data.equippedTitle = itemId;
                    break;
            }

            _saveService.MarkDirty();
            Debug.Log($"[CosmeticManager] Equipped {category}: {itemId}");
            return true;
        }

        /// <summary>
        /// Returns the currently equipped item ID for the given category.
        /// </summary>
        public string GetEquipped(CosmeticCategory category)
        {
            var data = _saveService.Data;
            return category switch
            {
                CosmeticCategory.BoardSkin    => data.equippedBoardSkin,
                CosmeticCategory.BlockSkin    => data.equippedBlockSkin,
                CosmeticCategory.ProfileFrame => data.equippedFrame,
                CosmeticCategory.PlayerTitle  => data.equippedTitle,
                _ => "default"
            };
        }

        /// <summary>
        /// Returns the SO for a given cosmetic ID, or null.
        /// </summary>
        public CosmeticItemSO GetCosmeticItem(string itemId)
        {
            _cosmeticLookup.TryGetValue(itemId, out var item);
            return item;
        }

        /// <summary>
        /// Returns all cosmetics of a given category.
        /// </summary>
        public List<CosmeticItemSO> GetCosmeticsByCategory(CosmeticCategory category)
        {
            var result = new List<CosmeticItemSO>();
            foreach (var item in _allCosmetics)
            {
                if (item != null && item.category == category)
                    result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// Returns all cosmetic definitions.
        /// </summary>
        public CosmeticItemSO[] GetAllCosmetics() => _allCosmetics;

        // ── Internal ─────────────────────────────────────────────

        private void EnsureDefaultItemsUnlocked()
        {
            foreach (var item in _allCosmetics)
            {
                if (item != null && item.unlockCondition == UnlockCondition.Default)
                {
                    if (!IsUnlocked(item.itemId))
                    {
                        _saveService.Data.unlockedCosmeticIds.Add(item.itemId);
                    }
                }
            }
            _saveService.MarkDirty();
        }

        private void HandleLevelUp(int newLevel)
        {
            foreach (var item in _allCosmetics)
            {
                if (item == null) continue;
                if (item.unlockCondition != UnlockCondition.Level) continue;
                if (item.unlockParam > newLevel) continue;
                if (IsUnlocked(item.itemId)) continue;

                TryUnlock(item.itemId);
                Debug.Log($"[CosmeticManager] Auto-unlocked at level {newLevel}: {item.displayName}");
            }

            _saveService.SaveIfDirty();
        }
    }
}
