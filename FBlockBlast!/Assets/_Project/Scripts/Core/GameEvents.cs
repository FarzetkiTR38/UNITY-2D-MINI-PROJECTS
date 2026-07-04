using System;
using UnityEngine;
using NeonGalaxy.Data;

namespace NeonGalaxy.Core
{
    /// <summary>
    /// Static event bus for decoupled communication between systems.
    /// All gameplay, meta, and UI events are declared here.
    /// Listeners subscribe/unsubscribe via += / -=.
    /// 
    /// IMPORTANT: Always unsubscribe in OnDisable/OnDestroy to prevent
    /// stale references across scene loads.
    /// </summary>
    public static class GameEvents
    {
        // ══════════════════════════════════════════════════════════
        // GAMEPLAY EVENTS
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// Fired when a piece is successfully placed on the board.
        /// Args: PieceInstance, board position (row, col).
        /// </summary>
        public static event Action<PieceInstance, Vector2Int> OnPiecePlaced;

        /// <summary>
        /// Fired when one or more lines are cleared after a placement.
        /// Args: full row indices array, row count, full col indices array, col count.
        /// </summary>
        public static event Action<int[], int, int[], int> OnLinesCleared;

        /// <summary>
        /// Fired when a Nova Cross event occurs (simultaneous row + column clear).
        /// </summary>
        public static event Action OnNovaCross;

        /// <summary>
        /// Fired when the combo counter changes.
        /// Args: new combo value.
        /// </summary>
        public static event Action<int> OnComboUpdated;

        /// <summary>
        /// Fired when the total run score changes.
        /// Args: new total score.
        /// </summary>
        public static event Action<int> OnScoreChanged;

        /// <summary>
        /// Fired when all 3 pieces in a batch have been placed.
        /// </summary>
        public static event Action OnBatchComplete;

        /// <summary>
        /// Fired when a new 3-piece batch is ready for the player.
        /// Args: array of 3 PieceInstances.
        /// </summary>
        public static event Action<PieceInstance[]> OnNewBatchReady;

        /// <summary>
        /// Fired when the game ends (no valid placements remain).
        /// Args: final score.
        /// </summary>
        public static event Action<int> OnGameOver;

        /// <summary>
        /// Fired when the game state changes.
        /// Args: new GameState.
        /// </summary>
        public static event Action<GameState> OnGameStateChanged;

        // ══════════════════════════════════════════════════════════
        // META EVENTS
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// Fired when the player levels up.
        /// Args: new level.
        /// </summary>
        public static event Action<int> OnLevelUp;

        /// <summary>
        /// Fired when an achievement is unlocked.
        /// Args: achievement ID.
        /// </summary>
        public static event Action<string> OnAchievementUnlocked;

        /// <summary>
        /// Fired when the player beats their best score.
        /// Args: new best score.
        /// </summary>
        public static event Action<int> OnNewBestScore;

        /// <summary>
        /// Fired when the player's coin balance changes.
        /// Args: new balance.
        /// </summary>
        public static event Action<int> OnCoinBalanceChanged;

        /// <summary>
        /// Fired when the player's gem balance changes.
        /// Args: new balance.
        /// </summary>
        public static event Action<int> OnGemBalanceChanged;

        /// <summary>
        /// Fired when a rewarded ad finishes.
        /// Args: true if reward granted, false if cancelled.
        /// </summary>
        public static event Action<bool> OnAdRewardReceived;

        // ══════════════════════════════════════════════════════════
        // UI EVENTS
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// Requests a floating score popup at a world position.
        /// Args: points, world position.
        /// </summary>
        public static event Action<int, Vector3> OnScorePopupRequested;

        // ══════════════════════════════════════════════════════════
        // PROFILE EVENTS
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// Fired when any profile data changes (name, avatar, linked account).
        /// </summary>
        public static event Action OnProfileUpdated;

        // ══════════════════════════════════════════════════════════
        // INVOKE METHODS (centralized null-safe invocation)
        // ══════════════════════════════════════════════════════════

        public static void InvokePiecePlaced(PieceInstance piece, Vector2Int pos)
            => OnPiecePlaced?.Invoke(piece, pos);

        public static void InvokeLinesCleared(int[] rows, int rowCount, int[] cols, int colCount)
            => OnLinesCleared?.Invoke(rows, rowCount, cols, colCount);

        public static void InvokeNovaCross()
            => OnNovaCross?.Invoke();

        public static void InvokeComboUpdated(int combo)
            => OnComboUpdated?.Invoke(combo);

        public static void InvokeScoreChanged(int score)
            => OnScoreChanged?.Invoke(score);

        public static void InvokeBatchComplete()
            => OnBatchComplete?.Invoke();

        public static void InvokeNewBatchReady(PieceInstance[] batch)
            => OnNewBatchReady?.Invoke(batch);

        public static void InvokeGameOver(int finalScore)
            => OnGameOver?.Invoke(finalScore);

        public static void InvokeGameStateChanged(GameState state)
            => OnGameStateChanged?.Invoke(state);

        public static void InvokeLevelUp(int newLevel)
            => OnLevelUp?.Invoke(newLevel);

        public static void InvokeAchievementUnlocked(string id)
            => OnAchievementUnlocked?.Invoke(id);

        public static void InvokeNewBestScore(int score)
            => OnNewBestScore?.Invoke(score);

        public static void InvokeCoinBalanceChanged(int newBalance)
            => OnCoinBalanceChanged?.Invoke(newBalance);

        public static void InvokeGemBalanceChanged(int newBalance)
            => OnGemBalanceChanged?.Invoke(newBalance);

        public static void InvokeAdRewardReceived(bool success)
            => OnAdRewardReceived?.Invoke(success);

        public static void InvokeScorePopupRequested(int points, Vector3 worldPos)
            => OnScorePopupRequested?.Invoke(points, worldPos);

        public static void InvokeProfileUpdated()
            => OnProfileUpdated?.Invoke();

        // ══════════════════════════════════════════════════════════
        // CLEANUP
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// Clears all event subscribers. Call during scene transitions
        /// or test teardown to prevent stale references.
        /// </summary>
        public static void ClearAll()
        {
            OnPiecePlaced = null;
            OnLinesCleared = null;
            OnNovaCross = null;
            OnComboUpdated = null;
            OnScoreChanged = null;
            OnBatchComplete = null;
            OnNewBatchReady = null;
            OnGameOver = null;
            OnGameStateChanged = null;
            OnLevelUp = null;
            OnAchievementUnlocked = null;
            OnNewBestScore = null;
            OnCoinBalanceChanged = null;
            OnGemBalanceChanged = null;
            OnAdRewardReceived = null;
            OnScorePopupRequested = null;
            OnProfileUpdated = null;
        }
    }
}
