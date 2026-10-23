namespace ArrowSwarm.Core
{
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;

    /// <summary>Owns one real touch ID and discards canceled or multi-finger gestures until every finger lifts.</summary>
    public partial class InputManager
    {
        private int _touchPointerId = -1;
        private bool _touchGestureBlocked;

        private void HandleTouchInput()
        {
            var screen = Touchscreen.current;
            if (screen == null) { _touchPointerId = -1; _touchGestureBlocked = false; return; }
            TouchControl active = null, owned = null;
            int count = 0;
            foreach (var touch in screen.touches)
            {
                if (touch.touchId.ReadValue() == _touchPointerId) owned = touch;
                if (!touch.press.isPressed) continue;
                active = touch; count++;
            }
            if (count > 1) { _touchGestureBlocked = true; CancelTouchPointer(); return; }
            if (_touchGestureBlocked)
            {
                CancelTouchPointer();
                if (count == 0) _touchGestureBlocked = false;
                return;
            }
            if (owned != null)
            {
                var phase = owned.phase.ReadValue();
                if (phase == TouchPhase.Canceled) { CancelTouchPointer(); return; }
                if (phase == TouchPhase.Ended)
                {
                    OnPointerUp(owned.position.ReadValue(), _touchPointerId);
                    _touchPointerId = -1; return;
                }
            }
            if (active == null) return;
            int id = active.touchId.ReadValue();
            if (active.phase.ReadValue() == TouchPhase.Began)
            {
                CancelTouchPointer(); _touchPointerId = id;
                OnPointerDown(active.position.ReadValue(), id);
            }
            else if (_touchPointerId == id && _isPointerDown && active.phase.ReadValue() == TouchPhase.Moved)
                OnPointerMove(active.position.ReadValue());
        }

        private void CancelTouchPointer() { _touchPointerId = -1; ResetPointerState(); }
    }
}
