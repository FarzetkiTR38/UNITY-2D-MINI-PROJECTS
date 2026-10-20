namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.LowLevel;
    using TouchPhase = UnityEngine.InputSystem.TouchPhase;

    /// <summary>Scopes synthetic devices and focus-independent test settings without changing the user's real devices.</summary>
    public static partial class Pass2TouchVerification
    {
        private sealed class Devices : IDisposable
        {
            internal readonly Touchscreen Screen;
            private readonly Touchscreen _previousTouch;
            private readonly Mouse _mouse, _previousMouse;
            private readonly InputSettings _originalSettings, _temporarySettings;

            internal Devices()
            {
                _previousTouch = Touchscreen.current; _previousMouse = Mouse.current;
                _originalSettings = InputSystem.settings; _temporarySettings = UnityEngine.Object.Instantiate(_originalSettings);
                _temporarySettings.hideFlags = HideFlags.HideAndDontSave;
                _temporarySettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                _temporarySettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                InputSystem.settings = _temporarySettings;
                _mouse = InputSystem.AddDevice<Mouse>("QualityPass2Mouse");
                InputSystem.QueueStateEvent(_mouse, new MouseState { position = new Vector2(UnityEngine.Screen.width * .5f, UnityEngine.Screen.height * .5f) });
                Screen = InputSystem.AddDevice<Touchscreen>("QualityPass2Touchscreen");
            }

            internal void Send(int id, TouchPhase phase, Vector2 position) => InputSystem.QueueStateEvent(Screen,
                new TouchState { touchId = id, phase = phase, position = position, pressure = phase == TouchPhase.Ended || phase == TouchPhase.Canceled ? 0f : 1f });

            public void Dispose()
            {
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
