namespace ArrowSwarm.Effects
{
    using System.Collections.Generic;
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Mob;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Manages particle effects spawning for game events.
    /// Pools particle system prefabs for performance.
    /// </summary>
    public class ParticleManager : Singleton<ParticleManager>
    {
        [Header("Particle Prefabs")]
        [SerializeField] private ParticleSystem _arrowTrailPrefab;
        [SerializeField] private ParticleSystem _mobDeathPrefab;
        [SerializeField] private ParticleSystem _confettiPrefab;
        [Header("Fireworks Prefabs")]
        [SerializeField] private ParticleSystem _fireworkRainbowPrefab;
        [SerializeField] private ParticleSystem _firework1Prefab;
        [SerializeField] private ParticleSystem _fireworkBasicPrefab;

        [Header("Prewarm (instantiated during boot, never mid-level)")]
        [Tooltip("A rainbow finish kills every live mob in one frame; keep enough death bursts ready.")]
        [SerializeField, Min(0)] private int _mobDeathPrewarm = 18;
        [SerializeField, Min(0)] private int _fireworkPrewarm = 4;
        [SerializeField, Min(0)] private int _confettiPrewarm = 1;

        private readonly Dictionary<string, Queue<ParticleSystem>> _pools = new();
        private readonly Dictionary<float, WaitForSeconds> _returnWaits = new();

        /// <summary>Particle systems created since startup (prewarm included); diagnostics only.</summary>
        public int CreatedCount { get; private set; }

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();
            Prewarm(_mobDeathPrefab, _mobDeathPrewarm);
            Prewarm(_firework1Prefab, _fireworkPrewarm);
            Prewarm(_confettiPrefab, _confettiPrewarm);
        }

        private void Prewarm(ParticleSystem prefab, int count)
        {
            if (prefab == null) return;
            if (!_pools.TryGetValue(prefab.name, out var pool)) _pools[prefab.name] = pool = new Queue<ParticleSystem>();
            while (pool.Count < count)
            {
                var ps = CreateInstance(prefab);
                ps.gameObject.SetActive(false);
                pool.Enqueue(ps);
            }
        }

        private ParticleSystem CreateInstance(ParticleSystem prefab)
        {
            CreatedCount++;
            return Instantiate(prefab, transform);
        }

        /// <summary>
        /// Spawns a particle effect at the given position.
        /// </summary>
        public void SpawnEffect(ParticleSystem prefab, Vector3 position, Color? color = null)
        {
            if (prefab == null) return;
            if (Data.DataManager.Instance != null && Data.DataManager.Instance.PlayerData != null && !Data.DataManager.Instance.PlayerData.vfxEnabled) return;

            ParticleSystem ps = GetFromPool(prefab);
            ps.transform.position = position;

            if (color.HasValue)
            {
                var main = ps.main;
                main.startColor = color.Value;
            }

            ps.Play(true);
            float lifetime = ps.main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants
                ? ps.main.startLifetime.constantMax
                : ps.main.startLifetime.constant;
            StartCoroutine(ReturnToPoolAfterDuration(ps, prefab, Mathf.Max(0.8f, ps.main.duration + lifetime)));
        }

        /// <summary>
        /// Spawns the pooled mob death particle effect at the given position.
        /// </summary>
        public void SpawnMobDeathEffect(Vector3 position, Color? color = null)
        {
            if (_mobDeathPrefab != null)
            {
                SpawnEffect(_mobDeathPrefab, position, color);
            }
        }

        /// <summary>
        /// Spawns a trail particle attached to a transform.
        /// </summary>
        public ParticleSystem SpawnTrail(ParticleSystem prefab, Transform parent, Color? color = null)
        {
            if (prefab == null) return null;
            if (Data.DataManager.Instance != null && Data.DataManager.Instance.PlayerData != null && !Data.DataManager.Instance.PlayerData.vfxEnabled) return null;

            ParticleSystem ps = GetFromPool(prefab);
            ps.transform.SetParent(parent, false);
            ps.transform.localPosition = Vector3.zero;

            if (color.HasValue)
            {
                var main = ps.main;
                main.startColor = color.Value;
            }

            ps.Play();
            return ps;
        }

        /// <summary>
        /// Spawns the celebration fireworks barrage.
        /// </summary>
        public void SpawnFireworksCelebration(Vector3? position = null)
        {
            if (Data.DataManager.Instance != null && Data.DataManager.Instance.PlayerData != null && !Data.DataManager.Instance.PlayerData.vfxEnabled) return;

            Vector3 center = position ?? (UnityEngine.Camera.main != null
                ? UnityEngine.Camera.main.transform.position
                : Vector3.zero);
            center.z = 0;

            StartCoroutine(FireworksBarrageRoutine(center));
        }

        private System.Collections.IEnumerator FireworksBarrageRoutine(Vector3 center)
        {
            if (_firework1Prefab == null) yield break;

            // 1. Center Burst
            SpawnEffect(_firework1Prefab, center + new Vector3(0f, 0.4f, 0f));
            yield return new WaitForSeconds(0.20f);

            // 2. Left Burst
            SpawnEffect(_firework1Prefab, center + new Vector3(-1.5f, -0.3f, 0f));
            yield return new WaitForSeconds(0.16f);

            // 3. Right Burst
            SpawnEffect(_firework1Prefab, center + new Vector3(1.5f, 0.2f, 0f));
            yield return new WaitForSeconds(0.22f);

            // 4. Upper Center Climax Burst
            SpawnEffect(_firework1Prefab, center + new Vector3(0f, 1.5f, 0f));
        }

        /// <summary>
        /// Spawns the celebration confetti shower.
        /// </summary>
        public void SpawnConfetti(Vector3? position = null)
        {
            if (_confettiPrefab == null) return;
            if (Data.DataManager.Instance != null && Data.DataManager.Instance.PlayerData != null && !Data.DataManager.Instance.PlayerData.vfxEnabled) return;

            Vector3 pos = position ?? (UnityEngine.Camera.main != null
                ? UnityEngine.Camera.main.transform.position + new Vector3(0f, 5f, 0f)
                : new Vector3(0f, 5f, 0f));
            pos.z = 0f;

            SpawnEffect(_confettiPrefab, pos);
        }

        private ParticleSystem GetFromPool(ParticleSystem prefab)
        {
            string key = prefab.name;
            if (!_pools.ContainsKey(key))
            {
                _pools[key] = new Queue<ParticleSystem>();
            }

            if (_pools[key].Count > 0)
            {
                var ps = _pools[key].Dequeue();
                ps.gameObject.SetActive(true);
                return ps;
            }

            return CreateInstance(prefab);
        }

        private System.Collections.IEnumerator ReturnToPoolAfterDuration(
            ParticleSystem ps, ParticleSystem prefab, float duration)
        {
            // Durations come from a handful of prefabs; cache the waits instead of allocating per burst.
            float rounded = Mathf.Round(duration * 20f) / 20f;
            if (!_returnWaits.TryGetValue(rounded, out var wait)) _returnWaits[rounded] = wait = new WaitForSeconds(rounded);
            yield return wait;

            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.transform.SetParent(transform, false);
                ps.gameObject.SetActive(false);

                string key = prefab.name;
                if (!_pools.ContainsKey(key))
                {
                    _pools[key] = new Queue<ParticleSystem>();
                }
                _pools[key].Enqueue(ps);
            }
        }
    }
}
