using UnityEngine;
using NeonGalaxy.Core;
using NeonGalaxy.Data;

namespace NeonGalaxy.Input
{
    /// <summary>
    /// Coordinates the drag-and-drop state machine for puzzle pieces.
    /// Manages finger offsets, checks grid placement validity, and updates the ghost preview.
    /// This is a pure C# class (non-MonoBehaviour) to keep input logic clean and testable.
    /// </summary>
    public class DragDropHandler
    {
        private readonly BoardController _boardController;
        private readonly BoardModel _boardModel;
        private readonly PieceTrayController _pieceTrayController;
        private readonly GhostPreviewController _ghostPreview; // Optional preview reference

        private PieceView _draggingPiece;
        private Vector3 _fingerOffset;
        private bool _isDragging;

        public bool IsDragging => _isDragging;
        public PieceView DraggingPiece => _draggingPiece;

        public DragDropHandler(
            BoardController boardController, 
            BoardModel boardModel, 
            PieceTrayController pieceTrayController,
            GhostPreviewController ghostPreview = null)
        {
            _boardController = boardController;
            _boardModel = boardModel;
            _pieceTrayController = pieceTrayController;
            _ghostPreview = ghostPreview;
            
            // Default offset: Position the piece slightly above the finger so it's not obscured.
            _fingerOffset = new Vector3(0f, 1.5f, 0f);
        }

        /// <summary>
        /// Sets a custom visual offset between the touch point and the piece pivot.
        /// </summary>
        public void SetFingerOffset(Vector3 offset)
        {
            _fingerOffset = offset;
        }

        /// <summary>
        /// Initiates the drag action for a specified PieceView.
        /// </summary>
        public void BeginDrag(PieceView pieceView, Vector3 touchWorldPos)
        {
            if (pieceView == null || _isDragging) return;

            _draggingPiece = pieceView;
            _isDragging = true;

            // Lift and scale up the piece view (offsetting by visual center so it centers horizontally on the touch point)
            _draggingPiece.AnimatePickup(touchWorldPos, _fingerOffset - _draggingPiece.VisualCenterOffset);

            // Show ghost preview overlay if available
            if (_ghostPreview != null && _draggingPiece.Piece != null)
            {
                _ghostPreview.Show(_draggingPiece.Piece);
            }
        }

        /// <summary>
        /// Updates the dragging piece's position and refreshes placement validity overlays.
        /// </summary>
        public void UpdateDrag(Vector3 touchWorldPos)
        {
            if (!_isDragging || _draggingPiece == null) return;

            // Update piece position directly under the finger (plus offset, minus visual center offset)
            Vector3 targetPos = touchWorldPos + _fingerOffset - _draggingPiece.VisualCenterOffset;
            _draggingPiece.transform.position = targetPos;

            // Get raw grid position (which can be out of bounds)
            _boardController.WorldToGrid(targetPos, out Vector2Int rawGridPos);

            // Clamp grid position to board boundaries for visual preview snapping
            int boardWidth = 8;
            int boardHeight = 8;
            if (_boardModel != null)
            {
                boardWidth = _boardModel.Width;
                boardHeight = _boardModel.Height;
            }

            Vector2Int clampedGridPos = new Vector2Int(
                Mathf.Clamp(rawGridPos.x, 0, boardWidth - 1),
                Mathf.Clamp(rawGridPos.y, 0, boardHeight - 1)
            );

            // Placement is valid ONLY if the raw position is inside the board boundaries AND placeable
            bool isInside = _boardController.IsValidGridPos(rawGridPos);
            bool canPlace = isInside && _boardModel.CanPlacePiece(_draggingPiece.Piece, rawGridPos.y, rawGridPos.x);

            if (_ghostPreview != null)
            {
                _ghostPreview.UpdatePosition(clampedGridPos, canPlace);
            }
        }

        /// <summary>
        /// Handles touch release. Validates drop position: 
        /// Returns (true, gridPos) on success, or (false, Vector2Int.zero) and animates the piece back on failure.
        /// </summary>
        public (bool placed, Vector2Int gridPos) EndDrag()
        {
            if (!_isDragging || _draggingPiece == null)
            {
                return (false, Vector2Int.zero);
            }

            bool placed = false;
            Vector2Int gridPos = Vector2Int.zero;

            Vector3 targetPos = _draggingPiece.transform.position;

            // Hide ghost preview instantly
            if (_ghostPreview != null)
            {
                _ghostPreview.Hide();
            }

            // Check final placement validity
            if (_boardController.WorldToGrid(targetPos, out Vector2Int calculatedGridPos))
            {
                if (_boardModel.CanPlacePiece(_draggingPiece.Piece, calculatedGridPos.y, calculatedGridPos.x))
                {
                    placed = true;
                    gridPos = calculatedGridPos;
                }
            }

            if (placed)
            {
                // Placement successful: trigger visual fade-out/destruct and clear active slot
                _pieceTrayController.OnPiecePlaced(_draggingPiece.SlotIndex);
            }
            else
            {
                // Placement failed: animate piece return to its tray slot
                _draggingPiece.AnimateReturn();
            }

            // Reset drag state
            _isDragging = false;
            _draggingPiece = null;

            return (placed, gridPos);
        }

        /// <summary>
        /// Forcefully cancels the active drag operation (e.g. on pause or system interrupts).
        /// </summary>
        public void CancelDrag()
        {
            if (!_isDragging || _draggingPiece == null) return;

            if (_ghostPreview != null)
            {
                _ghostPreview.Hide();
            }

            _draggingPiece.AnimateReturn();
            _isDragging = false;
            _draggingPiece = null;
        }
    }
}
