namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.LowLevel;
    using UnityEngine.UI;

    /// <summary>Uses real queued pointer states and preserves the physical current devices and InputSettings.</summary>
    public static partial class UiRollout01Verification
    {
        private static IEnumerator Click(NativeMouse mouse, Button button, bool squeeze = false, bool legacyTextAction = false)
        {
            var rect = button != null ? button.GetComponent<RectTransform>() : null;
            if (legacyTextAction)
            {
                // Existing Profile Skip is a text-only button, without a targetGraphic or squeeze component.
                // Hit its actual raycastable glyphs; do not weaken checks on any new authored control.
                Check(button != null && button.interactable && button.gameObject.activeInHierarchy,
                    "Preserved profile text action is active");
                TMPro.TMP_Text label = null;
                foreach (var text in button.GetComponentsInChildren<TMPro.TMP_Text>(true))
                    if (Visible(text) && text.raycastTarget && !string.IsNullOrWhiteSpace(text.text)) { label = text; break; }
                Check(label != null, "Preserved profile text action has live raycastable TMP glyphs");
                rect = label.rectTransform;
            }
            else VerifyButton(button, button != null ? button.name : "missing");
            Canvas.ForceUpdateCanvases(); var canvas = button.GetComponentInParent<Canvas>();
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector2 center = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
            mouse.Send(new Vector2(2, 2), false); yield return mouse.Wait(); yield return Settled(.06f);
            float baseline = button.transform.localScale.x;
            mouse.Send(center, true); yield return mouse.Wait(); yield return Settled(.12f);
            if (squeeze) Check(button.transform.localScale.x < baseline * .99f, "Real native press squeezes the actual " + button.name);
            mouse.Send(center, false); yield return mouse.Wait(); yield return Settled(.25f);
        }

        private sealed class NativeMouse : IDisposable
        {
            private readonly Mouse _previousMouse, _mouse;
            private readonly InputDevice _previousPointer;
            private readonly InputSettings _original, _temporary;
            private readonly GameObject _selection;
            private Vector2 _position;
            private bool _pressed, _pending, _acknowledged, _disposed;
            private int _frame;
            internal NativeMouse()
            {
                _previousMouse = Mouse.current; _previousPointer = Pointer.current; _original = InputSystem.settings;
                _selection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
                try
                {
                    if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
                    _temporary = UnityEngine.Object.Instantiate(_original); _temporary.hideFlags = HideFlags.HideAndDontSave;
                    _temporary.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    _temporary.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    InputSystem.settings = _temporary; _mouse = InputSystem.AddDevice<Mouse>("UiRollout01Mouse");
                    InputSystem.onAfterUpdate += AfterUpdate;
                }
                catch { Dispose(); throw; }
            }
            internal void Send(Vector2 position, bool pressed)
            {
                if (_pending) throw new InvalidOperationException("A previous native input event is still pending.");
                _position = position; _pressed = pressed; _pending = true; _acknowledged = false; _frame = -1;
                InputSystem.QueueStateEvent(_mouse, new MouseState { position = position }.WithButton(MouseButton.Left, pressed));
            }
            internal IEnumerator Wait()
            {
                yield return (Func<bool>)(() => _acknowledged && Time.frameCount > _frame + 1);
                _pending = false;
            }
            private void AfterUpdate()
            {
                if (InputState.currentUpdateType != InputUpdateType.Dynamic || !_pending || _acknowledged ||
                    _mouse.leftButton.isPressed != _pressed || (_mouse.position.ReadValue() - _position).sqrMagnitude > .01f) return;
                _acknowledged = true; _frame = Time.frameCount;
            }
            public void Dispose()
            {
                if (_disposed) return; _disposed = true; InputSystem.onAfterUpdate -= AfterUpdate;
                var failures = new System.Collections.Generic.List<Exception>();
                RestoreAttempt(failures, () => { if (_mouse != null && _mouse.added) InputSystem.RemoveDevice(_mouse); }, "temporary input device");
                RestoreAttempt(failures, () => InputSystem.settings = _original, "original InputSettings");
                RestoreAttempt(failures, () => { if (_temporary != null) UnityEngine.Object.DestroyImmediate(_temporary); }, "temporary InputSettings copy");
                RestoreAttempt(failures, () => { if (_previousMouse != null && _previousMouse.added) _previousMouse.MakeCurrent(); }, "original current mouse");
                RestoreAttempt(failures, () => { if (_previousPointer != null && _previousPointer.added) _previousPointer.MakeCurrent(); }, "original current pointer");
                RestoreAttempt(failures, () =>
                {
                    if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(
                        _selection != null && _selection.activeInHierarchy ? _selection : null);
                }, "original pointer selection");
                if (failures.Count > 0) throw new AggregateException("Native pointer restoration failed.", failures);
                Check(ReferenceEquals(InputSystem.settings, _original) && (_mouse == null || !_mouse.added), "Native pointer settings and temporary device restored");
                Check((_previousMouse == null || !_previousMouse.added || Mouse.current == _previousMouse) &&
                    (_previousPointer == null || !_previousPointer.added || Pointer.current == _previousPointer), "Original current mouse/pointer restored");
            }
        }
    }
}
