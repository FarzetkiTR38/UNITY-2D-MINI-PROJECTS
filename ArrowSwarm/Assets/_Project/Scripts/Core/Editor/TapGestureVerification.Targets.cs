namespace ArrowSwarm.Core.Editor
{
    using System.Collections;
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Utils;
    using UnityEngine;
    using UnityEngine.InputSystem.LowLevel;

    /// <summary>Target selection helpers for the tap gesture verification.</summary>
    public static partial class TapGestureVerification
    {
        private static readonly Vector2Int[] Sides = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        /// <summary>Screen position of an unfired arrow head away from the HUD.</summary>
        private static Vector2 Pick()
        {
            var cam = UnityEngine.Camera.main;
            var grid = GridManager.Instance;
            foreach (var arrow in ArrowSpawner.Instance.ActiveArrows)
            {
                if (!Selectable(arrow)) continue;
                Vector2 screen = cam.WorldToScreenPoint(arrow.HeadPoint.PointToWorld(grid.PointSpacing, grid.Origin));
                if (screen.x > Screen.width * 0.2f && screen.x < Screen.width * 0.8f &&
                    screen.y > Screen.height * 0.25f && screen.y < Screen.height * 0.75f) return screen;
            }
            Results.Add("NOTE: no selectable arrow head in the central region; tapping screen center");
            return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }

        /// <summary>Arrows that will raise OnArrowClicked when tapped (repeat taps on known-blocked arrows are silent).</summary>
        private static bool Selectable(Arrow arrow) =>
            arrow != null && !arrow.IsFired && !arrow.IsBlockedAnimating && !arrow.IsMarkedBlocked;

        private static bool IsCameraClamped()
        {
            var cam = UnityEngine.Camera.main;
            return cam.orthographicSize >= 0f && ArrowSwarm.Camera.CameraController.Instance == null;
        }
    }
}
