using UnityEngine;

namespace NeonGalaxy.Data
{
    /// <summary>
    /// Configuration for visual effects: particle prefab references,
    /// pool sizes, screen flash, and camera shake settings.
    /// Art direction: "satisfying but not visually noisy",
    /// "readable first, flashy second".
    /// Create instances via: Create → NeonGalaxy → VFX Config.
    /// </summary>
    [CreateAssetMenu(fileName = "VFXConfig", menuName = "NeonGalaxy/VFX Config", order = 20)]
    public class VFXConfigSO : ScriptableObject
    {
        [Header("Particle Prefabs")]
        [Tooltip("Particle effect for piece placement confirmation.")]
        public ParticleSystem placementVFXPrefab;

        [Tooltip("Particle effect for line clear sweep.")]
        public ParticleSystem lineClearVFXPrefab;

        [Tooltip("Particle effect for Nova Cross premium clear.")]
        public ParticleSystem novaCrossVFXPrefab;

        [Tooltip("Particle effect for combo milestone escalation.")]
        public ParticleSystem comboVFXPrefab;

        [Tooltip("Particle effect for game over board shatter.")]
        public ParticleSystem gameOverVFXPrefab;

        [Header("Pool Settings")]
        [Tooltip("Maximum number of placement VFX instances in the pool.")]
        public int placementPoolSize = 5;

        [Tooltip("Maximum number of line clear VFX instances in the pool.")]
        public int lineClearPoolSize = 8;

        [Tooltip("Maximum number of combo VFX instances in the pool.")]
        public int comboPoolSize = 3;

        [Header("Screen Flash")]
        [Tooltip("Flash color for Nova Cross events.")]
        public Color novaCrossFlashColor = new Color(0.4f, 1f, 0.95f, 0.5f);

        [Tooltip("Flash color for line clear events.")]
        public Color lineClearFlashColor = new Color(1f, 1f, 1f, 0.2f);

        [Tooltip("Duration of the screen flash in seconds.")]
        public float flashDuration = 0.12f;

        [Header("Camera Shake")]
        [Tooltip("Shake intensity for piece placement.")]
        public float placementShakeIntensity = 0.05f;

        [Tooltip("Shake intensity for Nova Cross.")]
        public float novaCrossShakeIntensity = 0.15f;

        [Tooltip("Shake intensity for game over.")]
        public float gameOverShakeIntensity = 0.25f;

        [Tooltip("Duration of camera shake in seconds.")]
        public float shakeDuration = 0.2f;

        [Header("Hit Stop")]
        [Tooltip("Duration of time-scale dip on line clear (freeze frame effect).")]
        public float hitStopDuration = 0.06f;

        [Tooltip("Time scale during hit stop (0 = full freeze, 0.1 = slow-mo).")]
        public float hitStopTimeScale = 0.05f;

        [Header("Performance")]
        [Tooltip("If true, reduces particle counts on low-end devices.")]
        public bool enableLowEndMode = false;

        [Tooltip("Particle count multiplier for low-end mode (0.0 to 1.0).")]
        [Range(0.1f, 1.0f)]
        public float lowEndParticleMultiplier = 0.5f;
    }
}
