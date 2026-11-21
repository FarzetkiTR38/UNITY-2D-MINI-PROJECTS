namespace ArrowSwarm.Puzzle
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;
    using ArrowSwarm.Core;

    /// <summary>Tap/flick input shares the camera's pan rules and respects UI, pinch gestures and puzzle locks.</summary>
    public sealed class PuzzleInput : MonoBehaviour
    {
        [SerializeField] private PuzzleSession _session;
        [SerializeField] private UnityEngine.Camera _camera;
        private readonly List<RaycastResult> _hits = new List<RaycastResult>();
        private PointerEventData _pointer;
        private Vector2 _down;
        private float _downTime;
        private bool _valid;
        private double _scanFloor, _lastScan, _handledStart = -1;
        private void Update()
        {
            _scanFloor = _lastScan; _lastScan = UnityEngine.InputSystem.LowLevel.InputState.currentTime;
            if (!_session.Ready) { _valid = false; return; }
            var touch = Touchscreen.current;
            if (touch != null)
            {
                int count = 0;
                foreach (var item in touch.touches) if (item.press.isPressed) count++;
                if (count > 1) { _valid = false; return; }
                var primary = touch.primaryTouch;
                if(primary.phase.ReadValue()==UnityEngine.InputSystem.TouchPhase.Canceled){_valid=false;return;}
                double start = primary.startTime.ReadValue();
                bool unseenTap = !primary.press.isPressed && !primary.press.wasReleasedThisFrame && start > _scanFloor &&
                    start != _handledStart && primary.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended;
                if (primary.press.wasPressedThisFrame || unseenTap) { Down(primary.startPosition.ReadValue()); _handledStart = start; }
                else if (primary.press.isPressed) Move(primary.position.ReadValue());
                // A press that began and ended between frames never reads as "released this frame".
                if (primary.press.wasReleasedThisFrame || unseenTap) Up(primary.position.ReadValue());
                if (count > 0 || primary.press.wasReleasedThisFrame || unseenTap) return;
            }
            var mouse = Mouse.current;
            if (mouse == null) return;
            if (mouse.leftButton.wasPressedThisFrame) Down(mouse.position.ReadValue());
            else if (mouse.leftButton.isPressed) Move(mouse.position.ReadValue());
            if (mouse.leftButton.wasReleasedThisFrame) Up(mouse.position.ReadValue());
        }
        private void Down(Vector2 p) { _down = p; _downTime = Time.unscaledTime; _valid = !OverUI(p); }
        private void Move(Vector2 p) { if (_valid && IsPan(p)) _valid = false; }
        private bool IsPan(Vector2 p) => TapGesture.IsPanIntent(Vector2.Distance(_down, p), Time.unscaledTime - _downTime);
        private void Up(Vector2 p)
        {
            // Short holds and small pulls fire the arrow under the original press.
            bool valid = _valid && !IsPan(p); _valid = false;
            if (!valid || OverUI(_down)) return;
            if (ArrowSwarm.Camera.CameraController.Instance.IsDragging) return;
            var config = GameManager.Instance != null ? GameManager.Instance.Config : null;
            float worldPerPixel = _camera.orthographicSize * 2f / Mathf.Max(1f, Screen.height);
            float finger = config != null ? TapGesture.MillimetersToPixels(config.ArrowTouchToleranceMm) * worldPerPixel : 0f;
            _session.Tap(_camera.ScreenToWorldPoint(new Vector3(_down.x,_down.y,-_camera.transform.position.z)), finger);
        }
        private bool OverUI(Vector2 p)
        {
            var system = EventSystem.current;
            if (system == null) return false;
            if (_pointer == null) _pointer = new PointerEventData(system);
            _pointer.position = p; _hits.Clear(); system.RaycastAll(_pointer,_hits);
            return _hits.Count > 0;
        }
    }
}
