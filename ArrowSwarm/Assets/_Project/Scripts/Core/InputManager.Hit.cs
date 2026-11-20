namespace ArrowSwarm.Core
{
    using UnityEngine;
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Utils;

    /// <summary>Resolves a tap to an arrow: exact cell first, then the nearest path within finger tolerance.</summary>
    public partial class InputManager
    {
        private const float FallbackTouchToleranceMm = 2.5f;
        private const float FallbackTouchMaxCells = 0.72f;

        private void ProcessClick(Vector2 screenPosition)
        {
            if (MainCamera == null) return;
            float camDist = Mathf.Abs(MainCamera.transform.position.z);
            if (camDist < 0.1f) camDist = 10f;
            Vector2 worldPos = MainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, camDist));

            var grid = GridManager.Instance;
            var spawner = ArrowSpawner.Instance;
            if (grid != null && spawner != null)
            {
                // 1. The arrow that owns the touched grid cell always wins, so neighbors are never stolen.
                var gPoint = grid.WorldToPoint(worldPos);
                if (gPoint != null)
                {
                    var arrow = spawner.GetArrowAt(gPoint.GridPosition);
                    if (TryFire(arrow)) return;
                }

                // 2. Empty cell: accept the closest arrow path within a small, cell-capped finger tolerance.
                if (TryFire(FindNearestArrow(worldPos, grid, spawner))) return;
            }

            // 3. Physics fallback for off-grid colliders.
            RaycastHit2D[] hits = Physics2D.RaycastAll(worldPos, Vector2.zero);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider != null && TryFire(hits[i].collider.GetComponentInParent<Arrow>())) break;
            }
        }

        private bool TryFire(Arrow arrow)
        {
            if (arrow == null || arrow.IsFired || !IsAllowedInTutorial(arrow)) return false;
            arrow.OnPlayerClick();
            return true;
        }

        private Arrow FindNearestArrow(Vector2 worldPos, GridManager grid, ArrowSpawner spawner)
        {
            var config = GameManager.Instance != null ? GameManager.Instance.Config : null;
            float spacing = grid.PointSpacing;
            float worldPerPixel = MainCamera.orthographicSize * 2f / Mathf.Max(1f, Screen.height);
            float fingerWorld = TapGesture.MillimetersToPixels(config != null ? config.ArrowTouchToleranceMm : FallbackTouchToleranceMm) * worldPerPixel;
            float maxCells = config != null ? config.ArrowTouchMaxCells : FallbackTouchMaxCells;
            float radius = Mathf.Min(spacing * 0.5f + fingerWorld, spacing * maxCells);
            float bestSqr = radius * radius;
            Arrow best = null;
            var arrows = spawner.ActiveArrows;
            for (int i = 0; i < arrows.Count; i++)
            {
                var arrow = arrows[i];
                if (arrow == null || arrow.IsFired || arrow.PathPoints.Count == 0) continue;
                var points = arrow.PathPoints;
                Vector2 previous = points[0].PointToWorld(spacing, grid.Origin);
                float d = (worldPos - previous).sqrMagnitude;
                for (int j = 1; j < points.Count; j++)
                {
                    Vector2 next = points[j].PointToWorld(spacing, grid.Origin);
                    d = Mathf.Min(d, SqrDistanceToSegment(worldPos, previous, next));
                    previous = next;
                }
                if (d < bestSqr) { bestSqr = d; best = arrow; }
            }
            return best;
        }

        private static float SqrDistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(1e-8f, ab.sqrMagnitude));
            return (p - (a + ab * t)).sqrMagnitude;
        }
    }
}
