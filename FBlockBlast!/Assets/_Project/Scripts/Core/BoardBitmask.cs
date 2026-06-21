using System.Runtime.CompilerServices;

namespace NeonGalaxy.Core
{
    /// <summary>
    /// High-performance 64-bit board representation for an 8×8 grid.
    /// Each bit represents a cell: bit index = row * 8 + col.
    /// Used by the batch generator for fast simulation without allocations.
    /// </summary>
    public struct BoardBitmask
    {
        /// <summary>
        /// The 64-bit mask. Bit N = cell at (row = N/8, col = N%8).
        /// </summary>
        public ulong Mask;

        // ── Precomputed row masks ────────────────────────────────
        // Row 0 = bits 0–7, Row 1 = bits 8–15, etc.
        private static readonly ulong[] RowMasks = new ulong[8]
        {
            0x00000000000000FFUL, // row 0
            0x000000000000FF00UL, // row 1
            0x0000000000FF0000UL, // row 2
            0x00000000FF000000UL, // row 3
            0x000000FF00000000UL, // row 4
            0x0000FF0000000000UL, // row 5
            0x00FF000000000000UL, // row 6
            0xFF00000000000000UL, // row 7
        };

        // ── Precomputed column masks ─────────────────────────────
        // Col 0 = bits 0,8,16,24,32,40,48,56
        private static readonly ulong[] ColMasks = new ulong[8]
        {
            0x0101010101010101UL,       // col 0
            0x0101010101010101UL << 1,  // col 1
            0x0101010101010101UL << 2,  // col 2
            0x0101010101010101UL << 3,  // col 3
            0x0101010101010101UL << 4,  // col 4
            0x0101010101010101UL << 5,  // col 5
            0x0101010101010101UL << 6,  // col 6
            0x0101010101010101UL << 7,  // col 7
        };

        /// <summary>
        /// Full board mask (all 64 bits set).
        /// </summary>
        public static readonly ulong FullBoard = 0xFFFFFFFFFFFFFFFFUL;

        public BoardBitmask(ulong mask)
        {
            Mask = mask;
        }

        // ── Cell Operations ──────────────────────────────────────

        /// <summary>
        /// Returns true if the cell at (row, col) is occupied.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsOccupied(int row, int col)
        {
            return (Mask & (1UL << (row * 8 + col))) != 0;
        }

        /// <summary>
        /// Sets the cell at (row, col) as occupied.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCell(int row, int col)
        {
            Mask |= (1UL << (row * 8 + col));
        }

        /// <summary>
        /// Clears the cell at (row, col).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearCell(int row, int col)
        {
            Mask &= ~(1UL << (row * 8 + col));
        }

        // ── Line Detection ───────────────────────────────────────

        /// <summary>
        /// Returns true if the given row is fully filled.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsRowFull(int row)
        {
            return (Mask & RowMasks[row]) == RowMasks[row];
        }

        /// <summary>
        /// Returns true if the given column is fully filled.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsColFull(int col)
        {
            return (Mask & ColMasks[col]) == ColMasks[col];
        }

        /// <summary>
        /// Counts how many rows are fully filled.
        /// </summary>
        public int CountFullRows()
        {
            int count = 0;
            for (int r = 0; r < 8; r++)
            {
                if (IsRowFull(r)) count++;
            }
            return count;
        }

        /// <summary>
        /// Counts how many columns are fully filled.
        /// </summary>
        public int CountFullCols()
        {
            int count = 0;
            for (int c = 0; c < 8; c++)
            {
                if (IsColFull(c)) count++;
            }
            return count;
        }

        /// <summary>
        /// Finds all full rows and columns. Returns total lines cleared and
        /// whether a Nova Cross occurred (at least one row AND one column full).
        /// Populates the output arrays with indices of full rows/columns.
        /// </summary>
        /// <param name="fullRows">Pre-allocated array (length ≥ 8). Filled with full row indices.</param>
        /// <param name="fullCols">Pre-allocated array (length ≥ 8). Filled with full column indices.</param>
        /// <param name="rowCount">Number of full rows found.</param>
        /// <param name="colCount">Number of full columns found.</param>
        /// <returns>True if at least one line was found.</returns>
        public bool FindFullLines(int[] fullRows, int[] fullCols, out int rowCount, out int colCount)
        {
            rowCount = 0;
            colCount = 0;

            for (int r = 0; r < 8; r++)
            {
                if (IsRowFull(r))
                    fullRows[rowCount++] = r;
            }

            for (int c = 0; c < 8; c++)
            {
                if (IsColFull(c))
                    fullCols[colCount++] = c;
            }

            return rowCount > 0 || colCount > 0;
        }

        // ── Line Clearing ────────────────────────────────────────

        /// <summary>
        /// Clears the specified row (sets all 8 bits to 0).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearRow(int row)
        {
            Mask &= ~RowMasks[row];
        }

        /// <summary>
        /// Clears the specified column (sets all 8 bits to 0).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearCol(int col)
        {
            Mask &= ~ColMasks[col];
        }

        /// <summary>
        /// Clears all full rows and columns. Returns total lines cleared.
        /// </summary>
        public int ClearFullLines()
        {
            int cleared = 0;

            for (int r = 0; r < 8; r++)
            {
                if (IsRowFull(r))
                {
                    ClearRow(r);
                    cleared++;
                }
            }

            // Re-check columns after row clears (a row clear might un-fill a column,
            // but we check against the ORIGINAL state. In Block Blast, both rows and
            // columns are detected simultaneously before any clearing happens.)
            // So we need to detect first, then clear. This method is for quick sim only.
            for (int c = 0; c < 8; c++)
            {
                if (IsColFull(c))
                {
                    ClearCol(c);
                    cleared++;
                }
            }

            return cleared;
        }

        /// <summary>
        /// Detects full lines on the CURRENT state, then clears them all.
        /// Returns total lines cleared and whether Nova Cross occurred.
        /// This is the correct "detect-then-clear" approach.
        /// </summary>
        public int DetectAndClearLines(out bool novaCross)
        {
            int[] rows = new int[8];
            int[] cols = new int[8];
            FindFullLines(rows, cols, out int rowCount, out int colCount);

            novaCross = rowCount > 0 && colCount > 0;

            // Clear all detected lines
            for (int i = 0; i < rowCount; i++)
                ClearRow(rows[i]);
            for (int i = 0; i < colCount; i++)
                ClearCol(cols[i]);

            return rowCount + colCount;
        }

        // ── Piece Placement ──────────────────────────────────────

        /// <summary>
        /// Computes the bitmask for a piece placed at the given board position.
        /// Returns 0 if any cell is out of bounds.
        /// </summary>
        public static ulong ComputePieceMask(UnityEngine.Vector2Int[] cellOffsets, int boardRow, int boardCol)
        {
            ulong pieceMask = 0;

            for (int i = 0; i < cellOffsets.Length; i++)
            {
                int r = boardRow + cellOffsets[i].y;
                int c = boardCol + cellOffsets[i].x;

                if (r < 0 || r >= 8 || c < 0 || c >= 8)
                    return 0; // Out of bounds

                pieceMask |= (1UL << (r * 8 + c));
            }

            return pieceMask;
        }

        /// <summary>
        /// Returns true if the piece can be placed at the given position
        /// (all cells in bounds and no overlap with occupied cells).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanPlace(ulong pieceMask)
        {
            return pieceMask != 0 && (Mask & pieceMask) == 0;
        }

        /// <summary>
        /// Places the piece (ORs the piece mask into the board).
        /// Caller should verify CanPlace() first.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Place(ulong pieceMask)
        {
            Mask |= pieceMask;
        }

        // ── Utility ──────────────────────────────────────────────

        /// <summary>
        /// Counts the number of occupied cells on the board.
        /// </summary>
        public int CountOccupied()
        {
            // Hamming weight (popcount) using Brian Kernighan's algorithm
            ulong v = Mask;
            int count = 0;
            while (v != 0)
            {
                v &= v - 1;
                count++;
            }
            return count;
        }

        /// <summary>
        /// Returns the number of empty cells on the board.
        /// </summary>
        public int CountEmpty()
        {
            return 64 - CountOccupied();
        }

        /// <summary>
        /// Returns the row mask for the given row index.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetRowMask(int row) => RowMasks[row];

        /// <summary>
        /// Returns the column mask for the given column index.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetColMask(int col) => ColMasks[col];
    }
}
