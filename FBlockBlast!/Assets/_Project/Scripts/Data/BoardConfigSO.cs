using System;
using UnityEngine;

namespace NeonGalaxy.Data
{
    /// <summary>
    /// Defines the visual appearance of a single block color variant.
    /// Each entry maps to a color index used by pieces.
    /// </summary>
    [Serializable]
    public struct BlockSkin
    {
        [Tooltip("The sprite used for this block color. Assign your colored block sprite here.")]
        public Sprite sprite;

        [Tooltip("Optional tint applied on top of the sprite. Use White (default) to show the sprite's original colors.")]
        public Color tintColor;
    }

    /// <summary>
    /// Configuration for the puzzle board: dimensions, visual sizing, and block skin palette.
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

        [Header("Block Skins")]
        [Tooltip("Sprite + tint for each block color. Index matches the color ID assigned to pieces.")]
        public BlockSkin[] blockSkins = new BlockSkin[]
        {
            new BlockSkin { sprite = null, tintColor = Color.white },  // 0: Cyan / Electric Blue
            new BlockSkin { sprite = null, tintColor = Color.white },  // 1: Purple / Violet
            new BlockSkin { sprite = null, tintColor = Color.white },  // 2: Hot Pink / Magenta
            new BlockSkin { sprite = null, tintColor = Color.white },  // 3: Neon Green
            new BlockSkin { sprite = null, tintColor = Color.white },  // 4: Orange / Amber
            new BlockSkin { sprite = null, tintColor = Color.white },  // 5: Neon Yellow
        };

        /// <summary>
        /// Total number of cells on the board.
        /// </summary>
        public int TotalCells => width * height;

        /// <summary>
        /// Returns the BlockSkin at the given color index, clamped to valid range.
        /// </summary>
        public BlockSkin GetBlockSkin(int index)
        {
            if (blockSkins == null || blockSkins.Length == 0)
                return new BlockSkin { sprite = null, tintColor = Color.white };

            int safeIndex = Mathf.Clamp(index, 0, blockSkins.Length - 1);
            return blockSkins[safeIndex];
        }

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
