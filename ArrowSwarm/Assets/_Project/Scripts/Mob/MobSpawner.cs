namespace ArrowSwarm.Mob
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using ArrowSwarm.Core;
    using ArrowSwarm.Path;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Spawns mobs in an infinite continuous stream while level is active using object pooling.
    /// Manages relaxed perimeter-proportional speed, clear spacing,
    /// and event-driven backward gap-closing triggered when mobs in the chain die.
    /// </summary>
    public class MobSpawner : Singleton<MobSpawner>
    {
        [SerializeField] private Mob _mobPrefab;
        [SerializeField] private Transform _mobParent;

        private ObjectPool<Mob> _mobPool;
        private readonly List<Mob> _activeMobs = new List<Mob>();
        private Coroutine _spawnCoroutine;
        private Coroutine _gapCloseCoroutine;
        private int _spawnedCount;
        private int _killedCount;
        private int _finishedCount;

        private float _baseMobSpeed = 1.0f;
        private float _mobScaleFactor = 1.0f;
        private float _desiredSpacing = 1.6f;
        private float _gapCloseMultiplier = 1.5f;
        private bool _isFrozen;

        /// <summary>List of currently active mobs.</summary>
        public IReadOnlyList<Mob> ActiveMobs => _activeMobs;

        /// <summary>Number of mobs spawned so far.</summary>
        public int SpawnedCount => _spawnedCount;

        /// <summary>Number of mobs killed.</summary>
        public int KilledCount => _killedCount;

        /// <summary>Current base movement speed for mobs.</summary>
        public float BaseMobSpeed => _baseMobSpeed;

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
#if UNITY_EDITOR
                _mobPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<Mob>("Assets/_Project/Prefabs/Mob.prefab");
#endif
                if (_mobPrefab == null)
                {
                    Debug.LogError("[ArrowSwarm] MobSpawner: Mob prefab not assigned!");
                    return;
                }
            }

            if (_mobParent == null)
            {
                _mobParent = transform;
            }

            if (_mobPool != null)
            {
                _mobPool.Clear();
            }

            _mobPool = new ObjectPool<Mob>(
                _mobPrefab, _mobParent, 20, 150,
                onGet: m => { if (m != null) m.gameObject.SetActive(true); },
                onRelease: m =>
                {
                    if (m != null)
                    {
                        m.ResetMob();
                        m.gameObject.SetActive(false);
                    }
                }
            );

            // Subscribe to mob events safely
            Mob.OnMobKilled -= HandleMobKilled;
            Mob.OnMobFinished -= HandleMobFinished;
            Mob.OnMobKilled += HandleMobKilled;
            Mob.OnMobFinished += HandleMobFinished;

            ArrowSwarm.Skills.FreezeManager.OnFreezeStarted -= HandleFreezeStarted;
            ArrowSwarm.Skills.FreezeManager.OnFreezeEnded -= HandleFreezeEnded;
            ArrowSwarm.Skills.FreezeManager.OnFreezeStarted += HandleFreezeStarted;
            ArrowSwarm.Skills.FreezeManager.OnFreezeEnded += HandleFreezeEnded;
        }

        /// <summary>
        /// Starts infinite continuous spawning according to level parameters.
        /// </summary>
        public void StartSpawning(LevelParams levelParams)
        {
            StopSpawning();
            ClearAllMobs();

            _spawnedCount = 0;
            _killedCount = 0;
            _finishedCount = 0;

            GameConfig config = GameManager.Instance?.Config;
            float transitSeconds = config != null ? config.TargetTransitSeconds : 25.0f;
            _gapCloseMultiplier = config != null ? config.GapCloseSpeedMultiplier : 1.5f;
            float spacingMult = config != null ? config.MobSpacingMultiplier : 2.0f;
            float baseScale = config != null ? config.BaseMobScale : 1.0f;
            float mapScale = levelParams.MapScaleFactor > 0 ? levelParams.MapScaleFactor : 1.0f;
            _mobScaleFactor = baseScale * mapScale;

            // Calculate perimeter transit speed
            float totalPathLength = PathManager.HasInstance ? PathManager.Instance.TotalPathLength : 26f;
            if (totalPathLength < 1f) totalPathLength = 26f;
            _baseMobSpeed = DifficultyCalculator.GetMobSpeedForTransit(totalPathLength, transitSeconds);

            // Calculate spacing: mob sprite width * spacing multiplier
            float mobWorldWidth = 0.8f * _mobScaleFactor;
            _desiredSpacing = mobWorldWidth * spacingMult;

            // Calculate spawn interval so mobs spawn with clean, relaxed separation
            float calculatedInterval = _desiredSpacing / Mathf.Max(0.1f, _baseMobSpeed);
            float minInterval = config != null ? config.MinSpawnInterval : 1.6f;
            float spawnInterval = Mathf.Max(minInterval, calculatedInterval);

            _spawnCoroutine = StartCoroutine(InfiniteContinuousSpawnRoutine(levelParams, spawnInterval));

            LogDebug($"Infinite spawning started: Length={totalPathLength:F1}, Speed={_baseMobSpeed:F2}, Scale={_mobScaleFactor:F2}x, Spacing={_desiredSpacing:F2}, Interval={spawnInterval:F2}s");
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
            if (_gapCloseCoroutine != null)
            {
                StopCoroutine(_gapCloseCoroutine);
                _gapCloseCoroutine = null;
            }
        }

        /// <summary>
        /// Destroys all remaining mobs (e.g., on level win with rainbow arrow).
        /// </summary>
        public void DestroyAllMobs()
        {
            StopSpawning();

            var mobsToDestroy = new List<Mob>(_activeMobs);
            for (int i = mobsToDestroy.Count - 1; i >= 0; i--)
            {
                if (mobsToDestroy[i] != null && mobsToDestroy[i].gameObject.activeSelf)
                {
                    mobsToDestroy[i].TakeDamage(9999);
                }
            }
            _activeMobs.Clear();
        }

        /// <summary>
        /// Returns all active mobs to the pool.
        /// </summary>
        public void ClearAllMobs()
        {
            StopSpawning();

            var mobsToClear = new List<Mob>(_activeMobs);
            _activeMobs.Clear();

            for (int i = mobsToClear.Count - 1; i >= 0; i--)
            {
                if (mobsToClear[i] != null)
                {
                    _mobPool.Release(mobsToClear[i]);
                }
            }
        }

        private IEnumerator InfiniteContinuousSpawnRoutine(LevelParams levelParams, float spawnInterval)
        {
            yield return new WaitForSeconds(1.0f);

            WaitForSeconds spawnWait = new WaitForSeconds(spawnInterval);
            int baseHP = levelParams.MobHP > 0 ? levelParams.MobHP : DifficultyCalculator.GetMobHP(levelParams.Level);

            while (true)
            {
                while (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                {
                    yield return new WaitForSeconds(0.5f);
                }

                while (_isFrozen)
                {
                    yield return null;
                }

                // Gradual HP progression over time
                int groupIndex = _spawnedCount / 4;
                int mobHP = baseHP + (groupIndex * 2);

                SpawnSingleMob(mobHP);
                yield return spawnWait;
            }
        }

        private void SpawnSingleMob(int hp)
        {
            Mob mob = _mobPool.Get();
            mob.Initialize(_spawnedCount, hp, _baseMobSpeed, _mobScaleFactor);
            if (_isFrozen) mob.SetFrozen(true);
            _activeMobs.Add(mob);
            _spawnedCount++;
        }

        private void HandleFreezeStarted(float duration)
        {
            _isFrozen = true;
            for (int i = 0; i < _activeMobs.Count; i++)
            {
                Mob m = _activeMobs[i];
                if (m != null && m.IsAlive)
                {
                    m.SetFrozen(true);
                }
            }
        }

        private void HandleFreezeEnded()
        {
            _isFrozen = false;
            for (int i = 0; i < _activeMobs.Count; i++)
            {
                Mob m = _activeMobs[i];
                if (m != null && m.IsAlive)
                {
                    m.SetFrozen(false);
                }
            }
        }

        private void HandleMobKilled(Mob mob)
        {
            _killedCount++;

            // Event-driven gap closing: check if there was a front group and rear group
            int killedIndex = _activeMobs.IndexOf(mob);
            RemoveMob(mob);

            // If a mob in the middle/rear died, pull the front group backward to close the gap
            if (killedIndex > 0 && _activeMobs.Count >= killedIndex)
            {
                TriggerEventDrivenGapClose(killedIndex - 1);
            }
        }

        /// <summary>
        /// Triggers event-driven gap closing when a mob is killed.
        /// Front mobs smoothly reverse along the path to connect with the trailing group.
        /// </summary>
        private void TriggerEventDrivenGapClose(int frontGroupEndIndex)
        {
            if (frontGroupEndIndex < 0 || frontGroupEndIndex >= _activeMobs.Count) return;
            if (_activeMobs.Count <= frontGroupEndIndex + 1) return; // No rear mob to close to

            Mob mobAhead = _activeMobs[frontGroupEndIndex];
            Mob mobBehind = _activeMobs[frontGroupEndIndex + 1];

            if (mobAhead == null || mobBehind == null || !mobAhead.IsAlive || !mobBehind.IsAlive) return;

            float frontDist = mobAhead.Movement.CurrentPathDistance;
            float rearDist = mobBehind.Movement.CurrentPathDistance;
            float actualGap = frontDist - rearDist;

            if (actualGap > _desiredSpacing + 0.10f)
            {
                float excessDistance = actualGap - _desiredSpacing;
                if (_gapCloseCoroutine != null)
                {
                    StopCoroutine(_gapCloseCoroutine);
                }
                _gapCloseCoroutine = StartCoroutine(GapCloseRoutine(frontGroupEndIndex, excessDistance));
            }
        }

        private IEnumerator GapCloseRoutine(int frontGroupEndIndex, float excessDistance)
        {
            float reverseSpeed = _baseMobSpeed * _gapCloseMultiplier;
            float relativeClosingSpeed = reverseSpeed + _baseMobSpeed;
            float duration = Mathf.Clamp(excessDistance / relativeClosingSpeed, 0.1f, 1.5f);

            // Reverse front mobs
            for (int i = 0; i <= frontGroupEndIndex && i < _activeMobs.Count; i++)
            {
                Mob m = _activeMobs[i];
                if (m != null && m.IsAlive)
                {
                    m.Movement.SetSpeed(-reverseSpeed);
                }
            }

            yield return new WaitForSeconds(duration);

            // Restore forward base speed
            for (int i = 0; i <= frontGroupEndIndex && i < _activeMobs.Count; i++)
            {
                Mob m = _activeMobs[i];
                if (m != null && m.IsAlive)
                {
                    m.Movement.SetSpeed(_baseMobSpeed);
                }
            }

            _gapCloseCoroutine = null;
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
            ArrowSwarm.Skills.FreezeManager.OnFreezeStarted -= HandleFreezeStarted;
            ArrowSwarm.Skills.FreezeManager.OnFreezeEnded -= HandleFreezeEnded;
            base.OnDestroy();
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] MobSpawner: {message}");
        }
    }
}
