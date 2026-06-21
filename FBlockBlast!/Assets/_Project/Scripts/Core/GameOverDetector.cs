using System.Collections.Generic;
using NeonGalaxy.Data;

namespace NeonGalaxy.Core
{
    /// <summary>
    /// Checks game-over states by verifying if any of the remaining pieces in the tray 
    /// can be placed on the current BoardModel layout.
    /// This is a pure C# class.
    /// </summary>
    public class GameOverDetector
    {
        /// <summary>
        /// Returns true if at least one remaining piece has at least one valid placement 
        /// anywhere on the board.
        /// </summary>
        /// <param name="board">The current board data model.</param>
        /// <param name="remainingPieces">The list of pieces currently in the tray (unplaced).</param>
        public bool CanAnyPieceBePlaced(BoardModel board, List<PieceInstance> remainingPieces)
        {
            if (board == null || remainingPieces == null || remainingPieces.Count == 0)
            {
                return true; // Safe fallback: if tray is empty or parameters are null, don't trigger game over
            }

            for (int i = 0; i < remainingPieces.Count; i++)
            {
                PieceInstance piece = remainingPieces[i];
                if (piece == null || piece.IsPlaced) continue;

                // Check if this piece definition fits anywhere on the grid
                if (board.HasValidPlacement(piece))
                {
                    return true; // Found at least one valid placement!
                }
            }

            return false; // All remaining pieces are blocked
        }
    }
}
