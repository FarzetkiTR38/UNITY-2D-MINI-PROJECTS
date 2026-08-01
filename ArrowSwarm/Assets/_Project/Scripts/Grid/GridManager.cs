namespace ArrowSwarm.Grid
{
    using System;
    using ArrowSwarm.Core;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Manages the game grid — creates cells, tracks arrow placement,
    /// and provides grid-related queries (e.g., is path clear).
    /// </summary>
    public class GridManager : Singleton<GridManager>
    {
        private GridCell[,] _cells;
        private int _width;
        private int _height;
        private float _cellSize;
        private Vector2 _origin;

        /// <summary>Grid width (columns).</summary>
        public int Width => _width;

        /// <summary>Grid height (rows).</summary>
        public int Height => _height;

        /// <summary>Cell size in world units.</summary>
        public float CellSize => _cellSize;

        /// <summary>Grid origin (bottom-left corner) in world space.</summary>
        public Vector2 Origin => _origin;

        /// <summary>Fired when the grid is initialized.</summary>
        public static event Action<int, int> OnGridInitialized;

        /// <summary>Fired when a cell's occupancy changes.</summary>
        public static event Action<Vector2Int, bool> OnCellOccupancyChanged;

        /// <summary>
        /// Initializes the grid from MapData.
        /// </summary>
        public void InitializeGrid(MapData mapData)
        {
            _width = mapData.GridWidth;
            _height = mapData.GridHeight;
            _cellSize = mapData.CellSize;
            _origin = mapData.GridOrigin;

            _cells = new GridCell[_width, _height];

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Vector2Int gridPos = new Vector2Int(x, y);
                    Vector2 worldPos = gridPos.GridToWorld(_cellSize, _origin);
                    _cells[x, y] = new GridCell(gridPos, worldPos);
                }
            }

            OnGridInitialized?.Invoke(_width, _height);
            LogDebug($"Grid initialized: {_width}x{_height}, CellSize={_cellSize}");
        }

        /// <summary>
        /// Gets the cell at the given grid position. Returns null if out of bounds.
        /// </summary>
        public GridCell GetCell(int x, int y)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height) return null;
            return _cells[x, y];
        }

        /// <summary>
        /// Gets the cell at the given grid position.
        /// </summary>
        public GridCell GetCell(Vector2Int pos)
        {
            return GetCell(pos.x, pos.y);
        }

        /// <summary>
        /// Places an arrow in the specified cell.
        /// </summary>
        public void PlaceArrow(Vector2Int pos, Arrow.Arrow arrow)
        {
            GridCell cell = GetCell(pos);
            if (cell == null) return;

            cell.IsOccupied = true;
            cell.OccupyingArrow = arrow;
            OnCellOccupancyChanged?.Invoke(pos, true);
        }

        /// <summary>
        /// Removes an arrow from the specified cell.
        /// </summary>
        public void RemoveArrow(Vector2Int pos)
        {
            GridCell cell = GetCell(pos);
            if (cell == null) return;

            cell.Clear();
            OnCellOccupancyChanged?.Invoke(pos, false);
        }

        /// <summary>
        /// Checks if the path from a grid position in the given direction
        /// is clear (no other arrows blocking) all the way to the grid edge.
        /// </summary>
        public bool IsPathClear(Vector2Int from, Arrow.ArrowDirection direction)
        {
            Vector2Int step = DirectionToVector(direction);
            Vector2Int current = from + step;

            while (current.IsInBounds(_width, _height))
            {
                GridCell cell = GetCell(current);
                if (cell != null && cell.IsOccupied)
                {
                    return false;
                }
                current += step;
            }

            return true;
        }

        /// <summary>
        /// Converts a world position to the nearest grid cell.
        /// Returns null if out of bounds.
        /// </summary>
        public GridCell WorldToCell(Vector2 worldPos)
        {
            Vector2Int gridPos = worldPos.WorldToGrid(_cellSize, _origin);
            return GetCell(gridPos);
        }

        /// <summary>
        /// Clears all cells, removing all arrow references.
        /// </summary>
        public void ClearGrid()
        {
            if (_cells == null) return;

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _cells[x, y].Clear();
                }
            }
        }

        /// <summary>
        /// Gets the world position where an arrow exits the grid
        /// from a given position in a given direction.
        /// </summary>
        public Vector2 GetGridExitPoint(Vector2Int from, Arrow.ArrowDirection direction)
        {
            Vector2Int step = DirectionToVector(direction);
            Vector2Int current = from;

            while ((current + step).IsInBounds(_width, _height))
            {
                current += step;
            }

            // One step beyond the grid edge
            Vector2 exitWorld = current.GridToWorld(_cellSize, _origin);
            exitWorld += new Vector2(step.x, step.y) * _cellSize;
            return exitWorld;
        }

        /// <summary>
        /// Converts an ArrowDirection to a Vector2Int step.
        /// </summary>
        public static Vector2Int DirectionToVector(Arrow.ArrowDirection direction)
        {
            return direction switch
            {
                Arrow.ArrowDirection.Up => Vector2Int.up,
                Arrow.ArrowDirection.Down => Vector2Int.down,
                Arrow.ArrowDirection.Left => Vector2Int.left,
                Arrow.ArrowDirection.Right => Vector2Int.right,
                _ => Vector2Int.zero
            };
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] GridManager: {message}");
        }
    }
}
