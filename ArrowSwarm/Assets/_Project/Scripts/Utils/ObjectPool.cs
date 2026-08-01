namespace ArrowSwarm.Utils
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Generic object pool for reusing GameObjects to avoid GC spikes.
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _pool = new Queue<T>();
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly int _maxSize;

        /// <summary>
        /// Creates a new object pool.
        /// </summary>
        /// <param name="prefab">Prefab to instantiate.</param>
        /// <param name="parent">Parent transform for pooled objects.</param>
        /// <param name="initialSize">Number of objects to pre-instantiate.</param>
        /// <param name="maxSize">Maximum pool size (0 = unlimited).</param>
        /// <param name="onGet">Called when an object is retrieved from the pool.</param>
        /// <param name="onRelease">Called when an object is returned to the pool.</param>
        public ObjectPool(T prefab, Transform parent, int initialSize = 10,
            int maxSize = 0, Action<T> onGet = null, Action<T> onRelease = null)
        {
            _prefab = prefab;
            _parent = parent;
            _maxSize = maxSize;
            _onGet = onGet;
            _onRelease = onRelease;

            for (int i = 0; i < initialSize; i++)
            {
                var obj = CreateNewObject();
                obj.gameObject.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        /// <summary>
        /// Gets an object from the pool or creates a new one if empty.
        /// </summary>
        public T Get()
        {
            T obj;

            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
            }
            else
            {
                obj = CreateNewObject();
            }

            obj.gameObject.SetActive(true);
            _onGet?.Invoke(obj);
            return obj;
        }

        /// <summary>
        /// Returns an object back to the pool.
        /// </summary>
        public void Release(T obj)
        {
            if (obj == null) return;

            _onRelease?.Invoke(obj);
            obj.gameObject.SetActive(false);

            if (_maxSize > 0 && _pool.Count >= _maxSize)
            {
                UnityEngine.Object.Destroy(obj.gameObject);
                return;
            }

            _pool.Enqueue(obj);
        }

        /// <summary>
        /// Clears all pooled objects.
        /// </summary>
        public void Clear()
        {
            while (_pool.Count > 0)
            {
                var obj = _pool.Dequeue();
                if (obj != null)
                {
                    UnityEngine.Object.Destroy(obj.gameObject);
                }
            }
        }

        /// <summary>
        /// Current number of available objects in the pool.
        /// </summary>
        public int CountInactive => _pool.Count;

        private T CreateNewObject()
        {
            var obj = UnityEngine.Object.Instantiate(_prefab, _parent);
            return obj;
        }
    }
}
