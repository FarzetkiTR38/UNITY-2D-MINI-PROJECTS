namespace ArrowSwarm.Grid
{
    using System;
    using System.Collections.Generic;

    using ArrowSwarm.Core;
    using ArrowSwarm.Path;
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
            if (GetComponent<GridVisualizer>() == null) gameObject.AddComponent<GridVisualizer>();
            if (GetComponent<MapContainerVisualizer>() == null) gameObject.AddComponent<MapContainerVisualizer>();

            _width = mapData.GridWidth;
            _height = mapData.GridHeight;
            _pointSpacing = (mapData != null && mapData.PointSpacing > 0) ? mapData.PointSpacing : 1.0f;

            // Calculate origin to center perfectly around (0, -0.5f) to leave room for top HUD
            float totalWidth = (_width - 1) * _pointSpacing;
            float totalHeight = (_height - 1) * _pointSpacing;
            
            _origin = new Vector2(-totalWidth / 2f, (-totalHeight / 2f) - 0.5f);

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

            // Direct calls ensure immediate rendering even if event subscriptions
            // haven't been set up yet (first click scenario)
            MapContainerVisualizer containerVis = GetComponent<MapContainerVisualizer>();
            if (containerVis != null)
            {
                containerVis.BuildThreeLayerTheme(_width, _height);
            }

            GridVisualizer gridVis = GetComponent<GridVisualizer>();
            if (gridVis != null)
            {
                gridVis.DrawPointGrid();
            }

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
            float s = _pointSpacing;
            Vector2 headWorld = headPoint.PointToWorld(s, _origin);
            float pathOffsetMult = PathManager.Instance?.PathOffsetMultiplier ?? 1.35f;

            float gridMinX = _origin.x;
            float gridMaxX = _origin.x + (_width - 1) * s;
            float gridMinY = _origin.y;
            float gridMaxY = _origin.y + (_height - 1) * s;

            switch (direction)
            {
                case ArrowSwarm.Arrow.ArrowDirection.Up:
                    return new Vector2(headWorld.x, gridMaxY + pathOffsetMult * s);
                case ArrowSwarm.Arrow.ArrowDirection.Down:
                    return new Vector2(headWorld.x, gridMinY - pathOffsetMult * s);
                case ArrowSwarm.Arrow.ArrowDirection.Left:
                    return new Vector2(gridMinX - pathOffsetMult * s, headWorld.y);
                case ArrowSwarm.Arrow.ArrowDirection.Right:
                    return new Vector2(gridMaxX + pathOffsetMult * s, headWorld.y);
                default:
                    return headWorld;
            }
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

        /// <summary>
        /// Gets the standard point spacing for grid placement (default: 1.0f).
        /// </summary>
        public static float CalculatePointSpacing(int width, int height, float orthoSize = 5.0f, float aspect = 0.5625f)
        {
            return 1.0f;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] GridManager: {message}");
        }
    }
}
