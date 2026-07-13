using NeonGalaxy.Core;
using NeonGalaxy.Data;
using UnityEngine;
using System.Collections.Generic;

namespace NeonGalaxy.Generation
{
    /// <summary>
    /// Generates a specific sequence of pieces for the First Time User Experience (FTUE) Tutorial.
    /// Ensures the player gets exactly the pieces needed to perform a Nova Cross.
    /// </summary>
    public class TutorialBatchGenerator : IBatchGenerator
    {
        private int _batchCount = 0;

        public PieceInstance[] GenerateBatch(BoardModel board, PiecePoolSO pool, int colorCount)
        {
            PieceInstance[] batch = new PieceInstance[3];
            
            if (_batchCount == 0)
            {
                // First batch: Provide pieces for a Nova Cross
                // We need a horizontal line (e.g., 3-block or 4-block horizontal)
                // A vertical line
                // A single block (1x1)
                
                PieceDefinitionSO hLine = FindPiece(pool, "tetromino_line_h") ?? FindPiece(pool, "tromino_line_h");
                PieceDefinitionSO vLine = FindPiece(pool, "tetromino_line_v") ?? FindPiece(pool, "tromino_line_v");
                PieceDefinitionSO dot = FindPiece(pool, "monomino"); // assuming 1x1 block exists

                // Fallbacks if exact names don't match
                if (hLine == null) hLine = pool.pieces[0];
                if (vLine == null) vLine = pool.pieces[0];
                if (dot == null) dot = pool.pieces[0];

                batch[0] = new PieceInstance(hLine, Random.Range(0, colorCount));
                batch[1] = new PieceInstance(vLine, Random.Range(0, colorCount));
                batch[2] = new PieceInstance(dot, Random.Range(0, colorCount));
            }
            else
            {
                // Fallback to random if tutorial somehow needs more batches before completing
                for (int i = 0; i < 3; i++)
                {
                    var def = pool.pieces[Random.Range(0, pool.pieces.Count)];
                    batch[i] = new PieceInstance(def, Random.Range(0, colorCount));
                }
            }

            _batchCount++;
            return batch;
        }

        private PieceDefinitionSO FindPiece(PiecePoolSO pool, string idSubstring)
        {
            foreach (var piece in pool.pieces)
            {
                if (piece.pieceId.ToLower().Contains(idSubstring.ToLower()) || piece.name.ToLower().Contains(idSubstring.ToLower()))
                    return piece;
            }
            return null;
        }
    }
}
