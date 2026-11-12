namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.LowLevel;
    using UnityEngine.UI;

    /// <summary>Exercises actual UI pointer events while preserving input configuration and physical device ownership.</summary>
    public static partial class UiRollout08Verification
    {
        private static IEnumerator Click(NativeMouse mouse, Button button, bool checkSqueeze)
        {
            yield return Settled(.1f);
            Check(button != null && button.interactable && button.gameObject.activeInHierarchy,
                "Actual target button is active and interactable");
            Canvas.ForceUpdateCanvases();
            var rect = button.GetComponent<RectTransform>(); var canvas = button.GetComponentInParent<Canvas>();
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector2 center = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
            mouse.Send(new Vector2(2, 2), false); yield return mouse.Wait(); yield return Settled(.08f);
            Canvas.ForceUpdateCanvases();
            center = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
            var hits = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = center }, hits);
            string hit = hits.Count > 0 ? hits[0].gameObject.name : "none";
            Check(hits.Count > 0 && ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) == button.gameObject,
                "Native target raycast resolves to " + button.name + "; screen=" + Screen.width + "x" + Screen.height +
                "; center=" + center + "; firstHit=" + hit);
            Vector3 baseline = button.transform.localScale;
            mouse.Send(center, true); yield return mouse.Wait(); yield return Settled(.12f);
            if (checkSqueeze) Check(button.transform.localScale.x < baseline.x * .99f,
                "Native popup button press visibly squeezes its existing feedback");
            mouse.Send(center, false); yield return mouse.Wait(); yield return Settled(.22f);
        }

        /// <summary>Owns one temporary native mouse and acknowledges Dynamic game-loop input updates.</summary>
        private sealed class NativeMouse : IDisposable
        {
            private readonly Mouse _previousMouse, _mouse;
            private readonly InputDevice _previousPointer;
            private readonly InputSettings _original, _temporary;
            private readonly UnityEngine.InputSystem.InputActionAsset _uiActions;
            private readonly UnityEngine.InputSystem.Utilities.ReadOnlyArray<InputDevice>? _uiDevices;
            private readonly GameObject _selection;
            private Vector2 _position;
            private bool _pressed, _pending, _acknowledged, _disposed;
            private int _frame;

            internal NativeMouse()
            {
                _previousMouse = Mouse.current; _previousPointer = Pointer.current; _original = InputSystem.settings;
                _selection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
                _temporary = UnityEngine.Object.Instantiate(_original); _temporary.hideFlags = HideFlags.HideAndDontSave;
                _temporary.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                _temporary.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                InputSystem.settings = _temporary; _mouse = InputSystem.AddDevice<Mouse>("UiRollout08Mouse");
                var module = EventSystem.current != null ? EventSystem.current.currentInputModule as UnityEngine.InputSystem.UI.InputSystemUIInputModule : null;
                _uiActions = module != null ? module.actionsAsset : null;
                if (_uiActions != null)
                {
                    var devices = _uiActions.devices;
                    _uiDevices = devices.HasValue ? new UnityEngine.InputSystem.Utilities.ReadOnlyArray<InputDevice>(devices.Value.ToArray()) :
                        (UnityEngine.InputSystem.Utilities.ReadOnlyArray<InputDevice>?)null;
                    _uiActions.devices = new InputDevice[] { _mouse };
                }
                InputSystem.onAfterUpdate += AfterUpdate;
            }

            internal void Send(Vector2 position, bool pressed)
            {
                if (_pending) throw new InvalidOperationException("Previous native event was not acknowledged.");
                _position = position; _pressed = pressed; _pending = true; _acknowledged = false; _frame = -1;
                InputSystem.QueueStateEvent(_mouse, new MouseState { position = position }.WithButton(MouseButton.Left, pressed));
            }

            internal IEnumerator Wait()
            {
                yield return (Func<bool>)(() => _acknowledged && Time.frameCount > _frame + 1);
                Results.Add("INPUT ACK: native board08 mouse " + (_pressed ? "down" : "up") + "; playerFrame=" + _frame);
                _pending = false;
            }

            private void AfterUpdate()
            {
                if (InputState.currentUpdateType != InputUpdateType.Dynamic || !_pending || _acknowledged ||
                    _mouse.leftButton.isPressed != _pressed || (_mouse.position.ReadValue() - _position).sqrMagnitude > .01f) return;
                _acknowledged = true; _frame = Time.frameCount;
            }

            /// <summary>Restores the original settings/current devices and releases only the synthetic mouse.</summary>
            public void Dispose()
            {
                if (_disposed) return; _disposed = true;
                InputSystem.onAfterUpdate -= AfterUpdate;
                if (_uiActions != null) _uiActions.devices = _uiDevices;
                if (_mouse != null && _mouse.added) InputSystem.RemoveDevice(_mouse);
                InputSystem.settings = _original;
                if (_temporary != null) UnityEngine.Object.DestroyImmediate(_temporary);
                if (_previousMouse != null && _previousMouse.added) _previousMouse.MakeCurrent();
                if (_previousPointer != null && _previousPointer.added) _previousPointer.MakeCurrent();
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(_selection != null && _selection.activeInHierarchy ? _selection : null);
                Check(ReferenceEquals(InputSystem.settings, _original) && (_mouse == null || !_mouse.added),
                    "Native board08 input restores original InputSettings and removes its owned mouse");
                Check(_uiActions == null || _uiActions.devices.Equals(_uiDevices), "Original UI action device filter restored");
                Check((_previousMouse == null || !_previousMouse.added || Mouse.current == _previousMouse) &&
                    (_previousPointer == null || !_previousPointer.added || Pointer.current == _previousPointer),
                    "Native board08 input restores original current mouse and pointer");
            }
        }
    }
}
