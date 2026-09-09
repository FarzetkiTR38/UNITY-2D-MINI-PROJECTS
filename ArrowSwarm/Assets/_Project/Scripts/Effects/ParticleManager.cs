namespace ArrowSwarm.Effects
{
    using System.Collections.Generic;
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Core;
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

        private readonly Dictionary<string, Queue<ParticleSystem>> _pools = new();

        private void OnEnable()
        {
            GameManager.OnLevelWon += HandleLevelWon;
        }

        private void OnDisable()
        {
            GameManager.OnLevelWon -= HandleLevelWon;
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

        private void HandleLevelWon()
        {
            SpawnConfetti();
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

            return Instantiate(prefab, transform);
        }

        private System.Collections.IEnumerator ReturnToPoolAfterDuration(
            ParticleSystem ps, ParticleSystem prefab, float duration)
        {
            yield return new WaitForSeconds(duration);

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
