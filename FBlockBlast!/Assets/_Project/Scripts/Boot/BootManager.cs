using UnityEngine;
using NeonGalaxy.Services;
using NeonGalaxy.Meta;
using NeonGalaxy.Data;
using NeonGalaxy.Utility;

namespace NeonGalaxy.Boot
{
    /// <summary>
    /// Application entry point. Attached to a root GameObject in the Boot scene.
    /// Initializes all services, loads save data, and transitions to the Home scene.
    /// 
    /// This object persists across all scenes (DontDestroyOnLoad).
    /// Boot scene must be build index 0.
    /// </summary>
    public class BootManager : MonoBehaviour
    {
        [Header("Configurations")]
        [SerializeField] private ProgressionConfigSO progressionConfig;
        [SerializeField] private AdPolicyConfigSO adPolicyConfig;

        [Header("Content Registries")]
        [SerializeField] private AchievementDefinitionSO[] achievementDefinitions;
        [SerializeField] private CosmeticItemSO[] cosmeticItems;

        [Header("Status")]
        [SerializeField, Tooltip("Shows initialization progress in inspector.")]
#pragma warning disable 0414
        private string _status = "Not started";
#pragma warning restore 0414

        private void Awake()
        {
            // Prevent duplicates if Boot scene is re-entered
            if (FindObjectsByType<BootManager>(FindObjectsSortMode.None).Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void Initialize()
        {
            _status = "Initializing services...";
            Debug.Log("[BootManager] Starting initialization...");

            // ── Layer 1: Core Services ───────────────────────────

            // Save Service (must be first — other services depend on save data)
            var saveService = new SaveService();
            saveService.Load();
            ServiceLocator.Register(saveService);
            Debug.Log("[BootManager] SaveService registered.");

            // ── Layer 2: Monetization Services ───────────────────

            // Ad Service (mock for now — replace with real SDK wrapper later)
            IAdService adService = new MockAdService();
            ServiceLocator.Register(adService);
            Debug.Log("[BootManager] IAdService (Mock) registered.");

            // IAP Service (mock for now — replace with Unity IAP wrapper later)
            IIAPService iapService = new MockIAPService();
            ServiceLocator.Register(iapService);
            Debug.Log("[BootManager] IIAPService (Mock) registered.");

            // ── Layer 3: Meta Managers ───────────────────────────

            // Progression Manager
            var progressionManager = new ProgressionManager(saveService, progressionConfig);
            ServiceLocator.Register(progressionManager);
            Debug.Log("[BootManager] ProgressionManager registered.");

            // Achievement Manager
            var achievementManager = new AchievementManager(saveService, achievementDefinitions);
            ServiceLocator.Register(achievementManager);
            Debug.Log("[BootManager] AchievementManager registered.");

            // Cosmetic Manager
            var cosmeticManager = new CosmeticManager(saveService, cosmeticItems);
            ServiceLocator.Register(cosmeticManager);
            Debug.Log("[BootManager] CosmeticManager registered.");

            // Currency Manager
            var currencyManager = new CurrencyManager(saveService);
            ServiceLocator.Register(currencyManager);
            Debug.Log("[BootManager] CurrencyManager registered.");

            // Ad Policy Manager
            var adPolicyManager = new AdPolicyManager(saveService, adService, adPolicyConfig);
            ServiceLocator.Register(adPolicyManager);
            Debug.Log("[BootManager] AdPolicyManager registered.");

            // ── Layer 4: Online Services ─────────────────────────

            // Leaderboard Service (mock for now — replace with UGS implementation later)
            ILeaderboardService leaderboardService = new MockLeaderboardService(saveService);
            ServiceLocator.Register(leaderboardService);
            Debug.Log("[BootManager] ILeaderboardService (Mock) registered.");

            // Async authentication (fire-and-forget, UI handles state)
            _ = leaderboardService.AuthenticateAsync();

            // Flush any pending score submissions from previous sessions
            _ = leaderboardService.FlushPendingSubmissionsAsync();

            // ── Layer 5: Analytics ───────────────────────────────

            IAnalyticsService analyticsService = new MockAnalyticsService();
            ServiceLocator.Register(analyticsService);
            AnalyticsEvents.Initialize(analyticsService);
            AnalyticsEvents.SessionStart();
            Debug.Log("[BootManager] IAnalyticsService (Mock) registered.");

            _status = "All services initialized. Loading Home scene...";
            Debug.Log("[BootManager] All services initialized (Phase 4 + Phase 5).");

            // ── Load Home scene ──────────────────────────────────
            LoadHomeScene();
        }

        private void LoadHomeScene()
        {
            SceneLoader.LoadScene(Constants.SCENE_HOME, () =>
            {
                _status = "Home scene loaded.";
                Debug.Log("[BootManager] Home scene loaded successfully.");
            });
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // Auto-save when app is backgrounded
                var saveService = ServiceLocator.Get<SaveService>();
                saveService?.SaveIfDirty();
            }
        }

        private void OnApplicationQuit()
        {
            // Final save on quit
            var saveService = ServiceLocator.Get<SaveService>();
            saveService?.SaveIfDirty();

            // Cleanup
            ServiceLocator.Clear();
        }
    }
}
