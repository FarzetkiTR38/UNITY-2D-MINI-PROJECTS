namespace ArrowSwarm.Modes.Editor
{
    using System.Collections;
    using System.Reflection;
    using ArrowSwarm.Core;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using TouchPhase = UnityEngine.InputSystem.TouchPhase;

    /// <summary>Exercises sparse touch slots and real pan/pinch lifetime transitions on the Defense camera.</summary>
    public static partial class Pass2TouchVerification
    {
        /// <summary>Checks sparse active controls, finger replacement and complete gesture cleanup.</summary>
        public static void BeginCamera() => Begin(CameraChecks, "camera-touch-verification.txt");

        private static IEnumerator CameraChecks()
        {
            var controller = ArrowSwarm.Camera.CameraController.Instance;
            using (new CameraPose(controller))
            using (var devices = new Devices())
            {
                Status = "Selecting real sparse touch controls 1 and 2";
                controller.SetZoomNormalized(.65f);
                yield return (System.Func<bool>)(() => Mathf.Abs(controller.GetComponent<UnityEngine.Camera>().orthographicSize -
                    Hidden<float>(controller, "_targetOrthoSize")) < .02f);
                Vector2 left = new Vector2(Screen.width * .4f, Screen.height * .5f);
                Vector2 center = new Vector2(Screen.width * .5f, Screen.height * .5f);
                Vector2 right = new Vector2(Screen.width * .6f, Screen.height * .5f);
                Check(controller.IsValidPanStartPosition(left) && controller.IsValidPanStartPosition(right),
                    "Authored gameplay viewport supplies valid gesture positions outside the HUD");
                devices.Send(1, TouchPhase.Began, left); yield return devices.WaitForEvents();
                TouchDiagnostic(devices, "after finger1 began");
                devices.Send(2, TouchPhase.Began, center); yield return devices.WaitForEvents();
                TouchDiagnostic(devices, "after finger2 began");
                devices.Send(3, TouchPhase.Began, right); yield return devices.WaitForEvents();
                TouchDiagnostic(devices, "after finger3 began");
                devices.Send(1, TouchPhase.Ended, left); yield return devices.WaitForEvents();
                TouchDiagnostic(devices, "after finger1 ended / sparse pair");
                Check(!devices.Slot(0).Pressed && devices.Slot(1).Pressed && devices.Slot(2).Pressed &&
                    devices.Slot(1).Id == 2 && devices.Slot(2).Id == 3,
                    "Actual game-update Input System state has pressed sparse slots1/2 (IDs2/3), with slot0 released");
                Check(Hidden<bool>(controller, "_isPinching") && Hidden<int>(controller, "_pinchFirstId") == 2 &&
                    Hidden<int>(controller, "_pinchSecondId") == 3, "Camera tracks the two active finger IDs rather than stale slot0");
                float target = Hidden<float>(controller, "_targetOrthoSize");
                center -= Vector2.right * 20f; right += Vector2.right * 20f;
                devices.Send(2, TouchPhase.Moved, center); devices.Send(3, TouchPhase.Moved, right); yield return devices.WaitForEvents();
                Check(Hidden<float>(controller, "_targetOrthoSize") < target - .05f,
                    "Moving sparse fingers apart zooms in with the correct direction");
                Status = "Replacing one finger without a zoom jump";
                target = Hidden<float>(controller, "_targetOrthoSize");
                devices.Send(2, TouchPhase.Ended, center); devices.Send(4, TouchPhase.Began, left); yield return devices.WaitForEvents();
                Check(Hidden<int>(controller, "_pinchFirstId") == 4 && Hidden<int>(controller, "_pinchSecondId") == 3 &&
                    Mathf.Abs(Hidden<float>(controller, "_targetOrthoSize") - target) < .0001f,
                    "A changed active pair rebases its initial distance without changing the zoom target");
                devices.Send(4, TouchPhase.Ended, left); yield return devices.WaitForEvents();
                Check(Hidden<int>(controller, "_panTouchId") == 3 && !Hidden<bool>(controller, "_isPinching"),
                    "The remaining finger in slot2 becomes the actual pan owner");
                right += Vector2.right * 25f; devices.Send(3, TouchPhase.Moved, right); yield return devices.WaitForEvents();
                Vector3 before = controller.transform.position;
                right += Vector2.right * 35f; devices.Send(3, TouchPhase.Moved, right); yield return devices.WaitForEvents();
                Check(controller.IsDragging && (controller.transform.position - before).sqrMagnitude > .00001f,
                    "The sparse remaining finger pans the real zoomed board");
                Status = "Releasing a real pan-to-pinch transition";
                devices.Send(5, TouchPhase.Began, left); yield return devices.WaitForEvents();
                Check(Hidden<bool>(controller, "_isPinching"), "Adding a finger changes the active pan into a pinch");
                devices.Send(5, TouchPhase.Ended, left); devices.Send(3, TouchPhase.Ended, right); yield return devices.WaitForEvents();
                Check(!controller.IsDragging && !Hidden<bool>(controller, "_isPinching") && !Hidden<bool>(controller, "_isTouchDragValid"),
                    "Releasing every finger after pan-to-pinch clears all drag/pinch ownership");
                Status = "Canceling a second real pan-to-pinch transition";
                devices.Send(6, TouchPhase.Began, center); yield return devices.WaitForEvents();
                devices.Send(6, TouchPhase.Moved, center + Vector2.right * 25f); yield return devices.WaitForEvents();
                devices.Send(7, TouchPhase.Began, left); yield return devices.WaitForEvents();
                devices.Send(6, TouchPhase.Canceled, center + Vector2.right * 25f);
                devices.Send(7, TouchPhase.Canceled, left); yield return devices.WaitForEvents();
                Check(!controller.IsDragging && !Hidden<bool>(controller, "_isPinching") && !Hidden<bool>(controller, "_touchGestureActive"),
                    "OS cancellation after pan-to-pinch clears the camera gesture too");
                controller.FitToMap(LevelManager.Instance.CurrentLevelData.Map); yield return .1f;
                var arrow = VisibleArrow(true); Check(arrow != null, "A legal visible arrow remains after camera gesture tests");
                Vector2 position = ScreenHead(arrow); int fired = LevelManager.Instance.ArrowsFired;
                devices.Send(8, TouchPhase.Began, position); yield return devices.WaitForEvents();
                devices.Send(8, TouchPhase.Ended, position); yield return devices.WaitForEvents();
                Check(arrow.IsFired && LevelManager.Instance.ArrowsFired == fired + 1,
                    "A new normal touch fires immediately after canceled camera gestures");
            }
            Check(true, "Original camera, devices and Input System settings restored after real gesture checks");
        }

        private static T Hidden<T>(object target, string name) => (T)target.GetType()
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);

        private static void TouchDiagnostic(Devices devices, string stage)
        {
            Results.Add("TOUCH STATE: " + stage + "; device=" + devices.Screen.deviceId +
                ", current=" + (Touchscreen.current != null ? Touchscreen.current.deviceId : -1) + "; " + devices.Diagnostics);
            for (int index = 0; index < devices.SlotCount; index++)
            {
                var touch = devices.Slot(index);
                if (index > 2 && touch.Id == 0) continue;
                Results.Add("  game slot=" + index + ", id=" + touch.Id + ", phase=" + touch.Phase +
                    ", pressed=" + touch.Pressed + ", position=" + touch.Position);
            }
        }
    }
}
