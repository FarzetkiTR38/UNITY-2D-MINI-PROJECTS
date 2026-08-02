namespace ArrowSwarm.Debug
{
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Debug and testing tools for development.
    /// Allows level jumping, speed control, infinite lives/tips,
    /// and displays debug information in the Inspector.
    /// </summary>
    public class DebugManager : Singleton<DebugManager>
    {
        [Header("Level Override")]
        [SerializeField] private bool _useDebugLevel;
        [SerializeField] private int _debugLevel = 1;

        [Header("Quick Jump Levels")]
        [SerializeField] private int[] _quickJumpLevels = { 1, 10, 50, 100, 250, 500, 1000 };

        [Header("Cheats")]
        [SerializeField] private bool _infiniteLives;
        [SerializeField] private bool _infiniteTips;
        [SerializeField] private bool _showArrowDirections;
        [SerializeField] private float _gameSpeedMultiplier = 1f;

        [Header("Debug Info (Read Only)")]
        [SerializeField] private int _currentLevel;
        [SerializeField] private int _currentTier;
        [SerializeField] private int _arrowCount;
        [SerializeField] private int _mobCount;
        [SerializeField] private string _currentState;
        [SerializeField] private bool _showDebugInfo;

        /// <summary>Whether debug level override is active.</summary>
        public bool UseDebugLevel => _useDebugLevel;

        /// <summary>The debug override level number.</summary>
        public int DebugLevel => _debugLevel;

        /// <summary>Whether infinite lives cheat is active.</summary>
        public bool InfiniteLives => _infiniteLives;

        /// <summary>Whether infinite tips cheat is active.</summary>
        public bool InfiniteTips => _infiniteTips;

        /// <summary>Whether arrow direction indicators are shown.</summary>
        public bool ShowArrowDirections => _showArrowDirections;

        private void OnEnable()
        {
            GameManager.OnGameStateChanged += HandleStateChanged;
            LevelManager.OnLevelReady += HandleLevelReady;
            GameManager.OnLivesChanged += HandleLivesChanged;
        }

        private void OnDisable()
        {
            GameManager.OnGameStateChanged -= HandleStateChanged;
            LevelManager.OnLevelReady -= HandleLevelReady;
            GameManager.OnLivesChanged -= HandleLivesChanged;
        }

        private void Update()
        {
            // Apply game speed multiplier
            if (Time.timeScale != 0f) // Don't override pause
            {
                Time.timeScale = _gameSpeedMultiplier;
            }

            // Infinite lives cheat
            if (_infiniteLives && GameManager.Instance != null &&
                GameManager.Instance.CurrentLives < GameManager.Instance.Config?.MaxLives)
            {
                GameManager.Instance.InitializeLives();
            }
        }

        /// <summary>
        /// Jumps to the specified level (editor only).
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void JumpToLevel(int level)
        {
            DataManager.Instance?.SetCurrentLevel(level);
            GameManager.Instance?.RestartLevel();
            LogDebug($"Jumped to level {level}");
        }

        /// <summary>
        /// Gets the level to use (debug override or real).
        /// </summary>
        public int GetEffectiveLevel()
        {
            if (_useDebugLevel) return _debugLevel;
            return DataManager.Instance?.PlayerData?.currentLevel ?? 1;
        }

        /// <summary>
        /// Logs a formatted difficulty report for the current level.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void LogDifficultyReport(int level)
        {
            var config = GameManager.Instance?.Config;
            if (config == null) return;

            var map = config.GetMapForLevel(level);
            if (map == null) return;

            var p = DifficultyCalculator.CalculateAll(
                level, map.GridWidth, map.GridHeight,
                config.MaxMobSpeed, config.MinSpawnInterval);

            UnityEngine.Debug.Log(
                $"[ArrowSwarm] Level {level} generated: " +
                $"Map={map.MapName}, {p}");
        }

        private void HandleStateChanged(GameState state)
        {
            _currentState = state.ToString();
        }

        private void HandleLevelReady(LevelParams p)
        {
            _currentLevel = p.Level;
            _currentTier = p.DifficultyTier;
            _arrowCount = p.ArrowCount;
            _mobCount = p.TotalMobs;
        }

        private void HandleLivesChanged(int lives)
        {
            // Debug info update
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            UnityEngine.Debug.Log($"[ArrowSwarm] DebugManager: {message}");
        }
    }
}
