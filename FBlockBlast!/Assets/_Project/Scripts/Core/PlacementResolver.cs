using System;
using System.Collections;
using UnityEngine;
using NeonGalaxy.Data;

namespace NeonGalaxy.Core
{
    /// <summary>
    /// Coordinates the multi-step placement resolution process after a piece is dropped.
    /// Handles visual updates, line clear checking, sweep animation waiting, 
    /// model board clears, and event bus dispatching.
    /// </summary>
    public class PlacementResolver : MonoBehaviour
    {
        /// <summary>
        /// Runs the placement resolution sequence. Yields control while visual clear animations play.
        /// </summary>
        /// <param name="board">The BoardModel data state.</param>
        /// <param name="view">The BoardController rendering component.</param>
        /// <param name="piece">The PieceInstance placed.</param>
        /// <param name="gridPos">The grid position where the piece pivot was dropped.</param>
        /// <param name="onComplete">Callback triggered with the final score/clear results.</param>
        public IEnumerator ResolvePlacementRoutine(
            BoardModel board, 
            BoardController view, 
            PieceInstance piece, 
            Vector2Int gridPos, 
            Action<PlacementResult> onComplete)
        {
            // 1. Update data model state
            board.PlacePiece(piece, gridPos.y, gridPos.x);

            // 2. Synchronize board cell visuals immediately (shows placed block shapes)
            view.RefreshBoard(board);

            // 3. Scan the grid for newly completed lines
            int totalLines = board.FindFullLines(out int[] rows, out int rowCount, out int[] cols, out int colCount);
            bool novaCross = rowCount > 0 && colCount > 0;

            // Safe copy of results to prevent transient buffer corruption
            var result = new PlacementResult(totalLines, novaCross, rows, rowCount, cols, colCount);

            // 4. If lines were completed, trigger clears and animations
            if (totalLines > 0)
            {
                // Dispatch Event Bus notices
                GameEvents.InvokeLinesCleared(result.ClearedRows, result.RowCount, result.ClearedCols, result.ColCount);
                if (novaCross)
                {
                    GameEvents.InvokeNovaCross();
                }

                // Trigger sweep/destruction animation on BoardController
                bool animationCompleted = false;
                view.AnimateLineClear(result.ClearedRows, result.RowCount, result.ClearedCols, result.ColCount, () => 
                {
                    animationCompleted = true;
                });

                // Yield execution frame-by-frame until the cascading visual clear completes
                while (!animationCompleted)
                {
                    yield return null;
                }

                // Update data model by clearing the full lines
                board.ClearLines(result.ClearedRows, result.RowCount, result.ClearedCols, result.ColCount);

                // Check if the board is now completely empty → MEGA celebration
                if (board.CountOccupied() == 0)
                {
                    GameEvents.InvokeBoardCleared();
                }

                // Synchronize board visuals again (cleared cells revert to empty grid background)
                view.RefreshBoard(board);
            }

            // 5. Fire general piece placed event
            GameEvents.InvokePiecePlaced(piece, gridPos);

            // 6. Complete process and notify caller (GameManager)
            onComplete?.Invoke(result);
        }
    }
}
