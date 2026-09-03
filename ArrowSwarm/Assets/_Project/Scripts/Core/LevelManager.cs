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
    using UnityEngine.SceneManagement;

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

        private bool _isLevelLoaded;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (IsPlayableGameScene(scene.name))
            {
                StartCoroutine(DeferredLoadLevel());
            }
        }

        private System.Collections.IEnumerator DeferredLoadLevel()
        {
            // Wait 1 frame so all scene components finish Awake, OnEnable, and Start
            yield return null;
            if (IsPlayableGameScene(SceneManager.GetActiveScene().name))
            {
                LoadLevel();
            }
        }

        private void Start()
        {
            StartCoroutine(DeferredLoadLevel());
        }

        private bool IsPlayableGameScene(string sceneName)
        {
            return sceneName == "GameScene"
                || sceneName == "MapTestScene"
                || sceneName.StartsWith("Map");
        }

        /// <summary>
        /// Called when GameScene loads. Generates and starts the current level.
        /// </summary>
        public void LoadLevel()
        {
            var mapCtrl = UnityEngine.Object.FindFirstObjectByType<MapSceneController>();
            if (mapCtrl != null)
            {
                LoadLevel(mapCtrl.DefaultLevel);
                return;
            }

            int level;
            if (ArrowSwarm.Debug.DebugManager.Instance != null && ArrowSwarm.Debug.DebugManager.Instance.UseDebugLevel)
            {
                level = ArrowSwarm.Debug.DebugManager.Instance.GetEffectiveLevel();
            }
            else
            {
                level = DataManager.Instance?.PlayerData?.currentLevel ?? 1;
            }
            LoadLevel(level);
        }

        /// <summary>
        /// Generates and starts a specific level.
        /// </summary>
        public void LoadLevel(int level)
        {
            _isLevelLoaded = true;
            CleanupLevel();
            
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
            InputManager.Instance?.BlockInput(0.4f);
            GameManager.Instance.SetState(GameState.Playing);

            // Handle Tutorial activation if TutorialManager / Tutorial_Root is in the scene (even if inactive)
            var tutorial = UnityEngine.Object.FindFirstObjectByType<Tutorial.TutorialManager>(FindObjectsInactive.Include);
            if (tutorial != null)
            {
                var mapCtrl = UnityEngine.Object.FindFirstObjectByType<MapSceneController>();
                bool isMapTestTutorial = mapCtrl != null && mapCtrl.EnableTutorialTest;
                bool isTutorial = isMapTestTutorial || (level <= 1 && (DataManager.Instance == null || !DataManager.Instance.IsTutorialCompleted));

                if (isTutorial)
                {
                    if (!tutorial.gameObject.activeSelf)
                    {
                        tutorial.gameObject.SetActive(true);
                    }
                    tutorial.StartTutorial();
                }
                else
                {
                    if (tutorial.gameObject.activeSelf)
                    {
                        tutorial.EndTutorialSilently();
                    }
                }
            }

            OnLevelReady?.Invoke(_currentParams);
            OnArrowCountChanged?.Invoke(_arrowsFired, _totalArrows);

            LogDebug($"Level {level} loaded and started. {_currentParams}");
        }

        /// <summary>
        /// Advances to the highest unlocked level.
        /// </summary>
        public void NextLevel()
        {
            CleanupLevel();

            int nextLevel = DataManager.Instance?.PlayerData?.highestLevel ?? 1;
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

            // Clean up systems safely
            if (ArrowSpawner.HasInstance) ArrowSpawner.Instance.ClearAllArrows();
            if (MobSpawner.HasInstance)
            {
                MobSpawner.Instance.StopSpawning();
                MobSpawner.Instance.ClearAllMobs();
            }
            if (PathManager.HasInstance) PathManager.Instance.ClearPath();
            if (GridManager.HasInstance) GridManager.Instance.ClearGrid();
        }

        private void InitializeSystems()
        {
            MapData map = _currentLevelData.Map;

            // Initialize Grid (point-based)
            GridManager.Instance.InitializeGrid(map);

            // Initialize Path
            PathManager.Instance.InitializePath(map);

            // Fit Camera to Map
            ArrowSwarm.Camera.CameraController.Instance?.FitToMap(map);

            // Initialize Pools
            ArrowSpawner.Instance.InitializePool();
            MobSpawner.Instance.InitializePool();

            // Spawn Arrows with pre-generated multi-point placements
            ArrowSpawner.Instance.SpawnArrows(_currentLevelData.ArrowPlacements, map);

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
