namespace ArrowSwarm.Core
{
    using UnityEngine;

    /// <summary>
    /// Central configuration ScriptableObject that holds all tunable game parameters.
    /// Assigned via Inspector — no magic numbers in code.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "ArrowSwarm/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Lives")]
        [SerializeField] private int _maxLives = 3;

        [Header("Arrow")]
        [SerializeField] private float _arrowMoveSpeed = 15f;
        [SerializeField] private Color[] _arrowColors = new Color[]
        {
            new Color(0.25f, 0.76f, 0.79f, 1f), // #3FC1C9 Turkuaz/Cyan
            new Color(0.65f, 0.46f, 0.86f, 1f), // #A675DB Mor/Purple
            new Color(1.00f, 0.33f, 0.46f, 1f), // #FF5376 Pembe/Kırmızı
            new Color(1.00f, 0.64f, 0.11f, 1f), // #FFA41B Turuncu/Amber
            new Color(0.23f, 0.51f, 0.96f, 1f), // #3B82F6 Mavi/Royal Blue
        };

        [Header("Tips")]
        [SerializeField] private int _startingTips = 3;
        [SerializeField] private int _dailyLoginTipBonus = 1;
        [SerializeField] private int _adWatchTipReward = 1;

        [Header("Mob")]
        [SerializeField] private float _maxMobSpeed = 20f;
        [SerializeField] private float _minSpawnInterval = 1.6f;
        [SerializeField] private float _targetTransitSeconds = 25.0f;
        [SerializeField] private float _baseMobScale = 1.0f;
        [SerializeField] private float _gapCloseSpeedMultiplier = 5.0f;
        [SerializeField] private float _mobSpacingMultiplier = 1.18f;

        [Header("Camera")]
        [SerializeField] private float _minZoom = 1f;
        [SerializeField] private float _maxZoom = 3f;

        [Header("Timing")]
        [SerializeField] private float _mobSpawnDelay = 2.5f;

        [Header("Solvability")]
        [SerializeField] private int _maxRegenerateAttempts = 100;
        [SerializeField] private float _winabilityRatio = 0.6f;
        [SerializeField] private float _difficultyReductionOnFail = 0.1f;

        [Header("Rainbow Arrow")]
        [SerializeField] private int _rainbowArrowDamage = 999;

        [Header("Maps")]
        [SerializeField] private MapData[] _maps;

        // --- Properties ---
        /// <summary>Maximum number of lives per level.</summary>
        public int MaxLives => _maxLives;

        /// <summary>Arrow movement speed in units/second.</summary>
        public float ArrowMoveSpeed => _arrowMoveSpeed;

        /// <summary>Arrow color array indexed by weight bracket.</summary>
        public Color[] ArrowColors => _arrowColors;

        /// <summary>Number of tips the player starts with.</summary>
        public int StartingTips => _startingTips;

        /// <summary>Tips granted per daily login.</summary>
        public int DailyLoginTipBonus => _dailyLoginTipBonus;

        /// <summary>Tips granted per ad watch.</summary>
        public int AdWatchTipReward => _adWatchTipReward;

        /// <summary>Maximum mob movement speed (clamp ceiling).</summary>
        public float MaxMobSpeed => _maxMobSpeed;

        /// <summary>Minimum spawn interval in seconds (clamp floor).</summary>
        public float MinSpawnInterval => _minSpawnInterval;

        /// <summary>Target seconds for a mob to traverse the entire perimeter.</summary>
        public float TargetTransitSeconds => _targetTransitSeconds;

        /// <summary>Speed multiplier applied when closing a gap.</summary>
        public float GapCloseSpeedMultiplier => _gapCloseSpeedMultiplier;

        /// <summary>Multiplier for spacing between adjacent mobs.</summary>
        public float MobSpacingMultiplier => _mobSpacingMultiplier;

        /// <summary>Base scale factor for mobs on Map 1.</summary>
        public float BaseMobScale => _baseMobScale;

        /// <summary>Minimum camera zoom (fit all).</summary>
        public float MinZoom => _minZoom;

        /// <summary>Maximum camera zoom (3x close-up).</summary>
        public float MaxZoom => _maxZoom;

        /// <summary>Seconds to wait after level start before spawning mobs.</summary>
        public float MobSpawnDelay => _mobSpawnDelay;

        /// <summary>Max attempts to regenerate a solvable level.</summary>
        public int MaxRegenerateAttempts => _maxRegenerateAttempts;

        /// <summary>Minimum ratio of total arrow damage to total mob HP.</summary>
        public float WinabilityRatio => _winabilityRatio;

        /// <summary>Fraction to reduce difficulty by when generation fails.</summary>
        public float DifficultyReductionOnFail => _difficultyReductionOnFail;

        /// <summary>Damage dealt by the rainbow (last) arrow.</summary>
        public int RainbowArrowDamage => _rainbowArrowDamage;

        /// <summary>Array of map data assets (5 maps, cyclic).</summary>
        public MapData[] Maps => _maps;

        /// <summary>
        /// Gets the map data for the given level number using DifficultyCalculator.
        /// </summary>
        public MapData GetMapForLevel(int level)
        {
            if (_maps == null || _maps.Length == 0) return null;
            int mapIndex = DifficultyCalculator.GetMapIndex(level);
            if (mapIndex >= 0 && mapIndex < _maps.Length)
            {
                return _maps[mapIndex];
            }
            return _maps[Mathf.Abs(mapIndex) % _maps.Length];
        }

        /// <summary>
        /// Gets a random arrow color from the color palette (independent of weight).
        /// </summary>
        public Color GetRandomArrowColor()
        {
            if (_arrowColors == null || _arrowColors.Length == 0) return Color.white;
            return _arrowColors[Random.Range(0, _arrowColors.Length)];
        }

        /// <summary>
        /// Legacy fallback for weight-based color.
        /// </summary>
        public Color GetArrowColor(int weight)
        {
            return GetRandomArrowColor();
        }
    }
}
