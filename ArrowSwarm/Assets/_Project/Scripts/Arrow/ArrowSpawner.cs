namespace ArrowSwarm.Arrow
{
    using System;
    using System.Collections.Generic;
    using ArrowSwarm.Core;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Spawns and places arrows on the grid using object pooling.
    /// Handles direction assignment based on difficulty parameters.
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
        /// Spawns arrows on the grid based on level parameters.
        /// </summary>
        public void SpawnArrows(LevelParams levelParams, MapData mapData)
        {
            ClearAllArrows();

            GridManager grid = GridManager.Instance;
            int arrowCount = levelParams.ArrowCount;

            // Generate random positions
            List<Vector2Int> positions = GenerateRandomPositions(
                mapData.GridWidth, mapData.GridHeight, arrowCount);

            // Place arrows
            for (int i = 0; i < positions.Count; i++)
            {
                Vector2Int pos = positions[i];
                bool isLast = (i == positions.Count - 1);

                ArrowDirection direction = DetermineDirection(
                    pos, mapData.GridWidth, mapData.GridHeight, levelParams.OutwardChance);
                int weight = UnityEngine.Random.Range(levelParams.MinWeight, levelParams.MaxWeight + 1);

                Arrow arrow = _arrowPool.Get();
                arrow.transform.position = pos.GridToWorld(mapData.CellSize, mapData.GridOrigin);
                arrow.Initialize(pos, direction, weight, isLast);

                grid.PlaceArrow(pos, arrow);
                _activeArrows.Add(arrow);
            }

            // Subscribe to arrow fire events
            Arrow.OnArrowFiredEvent += HandleArrowFired;

            LogDebug($"Spawned {positions.Count} arrows. MinW={levelParams.MinWeight}, MaxW={levelParams.MaxWeight}");
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
            if (RemainingArrows <= 0)
            {
                OnAllArrowsFired?.Invoke();
                GameManager.Instance.HandleAllArrowsFired();
                LogDebug("All arrows fired!");
            }
        }

        private List<Vector2Int> GenerateRandomPositions(int width, int height, int count)
        {
            List<Vector2Int> allPositions = new List<Vector2Int>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    allPositions.Add(new Vector2Int(x, y));
                }
            }

            // Fisher-Yates shuffle
            for (int i = allPositions.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (allPositions[i], allPositions[j]) = (allPositions[j], allPositions[i]);
            }

            count = Mathf.Min(count, allPositions.Count);
            return allPositions.GetRange(0, count);
        }

        private ArrowDirection DetermineDirection(Vector2Int pos, int gridWidth, int gridHeight, float outwardChance)
        {
            if (UnityEngine.Random.value < outwardChance)
            {
                // Try to face outward (toward nearest edge)
                return GetOutwardDirection(pos, gridWidth, gridHeight);
            }
            else
            {
                // Random direction
                return (ArrowDirection)UnityEngine.Random.Range(0, 4);
            }
        }

        private ArrowDirection GetOutwardDirection(Vector2Int pos, int gridWidth, int gridHeight)
        {
            // Find distances to each edge
            int distLeft = pos.x;
            int distRight = gridWidth - 1 - pos.x;
            int distDown = pos.y;
            int distUp = gridHeight - 1 - pos.y;

            // Find minimum distance — face toward nearest edge
            int minDist = Mathf.Min(distLeft, Mathf.Min(distRight, Mathf.Min(distDown, distUp)));

            // Collect all directions with minimum distance
            List<ArrowDirection> candidates = new List<ArrowDirection>(4);
            if (distLeft == minDist) candidates.Add(ArrowDirection.Left);
            if (distRight == minDist) candidates.Add(ArrowDirection.Right);
            if (distDown == minDist) candidates.Add(ArrowDirection.Down);
            if (distUp == minDist) candidates.Add(ArrowDirection.Up);

            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] ArrowSpawner: {message}");
        }
    }
}
