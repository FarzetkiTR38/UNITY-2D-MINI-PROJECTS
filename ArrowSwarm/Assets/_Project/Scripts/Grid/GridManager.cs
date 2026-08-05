namespace ArrowSwarm.Grid
{
    using System;
    using System.Collections.Generic;

    using ArrowSwarm.Core;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Manages the point-based game grid. Creates points at intersections,
    /// tracks arrow placement across multiple points, and provides
    /// edge-based queries for arrow click validation.
    /// </summary>
    public class GridManager : Singleton<GridManager>
    {
        private GridPoint[,] _points;
        private int _width;
        private int _height;
        private float _pointSpacing;
        private Vector2 _origin;

        /// <summary>Grid width (number of point columns).</summary>
        public int Width => _width;

        /// <summary>Grid height (number of point rows).</summary>
        public int Height => _height;

        /// <summary>Distance between adjacent points in world units.</summary>
        public float PointSpacing => _pointSpacing;

        /// <summary>Grid origin (bottom-left point) in world space.</summary>
        public Vector2 Origin => _origin;

        /// <summary>Fired when the grid is initialized.</summary>
        public static event Action<int, int> OnGridInitialized;

        /// <summary>Fired when a point's occupancy changes.</summary>
        public static event Action<Vector2Int, bool> OnPointOccupancyChanged;

        /// <summary>
        /// Initializes the point grid from MapData.
        /// </summary>
        public void InitializeGrid(MapData mapData)
        {
            _width = mapData.GridWidth;
            _height = mapData.GridHeight;
            _pointSpacing = mapData.PointSpacing;
            _origin = mapData.GridOrigin;

            _points = new GridPoint[_width, _height];

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Vector2Int gridPos = new Vector2Int(x, y);
                    Vector2 worldPos = gridPos.PointToWorld(_pointSpacing, _origin);
                    _points[x, y] = new GridPoint(gridPos, worldPos);
                }
            }

            OnGridInitialized?.Invoke(_width, _height);
            LogDebug($"Grid initialized: {_width}x{_height}, Spacing={_pointSpacing}");
        }

        /// <summary>
        /// Gets the point at the given grid position. Returns null if out of bounds.
        /// </summary>
        public GridPoint GetPoint(int x, int y)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height) return null;
            return _points[x, y];
        }

        /// <summary>
        /// Gets the point at the given grid position.
        /// </summary>
        public GridPoint GetPoint(Vector2Int pos)
        {
            return GetPoint(pos.x, pos.y);
        }

        /// <summary>
        /// Places an arrow across multiple points on the grid.
        /// All points in the arrow's path are marked as occupied.
        /// </summary>
        public void PlaceArrowOnPoints(List<Vector2Int> pathPoints, ArrowSwarm.Arrow.Arrow arrow)
        {
            if (pathPoints == null) return;

            for (int i = 0; i < pathPoints.Count; i++)
            {
                GridPoint point = GetPoint(pathPoints[i]);
                if (point == null) continue;

                point.IsOccupied = true;
                point.OccupyingArrow = arrow;
                OnPointOccupancyChanged?.Invoke(pathPoints[i], true);
            }
        }

        /// <summary>
        /// Removes an arrow from all its occupied points.
        /// </summary>
        public void RemoveArrowFromPoints(List<Vector2Int> pathPoints)
        {
            if (pathPoints == null) return;

            for (int i = 0; i < pathPoints.Count; i++)
            {
                GridPoint point = GetPoint(pathPoints[i]);
                if (point == null) continue;

                point.Clear();
                OnPointOccupancyChanged?.Invoke(pathPoints[i], false);
            }
        }

        /// <summary>
        /// Checks if a specific point is occupied by an arrow.
        /// </summary>
        public bool IsPointOccupied(Vector2Int pos)
        {
            GridPoint point = GetPoint(pos);
            return point != null && point.IsOccupied;
        }

        /// <summary>
        /// Checks if a point is on the edge of the grid.
        /// </summary>
        public bool IsEdgePoint(Vector2Int pos)
        {
            return pos.IsEdge(_width, _height);
        }

        /// <summary>
        /// Checks if an arrow's path is clear in the direction it faces,
        /// all the way to the grid edge. (No other arrows blocking it)
        /// </summary>
        public bool IsPathClear(Vector2Int headPoint, ArrowSwarm.Arrow.ArrowDirection headDir)
        {
            Vector2Int step = DirectionToVector(headDir);
            Vector2Int current = headPoint + step;
            
            while (current.IsInBounds(_width, _height))
            {
                GridPoint point = GetPoint(current);
                if (point != null && point.IsOccupied)
                {
                    return false;
                }
                current += step;
            }
            return true;
        }

        /// <summary>
        /// Gets the world position of a grid exit point (on the surrounding enemy path rectangle).
        /// </summary>
        public Vector2 GetGridExitPoint(Vector2Int headPoint, ArrowSwarm.Arrow.ArrowDirection direction)
        {
            Vector2Int exit = headPoint;
            switch(direction)
            {
                case ArrowSwarm.Arrow.ArrowDirection.Up: exit.y = _height; break;
                case ArrowSwarm.Arrow.ArrowDirection.Down: exit.y = -1; break;
                case ArrowSwarm.Arrow.ArrowDirection.Left: exit.x = -1; break;
                case ArrowSwarm.Arrow.ArrowDirection.Right: exit.x = _width; break;
            }
            return exit.PointToWorld(_pointSpacing, _origin);
        }

        /// <summary>
        /// Converts a world position to the nearest grid point.
        /// Returns null if out of bounds.
        /// </summary>
        public GridPoint WorldToPoint(Vector2 worldPos)
        {
            Vector2Int gridPos = worldPos.WorldToPoint(_pointSpacing, _origin);
            return GetPoint(gridPos);
        }

        /// <summary>
        /// Clears all points, removing all arrow references.
        /// </summary>
        public void ClearGrid()
        {
            if (_points == null) return;

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _points[x, y].Clear();
                }
            }
        }

        /// <summary>
        /// Converts an ArrowDirection to a Vector2Int step.
        /// </summary>
        public static Vector2Int DirectionToVector(ArrowSwarm.Arrow.ArrowDirection direction)
        {
            return direction switch
            {
                ArrowSwarm.Arrow.ArrowDirection.Up => Vector2Int.up,
                ArrowSwarm.Arrow.ArrowDirection.Down => Vector2Int.down,
                ArrowSwarm.Arrow.ArrowDirection.Left => Vector2Int.left,
                ArrowSwarm.Arrow.ArrowDirection.Right => Vector2Int.right,
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
