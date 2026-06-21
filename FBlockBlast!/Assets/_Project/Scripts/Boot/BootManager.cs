using UnityEngine;
using NeonGalaxy.Services;
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
        [Header("Status")]
        [SerializeField, Tooltip("Shows initialization progress in inspector.")]
        private string _status = "Not started";

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

            // ── Create and register services ─────────────────────

            // Save Service (must be first — other services may need save data)
            var saveService = new SaveService();
            saveService.Load();
            ServiceLocator.Register(saveService);
            Debug.Log("[BootManager] SaveService registered.");

            // Future services will be registered here in Phase 4:
            // - AuthService
            // - LeaderboardService
            // - AdService / AdPolicyService
            // - IAPService
            // - AudioService
            // - AnalyticsService
            // - ProgressionManager
            // - AchievementManager
            // - CosmeticManager

            _status = "Services initialized. Loading Home scene...";
            Debug.Log("[BootManager] All Phase 2 services initialized.");

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
