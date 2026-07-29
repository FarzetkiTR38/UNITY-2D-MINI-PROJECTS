using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeonGalaxy.Data;

namespace NeonGalaxy.Core
{
    /// <summary>
    /// Visual representation of a single puzzle piece in the tray or during drag-and-drop.
    /// Spawns individual blocks dynamically based on the piece's cell offsets.
    /// Handles pickup scaling, placement scaling, and reset animations.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class PieceView : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private float trayScale = 0.55f;
        [SerializeField] private float dragScale = 0.9f;
        [SerializeField] private float animDuration = 0.15f;

        [Header("Touch Softness")]
        [Tooltip("Extra padding added to each side of the collider (world units). " +
                 "Makes pieces easier to grab without touching them exactly.")]
        [SerializeField] private float colliderPadding = 0.4f;

        public float TrayScale => trayScale;
        public Vector3 VisualCenterOffset { get; private set; }

        public PieceInstance Piece { get; private set; }
        public int SlotIndex { get; private set; }

        private List<SpriteRenderer> _blockRenderers = new List<SpriteRenderer>();
        private Vector3 _originalTrayPosition;
        private Coroutine _activeAnimation;
        private BoxCollider2D _collider;
        private bool _isInteractable = true;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
        }

        /// <summary>
        /// Instantiates the block visual renderers and sets up colliders based on the piece shape.
        /// </summary>
        public void Setup(PieceInstance pieceInstance, int slotIndex, Sprite blockSprite, Color tintColor, float cellSize, float cellSpacing)
        {
            Piece = pieceInstance;
            SlotIndex = slotIndex;
            _isInteractable = true;

            // Clear previous blocks if re-used
            foreach (var renderer in _blockRenderers)
            {
                if (renderer != null) Destroy(renderer.gameObject);
            }
            _blockRenderers.Clear();

            // Set the tray scale instantly
            transform.localScale = Vector3.one * trayScale;



            float totalCell = cellSize + cellSpacing;

            // Calculate the bounds to find the visual center offset of the piece relative to pivot (0,0)
            RectInt bounds = Piece.Definition.GetBounds();
            float centerX = (bounds.x + (bounds.width - 1f) / 2f) * totalCell;
            float centerY = (bounds.y + (bounds.height - 1f) / 2f) * totalCell;
            VisualCenterOffset = new Vector3(centerX, centerY, 0f);

            // Spawn blocks at cell offset coordinates
            foreach (Vector2Int offset in Piece.CellOffsets)
            {
                GameObject blockObj = new GameObject($"Block_{offset.x}_{offset.y}");
                blockObj.transform.SetParent(transform);
                // Position relative to pivot
                blockObj.transform.localPosition = new Vector3(offset.x * totalCell, offset.y * totalCell, 0f);
                // Scale the block to size
                blockObj.transform.localScale = new Vector3(cellSize, cellSize, 1f);

                SpriteRenderer sr = blockObj.AddComponent<SpriteRenderer>();
                sr.sprite = blockSprite;
                sr.color = tintColor;
                sr.sortingLayerName = "Board Blocks";
                sr.sortingOrder = 10; // Sorting order on top of board blocks

                _blockRenderers.Add(sr);
            }

            // Dynamically calculate and configure the BoxCollider2D bounds for touch detection
            // Add padding to make the touch target larger and more forgiving
            _collider.size = new Vector2(
                bounds.width * totalCell + colliderPadding * 2f,
                bounds.height * totalCell + colliderPadding * 2f
            );
            _collider.offset = new Vector2(
                (bounds.x + (bounds.width - 1f) / 2f) * totalCell,
                (bounds.y + (bounds.height - 1f) / 2f) * totalCell
            );
            _collider.enabled = true;
        }

        public void UpdateSkin(Sprite blockSprite, Color tintColor)
        {
            foreach (var renderer in _blockRenderers)
            {
                if (renderer != null)
                {
                    renderer.sprite = blockSprite;
                    renderer.color = tintColor;
                }
            }
        }

        /// <summary>
        /// Stores the original home position of this piece in the tray.
        /// </summary>
        public void SetOriginalTrayPosition(Vector3 position)
        {
            _originalTrayPosition = position;
        }

        /// <summary>
        /// Sets if the piece can be interacted with / dragged.
        /// </summary>
        public void SetInteractable(bool value)
        {
            _isInteractable = value;
            _collider.enabled = value;
        }

        /// <summary>
        /// Scales the piece up to its full size and lifts it slightly when picked up.
        /// </summary>
        public void AnimatePickup()
        {
            StopActiveAnimation();
            _activeAnimation = StartCoroutine(ScaleRoutine(Vector3.one * dragScale));
        }

        /// <summary>
        /// Animates the piece back to its original slot in the tray, scaling it back down.
        /// </summary>
        public void AnimateReturn(Action onComplete = null)
        {
            StopActiveAnimation();
            _activeAnimation = StartCoroutine(ReturnRoutine(onComplete));
        }

        /// <summary>
        /// Animates the piece shrinking and fading out when placed on the board.
        /// </summary>
        public void AnimatePlaced(Action onComplete = null)
        {
            StopActiveAnimation();
            _activeAnimation = StartCoroutine(PlacedRoutine(onComplete));
        }

        private IEnumerator ScaleRoutine(Vector3 targetScale)
        {
            Vector3 startScale = transform.localScale;
            float elapsed = 0f;

            while (elapsed < animDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animDuration;
                // Use a smooth ease-out curve
                float ease = 1f - Mathf.Pow(1f - t, 3);

                transform.localScale = Vector3.Lerp(startScale, targetScale, ease);
                yield return null;
            }

            transform.localScale = targetScale;
            _activeAnimation = null;
        }

        private IEnumerator ReturnRoutine(Action onComplete)
        {
            Vector3 startScale = transform.localScale;
            Vector3 startPos = transform.position;
            Vector3 targetScale = Vector3.one * trayScale;
            float elapsed = 0f;

            while (elapsed < animDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animDuration;
                float ease = 1f - Mathf.Pow(1f - t, 3);

                transform.localScale = Vector3.Lerp(startScale, targetScale, ease);
                transform.position = Vector3.Lerp(startPos, _originalTrayPosition, ease);
                yield return null;
            }

            transform.localScale = targetScale;
            transform.position = _originalTrayPosition;
            onComplete?.Invoke();
            _activeAnimation = null;
        }

        private IEnumerator PlacedRoutine(Action onComplete)
        {
            _collider.enabled = false;
            Vector3 startScale = transform.localScale;
            float elapsed = 0f;

            while (elapsed < animDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animDuration;
                float ease = 1f - Mathf.Pow(1f - t, 3);

                // Shrink
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, ease);

                // Fade alpha of block renderers
                foreach (var sr in _blockRenderers)
                {
                    if (sr != null)
                    {
                        Color c = sr.color;
                        sr.color = new Color(c.r, c.g, c.b, Mathf.Lerp(1f, 0f, ease));
                    }
                }

                yield return null;
            }

            onComplete?.Invoke();
            Destroy(gameObject);
        }

        private void StopActiveAnimation()
        {
            if (_activeAnimation != null)
            {
                StopCoroutine(_activeAnimation);
                _activeAnimation = null;
            }
        }

        private void OnDestroy()
        {
            StopActiveAnimation();
        }
    }
}
