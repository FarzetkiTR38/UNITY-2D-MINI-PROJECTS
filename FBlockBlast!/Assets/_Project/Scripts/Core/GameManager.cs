using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeonGalaxy.Data;
using NeonGalaxy.Input;
using NeonGalaxy.UI;
using NeonGalaxy.Services;
using NeonGalaxy.Meta;
using NeonGalaxy.Generation;
using NeonGalaxy.Utility;

namespace NeonGalaxy.Core
{
    /// <summary>
    /// Central gameplay coordinator. Manages the core GameState machine, 
    /// instantiates logical managers, handles pause/resume transitions, 
    /// score popups, and updates save profiles.
    /// </summary>
    [RequireComponent(typeof(PlacementResolver))]
    public class GameManager : MonoBehaviour
    {
        [Header("Configurations")]
        [SerializeField] private BoardConfigSO boardConfig;
        [SerializeField] private PiecePoolSO piecePool;
        [SerializeField] private ScoreConfigSO scoreConfig;
        [SerializeField] private ComboConfigSO comboConfig;

        [Header("Scene Controller References")]
        [SerializeField] private BoardController boardController;
        [SerializeField] private PieceTrayController pieceTrayController;
        [SerializeField] private TouchInputController touchInputController;
        [SerializeField] private GhostPreviewController ghostPreviewController;

        [Header("UI Controller References")]
        [SerializeField] private GameplayHUDController hudController;
        [SerializeField] private GameOverPopupController gameOverPopup;
        [SerializeField] private ResultsScreenController resultsScreen;
        [SerializeField] private PausePopupController pausePopup;

        [Header("Prefabs")]
        [SerializeField] private ScorePopupView scorePopupPrefab;

        private BoardModel _boardModel;
        private ComboManager _comboManager;
        private ScoreManager _scoreManager;
        private IBatchGenerator _batchGenerator;
        private PlacementResolver _placementResolver;
        private GameOverDetector _gameOverDetector;
        
        private GameState _currentState;
        private GameState _stateBeforePause;
        private SaveService _saveService;
        private int _runLinesCleared;
        private int _runBestCombo;
        private int _revivesUsedThisRun;

        public GameState CurrentState => _currentState;

        private void Awake()
        {
            _placementResolver = GetComponent<PlacementResolver>();
        }

        private void Start()
        {
            InitializeSaveService();
            InitializeCoreGameplay();
            StartGame();
        }

        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks across scene updates
            if (touchInputController != null)
            {
                touchInputController.OnPieceDropped -= HandlePieceDropped;
            }
            GameEvents.OnScorePopupRequested -= HandleScorePopupRequested;

            if (gameOverPopup != null)
            {
                gameOverPopup.OnRetryClicked -= HandleRetryGame;
                gameOverPopup.OnHomeClicked -= HandleQuitToHome;
            }

            if (resultsScreen != null)
            {
                resultsScreen.OnRetryClicked -= HandleRetryGame;
                resultsScreen.OnHomeClicked -= HandleQuitToHome;
            }

            if (pausePopup != null)
            {
                pausePopup.OnResumeClicked -= ResumeGame;
                pausePopup.OnQuitClicked -= HandleQuitToHome;
            }
        }

        private void InitializeSaveService()
        {
            // Resolve SaveService from boot ServiceLocator, or instantiate a standalone fallback
            if (Boot.ServiceLocator.Has<SaveService>())
            {
                _saveService = Boot.ServiceLocator.Get<SaveService>();
            }
            else
            {
                _saveService = new SaveService();
                _saveService.Load();
            }
        }

        private void InitializeCoreGameplay()
        {
            // Create data instances
            _boardModel = new BoardModel(boardConfig);
            _comboManager = new ComboManager(comboConfig);
            _scoreManager = new ScoreManager(scoreConfig, _comboManager);
            _batchGenerator = new ComboFriendlyBatchGenerator();
            _gameOverDetector = new GameOverDetector();

            // Initialize rendering boards
            boardController.Initialize(boardConfig);
            touchInputController.Initialize(_boardModel, ghostPreviewController);

            // Connect event listeners
            touchInputController.OnPieceDropped += HandlePieceDropped;
            GameEvents.OnScorePopupRequested += HandleScorePopupRequested;

            if (gameOverPopup != null)
            {
                gameOverPopup.OnRetryClicked += HandleRetryGame;
                gameOverPopup.OnHomeClicked += HandleQuitToHome;
            }

            if (resultsScreen != null)
            {
                resultsScreen.OnRetryClicked += HandleRetryGame;
                resultsScreen.OnHomeClicked += HandleQuitToHome;
            }

            if (pausePopup != null)
            {
                pausePopup.OnResumeClicked -= ResumeGame;
                pausePopup.OnResumeClicked += ResumeGame;
                pausePopup.OnQuitClicked += HandleQuitToHome;
            }
        }

        /// <summary>
        /// Resets scoring, combos, board states, and spawns the initial batch.
        /// </summary>
        public void StartGame()
        {
            Time.timeScale = 1f;

            _boardModel.Reset();
            _comboManager.Reset();
            _scoreManager.Reset();
            _runLinesCleared = 0;
            _runBestCombo = 0;
            _revivesUsedThisRun = 0;

            boardController.RefreshBoard(_boardModel);
            pieceTrayController.ClearTray();

            if (hudController != null)
            {
                hudController.UpdateBestScore(GetHighScore());
                // Force HUD reset
                GameEvents.InvokeScoreChanged(0);
                GameEvents.InvokeComboUpdated(0);
            }

            if (gameOverPopup != null) gameOverPopup.Hide();
            if (resultsScreen != null) resultsScreen.Hide();
            if (pausePopup != null) pausePopup.Hide();

            // Register total run count statistics
            if (_saveService != null)
            {
                _saveService.IncrementTotalRuns();
                _saveService.Save();
            }

            TransitionState(GameState.WaitingForBatch);
        }

        /// <summary>
        /// Switches the gameplay state machine to a new phase.
        /// </summary>
        public void TransitionState(GameState newState)
        {
            _currentState = newState;
            GameEvents.InvokeGameStateChanged(newState);

            switch (_currentState)
            {
                case GameState.WaitingForBatch:
                    GenerateAndSetNewBatch();
                    break;

                case GameState.PieceSelection:
                    touchInputController.IsInputEnabled = true;
                    pieceTrayController.SetInteractable(true);
                    break;

                case GameState.PieceDragging:
                    // Handled by TouchInputController / DragDropHandler
                    break;

                case GameState.ValidPlacement:
                    // Disable inputs while placement animations resolve
                    touchInputController.IsInputEnabled = false;
                    pieceTrayController.SetInteractable(false);
                    break;

                case GameState.ClearAnimation:
                    // Managed inside the placement resolver coroutine
                    break;

                case GameState.CheckGameOver:
                    EvaluateGameOverAndTransitions();
                    break;

                case GameState.GameOver:
                    HandleGameOverState();
                    break;

                case GameState.Reviving:
                    // Handled by AttemptRevive() coroutine
                    break;

                case GameState.Paused:
                    // Handled explicitly inside PauseGame()
                    break;
            }
        }

        private void GenerateAndSetNewBatch()
        {
            int colorPaletteCount = boardConfig.blockSkins.Length;
            PieceInstance[] batch = _batchGenerator.GenerateBatch(_boardModel, piecePool, colorPaletteCount);

            // Spawns pieces visually in the tray
            GameEvents.InvokeNewBatchReady(batch);
            
            TransitionState(GameState.PieceSelection);
        }

        private void EvaluateGameOverAndTransitions()
        {
            List<PieceInstance> remaining = pieceTrayController.GetRemainingPieces();

            if (remaining.Count == 0)
            {
                // All pieces in the current batch placed: evaluate combos and batch scoring bonuses
                _comboManager.OnBatchComplete();
                _scoreManager.OnBatchComplete();

                if (_saveService != null)
                {
                    _saveService.TryUpdateBestCombo(_comboManager.CurrentCombo);
                    _saveService.Save();
                }

                TransitionState(GameState.WaitingForBatch);
            }
            else
            {
                // Unplaced pieces remain: check if any of them can fit on the board
                bool canPlay = _gameOverDetector.CanAnyPieceBePlaced(_boardModel, remaining);

                if (canPlay)
                {
                    TransitionState(GameState.PieceSelection);
                }
                else
                {
                    TransitionState(GameState.GameOver);
                }
            }
        }

        private void HandleGameOverState()
        {
            touchInputController.IsInputEnabled = false;
            pieceTrayController.SetInteractable(false);

            int finalScore = _scoreManager.TotalScore;
            bool isNewBest = SaveHighScore(finalScore);

            // Show results screen (full Sprint 4 version) or fallback to simple popup
            if (resultsScreen != null)
            {
                // Ad policy check — show interstitial before results if appropriate
                var adPolicyManager = Boot.ServiceLocator.Get<AdPolicyManager>();
                adPolicyManager?.OnGameOverTriggered(() =>
                {
                    resultsScreen.Show(finalScore, GetHighScore(), isNewBest,
                                       _runLinesCleared, _runBestCombo);
                });

                // If no ad policy manager, show results immediately
                if (adPolicyManager == null)
                {
                    resultsScreen.Show(finalScore, GetHighScore(), isNewBest,
                                       _runLinesCleared, _runBestCombo);
                }
            }
            else if (gameOverPopup != null)
            {
                gameOverPopup.Show(finalScore, GetHighScore(), isNewBest);
            }

            GameEvents.InvokeGameOver(finalScore);
        }

        private void HandlePieceDropped(PieceInstance piece, Vector2Int gridPos, int slotIndex)
        {
            TransitionState(GameState.ValidPlacement);
            StartCoroutine(PlacementResolutionRoutine(piece, gridPos, slotIndex));
        }

        private IEnumerator PlacementResolutionRoutine(PieceInstance piece, Vector2Int gridPos, int slotIndex)
        {
            Vector3 placementWorldPos = boardController.GridToWorld(gridPos.y, gridPos.x);

            // Execute the sequential resolution steps (fills model, yields for sweep anim, clears model, refreshes views)
            yield return StartCoroutine(_placementResolver.ResolvePlacementRoutine(
                _boardModel,
                boardController,
                piece,
                gridPos,
                (result) =>
                {
                    // Compute score managers and update tray slot statuses
                    _comboManager.OnPlacementResolved(result);
                    _scoreManager.OnPiecePlaced(piece, result, placementWorldPos);
                    pieceTrayController.OnPiecePlaced(slotIndex);

                    // Track run stats for results screen
                    _runLinesCleared += result.LinesCleared;
                    if (_comboManager.CurrentCombo > _runBestCombo)
                        _runBestCombo = _comboManager.CurrentCombo;

                    if (_saveService != null && result.LinesCleared > 0)
                    {
                        _saveService.AddLinesCleared(result.LinesCleared);
                        if (result.NovaCross)
                        {
                            _saveService.AddNovaCross();
                        }
                        _saveService.Save();
                    }
                }
            ));

            TransitionState(GameState.CheckGameOver);
        }

        /// <summary>
        /// Pauses gameplay and opens the pause popup.
        /// </summary>
        public void PauseGame()
        {
            if (_currentState == GameState.Paused || _currentState == GameState.GameOver) return;

            // Force cancel active dragging
            touchInputController.CancelActiveDrag();

            _stateBeforePause = _currentState;
            Time.timeScale = 0f;

            if (pausePopup != null)
            {
                pausePopup.Show();
            }

            TransitionState(GameState.Paused);
        }

        /// <summary>
        /// Resumes gameplay and hides the pause popup.
        /// </summary>
        public void ResumeGame()
        {
            if (_currentState != GameState.Paused) return;

            Time.timeScale = 1f;

            if (pausePopup != null)
            {
                pausePopup.Hide();
            }

            TransitionState(_stateBeforePause);
        }

        private void HandleRetryGame()
        {
            StartGame();
        }

        private void HandleQuitToHome()
        {
            Time.timeScale = 1f;
            SceneLoader.LoadScene(Constants.SCENE_HOME);
        }

        // ── Revive System ────────────────────────────────────────

        /// <summary>
        /// Returns true if the player is eligible for a revive this run.
        /// </summary>
        private bool CanRevive()
        {
            if (_revivesUsedThisRun >= Constants.MAX_REVIVES_PER_RUN)
                return false;

            var adService = Boot.ServiceLocator.Get<IAdService>();
            return adService != null && adService.IsRewardedAdReady;
        }

        /// <summary>
        /// Offers the player a rewarded ad to revive.
        /// If accepted: clears the N fullest rows, refreshes the board, and resumes play.
        /// If declined: transitions to GameOver.
        /// </summary>
        private void AttemptRevive()
        {
            var adService = Boot.ServiceLocator.Get<IAdService>();
            if (adService == null)
            {
                TransitionState(GameState.GameOver);
                return;
            }

            adService.ShowRewardedAd((success) =>
            {
                if (success)
                {
                    _revivesUsedThisRun++;

                    // Clear the fullest rows to make room
                    int rowsToClear = Constants.REVIVE_ROWS_TO_CLEAR;
                    _boardModel.ClearFullestRows(rowsToClear);
                    boardController.RefreshBoard(_boardModel);

                    // Notify ad policy that a rewarded ad was watched
                    var adPolicyManager = Boot.ServiceLocator.Get<Meta.AdPolicyManager>();
                    adPolicyManager?.OnRewardedAdWatched();

                    Debug.Log($"[GameManager] Revive successful! Cleared {rowsToClear} rows.");

                    // Resume play — re-check if pieces can now be placed
                    TransitionState(GameState.CheckGameOver);
                }
                else
                {
                    // Player declined or ad failed — proceed to game over
                    Debug.Log("[GameManager] Revive declined. Proceeding to Game Over.");
                    TransitionState(GameState.GameOver);
                }
            });
        }

        private void HandleScorePopupRequested(int score, Vector3 worldPos)
        {
            if (scorePopupPrefab != null)
            {
                ScorePopupView popup = Instantiate(scorePopupPrefab);
                popup.Setup(score, worldPos);
            }
        }

        private int GetHighScore()
        {
            if (_saveService != null)
            {
                return _saveService.Data.bestScore;
            }
            return PlayerPrefs.GetInt("NeonGalaxy_HighScore", 0);
        }

        private bool SaveHighScore(int score)
        {
            if (_saveService != null)
            {
                bool updated = _saveService.TryUpdateBestScore(score);
                if (updated)
                {
                    _saveService.Save();
                    GameEvents.InvokeNewBestScore(score);
                }
                return updated;
            }

            int oldHighScore = PlayerPrefs.GetInt("NeonGalaxy_HighScore", 0);
            if (score > oldHighScore)
            {
                PlayerPrefs.SetInt("NeonGalaxy_HighScore", score);
                PlayerPrefs.Save();
                GameEvents.InvokeNewBestScore(score);
                return true;
            }
            return false;
        }
    }
}
