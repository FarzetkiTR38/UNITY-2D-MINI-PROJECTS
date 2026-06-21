using UnityEngine;

namespace NeonGalaxy.Data
{
    /// <summary>
    /// Defines the shape and metadata of a single puzzle piece.
    /// Each piece is a collection of cell offsets relative to a pivot at (0,0).
    /// Create instances via: Create → NeonGalaxy → Piece Definition.
    /// </summary>
    [CreateAssetMenu(fileName = "NewPiece", menuName = "NeonGalaxy/Piece Definition", order = 0)]
    public class PieceDefinitionSO : ScriptableObject
    {
        [Tooltip("Unique identifier for this piece (e.g., 'tetromino_t', 'pentomino_cross')")]
        public string pieceId;

        [Tooltip("Display name shown in debug/editor (e.g., 'T-Block', 'Cross')")]
        public string displayName;

        [Tooltip("Cell positions relative to pivot (0,0). Defines the shape of the piece.")]
        public Vector2Int[] cellOffsets;

        [Tooltip("Size category for spawn weight and batch generator heuristics.")]
        public PieceCategory category;

        [Tooltip("Base weight for random selection. 1.0 = normal frequency.")]
        [Range(0.1f, 3.0f)]
        public float spawnWeight = 1.0f;

        [Tooltip("Optional preview sprite override for the piece tray. If null, auto-generated from cells.")]
        public Sprite previewSprite;

        /// <summary>
        /// Number of cells in this piece.
        /// </summary>
        public int CellCount => cellOffsets != null ? cellOffsets.Length : 0;

        /// <summary>
        /// Computes the bounding box of this piece's cell offsets.
        /// </summary>
        public RectInt GetBounds()
        {
            if (cellOffsets == null || cellOffsets.Length == 0)
                return new RectInt(0, 0, 0, 0);

            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;

            for (int i = 0; i < cellOffsets.Length; i++)
            {
                var offset = cellOffsets[i];
                if (offset.x < minX) minX = offset.x;
                if (offset.y < minY) minY = offset.y;
                if (offset.x > maxX) maxX = offset.x;
                if (offset.y > maxY) maxY = offset.y;
            }

            return new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(pieceId))
                pieceId = name;
        }
#endif
    }
}
