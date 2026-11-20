namespace ArrowSwarm.Core
{
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;
    using UnityEngine.InputSystem.LowLevel;

    /// <summary>
    /// Owns one real touch ID and discards canceled or multi-finger gestures until every finger lifts.
    /// A touch first seen mid-press (missed Began frame) or begun and ended within one frame still counts.
    /// </summary>
    public partial class InputManager
    {
        private int _touchPointerId = -1;
        private int _lastFinishedTouchId = -1;
        private bool _touchGestureBlocked;
        private double _lastTouchScanTime, _touchScanFloor;

        /// <summary>Called every frame, even while input is blocked, so stale touches are never replayed.</summary>
        private void AdvanceTouchScanWindow()
        {
            _touchScanFloor = _lastTouchScanTime;
            _lastTouchScanTime = InputState.currentTime;
        }

        private void HandleTouchInput()
        {
            var screen = Touchscreen.current;
            if (screen == null) { _touchPointerId = -1; _touchGestureBlocked = false; return; }
            TouchControl owned = null, fresh = null, quick = null;
            int count = 0;
            foreach (var touch in screen.touches)
            {
                int id = touch.touchId.ReadValue();
                bool pressed = touch.press.isPressed;
                if (pressed) count++;
                if (id != 0 && id == _touchPointerId) owned = touch;
                else if (pressed && fresh == null) fresh = touch;
                else if (!pressed && quick == null && IsUnseenCompletedTouch(touch, id)) quick = touch;
            }
            if (count > 1) { _touchGestureBlocked = true; CancelTouchPointer(); return; }
            if (_touchGestureBlocked)
            {
                CancelTouchPointer();
                if (count == 0) _touchGestureBlocked = false;
                return;
            }
            if (owned != null) { UpdateOwnedTouch(owned); return; }
            if (_touchPointerId != -1) CancelTouchPointer();

            if (fresh != null)
            {
                _touchPointerId = fresh.touchId.ReadValue();
                OnPointerDown(fresh.startPosition.ReadValue(), _touchPointerId, HeldSeconds(fresh));
                OnPointerMove(fresh.position.ReadValue());
            }
            else if (quick != null)
            {
                // Pressed and released between two frames: treat it as a complete tap.
                int id = quick.touchId.ReadValue();
                OnPointerDown(quick.startPosition.ReadValue(), id, HeldSeconds(quick));
                OnPointerUp(quick.position.ReadValue(), id);
                _lastFinishedTouchId = id;
            }
        }

        private void UpdateOwnedTouch(TouchControl owned)
        {
            var phase = owned.phase.ReadValue();
            if (phase == TouchPhase.Canceled) { CancelTouchPointer(); return; }
            if (!owned.press.isPressed || phase == TouchPhase.Ended)
            {
                OnPointerUp(owned.position.ReadValue(), _touchPointerId);
                _lastFinishedTouchId = _touchPointerId;
                _touchPointerId = -1;
                return;
            }
            if (_isPointerDown) OnPointerMove(owned.position.ReadValue());
        }

        // A press that began and ended between two frames never reads as pressed or "released this frame".
        private bool IsUnseenCompletedTouch(TouchControl touch, int id)
        {
            return id != 0 && id != _lastFinishedTouchId && touch.phase.ReadValue() == TouchPhase.Ended &&
                touch.startTime.ReadValue() > _touchScanFloor;
        }

        private static float HeldSeconds(TouchControl touch)
        {
            return (float)System.Math.Max(0.0, InputState.currentTime - touch.startTime.ReadValue());
        }

        private void CancelTouchPointer() { _touchPointerId = -1; ResetPointerState(); }
    }
}
