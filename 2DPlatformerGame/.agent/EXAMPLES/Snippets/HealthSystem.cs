// ============================================================================
// HealthSystem.cs
// Purpose: Reusable health system for any damageable entity
// Dependencies: None (standalone component)
// Unity Version: 6000.3.18f1
// ============================================================================

using System;
using UnityEngine;

namespace GameName.Gameplay.Combat
{
    /// <summary>
    /// Manages health, damage, healing, invincibility, and death for any entity.
    /// Implements <see cref="IDamageable"/> and <see cref="IHealable"/> interfaces.
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> Universal health management component. Attach to any
    /// GameObject that can take damage (player, enemies, destructibles).</para>
    /// <para><b>Inspector Setup:</b> Set max health and invincibility duration.
    /// Optionally assign event channels for cross-system notifications.</para>
    /// <para><b>Performance:</b> No allocations. Event invocation only.</para>
    /// </remarks>
    [DisallowMultipleComponent]
    public class HealthSystem : MonoBehaviour, IDamageable, IHealable
    {
        #region Constants

        private const int MinHealth = 0;

        #endregion

        #region Serialized Fields

        [Header("Health")]
        [Tooltip("Maximum health points for this entity.")]
        [SerializeField, Min(1)]
        private int _maxHealth = 100;

        [Space(10)]
        [Header("Invincibility")]
        [Tooltip("Duration of invincibility after taking damage.")]
        [SerializeField, Range(0f, 5f)]
        private float _invincibilityDuration = 1f;

        [Space(10)]
        [Header("Event Channels (Optional)")]
        [Tooltip("Raised when this entity takes damage.")]
        [SerializeField] private IntEventChannel _onDamagedChannel;

        [Tooltip("Raised when this entity dies.")]
        [SerializeField] private VoidEventChannel _onDiedChannel;

        #endregion

        #region Private Fields

        private int _currentHealth;
        private bool _isInvincible;
        private float _invincibilityTimer;
        private bool _isDead;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public int CurrentHealth => _currentHealth;

        /// <inheritdoc/>
        public int MaxHealth => _maxHealth;

        /// <inheritdoc/>
        public bool IsAlive => !_isDead;

        /// <summary>Gets a value indicating whether the entity is invincible.</summary>
        public bool IsInvincible => _isInvincible;

        /// <summary>Gets the health as a normalized ratio (0 to 1).</summary>
        public float HealthRatio => _maxHealth > 0 ? (float)_currentHealth / _maxHealth : 0f;

        #endregion

        #region Events

        /// <summary>Raised when health changes. Parameters: (currentHealth, maxHealth).</summary>
        public event Action<int, int> OnHealthChanged;

        /// <summary>Raised when damage is received. Parameter: damageAmount.</summary>
        public event Action<int> OnDamaged;

        /// <summary>Raised when healing is received. Parameter: healAmount.</summary>
        public event Action<int> OnHealed;

        /// <summary>Raised when the entity dies.</summary>
        public event Action OnDied;

        /// <summary>Raised when invincibility state changes. Parameter: isInvincible.</summary>
        public event Action<bool> OnInvincibilityChanged;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _currentHealth = _maxHealth;
            _isDead = false;
        }

        private void Update()
        {
            if (_isInvincible)
            {
                _invincibilityTimer -= Time.deltaTime;
                if (_invincibilityTimer <= 0f)
                {
                    _isInvincible = false;
                    OnInvincibilityChanged?.Invoke(false);
                }
            }
        }

        #endregion

        #region Public Methods — IDamageable

        /// <inheritdoc/>
        public bool TakeDamage(int amount, GameObject source = null)
        {
            if (amount < 0)
            {
                Debug.LogError($"[{name}] Negative damage: {amount}. Use Heal() instead.", this);
                return false;
            }

            if (_isDead) return false;
            if (_isInvincible) return false;
            if (amount == 0) return false;

            _currentHealth = Mathf.Max(MinHealth, _currentHealth - amount);

            OnDamaged?.Invoke(amount);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            _onDamagedChannel?.RaiseEvent(amount);

            // Start invincibility
            if (_invincibilityDuration > 0f)
            {
                _isInvincible = true;
                _invincibilityTimer = _invincibilityDuration;
                OnInvincibilityChanged?.Invoke(true);
            }

            if (_currentHealth <= MinHealth)
            {
                Die();
            }

            return true;
        }

        #endregion

        #region Public Methods — IHealable

        /// <summary>
        /// Heals the entity by the specified amount.
        /// Does nothing if the entity is dead.
        /// </summary>
        /// <param name="amount">The heal amount. Must be non-negative.</param>
        public void Heal(int amount)
        {
            if (amount < 0)
            {
                Debug.LogError($"[{name}] Negative heal: {amount}. Use TakeDamage() instead.", this);
                return;
            }

            if (_isDead) return;
            if (amount == 0) return;

            int previousHealth = _currentHealth;
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);

            int actualHeal = _currentHealth - previousHealth;
            if (actualHeal > 0)
            {
                OnHealed?.Invoke(actualHeal);
                OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            }
        }

        /// <summary>Sets max health and optionally heals to full.</summary>
        /// <param name="newMaxHealth">The new maximum health.</param>
        /// <param name="healToFull">If true, current health is set to new max.</param>
        public void SetMaxHealth(int newMaxHealth, bool healToFull = false)
        {
            _maxHealth = Mathf.Max(1, newMaxHealth);
            if (healToFull)
            {
                _currentHealth = _maxHealth;
            }
            else
            {
                _currentHealth = Mathf.Min(_currentHealth, _maxHealth);
            }
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        /// <summary>Enables invincibility for the specified duration.</summary>
        /// <param name="duration">Duration in seconds.</param>
        public void SetInvincible(float duration)
        {
            _isInvincible = true;
            _invincibilityTimer = duration;
            OnInvincibilityChanged?.Invoke(true);
        }

        /// <summary>Revives the entity with the specified health.</summary>
        /// <param name="healthAmount">Health to revive with. Defaults to max.</param>
        public void Revive(int healthAmount = -1)
        {
            if (!_isDead) return;

            _isDead = false;
            _currentHealth = healthAmount > 0 ? Mathf.Min(healthAmount, _maxHealth) : _maxHealth;
            _isInvincible = false;
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        #endregion

        #region Private Methods

        private void Die()
        {
            if (_isDead) return;

            _isDead = true;
            _isInvincible = false;
            OnDied?.Invoke();
            _onDiedChannel?.RaiseEvent();
        }

        #endregion

        #region Context Menu

        [ContextMenu("Debug/Reset Health")]
        private void DebugResetHealth()
        {
            _currentHealth = _maxHealth;
            _isDead = false;
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        [ContextMenu("Debug/Deal 10 Damage")]
        private void DebugDeal10Damage() => TakeDamage(10);

        [ContextMenu("Debug/Kill")]
        private void DebugKill() => TakeDamage(_currentHealth);

        [ContextMenu("Debug/Heal Full")]
        private void DebugHealFull() => Heal(_maxHealth);

        #endregion
    }

    /// <summary>Defines the contract for entities that can take damage.</summary>
    public interface IDamageable
    {
        /// <summary>Gets the current health.</summary>
        int CurrentHealth { get; }

        /// <summary>Gets the maximum health.</summary>
        int MaxHealth { get; }

        /// <summary>Gets whether the entity is alive.</summary>
        bool IsAlive { get; }

        /// <summary>Applies damage. Returns true if damage was applied.</summary>
        bool TakeDamage(int amount, GameObject source = null);
    }

    /// <summary>Defines the contract for entities that can be healed.</summary>
    public interface IHealable
    {
        /// <summary>Heals the entity by the specified amount.</summary>
        void Heal(int amount);
    }
}
