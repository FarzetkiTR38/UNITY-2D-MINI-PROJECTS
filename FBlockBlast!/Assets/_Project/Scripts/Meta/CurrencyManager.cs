using UnityEngine;
using NeonGalaxy.Data;
using NeonGalaxy.Services;
using NeonGalaxy.Core;

namespace NeonGalaxy.Meta
{
    /// <summary>
    /// Manages the player's coin balance.
    /// All coin operations go through this manager to ensure
    /// consistent save-state and event firing.
    /// 
    /// Registered in ServiceLocator at boot time.
    /// </summary>
    public class CurrencyManager
    {
        private readonly SaveService _saveService;

        public CurrencyManager(SaveService saveService)
        {
            _saveService = saveService;
        }

        // ── Public API ───────────────────────────────────────────

        /// <summary>
        /// Returns the current coin balance.
        /// </summary>
        public int GetBalance() => _saveService.Data.coins;

        /// <summary>
        /// Adds coins to the player's balance.
        /// </summary>
        public void AddCoins(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[CurrencyManager] AddCoins called with non-positive amount: {amount}");
                return;
            }

            _saveService.Data.coins += amount;
            _saveService.MarkDirty();
            _saveService.Save();

            GameEvents.InvokeCoinBalanceChanged(_saveService.Data.coins);
            Debug.Log($"[CurrencyManager] Added {amount} coins. New balance: {_saveService.Data.coins}");
        }

        /// <summary>
        /// Attempts to spend coins. Returns true if the player had enough.
        /// </summary>
        public bool SpendCoins(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[CurrencyManager] SpendCoins called with non-positive amount: {amount}");
                return false;
            }

            if (_saveService.Data.coins < amount)
            {
                Debug.Log($"[CurrencyManager] Insufficient coins. Need {amount}, have {_saveService.Data.coins}");
                return false;
            }

            _saveService.Data.coins -= amount;
            _saveService.MarkDirty();
            _saveService.Save();

            GameEvents.InvokeCoinBalanceChanged(_saveService.Data.coins);
            Debug.Log($"[CurrencyManager] Spent {amount} coins. New balance: {_saveService.Data.coins}");
            return true;
        }

        /// <summary>
        /// Returns true if the player can afford the given amount.
        /// </summary>
        public bool CanAfford(int amount) => _saveService.Data.coins >= amount;
    }
}
