using UnityEngine;
using NeonGalaxy.Data;

namespace NeonGalaxy.Core
{
    /// <summary>
    /// Pure data model for the 8×8 puzzle board.
    /// Contains all gameplay logic: placement validation, line detection,
    /// line clearing, and game-over detection.
    /// No MonoBehaviour — no visual concerns. Owned by GameManager.
    /// </summary>
    public class BoardModel
    {
        // ── Board State ──────────────────────────────────────────

        /// <summary>
        /// Cell occupation state. True = occupied.
        /// Indexed as [row, col] where (0,0) is bottom-left.
        /// </summary>
        private readonly bool[,] _cells;

        /// <summary>
        /// Color index of each cell (valid only when occupied).
        /// </summary>
        private readonly int[,] _colors;

        public int Width { get; }
        public int Height { get; }

        // ── Pre-allocated scratch arrays (avoid GC in hot paths) ─
        private readonly int[] _scratchRows = new int[8];
        private readonly int[] _scratchCols = new int[8];

        // ── Constructor ──────────────────────────────────────────

        public BoardModel(int width = 8, int height = 8)
        {
            Width = width;
            Height = height;
            _cells = new bool[height, width];
            _colors = new int[height, width];
        }

        public BoardModel(BoardConfigSO config) : this(config.width, config.height) { }

        // ── Query ────────────────────────────────────────────────

        /// <summary>
        /// Returns true if the cell at (row, col) is occupied.
        /// </summary>
        public bool IsOccupied(int row, int col)
        {
            if (row < 0 || row >= Height || col < 0 || col >= Width)
                return true; // Out of bounds treated as occupied

            return _cells[row, col];
        }

        /// <summary>
        /// Returns the color index of the cell at (row, col).
        /// Only meaningful if the cell is occupied.
        /// </summary>
        public int GetColor(int row, int col)
        {
            if (row < 0 || row >= Height || col < 0 || col >= Width)
                return -1;

            return _colors[row, col];
        }

        /// <summary>
        /// Returns true if the cell is empty and in bounds.
        /// </summary>
        public bool IsEmpty(int row, int col)
        {
            if (row < 0 || row >= Height || col < 0 || col >= Width)
                return false;

            return !_cells[row, col];
        }

        // ── Piece Validation ─────────────────────────────────────

        /// <summary>
        /// Returns true if the given piece can be placed at the specified
        /// board position (all cells in bounds and landing on empty cells).
        /// </summary>
        /// <param name="cellOffsets">Piece cell offsets relative to pivot.</param>
        /// <param name="boardRow">Target row for the pivot.</param>
        /// <param name="boardCol">Target column for the pivot.</param>
        public bool CanPlacePiece(Vector2Int[] cellOffsets, int boardRow, int boardCol)
        {
            for (int i = 0; i < cellOffsets.Length; i++)
            {
                int r = boardRow + cellOffsets[i].y;
                int c = boardCol + cellOffsets[i].x;

                if (r < 0 || r >= Height || c < 0 || c >= Width)
                    return false;

                if (_cells[r, c])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Overload accepting a PieceInstance.
        /// </summary>
        public bool CanPlacePiece(PieceInstance piece, int boardRow, int boardCol)
        {
            return CanPlacePiece(piece.CellOffsets, boardRow, boardCol);
        }

        /// <summary>
        /// Returns true if the piece can be placed ANYWHERE on the board.
        /// Used for game-over detection.
        /// </summary>
        public bool HasValidPlacement(Vector2Int[] cellOffsets)
        {
            for (int r = 0; r < Height; r++)
            {
                for (int c = 0; c < Width; c++)
                {
                    if (CanPlacePiece(cellOffsets, r, c))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Overload accepting a PieceInstance.
        /// </summary>
        public bool HasValidPlacement(PieceInstance piece)
        {
            return HasValidPlacement(piece.CellOffsets);
        }

        // ── Piece Placement ──────────────────────────────────────

        /// <summary>
        /// Places a piece on the board. Does NOT validate — caller must call
        /// CanPlacePiece() first.
        /// </summary>
        /// <param name="cellOffsets">Piece cell offsets.</param>
        /// <param name="boardRow">Target row for the pivot.</param>
        /// <param name="boardCol">Target column for the pivot.</param>
        /// <param name="colorIndex">Color index to assign to the cells.</param>
        public void PlacePiece(Vector2Int[] cellOffsets, int boardRow, int boardCol, int colorIndex)
        {
            for (int i = 0; i < cellOffsets.Length; i++)
            {
                int r = boardRow + cellOffsets[i].y;
                int c = boardCol + cellOffsets[i].x;

                _cells[r, c] = true;
                _colors[r, c] = colorIndex;
            }
        }

        /// <summary>
        /// Overload accepting a PieceInstance.
        /// </summary>
        public void PlacePiece(PieceInstance piece, int boardRow, int boardCol)
        {
            PlacePiece(piece.CellOffsets, boardRow, boardCol, piece.ColorIndex);
        }

        // ── Line Detection & Clearing ────────────────────────────

        /// <summary>
        /// Checks for fully filled rows and columns. Returns total lines found.
        /// Populates output arrays with indices of full rows and columns.
        /// </summary>
        public int FindFullLines(out int[] fullRows, out int rowCount, out int[] fullCols, out int colCount)
        {
            rowCount = 0;
            colCount = 0;

            // Check rows
            for (int r = 0; r < Height; r++)
            {
                bool full = true;
                for (int c = 0; c < Width; c++)
                {
                    if (!_cells[r, c])
                    {
                        full = false;
                        break;
                    }
                }
                if (full)
                    _scratchRows[rowCount++] = r;
            }

            // Check columns
            for (int c = 0; c < Width; c++)
            {
                bool full = true;
                for (int r = 0; r < Height; r++)
                {
                    if (!_cells[r, c])
                    {
                        full = false;
                        break;
                    }
                }
                if (full)
                    _scratchCols[colCount++] = c;
            }

            fullRows = _scratchRows;
            fullCols = _scratchCols;

            return rowCount + colCount;
        }

        /// <summary>
        /// Clears the specified rows and columns. Sets cells to empty.
        /// </summary>
        public void ClearLines(int[] rows, int rowCount, int[] cols, int colCount)
        {
            // Clear rows
            for (int i = 0; i < rowCount; i++)
            {
                int r = rows[i];
                for (int c = 0; c < Width; c++)
                {
                    _cells[r, c] = false;
                    _colors[r, c] = 0;
                }
            }

            // Clear columns
            for (int i = 0; i < colCount; i++)
            {
                int col = cols[i];
                for (int r = 0; r < Height; r++)
                {
                    _cells[r, col] = false;
                    _colors[r, col] = 0;
                }
            }
        }

        /// <summary>
        /// Convenience: detects and clears all full lines in one call.
        /// Returns total lines cleared and whether a Nova Cross occurred.
        /// </summary>
        public int DetectAndClearLines(out bool novaCross)
        {
            int totalLines = FindFullLines(out int[] rows, out int rowCount, out int[] cols, out int colCount);
            novaCross = rowCount > 0 && colCount > 0;

            if (totalLines > 0)
                ClearLines(rows, rowCount, cols, colCount);

            return totalLines;
        }

        // ── Bitmask Conversion ───────────────────────────────────

        /// <summary>
        /// Creates a BoardBitmask snapshot of the current board state.
        /// Used by the batch generator for fast simulation.
        /// </summary>
        public BoardBitmask ToBitmask()
        {
            ulong mask = 0;
            for (int r = 0; r < Height && r < 8; r++)
            {
                for (int c = 0; c < Width && c < 8; c++)
                {
                    if (_cells[r, c])
                        mask |= (1UL << (r * 8 + c));
                }
            }
            return new BoardBitmask(mask);
        }

        // ── Board Reset ──────────────────────────────────────────

        /// <summary>
        /// Clears the entire board.
        /// </summary>
        public void Reset()
        {
            for (int r = 0; r < Height; r++)
            {
                for (int c = 0; c < Width; c++)
                {
                    _cells[r, c] = false;
                    _colors[r, c] = 0;
                }
            }
        }

        // ── Utility ──────────────────────────────────────────────

        /// <summary>
        /// Counts the total number of occupied cells.
        /// </summary>
        public int CountOccupied()
        {
            int count = 0;
            for (int r = 0; r < Height; r++)
            {
                for (int c = 0; c < Width; c++)
                {
                    if (_cells[r, c]) count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Returns the occupancy ratio (0.0 = empty, 1.0 = full).
        /// </summary>
        public float GetOccupancyRatio()
        {
            return (float)CountOccupied() / (Width * Height);
        }

        /// <summary>
        /// Clears the bottom N rows (used for revive mechanic).
        /// </summary>
        public void ClearBottomRows(int rowCount)
        {
            for (int r = 0; r < rowCount && r < Height; r++)
            {
                for (int c = 0; c < Width; c++)
                {
                    _cells[r, c] = false;
                    _colors[r, c] = 0;
                }
            }
        }

        /// <summary>
        /// Clears the N rows with the most occupied cells.
        /// Used by the revive system to maximize breathing room.
        /// Returns the indices of the cleared rows.
        /// </summary>
        public int[] ClearFullestRows(int count)
        {
            // Score each row by occupancy
            int[] rowScores = new int[Height];
            for (int r = 0; r < Height; r++)
            {
                int occupied = 0;
                for (int c = 0; c < Width; c++)
                {
                    if (_cells[r, c]) occupied++;
                }
                rowScores[r] = occupied;
            }

            // Find the N fullest rows
            int[] selectedRows = new int[Mathf.Min(count, Height)];
            for (int i = 0; i < selectedRows.Length; i++)
            {
                int bestRow = -1;
                int bestScore = -1;
                for (int r = 0; r < Height; r++)
                {
                    if (rowScores[r] > bestScore)
                    {
                        bestScore = rowScores[r];
                        bestRow = r;
                    }
                }

                if (bestRow >= 0 && bestScore > 0)
                {
                    selectedRows[i] = bestRow;
                    rowScores[bestRow] = -1; // Exclude from future selection
                }
                else
                {
                    selectedRows[i] = i; // Fallback to sequential rows
                }
            }

            // Clear selected rows
            for (int i = 0; i < selectedRows.Length; i++)
            {
                int r = selectedRows[i];
                for (int c = 0; c < Width; c++)
                {
                    _cells[r, c] = false;
                    _colors[r, c] = 0;
                }
            }

            return selectedRows;
        }
    }
}
