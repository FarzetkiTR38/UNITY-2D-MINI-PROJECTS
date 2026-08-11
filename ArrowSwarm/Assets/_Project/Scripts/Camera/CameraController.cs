namespace ArrowSwarm.Camera
{
    using System;
    using ArrowSwarm.Core;
    using ArrowSwarm.Utils;
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// Controls the orthographic camera: auto-fit to map,
    /// pinch-to-zoom, pan with touch/mouse, and zoom limits.
    /// </summary>
    public class CameraController : Singleton<CameraController>
    {
        [SerializeField] private float _zoomSpeed = 0.5f;
        [SerializeField] private float _smoothTime = 0.1f;
        [SerializeField] private float _padding = 1f;
        [SerializeField] private float _topHudMargin = 1.5f;
        [SerializeField] private float _bottomHudMargin = 1.8f;

        private UnityEngine.Camera _camera;
        private float _defaultOrthoSize;
        private float _minOrthoSize;
        private float _maxOrthoSize;
        private Vector2 _mapCenter;
        private Vector2 _mapExtents;
        private Vector2 _gridMin;
        private Vector2 _gridMax;

        // Touch & Mouse Drag tracking
        private bool _isPanning;
        private bool _isDragging;
        private Vector2 _dragStartScreenPos;
        private Vector2 _lastPanPosition;
        private float _lastPinchDistance;
        private bool _isPinching;
        private const float DragThreshold = 10f; // Pixels required to initiate camera pan

        // Smooth zoom
        private float _targetOrthoSize;
        private float _zoomVelocity;

        /// <summary>Returns true if player is currently dragging/panning the camera.</summary>
        public bool IsDragging => _isDragging;

        /// <summary>Current zoom level (1 = default, higher = zoomed in).</summary>
        public float CurrentZoom => _defaultOrthoSize / _camera.orthographicSize;

        /// <summary>Current zoom normalized (0 = min zoom, 1 = max zoom).</summary>
        public float ZoomNormalized
        {
            get
            {
                float range = _maxOrthoSize - _minOrthoSize;
                if (range <= 0) return 0;
                return 1f - (_camera.orthographicSize - _minOrthoSize) / range;
            }
        }

        /// <summary>Fired when zoom level changes (normalized 0-1).</summary>
        public static event Action<float> OnZoomChanged;

        protected override void OnSingletonAwake()
        {
            _camera = UnityEngine.Camera.main;
            if (_camera == null)
            {
                _camera = GetComponent<UnityEngine.Camera>();
            }
        }

        /// <summary>
        /// Sets up the camera to fit the map with given dimensions.
        /// </summary>
        public void FitToMap(MapData mapData)
        {
            float spacing = ArrowSwarm.Grid.GridManager.Instance.PointSpacing;
            Vector2 origin = ArrowSwarm.Grid.GridManager.Instance.Origin;

            float mapWidth = mapData.GridWidth * spacing + _padding * 2;
            float mapHeight = mapData.GridHeight * spacing + _padding * 2;

            float totalGridWidth = (mapData.GridWidth - 1) * spacing;
            float totalGridHeight = (mapData.GridHeight - 1) * spacing;

            _mapCenter = origin + new Vector2(totalGridWidth * 0.5f, totalGridHeight * 0.5f);
            _mapExtents = new Vector2(mapWidth * 0.5f, mapHeight * 0.5f);

            // Store Grid bounds with half spacing margin so all grid points stay on screen at limits
            _gridMin = origin - new Vector2(spacing * 0.5f, spacing * 0.5f);
            _gridMax = origin + new Vector2(totalGridWidth + spacing * 0.5f, totalGridHeight + spacing * 0.5f);

            // Calculate ortho size to fit entire map within the visible viewport between Top HUD and Bottom HUD
            float aspect = (float)Screen.width / Screen.height;
            float orthoWidth = mapWidth / (2f * aspect);
            float availableHeightSpace = mapHeight + (_topHudMargin + _bottomHudMargin);
            float orthoHeight = availableHeightSpace / 2f;
            _defaultOrthoSize = Mathf.Max(orthoWidth, orthoHeight);

            GameConfig config = GameManager.Instance?.Config;
            float maxZoom = config?.MaxZoom ?? 3f;
            float minZoom = config?.MinZoom ?? 1f;

            _maxOrthoSize = _defaultOrthoSize / minZoom;
            _minOrthoSize = _defaultOrthoSize / maxZoom;

            _camera.orthographicSize = _defaultOrthoSize;
            _targetOrthoSize = _defaultOrthoSize;
            transform.position = new Vector3(_mapCenter.x, _mapCenter.y, transform.position.z);

            LogDebug($"Camera fit: Center={_mapCenter}, OrthoSize={_defaultOrthoSize}, " +
                     $"GridMin={_gridMin}, GridMax={_gridMax}");
        }

        /// <summary>
        /// Sets zoom from a normalized value (0 = min zoom, 1 = max zoom).
        /// Called by the zoom slider UI.
        /// </summary>
        public void SetZoomNormalized(float normalized)
        {
            normalized = Mathf.Clamp01(normalized);
            _targetOrthoSize = Mathf.Lerp(_maxOrthoSize, _minOrthoSize, normalized);
        }

        /// <summary>
        /// Resets camera to default (fit all) position.
        /// </summary>
        public void ResetCamera()
        {
            _targetOrthoSize = _defaultOrthoSize;
            transform.position = new Vector3(_mapCenter.x, _mapCenter.y, transform.position.z);
        }

        private void Update()
        {
            if (GameManager.Instance?.CurrentState != GameState.Playing &&
                GameManager.Instance?.CurrentState != GameState.Paused) return;

            HandleTouchInput();
            HandleMouseInput();
            UpdateZoom();
        }

        private void HandleTouchInput()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen == null) return;

            int touchCount = touchscreen.touches.Count;
            int activeTouches = 0;
            for (int i = 0; i < touchCount; i++)
            {
                if (touchscreen.touches[i].press.isPressed) activeTouches++;
            }

            if (activeTouches == 2)
            {
                HandlePinchZoom(touchscreen);
            }
            else if (activeTouches == 1 && !_isPinching)
            {
                HandleTouchPan(touchscreen);
            }
            else
            {
                if (_isPanning)
                {
                    _isPanning = false;
                    StartCoroutine(ResetDraggingNextFrame());
                }
                _isPinching = false;
            }
        }

        private void HandlePinchZoom(Touchscreen touchscreen)
        {
            var touch0 = touchscreen.touches[0];
            var touch1 = touchscreen.touches[1];

            float currentDistance = Vector2.Distance(
                touch0.position.ReadValue(), touch1.position.ReadValue());

            if (!_isPinching)
            {
                _isPinching = true;
                _lastPinchDistance = currentDistance;
                _isPanning = false;
                return;
            }

            float delta = currentDistance - _lastPinchDistance;
            _targetOrthoSize -= delta * _zoomSpeed * 0.01f;
            _targetOrthoSize = Mathf.Clamp(_targetOrthoSize, _minOrthoSize, _maxOrthoSize);
            _lastPinchDistance = currentDistance;
        }

        private void HandleTouchPan(Touchscreen touchscreen)
        {
            var touch = touchscreen.touches[0];
            Vector2 touchPos = touch.position.ReadValue();

            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                _dragStartScreenPos = touchPos;
                _lastPanPosition = touchPos;
                _isPanning = false;
                _isDragging = false;
                return;
            }

            float dist = Vector2.Distance(touchPos, _dragStartScreenPos);
            if (!_isPanning && dist > DragThreshold)
            {
                _isPanning = true;
                _isDragging = true;
                _lastPanPosition = touchPos;
            }

            if (_isPanning)
            {
                Vector2 delta = touchPos - _lastPanPosition;
                Vector3 worldDelta = _camera.ScreenToWorldPoint(Vector3.zero) -
                                     _camera.ScreenToWorldPoint(new Vector3(delta.x, delta.y, 0));
                PanCamera(worldDelta);
                _lastPanPosition = touchPos;
            }
        }

        private void HandleMouseInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            // Mouse scroll zoom
            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                _targetOrthoSize -= scroll * _zoomSpeed * 0.1f;
                _targetOrthoSize = Mathf.Clamp(_targetOrthoSize, _minOrthoSize, _maxOrthoSize);
            }

            // Left-click or Right-click drag pan
            if (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame)
            {
                _dragStartScreenPos = mouse.position.ReadValue();
                _lastPanPosition = _dragStartScreenPos;
                _isPanning = false;
                _isDragging = false;
            }
            else if (mouse.leftButton.isPressed || mouse.rightButton.isPressed)
            {
                Vector2 mousePos = mouse.position.ReadValue();
                float dist = Vector2.Distance(mousePos, _dragStartScreenPos);

                if (!_isPanning && dist > DragThreshold)
                {
                    _isPanning = true;
                    _isDragging = true;
                    _lastPanPosition = mousePos;
                }

                if (_isPanning)
                {
                    Vector2 delta = mousePos - _lastPanPosition;
                    Vector3 worldDelta = _camera.ScreenToWorldPoint(Vector3.zero) -
                                         _camera.ScreenToWorldPoint(new Vector3(delta.x, delta.y, 0));
                    PanCamera(worldDelta);
                    _lastPanPosition = mousePos;
                }
            }
            else if (mouse.leftButton.wasReleasedThisFrame || mouse.rightButton.wasReleasedThisFrame)
            {
                if (_isPanning)
                {
                    _isPanning = false;
                    StartCoroutine(ResetDraggingNextFrame());
                }
            }
        }

        private System.Collections.IEnumerator ResetDraggingNextFrame()
        {
            yield return null;
            _isDragging = false;
        }

        private void PanCamera(Vector3 worldDelta)
        {
            Vector3 newPos = transform.position + worldDelta;
            newPos = ClampPosition(newPos);
            transform.position = newPos;
        }

        private void UpdateZoom()
        {
            float currentSize = _camera.orthographicSize;
            if (Mathf.Abs(currentSize - _targetOrthoSize) > 0.01f)
            {
                _camera.orthographicSize = Mathf.SmoothDamp(
                    currentSize, _targetOrthoSize, ref _zoomVelocity, _smoothTime);
                
                // Keep camera position within bounds after zoom adjustment
                transform.position = ClampPosition(transform.position);
                OnZoomChanged?.Invoke(ZoomNormalized);
            }
        }

        private Vector3 ClampPosition(Vector3 pos)
        {
            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;

            // Effective viewport height margins below Top HUD and above Bottom HUD
            float effectiveHalfHeightTop = halfHeight - _topHudMargin;
            float effectiveHalfHeightBottom = halfHeight - _bottomHudMargin;

            // Clamping guarantees that all grid points [_gridMin, _gridMax] remain strictly inside the red box play viewport
            float minX = Mathf.Min(_gridMin.x + halfWidth, _gridMax.x - halfWidth);
            float maxX = Mathf.Max(_gridMin.x + halfWidth, _gridMax.x - halfWidth);

            float minY = Mathf.Min(_gridMin.y + effectiveHalfHeightBottom, _gridMax.y - effectiveHalfHeightTop);
            float maxY = Mathf.Max(_gridMin.y + effectiveHalfHeightBottom, _gridMax.y - effectiveHalfHeightTop);

            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);

            return pos;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] CameraController: {message}");
        }
    }
}
