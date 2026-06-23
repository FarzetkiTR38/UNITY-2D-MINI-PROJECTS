using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonGalaxy.Boot;
using NeonGalaxy.Services;
using NeonGalaxy.Meta;
using NeonGalaxy.Utility;
using NeonGalaxy.Core;

namespace NeonGalaxy.UI
{
    /// <summary>
    /// Controls the Home Screen — the main menu that greets the player
    /// after boot. Displays profile info, navigates to Gameplay,
    /// and opens sub-screens (Leaderboard, Settings, Shop).
    /// </summary>
    public class HomeScreenController : MonoBehaviour
    {
        [Header("Profile")]
        [SerializeField] private ProfileDisplayWidget profileWidget;

        [Header("Currency")]
        [SerializeField] private TextMeshProUGUI coinText;

        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button leaderboardButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button shopButton;

        [Header("Popup Panels")]
        [SerializeField] private GameObject leaderboardPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject shopPanel;

        private void Start()
        {
            SetupButtons();
            RefreshUI();
        }

        private void OnEnable()
        {
            GameEvents.OnCoinBalanceChanged += HandleCoinBalanceChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnCoinBalanceChanged -= HandleCoinBalanceChanged;
        }

        // ── UI Refresh ───────────────────────────────────────────

        private void RefreshUI()
        {
            // Profile
            if (profileWidget != null)
                profileWidget.Refresh();

            // Coins
            RefreshCoinDisplay();
        }

        private void RefreshCoinDisplay()
        {
            var currencyManager = ServiceLocator.Get<CurrencyManager>();
            if (coinText != null && currencyManager != null)
            {
                coinText.text = currencyManager.GetBalance().ToString("N0");
            }
        }

        // ── Button Setup ─────────────────────────────────────────

        private void SetupButtons()
        {
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);

            if (leaderboardButton != null)
                leaderboardButton.onClick.AddListener(OnLeaderboardClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (shopButton != null)
                shopButton.onClick.AddListener(OnShopClicked);
        }

        // ── Button Handlers ──────────────────────────────────────

        private void OnPlayClicked()
        {
            Debug.Log("[HomeScreen] Play button clicked — loading Gameplay scene.");
            SceneLoader.LoadScene(Constants.SCENE_GAMEPLAY);
        }

        private void OnLeaderboardClicked()
        {
            Debug.Log("[HomeScreen] Leaderboard button clicked.");
            CloseAllPanels();
            if (leaderboardPanel != null)
                leaderboardPanel.SetActive(true);
        }

        private void OnSettingsClicked()
        {
            Debug.Log("[HomeScreen] Settings button clicked.");
            CloseAllPanels();
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
        }

        private void OnShopClicked()
        {
            Debug.Log("[HomeScreen] Shop button clicked.");
            CloseAllPanels();
            if (shopPanel != null)
                shopPanel.SetActive(true);
        }

        private void CloseAllPanels()
        {
            if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (shopPanel != null) shopPanel.SetActive(false);
        }

        // ── Event Handlers ───────────────────────────────────────

        private void HandleCoinBalanceChanged(int newBalance)
        {
            if (coinText != null)
                coinText.text = newBalance.ToString("N0");
        }
    }
}
