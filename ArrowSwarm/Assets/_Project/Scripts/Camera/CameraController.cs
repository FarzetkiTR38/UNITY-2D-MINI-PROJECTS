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
    public class CameraController : MonoBehaviour
    {
        private static CameraController _instance;

        /// <summary>Global access point to the active scene's CameraController.</summary>
        public static CameraController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<CameraController>(FindObjectsInactive.Include);
                }
                return _instance;
            }
        }

        /// <summary>True if a valid CameraController instance exists in the active scene.</summary>
        public static bool HasInstance => _instance != null || FindFirstObjectByType<CameraController>() != null;

        [SerializeField] private float _zoomSpeed = 0.5f;
        [SerializeField] private float _smoothTime = 0.1f;
        [SerializeField] private float _padding = 1f;
        [SerializeField] private float _topHudMargin = 1.5f;
        [SerializeField] private float _bottomHudMargin = 1.8f;

        [Header("HUD Boundaries")]
        [SerializeField] private RectTransform _topPanelRect;
        [SerializeField] private RectTransform _bottomPanelRect;
        [SerializeField] private float _fallbackTopMarginRatio = 0.15f;
        [SerializeField] private float _fallbackBottomMarginRatio = 0.15f;

        private UnityEngine.Camera _camera;
        private UnityEngine.Camera Cam => _camera != null ? _camera : (_camera = GetComponent<UnityEngine.Camera>() ?? UnityEngine.Camera.main);
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
        private bool _isTouchDragValid;
        private bool _isMouseDragValid;
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

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _camera = GetComponent<UnityEngine.Camera>() ?? UnityEngine.Camera.main;
            AutoFindHudPanels();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// Automatically locates TopBar and BottomBar RectTransforms if not explicitly set.
        /// </summary>
        private void AutoFindHudPanels()
        {
            if (_topPanelRect != null && _bottomPanelRect != null) return;

            var hud = FindFirstObjectByType<ArrowSwarm.UI.GameHUD>();
            if (hud != null)
            {
                if (_topPanelRect == null) _topPanelRect = hud.TopPanelRect;
                if (_bottomPanelRect == null) _bottomPanelRect = hud.BottomPanelRect;
            }

            if (_topPanelRect != null && _bottomPanelRect != null) return;

            var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var canvas in canvases)
            {
                if (canvas == null) continue;
                if (_topPanelRect == null)
                {
                    Transform t = canvas.transform.Find("TopBar") ?? canvas.transform.Find("TopPanel") ?? canvas.transform.Find("Header");
                    if (t != null) _topPanelRect = t as RectTransform;
                }
                if (_bottomPanelRect == null)
                {
                    Transform b = canvas.transform.Find("BottomBar") ?? canvas.transform.Find("BottomPanel") ?? canvas.transform.Find("Footer");
                    if (b != null) _bottomPanelRect = b as RectTransform;
                }
            }
        }

        /// <summary>
        /// Determines whether a screen position is in the valid game area
        /// strictly between the Top HUD and Bottom HUD, and not over any UI element.
        /// </summary>
        public bool IsValidPanStartPosition(Vector2 screenPosition, int touchId = -1)
        {
            // 1. Check if pointer/touch is interacting with any UI element (slider, button, etc.)
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                bool isOverUI = touchId >= 0
                    ? UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touchId)
                    : UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

                if (isOverUI) return false;
            }

            AutoFindHudPanels();

            // 2. Check Top HUD boundary (top cutoff)
            float topLimit = Screen.height * (1f - _fallbackTopMarginRatio);
            if (_topPanelRect != null && _topPanelRect.gameObject.activeInHierarchy)
            {
                Vector3[] topCorners = new Vector3[4];
                _topPanelRect.GetWorldCorners(topCorners);
                topLimit = Mathf.Min(topCorners[0].y, topCorners[3].y);
            }

            // 3. Check Bottom HUD boundary (bottom cutoff)
            float bottomLimit = Screen.height * _fallbackBottomMarginRatio;
            if (_bottomPanelRect != null && _bottomPanelRect.gameObject.activeInHierarchy)
            {
                Vector3[] bottomCorners = new Vector3[4];
                _bottomPanelRect.GetWorldCorners(bottomCorners);
                bottomLimit = Mathf.Max(bottomCorners[1].y, bottomCorners[2].y);
            }

            // Must be strictly between bottomLimit and topLimit
            if (screenPosition.y >= topLimit || screenPosition.y <= bottomLimit)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the normalized screen height fractions occupied by Top HUD and Bottom HUD.
        /// TopBar is typically ~8.5% of portrait screen height, BottomBar is ~13%.
        /// </summary>
        public void GetHudRatios(out float topRatio, out float bottomRatio)
        {
            AutoFindHudPanels();

            // Calibrated reference ratios for 1080x1920 UI:
            topRatio = 0.085f;
            if (_topPanelRect != null && _topPanelRect.gameObject.activeInHierarchy && Screen.height > 0)
            {
                Vector3[] topCorners = new Vector3[4];
                _topPanelRect.GetWorldCorners(topCorners);
                float topLimit = Mathf.Min(topCorners[0].y, topCorners[3].y);
                float measuredRatio = (Screen.height - topLimit) / (float)Screen.height;
                if (measuredRatio > 0.03f && measuredRatio < 0.30f)
                {
                    topRatio = measuredRatio;
                }
            }

            bottomRatio = 0.130f;
            if (_bottomPanelRect != null && _bottomPanelRect.gameObject.activeInHierarchy && Screen.height > 0)
            {
                Vector3[] bottomCorners = new Vector3[4];
                _bottomPanelRect.GetWorldCorners(bottomCorners);
                float bottomLimit = Mathf.Max(bottomCorners[1].y, bottomCorners[2].y);
                float measuredRatio = bottomLimit / (float)Screen.height;
                if (measuredRatio > 0.05f && measuredRatio < 0.35f)
                {
                    bottomRatio = measuredRatio;
                }
            }
        }

        /// <summary>
        /// Sets up the camera to fit the map with given dimensions.
        /// Guaranteed to center the map with EXACTLY equal vertical gaps above and below,
        /// and perfectly equal horizontal gaps on the left and right.
        /// </summary>
        public void FitToMap(MapData mapData)
        {
            float spacing = ArrowSwarm.Grid.GridManager.HasInstance
                ? ArrowSwarm.Grid.GridManager.Instance.PointSpacing
                : 1.0f;
            Vector2 origin = ArrowSwarm.Grid.GridManager.HasInstance
                ? ArrowSwarm.Grid.GridManager.Instance.Origin
                : new Vector2(-((mapData.GridWidth - 1) * spacing) / 2f, -((mapData.GridHeight - 1) * spacing) / 2f);

            float totalGridWidth = (mapData.GridWidth - 1) * spacing;
            float totalGridHeight = (mapData.GridHeight - 1) * spacing;

            _mapCenter = origin + new Vector2(totalGridWidth * 0.5f, totalGridHeight * 0.5f);

            // Full visual dimensions of outer track container card (Layer 2)
            float scaleFactor = DifficultyCalculator.GetMapScaleFactor(mapData.GridWidth, mapData.GridHeight);
            float cardMargin = 0.50f * spacing;
            float halfTrackWidth = 0.60f * scaleFactor * spacing;
            float boardPadding = cardMargin + 2f * halfTrackWidth;

            float visualBoardWidth = totalGridWidth + 2f * boardPadding;
            float visualBoardHeight = totalGridHeight + 2f * boardPadding;

            _mapExtents = new Vector2(visualBoardWidth * 0.5f, visualBoardHeight * 0.5f);

            _gridMin = origin - new Vector2(boardPadding, boardPadding);
            _gridMax = origin + new Vector2(totalGridWidth + boardPadding, totalGridHeight + boardPadding);

            float aspect = (Screen.height > 0) ? ((float)Screen.width / Screen.height) : (9f / 16f);
            if (float.IsNaN(aspect) || float.IsInfinity(aspect) || aspect <= 0.01f) aspect = 9f / 16f;

            // Target playable coverage:
            // 92% width coverage fills the screen comfortably while keeping clean ~4% side margins.
            // 72% max height coverage guarantees generous margins, completely clearing HUDs.
            const float targetWidthRatio = 0.92f;
            const float maxTargetHeightRatio = 0.72f;

            float orthoFromWidth = visualBoardWidth / (2f * aspect * targetWidthRatio);
            float orthoFromHeight = visualBoardHeight / (2f * maxTargetHeightRatio);
            _defaultOrthoSize = Mathf.Max(orthoFromWidth, orthoFromHeight);
            if (float.IsNaN(_defaultOrthoSize) || float.IsInfinity(_defaultOrthoSize) || _defaultOrthoSize <= 0.1f)
            {
                _defaultOrthoSize = 10f;
            }

            GameConfig config = GameManager.Instance?.Config;
            float maxZoom = config?.MaxZoom ?? 3f;
            float minZoom = config?.MinZoom ?? 1f;

            _maxOrthoSize = _defaultOrthoSize / minZoom;
            _minOrthoSize = _defaultOrthoSize / maxZoom;

            if (Cam != null) Cam.orthographicSize = _defaultOrthoSize;
            _targetOrthoSize = _defaultOrthoSize;

            float camZ = float.IsNaN(transform.position.z) ? -10f : transform.position.z;
            if (Mathf.Abs(camZ) < 0.1f) camZ = -10f;

            transform.position = new Vector3(_mapCenter.x, _mapCenter.y, camZ);

            LogDebug($"Camera fit: Center={_mapCenter}, OrthoSize={_defaultOrthoSize}, " +
                     $"BoardSize={visualBoardWidth}x{visualBoardHeight}");
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
                _isTouchDragValid = false;
            }
        }

        private void HandlePinchZoom(Touchscreen touchscreen)
        {
            var touch0 = touchscreen.touches[0];
            var touch1 = touchscreen.touches[1];

            Vector2 pos0 = touch0.position.ReadValue();
            Vector2 pos1 = touch1.position.ReadValue();

            if (!_isPinching)
            {
                if (!IsValidPanStartPosition(pos0, touch0.touchId.ReadValue()) ||
                    !IsValidPanStartPosition(pos1, touch1.touchId.ReadValue()))
                {
                    return;
                }

                _isPinching = true;
                _lastPinchDistance = Vector2.Distance(pos0, pos1);
                _isPanning = false;
                _isTouchDragValid = false;
                return;
            }

            float currentDistance = Vector2.Distance(pos0, pos1);
            float delta = currentDistance - _lastPinchDistance;
            _targetOrthoSize -= delta * _zoomSpeed * 0.01f;
            _targetOrthoSize = Mathf.Clamp(_targetOrthoSize, _minOrthoSize, _maxOrthoSize);
            _lastPinchDistance = currentDistance;
        }

        private void HandleTouchPan(Touchscreen touchscreen)
        {
            var touch = touchscreen.touches[0];
            Vector2 touchPos = touch.position.ReadValue();
            var phase = touch.phase.ReadValue();

            if (phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                _isTouchDragValid = IsValidPanStartPosition(touchPos, touch.touchId.ReadValue());
                if (!_isTouchDragValid)
                {
                    _isPanning = false;
                    _isDragging = false;
                    return;
                }

                _dragStartScreenPos = touchPos;
                _lastPanPosition = touchPos;
                _isPanning = false;
                _isDragging = false;
                return;
            }

            if (!_isTouchDragValid) return;

            if (phase == UnityEngine.InputSystem.TouchPhase.Moved || phase == UnityEngine.InputSystem.TouchPhase.Stationary)
            {
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
                    float unitsPerPixel = (Cam.orthographicSize * 2f) / Mathf.Max(1f, Screen.height);
                    Vector3 worldDelta = new Vector3(-delta.x * unitsPerPixel, -delta.y * unitsPerPixel, 0f);
                    PanCamera(worldDelta);
                    _lastPanPosition = touchPos;
                }
            }
            else if (phase == UnityEngine.InputSystem.TouchPhase.Ended || phase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                _isTouchDragValid = false;
                if (_isPanning)
                {
                    _isPanning = false;
                    StartCoroutine(ResetDraggingNextFrame());
                }
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
                Vector2 mousePos = mouse.position.ReadValue();
                _isMouseDragValid = IsValidPanStartPosition(mousePos, -1);
                if (!_isMouseDragValid)
                {
                    _isPanning = false;
                    _isDragging = false;
                    return;
                }

                _dragStartScreenPos = mousePos;
                _lastPanPosition = _dragStartScreenPos;
                _isPanning = false;
                _isDragging = false;
            }
            else if (_isMouseDragValid && (mouse.leftButton.isPressed || mouse.rightButton.isPressed))
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
                    float unitsPerPixel = (Cam.orthographicSize * 2f) / Mathf.Max(1f, Screen.height);
                    Vector3 worldDelta = new Vector3(-delta.x * unitsPerPixel, -delta.y * unitsPerPixel, 0f);
                    PanCamera(worldDelta);
                    _lastPanPosition = mousePos;
                }
            }
            else if (mouse.leftButton.wasReleasedThisFrame || mouse.rightButton.wasReleasedThisFrame)
            {
                _isMouseDragValid = false;
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

            GetHudRatios(out float topRatio, out float bottomRatio);
            float topMarginWorld = halfHeight * 2f * topRatio;
            float bottomMarginWorld = halfHeight * 2f * bottomRatio;

            // Effective viewport height margins below Top HUD and above Bottom HUD
            float effectiveHalfHeightTop = halfHeight - topMarginWorld;
            float effectiveHalfHeightBottom = halfHeight - bottomMarginWorld;

            // Clamping guarantees that all grid points [_gridMin, _gridMax] remain strictly inside the visible play viewport
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
