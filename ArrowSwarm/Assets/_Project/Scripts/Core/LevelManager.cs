namespace ArrowSwarm.Core
{
    using System;
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Data;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Mob;
    using ArrowSwarm.Path;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Manages the level lifecycle: loading, generating, starting, and ending levels.
    /// Coordinates all game systems (Grid, Arrow, Path, Mob).
    /// </summary>
    public class LevelManager : Singleton<LevelManager>
    {
        private LevelGenerator.LevelData _currentLevelData;
        private LevelParams _currentParams;
        private int _arrowsFired;
        private int _totalArrows;

        /// <summary>Current level data.</summary>
        public LevelGenerator.LevelData CurrentLevelData => _currentLevelData;

        /// <summary>Current level parameters.</summary>
        public LevelParams CurrentParams => _currentParams;

        /// <summary>Number of arrows fired so far.</summary>
        public int ArrowsFired => _arrowsFired;

        /// <summary>Total arrows in this level.</summary>
        public int TotalArrows => _totalArrows;

        // --- Events ---
        /// <summary>Fired when a level starts loading.</summary>
        public static event Action<int> OnLevelLoading;

        /// <summary>Fired when a level is ready to play.</summary>
        public static event Action<LevelParams> OnLevelReady;

        /// <summary>Fired when arrow count changes (fired, total).</summary>
        public static event Action<int, int> OnArrowCountChanged;

        private void Start()
        {
            // Auto-load level when GameScene starts
            LoadLevel();
        }

        /// <summary>
        /// Called when GameScene loads. Generates and starts the current level.
        /// </summary>
        public void LoadLevel()
        {
            int level = DataManager.Instance?.PlayerData?.currentLevel ?? 1;
            LoadLevel(level);
        }

        /// <summary>
        /// Generates and starts a specific level.
        /// </summary>
        public void LoadLevel(int level)
        {
            GameConfig config = GameManager.Instance?.Config;
            if (config == null)
            {
                Debug.LogError("[ArrowSwarm] LevelManager: GameConfig not found!");
                return;
            }

            OnLevelLoading?.Invoke(level);
            GameManager.Instance.SetState(GameState.Loading);

            // Generate level
            _currentLevelData = LevelGenerator.Generate(level, config);
            if (!_currentLevelData.IsValid)
            {
                Debug.LogError($"[ArrowSwarm] LevelManager: Failed to generate level {level}!");
                return;
            }

            _currentParams = _currentLevelData.Params;
            _arrowsFired = 0;
            _totalArrows = _currentLevelData.ArrowPlacements.Count;

            // Initialize systems
            InitializeSystems();

            // Subscribe to events
            Arrow.OnArrowFiredEvent += HandleArrowFired;
            ArrowSpawner.OnAllArrowsFired += HandleAllArrowsFired;

            // Start playing
            GameManager.Instance.InitializeLives();
            GameManager.Instance.SetState(GameState.Playing);

            OnLevelReady?.Invoke(_currentParams);
            OnArrowCountChanged?.Invoke(_arrowsFired, _totalArrows);

            LogDebug($"Level {level} loaded and started. {_currentParams}");
        }

        /// <summary>
        /// Advances to the next level.
        /// </summary>
        public void NextLevel()
        {
            CleanupLevel();

            int nextLevel = (DataManager.Instance?.PlayerData?.currentLevel ?? 1) + 1;
            DataManager.Instance?.SetCurrentLevel(nextLevel);

            GameManager.Instance.StartGame();
        }

        /// <summary>
        /// Retries the current level.
        /// </summary>
        public void RetryLevel()
        {
            CleanupLevel();
            GameManager.Instance.RestartLevel();
        }

        /// <summary>
        /// Cleans up the current level before loading a new one.
        /// </summary>
        public void CleanupLevel()
        {
            // Unsubscribe
            Arrow.OnArrowFiredEvent -= HandleArrowFired;
            ArrowSpawner.OnAllArrowsFired -= HandleAllArrowsFired;

            // Clean up systems
            ArrowSpawner.Instance?.ClearAllArrows();
            MobSpawner.Instance?.StopSpawning();
            MobSpawner.Instance?.ClearAllMobs();
            GridManager.Instance?.ClearGrid();
        }

        private void InitializeSystems()
        {
            MapData map = _currentLevelData.Map;

            // Initialize Grid
            GridManager.Instance.InitializeGrid(map);

            // Initialize Path
            PathManager.Instance.InitializePath(map);

            // Initialize Pools
            ArrowSpawner.Instance.InitializePool();
            MobSpawner.Instance.InitializePool();

            // Spawn Arrows
            ArrowSpawner.Instance.SpawnArrows(_currentParams, map);

            // Start Mob Spawning
            MobSpawner.Instance.StartSpawning(_currentParams);
        }

        private void HandleArrowFired(Arrow arrow)
        {
            _arrowsFired++;
            OnArrowCountChanged?.Invoke(_arrowsFired, _totalArrows);
        }

        private void HandleAllArrowsFired()
        {
            // Destroy remaining mobs (rainbow arrow effect)
            MobSpawner.Instance?.DestroyAllMobs();
            MobSpawner.Instance?.StopSpawning();
        }

        protected override void OnDestroy()
        {
            CleanupLevel();
            base.OnDestroy();
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] LevelManager: {message}");
        }
    }
}
