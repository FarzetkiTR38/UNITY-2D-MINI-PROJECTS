using System;

namespace NeonGalaxy.Data
{
    /// <summary>
    /// Serializable snapshot of an active gameplay run.
    /// Stored inside SaveData to allow resume-from-where-you-left-off
    /// when the application is killed or minimized mid-game.
    /// </summary>
    [Serializable]
    public class RunStateData
    {
        /// <summary>
        /// True if there is a valid in-progress run to resume.
        /// Set to false on Game Over or new game start.
        /// </summary>
        public bool hasActiveRun = false;

        // ── Board State (8×8 = 64 cells, flattened) ─────────────
        public bool[] cellOccupied = new bool[64];
        public int[] cellColors = new int[64];

        // ── Score & Combo ────────────────────────────────────────
        public int totalScore;
        public int currentCombo;
        public int batchLinesCleared;
        public bool batchHadNovaCross;

        // ── Run Statistics ───────────────────────────────────────
        public int runLinesCleared;
        public int runBestCombo;
        public int revivesUsedThisRun;

        // ── Tray Pieces (3 slots) ────────────────────────────────
        /// <summary>
        /// PieceDefinitionSO.pieceId for each tray slot.
        /// Empty string means no piece in that slot.
        /// </summary>
        public string[] trayPieceDefinitionIds = new string[3] { "", "", "" };

        /// <summary>
        /// Color index for each tray slot piece.
        /// </summary>
        public int[] trayPieceColorIndices = new int[3];

        /// <summary>
        /// Whether each tray slot piece has already been placed.
        /// </summary>
        public bool[] trayPiecePlaced = new bool[3];

        /// <summary>
        /// Resets all fields to defaults (no active run).
        /// </summary>
        public void Clear()
        {
            hasActiveRun = false;
            totalScore = 0;
            currentCombo = 0;
            batchLinesCleared = 0;
            batchHadNovaCross = false;
            runLinesCleared = 0;
            runBestCombo = 0;
            revivesUsedThisRun = 0;

            for (int i = 0; i < 64; i++)
            {
                cellOccupied[i] = false;
                cellColors[i] = 0;
            }

            for (int i = 0; i < 3; i++)
            {
                trayPieceDefinitionIds[i] = "";
                trayPieceColorIndices[i] = 0;
                trayPiecePlaced[i] = false;
            }
        }
    }
}
