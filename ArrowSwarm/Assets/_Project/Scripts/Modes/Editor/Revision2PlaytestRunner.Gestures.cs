namespace ArrowSwarm.Modes.Editor
{
    using System.Collections;
    using ArrowSwarm.UI;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public static partial class Revision2PlaytestRunner
    {
        private static GameObject _pressed;
        private static PointerEventData _pointer;
        private static GameObject _lastGestureObject;
        private static PointerEventData _lastGesturePointer;
        private static Vector2 Screen(RectTransform rect, Vector2 local) => RectTransformUtility.WorldToScreenPoint(
            rect.GetComponentInParent<Canvas>().worldCamera, rect.TransformPoint(local));

        private static void Gesture(RectTransform track, Vector2 start, Vector2 end)
        {
            _pressed = track.gameObject;
            _pointer = new PointerEventData(EventSystem.current) { pointerId = -17, button = PointerEventData.InputButton.Left,
                position = Screen(track, start), eligibleForClick = true, pointerEnter = track.gameObject };
            ExecuteEvents.Execute(_pressed, _pointer, ExecuteEvents.pointerDownHandler);
            _pointer.position = Screen(track, end); ExecuteEvents.Execute(_pressed, _pointer, ExecuteEvents.dragHandler);
            ExecuteEvents.Execute(_pressed, _pointer, ExecuteEvents.pointerUpHandler);
            // A second release from the same captured pointer must never consume another turn.
            ExecuteEvents.Execute(_pressed, _pointer, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(_pressed, _pointer, ExecuteEvents.pointerExitHandler);
            _lastGestureObject = _pressed; _lastGesturePointer = _pointer;
            _pressed = null; _pointer = null;
        }

        private static void ReplayGestureRelease()
        { if (_lastGestureObject != null) ExecuteEvents.Execute(_lastGestureObject, _lastGesturePointer, ExecuteEvents.pointerUpHandler); }

        private static IEnumerator RocketFeedback()
        {
            var button = Rocket; var image = button.targetGraphic as Image; var effect = button.GetComponent<UIButtonFeedback>();
            Check(image != null && image.sprite != null && effect != null, "Rocket: visible authored image has existing button feedback");
            var target = Ref<Transform>(effect, "_targetTransform") ?? button.transform;
            Vector3 scale = target.localScale; Color color = image.canvasRenderer.GetColor();
            _pressed = button.gameObject;
            _pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left, pointerEnter = _pressed };
            ExecuteEvents.Execute(_pressed, _pointer, ExecuteEvents.pointerDownHandler); yield return .13f;
            bool squeezed = target.localScale.sqrMagnitude < scale.sqrMagnitude * .98f;
            Color pressed = image.canvasRenderer.GetColor(); bool darker = pressed.r + pressed.g + pressed.b < color.r + color.g + color.b - .03f;
            ReleasePointer(); yield return .25f;
            Check(squeezed && darker && Vector3.Distance(target.localScale, scale) < .002f,
                "Rocket: held pointer visibly scales/tints its artwork, release restores authored scale");
        }

        private static void ReleasePointer()
        {
            if (_pressed != null && _pointer != null)
            {
                ExecuteEvents.Execute(_pressed, _pointer, ExecuteEvents.pointerUpHandler);
                ExecuteEvents.Execute(_pressed, _pointer, ExecuteEvents.pointerExitHandler);
            }
            _pressed = null; _pointer = null;
        }
    }
}
