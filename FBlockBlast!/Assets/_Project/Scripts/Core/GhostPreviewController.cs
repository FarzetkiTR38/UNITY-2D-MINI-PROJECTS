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
        [SerializeField] private float boardScale = 0.9f;
        [Range(0f, 1f)]
        [SerializeField] private float previewAlpha = 0.25f; // Opacity for the preview


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
                // Sprite will be set dynamically in Show() based on the piece's skin
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

            BlockSkin skin = boardConfig.GetBlockSkin(piece.ColorIndex);

            // Configure cell positions relative to this controller's origin
            for (int i = 0; i < MaxGhostCells; i++)
            {
                SpriteRenderer sr = _cellPool[i];
                if (i < piece.CellOffsets.Length)
                {
                    Vector2Int offset = piece.CellOffsets[i];
                    sr.transform.localPosition = new Vector3(offset.x * totalCell * boardScale, offset.y * totalCell * boardScale, 0f);
                    sr.transform.localScale = new Vector3(cellSize * boardScale, cellSize * boardScale, 1f);
                    
                    // Apply the piece's actual sprite
                    sr.sprite = skin.sprite;
                    
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

            // If it's not a valid placement, hide the preview completely
            if (!isValid)
            {
                for (int i = 0; i < _activePiece.CellOffsets.Length; i++)
                {
                    _cellPool[i].gameObject.SetActive(false);
                }
                return;
            }

            // Snap the parent transform to the world coordinates of the board cell pivot
            Vector3 targetWorldPos = boardController.GridToWorld(gridPivot.y, gridPivot.x);
            transform.position = targetWorldPos;

            // Fetch the piece's skin to get its original tint color
            BlockSkin skin = boardConfig.GetBlockSkin(_activePiece.ColorIndex);
            
            // Apply original color but with reduced opacity
            Color previewColor = new Color(skin.tintColor.r, skin.tintColor.g, skin.tintColor.b, previewAlpha);

            // Reactivate and tint the active cells in the shape
            for (int i = 0; i < _activePiece.CellOffsets.Length; i++)
            {
                _cellPool[i].gameObject.SetActive(true);
                _cellPool[i].color = previewColor;
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
