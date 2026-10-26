namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using ArrowSwarm.Data;
    using ArrowSwarm.UI;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.LowLevel;
    using UnityEngine.UI;

    /// <summary>Exercises Retry with actual native pointer updates while preserving the caller's input devices.</summary>
    public static partial class Quality5LeaderboardVerification
    {
        private static IEnumerator NativeRetry(Scope scope)
        {
            var button = Field<Button>(scope.Board, "_retryButton");
            var feedback = button.GetComponent<UIButtonFeedback>();
            if (feedback == null) throw new InvalidOperationException("Authored Retry feedback is missing.");
            var target = Field<Transform>(feedback, "_targetTransform");
            if (target == null) target = button.transform;
            using (var device = new NativeMouse())
            {
                EventSystem.current.SetSelectedGameObject(null);
                device.Send(new Vector2(2, 2), false); yield return device.Wait(); yield return Settled();
                Vector3 baseline = target.localScale;
                var rect = button.transform as RectTransform;
                var canvas = button.GetComponentInParent<Canvas>();
                var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
                Vector2 center = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
                int before = scope.Cloud.Fetches;
                device.Send(center, true); yield return device.Wait(); yield return Settled(.12f);
                Check(target.localScale.x < baseline.x * .99f && scope.Cloud.Fetches == before,
                    "native Retry press visibly squeezes authored feedback without fetching before release");
                device.Send(center, false); yield return device.Wait(); yield return Settled();
                Check(scope.Cloud.Fetches == before + 1 && LeaderboardManager.Instance.RefreshState == LeaderboardRefreshState.Loading,
                    "native Retry release invokes exactly one real refresh callback");
                Check(Vector3.Distance(target.localScale, baseline) < .0001f,
                    "native release restores Retry scale even as Loading disables its button");
                device.Send(center, true); yield return device.Wait(); yield return Settled(.12f);
                device.Send(center, false); yield return device.Wait(); yield return Settled();
                Check(scope.Cloud.Fetches == before + 1 && !button.interactable,
                    "second native click on busy Retry cannot start another provider fetch");
                device.Send(new Vector2(2, 2), false); yield return device.Wait(); yield return Settled();
                EventSystem.current.SetSelectedGameObject(null);
                Check(Vector3.Distance(target.localScale, baseline) < .0001f,
                    "busy click and pointer exit leave no persistent Retry scale drift");
            }
        }

        /// <summary>Owns synthetic native input and acknowledges only real Dynamic player-loop updates.</summary>
        private sealed class NativeMouse : IDisposable
        {
            private readonly Mouse _previousMouse, _mouse;
            private readonly InputDevice _previousPointer;
            private readonly InputSettings _original, _temporary;
            private Vector2 _position;
            private bool _pressed, _pending, _acknowledged, _disposed;
            private int _frame;

            internal NativeMouse()
            {
                _previousMouse = Mouse.current; _previousPointer = Pointer.current; _original = InputSystem.settings;
                _temporary = UnityEngine.Object.Instantiate(_original); _temporary.hideFlags = HideFlags.HideAndDontSave;
                _temporary.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                _temporary.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                InputSystem.settings = _temporary; _mouse = InputSystem.AddDevice<Mouse>("Quality5LeaderboardMouse");
                InputSystem.onAfterUpdate += AfterUpdate;
            }

            internal void Send(Vector2 position, bool pressed)
            {
                if (_pending) throw new InvalidOperationException("Previous native input was not acknowledged.");
                _position = position; _pressed = pressed; _pending = true; _acknowledged = false; _frame = -1;
                InputSystem.QueueStateEvent(_mouse, new MouseState { position = position }.WithButton(MouseButton.Left, pressed));
            }

            internal IEnumerator Wait()
            {
                yield return (Func<bool>)(() => _acknowledged && Time.frameCount > _frame + 1);
                Results.Add("INPUT ACK: actual native Retry mouse " + (_pressed ? "down" : "up") + "; playerFrame=" + _frame);
                _pending = false;
            }

            private void AfterUpdate()
            {
                if (InputState.currentUpdateType != InputUpdateType.Dynamic || !_pending || _acknowledged ||
                    _mouse.leftButton.isPressed != _pressed || (_mouse.position.ReadValue() - _position).sqrMagnitude > .01f) return;
                _acknowledged = true; _frame = Time.frameCount;
            }

            /// <summary>Removes only the owned mouse and restores exact settings and current device identities.</summary>
            public void Dispose()
            {
                if (_disposed) return; _disposed = true;
                InputSystem.onAfterUpdate -= AfterUpdate;
                if (_mouse != null && _mouse.added) InputSystem.RemoveDevice(_mouse);
                InputSystem.settings = _original;
                if (_temporary != null) UnityEngine.Object.DestroyImmediate(_temporary);
                if (_previousMouse != null && _previousMouse.added) _previousMouse.MakeCurrent();
                if (_previousPointer != null && _previousPointer.added) _previousPointer.MakeCurrent();
                Check(ReferenceEquals(InputSystem.settings, _original) && (_mouse == null || !_mouse.added),
                    "native Retry restores original InputSettings and removes its owned mouse");
                Check((_previousMouse == null || !_previousMouse.added || Mouse.current == _previousMouse) &&
                    (_previousPointer == null || !_previousPointer.added || Pointer.current == _previousPointer),
                    "native Retry restores the original current mouse and pointer");
            }
        }
    }
}
