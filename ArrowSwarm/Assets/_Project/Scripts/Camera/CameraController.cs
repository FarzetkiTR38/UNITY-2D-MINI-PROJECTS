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
        [SerializeField] private float _panSpeed = 10f;
        [SerializeField] private float _zoomSpeed = 0.5f;
        [SerializeField] private float _smoothTime = 0.1f;
        [SerializeField] private float _padding = 1f;

        private UnityEngine.Camera _camera;
        private float _defaultOrthoSize;
        private float _minOrthoSize;
        private float _maxOrthoSize;
        private Vector2 _mapCenter;
        private Vector2 _mapExtents;

        // Touch tracking
        private bool _isPanning;
        private Vector2 _lastPanPosition;
        private float _lastPinchDistance;
        private bool _isPinching;

        // Smooth zoom
        private float _targetOrthoSize;
        private float _zoomVelocity;

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
            float mapWidth = mapData.GridWidth * mapData.CellSize + _padding * 2;
            float mapHeight = mapData.GridHeight * mapData.CellSize + _padding * 2;

            _mapCenter = mapData.GridOrigin + new Vector2(
                mapData.GridWidth * mapData.CellSize * 0.5f,
                mapData.GridHeight * mapData.CellSize * 0.5f);

            _mapExtents = new Vector2(mapWidth * 0.5f, mapHeight * 0.5f);

            // Calculate ortho size to fit entire map
            float aspect = (float)Screen.width / Screen.height;
            float orthoWidth = mapWidth / (2f * aspect);
            float orthoHeight = mapHeight / 2f;
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
                     $"ZoomRange=[{_minOrthoSize:F1}, {_maxOrthoSize:F1}]");
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
                _isPanning = false;
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

            if (!_isPanning)
            {
                _isPanning = true;
                _lastPanPosition = touchPos;
                return;
            }

            Vector2 delta = touchPos - _lastPanPosition;
            Vector3 worldDelta = _camera.ScreenToWorldPoint(Vector3.zero) -
                                 _camera.ScreenToWorldPoint(new Vector3(delta.x, delta.y, 0));
            PanCamera(worldDelta);
            _lastPanPosition = touchPos;
        }

        private void HandleMouseInput()
        {
            // Mouse scroll zoom (editor testing)
            var mouse = Mouse.current;
            if (mouse == null) return;

            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                _targetOrthoSize -= scroll * _zoomSpeed * 0.1f;
                _targetOrthoSize = Mathf.Clamp(_targetOrthoSize, _minOrthoSize, _maxOrthoSize);
            }

            // Right-click drag pan (editor testing)
            if (mouse.rightButton.isPressed)
            {
                Vector2 mouseDelta = mouse.delta.ReadValue();
                Vector3 worldDelta = _camera.ScreenToWorldPoint(Vector3.zero) -
                                     _camera.ScreenToWorldPoint(new Vector3(mouseDelta.x, mouseDelta.y, 0));
                PanCamera(worldDelta);
            }
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
                OnZoomChanged?.Invoke(ZoomNormalized);
            }
        }

        private Vector3 ClampPosition(Vector3 pos)
        {
            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;

            float minX = _mapCenter.x - _mapExtents.x + halfWidth;
            float maxX = _mapCenter.x + _mapExtents.x - halfWidth;
            float minY = _mapCenter.y - _mapExtents.y + halfHeight;
            float maxY = _mapCenter.y + _mapExtents.y - halfHeight;

            // If camera view is larger than map, center it
            if (minX > maxX) pos.x = _mapCenter.x;
            else pos.x = Mathf.Clamp(pos.x, minX, maxX);

            if (minY > maxY) pos.y = _mapCenter.y;
            else pos.y = Mathf.Clamp(pos.y, minY, maxY);

            return pos;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] CameraController: {message}");
        }
    }
}
