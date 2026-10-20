namespace ArrowSwarm.UI
{
    using ArrowSwarm.Puzzle;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.UI;

    /// <summary>One deliberate swipe or side tap rotates the fixed board's colored border once.</summary>
    public sealed class FrameSwipeUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] private bool _tapToRotate = true;
        [Tooltip("Total center fraction of the bar width that ignores taps.")]
        [SerializeField, Range(0f, .8f)] private float _tapCenterDeadZoneFraction = .14f;
        private Vector2 _start;
        private int _pointerId = int.MinValue;
        private int _attemptVersion;
        private PuzzleSession _owner;
        private Canvas _canvas;
        private RectTransform _rect;
        private void Awake() { _canvas = GetComponentInParent<Canvas>(); _rect = transform as RectTransform; }
        private void OnEnable() { PuzzleSession.OnChanged += SessionChanged; }

        private static bool CanRotate(PuzzleSession session) => session != null && session.Ready &&
            session.Mode == ArrowSwarm.Modes.GameModeId.ColorShift && session.Turns > 0;

        /// <summary>Starts a gesture only when the puzzle accepts interaction.</summary>
        public void OnPointerDown(PointerEventData e)
        {
            if (_pointerId != int.MinValue || e.button != PointerEventData.InputButton.Left || !CanRotate(PuzzleSession.Active)) return;
            _start = e.position; _pointerId = e.pointerId;
            _owner = PuzzleSession.Active; _attemptVersion = _owner.AttemptVersion;
        }

        /// <summary>Captures drag so camera input cannot consume this UI gesture.</summary>
        public void OnDrag(PointerEventData e)
        {
            if (e.pointerId != _pointerId || PuzzleSession.Active == null) return;
            float threshold = PuzzleSession.Active.Config.TapThreshold * CanvasScale;
            if ((e.position - _start).sqrMagnitude > threshold * threshold) e.eligibleForClick = false;
        }

        /// <summary>Converts a deliberate horizontal swipe or left/right tap into one quarter turn.</summary>
        public void OnPointerUp(PointerEventData e)
        {
            if (e.pointerId != _pointerId) return;
            var owner = _owner; int attempt = _attemptVersion;
            ResetGesture();
            var session = PuzzleSession.Active;
            if (session != owner || session == null || session.AttemptVersion != attempt || !CanRotate(session) || IsCanceledTouch(e)) return;
            Vector2 delta = e.position - _start;
            float scale = CanvasScale;
            if (Mathf.Abs(delta.x) >= session.Config.SwipeThreshold * scale && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            { e.eligibleForClick = false; session.Rotate(delta.x > 0f ? 1 : -1); return; }
            float tapThreshold = session.Config.TapThreshold * scale;
            if (!_tapToRotate || delta.sqrMagnitude > tapThreshold * tapThreshold || _rect == null ||
                !RectTransformUtility.RectangleContainsScreenPoint(_rect, e.position, e.pressEventCamera)) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, e.position, e.pressEventCamera, out Vector2 local)) return;
            float offset = local.x - _rect.rect.center.x;
            if (Mathf.Abs(offset) <= _rect.rect.width * _tapCenterDeadZoneFraction * .5f) return;
            e.eligibleForClick = false; session.Rotate(offset > 0f ? 1 : -1);
        }

        private float CanvasScale => _canvas != null ? _canvas.scaleFactor : 1f;
        /// <summary>Rotates counterclockwise for an optional authored left-side control.</summary>
        public void RotateLeft() { if (CanRotate(PuzzleSession.Active)) PuzzleSession.Active.Rotate(-1); }
        /// <summary>Rotates clockwise for an optional authored right-side control.</summary>
        public void RotateRight() { if (CanRotate(PuzzleSession.Active)) PuzzleSession.Active.Rotate(1); }
        private void SessionChanged()
        {
            var session = PuzzleSession.Active;
            if (_pointerId != int.MinValue && (session != _owner || session == null ||
                session.AttemptVersion != _attemptVersion || !CanRotate(session))) ResetGesture();
        }

        private static bool IsCanceledTouch(PointerEventData data)
        {
            if (!(data is ExtendedPointerEventData pointer) || !(pointer.control?.device is Touchscreen screen)) return false;
            foreach (var touch in screen.touches)
                if (touch.touchId.ReadValue() == pointer.touchId)
                    return touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled;
            return false;
        }

        private void ResetGesture() { _pointerId = int.MinValue; _owner = null; }
        private void OnDisable() { PuzzleSession.OnChanged -= SessionChanged; ResetGesture(); }
    }
}
