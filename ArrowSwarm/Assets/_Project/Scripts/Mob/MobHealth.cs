namespace ArrowSwarm.Mob
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Manages mob health points, damage reception, and death.
    /// </summary>
    public class MobHealth : MonoBehaviour
    {
        [SerializeField] private int _maxHP;
        [SerializeField] private int _currentHP;

        /// <summary>Maximum HP of this mob.</summary>
        public int MaxHP => _maxHP;

        /// <summary>Current HP of this mob.</summary>
        public int CurrentHP => _currentHP;

        /// <summary>Whether this mob is alive.</summary>
        public bool IsAlive => _currentHP > 0;

        /// <summary>HP ratio (0 to 1).</summary>
        public float HPRatio => _maxHP > 0 ? (float)_currentHP / _maxHP : 0f;

        /// <summary>Fired when this mob takes damage (damage amount, remaining HP).</summary>
        public event Action<int, int> OnDamageTaken;

        /// <summary>Fired when this mob dies.</summary>
        public event Action OnDeath;

        /// <summary>
        /// Initializes mob health with the given max HP.
        /// </summary>
        public void Initialize(int maxHP)
        {
            _maxHP = maxHP;
            _currentHP = maxHP;
        }

        /// <summary>
        /// Applies damage to the mob.
        /// </summary>
        /// <param name="damage">Amount of damage to apply.</param>
        /// <returns>True if the mob died from this damage.</returns>
        public bool ApplyDamage(int damage)
        {
            if (!IsAlive) return false;

            _currentHP = Mathf.Max(0, _currentHP - damage);
            OnDamageTaken?.Invoke(damage, _currentHP);

            LogDebug($"Took {damage} damage. HP: {_currentHP}/{_maxHP}");

            if (_currentHP <= 0)
            {
                Die();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Resets health for pool reuse.
        /// </summary>
        public void ResetHealth()
        {
            _currentHP = _maxHP;
        }

        private void Die()
        {
            OnDeath?.Invoke();
            LogDebug("Mob died!");
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] MobHealth: {message}");
        }
    }
}
