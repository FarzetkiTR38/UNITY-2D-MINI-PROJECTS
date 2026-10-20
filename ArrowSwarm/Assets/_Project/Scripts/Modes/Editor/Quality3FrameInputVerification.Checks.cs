namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using ArrowSwarm.Puzzle;
    using ArrowSwarm.UI;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem.UI;
    using UnityEngine.UI;
    using TouchPhase = UnityEngine.InputSystem.TouchPhase;

    /// <summary>Drives actual queued touches through the authored rotation bar and pause popup buttons.</summary>
    public static partial class Quality3FrameInputVerification
    {
        private static IEnumerator GestureChecks(PuzzleSession session)
        {
            var hud = UnityEngine.Object.FindFirstObjectByType<PuzzleHUD>();
            var popups = UnityEngine.Object.FindFirstObjectByType<PuzzlePopups>();
            var bar = Ref<Button>(hud, "_rotationButton"); var swipe = bar.GetComponent<FrameSwipeUI>();
            Canvas.ForceUpdateCanvases();
            string config = UnityEditor.EditorJsonUtility.ToJson(session.Config);
            Check(EventSystem.current != null && EventSystem.current.currentInputModule is InputSystemUIInputModule,
                "Authored Color Shift uses the real InputSystemUIInputModule");
            Check(swipe != null && session.Preview, "Real rotation bar owns gestures in a save-isolated preview attempt");
            using (new CameraPose(ArrowSwarm.Camera.CameraController.Instance))
            using (var devices = new Devices())
            {
                yield return FreshAttempt(session);
                Status = "Sending a canceled native touch to the right side of the rotation bar";
                Vector2 right = BarPoint(bar, .32f);
                int turns = session.Turns, orientation = session.Orientation;
                devices.Send(10, TouchPhase.Began, right); yield return devices.WaitForEvents();
                Check(HasOwner(swipe), "Native UI PointerDown actually acquired the right-side rotation gesture");
                devices.Send(10, TouchPhase.Canceled, right); yield return devices.WaitForEvents();
                Verify(session.Turns == turns && session.Orientation == orientation,
                    "OS-canceled right-side bar touch preserves turn budget and orientation (before=" + turns + ", after=" + session.Turns + ")");
                yield return FreshTapAndSwipe(session, devices, bar, "after OS cancellation", 20);

                Status = "Holding a native bar touch across the real pause and retry buttons";
                yield return FreshAttempt(session); right = BarPoint(bar, .32f);
                devices.Send(40, TouchPhase.Began, right); yield return devices.WaitForEvents();
                Check(HasOwner(swipe), "A new held UI gesture owns the bar before pausing");
                yield return Click(devices, 41, Ref<Button>(hud, "_pause"), "authored pause button");
                Check(session.Paused && Time.timeScale == 0f, "Second real finger opens pause while the first stays on the bar");
                yield return Click(devices, 42, Ref<Button>(popups, "_pauseRetry"), "authored pause retry button");
                Check(!session.Paused && session.Ready, "Real retry button starts a fresh playable attempt");
                turns = session.Turns; orientation = session.Orientation;
                devices.Send(40, TouchPhase.Ended, right); yield return devices.WaitForEvents();
                Verify(session.Turns == turns && session.Orientation == orientation,
                    "Old UI gesture release cannot spend the retried attempt's turn (before=" + turns + ", after=" + session.Turns + ")");
                yield return FreshTapAndSwipe(session, devices, bar, "after retry with a held finger", 50);

                Status = "Holding a native bar touch across the real pause and resume buttons";
                yield return FreshAttempt(session); right = BarPoint(bar, .32f);
                devices.Send(70, TouchPhase.Began, right); yield return devices.WaitForEvents();
                Check(HasOwner(swipe), "A held UI gesture owns the bar before the pause/resume cycle");
                yield return Click(devices, 71, Ref<Button>(hud, "_pause"), "authored pause button");
                Check(session.Paused, "Real pause button pauses the current attempt");
                yield return Click(devices, 72, Ref<Button>(popups, "_resume"), "authored resume button");
                Check(!session.Paused && session.Ready, "Real resume button restores the same playable attempt");
                turns = session.Turns; orientation = session.Orientation;
                devices.Send(70, TouchPhase.Ended, right); yield return devices.WaitForEvents();
                Verify(session.Turns == turns && session.Orientation == orientation,
                    "Pre-pause UI gesture release cannot spend a turn after resume (before=" + turns + ", after=" + session.Turns + ")");
                yield return FreshTapAndSwipe(session, devices, bar, "after pause/resume with a held finger", 80);
            }
            Check(UnityEditor.EditorJsonUtility.ToJson(session.Config) == config, "Authored Color Shift configuration remained unchanged");
            Check(true, "Original camera, input settings and current devices restored after native UI gestures");
        }

        private static IEnumerator FreshTapAndSwipe(PuzzleSession session, Devices devices, Button bar, string label, int id)
        {
            yield return FreshAttempt(session); Vector2 right = BarPoint(bar, .32f);
            int turns = session.Turns, orientation = session.Orientation;
            devices.Send(id, TouchPhase.Began, right); yield return devices.WaitForEvents();
            Check(HasOwner(bar.GetComponent<FrameSwipeUI>()), label + ": fresh normal tap acquires actual UI ownership");
            devices.Send(id, TouchPhase.Ended, right); yield return devices.WaitForEvents();
            Verify(session.Turns == turns - 1 && session.Orientation == (orientation ^ 1), label + ": new valid side tap rotates exactly once");
            yield return FreshAttempt(session);
            Vector2 left = BarPoint(bar, -.32f); right = BarPoint(bar, .32f);
            turns = session.Turns; orientation = session.Orientation;
            devices.Send(id + 1, TouchPhase.Began, left); yield return devices.WaitForEvents();
            devices.Send(id + 1, TouchPhase.Moved, right); yield return devices.WaitForEvents();
            devices.Send(id + 1, TouchPhase.Ended, right); yield return devices.WaitForEvents();
            Verify(session.Turns == turns - 1 && session.Orientation == (orientation ^ 1), label + ": new valid swipe rotates exactly once");
        }

        private static IEnumerator FreshAttempt(PuzzleSession session)
        {
            session.Retry(); int frame = Time.frameCount;
            yield return (Func<bool>)(() => session.Ready && Time.frameCount > frame + 1);
            Canvas.ForceUpdateCanvases();
            int layoutFrame = Time.frameCount;
            yield return (Func<bool>)(() => Time.frameCount > layoutFrame);
            Check(session.Turns > 0, "Preview level one provides a positive rotation budget for the fresh attempt");
        }

        private static IEnumerator Click(Devices devices, int id, Button button, string label)
        {
            yield return (Func<bool>)(() => button != null && button.gameObject.activeInHierarchy && button.IsInteractable());
            Vector2 point = ScreenPoint(button.transform as RectTransform, Vector2.zero);
            Check(TopHitBelongsTo(button.transform, point), label + ": native raycast reaches the actual authored control");
            devices.Send(id, TouchPhase.Began, point); yield return devices.WaitForEvents();
            devices.Send(id, TouchPhase.Ended, point); yield return devices.WaitForEvents();
        }

        private static Vector2 BarPoint(Button bar, float fraction)
        {
            var rect = bar.transform as RectTransform;
            Vector2 point = ScreenPoint(rect, new Vector2(rect.rect.width * fraction, 0f));
            Check(TopHitBelongsTo(bar.transform, point), "Native UI raycast reaches the authored rotation-bar side");
            return point;
        }

        private static Vector2 ScreenPoint(RectTransform rect, Vector2 offset)
        {
            var canvas = rect.GetComponentInParent<Canvas>();
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            return RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center + offset));
        }

        private static bool TopHitBelongsTo(Transform target, Vector2 point)
        {
            var hits = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = point }, hits);
            return hits.Count > 0 && (hits[0].gameObject.transform == target || hits[0].gameObject.transform.IsChildOf(target));
        }

        private static bool HasOwner(FrameSwipeUI swipe) => Ref<int>(swipe, "_pointerId") != int.MinValue;
        private static T Ref<T>(object target, string name) => (T)target.GetType()
            .GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(target);
    }
}
