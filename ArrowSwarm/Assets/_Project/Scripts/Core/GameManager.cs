namespace ArrowSwarm.Core
{
    using System;
    using ArrowSwarm.Utils;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Central game state machine that manages game flow,
    /// coordinates managers, and handles scene transitions.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private GameConfig _gameConfig;

        private GameState _currentState = GameState.Loading;
        private int _currentLives;

        /// <summary>Current game configuration asset.</summary>
        public GameConfig Config
        {
            get
            {
                if (_gameConfig == null)
                {
#if UNITY_EDITOR
                    string[] guids = UnityEditor.AssetDatabase.FindAssets("t:GameConfig");
                    if (guids.Length > 0)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                        _gameConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<GameConfig>(path);
                    }
#endif
                }
                return _gameConfig;
            }
        }

        /// <summary>Current game state.</summary>
        public GameState CurrentState => _currentState;

        /// <summary>Current remaining lives.</summary>
        public int CurrentLives => _currentLives;

        // --- Events ---
        /// <summary>Fired when the game state changes.</summary>
        public static event Action<GameState> OnGameStateChanged;

        /// <summary>Fired when lives change (new lives count).</summary>
        public static event Action<int> OnLivesChanged;

        /// <summary>Fired when the level is won.</summary>
        public static event Action OnLevelWon;

        /// <summary>Fired when the level is lost.</summary>
        public static event Action OnLevelLost;

        /// <summary>Fired when an arrow is fired successfully.</summary>
        public static event Action OnArrowFired;

        /// <summary>Fired when a wrong click occurs.</summary>
        public static event Action OnWrongClick;

        /// <summary>Fired when a mob reaches the finish point.</summary>
        public static event Action OnMobReachedFinish;

        protected override void OnSingletonAwake()
        {
            if (_gameConfig == null)
            {
#if UNITY_EDITOR
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:GameConfig");
                if (guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    _gameConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<GameConfig>(path);
                    LogDebug("GameConfig automatically assigned via Editor fallback.");
                }
#endif
                if (_gameConfig == null)
                {
                    Debug.LogError("[ArrowSwarm] GameManager: GameConfig is not assigned!");
                }
            }
            
            // Initialize global managers
            var inputMgr = InputManager.Instance;
            var hapticMgr = HapticManager.Instance;
            var touchEffectMgr = ArrowSwarm.Effects.TouchEffectManager.Instance;
        }

        /// <summary>
        /// Changes the game state and notifies all listeners.
        /// </summary>
        public void SetState(GameState newState)
        {
            if (_currentState == newState) return;

            GameState previousState = _currentState;
            _currentState = newState;
            LogDebug($"State changed: {previousState} → {newState}");
            OnGameStateChanged?.Invoke(_currentState);
        }

        /// <summary>
        /// Initializes lives for a new level.
        /// </summary>
        public void InitializeLives()
        {
            _currentLives = _gameConfig != null ? _gameConfig.MaxLives : 3;
            OnLivesChanged?.Invoke(_currentLives);
        }

        /// <summary>
        /// Called when an arrow is fired successfully.
        /// </summary>
        public void HandleArrowFired()
        {
            OnArrowFired?.Invoke();
        }

        /// <summary>
        /// Called when the player makes a wrong click (blocked arrow).
        /// Reduces lives by 1 and checks for game over.
        /// </summary>
        public void HandleWrongClick()
        {
            LoseLife();
            OnWrongClick?.Invoke();
        }

        /// <summary>
        /// Called when a mob reaches the finish point.
        /// Reduces lives by 1 and checks for game over.
        /// </summary>
        public void HandleMobReachedFinish()
        {
            LoseLife();
            OnMobReachedFinish?.Invoke();
        }

        /// <summary>
        /// Called when all arrows have been fired successfully.
        /// Triggers win state.
        /// </summary>
        public void HandleAllArrowsFired()
        {
            SetState(GameState.Win);
            OnLevelWon?.Invoke();
            LogDebug("Level WON!");
        }

        /// <summary>
        /// Loads the game scene to start playing with smooth transition.
        /// </summary>
        public void StartGame()
        {
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene("GameScene");
            }
            else
            {
                SceneManager.LoadScene("GameScene");
            }
        }

        /// <summary>
        /// Returns to the main menu scene with smooth transition.
        /// </summary>
        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SetState(GameState.Menu);
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene("MainMenuScene");
            }
            else
            {
                SceneManager.LoadScene("MainMenuScene");
            }
        }

        /// <summary>
        /// Restarts the current level with smooth transition.
        /// </summary>
        public void RestartLevel()
        {
            Time.timeScale = 1f;
            SetState(GameState.Loading);
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene("GameScene");
            }
            else
            {
                SceneManager.LoadScene("GameScene");
            }
        }

        /// <summary>
        /// Pauses the game.
        /// </summary>
        public void PauseGame()
        {
            if (_currentState != GameState.Playing) return;
            Time.timeScale = 0f;
            SetState(GameState.Paused);
        }

        /// <summary>
        /// Resumes the game from pause.
        /// </summary>
        public void ResumeGame()
        {
            if (_currentState != GameState.Paused) return;
            Time.timeScale = 1f;
            InputManager.Instance?.BlockInput(0.35f);
            SetState(GameState.Playing);
        }

        private void LoseLife()
        {
            if (_currentState != GameState.Playing) return;

            _currentLives--;
            _currentLives = Mathf.Max(0, _currentLives);
            OnLivesChanged?.Invoke(_currentLives);
            LogDebug($"Life lost! Remaining: {_currentLives}");

            if (_currentLives <= 0)
            {
                SetState(GameState.Lose);
                OnLevelLost?.Invoke();
                LogDebug("Level LOST!");
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] GameManager: {message}");
        }
    }

    /// <summary>
    /// Represents all possible game states.
    /// </summary>
    public enum GameState
    {
        /// <summary>Level is being generated/loaded.</summary>
        Loading,

        /// <summary>Player is in the main menu.</summary>
        Menu,

        /// <summary>Level is actively being played.</summary>
        Playing,

        /// <summary>Game is paused.</summary>
        Paused,

        /// <summary>Player won the level.</summary>
        Win,

        /// <summary>Player lost the level.</summary>
        Lose
    }
}
