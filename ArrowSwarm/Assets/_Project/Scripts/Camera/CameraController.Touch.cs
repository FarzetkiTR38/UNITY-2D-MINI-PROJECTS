namespace ArrowSwarm.Camera
{
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;
    using TouchPhase = UnityEngine.InputSystem.TouchPhase;

    /// <summary>Reads active touch controls and rebases each pan or pinch when its actual finger identities change.</summary>
    public partial class CameraController
    {
        private int _panTouchId = -1, _pinchFirstId = -1, _pinchSecondId = -1, _gestureVersion;
        private bool _touchGestureActive;

        private void HandleTouchInput()
        {
            var screen = Touchscreen.current;
            TouchControl first = null, second = null;
            int count = 0;
            if (screen != null)
                foreach (var touch in screen.touches)
                {
                    if (!touch.press.isPressed) continue;
                    if (count == 0) first = touch; else if (count == 1) second = touch;
                    count++;
                }
            if (count == 2) { _touchGestureActive = true; HandlePinchZoom(first, second); }
            else if (count == 1)
            {
                _touchGestureActive = true;
                if (_isPinching) { _isPinching = false; _panTouchId = -1; }
                HandleTouchPan(first);
            }
            else EndTouchGesture();
        }

        private void HandlePinchZoom(TouchControl first, TouchControl second)
        {
            Vector2 firstPosition = first.position.ReadValue(), secondPosition = second.position.ReadValue();
            int firstId = first.touchId.ReadValue(), secondId = second.touchId.ReadValue();
            if (!_isPinching || _pinchFirstId != firstId || _pinchSecondId != secondId)
            {
                if (!IsValidPanStartPosition(firstPosition, firstId) || !IsValidPanStartPosition(secondPosition, secondId))
                { _isPinching = false; _isTouchDragValid = false; return; }
                _gestureVersion++; _isPinching = true; _pinchFirstId = firstId; _pinchSecondId = secondId;
                _lastPinchDistance = Vector2.Distance(firstPosition, secondPosition);
                _isPanning = false; _isTouchDragValid = false; _panTouchId = -1;
                return;
            }
            float distance = Vector2.Distance(firstPosition, secondPosition);
            _targetOrthoSize = Mathf.Clamp(_targetOrthoSize - (distance - _lastPinchDistance) * _zoomSpeed * .01f,
                _minOrthoSize, _maxOrthoSize);
            _lastPinchDistance = distance;
        }

        private void HandleTouchPan(TouchControl touch)
        {
            Vector2 position = touch.position.ReadValue();
            int id = touch.touchId.ReadValue();
            if (touch.phase.ReadValue() == TouchPhase.Began || id != _panTouchId)
            {
                _gestureVersion++; _panTouchId = id;
                _isTouchDragValid = IsValidPanStartPosition(position, id);
                _dragStartScreenPos = _lastPanPosition = position;
                _isPanning = _isDragging = false;
                return;
            }
            if (!_isTouchDragValid) return;
            float distance = Vector2.Distance(position, _dragStartScreenPos);
            if (!_isPanning && distance > DragThreshold)
            { _isPanning = _isDragging = true; _lastPanPosition = position; }
            if (_isPanning)
            {
                Vector2 delta = position - _lastPanPosition;
                float units = Cam.orthographicSize * 2f / Mathf.Max(1f, Screen.height);
                PanCamera(new Vector3(-delta.x * units, -delta.y * units, 0f));
                _lastPanPosition = position;
            }
        }

        private void EndTouchGesture()
        {
            if (!_touchGestureActive) return;
            _touchGestureActive = false; _isTouchDragValid = _isPinching = false;
            _panTouchId = _pinchFirstId = _pinchSecondId = -1;
            if (_isDragging)
            {
                _isPanning = false;
                StartCoroutine(ResetDraggingNextFrame());
            }
        }
    }
}
