namespace ArrowSwarm.Grid
{
    using UnityEngine;

    /// <summary>
    /// Represents a single cell in the game grid.
    /// Holds position data and tracks whether an arrow occupies it.
    /// </summary>
    [System.Serializable]
    public class GridCell
    {
        [SerializeField] private Vector2Int _gridPosition;
        [SerializeField] private Vector2 _worldPosition;
        [SerializeField] private bool _isOccupied;

        /// <summary>Grid coordinate (col, row).</summary>
        public Vector2Int GridPosition => _gridPosition;

        /// <summary>World-space center position of this cell.</summary>
        public Vector2 WorldPosition => _worldPosition;

        /// <summary>Whether an arrow currently occupies this cell.</summary>
        public bool IsOccupied
        {
            get => _isOccupied;
            set => _isOccupied = value;
        }

        /// <summary>
        /// Reference to the Arrow in this cell (null if empty).
        /// </summary>
        public Arrow.Arrow OccupyingArrow { get; set; }

        /// <summary>
        /// Creates a new GridCell at the specified position.
        /// </summary>
        public GridCell(Vector2Int gridPos, Vector2 worldPos)
        {
            _gridPosition = gridPos;
            _worldPosition = worldPos;
            _isOccupied = false;
            OccupyingArrow = null;
        }

        /// <summary>
        /// Clears the cell, removing any arrow reference.
        /// </summary>
        public void Clear()
        {
            _isOccupied = false;
            OccupyingArrow = null;
        }
    }
}
