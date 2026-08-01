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
            new Color(0.39f, 0.71f, 0.96f, 1f), // #64B5F6 Mavi (weight 1-2)
            new Color(0.51f, 0.78f, 0.52f, 1f), // #81C784 Yeşil (weight 3-4)
            new Color(1.00f, 0.72f, 0.30f, 1f), // #FFB74D Turuncu (weight 5-6)
            new Color(0.73f, 0.41f, 0.78f, 1f), // #BA68C8 Mor (weight 7-8)
            new Color(0.94f, 0.38f, 0.57f, 1f), // #F06292 Pembe (weight 9-10)
        };

        [Header("Tips")]
        [SerializeField] private int _startingTips = 3;
        [SerializeField] private int _dailyLoginTipBonus = 1;
        [SerializeField] private int _adWatchTipReward = 1;

        [Header("Mob")]
        [SerializeField] private float _maxMobSpeed = 20f;
        [SerializeField] private float _minSpawnInterval = 0.4f;

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
        /// Gets the map data for the given level number (cyclic: 5 maps).
        /// </summary>
        public MapData GetMapForLevel(int level)
        {
            if (_maps == null || _maps.Length == 0) return null;
            int mapIndex = (level - 1) % _maps.Length;
            return _maps[mapIndex];
        }

        /// <summary>
        /// Gets the arrow color based on weight value.
        /// Weight 1-2 → index 0, Weight 3-4 → index 1, etc.
        /// </summary>
        public Color GetArrowColor(int weight)
        {
            if (_arrowColors == null || _arrowColors.Length == 0) return Color.white;
            int index = Mathf.Clamp((weight - 1) / 2, 0, _arrowColors.Length - 1);
            return _arrowColors[index];
        }
    }
}
