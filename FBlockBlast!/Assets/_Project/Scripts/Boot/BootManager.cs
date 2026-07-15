using UnityEngine;
using UnityEngine.SceneManagement;
using NeonGalaxy.Services;
using NeonGalaxy.Meta;
using NeonGalaxy.Data;
using NeonGalaxy.Utility;
using Unity.Services.Core;

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
        [SerializeField] private ProfileAvatarRegistrySO avatarRegistry;

        [Header("Loading Screen")]
        [SerializeField, Tooltip("Reference to the LoadingScreenController in Boot scene.")]
        private LoadingScreenController loadingScreen;

        [SerializeField, Tooltip("Minimum time (seconds) the loading screen is shown, even if init is faster.")]
        private float minLoadingDuration = 2.5f;

        [SerializeField, Tooltip("Delay (seconds) after bar reaches 100% before fade-out begins.")]
        private float postCompleteDelay = 0.5f;

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
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            float startTime = Time.realtimeSinceStartup;

            // ── Step 0: Start ────────────────────────────────────
            ReportProgress(0f, "Starting...");
            await Awaitable.NextFrameAsync();

            // ── Step 1: UGS Initialize ───────────────────────────
            _status = "Initializing UGS...";
            Debug.Log("[BootManager] Starting UGS initialization...");
            
            try 
            {
                await UnityServices.InitializeAsync();
                Debug.Log("[BootManager] Unity Gaming Services Initialized.");
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }

            ReportProgress(0.15f, "UGS initialized.");
            await Awaitable.NextFrameAsync();

            // ── Step 2: Core Services ────────────────────────────
            _status = "Loading save data...";
            Debug.Log("[BootManager] Starting initialization...");

            // Save Service (must be first — other services depend on save data)
            var saveService = new SaveService();
            saveService.Load();
            ServiceLocator.Register(saveService);
            Debug.Log("[BootManager] SaveService registered.");

            ReportProgress(0.25f, "Save data loaded.");
            await Awaitable.NextFrameAsync();

            // ── Step 3: Monetization Services ────────────────────

            // Ad Service (Real Unity Ads)
            IAdService adService = new UnityAdService();
            ServiceLocator.Register(adService);
            Debug.Log("[BootManager] IAdService (UnityAds) registered.");

            // IAP Service (Real Unity Purchasing)
            IIAPService iapService = new UnityIAPService();
            ServiceLocator.Register(iapService);
            Debug.Log("[BootManager] IIAPService (UnityIAP) registered.");

            ReportProgress(0.40f, "Monetization ready.");
            await Awaitable.NextFrameAsync();

            // ── Step 4: Meta Managers ────────────────────────────

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

            ReportProgress(0.60f, "Meta systems ready.");
            await Awaitable.NextFrameAsync();

            // ── Step 5: Auth & Cloud & Profile ───────────────────

            // Profile Manager
            var authService = new UGSAuthService();
            await authService.SignInAnonymouslyAsync();
            ServiceLocator.Register<IAuthService>(authService);
            Debug.Log("[BootManager] UGSAuthService registered.");

            ICloudSaveService cloudSaveService = new UGSCloudSaveService();
            ServiceLocator.Register(cloudSaveService);
            Debug.Log("[BootManager] UGSCloudSaveService registered.");

            var profileManager = new ProfileManager(saveService, authService, cloudSaveService, avatarRegistry);
            profileManager.InitializeGuestProfile();
            ServiceLocator.Register(profileManager);
            Debug.Log("[BootManager] ProfileManager registered.");

            ReportProgress(0.80f, "Profile ready.");
            await Awaitable.NextFrameAsync();

            // ── Step 6: Online Services & Analytics ──────────────

            // Leaderboard Service (real UGS implementation)
            ILeaderboardService leaderboardService = new UGSLeaderboardService(saveService);
            ServiceLocator.Register(leaderboardService);
            Debug.Log("[BootManager] ILeaderboardService (UGS) registered.");

            // Async authentication (fire-and-forget, UI handles state)
            _ = leaderboardService.AuthenticateAsync();

            // Flush any pending score submissions from previous sessions
            _ = leaderboardService.FlushPendingSubmissionsAsync();

            IAnalyticsService analyticsService = new MockAnalyticsService();
            ServiceLocator.Register(analyticsService);
            AnalyticsEvents.Initialize(analyticsService);
            AnalyticsEvents.SessionStart();
            Debug.Log("[BootManager] IAnalyticsService (Mock) registered.");

            ReportProgress(0.95f, "Online services ready.");
            await Awaitable.NextFrameAsync();

            // ── Minimum Time Guarantee ───────────────────────────
            float elapsedSeconds = Time.realtimeSinceStartup - startTime;
            if (elapsedSeconds < minLoadingDuration)
            {
                float remainingSec = minLoadingDuration - elapsedSeconds;
                Debug.Log($"[BootManager] Init took {elapsedSeconds:F2}s. Waiting {remainingSec:F2}s for minimum loading duration.");
                await Awaitable.WaitForSecondsAsync(remainingSec);
            }

            // ── Step 7: Complete ─────────────────────────────────
            ReportProgress(1.0f, "All services initialized.");
            _status = "All services initialized. Loading Home scene...";
            Debug.Log("[BootManager] All services initialized (Phase 4 + Phase 5).");

            // Wait for bar animation to visually reach 100%
            await WaitForBarAnimation();

            // Small pause at 100% before fade-out
            await Awaitable.WaitForSecondsAsync(postCompleteDelay);

            // ── Transition to Home ───────────────────────────────
            TransitionToHome();
        }

        /// <summary>
        /// Reports progress to the loading screen (if assigned).
        /// </summary>
        private void ReportProgress(float progress, string debugLabel)
        {
            Debug.Log($"[BootManager] ReportProgress({progress:F2}, \"{debugLabel}\") — loadingScreen={(loadingScreen != null ? "ASSIGNED" : "NULL")}");

            if (loadingScreen != null)
                loadingScreen.SetProgress(progress);
            else
                Debug.LogWarning("[BootManager] loadingScreen is NULL! Assign LoadingScreenController in Inspector.");

            _status = debugLabel;
        }

        /// <summary>
        /// Waits until the loading bar's visual animation catches up to 100%.
        /// Uses Unity Awaitable to guarantee main thread execution.
        /// </summary>
        private async Awaitable WaitForBarAnimation()
        {
            if (loadingScreen == null) return;

            while (!loadingScreen.HasReachedTarget())
            {
                await Awaitable.NextFrameAsync();
            }
        }

        /// <summary>
        /// Seamless transition: preloads the Home scene, activates it behind the
        /// loading screen (which persists via DontDestroyOnLoad), waits for the scene
        /// to render, then fades out the loading screen to reveal the Home scene.
        /// No blank/blue screen gap is possible.
        /// </summary>
        private async void TransitionToHome()
        {
            // Step 1: Start loading Home scene in background (don't activate yet)
            var loadOp = SceneManager.LoadSceneAsync(Constants.SCENE_HOME);
            if (loadOp == null)
            {
                Debug.LogError($"[BootManager] Failed to start loading scene '{Constants.SCENE_HOME}'. Is it in Build Settings?");
                return;
            }
            loadOp.allowSceneActivation = false;
            Debug.Log("[BootManager] Home scene preload started.");

            // Step 2: Wait until scene is fully loaded in memory (0.9 = ready, waiting for activation)
            while (loadOp.progress < 0.9f)
            {
                await Awaitable.NextFrameAsync();
            }
            Debug.Log("[BootManager] Home scene preloaded and ready for activation.");

            // Step 3: Make loading screen survive the scene change
            if (loadingScreen != null)
                DontDestroyOnLoad(loadingScreen.gameObject);

            // Step 4: Activate the scene — loading screen covers everything on top
            loadOp.allowSceneActivation = true;

            // Wait for scene to fully finish loading
            while (!loadOp.isDone)
            {
                await Awaitable.NextFrameAsync();
            }

            // Wait 2 extra frames so the Home scene camera renders behind the loading screen
            await Awaitable.NextFrameAsync();
            await Awaitable.NextFrameAsync();

            _status = "Home scene loaded.";
            Debug.Log("[BootManager] Home scene active and rendered. Starting fade-out.");

            // Step 5: Now fade out loading screen to reveal the already-rendered Home scene
            if (loadingScreen != null)
            {
                loadingScreen.FadeOutAndComplete(null);
                // FadeOutCoroutine will Destroy the loading screen canvas after fade completes
            }
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
