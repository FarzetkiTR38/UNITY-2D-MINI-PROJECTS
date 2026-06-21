using System.Collections.Generic;
using UnityEngine;

namespace NeonGalaxy.VFX
{
    /// <summary>
    /// Generic object pool for ParticleSystem instances.
    /// Pre-allocates a set of instances and recycles them
    /// to avoid runtime allocations and GC pressure.
    /// </summary>
    public class VFXPool
    {
        private readonly ParticleSystem _prefab;
        private readonly Transform _parent;
        private readonly Queue<ParticleSystem> _available;
        private readonly HashSet<ParticleSystem> _active;
        private readonly int _maxSize;

        public VFXPool(ParticleSystem prefab, Transform parent, int initialSize)
        {
            _prefab = prefab;
            _parent = parent;
            _maxSize = Mathf.Max(initialSize, 1);
            _available = new Queue<ParticleSystem>(initialSize);
            _active = new HashSet<ParticleSystem>();

            // Pre-warm pool
            for (int i = 0; i < initialSize; i++)
            {
                var instance = CreateInstance();
                _available.Enqueue(instance);
            }
        }

        /// <summary>
        /// Gets a particle system from the pool, positions it, and plays it.
        /// </summary>
        public ParticleSystem Get(Vector3 position)
        {
            ParticleSystem ps;

            if (_available.Count > 0)
            {
                ps = _available.Dequeue();
            }
            else if (_active.Count < _maxSize)
            {
                ps = CreateInstance();
            }
            else
            {
                // Pool exhausted — return null (graceful degrade)
                return null;
            }

            if (ps == null)
            {
                ps = CreateInstance();
            }

            ps.transform.position = position;
            ps.gameObject.SetActive(true);
            ps.Play(true);
            _active.Add(ps);

            return ps;
        }

        /// <summary>
        /// Gets a particle system and applies a color override to the main module.
        /// </summary>
        public ParticleSystem Get(Vector3 position, Color color)
        {
            var ps = Get(position);
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = color;
            }
            return ps;
        }

        /// <summary>
        /// Returns a particle system to the pool.
        /// </summary>
        public void Return(ParticleSystem ps)
        {
            if (ps == null) return;

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.gameObject.SetActive(false);
            _active.Remove(ps);
            _available.Enqueue(ps);
        }

        /// <summary>
        /// Checks active particles and returns any that have finished playing.
        /// Call this periodically (e.g., in Update) to auto-return finished effects.
        /// </summary>
        public void RecycleFinished()
        {
            var toReturn = new List<ParticleSystem>();
            foreach (var ps in _active)
            {
                if (ps == null || (!ps.isPlaying && !ps.isEmitting))
                {
                    toReturn.Add(ps);
                }
            }

            for (int i = 0; i < toReturn.Count; i++)
            {
                Return(toReturn[i]);
            }
        }

        /// <summary>
        /// Returns all active instances to the pool.
        /// </summary>
        public void ReturnAll()
        {
            var allActive = new List<ParticleSystem>(_active);
            for (int i = 0; i < allActive.Count; i++)
            {
                Return(allActive[i]);
            }
        }

        private ParticleSystem CreateInstance()
        {
            if (_prefab == null) return null;

            var instance = Object.Instantiate(_prefab, _parent);
            instance.gameObject.SetActive(false);

            // Ensure auto-play is off
            var main = instance.main;
            main.playOnAwake = false;

            return instance;
        }
    }
}
