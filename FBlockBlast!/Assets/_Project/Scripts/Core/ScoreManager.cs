using UnityEngine;
using NeonGalaxy.Data;

namespace NeonGalaxy.Core
{
    /// <summary>
    /// Implements the game's full scoring pipeline.
    /// Calculates placement scores, line clear scores, combo multipliers, 
    /// and batch-level bonuses from the configurations.
    /// This is a pure C# class.
    /// </summary>
    public class ScoreManager
    {
        private readonly ScoreConfigSO _scoreConfig;
        private readonly ComboManager _comboManager;

        public int TotalScore { get; private set; }

        public ScoreManager(ScoreConfigSO scoreConfig, ComboManager comboManager)
        {
            _scoreConfig = scoreConfig;
            _comboManager = comboManager;
            Reset();
            GameEvents.OnBoardCleared += HandleBoardCleared;
        }

        public void Cleanup()
        {
            GameEvents.OnBoardCleared -= HandleBoardCleared;
        }

        private void HandleBoardCleared()
        {
            if (_scoreConfig == null) return;
            TotalScore += _scoreConfig.boardClearBonus;
            GameEvents.InvokeScoreChanged(TotalScore);
            GameEvents.InvokeScorePopupRequested(_scoreConfig.boardClearBonus, Vector3.zero);
        }

        /// <summary>
        /// Resets the total score back to zero.
        /// </summary>
        public void Reset()
        {
            TotalScore = 0;
        }

        /// <summary>
        /// Handles placement scoring, updates the total, and triggers score popup requests.
        /// </summary>
        public void OnPiecePlaced(PieceInstance piece, PlacementResult result, Vector3 worldPos)
        {
            if (_scoreConfig == null || piece == null) return;

            // 1. Calculate placement score (points per filled cell)
            int placementScore = _scoreConfig.CalculatePlacementScore(piece.CellCount);

            // 2. Calculate line clear score with active combo multiplier
            int clearScore = 0;
            if (result.LinesCleared > 0)
            {
                int clearBase = _scoreConfig.CalculateLineClearBase(result.LinesCleared);
                int novaBonus = result.NovaCross ? _scoreConfig.novaCrossBonus : 0;
                
                // Read combo multiplier from the ScoreConfigSO using the current combo level
                float comboMultiplier = _scoreConfig.GetComboMultiplier(_comboManager.CurrentCombo);

                clearScore = Mathf.RoundToInt((clearBase + novaBonus) * comboMultiplier);
            }

            int placementTotal = placementScore + clearScore;
            TotalScore += placementTotal;

            // Fire score update event
            GameEvents.InvokeScoreChanged(TotalScore);

            // Request floating score popup at placement position
            GameEvents.InvokeScorePopupRequested(placementTotal, worldPos);
        }

        /// <summary>
        /// Applies batch quality bonuses at the end of a 3-piece batch.
        /// </summary>
        public void OnBatchComplete()
        {
            if (_scoreConfig == null) return;

            // Calculate batch bonus based on the total lines cleared in this batch
            int batchBonusBase = _scoreConfig.GetBatchBonus(_comboManager.BatchLinesCleared);

            if (batchBonusBase > 0)
            {
                // Multiplied by active combo multiplier
                float comboMultiplier = _scoreConfig.GetComboMultiplier(_comboManager.CurrentCombo);
                int finalBatchBonus = Mathf.RoundToInt(batchBonusBase * comboMultiplier);

                TotalScore += finalBatchBonus;

                // Fire score update event
                GameEvents.InvokeScoreChanged(TotalScore);
            }
        }
    }
}
