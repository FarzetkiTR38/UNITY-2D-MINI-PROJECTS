using UnityEngine;

namespace NeonGalaxy.Data
{
    /// <summary>
    /// Runtime instance of a piece during gameplay.
    /// Combines a piece definition with a color assignment.
    /// This is a plain C# class (not a MonoBehaviour or SO).
    /// </summary>
    public class PieceInstance
    {
        /// <summary>
        /// The piece shape definition.
        /// </summary>
        public PieceDefinitionSO Definition { get; private set; }

        /// <summary>
        /// Index into the board config's color palette.
        /// </summary>
        public int ColorIndex { get; private set; }

        /// <summary>
        /// Whether this piece has been placed on the board during the current batch.
        /// </summary>
        public bool IsPlaced { get; set; }

        public PieceInstance(PieceDefinitionSO definition, int colorIndex)
        {
            Definition = definition;
            ColorIndex = colorIndex;
            IsPlaced = false;
        }

        /// <summary>
        /// Shortcut to the piece's cell offsets.
        /// </summary>
        public Vector2Int[] CellOffsets => Definition.cellOffsets;

        /// <summary>
        /// Shortcut to the piece's cell count.
        /// </summary>
        public int CellCount => Definition.CellCount;
    }
}
