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
        [SerializeField] private TextMeshProUGUI gemText;

        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button leaderboardButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button creditsButton;

        [Header("Popup Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject leaderboardPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private GameObject creditsPanel;

        private void Start()
        {
            SetupButtons();
            RefreshUI();
            
            // Default to showing only main panel
            OnBackClicked();
        }

        private void OnEnable()
        {
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
            // Profile
            if (profileWidget != null)
                profileWidget.Refresh();

            // Coins
            RefreshCoinDisplay();
        }

        private void RefreshCoinDisplay()
        {
            var currencyManager = ServiceLocator.Get<CurrencyManager>();
            if (currencyManager != null)
            {
                if (coinText != null)
                    coinText.text = currencyManager.GetBalance().ToString("N0");
                
                if (gemText != null)
                    gemText.text = currencyManager.GetGemBalance().ToString("N0");
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

            if (creditsButton != null)
                creditsButton.onClick.AddListener(OnCreditsClicked);
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
            OpenPanel(leaderboardPanel);
        }

        private void OnSettingsClicked()
        {
            Debug.Log("[HomeScreen] Settings button clicked.");
            OpenPanel(settingsPanel);
        }

        private void OnShopClicked()
        {
            Debug.Log("[HomeScreen] Shop button clicked.");
            OpenPanel(shopPanel);
        }

        private void OnCreditsClicked()
        {
            Debug.Log("[HomeScreen] Credits button clicked.");
            OpenPanel(creditsPanel);
        }

        private void OpenPanel(GameObject panel)
        {
            CloseAllPanels();
            if (mainPanel != null) mainPanel.SetActive(false);
            if (panel != null) panel.SetActive(true);
        }

        public void OnBackClicked()
        {
            Debug.Log("[HomeScreen] Back button clicked.");
            CloseAllPanels();
            if (mainPanel != null) mainPanel.SetActive(true);
        }

        private void CloseAllPanels()
        {
            if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (shopPanel != null) shopPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);
        }

        // ── Event Handlers ───────────────────────────────────────

        private void HandleCoinBalanceChanged(int newBalance)
        {
            if (coinText != null)
                coinText.text = newBalance.ToString("N0");
        }

        private void HandleGemBalanceChanged(int newBalance)
        {
            if (gemText != null)
                gemText.text = newBalance.ToString("N0");
        }
    }
}
