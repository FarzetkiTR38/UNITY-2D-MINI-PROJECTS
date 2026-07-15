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

        [Header("Cell Burst")]
        [Tooltip("Number of particles per cell burst during line clear.")]
        public int cellBurstParticleCount = 15;

        [Tooltip("Speed of cell burst particles.")]
        public float cellBurstSpeed = 3f;

        [Tooltip("Lifetime of cell burst particles.")]
        public float cellBurstLifetime = 0.4f;

        [Tooltip("Pool size for cell burst effects.")]
        public int cellBurstPoolSize = 20;

        [Header("Sweep Line")]
        [Tooltip("Duration of the sweep line traveling across a row/column.")]
        public float sweepLineDuration = 0.3f;

        [Tooltip("Color tint of the sweep line particles.")]
        public Color sweepLineColor = new Color(1f, 1f, 1f, 0.8f);

        [Tooltip("Pool size for sweep line effects.")]
        public int sweepLinePoolSize = 4;

        [Header("Board Clear (Mega)")]
        [Tooltip("Particle effect prefab for board clear supernova. If null, generated procedurally.")]
        public ParticleSystem boardClearVFXPrefab;

        [Tooltip("Total particle count for the board clear supernova.")]
        public int boardClearParticleCount = 120;

        [Tooltip("Duration of the board clear celebration.")]
        public float boardClearDuration = 1.5f;

        [Tooltip("Maximum size of the shockwave ring.")]
        public float boardClearShockwaveSize = 12f;

        [Tooltip("Screen flash color for board clear.")]
        public Color boardClearFlashColor = new Color(0.5f, 0.8f, 1f, 0.6f);

        [Tooltip("Camera shake intensity for board clear.")]
        public float boardClearShakeIntensity = 0.3f;

        [Header("Procedural Fallback")]
        [Tooltip("If true, generates particle systems at runtime when prefab slots are empty.")]
        public bool useProceduralFallback = true;

        private void OnEnable()
        {
            // Initialize defaults for existing assets where these fields were added later
            if (cellBurstParticleCount == 0) cellBurstParticleCount = 15;
            if (cellBurstSpeed == 0f) cellBurstSpeed = 3f;
            if (cellBurstLifetime == 0f) cellBurstLifetime = 0.4f;
            if (cellBurstPoolSize == 0) cellBurstPoolSize = 20;

            if (sweepLineDuration == 0f) sweepLineDuration = 0.3f;
            if (sweepLineColor == Color.clear) sweepLineColor = new Color(1f, 1f, 1f, 0.8f);
            if (sweepLinePoolSize == 0) sweepLinePoolSize = 4;

            if (boardClearParticleCount == 0) boardClearParticleCount = 120;
            if (boardClearDuration == 0f) boardClearDuration = 1.5f;
            if (boardClearShockwaveSize == 0f) boardClearShockwaveSize = 12f;
            if (boardClearFlashColor == Color.clear) boardClearFlashColor = new Color(0.5f, 0.8f, 1f, 0.6f);
            if (boardClearShakeIntensity == 0f) boardClearShakeIntensity = 0.3f;

            // Force fallback if pool sizes were 0 (indicates old version of asset)
            if (cellBurstPoolSize == 20) useProceduralFallback = true;
        }
    }
}
