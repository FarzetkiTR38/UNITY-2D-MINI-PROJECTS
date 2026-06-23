using UnityEngine;

namespace NeonGalaxy.Utility
{
    /// <summary>
    /// Applies performance-related settings at startup.
    /// Sets target frame rate, handles GC optimization,
    /// and provides device tier detection for quality scaling.
    /// </summary>
    public class PerformanceConfig : MonoBehaviour
    {
        [Header("Frame Rate")]
        [Tooltip("Target frame rate for mobile. 60 is standard for smooth gameplay.")]
        [SerializeField] private int targetFrameRate = 60;

        [Header("GC Optimization")]
        [Tooltip("If true, performs GC collection during scene transitions.")]
#pragma warning disable 0414
        [SerializeField] private bool gcOnSceneTransitions = true;
#pragma warning restore 0414

        [Header("Device Tier Detection")]
        [SerializeField, Tooltip("Minimum system memory (MB) to be classified as high-end.")]
        private int highEndMemoryThreshold = 3072; // 3 GB

        /// <summary>
        /// Current device performance tier.
        /// </summary>
        public static DeviceTier CurrentDeviceTier { get; private set; } = DeviceTier.Mid;

        private void Awake()
        {
            // Set target frame rate
            Application.targetFrameRate = targetFrameRate;

            // Prevent screen dimming during gameplay
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // Detect device tier
            DetectDeviceTier();

            Debug.Log($"[PerformanceConfig] TargetFPS={targetFrameRate}, DeviceTier={CurrentDeviceTier}, " +
                      $"SystemMemory={SystemInfo.systemMemorySize}MB, GPU={SystemInfo.graphicsDeviceName}");
        }

        private void DetectDeviceTier()
        {
            int memoryMB = SystemInfo.systemMemorySize;

            if (memoryMB >= highEndMemoryThreshold)
            {
                CurrentDeviceTier = DeviceTier.High;
            }
            else if (memoryMB >= 2048)
            {
                CurrentDeviceTier = DeviceTier.Mid;
            }
            else
            {
                CurrentDeviceTier = DeviceTier.Low;
            }
        }

        /// <summary>
        /// Call during scene transitions to reduce GC pressure.
        /// </summary>
        public static void OnSceneTransition()
        {
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
        }

        private void OnDestroy()
        {
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }
    }

    public enum DeviceTier
    {
        Low,
        Mid,
        High
    }
}
