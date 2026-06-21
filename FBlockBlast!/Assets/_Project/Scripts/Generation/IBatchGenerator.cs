using NeonGalaxy.Core;
using NeonGalaxy.Data;

namespace NeonGalaxy.Generation
{
    /// <summary>
    /// Spawning contract interface for creating batches of puzzle pieces.
    /// Allows swapping between simple random generation and combo-friendly heuristics.
    /// </summary>
    public interface IBatchGenerator
    {
        /// <summary>
        /// Generates a batch of pieces.
        /// </summary>
        /// <param name="board">Current board model state for placement simulation.</param>
        /// <param name="pool">The pool of piece definitions to choose from.</param>
        /// <param name="colorCount">The number of available colors in the block palette.</param>
        /// <returns>An array of PieceInstance objects ready to spawn in the tray.</returns>
        PieceInstance[] GenerateBatch(BoardModel board, PiecePoolSO pool, int colorCount);
    }
}
