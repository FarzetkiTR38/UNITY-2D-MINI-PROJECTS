using System;
using System.Collections.Generic;
using UnityEngine;
using NeonGalaxy.Data;

namespace NeonGalaxy.Core
{
    /// <summary>
    /// Manages the 3 piece slots at the bottom of the screen.
    /// Handles batch visual spawning, piece pickup/placement callbacks,
    /// and tracks remaining pieces for game-over checks.
    /// </summary>
    public class PieceTrayController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private BoardConfigSO boardConfig;

        [Header("Slots")]
        [SerializeField] private Transform[] slots = new Transform[3];

        [Header("Prefabs")]
        [SerializeField] private PieceView piecePrefab;

        private readonly PieceView[] _activePieceViews = new PieceView[3];

        private void OnEnable()
        {
            GameEvents.OnNewBatchReady += HandleNewBatchReady;
        }

        private void OnDisable()
        {
            GameEvents.OnNewBatchReady -= HandleNewBatchReady;
        }

        /// <summary>
        /// Clears the tray and spawns a new batch of 3 pieces.
        /// </summary>
        public void SetBatch(PieceInstance[] batch)
        {
            ClearTray();

            if (batch == null) return;

            for (int i = 0; i < batch.Length && i < 3; i++)
            {
                PieceInstance piece = batch[i];
                if (piece == null || piece.IsPlaced) continue;

                Transform slot = GetOrCreateSlot(i);
                PieceView pieceView = Instantiate(piecePrefab, slot);
                
                // Determine block color from config palette
                Color blockColor = GetColorFromPalette(piece.ColorIndex);

                // Initialize the PieceView
                pieceView.Setup(piece, i, blockColor, boardConfig.cellSize, boardConfig.cellSpacing);
                pieceView.transform.localPosition = Vector3.zero;
                pieceView.SetOriginalTrayPosition(pieceView.transform.position);

                _activePieceViews[i] = pieceView;
            }
        }

        /// <summary>
        /// Clears all slots in the tray.
        /// </summary>
        public void ClearTray()
        {
            for (int i = 0; i < 3; i++)
            {
                if (_activePieceViews[i] != null)
                {
                    Destroy(_activePieceViews[i].gameObject);
                    _activePieceViews[i] = null;
                }
            }
        }

        /// <summary>
        /// Returns the PieceView currently in the specified slot index.
        /// </summary>
        public PieceView GetPieceView(int index)
        {
            if (index < 0 || index >= 3) return null;
            return _activePieceViews[index];
        }

        /// <summary>
        /// Disables the piece view at the specified slot index and triggers its placement animation.
        /// </summary>
        public void OnPiecePlaced(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= 3) return;

            PieceView pieceView = _activePieceViews[slotIndex];
            if (pieceView != null)
            {
                pieceView.Piece.IsPlaced = true;
                pieceView.AnimatePlaced();
                _activePieceViews[slotIndex] = null;
            }
        }

        /// <summary>
        /// Returns the list of unplaced PieceInstances currently in the tray.
        /// </summary>
        public List<PieceInstance> GetRemainingPieces()
        {
            var list = new List<PieceInstance>();
            for (int i = 0; i < 3; i++)
            {
                if (_activePieceViews[i] != null && _activePieceViews[i].Piece != null)
                {
                    list.Add(_activePieceViews[i].Piece);
                }
            }
            return list;
        }

        /// <summary>
        /// Enables or disables interactions on all remaining pieces in the tray.
        /// </summary>
        public void SetInteractable(bool value)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_activePieceViews[i] != null)
                {
                    _activePieceViews[i].SetInteractable(value);
                }
            }
        }

        private void HandleNewBatchReady(PieceInstance[] batch)
        {
            SetBatch(batch);
        }

        private Transform GetOrCreateSlot(int index)
        {
            // If explicit slot transform is assigned in Inspector, use it.
            if (slots != null && index < slots.Length && slots[index] != null)
            {
                return slots[index];
            }

            // Fallback: Create dynamic slot positions relative to the tray root
            string slotName = $"Slot_{index}";
            float horizontalOffset = (index - 1) * 2.7f; // Spaced 2.7 units apart instead of 3.5 for better screen fit
            Transform existing = transform.Find(slotName);
            if (existing != null)
            {
                existing.localPosition = new Vector3(horizontalOffset, 0f, 0f);
                return existing;
            }

            GameObject newSlot = new GameObject(slotName);
            newSlot.transform.SetParent(transform);
            newSlot.transform.localPosition = new Vector3(horizontalOffset, 0f, 0f);

            return newSlot.transform;
        }

        private Color GetColorFromPalette(int index)
        {
            if (boardConfig == null || boardConfig.blockPalette == null || boardConfig.blockPalette.Length == 0)
                return Color.white;

            int safeIndex = Mathf.Clamp(index, 0, boardConfig.blockPalette.Length - 1);
            return boardConfig.blockPalette[safeIndex];
        }
    }
}
