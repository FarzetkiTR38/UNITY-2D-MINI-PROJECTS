using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace NeonGalaxy.UI
{
    /// <summary>
    /// Image-based custom slider that uses Image.fillAmount for the active bar
    /// and moves a handle (SlideCircle) along the bar.
    /// Supports drag on handle and click-to-set on the background bar.
    /// </summary>
    public class CustomSliderUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [Header("Slider Images")]
        [Tooltip("The colorful/neon fill image (Image Type = Filled, Horizontal, Left)")]
        [SerializeField] private Image fillImage;

        [Tooltip("The gray/disabled background bar image")]
        [SerializeField] private Image backgroundImage;

        [Tooltip("The draggable circle handle")]
        [SerializeField] private RectTransform handleRect;

        [Header("Settings")]
        [SerializeField] [Range(0f, 1f)] private float value = 1f;

        /// <summary>
        /// Fired when the slider value changes. Passes the new value (0-1).
        /// </summary>
        public event Action<float> OnValueChanged;

        private RectTransform _backgroundRect;
        private bool _isDragging;

        /// <summary>
        /// Current slider value (0 to 1).
        /// </summary>
        public float Value
        {
            get => value;
            set => SetValue(value, true);
        }

        private void Awake()
        {
            if (backgroundImage != null)
                _backgroundRect = backgroundImage.GetComponent<RectTransform>();

            // Ensure fill image is set to Filled type
            if (fillImage != null)
            {
                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Horizontal;
                fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            }
        }

        private void Start()
        {
            // Apply initial value without firing event
            ApplyValue();
        }

        /// <summary>
        /// Sets the slider value without triggering the OnValueChanged event.
        /// Useful for loading saved settings.
        /// </summary>
        public void SetValueWithoutNotify(float newValue)
        {
            SetValue(newValue, false);
        }

        private void SetValue(float newValue, bool notify)
        {
            newValue = Mathf.Clamp01(newValue);
            if (Mathf.Approximately(value, newValue)) return;

            value = newValue;
            ApplyValue();

            if (notify)
                OnValueChanged?.Invoke(value);
        }

        private void ApplyValue()
        {
            // Update fill amount
            if (fillImage != null)
                fillImage.fillAmount = value;

            // Update handle position
            UpdateHandlePosition();
        }

        private void UpdateHandlePosition()
        {
            if (handleRect == null || _backgroundRect == null) return;

            // Get the background bar's width in local space
            float barWidth = _backgroundRect.rect.width;

            // Calculate handle position: from left edge to right edge of the bar
            float leftEdge = _backgroundRect.anchoredPosition.x - barWidth * _backgroundRect.pivot.x;
            float rightEdge = leftEdge + barWidth;

            float handleX = Mathf.Lerp(leftEdge, rightEdge, value);

            handleRect.anchoredPosition = new Vector2(
                handleX,
                handleRect.anchoredPosition.y
            );
        }

        // ── Drag Handlers ────────────────────────────────────────

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            UpdateValueFromPointer(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isDragging) return;
            UpdateValueFromPointer(eventData);
        }

        private void UpdateValueFromPointer(PointerEventData eventData)
        {
            if (_backgroundRect == null) return;

            // Convert screen position to local position within the background rect
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _backgroundRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            );

            // Calculate normalized value based on position within the rect
            float barWidth = _backgroundRect.rect.width;
            float leftEdge = -barWidth * _backgroundRect.pivot.x;

            float normalizedValue = (localPoint.x - leftEdge) / barWidth;
            SetValue(normalizedValue, true);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Allow preview in editor
            if (fillImage != null)
            {
                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Horizontal;
                fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                fillImage.fillAmount = value;
            }
            UpdateHandlePosition();
        }
#endif
    }
}
