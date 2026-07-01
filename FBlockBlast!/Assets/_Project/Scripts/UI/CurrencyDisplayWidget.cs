using UnityEngine;
using TMPro;
using NeonGalaxy.Core;
using NeonGalaxy.Services;
using NeonGalaxy.Meta;
using NeonGalaxy.Boot;

namespace NeonGalaxy.UI
{
    /// <summary>
    /// Attach this to any TextMeshProUGUI object in your scene or prefab to automatically
    /// display and update the player's coin or gem balance independently.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class CurrencyDisplayWidget : MonoBehaviour
    {
        public enum CurrencyType { Coin, Gem }
        
        [Header("Settings")]
        [Tooltip("Which currency should this text display?")]
        public CurrencyType currencyType = CurrencyType.Coin;
        
        [Tooltip("Optional prefix, like '🪙 ' or '💎 '")]
        public string prefix = ""; 

        private TextMeshProUGUI _text;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            Refresh();
        }

        private void OnEnable()
        {
            // Subscribe to the correct event
            if (currencyType == CurrencyType.Coin)
                GameEvents.OnCoinBalanceChanged += HandleBalanceChanged;
            else
                GameEvents.OnGemBalanceChanged += HandleBalanceChanged;
                
            Refresh();
        }

        private void OnDisable()
        {
            // Unsubscribe
            if (currencyType == CurrencyType.Coin)
                GameEvents.OnCoinBalanceChanged -= HandleBalanceChanged;
            else
                GameEvents.OnGemBalanceChanged -= HandleBalanceChanged;
        }

        private void Refresh()
        {
            if (ServiceLocator.Has<CurrencyManager>())
            {
                var currencyManager = ServiceLocator.Get<CurrencyManager>();
                int balance = currencyType == CurrencyType.Coin ? currencyManager.GetBalance() : currencyManager.GetGemBalance();
                HandleBalanceChanged(balance);
            }
        }

        private void HandleBalanceChanged(int newBalance)
        {
            if (_text != null)
            {
                _text.text = prefix + newBalance.ToString("N0");
            }
        }
    }
}
