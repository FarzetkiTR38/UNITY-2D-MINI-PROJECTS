namespace ArrowSwarm.UI
{
    using ArrowSwarm.Puzzle;
    using UnityEngine;
    using UnityEngine.EventSystems;

    /// <summary>Wide rotation track accepts one horizontal gesture per pointer release.</summary>
    public sealed class FrameSwipeUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        private Vector2 _start;
        private int _pointerId = int.MinValue;
        /// <summary>Starts a gesture only when the puzzle accepts interaction.</summary>
        public void OnPointerDown(PointerEventData e)
        { if (PuzzleSession.Active != null && PuzzleSession.Active.Ready) { _start=e.position; _pointerId=e.pointerId; } }
        /// <summary>Captures drag so camera input cannot consume this UI gesture.</summary>
        public void OnDrag(PointerEventData e) { }
        /// <summary>Converts a deliberate horizontal release into a single quarter turn.</summary>
        public void OnPointerUp(PointerEventData e)
        {
            if (e.pointerId != _pointerId) return;
            _pointerId = int.MinValue;
            var session = PuzzleSession.Active;
            if (session == null) return;
            Vector2 delta=e.position-_start;
            float scale = GetComponentInParent<Canvas>().scaleFactor;
            if (Mathf.Abs(delta.x) >= session.Config.SwipeThreshold*scale && Mathf.Abs(delta.x)>Mathf.Abs(delta.y)) session.Rotate(delta.x>0?1:-1);
        }
        private void OnDisable() { _pointerId = int.MinValue; }
    }
}
