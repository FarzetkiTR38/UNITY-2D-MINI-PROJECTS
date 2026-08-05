namespace ArrowSwarm.Grid
{
    using UnityEngine;

    /// <summary>
    /// Represents a single point (intersection) on the game grid.
    /// Holds position data and tracks whether an arrow occupies this point.
    /// </summary>
    [System.Serializable]
    public class GridPoint
    {
        [SerializeField] private Vector2Int _gridPosition;
        [SerializeField] private Vector2 _worldPosition;
        [SerializeField] private bool _isOccupied;

        /// <summary>Grid coordinate (col, row).</summary>
        public Vector2Int GridPosition => _gridPosition;

        /// <summary>World-space position of this point.</summary>
        public Vector2 WorldPosition => _worldPosition;

        /// <summary>Whether an arrow currently occupies this point.</summary>
        public bool IsOccupied
        {
            get => _isOccupied;
            set => _isOccupied = value;
        }

        /// <summary>
        /// Reference to the Arrow using this point (null if empty).
        /// </summary>
        public Arrow.Arrow OccupyingArrow { get; set; }

        /// <summary>
        /// Creates a new GridPoint at the specified position.
        /// </summary>
        public GridPoint(Vector2Int gridPos, Vector2 worldPos)
        {
            _gridPosition = gridPos;
            _worldPosition = worldPos;
            _isOccupied = false;
            OccupyingArrow = null;
        }

        /// <summary>
        /// Clears the point, removing any arrow reference.
        /// </summary>
        public void Clear()
        {
            _isOccupied = false;
            OccupyingArrow = null;
        }
    }
}
