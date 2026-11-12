namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.LowLevel;

    /// <summary>Exercises actual UI pointer events while preserving input configuration and physical device ownership.</summary>
    public static partial class Revision6PauseVerification
    {
        /// <summary>Owns one temporary native mouse and acknowledges Dynamic game-loop input updates.</summary>
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
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
                _temporary = UnityEngine.Object.Instantiate(_original); _temporary.hideFlags = HideFlags.HideAndDontSave;
                _temporary.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                _temporary.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                InputSystem.settings = _temporary; _mouse = InputSystem.AddDevice<Mouse>("Revision6PauseMouse");
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
                Results.Add("INPUT ACK: native Pause/Resume mouse " + (_pressed ? "down" : "up") + "; playerFrame=" + _frame);
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
                if (_mouse != null && _mouse.added) InputSystem.RemoveDevice(_mouse);
                InputSystem.settings = _original;
                if (_temporary != null) UnityEngine.Object.DestroyImmediate(_temporary);
                if (_previousMouse != null && _previousMouse.added) _previousMouse.MakeCurrent();
                if (_previousPointer != null && _previousPointer.added) _previousPointer.MakeCurrent();
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(_selection != null && _selection.activeInHierarchy ? _selection : null);
                Check(ReferenceEquals(InputSystem.settings, _original) && (_mouse == null || !_mouse.added),
                    "Native Pause input restores original InputSettings and removes its owned mouse");
                Check((_previousMouse == null || !_previousMouse.added || Mouse.current == _previousMouse) &&
                    (_previousPointer == null || !_previousPointer.added || Pointer.current == _previousPointer),
                    "Native Pause input restores original current mouse and pointer");
            }
        }
    }
}
