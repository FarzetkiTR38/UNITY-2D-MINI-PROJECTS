// ============================================================================
// ObjectPool.cs
// Purpose: Lightweight generic object pool for non-MonoBehaviour objects
// Dependencies: None
// Unity Version: 6000.3.18f1
// ============================================================================

using System;
using System.Collections.Generic;

namespace GameName.Core.Patterns
{
    /// <summary>
    /// Generic object pool for non-Unity objects (data containers, command objects, etc.).
    /// For GameObject pooling, use <see cref="GameObjectPool"/> or Unity's ObjectPool.
    /// </summary>
    /// <typeparam name="T">The type of object to pool. Must be a reference type with a parameterless constructor.</typeparam>
    /// <remarks>
    /// <para><b>Purpose:</b> Eliminates GC allocation for frequently created/destroyed
    /// plain C# objects like event args, data containers, and list builders.</para>
    /// <para><b>Usage:</b></para>
    /// <code>
    /// var pool = new GenericPool&lt;DamageInfo&gt;(initialCapacity: 16);
    /// DamageInfo info = pool.Get();
    /// info.Amount = 25;
    /// info.Source = attacker;
    /// // ... use info ...
    /// pool.Return(info);
    /// </code>
    /// <para><b>Performance:</b> O(1) Get and Return. No GC allocations
    /// after initial capacity is reached.</para>
    /// </remarks>
    public class GenericPool<T> where T : class, new()
    {
        #region Private Fields

        private readonly Stack<T> _pool;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onReturn;
        private readonly int _maxSize;

        #endregion

        #region Properties

        /// <summary>Gets the number of objects currently in the pool.</summary>
        public int Count => _pool.Count;

        /// <summary>Gets the maximum pool size.</summary>
        public int MaxSize => _maxSize;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new generic object pool.
        /// </summary>
        /// <param name="initialCapacity">Number of objects to pre-create.</param>
        /// <param name="maxSize">Maximum pool size. Excess returns are discarded.</param>
        /// <param name="onGet">Optional callback when an object is retrieved.</param>
        /// <param name="onReturn">Optional callback when an object is returned (use for reset).</param>
        public GenericPool(
            int initialCapacity = 8,
            int maxSize = 64,
            Action<T> onGet = null,
            Action<T> onReturn = null)
        {
            _pool = new Stack<T>(initialCapacity);
            _maxSize = maxSize;
            _onGet = onGet;
            _onReturn = onReturn;

            // Pre-warm
            for (int i = 0; i < initialCapacity; i++)
            {
                _pool.Push(new T());
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves an object from the pool, or creates a new one if empty.
        /// </summary>
        /// <returns>A pooled or new instance of T.</returns>
        public T Get()
        {
            T obj = _pool.Count > 0 ? _pool.Pop() : new T();
            _onGet?.Invoke(obj);
            return obj;
        }

        /// <summary>
        /// Returns an object to the pool for reuse.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        public void Return(T obj)
        {
            if (obj == null) return;

            _onReturn?.Invoke(obj);

            if (_pool.Count < _maxSize)
            {
                _pool.Push(obj);
            }
        }

        /// <summary>Clears the pool, releasing all pooled objects.</summary>
        public void Clear()
        {
            _pool.Clear();
        }

        #endregion
    }
}
