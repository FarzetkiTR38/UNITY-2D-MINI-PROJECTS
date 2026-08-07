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
        [SerializeField] private ParticleSystem _hitSparkPrefab;
        [SerializeField] private ParticleSystem _confettiPrefab;
        [SerializeField] private ParticleSystem _rainbowTrailPrefab;

        private readonly Dictionary<string, Queue<ParticleSystem>> _pools = new();

        private void OnEnable()
        {
            Arrow.OnArrowFiredEvent += HandleArrowFired;
            GameManager.OnLevelWon += HandleLevelWon;
        }

        private void OnDisable()
        {
            Arrow.OnArrowFiredEvent -= HandleArrowFired;
            GameManager.OnLevelWon -= HandleLevelWon;
        }

        /// <summary>
        /// Spawns a particle effect at the given position.
        /// </summary>
        public void SpawnEffect(ParticleSystem prefab, Vector3 position, Color? color = null)
        {
            if (prefab == null) return;

            ParticleSystem ps = GetFromPool(prefab);
            ps.transform.position = position;

            if (color.HasValue)
            {
                var main = ps.main;
                main.startColor = color.Value;
            }

            ps.Play();
            StartCoroutine(ReturnToPoolAfterDuration(ps, prefab, ps.main.duration + ps.main.startLifetime.constantMax));
        }

        /// <summary>
        /// Spawns a trail particle attached to a transform.
        /// </summary>
        public ParticleSystem SpawnTrail(ParticleSystem prefab, Transform parent, Color? color = null)
        {
            if (prefab == null) return null;

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

        private void HandleArrowFired(Arrow arrow)
        {
            var prefab = arrow.IsRainbow ? _rainbowTrailPrefab : _arrowTrailPrefab;
            if (prefab != null)
            {
                Color arrowColor = GameManager.Instance?.Config?.GetArrowColor(arrow.Weight) ?? Color.white;
                SpawnTrail(prefab, arrow.transform, arrowColor);
            }
        }



        private void HandleLevelWon()
        {
            if (_confettiPrefab != null)
            {
                Vector3 center = UnityEngine.Camera.main != null
                    ? UnityEngine.Camera.main.transform.position
                    : Vector3.zero;
                center.z = 0;
                SpawnEffect(_confettiPrefab, center);
            }
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
