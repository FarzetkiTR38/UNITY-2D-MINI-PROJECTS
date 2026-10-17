namespace ArrowSwarm.Puzzle
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;

    /// <summary>Tap input respects UI, pinch gestures, camera drags and puzzle animation locks.</summary>
    public sealed class PuzzleInput : MonoBehaviour
    {
        [SerializeField] private PuzzleSession _session;
        [SerializeField] private UnityEngine.Camera _camera;
        private readonly List<RaycastResult> _hits = new List<RaycastResult>();
        private PointerEventData _pointer;
        private Vector2 _down;
        private bool _valid;
        private void Update()
        {
            if (!_session.Ready) { _valid = false; return; }
            var touch = Touchscreen.current;
            if (touch != null)
            {
                int count = 0;
                foreach (var item in touch.touches) if (item.press.isPressed) count++;
                if (count > 1) { _valid = false; return; }
                var primary = touch.primaryTouch;
                if (primary.press.wasPressedThisFrame) Down(primary.position.ReadValue());
                if (primary.press.wasReleasedThisFrame) Up(primary.position.ReadValue());
                if (count > 0 || primary.press.wasReleasedThisFrame) return;
            }
            var mouse = Mouse.current;
            if (mouse == null) return;
            if (mouse.leftButton.wasPressedThisFrame) Down(mouse.position.ReadValue());
            if (mouse.leftButton.wasReleasedThisFrame) Up(mouse.position.ReadValue());
        }
        private void Down(Vector2 p) { _down = p; _valid = !OverUI(p); }
        private void Up(Vector2 p)
        {
            bool valid = _valid; _valid = false;
            if (!valid || OverUI(p) || Vector2.Distance(_down,p) > _session.Config.TapThreshold) return;
            if (ArrowSwarm.Camera.CameraController.Instance.IsDragging) return;
            _session.Tap(_camera.ScreenToWorldPoint(new Vector3(p.x,p.y,-_camera.transform.position.z)));
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
