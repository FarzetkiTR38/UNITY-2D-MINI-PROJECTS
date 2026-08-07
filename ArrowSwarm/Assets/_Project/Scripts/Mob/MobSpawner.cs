namespace ArrowSwarm.Mob
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using ArrowSwarm.Core;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Spawns mobs at timed intervals using object pooling.
    /// Manages the spawn schedule based on level difficulty parameters.
    /// </summary>
    public class MobSpawner : Singleton<MobSpawner>
    {
        [SerializeField] private Mob _mobPrefab;
        [SerializeField] private Transform _mobParent;

        private ObjectPool<Mob> _mobPool;
        private readonly List<Mob> _activeMobs = new List<Mob>();
        private Coroutine _spawnCoroutine;
        private int _totalToSpawn;
        private int _spawnedCount;
        private int _killedCount;
        private int _finishedCount;

        /// <summary>List of currently active mobs.</summary>
        public IReadOnlyList<Mob> ActiveMobs => _activeMobs;

        /// <summary>Number of mobs spawned so far.</summary>
        public int SpawnedCount => _spawnedCount;

        /// <summary>Number of mobs killed.</summary>
        public int KilledCount => _killedCount;

        /// <summary>Fired when all mobs are either killed or finished.</summary>
        public static event Action OnAllMobsHandled;

        protected override void OnSingletonAwake()
        {
            if (_mobParent == null)
            {
                _mobParent = transform;
            }
        }

        /// <summary>
        /// Initializes the mob pool.
        /// </summary>
        public void InitializePool()
        {
            if (_mobPrefab == null)
            {
                Debug.LogError("[ArrowSwarm] MobSpawner: Mob prefab not assigned!");
                return;
            }

            _mobPool = new ObjectPool<Mob>(
                _mobPrefab, _mobParent, 15, 100,
                onGet: m => m.gameObject.SetActive(true),
                onRelease: m =>
                {
                    m.ResetMob();
                    m.gameObject.SetActive(false);
                }
            );

            // Subscribe to mob events
            Mob.OnMobKilled += HandleMobKilled;
            Mob.OnMobFinished += HandleMobFinished;
        }

        /// <summary>
        /// Starts spawning mobs based on level parameters.
        /// </summary>
        public void StartSpawning(LevelParams levelParams)
        {
            StopSpawning();
            ClearAllMobs();

            _totalToSpawn = int.MaxValue; // Infinite spawning
            _spawnedCount = 0;
            _killedCount = 0;
            _finishedCount = 0;

            float delay = 1.25f; // Start quickly
            _spawnCoroutine = StartCoroutine(SpawnRoutine(levelParams, delay));

            LogDebug($"Infinite spawn started. Delay={delay}s");
        }

        /// <summary>
        /// Stops the spawn coroutine.
        /// </summary>
        public void StopSpawning()
        {
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }
        }

        /// <summary>
        /// Destroys all remaining mobs (e.g., on level win with rainbow arrow).
        /// </summary>
        public void DestroyAllMobs()
        {
            StopSpawning();

            for (int i = _activeMobs.Count - 1; i >= 0; i--)
            {
                Mob mob = _activeMobs[i];
                mob.TakeDamage(9999); // Force kill
            }
        }

        /// <summary>
        /// Returns all active mobs to the pool.
        /// </summary>
        public void ClearAllMobs()
        {
            for (int i = _activeMobs.Count - 1; i >= 0; i--)
            {
                _mobPool.Release(_activeMobs[i]);
            }
            _activeMobs.Clear();
        }

        private IEnumerator SpawnRoutine(LevelParams levelParams, float initialDelay)
        {
            yield return new WaitForSeconds(initialDelay);

            // Askeri düzen (sıralı ve sık) için sabit kısa bir aralık
            WaitForSeconds spawnWait = new WaitForSeconds(0.85f);

            while (true)
            {
                // Only spawn if playing
                if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                {
                    yield return new WaitForSeconds(1f);
                    continue;
                }

                SpawnSingleMob(levelParams);
                yield return spawnWait;
            }
        }

        private void SpawnSingleMob(LevelParams levelParams)
        {
            Mob mob = _mobPool.Get();
            
            // Can ölçeği (HP Scaling): 3'erli gruplar halinde artıyor
            int groupIndex = _spawnedCount / 3;
            int hp = 5;
            if (groupIndex == 1) hp = 7;
            else if (groupIndex == 2) hp = 10;
            else if (groupIndex > 2) hp = 10 + (groupIndex - 2) * 4;

            // Daha yavaş sabit hız
            float speed = 1f;

            mob.Initialize(_spawnedCount, hp, speed);
            _activeMobs.Add(mob);
            _spawnedCount++;
        }

        private void HandleMobKilled(Mob mob)
        {
            _killedCount++;
            RemoveMob(mob);
        }

        private void HandleMobFinished(Mob mob)
        {
            _finishedCount++;
            RemoveMob(mob);
        }

        private void RemoveMob(Mob mob)
        {
            _activeMobs.Remove(mob);
            _mobPool.Release(mob);
        }

        protected override void OnDestroy()
        {
            Mob.OnMobKilled -= HandleMobKilled;
            Mob.OnMobFinished -= HandleMobFinished;
            base.OnDestroy();
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] MobSpawner: {message}");
        }
    }
}
