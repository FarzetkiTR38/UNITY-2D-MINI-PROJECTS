// ============================================================================
// ObjectPoolTemplate.cs
// Purpose: Generic object pool wrapper around Unity's ObjectPool<T>
// Dependencies: UnityEngine.Pool
// Unity Version: 6000.3.18f1
// ============================================================================

using System;
using UnityEngine;
using UnityEngine.Pool;

namespace GameName.Core.Patterns
{
    /// <summary>
    /// Represents an object that can be pooled using the object pool system.
    /// Implement this interface on any MonoBehaviour that will be pooled.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>Called when the object is retrieved from the pool. Reset state here.</summary>
        void OnGetFromPool();

        /// <summary>Called when the object is returned to the pool. Clean up here.</summary>
        void OnReturnToPool();
    }

    /// <summary>
    /// Generic MonoBehaviour object pool for prefab instances.
    /// Wraps Unity's <see cref="ObjectPool{T}"/> with prefab instantiation
    /// and automatic IPoolable callbacks.
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> Eliminates Instantiate/Destroy overhead for
    /// frequently spawned objects (projectiles, VFX, enemies, pickups).</para>
    /// <para><b>Inspector Setup:</b></para>
    /// <list type="bullet">
    ///   <item>Assign the prefab to pool.</item>
    ///   <item>Set default capacity for pre-warming.</item>
    ///   <item>Set max size to prevent unbounded growth.</item>
    /// </list>
    /// <para><b>Usage:</b></para>
    /// <code>
    /// // Get from pool:
    /// GameObject obj = _pool.Get();
    /// obj.transform.position = spawnPosition;
    ///
    /// // Return to pool:
    /// _pool.Return(obj);
    /// </code>
    /// <para><b>Performance:</b> Pre-warms pool in Awake. Get/Return are O(1).
    /// No GC allocations during normal operation.</para>
    /// </remarks>
    public class GameObjectPool : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Pool Configuration")]
        [Tooltip("The prefab to pool instances of.")]
        [SerializeField]
        private GameObject _prefab;

        [Tooltip("Number of instances to pre-create on startup.")]
        [SerializeField, Min(0)]
        private int _defaultCapacity = 10;

        [Tooltip("Maximum number of pooled instances. Excess will be destroyed.")]
        [SerializeField, Min(1)]
        private int _maxSize = 50;

        [Tooltip("Pre-warm the pool in Awake by instantiating defaultCapacity objects.")]
        [SerializeField]
        private bool _preWarm = true;

        [Header("Debug")]
        [Tooltip("Parent transform for pooled objects. Uses this transform if null.")]
        [SerializeField]
        private Transform _poolParent;

        #endregion

        #region Private Fields

        private ObjectPool<GameObject> _pool;

        #endregion

        #region Properties

        /// <summary>Gets the number of objects currently in the pool (inactive).</summary>
        public int CountInactive => _pool?.CountInactive ?? 0;

        /// <summary>Gets the total number of objects created by this pool.</summary>
        public int CountAll => _pool?.CountAll ?? 0;

        /// <summary>Gets the number of objects currently active (in use).</summary>
        public int CountActive => _pool != null ? _pool.CountAll - _pool.CountInactive : 0;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_prefab == null)
            {
                Debug.LogError($"[{name}] Pool prefab is not assigned!", this);
                return;
            }

            if (_poolParent == null)
            {
                _poolParent = transform;
            }

            _pool = new ObjectPool<GameObject>(
                createFunc: CreatePooledObject,
                actionOnGet: OnGetFromPool,
                actionOnRelease: OnReturnToPool,
                actionOnDestroy: OnDestroyPooledObject,
                collectionCheck: false,
                defaultCapacity: _defaultCapacity,
                maxSize: _maxSize
            );

            if (_preWarm)
            {
                PreWarmPool();
            }
        }

        private void OnValidate()
        {
            if (_prefab == null)
            {
                Debug.LogWarning($"[{name}] Pool prefab is not assigned.", this);
            }

            if (_maxSize < _defaultCapacity)
            {
                Debug.LogWarning($"[{name}] Max size ({_maxSize}) is less than default capacity ({_defaultCapacity}).", this);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets an object from the pool. Activates it and calls IPoolable.OnGetFromPool.
        /// </summary>
        /// <returns>An active GameObject from the pool.</returns>
        public GameObject Get()
        {
            if (_pool == null)
            {
                Debug.LogError($"[{name}] Pool is not initialized!", this);
                return null;
            }

            return _pool.Get();
        }

        /// <summary>
        /// Gets an object from the pool and positions it.
        /// </summary>
        /// <param name="position">The position to place the object.</param>
        /// <param name="rotation">The rotation to apply to the object.</param>
        /// <returns>An active, positioned GameObject from the pool.</returns>
        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            GameObject obj = Get();
            if (obj != null)
            {
                obj.transform.SetPositionAndRotation(position, rotation);
            }
            return obj;
        }

        /// <summary>
        /// Returns an object to the pool. Deactivates it and calls IPoolable.OnReturnToPool.
        /// </summary>
        /// <param name="obj">The object to return to the pool.</param>
        public void Return(GameObject obj)
        {
            if (obj == null)
            {
                Debug.LogWarning($"[{name}] Attempted to return null object to pool.", this);
                return;
            }

            if (_pool == null)
            {
                Debug.LogWarning($"[{name}] Pool is not initialized. Destroying object.", this);
                Destroy(obj);
                return;
            }

            _pool.Release(obj);
        }

        /// <summary>
        /// Returns an object to the pool after a delay.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        /// <param name="delay">Delay in seconds before returning.</param>
        public async void ReturnDelayed(GameObject obj, float delay)
        {
            if (obj == null || _pool == null) return;

            await Awaitable.WaitForSecondsAsync(delay);

            if (obj != null && obj.activeSelf)
            {
                Return(obj);
            }
        }

        /// <summary>Clears the pool, destroying all inactive objects.</summary>
        public void Clear()
        {
            _pool?.Clear();
        }

        #endregion

        #region Private Methods

        private GameObject CreatePooledObject()
        {
            GameObject obj = Instantiate(_prefab, _poolParent);
            obj.SetActive(false);
            return obj;
        }

        private void OnGetFromPool(GameObject obj)
        {
            obj.SetActive(true);

            if (obj.TryGetComponent(out IPoolable poolable))
            {
                poolable.OnGetFromPool();
            }
        }

        private void OnReturnToPool(GameObject obj)
        {
            if (obj.TryGetComponent(out IPoolable poolable))
            {
                poolable.OnReturnToPool();
            }

            obj.SetActive(false);
            obj.transform.SetParent(_poolParent);
        }

        private void OnDestroyPooledObject(GameObject obj)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        private void PreWarmPool()
        {
            GameObject[] preWarmed = new GameObject[_defaultCapacity];

            for (int i = 0; i < _defaultCapacity; i++)
            {
                preWarmed[i] = _pool.Get();
            }

            for (int i = 0; i < _defaultCapacity; i++)
            {
                _pool.Release(preWarmed[i]);
            }
        }

        #endregion

        #region Context Menu

        [ContextMenu("Debug/Log Pool Stats")]
        private void DebugLogPoolStats()
        {
            Debug.Log($"[{name}] Pool Stats — Active: {CountActive}, " +
                      $"Inactive: {CountInactive}, Total: {CountAll}", this);
        }

        #endregion
    }
}
