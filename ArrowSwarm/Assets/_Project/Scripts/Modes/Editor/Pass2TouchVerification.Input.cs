namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using ArrowSwarm.Core;
    using ArrowSwarm.Grid;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using TouchPhase = UnityEngine.InputSystem.TouchPhase;
    using DefenseArrow = ArrowSwarm.Arrow.Arrow;

    /// <summary>Checks canceled OS touches and multi-finger tap suppression against real Defense arrows.</summary>
    public static partial class Pass2TouchVerification
    {
        private static IEnumerator InputChecks()
        {
            Status = "Canceling a real touch over a blocked Defense arrow";
            var controller = ArrowSwarm.Camera.CameraController.Instance;
            using (new CameraPose(controller))
            using (var devices = new Devices())
            {
                controller.enabled = false; InputManager.Instance.BlockInput(0f); yield return .08f;
                DefenseArrow blocked = VisibleArrow(false), legal = VisibleArrow(true);
                Check(blocked != null && legal != null, "Real Defense101 contains visible blocked and legal touch fixtures");
                Vector2 position = ScreenHead(blocked); int lives = GameManager.Instance.CurrentLives;
                int fired = LevelManager.Instance.ArrowsFired;
                devices.Send(1, TouchPhase.Began, position); yield return .06f;
                devices.Send(1, TouchPhase.Canceled, position); yield return .08f;
                Check(!blocked.IsBlockedAnimating && !blocked.IsMarkedBlocked && GameManager.Instance.CurrentLives == lives &&
                    LevelManager.Instance.ArrowsFired == fired, "Canceled touch never starts a shot or spends a life");
                Status = "Rejecting a real multi-finger gesture as an arrow tap";
                devices.Send(2, TouchPhase.Began, position); yield return .06f;
                devices.Send(3, TouchPhase.Began, position + Vector2.right * 40f); yield return .06f;
                devices.Send(2, TouchPhase.Ended, position); yield return .06f;
                devices.Send(3, TouchPhase.Ended, position + Vector2.right * 40f); yield return .08f;
                Check(!blocked.IsBlockedAnimating && !blocked.IsMarkedBlocked && GameManager.Instance.CurrentLives == lives &&
                    LevelManager.Instance.ArrowsFired == fired, "Ending a two-finger gesture never becomes a Defense tap");
                devices.Send(4, TouchPhase.Began, position); yield return .06f;
                devices.Send(5, TouchPhase.Began, position + Vector2.right * 40f); yield return .06f;
                devices.Send(4, TouchPhase.Canceled, position); devices.Send(5, TouchPhase.Canceled, position); yield return .08f;
                Check(GameManager.Instance.CurrentLives == lives && !blocked.IsMarkedBlocked,
                    "Canceling every finger clears gesture ownership without a wrong-click penalty");
                Status = "Firing a new normal single-finger touch after cancellation";
                position = ScreenHead(legal); devices.Send(6, TouchPhase.Began, position); yield return .06f;
                devices.Send(6, TouchPhase.Ended, position); yield return .08f;
                Check(legal.IsFired && LevelManager.Instance.ArrowsFired == fired + 1 && GameManager.Instance.CurrentLives == lives,
                    "A fresh single-finger release still fires exactly one legal Defense arrow");
                Check(Touchscreen.current == devices.Screen, "Production input consumed the synthetic current Touchscreen device");
            }
            Check(true, "Original camera pose/enabled state, input devices and Input System settings restored");
        }

        private static DefenseArrow VisibleArrow(bool legal)
        {
            var controller = ArrowSwarm.Camera.CameraController.Instance;
            foreach (var arrow in ArrowSwarm.Arrow.ArrowSpawner.Instance.ActiveArrows)
            {
                if (arrow == null || arrow.IsFired || arrow.IsBlockedAnimating ||
                    GridManager.Instance.IsPathClear(arrow.HeadPoint, arrow.HeadDirection) != legal) continue;
                Vector2 screen = ScreenHead(arrow);
                if (screen.x > 10f && screen.x < Screen.width - 10f && controller.IsValidPanStartPosition(screen)) return arrow;
            }
            return null;
        }

        private static Vector2 ScreenHead(DefenseArrow arrow)
        {
            var grid = GridManager.Instance;
            Vector2 world = grid.Origin + (Vector2)arrow.HeadPoint * grid.PointSpacing;
            return ArrowSwarm.Camera.CameraController.Instance.GetComponent<UnityEngine.Camera>().WorldToScreenPoint(world);
        }
    }
}
