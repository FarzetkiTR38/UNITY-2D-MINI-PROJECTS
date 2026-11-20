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
    public partial class InputManager : Singleton<InputManager>
    {
        private const float DefaultBlockDuration = 0.35f;

        private Camera _mainCamera;
        private float _inputBlockedUntilTime;
        private bool _isGameplayPressValid;
        private bool _isPointerDown;
        private Vector2 _pointerDownPosition;
        private float _pointerDownTime;

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
            AdvanceTouchScanWindow();
            if (ArrowSwarm.Puzzle.PuzzleSession.Active != null) { ResetPointerState(); return; }
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

        /// <param name="heldSeconds">Time the press already lasted when it was first observed.</param>
        private void OnPointerDown(Vector2 screenPos, int touchId, float heldSeconds = 0f)
        {
            _isPointerDown = true;
            _pointerDownPosition = screenPos;
            _pointerDownTime = Time.unscaledTime - Mathf.Max(0f, heldSeconds);
            _isGameplayPressValid = !IsPointerOverUI(screenPos, touchId);
        }

        private void OnPointerMove(Vector2 screenPos)
        {
            if (_isGameplayPressValid && IsPanGesture(screenPos)) _isGameplayPressValid = false;
        }

        private void OnPointerUp(Vector2 screenPos, int touchId)
        {
            bool wasValid = _isGameplayPressValid && !IsPanGesture(screenPos);
            Vector2 pressPosition = _pointerDownPosition;
            ResetPointerState();

            // Short holds and small pulls fire; the arrow under the original press is the target.
            if (!wasValid || IsPointerOverUI(pressPosition, touchId)) return;
            ProcessClick(pressPosition);
        }

        private bool IsPanGesture(Vector2 screenPos)
        {
            return TapGesture.IsPanIntent(Vector2.Distance(screenPos, _pointerDownPosition),
                Time.unscaledTime - _pointerDownTime);
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
