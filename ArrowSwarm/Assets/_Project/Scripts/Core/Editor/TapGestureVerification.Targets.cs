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

        /// <summary>Taps just beyond half a cell from an arrow path (empty cell or board edge), inside the finger tolerance.</summary>
        private static IEnumerator FingerTolerance()
        {
            var cam = UnityEngine.Camera.main; var grid = GridManager.Instance; float s = grid.PointSpacing;
            float worldPerPixel = cam.orthographicSize * 2f / Mathf.Max(1f, Screen.height);
            float finger = TapGesture.MillimetersToPixels(GameManager.Instance.Config.ArrowTouchToleranceMm) * worldPerPixel;
            float radius = Mathf.Min(s * 0.5f + finger, s * GameManager.Instance.Config.ArrowTouchMaxCells);
            float offset = Mathf.Lerp(s * 0.5f, radius, 0.6f);
            foreach (var arrow in ArrowSpawner.Instance.ActiveArrows)
            {
                if (!Selectable(arrow)) continue;
                foreach (var point in arrow.PathPoints)
                    foreach (var side in Sides)
                    {
                        var empty = point + side;
                        // Either an empty cell or just outside the board edge.
                        if (grid.GetPoint(empty) != null && ArrowSpawner.Instance.GetArrowAt(empty) != null) continue;
                        Vector2 world = point.PointToWorld(s, grid.Origin) + (Vector2)side * offset;
                        Vector2 screen = cam.WorldToScreenPoint(world);
                        if (screen.x < Screen.width * 0.08f || screen.x > Screen.width * 0.92f ||
                            screen.y < Screen.height * 0.18f || screen.y > Screen.height * 0.8f) continue;
                        yield return Gesture((grid.GetPoint(empty) == null ? "beyond the board edge " : "empty cell ") + (offset / s).ToString("F2") + " cells from a path selects that arrow",
                            screen, Vector2.zero, 0.05f, 1);
                        yield break;
                    }
            }
            int selectable = 0; foreach (var a in ArrowSpawner.Instance.ActiveArrows) if (Selectable(a)) selectable++;
            Check(false, "found an empty cell beside an arrow for the tolerance check (selectable arrows " + selectable + ")");
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
