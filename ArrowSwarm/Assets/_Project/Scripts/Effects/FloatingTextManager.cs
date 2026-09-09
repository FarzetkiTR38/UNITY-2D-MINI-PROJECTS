namespace ArrowSwarm.Effects
{
    using System.Collections.Generic;
    using ArrowSwarm.Data;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Manages an object pool of FloatingTextItem instances to display
    /// damage numbers, critical hits, and kill callouts with zero GC allocations.
    /// </summary>
    public class FloatingTextManager : Singleton<FloatingTextManager>
    {
        [Header("Colors")]
        [SerializeField] private Color _damageColor = new Color(1.0f, 0.72f, 0.30f, 1f); // #FFB74D
        [SerializeField] private Color _killColor = new Color(0.91f, 0.27f, 0.38f, 1f);   // #E94560

        [Header("Pool Settings")]
        [SerializeField] private int _initialPoolSize = 12;

        private readonly Queue<FloatingTextItem> _pool = new Queue<FloatingTextItem>();
        private Transform _poolContainer;

        protected override void OnSingletonAwake()
        {
            EnsurePoolContainer();
            WarmPool();
        }

        private void EnsurePoolContainer()
        {
            if (_poolContainer == null)
            {
                var container = new GameObject("FloatingTextContainer");
                container.transform.SetParent(transform, false);
                _poolContainer = container.transform;
            }
        }

        private void WarmPool()
        {
            for (int i = 0; i < _initialPoolSize; i++)
            {
                CreatePoolItem();
            }
        }

        private FloatingTextItem CreatePoolItem()
        {
            EnsurePoolContainer();
            var go = new GameObject("FloatingTextItem");
            go.transform.SetParent(_poolContainer, false);
            var item = go.AddComponent<FloatingTextItem>();
            go.SetActive(false);
            _pool.Enqueue(item);
            return item;
        }

        /// <summary>
        /// Displays a floating damage text above the specified position.
        /// </summary>
        public void ShowDamage(Vector3 worldPos, int damage, bool isKill = false)
        {
            if (DataManager.Instance?.PlayerData != null && !DataManager.Instance.PlayerData.vfxEnabled)
            {
                return;
            }

            FloatingTextItem item = _pool.Count > 0 ? _pool.Dequeue() : CreatePoolItem();
            string text = isKill ? "KILL!" : $"-{damage}";
            Color color = isKill ? _killColor : _damageColor;

            item.Spawn(text, worldPos, color, ReturnToPool);
        }

        private void ReturnToPool(FloatingTextItem item)
        {
            if (item != null)
            {
                _pool.Enqueue(item);
            }
        }
    }
}
