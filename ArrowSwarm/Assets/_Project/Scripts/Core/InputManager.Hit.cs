namespace ArrowSwarm.Core
{
    using UnityEngine;
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Utils;

    /// <summary>Resolves a tap to the arrow that owns the touched grid cell, with a physics fallback.</summary>
    public partial class InputManager
    {
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

            }

            // 2. Physics fallback for off-grid colliders.
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

    }
}
