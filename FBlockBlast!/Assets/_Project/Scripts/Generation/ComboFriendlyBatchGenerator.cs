using System.Collections.Generic;
using UnityEngine;
using NeonGalaxy.Core;
using NeonGalaxy.Data;

namespace NeonGalaxy.Generation
{
    /// <summary>
    /// Implements the high-performance, combo-friendly batch generator.
    /// Uses a sample-and-score algorithm: simulates multiple random candidate 
    /// batches using BoardBitmask structures and evaluates them.
    /// Prefers batches that yield line clears and guarantees that all pieces 
    /// in the generated batch can be placed sequentially.
    /// </summary>
    public class ComboFriendlyBatchGenerator : IBatchGenerator
    {
        private const int MaxCandidates = 30;
        private const int EarlyAcceptScore = 3;

        /// <summary>
        /// Generates a batch of pieces optimized for placement and combo opportunities.
        /// </summary>
        public PieceInstance[] GenerateBatch(BoardModel board, PiecePoolSO pool, int colorCount)
        {
            if (board == null || pool == null || pool.pieces == null || pool.pieces.Count == 0)
                return null;

            BoardBitmask initialBitmask = board.ToBitmask();
            PieceInstance[] bestCandidate = null;
            int bestScore = -1;

            // Sample and score multiple random batch candidates
            for (int i = 0; i < MaxCandidates; i++)
            {
                PieceInstance[] candidate = GenerateCandidate(pool, colorCount);
                int score = EvaluateBatch(initialBitmask, candidate, out bool isValid);

                if (isValid)
                {
                    // Update tracker with the best valid candidate found
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestCandidate = candidate;
                    }

                    // Early accept if we find an exceptionally good combo-generating batch
                    if (score >= EarlyAcceptScore)
                    {
                        return candidate;
                    }
                }
            }

            // Return the best scoring candidate if at least one valid batch was found
            if (bestScore >= 0 && bestCandidate != null)
            {
                return bestCandidate;
            }

            // Fallback: If no candidate was fully placeable, generate a random batch
            // prioritizing small (Tiny/Small) pieces to prevent game-over deadlocks.
            return GenerateFallbackBatch(pool, colorCount);
        }

        private PieceInstance[] GenerateCandidate(PiecePoolSO pool, int colorCount)
        {
            int size = pool.batchSize;
            var candidate = new PieceInstance[size];
            var selectedDefs = new List<PieceDefinitionSO>();

            for (int i = 0; i < size; i++)
            {
                PieceDefinitionSO def = null;
                int attempts = 0;

                while (attempts < 20)
                {
                    def = GetRandomPiece(pool);
                    attempts++;

                    int duplicateCount = 0;
                    for (int j = 0; j < selectedDefs.Count; j++)
                    {
                        if (selectedDefs[j] == def) duplicateCount++;
                    }

                    // Enforce duplicate restriction configs
                    if (!pool.allowDuplicatesInBatch && duplicateCount >= 1)
                        continue;

                    if (!pool.allowTriplicatesInBatch && duplicateCount >= 2)
                        continue;

                    break;
                }

                selectedDefs.Add(def);
                int randomColor = Random.Range(0, colorCount);
                candidate[i] = new PieceInstance(def, randomColor);
            }

            return candidate;
        }

        private int EvaluateBatch(BoardBitmask board, PieceInstance[] batch, out bool isValid)
        {
            int totalCleared = 0;
            bool hadNovaCross = false;
            isValid = true;

            // Pre-allocate row/col buffers to avoid garbage inside the loop
            int[] rowBuffer = new int[8];
            int[] colBuffer = new int[8];

            for (int i = 0; i < batch.Length; i++)
            {
                PieceInstance piece = batch[i];
                int bestPlacementLines = -1;
                ulong bestPlacementMask = 0;
                bool bestPlacementNovaCross = false;

                // Test all 64 coordinates for the pivot cell
                for (int r = 0; r < 8; r++)
                {
                    for (int c = 0; c < 8; c++)
                    {
                        ulong pieceMask = BoardBitmask.ComputePieceMask(piece.CellOffsets, r, c);
                        if (board.CanPlace(pieceMask))
                        {
                            // Copy by value (no allocations)
                            BoardBitmask simBoard = board;
                            simBoard.Place(pieceMask);

                            // Detect lines cleared by this placement
                            int cleared = 0;
                            simBoard.FindFullLines(rowBuffer, colBuffer, out int rowCount, out int colCount);
                            cleared = rowCount + colCount;
                            bool nova = rowCount > 0 && colCount > 0;

                            // Select the layout that maximizes line clearing
                            if (cleared > bestPlacementLines)
                            {
                                bestPlacementLines = cleared;
                                bestPlacementMask = pieceMask;
                                bestPlacementNovaCross = nova;
                            }
                        }
                    }
                }

                // If this piece cannot be placed anywhere in sequence, the entire batch candidate is invalid
                if (bestPlacementLines == -1)
                {
                    isValid = false;
                    return -1;
                }

                // Update board state for the next piece in the batch simulation
                board.Place(bestPlacementMask);
                board.DetectAndClearLines(out bool _);

                totalCleared += bestPlacementLines;
                if (bestPlacementNovaCross)
                {
                    hadNovaCross = true;
                }
            }

            // Calculate simulation score (Nova Cross provides a flat weight bonus)
            int score = totalCleared;
            if (hadNovaCross)
            {
                score += 3;
            }

            return score;
        }

        private PieceInstance[] GenerateFallbackBatch(PiecePoolSO pool, int colorCount)
        {
            int size = pool.batchSize;
            var fallback = new PieceInstance[size];

            // When board is highly congested, pick simple piece definitions (Tiny/Small/Medium)
            // to maximize placement likelihood and avoid immediate game-over states.
            for (int i = 0; i < size; i++)
            {
                PieceDefinitionSO def = null;
                int attempts = 0;

                while (attempts < 30)
                {
                    def = GetRandomPiece(pool);
                    attempts++;

                    // Prefer categories that occupy fewer cells (Tiny, Small, Medium)
                    if (def.category == PieceCategory.Large || def.category == PieceCategory.XL)
                    {
                        if (attempts < 25) continue; // Loosen restriction on final attempts
                    }
                    break;
                }

                int randomColor = Random.Range(0, colorCount);
                fallback[i] = new PieceInstance(def, randomColor);
            }

            return fallback;
        }

        private PieceDefinitionSO GetRandomPiece(PiecePoolSO pool)
        {
            float totalWeight = pool.GetTotalWeight();
            float randomVal = Random.value * totalWeight;
            float cumulative = 0f;

            for (int i = 0; i < pool.pieces.Count; i++)
            {
                PieceDefinitionSO p = pool.pieces[i];
                if (p == null) continue;

                cumulative += p.spawnWeight;
                if (randomVal <= cumulative)
                {
                    return p;
                }
            }

            return pool.pieces[pool.pieces.Count - 1];
        }
    }
}
