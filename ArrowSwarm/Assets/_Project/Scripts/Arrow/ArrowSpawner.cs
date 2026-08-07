namespace ArrowSwarm.Arrow
{
    using System;
    using System.Collections.Generic;
    using ArrowSwarm.Core;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Spawns and places multi-point arrows on the grid using object pooling.
    /// Manages rainbow promotion: when only 1 arrow remains, it becomes rainbow.
    /// </summary>
    public class ArrowSpawner : Singleton<ArrowSpawner>
    {
        [SerializeField] private Arrow _arrowPrefab;
        [SerializeField] private Transform _arrowParent;

        private ObjectPool<Arrow> _arrowPool;
        private readonly List<Arrow> _activeArrows = new List<Arrow>();

        /// <summary>List of currently active (placed) arrows.</summary>
        public IReadOnlyList<Arrow> ActiveArrows => _activeArrows;

        /// <summary>Number of arrows remaining (not yet fired).</summary>
        public int RemainingArrows
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _activeArrows.Count; i++)
                {
                    if (!_activeArrows[i].IsFired) count++;
                }
                return count;
            }
        }

        /// <summary>Total arrows placed this level.</summary>
        public int TotalArrows => _activeArrows.Count;

        /// <summary>Fired when all arrows have been fired.</summary>
        public static event Action OnAllArrowsFired;

        protected override void OnSingletonAwake()
        {
            if (_arrowParent == null)
            {
                _arrowParent = transform;
            }
        }

        /// <summary>
        /// Initializes the arrow pool with the arrow prefab.
        /// </summary>
        public void InitializePool()
        {
            if (_arrowPrefab == null)
            {
                Debug.LogError("[ArrowSwarm] ArrowSpawner: Arrow prefab not assigned!");
                return;
            }

            _arrowPool = new ObjectPool<Arrow>(
                _arrowPrefab, _arrowParent, 20, 100,
                onGet: a => a.gameObject.SetActive(true),
                onRelease: a =>
                {
                    a.ResetArrow();
                    a.gameObject.SetActive(false);
                }
            );
        }

        /// <summary>
        /// Spawns arrows on the grid based on pre-generated placements.
        /// Each placement contains a multi-point path and head direction.
        /// </summary>
        public void SpawnArrows(List<SolvabilityChecker.ArrowPlacement> placements, MapData mapData)
        {
            ClearAllArrows();

            GridManager grid = GridManager.Instance;
            float spacing = grid.PointSpacing;
            Vector2 origin = grid.Origin;

            for (int i = 0; i < placements.Count; i++)
            {
                var placement = placements[i];

                Arrow arrow = _arrowPool.Get();

                // Position at head point
                Vector2 headWorld = placement.PathPoints[0].PointToWorld(spacing, origin);
                arrow.transform.position = new Vector3(headWorld.x, headWorld.y, 0f);

                // Initialize with path data (rainbow = false initially)
                arrow.Initialize(placement.PathPoints, placement.HeadDirection, false);

                // Register on grid
                grid.PlaceArrowOnPoints(placement.PathPoints, arrow);
                _activeArrows.Add(arrow);
            }

            // Subscribe to arrow fire events
            Arrow.OnArrowFiredEvent += HandleArrowFired;

            LogDebug($"Spawned {placements.Count} multi-point arrows.");
        }

        /// <summary>
        /// Clears all active arrows and returns them to pool.
        /// </summary>
        public void ClearAllArrows()
        {
            Arrow.OnArrowFiredEvent -= HandleArrowFired;

            for (int i = _activeArrows.Count - 1; i >= 0; i--)
            {
                _arrowPool.Release(_activeArrows[i]);
            }
            _activeArrows.Clear();
        }

        private void HandleArrowFired(Arrow arrow)
        {
            int remaining = RemainingArrows;

            if (remaining <= 0)
            {
                OnAllArrowsFired?.Invoke();
                GameManager.Instance.HandleAllArrowsFired();
                LogDebug("All arrows fired!");
            }
            else if (remaining == 1)
            {
                // Promote last remaining arrow to rainbow
                PromoteLastArrowToRainbow();
            }
        }

        /// <summary>
        /// Finds the last unfired arrow and promotes it to rainbow.
        /// </summary>
        private void PromoteLastArrowToRainbow()
        {
            for (int i = 0; i < _activeArrows.Count; i++)
            {
                if (!_activeArrows[i].IsFired)
                {
                    _activeArrows[i].SetRainbow(true);
                    LogDebug($"Arrow at {_activeArrows[i].HeadPoint} promoted to RAINBOW!");
                    break;
                }
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] ArrowSpawner: {message}");
        }
    }
}
