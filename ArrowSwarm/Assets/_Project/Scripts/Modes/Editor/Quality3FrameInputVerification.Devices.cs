namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Reflection;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.LowLevel;
    using TouchPhase = UnityEngine.InputSystem.TouchPhase;

    /// <summary>Scopes synthetic devices and focus-independent test settings without changing the user's real devices.</summary>
    public static partial class Quality3FrameInputVerification
    {
        private sealed class Devices : IDisposable
        {
            internal readonly Touchscreen Screen;
            private readonly Touchscreen _previousTouch;
            private readonly Mouse _mouse, _previousMouse;
            private readonly InputSettings _originalSettings, _temporarySettings;
            private readonly List<ExpectedTouch> _pending = new List<ExpectedTouch>();
            private int _updates, _lastUpdateFrame = -1;
            private InputUpdateType _lastUpdateType;
            private readonly ObservedTouch[] _gameTouches;
            private int _lastGameFrame = -1, _lastGameDevice = -1;

            /// <summary>Copies the actual game-update state before Unity switches to its separate Editor input buffer.</summary>
            internal readonly struct ObservedTouch
            {
                internal readonly int Id;
                internal readonly TouchPhase Phase;
                internal readonly Vector2 Position;
                internal readonly bool Pressed;

                internal ObservedTouch(UnityEngine.InputSystem.Controls.TouchControl touch)
                { Id = touch.touchId.ReadValue(); Phase = touch.phase.ReadValue(); Position = touch.position.ReadValue(); Pressed = touch.press.isPressed; }
            }

            internal int SlotCount => _gameTouches.Length;
            internal ObservedTouch Slot(int index) => _gameTouches[index];

            private struct ExpectedTouch
            {
                internal int Id, AckFrame;
                internal TouchPhase Phase;
                internal Vector2 Position;
                internal bool Acknowledged;
            }

            internal string Diagnostics => "playerFrame=" + Time.frameCount + ", afterUpdates=" + _updates +
                ", lastAfterUpdateFrame=" + _lastUpdateFrame + ", lastUpdateType=" + _lastUpdateType +
                ", lastGameFrame=" + _lastGameFrame + ", lastGameCurrentDevice=" + _lastGameDevice +
                ", updateMode=" + InputSystem.settings.updateMode + ", pending=" + _pending.Count;

            internal Devices()
            {
                _previousTouch = Touchscreen.current; _previousMouse = Mouse.current;
                _originalSettings = InputSystem.settings; _temporarySettings = UnityEngine.Object.Instantiate(_originalSettings);
                _temporarySettings.hideFlags = HideFlags.HideAndDontSave;
                _temporarySettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                _temporarySettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                InputSystem.settings = _temporarySettings;
                _mouse = InputSystem.AddDevice<Mouse>("QualityPass3Mouse");
                InputSystem.QueueStateEvent(_mouse, new MouseState { position = new Vector2(UnityEngine.Screen.width * .5f, UnityEngine.Screen.height * .5f) });
                Screen = InputSystem.AddDevice<Touchscreen>("QualityPass3Touchscreen");
                _gameTouches = new ObservedTouch[Screen.touches.Count];
                InputSystem.onAfterUpdate += AfterInputUpdate;
            }

            internal void Send(int id, TouchPhase phase, Vector2 position)
            {
                _pending.Add(new ExpectedTouch { Id = id, Phase = phase, Position = position, AckFrame = -1 });
                InputSystem.QueueStateEvent(Screen, new TouchState { touchId = id, phase = phase, position = position,
                    pressure = phase == TouchPhase.Ended || phase == TouchPhase.Canceled ? 0f : 1f });
            }

            internal IEnumerator WaitForEvents()
            {
                yield return (Func<bool>)(() =>
                {
                    foreach (var item in _pending)
                        if (!item.Acknowledged || Time.frameCount <= item.AckFrame + 1) return false;
                    return true;
                });
                Results.Add("INPUT ACK: " + _pending.Count + " actual touch events; " + Diagnostics);
                _pending.Clear();
            }

            private void AfterInputUpdate()
            {
                _updates++; _lastUpdateFrame = Time.frameCount; _lastUpdateType = InputState.currentUpdateType;
                if (_lastUpdateType == InputUpdateType.Editor) return;
                _lastGameFrame = Time.frameCount; _lastGameDevice = Touchscreen.current != null ? Touchscreen.current.deviceId : -1;
                for (int index = 0; index < Screen.touches.Count; index++) _gameTouches[index] = new ObservedTouch(Screen.touches[index]);
                for (int index = 0; index < _pending.Count; index++)
                {
                    var expected = _pending[index];
                    if (expected.Acknowledged) continue;
                    foreach (var touch in _gameTouches)
                    {
                        if (touch.Id != expected.Id || touch.Phase != expected.Phase ||
                            (touch.Position - expected.Position).sqrMagnitude > .001f) continue;
                        expected.Acknowledged = true; expected.AckFrame = Time.frameCount; _pending[index] = expected; break;
                    }
                }
            }

            public void Dispose()
            {
                Results.Add("INPUT FINAL: " + Diagnostics);
                foreach (var item in _pending)
                    Results.Add("PENDING: id=" + item.Id + ", phase=" + item.Phase + ", acknowledged=" + item.Acknowledged + ", ackFrame=" + item.AckFrame);
                InputSystem.onAfterUpdate -= AfterInputUpdate;
                if (Screen != null && Screen.added) InputSystem.RemoveDevice(Screen);
                if (_mouse != null && _mouse.added) InputSystem.RemoveDevice(_mouse);
                InputSystem.settings = _originalSettings;
                UnityEngine.Object.DestroyImmediate(_temporarySettings);
                if (_previousTouch != null && _previousTouch.added) _previousTouch.MakeCurrent();
                if (_previousMouse != null && _previousMouse.added) _previousMouse.MakeCurrent();
            }
        }

        private sealed class CameraPose : IDisposable
        {
            private readonly ArrowSwarm.Camera.CameraController _controller;
            private readonly UnityEngine.Camera _camera;
            private readonly Vector3 _position;
            private readonly Quaternion _rotation;
            private readonly float _size;
            private readonly bool _enabled;
            private readonly Dictionary<FieldInfo, object> _fields = new Dictionary<FieldInfo, object>();

            internal CameraPose(ArrowSwarm.Camera.CameraController controller)
            {
                _controller = controller; _camera = controller.GetComponent<UnityEngine.Camera>();
                _position = controller.transform.position; _rotation = controller.transform.rotation;
                _size = _camera.orthographicSize; _enabled = controller.enabled;
                foreach (string name in new[] { "_isPanning", "_isDragging", "_isTouchDragValid", "_isMouseDragValid", "_dragStartScreenPos",
                    "_lastPanPosition", "_lastPinchDistance", "_isPinching", "_targetOrthoSize", "_zoomVelocity", "_panTouchId",
                    "_pinchFirstId", "_pinchSecondId", "_gestureVersion", "_touchGestureActive" })
                {
                    var field = controller.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
                    if (field != null) _fields.Add(field, field.GetValue(controller));
                }
            }

            public void Dispose()
            {
                if (_controller == null) return;
                _controller.transform.SetPositionAndRotation(_position, _rotation); _camera.orthographicSize = _size;
                foreach (var item in _fields) item.Key.SetValue(_controller, item.Value);
                _controller.enabled = _enabled;
            }
        }
    }
}
