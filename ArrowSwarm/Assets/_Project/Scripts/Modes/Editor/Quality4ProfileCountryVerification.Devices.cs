namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.LowLevel;

    /// <summary>Owns a temporary native mouse and acknowledges actual Dynamic player updates, not separate Editor buffers.</summary>
    public static partial class Quality4ProfileCountryVerification
    {
        private sealed class Devices : IDisposable
        {
            private readonly Mouse _previousMouse, _mouse;
            private readonly InputDevice _previousPointer;
            private readonly InputSettings _original, _temporary;
            private Vector2 _position;
            private bool _pressed, _pending, _acknowledged;
            private int _ackFrame = -1, _updates;
            private bool _disposed;

            internal Devices()
            {
                _previousMouse = Mouse.current; _previousPointer = Pointer.current; _original = InputSystem.settings;
                _temporary = UnityEngine.Object.Instantiate(_original); _temporary.hideFlags = HideFlags.HideAndDontSave;
                _temporary.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                _temporary.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                InputSystem.settings = _temporary; _mouse = InputSystem.AddDevice<Mouse>("QualityPass4ProfileMouse");
                InputSystem.onAfterUpdate += AfterUpdate;
            }

            internal void Send(Vector2 position, bool pressed)
            {
                if (_pending) throw new InvalidOperationException("The previous native mouse state has not been acknowledged.");
                _position = position; _pressed = pressed; _pending = true; _acknowledged = false; _ackFrame = -1;
                InputSystem.QueueStateEvent(_mouse, new MouseState { position = position }.WithButton(MouseButton.Left, pressed));
            }

            internal IEnumerator WaitForEvent()
            {
                yield return (Func<bool>)(() => _acknowledged && Time.frameCount > _ackFrame + 1);
                Results.Add("INPUT ACK: native mouse " + (_pressed ? "down" : "up") + "; dynamicFrame=" + _ackFrame +
                    "; settledPlayerFrame=" + Time.frameCount + "; dynamicUpdates=" + _updates);
                _pending = false;
            }

            private void AfterUpdate()
            {
                if (InputState.currentUpdateType != InputUpdateType.Dynamic) return;
                _updates++;
                if (!_pending || _acknowledged || _mouse.leftButton.isPressed != _pressed ||
                    (_mouse.position.ReadValue() - _position).sqrMagnitude > .01f) return;
                _acknowledged = true; _ackFrame = Time.frameCount;
            }

            public void Dispose()
            {
                if (_disposed) return; _disposed = true;
                InputSystem.onAfterUpdate -= AfterUpdate;
                if (_mouse != null && _mouse.added) InputSystem.RemoveDevice(_mouse);
                InputSystem.settings = _original;
                if (_temporary != null) UnityEngine.Object.DestroyImmediate(_temporary);
                if (_previousMouse != null && _previousMouse.added) _previousMouse.MakeCurrent();
                if (_previousPointer != null && _previousPointer.added) _previousPointer.MakeCurrent();
                Check(ReferenceEquals(InputSystem.settings, _original), "Exact original InputSettings restored");
                Check(_mouse == null || !_mouse.added, "Temporary native mouse removed");
            }
        }
    }
}
