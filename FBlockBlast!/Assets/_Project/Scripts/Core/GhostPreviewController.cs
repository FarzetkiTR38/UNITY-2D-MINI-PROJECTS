using System.Collections.Generic;
using UnityEngine;
using NeonGalaxy.Data;

namespace NeonGalaxy.Core
{
    /// <summary>
    /// Displays a semi-transparent visual preview of the dragging piece snapped to the board.
    /// Tints green/valid or red/invalid depending on collision and boundary checks.
    /// Uses a pre-allocated pool of block sprites to avoid GC allocations during drag.
    /// </summary>
    public class GhostPreviewController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private BoardConfigSO boardConfig;
        [SerializeField] private BoardController boardController;

        [Header("Visuals")]
        [SerializeField] private Sprite ghostBlockSprite;
        [SerializeField] private float boardScale = 0.9f;
        [SerializeField] private Color validTint = new Color(0.2f, 1.0f, 0.5f, 0.45f); // Neon Green with alpha
        [SerializeField] private Color invalidTint = new Color(1.0f, 0.2f, 0.2f, 0.45f); // Neon Red with alpha

        private const int MaxGhostCells = 9; // Large piece has at most 9 cells
        private readonly List<SpriteRenderer> _cellPool = new List<SpriteRenderer>();
        private PieceInstance _activePiece;
        private bool _isShowing;

        private void Awake()
        {
            InitializePool();
        }

        private void InitializePool()
        {
            // Pre-allocate the ghost block visual pool
            for (int i = 0; i < MaxGhostCells; i++)
            {
                GameObject cellObj = new GameObject($"GhostCell_{i}");
                cellObj.transform.SetParent(transform);
                cellObj.transform.localPosition = Vector3.zero;

                SpriteRenderer sr = cellObj.AddComponent<SpriteRenderer>();
                sr.sprite = ghostBlockSprite;
                sr.sortingLayerName = "Board Blocks";
                sr.sortingOrder = 5; // Layered between Board Background (0) and Dragging Piece (10)
                cellObj.SetActive(false);

                _cellPool.Add(sr);
            }
        }

        /// <summary>
        /// Activates the ghost cells matching the shape of the dragging piece.
        /// </summary>
        public void Show(PieceInstance piece)
        {
            if (piece == null || boardConfig == null) return;

            _activePiece = piece;
            _isShowing = true;

            float cellSize = boardConfig.cellSize;
            float cellSpacing = boardConfig.cellSpacing;
            float totalCell = cellSize + cellSpacing;

            // Configure cell positions relative to this controller's origin
            for (int i = 0; i < MaxGhostCells; i++)
            {
                SpriteRenderer sr = _cellPool[i];
                if (i < piece.CellOffsets.Length)
                {
                    Vector2Int offset = piece.CellOffsets[i];
                    sr.transform.localPosition = new Vector3(offset.x * totalCell * boardScale, offset.y * totalCell * boardScale, 0f);
                    sr.transform.localScale = new Vector3(cellSize * boardScale, cellSize * boardScale, 1f);
                    sr.gameObject.SetActive(true);
                }
                else
                {
                    sr.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Snaps the ghost preview parent transform to the board coordinates 
        /// and colors the cells based on placement validity.
        /// </summary>
        public void UpdatePosition(Vector2Int gridPivot, bool isValid)
        {
            if (!_isShowing || _activePiece == null || boardController == null) return;

            // Snap the parent transform to the world coordinates of the board cell pivot
            Vector3 targetWorldPos = boardController.GridToWorld(gridPivot.y, gridPivot.x);
            transform.position = targetWorldPos;

            // Determine overlay colors
            Color tintColor = isValid ? validTint : invalidTint;

            // Tint only the active cells in the shape
            for (int i = 0; i < _activePiece.CellOffsets.Length; i++)
            {
                _cellPool[i].color = tintColor;
            }
        }

        /// <summary>
        /// Disables all active ghost preview cells.
        /// </summary>
        public void Hide()
        {
            _activePiece = null;
            _isShowing = false;

            // Deactivate all cells in the pool
            for (int i = 0; i < MaxGhostCells; i++)
            {
                _cellPool[i].gameObject.SetActive(false);
            }
        }
    }
}
