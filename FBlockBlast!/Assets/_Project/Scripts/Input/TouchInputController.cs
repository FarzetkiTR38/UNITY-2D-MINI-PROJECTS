using System;
using UnityEngine;
using UnityEngine.InputSystem;
using NeonGalaxy.Core;
using NeonGalaxy.Data;

namespace NeonGalaxy.Input
{
    /// <summary>
    /// Captures touch/mouse input on mobile and desktop platforms.
    /// Initiates and updates drag-and-drop operations using DragDropHandler.
    /// Notifies GameManager upon successful piece drop.
    /// </summary>
    public class TouchInputController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private BoardController boardController;
        [SerializeField] private PieceTrayController pieceTrayController;

        [Header("Settings")]
        [SerializeField] private Vector3 fingerOffset = new Vector3(0f, 1.5f, 0f);

        /// <summary>
        /// Fired when a piece is successfully dropped on the board.
        /// Args: PieceInstance, target grid position, and tray slot index.
        /// </summary>
        public event Action<PieceInstance, Vector2Int, int> OnPieceDropped;

        private DragDropHandler _dragDropHandler;
        private BoardModel _boardModel;
        private bool _isInputEnabled = true;

        public bool IsInputEnabled
        {
            get => _isInputEnabled;
            set => _isInputEnabled = value;
        }

        private void Awake()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        /// <summary>
        /// Initializes the drag-and-drop handler with board model and optional ghost preview.
        /// </summary>
        public void Initialize(BoardModel boardModel, GhostPreviewController ghostPreview = null)
        {
            _boardModel = boardModel;
            _dragDropHandler = new DragDropHandler(boardController, _boardModel, pieceTrayController, ghostPreview);
            _dragDropHandler.SetFingerOffset(fingerOffset);
        }

        private void Update()
        {
            if (!_isInputEnabled || _boardModel == null || _dragDropHandler == null)
            {
                // Force cancel drag if input is disabled mid-operation
                if (_dragDropHandler != null && _dragDropHandler.IsDragging)
                {
                    _dragDropHandler.CancelDrag();
                }
                return;
            }

            HandleInput();
        }

        private void HandleInput()
        {
            var pointer = Pointer.current;
            if (pointer == null) return;

            // Begin Drag
            if (pointer.press.wasPressedThisFrame)
            {
                Vector3 touchWorldPos = GetTouchWorldPosition();
                RaycastHit2D hit = Physics2D.Raycast(touchWorldPos, Vector2.zero);

                if (hit.collider != null)
                {
                    PieceView pieceView = hit.collider.GetComponent<PieceView>();
                    if (pieceView != null)
                    {
                        _dragDropHandler.BeginDrag(pieceView, touchWorldPos);
                    }
                }
            }
            // Update Drag
            else if (pointer.press.isPressed && _dragDropHandler.IsDragging)
            {
                Vector3 touchWorldPos = GetTouchWorldPosition();
                _dragDropHandler.UpdateDrag(touchWorldPos);
            }
            // End Drag
            else if (pointer.press.wasReleasedThisFrame && _dragDropHandler.IsDragging)
            {
                PieceView currentPieceView = _dragDropHandler.DraggingPiece;
                int slotIndex = currentPieceView != null ? currentPieceView.SlotIndex : -1;
                PieceInstance pieceInstance = currentPieceView != null ? currentPieceView.Piece : null;

                var (placed, gridPos) = _dragDropHandler.EndDrag();

                if (placed && pieceInstance != null && slotIndex != -1)
                {
                    // Notify listeners (GameManager) that the player successfully placed the piece
                    OnPieceDropped?.Invoke(pieceInstance, gridPos, slotIndex);
                }
            }
        }

        private Vector3 GetTouchWorldPosition()
        {
            var pointer = Pointer.current;
            Vector3 screenPos = pointer != null ? (Vector3)pointer.position.ReadValue() : Vector3.zero;
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
            worldPos.z = 0f; // Force flat 2D plane
            return worldPos;
        }

        /// <summary>
        /// Explicitly cancels the current dragging operation (e.g. when pausing the game).
        /// </summary>
        public void CancelActiveDrag()
        {
            if (_dragDropHandler != null && _dragDropHandler.IsDragging)
            {
                _dragDropHandler.CancelDrag();
            }
        }
    }
}
