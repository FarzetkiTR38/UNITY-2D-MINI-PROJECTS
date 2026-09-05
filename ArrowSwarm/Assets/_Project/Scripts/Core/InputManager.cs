namespace ArrowSwarm.Core
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Utils;

    /// <summary>
    /// Handles player input globally for clicking/touching arrows
    /// using the New Input System with strict UI and scene transition bleed-through prevention.
    /// </summary>
    public class InputManager : Singleton<InputManager>
    {
        private const float DefaultBlockDuration = 0.35f;
        private const float MaxTapDistance = 30f;

        private Camera _mainCamera;
        private float _inputBlockedUntilTime;
        private bool _isGameplayPressValid;
        private bool _isPointerDown;
        private Vector2 _pointerDownPosition;

        private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>(8);
        private PointerEventData _cachedPointerData;

        private Camera MainCamera => _mainCamera != null ? _mainCamera : (_mainCamera = Camera.main ?? FindFirstObjectByType<Camera>());

        protected override void OnSingletonAwake() => BlockInput(DefaultBlockDuration);

        private void OnEnable()
        {
            GameManager.OnGameStateChanged += HandleGameStateChanged;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            GameManager.OnGameStateChanged -= HandleGameStateChanged;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            BlockInput(DefaultBlockDuration);
        }

        /// <summary>Blocks gameplay arrow clicks for the specified duration (seconds).</summary>
        public void BlockInput(float duration = DefaultBlockDuration)
        {
            _inputBlockedUntilTime = Mathf.Max(_inputBlockedUntilTime, Time.unscaledTime + duration);
            ResetPointerState();
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.Playing) BlockInput(DefaultBlockDuration);
            else ResetPointerState();
        }

        private void ResetPointerState()
        {
            _isGameplayPressValid = false;
            _isPointerDown = false;
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing ||
                Time.unscaledTime < _inputBlockedUntilTime || MainCamera == null ||
                (ArrowSwarm.Camera.CameraController.Instance != null && ArrowSwarm.Camera.CameraController.Instance.IsDragging))
            {
                ResetPointerState();
                return;
            }

            HandleMouseInput();
            HandleTouchInput();
        }

        private void HandleMouseInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;
            Vector2 pos = mouse.position.ReadValue();

            if (mouse.leftButton.wasPressedThisFrame) OnPointerDown(pos, -1);
            else if (_isPointerDown && mouse.leftButton.isPressed) OnPointerMove(pos);

            if (mouse.leftButton.wasReleasedThisFrame) OnPointerUp(pos, -1);
        }

        private void HandleTouchInput()
        {
            var ts = Touchscreen.current;
            if (ts == null || ts.touches.Count == 0) return;

            var touch = ts.touches[0];
            var phase = touch.phase.ReadValue();
            Vector2 pos = touch.position.ReadValue();
            int id = touch.touchId.ReadValue();

            if (phase == UnityEngine.InputSystem.TouchPhase.Began) OnPointerDown(pos, id);
            else if (_isPointerDown && phase == UnityEngine.InputSystem.TouchPhase.Moved) OnPointerMove(pos);
            else if (phase == UnityEngine.InputSystem.TouchPhase.Ended || phase == UnityEngine.InputSystem.TouchPhase.Canceled) OnPointerUp(pos, id);
        }

        private void OnPointerDown(Vector2 screenPos, int touchId)
        {
            _isPointerDown = true;
            _pointerDownPosition = screenPos;
            _isGameplayPressValid = !IsPointerOverUI(screenPos, touchId);
        }

        private void OnPointerMove(Vector2 screenPos)
        {
            if (_isGameplayPressValid && Vector2.Distance(screenPos, _pointerDownPosition) > MaxTapDistance)
            {
                _isGameplayPressValid = false;
            }
        }

        private void OnPointerUp(Vector2 screenPos, int touchId)
        {
            bool wasValid = _isGameplayPressValid;
            ResetPointerState();

            if (!wasValid || IsPointerOverUI(screenPos, touchId)) return;
            if (Vector2.Distance(screenPos, _pointerDownPosition) > MaxTapDistance) return;

            ProcessClick(screenPos);
        }

        private bool IsPointerOverUI(Vector2 screenPos, int touchId = -1)
        {
            var es = EventSystem.current;
            if (es == null) return false;
            if (touchId >= 0 && es.IsPointerOverGameObject(touchId)) return true;
            if (es.IsPointerOverGameObject()) return true;

            if (_cachedPointerData == null) _cachedPointerData = new PointerEventData(es);
            _cachedPointerData.position = screenPos;
            _raycastResults.Clear();
            es.RaycastAll(_cachedPointerData, _raycastResults);
            return _raycastResults.Count > 0;
        }

        private void ProcessClick(Vector2 screenPosition)
        {
            if (MainCamera == null) return;
            float camDist = Mathf.Abs(MainCamera.transform.position.z);
            if (camDist < 0.1f) camDist = 10f;
            Vector2 worldPos = MainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, camDist));

            // 1. Grid-based lookup: Find arrow occupying clicked cell
            if (GridManager.Instance != null && ArrowSpawner.Instance != null)
            {
                var gPoint = GridManager.Instance.WorldToPoint(worldPos);
                if (gPoint != null)
                {
                    var arrow = ArrowSpawner.Instance.GetArrowAt(gPoint.GridPosition);
                    if (arrow != null && !arrow.IsFired && IsAllowedInTutorial(arrow))
                    {
                        arrow.OnPlayerClick();
                        return;
                    }
                }
            }

            // 2. Physics Raycast fallback for off-grid touch
            RaycastHit2D[] hits = Physics2D.RaycastAll(worldPos, Vector2.zero);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider != null)
                {
                    var arrow = hits[i].collider.GetComponentInParent<Arrow>();
                    if (arrow != null && !arrow.IsFired && IsAllowedInTutorial(arrow))
                    {
                        arrow.OnPlayerClick();
                        break;
                    }
                }
            }
        }

        private bool IsAllowedInTutorial(Arrow arrow)
        {
            var tut = ArrowSwarm.Tutorial.TutorialManager.Instance;
            if (tut != null && tut.IsTutorialActive)
            {
                return arrow.HeadPoint == tut.CurrentTargetGridPos ||
                       GridManager.Instance.IsPathClear(arrow.HeadPoint, arrow.HeadDirection);
            }
            return true;
        }
    }
}
