using System;
using System.Collections.Generic;
using UnityEngine;
using NeonGalaxy.Data;

namespace NeonGalaxy.Core
{
    /// <summary>
    /// Renders and manages the visual grid of the 8x8 Board.
    /// Converts board coordinates between world-space and grid-space.
    /// Animates line clears and grid state updates.
    /// </summary>
    public class BoardController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private BoardConfigSO config;

        [Header("Prefabs")]
        [SerializeField] private CellView cellPrefab;

        private CellView[,] _cellViews;
        private int _width;
        private int _height;
        private float _cellSize;
        private float _cellSpacing;
        private float _halfWidth;
        private float _halfHeight;

        private void Awake()
        {
            if (config != null)
            {
                Initialize(config);
            }
        }

        /// <summary>
        /// Initializes the board layout using dimensions and offsets from BoardConfigSO.
        /// </summary>
        public void Initialize(BoardConfigSO boardConfig)
        {
            config = boardConfig;
            _width = config.width;
            _height = config.height;
            _cellSize = config.cellSize;
            _cellSpacing = config.cellSpacing;

            // Calculate overall board dimensions to center it around this transform's position
            float totalWidth = _width * _cellSize + (_width - 1) * _cellSpacing;
            float totalHeight = _height * _cellSize + (_height - 1) * _cellSpacing;
            _halfWidth = totalWidth / 2f;
            _halfHeight = totalHeight / 2f;

            // Clean up existing cells if any (e.g. during re-initialization or restart)
            if (_cellViews != null)
            {
                for (int r = 0; r < _height; r++)
                {
                    for (int c = 0; c < _width; c++)
                    {
                        if (_cellViews[r, c] != null)
                        {
                            Destroy(_cellViews[r, c].gameObject);
                        }
                    }
                }
            }

            _cellViews = new CellView[_height, _width];

            // Spawn the grid of CellViews
            for (int r = 0; r < _height; r++)
            {
                for (int c = 0; c < _width; c++)
                {
                    Vector3 localPos = GetLocalCellPosition(r, c);
                    CellView cell = Instantiate(cellPrefab, transform);
                    cell.transform.localPosition = localPos;
                    cell.transform.localScale = new Vector3(_cellSize, _cellSize, 1f);
                    cell.name = $"Cell_{r}_{c}";
                    cell.SetEmpty();
                    _cellViews[r, c] = cell;
                }
            }
        }

        /// <summary>
        /// Syncs the cell visuals with the data inside the BoardModel.
        /// </summary>
        public void RefreshBoard(BoardModel boardModel)
        {
            if (_cellViews == null)
            {
                Initialize(config);
            }

            for (int r = 0; r < _height; r++)
            {
                for (int c = 0; c < _width; c++)
                {
                    CellView cell = _cellViews[r, c];
                    if (cell == null) continue;

                    if (boardModel.IsOccupied(r, c))
                    {
                        int colorIdx = boardModel.GetColor(r, c);
                        BlockSkin skin = config.GetBlockSkin(colorIdx);
                        cell.SetOccupied(skin.sprite, skin.tintColor);
                    }
                    else
                    {
                        cell.SetEmpty();
                    }
                }
            }
        }

        /// <summary>
        /// Translates a grid position (row, col) to a world-space position.
        /// </summary>
        public Vector3 GridToWorld(int row, int col)
        {
            Vector3 localPos = GetLocalCellPosition(row, col);
            return transform.TransformPoint(localPos);
        }

        /// <summary>
        /// Converts a world-space position to the closest grid position (row, col).
        /// Returns true if the position is within the board bounds.
        /// </summary>
        public bool WorldToGrid(Vector3 worldPos, out Vector2Int gridPos)
        {
            Vector3 localPos = transform.InverseTransformPoint(worldPos);
            float totalCell = _cellSize + _cellSpacing;

            // Invert the layout position formulas to find the grid indexes
            int col = Mathf.RoundToInt((localPos.x + _halfWidth - _cellSize / 2f) / totalCell);
            int row = Mathf.RoundToInt((localPos.y + _halfHeight - _cellSize / 2f) / totalCell);

            gridPos = new Vector2Int(col, row); // x = col, y = row
            return IsValidGridPos(gridPos);
        }

        /// <summary>
        /// Checks if the grid position is within board bounds.
        /// </summary>
        public bool IsValidGridPos(Vector2Int gridPos)
        {
            return gridPos.x >= 0 && gridPos.x < _width && gridPos.y >= 0 && gridPos.y < _height;
        }

        /// <summary>
        /// Returns the CellView at (row, col). Returns null if coordinates are out of bounds.
        /// </summary>
        public CellView GetCellView(int row, int col)
        {
            if (row < 0 || row >= _height || col < 0 || col >= _width)
                return null;
            return _cellViews[row, col];
        }

        /// <summary>
        /// Triggers visual clear animations for the specified rows and columns.
        /// Sweeps row clear left-to-right, column clear bottom-to-top, and Nova Cross outward.
        /// </summary>
        public void AnimateLineClear(int[] rows, int rowCount, int[] cols, int colCount, Action onComplete)
        {
            var cellsToAnimate = new HashSet<Vector2Int>();

            for (int i = 0; i < rowCount; i++)
            {
                int r = rows[i];
                for (int c = 0; c < _width; c++)
                {
                    cellsToAnimate.Add(new Vector2Int(c, r));
                }
            }

            for (int i = 0; i < colCount; i++)
            {
                int c = cols[i];
                for (int r = 0; r < _height; r++)
                {
                    cellsToAnimate.Add(new Vector2Int(c, r));
                }
            }

            if (cellsToAnimate.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            int remainingAnimations = cellsToAnimate.Count;

            foreach (var pos in cellsToAnimate)
            {
                CellView cell = GetCellView(pos.y, pos.x);
                if (cell != null)
                {
                    // Calculate sweeping visual delay
                    float delay = 0f;
                    if (rowCount > 0 && colCount > 0)
                    {
                        // Nova Cross: clear starts from center/intersection outward
                        delay = Mathf.Min(pos.x, pos.y) * 0.03f;
                    }
                    else if (rowCount > 0)
                    {
                        delay = pos.x * 0.03f; // Row clear: sweeps left-to-right
                    }
                    else
                    {
                        delay = pos.y * 0.03f; // Column clear: sweeps bottom-to-top
                    }

                    cell.PlayClearAnimation(delay, () =>
                    {
                        remainingAnimations--;
                        if (remainingAnimations == 0)
                        {
                            onComplete?.Invoke();
                        }
                    });
                }
                else
                {
                    remainingAnimations--;
                    if (remainingAnimations == 0)
                    {
                        onComplete?.Invoke();
                    }
                }
            }
        }

        private Vector3 GetLocalCellPosition(int row, int col)
        {
            float posX = -_halfWidth + col * (_cellSize + _cellSpacing) + _cellSize / 2f;
            float posY = -_halfHeight + row * (_cellSize + _cellSpacing) + _cellSize / 2f;
            return new Vector3(posX, posY, 0f);
        }

        private Color GetColorFromPalette(int index)
        {
            return config.GetBlockSkin(index).tintColor;
        }
    }
}
