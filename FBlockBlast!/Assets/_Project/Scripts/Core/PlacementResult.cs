using System;

namespace NeonGalaxy.Core
{
    /// <summary>
    /// Data-transfer object summarizing the outcomes of placing a piece.
    /// Tracks cleared lines, Nova Cross occurrences, and coordinates.
    /// </summary>
    public struct PlacementResult
    {
        public int LinesCleared;
        public bool NovaCross;
        public int[] ClearedRows;
        public int RowCount;
        public int[] ClearedCols;
        public int ColCount;

        public PlacementResult(int linesCleared, bool novaCross, int[] clearedRows, int rowCount, int[] clearedCols, int colCount)
        {
            LinesCleared = linesCleared;
            NovaCross = novaCross;
            
            // Copy data from transient buffers to prevent scratch overwrite bugs
            RowCount = rowCount;
            ClearedRows = new int[rowCount];
            if (rowCount > 0 && clearedRows != null)
            {
                Array.Copy(clearedRows, ClearedRows, rowCount);
            }

            ColCount = colCount;
            ClearedCols = new int[colCount];
            if (colCount > 0 && clearedCols != null)
            {
                Array.Copy(clearedCols, ClearedCols, colCount);
            }
        }
    }
}
