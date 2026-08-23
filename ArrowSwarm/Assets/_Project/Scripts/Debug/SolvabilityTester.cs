namespace ArrowSwarm.Debug
{
    using System.Collections.Generic;
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Utils;
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// In-game solvability tester. Scans all grid points in matrix order
    /// (row 0 left-to-right, then row 1, etc.) and fires every arrow
    /// whose head is at that point AND has a clear path.
    /// 
    /// Usage: Attach to any GameObject in GameScene. Press the assigned
    /// button or call RunSolvabilitySweep() to execute one pass.
    /// Repeat until no arrows can fire. If arrows remain → level is unsolvable.
    /// </summary>
    public class SolvabilityTester : MonoBehaviour
    {
        [SerializeField] private Key _triggerKey = Key.T;
        [SerializeField] private bool _showOnGUIButton = true;

        private int _passNumber;
        private int _totalFiredAcrossAllPasses;

        /// <summary>
        /// Runs a single solvability sweep across the entire grid.
        /// Scans points in matrix order: y=0..height, x=0..width.
        /// For each point, if an unfired arrow's HEAD is there and
        /// its path is clear, it fires that arrow.
        /// </summary>
        public void RunSolvabilitySweep()
        {
            GridManager grid = GridManager.Instance;
            ArrowSpawner spawner = ArrowSpawner.Instance;

            if (grid == null || spawner == null)
            {
                UnityEngine.Debug.LogError("[ArrowSwarm] SolvabilityTester: GridManager or ArrowSpawner not found!");
                return;
            }

            int width = grid.Width;
            int height = grid.Height;
            _passNumber++;

            int firedThisPass = 0;
            int remainingBefore = spawner.RemainingArrows;

            UnityEngine.Debug.Log($"[ArrowSwarm] ═══════════════════════════════════════════");
            UnityEngine.Debug.Log($"[ArrowSwarm] SolvabilityTester: PASS #{_passNumber} starting...");
            UnityEngine.Debug.Log($"[ArrowSwarm] Arrows remaining before sweep: {remainingBefore}");
            UnityEngine.Debug.Log($"[ArrowSwarm] ═══════════════════════════════════════════");

            if (remainingBefore == 0)
            {
                UnityEngine.Debug.Log($"[ArrowSwarm] SolvabilityTester: ✅ Board is CLEAR! All arrows fired successfully.");
                return;
            }

            // Collect all fireable arrows first to avoid modifying collection during iteration
            var fireableArrows = new List<ArrowSwarm.Arrow.Arrow>();

            // Scan in matrix order: row by row (y=0 first), left to right (x=0 first)
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    GridPoint point = grid.GetPoint(pos);

                    if (point == null || !point.IsOccupied || point.OccupyingArrow == null)
                        continue;

                    ArrowSwarm.Arrow.Arrow arrow = point.OccupyingArrow;

                    // Only process if this point is the arrow's HEAD (not body)
                    if (arrow.IsFired || arrow.HeadPoint != pos)
                        continue;

                    // Check if path is clear (can fire)
                    bool canFire = grid.IsPathClear(arrow.HeadPoint, arrow.HeadDirection);

                    if (canFire)
                    {
                        fireableArrows.Add(arrow);
                        UnityEngine.Debug.Log(
                            $"[ArrowSwarm] SolvabilityTester: ✅ [{x},{y}] Arrow HEAD facing {arrow.HeadDirection} — PATH CLEAR → will fire (W={arrow.Weight})");
                    }
                    else
                    {
                        UnityEngine.Debug.Log(
                            $"[ArrowSwarm] SolvabilityTester: ❌ [{x},{y}] Arrow HEAD facing {arrow.HeadDirection} — BLOCKED");
                    }
                }
            }

            // Now fire all collected arrows
            for (int i = 0; i < fireableArrows.Count; i++)
            {
                var arrow = fireableArrows[i];
                if (!arrow.IsFired) // Double-check in case a previous fire in this batch affected it
                {
                    arrow.OnPlayerClick();
                    firedThisPass++;
                }
            }

            _totalFiredAcrossAllPasses += firedThisPass;
            int remainingAfter = spawner.RemainingArrows;

            UnityEngine.Debug.Log($"[ArrowSwarm] ───────────────────────────────────────────");
            UnityEngine.Debug.Log($"[ArrowSwarm] SolvabilityTester: Pass #{_passNumber} complete.");
            UnityEngine.Debug.Log($"[ArrowSwarm] Fired this pass: {firedThisPass}");
            UnityEngine.Debug.Log($"[ArrowSwarm] Remaining arrows: {remainingAfter}");
            UnityEngine.Debug.Log($"[ArrowSwarm] Total fired (all passes): {_totalFiredAcrossAllPasses}");

            // Deadlock detection
            if (firedThisPass == 0 && remainingAfter > 0)
            {
                UnityEngine.Debug.LogError(
                    $"[ArrowSwarm] SolvabilityTester: 🚨 DEADLOCK DETECTED! " +
                    $"{remainingAfter} arrows remain but NONE can fire. Level is UNSOLVABLE!");

                // Log all stuck arrows with details
                LogStuckArrows(grid, spawner);
            }
            else if (remainingAfter == 0)
            {
                UnityEngine.Debug.Log(
                    $"[ArrowSwarm] SolvabilityTester: ✅ BOARD CLEARED in {_passNumber} passes! Level is SOLVABLE.");
            }
            else
            {
                UnityEngine.Debug.Log(
                    $"[ArrowSwarm] SolvabilityTester: ⏳ {remainingAfter} arrows still on board. Press again to continue.");
            }

            UnityEngine.Debug.Log($"[ArrowSwarm] ═══════════════════════════════════════════");
        }

        /// <summary>
        /// Resets the pass counter. Call when a new level loads.
        /// </summary>
        public void ResetCounter()
        {
            _passNumber = 0;
            _totalFiredAcrossAllPasses = 0;
        }

        /// <summary>
        /// Logs detailed information about all stuck (unfireable) arrows.
        /// </summary>
        private void LogStuckArrows(GridManager grid, ArrowSpawner spawner)
        {
            var activeArrows = spawner.ActiveArrows;

            for (int i = 0; i < activeArrows.Count; i++)
            {
                var arrow = activeArrows[i];
                if (arrow == null || arrow.IsFired) continue;

                Vector2Int head = arrow.HeadPoint;
                ArrowDirection dir = arrow.HeadDirection;
                Vector2Int step = GridManager.DirectionToVector(dir);
                if (step == Vector2Int.zero) continue;

                // Find what's blocking this arrow
                string blocker = "edge-not-reached";
                Vector2Int scanPos = head + step;

                while (scanPos.IsInBounds(grid.Width, grid.Height))
                {
                    GridPoint pt = grid.GetPoint(scanPos);
                    if (pt != null && pt.IsOccupied && pt.OccupyingArrow != null)
                    {
                        var blockingArrow = pt.OccupyingArrow;
                        bool isSelf = blockingArrow == arrow;
                        blocker = isSelf
                            ? $"SELF-BLOCKING at [{scanPos.x},{scanPos.y}]"
                            : $"Blocked by arrow at [{scanPos.x},{scanPos.y}] (head=[{blockingArrow.HeadPoint.x},{blockingArrow.HeadPoint.y}] dir={blockingArrow.HeadDirection})";
                        break;
                    }
                    scanPos += step;
                }

                // Build path string
                var pathStr = new System.Text.StringBuilder();
                var points = arrow.PathPoints;
                for (int p = 0; p < points.Count; p++)
                {
                    if (p > 0) pathStr.Append(" → ");
                    pathStr.Append($"[{points[p].x},{points[p].y}]");
                }

                UnityEngine.Debug.LogWarning(
                    $"[ArrowSwarm] SolvabilityTester: STUCK Arrow #{i}: " +
                    $"Head=[{head.x},{head.y}] Dir={dir} W={arrow.Weight} | " +
                    $"Reason: {blocker} | Path: {pathStr}");
            }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard[_triggerKey].wasPressedThisFrame)
            {
                RunSolvabilitySweep();
            }
        }

        private void OnGUI()
        {
            if (!_showOnGUIButton) return;

            // Simple debug button on top-left of the screen
            GUI.skin.button.fontSize = 16;
            if (GUI.Button(new Rect(20, 120, 220, 50), $"⚡ Test Sweep (Pass #{_passNumber})"))
            {
                RunSolvabilitySweep();
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            UnityEngine.Debug.Log($"[ArrowSwarm] SolvabilityTester: {message}");
        }
    }
}
