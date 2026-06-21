using UnityEngine;

namespace NeonGalaxy.Data
{
    /// <summary>
    /// Configuration for the puzzle board: dimensions, visual sizing, and block color palette.
    /// Create instances via: Create → NeonGalaxy → Board Config.
    /// </summary>
    [CreateAssetMenu(fileName = "BoardConfig", menuName = "NeonGalaxy/Board Config", order = 10)]
    public class BoardConfigSO : ScriptableObject
    {
        [Header("Board Dimensions")]
        [Tooltip("Number of columns.")]
        public int width = 8;

        [Tooltip("Number of rows.")]
        public int height = 8;

        [Header("Visual")]
        [Tooltip("World-space size of each cell in units.")]
        public float cellSize = 1.0f;

        [Tooltip("Spacing between cells in units.")]
        public float cellSpacing = 0.05f;

        [Header("Block Palette")]
        [Tooltip("Colors assigned to pieces. Index is used as color ID.")]
        public Color[] blockPalette = new Color[]
        {
            new Color(0.0f, 0.9f, 1.0f, 1.0f),   // Cyan / Electric Blue
            new Color(0.6f, 0.0f, 1.0f, 1.0f),   // Purple / Violet
            new Color(1.0f, 0.2f, 0.6f, 1.0f),   // Hot Pink / Magenta
            new Color(0.0f, 1.0f, 0.5f, 1.0f),   // Neon Green
            new Color(1.0f, 0.6f, 0.0f, 1.0f),   // Orange / Amber
            new Color(1.0f, 1.0f, 0.0f, 1.0f),   // Neon Yellow
        };

        /// <summary>
        /// Total number of cells on the board.
        /// </summary>
        public int TotalCells => width * height;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (width < 4) width = 4;
            if (width > 12) width = 12;
            if (height < 4) height = 4;
            if (height > 12) height = 12;
            if (cellSize < 0.1f) cellSize = 0.1f;
            if (cellSpacing < 0f) cellSpacing = 0f;
        }
#endif
    }
}
