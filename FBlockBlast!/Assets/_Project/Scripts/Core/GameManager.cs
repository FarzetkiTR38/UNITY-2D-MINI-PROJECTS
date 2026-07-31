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

        [Header("Combo Limits")]
        [Tooltip("If true, the combo multiplier is capped at the maximum value in ScoreConfig (5.0x). If false, it grows infinitely by 0.1x per combo.")]
        [SerializeField] private bool limitComboMultiplier = true;

        [Header("Gameplay Feel")]
        [Tooltip("When enabled, the piece smoothly follows the finger with a slight delay. When disabled, it instantly snaps to the finger position.")]
        [SerializeField] private bool useSmoothDrag = true;
        [Tooltip("Multiplies the drag distance. 1 = 1:1 with finger, 1.5 = piece moves 50% faster than finger.")]
        [SerializeField] [Range(1f, 3f)] private float dragSensitivityMultiplier = 1.5f;

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

        /// <summary>
        /// When true, the Game Over popup's Home button will navigate to the lobby
        /// instead of staying on the gameplay scene. Set by HandleQuitToHome().
        /// </summary>
        private bool _quitToHomeAfterGameOver;

        public GameState CurrentState => _currentState;
        public BoardModel BoardModel => _boardModel;

        public void SetBatchGenerator(IBatchGenerator newGenerator)
        {
            _batchGenerator = newGenerator;
        }

        private void Awake()
        {
            _placementResolver = GetComponent<PlacementResolver>();
        }

        private void Start()
        {
            InitializeSaveService();
            InitializeCoreGameplay();

            // Check for a saved in-progress run to resume
            if (_saveService != null && _saveService.Data.activeRun.hasActiveRun)
            {
                ResumeFromSavedState();
            }
            else
            {
                StartGame();
            }
        }

        private void OnEnable()
        {
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.Paused && _currentState != GameState.Paused)
            {
                PauseGame();
            }
        }

        private void OnDestroy()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            _scoreManager?.Cleanup();

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
                resultsScreen.OnContinueWithAd -= HandleContinueWithAd;
                resultsScreen.OnContinueWithGems -= HandleContinueWithGems;
                resultsScreen.OnDeclined -= HandleContinueDeclined;
            }

            if (pausePopup != null)
            {
                pausePopup.OnResumeClicked -= ResumeGame;
                pausePopup.OnRestartClicked -= HandleRetryGame;
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



        public void ApplyEquippedSkin()
        {
            if (Boot.ServiceLocator.Has<CosmeticManager>())
            {
                var cosmeticManager = Boot.ServiceLocator.Get<CosmeticManager>();
                string activeSkinId = cosmeticManager.GetEquipped(CosmeticCategory.BlockSkin);
                
                if (!string.IsNullOrEmpty(activeSkinId))
                {
                    boardConfig.SetActiveSkin(activeSkinId);
                }
            }

            if (boardController != null && _boardModel != null)
            {
                boardController.RefreshBoard(_boardModel);
            }
            if (pieceTrayController != null)
            {
                pieceTrayController.RefreshTrayVisuals();
            }
        }

        private void InitializeCoreGameplay()
        {
            // Apply equipped block skin to board config
            if (Boot.ServiceLocator.Has<CosmeticManager>())
            {
                var cosmeticManager = Boot.ServiceLocator.Get<CosmeticManager>();
                string activeSkinId = cosmeticManager.GetEquipped(CosmeticCategory.BlockSkin);
                
                if (!string.IsNullOrEmpty(activeSkinId))
                {
                    boardConfig.SetActiveSkin(activeSkinId);
                }
            }

            // Create data instances
            _boardModel = new BoardModel(boardConfig);
            _comboManager = new ComboManager(comboConfig);
            _scoreManager = new ScoreManager(scoreConfig, _comboManager);
            _scoreManager.SetLimitComboMultiplier(limitComboMultiplier);
            
            if (_saveService != null && !_saveService.Data.hasCompletedTutorial)
            {
                _batchGenerator = new NeonGalaxy.Generation.TutorialBatchGenerator();
                // We will also enable the TutorialController later in StartGame
            }
            else
            {
                _batchGenerator = new ComboFriendlyBatchGenerator();
            }

            _gameOverDetector = new GameOverDetector();

            // Initialize rendering boards
            boardController.Initialize(boardConfig);
            touchInputController.Initialize(_boardModel, ghostPreviewController);
            touchInputController.SetDragMultiplier(dragSensitivityMultiplier);
            touchInputController.SetUseSmoothDrag(useSmoothDrag);

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
                resultsScreen.OnContinueWithAd += HandleContinueWithAd;
                resultsScreen.OnContinueWithGems += HandleContinueWithGems;
                resultsScreen.OnDeclined += HandleContinueDeclined;
            }

            if (pausePopup != null)
            {
                pausePopup.OnResumeClicked -= ResumeGame;
                pausePopup.OnResumeClicked += ResumeGame;

                pausePopup.OnRestartClicked -= HandleRetryGame;
                pausePopup.OnRestartClicked += HandleRetryGame;

                pausePopup.OnQuitClicked -= HandleQuitToHome;
                pausePopup.OnQuitClicked += HandleQuitToHome;
            }
        }

        /// <summary>
        /// Resets scoring, combos, board states, and spawns the initial batch.
        /// </summary>
        public void StartGame()
        {
            Time.timeScale = 1f;
            _quitToHomeAfterGameOver = false;

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
            if (resultsScreen != null) resultsScreen.HideImmediate();
            if (pausePopup != null) pausePopup.Hide();

            // Clear any previously saved run state (fresh start)
            ClearRunState();

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
                    HandleRevivingState();
                    break;

                case GameState.Paused:
                    // Handled explicitly inside PauseGame()
                    break;
            }
        }

        private void GenerateAndSetNewBatch()
        {
            int colorPaletteCount = boardConfig.ActiveColorCount;
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
                    // Show continue popup if revives are available, else go straight to game over
                    if (_revivesUsedThisRun < Constants.MAX_REVIVES_PER_RUN)
                    {
                        TransitionState(GameState.Reviving);
                    }
                    else
                    {
                        TransitionState(GameState.GameOver);
                    }
                }
            }
        }

        /// <summary>
        /// Shows the "Continue Your Run?" popup before actual game over.
        /// The player can watch an ad, spend gems, or decline.
        /// </summary>
        private void HandleRevivingState()
        {
            touchInputController.IsInputEnabled = false;
            pieceTrayController.SetInteractable(false);

            int finalScore = _scoreManager.TotalScore;

            if (resultsScreen != null)
            {
                resultsScreen.Show(finalScore, GetHighScore());
            }
            else
            {
                // No results screen available — go straight to game over
                TransitionState(GameState.GameOver);
            }
        }

        /// <summary>
        /// Final game over state. Shows game over popup and fires the event.
        /// </summary>
        private void HandleGameOverState()
        {
            touchInputController.IsInputEnabled = false;
            pieceTrayController.SetInteractable(false);

            // Hide the continue popup if it's still visible
            if (resultsScreen != null)
                resultsScreen.HideImmediate();

            // Clear saved run state — the run is over
            ClearRunState();

            int finalScore = _scoreManager.TotalScore;
            bool isNewBest = SaveHighScore(finalScore);

            // --- PROGRESSION (XP) ---
            int xpEarned = 0;
            if (Boot.ServiceLocator.Has<ProgressionManager>())
            {
                var progressionManager = Boot.ServiceLocator.Get<ProgressionManager>();
                var result = progressionManager.ProcessRunResult(finalScore);
                xpEarned = result.XPEarned;
            }

            // --- GOLD (COINS) EARNING ---
            // 1 million score = 10,000 gold -> score / 100
            int goldEarned = finalScore / 100;
            if (_saveService != null && goldEarned > 0)
            {
                _saveService.Data.coins += goldEarned;
                _saveService.MarkDirty();
                _saveService.Save();
            }

            if (gameOverPopup != null)
            {
                gameOverPopup.Show(finalScore, GetHighScore(), isNewBest, xpEarned, goldEarned);
            }

            GameEvents.InvokeGameOver(finalScore);

            // --- LEADERBOARD SUBMISSION ---
            if (Boot.ServiceLocator.Has<ILeaderboardService>())
            {
                var leaderboard = Boot.ServiceLocator.Get<ILeaderboardService>();
                _ = leaderboard.SubmitScoreAsync(finalScore);
            }

            // --- CLOUD SYNC (Offline Fallback) ---
            if (Boot.ServiceLocator.Has<ProfileManager>())
            {
                var profileManager = Boot.ServiceLocator.Get<ProfileManager>();
                _ = profileManager.SyncWithCloud();
            }
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

            // Auto-save run state after each placement
            SaveRunState();

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
                pausePopup.Show(_scoreManager.TotalScore);
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
            ClearRunState();
            _quitToHomeAfterGameOver = false;
            StartGame();
        }

        private void HandleQuitToHome()
        {
            Time.timeScale = 1f;

            // If already in Game Over state (popup is showing), go to lobby
            if (_currentState == GameState.GameOver)
            {
                ClearRunState();
                SceneLoader.LoadScene(Constants.SCENE_HOME);
                return;
            }

            // Pause menu → Home: trigger the full Game Over flow first
            // so the player receives XP, gold, and sees the results
            if (pausePopup != null) pausePopup.Hide();

            _quitToHomeAfterGameOver = true;
            ClearRunState();
            TransitionState(GameState.GameOver);
        }

        // ── Continue / Revive System ─────────────────────────────

        /// <summary>
        /// Called when the player chooses to watch an ad from the continue popup.
        /// </summary>
        private void HandleContinueWithAd()
        {
            var adService = Boot.ServiceLocator.Get<IAdService>();
            if (adService == null)
            {
                Debug.LogWarning("[GameManager] No ad service available. Proceeding to Game Over.");
                TransitionState(GameState.GameOver);
                return;
            }

            adService.ShowRewardedAd((success) =>
            {
                if (success)
                {
                    ExecuteRevive();

                    // Notify ad policy that a rewarded ad was watched
                    var adPolicyManager = Boot.ServiceLocator.Get<Meta.AdPolicyManager>();
                    adPolicyManager?.OnRewardedAdWatched();

                    Debug.Log("[GameManager] Continue via ad successful!");
                }
                else
                {
                    Debug.Log("[GameManager] Ad failed or cancelled. Proceeding to Game Over.");
                    TransitionState(GameState.GameOver);
                }
            });
        }

        /// <summary>
        /// Called when the player chooses to spend gems from the continue popup.
        /// </summary>
        private void HandleContinueWithGems()
        {
            int gemCost = resultsScreen != null ? resultsScreen.GetGemCost() : 50;

            var currencyManager = Boot.ServiceLocator.Get<Meta.CurrencyManager>();
            if (currencyManager != null && currencyManager.SpendGems(gemCost))
            {
                ExecuteRevive();
                Debug.Log($"[GameManager] Continue via gems successful! Spent {gemCost} gems.");
            }
            else
            {
                Debug.LogWarning($"[GameManager] Not enough gems to continue. Need {gemCost}.");
                // Re-enable buttons so the player can choose another option
                // The resultsScreen countdown is already stopped, so we just proceed to game over
                TransitionState(GameState.GameOver);
            }
        }

        /// <summary>
        /// Called when the player declines or the countdown expires.
        /// </summary>
        private void HandleContinueDeclined()
        {
            Debug.Log("[GameManager] Player declined to continue. Proceeding to Game Over.");
            TransitionState(GameState.GameOver);
        }

        /// <summary>
        /// Executes the revive logic: clears the fullest rows, refreshes board, and resumes play.
        /// </summary>
        private void ExecuteRevive()
        {
            _revivesUsedThisRun++;

            // Hide the continue popup
            if (resultsScreen != null)
                resultsScreen.HideImmediate();

            // Clear the entire board
            _boardModel.Reset();
            boardController.RefreshBoard(_boardModel);

            Debug.Log("[GameManager] Revive executed! Board fully cleared.");

            // Resume play — re-check if pieces can now be placed
            TransitionState(GameState.CheckGameOver);
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

        // ── Run State Save / Resume System ────────────────────────

        /// <summary>
        /// Saves the current run state to disk. Called after each placement
        /// and when the app is paused/quit.
        /// </summary>
        private void SaveRunState()
        {
            if (_saveService == null) return;
            if (_currentState == GameState.GameOver) return;

            var run = _saveService.Data.activeRun;
            run.hasActiveRun = true;

            // Board state
            _boardModel.ExportState(out run.cellOccupied, out run.cellColors);

            // Score & combo
            run.totalScore = _scoreManager.TotalScore;
            run.currentCombo = _comboManager.CurrentCombo;
            run.batchLinesCleared = _comboManager.BatchLinesCleared;
            run.batchHadNovaCross = _comboManager.BatchHadNovaCross;

            // Run stats
            run.runLinesCleared = _runLinesCleared;
            run.runBestCombo = _runBestCombo;
            run.revivesUsedThisRun = _revivesUsedThisRun;

            // Tray pieces
            for (int i = 0; i < 3; i++)
            {
                PieceView pv = pieceTrayController.GetPieceView(i);
                if (pv != null && pv.Piece != null)
                {
                    run.trayPieceDefinitionIds[i] = pv.Piece.Definition.pieceId;
                    run.trayPieceColorIndices[i] = pv.Piece.ColorIndex;
                    run.trayPiecePlaced[i] = pv.Piece.IsPlaced;
                }
                else
                {
                    run.trayPieceDefinitionIds[i] = "";
                    run.trayPieceColorIndices[i] = 0;
                    run.trayPiecePlaced[i] = true;
                }
            }

            _saveService.MarkDirty();
            _saveService.Save();

            Debug.Log("[GameManager] Run state saved.");
        }

        /// <summary>
        /// Clears the saved run state (game over, retry, or quit to home).
        /// </summary>
        private void ClearRunState()
        {
            if (_saveService == null) return;

            _saveService.Data.activeRun.Clear();
            _saveService.MarkDirty();
            _saveService.Save();

            Debug.Log("[GameManager] Run state cleared.");
        }

        /// <summary>
        /// Resumes gameplay from a previously saved run state.
        /// Restores board, score, combo, tray pieces, and transitions to PieceSelection.
        /// </summary>
        private void ResumeFromSavedState()
        {
            Time.timeScale = 1f;
            _quitToHomeAfterGameOver = false;

            var run = _saveService.Data.activeRun;

            // Restore board state
            _boardModel.ImportState(run.cellOccupied, run.cellColors);
            boardController.RefreshBoard(_boardModel);

            // Restore score & combo
            _scoreManager.RestoreScore(run.totalScore);
            _comboManager.RestoreState(run.currentCombo, run.batchLinesCleared, run.batchHadNovaCross);

            // Restore run stats
            _runLinesCleared = run.runLinesCleared;
            _runBestCombo = run.runBestCombo;
            _revivesUsedThisRun = run.revivesUsedThisRun;

            // Restore tray pieces
            pieceTrayController.ClearTray();
            var restoredBatch = new List<PieceInstance>();
            bool allPlaced = true;

            for (int i = 0; i < 3; i++)
            {
                if (!string.IsNullOrEmpty(run.trayPieceDefinitionIds[i]) && !run.trayPiecePlaced[i])
                {
                    var def = piecePool.FindByPieceId(run.trayPieceDefinitionIds[i]);
                    if (def != null)
                    {
                        var piece = new PieceInstance(def, run.trayPieceColorIndices[i]);
                        restoredBatch.Add(piece);
                        allPlaced = false;
                    }
                    else
                    {
                        Debug.LogWarning($"[GameManager] Could not find piece definition '{run.trayPieceDefinitionIds[i]}' for resume. Skipping.");
                    }
                }
            }

            // Update HUD
            if (hudController != null)
            {
                hudController.UpdateBestScore(GetHighScore());
                GameEvents.InvokeScoreChanged(run.totalScore);
                GameEvents.InvokeComboUpdated(run.currentCombo);
            }

            if (gameOverPopup != null) gameOverPopup.Hide();
            if (resultsScreen != null) resultsScreen.HideImmediate();
            if (pausePopup != null) pausePopup.Hide();

            Debug.Log($"[GameManager] Resuming saved run. Score: {run.totalScore}, Combo: {run.currentCombo}, Remaining pieces: {restoredBatch.Count}");

            if (allPlaced || restoredBatch.Count == 0)
            {
                // All tray pieces were placed — generate a new batch
                TransitionState(GameState.WaitingForBatch);
            }
            else
            {
                // Spawn the remaining pieces and let the player continue
                GameEvents.InvokeNewBatchReady(restoredBatch.ToArray());
                TransitionState(GameState.PieceSelection);
            }
        }

        // ── Application Lifecycle (Auto-save) ────────────────────

        private void OnApplicationPause(bool paused)
        {
            if (paused && _currentState != GameState.GameOver)
            {
                SaveRunState();
            }
        }

        private void OnApplicationQuit()
        {
            if (_currentState != GameState.GameOver)
            {
                SaveRunState();
            }
        }
    }
}
