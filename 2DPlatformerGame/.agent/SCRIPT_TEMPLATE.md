# Script Template Guide

## Overview

This document provides production-ready script templates for all common Unity script types. Every generated script MUST follow these templates to ensure consistency, documentation quality, and Inspector usability.

---

## 1. MonoBehaviour Template

```csharp
// ============================================================================
// [ClassName].cs
// Purpose: [One-line description of what this component does]
// Dependencies: [Required components, systems, or services]
// Unity Version: 6000.3.18f1
// ============================================================================

using System;
using UnityEngine;

namespace GameName.Gameplay
{
    /// <summary>
    /// [Detailed description of the class purpose and behavior.]
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> [Why this class exists and what problem it solves.]</para>
    /// <para><b>Dependencies:</b>
    /// <list type="bullet">
    ///   <item><see cref="Rigidbody2D"/> — Required for physics-based movement.</item>
    ///   <item><see cref="Animator"/> — Required for animation playback.</item>
    /// </list>
    /// </para>
    /// <para><b>Inspector Setup:</b>
    /// <list type="bullet">
    ///   <item>Assign <c>_groundCheck</c> to the child Transform used for ground detection.</item>
    ///   <item>Set <c>_groundLayer</c> to the physics layer used for ground tiles.</item>
    ///   <item>Tune <c>_moveSpeed</c> and <c>_jumpForce</c> for desired feel.</item>
    /// </list>
    /// </para>
    /// <para><b>Usage:</b> Attach to the Player GameObject. Requires InputHandler
    /// component to feed input data.</para>
    /// <para><b>Performance:</b> Ground check uses OverlapCircle (single allocation).
    /// Animation hashes are cached as static readonly.</para>
    /// <para><b>Extension:</b> Override movement in derived classes or swap
    /// movement strategy via IMovementStrategy interface.</para>
    /// </remarks>
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public class PlayerMovement : MonoBehaviour
    {
        #region Constants

        private const float CoyoteTimeDuration = 0.12f;
        private const float JumpBufferDuration = 0.15f;
        private const float GroundCheckRadius = 0.2f;

        private static readonly int AnimHash_Speed = Animator.StringToHash("Speed");
        private static readonly int AnimHash_IsGrounded = Animator.StringToHash("IsGrounded");

        #endregion

        #region Serialized Fields

        [Header("Movement")]
        [Tooltip("Maximum horizontal movement speed in units per second.")]
        [SerializeField, Min(0f)]
        private float _moveSpeed = 8f;

        [Tooltip("Vertical force applied when jumping.")]
        [SerializeField, Min(0f)]
        private float _jumpForce = 14f;

        [Tooltip("Gravity multiplier when falling for snappier feel.")]
        [SerializeField, Range(1f, 10f)]
        private float _fallMultiplier = 2.5f;

        [Tooltip("Gravity multiplier for short jumps (release button early).")]
        [SerializeField, Range(1f, 10f)]
        private float _lowJumpMultiplier = 2f;

        [Space(10)]

        [Header("Ground Detection")]
        [Tooltip("Child transform marking the ground check position.")]
        [SerializeField]
        private Transform _groundCheckPoint;

        [Tooltip("Layers considered as ground for ground detection.")]
        [SerializeField]
        private LayerMask _groundLayer;

        [Space(10)]

        [Header("Debug")]
        [Tooltip("Show ground check gizmo in Scene view.")]
        [SerializeField]
        private bool _showDebugGizmos = true;

        #endregion

        #region Private Fields

        private Rigidbody2D _rigidbody2D;
        private Animator _animator;
        private Vector2 _moveInput;
        private bool _isGrounded;
        private bool _wasGrounded;
        private bool _jumpRequested;
        private float _coyoteTimeCounter;
        private float _jumpBufferCounter;
        private bool _isFacingRight = true;

        #endregion

        #region Properties

        /// <summary>Gets a value indicating whether the player is on the ground.</summary>
        public bool IsGrounded => _isGrounded;

        /// <summary>Gets the current horizontal speed magnitude.</summary>
        public float CurrentSpeed => Mathf.Abs(_rigidbody2D.linearVelocity.x);

        /// <summary>Gets the current vertical velocity.</summary>
        public float VerticalVelocity => _rigidbody2D.linearVelocity.y;

        #endregion

        #region Events

        /// <summary>Raised when the player lands after being airborne.</summary>
        public event Action OnLanded;

        /// <summary>Raised when the player successfully jumps.</summary>
        public event Action OnJumped;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            TryGetComponent(out _animator);

            Debug.Assert(_rigidbody2D != null, $"[{name}] Rigidbody2D is required.", this);
        }

        private void Update()
        {
            CheckGround();
            HandleCoyoteTime();
            HandleJumpBuffer();
            UpdateAnimator();
            HandleFlip();
        }

        private void FixedUpdate()
        {
            ApplyMovement();
            ApplyBetterJumpGravity();
        }

        private void OnValidate()
        {
            if (_groundCheckPoint == null)
            {
                Debug.LogWarning($"[{name}] Ground Check Point is not assigned.", this);
            }

            if (_groundLayer == 0)
            {
                Debug.LogWarning($"[{name}] Ground Layer is not set.", this);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!_showDebugGizmos || _groundCheckPoint == null) return;

            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(_groundCheckPoint.position, GroundCheckRadius);
        }

        #endregion

        #region Public Methods

        /// <summary>Sets the movement input direction.</summary>
        /// <param name="input">The input vector. X component used for horizontal movement.</param>
        public void SetMoveInput(Vector2 input)
        {
            _moveInput = input;
        }

        /// <summary>Requests a jump. Will execute if grounded or within coyote time.</summary>
        public void RequestJump()
        {
            _jumpBufferCounter = JumpBufferDuration;
        }

        #endregion

        #region Private Methods

        private void CheckGround()
        {
            _wasGrounded = _isGrounded;

            _isGrounded = _groundCheckPoint != null &&
                          Physics2D.OverlapCircle(
                              _groundCheckPoint.position,
                              GroundCheckRadius,
                              _groundLayer);

            if (!_wasGrounded && _isGrounded)
            {
                OnLanded?.Invoke();
            }
        }

        private void HandleCoyoteTime()
        {
            if (_isGrounded)
            {
                _coyoteTimeCounter = CoyoteTimeDuration;
            }
            else
            {
                _coyoteTimeCounter -= Time.deltaTime;
            }
        }

        private void HandleJumpBuffer()
        {
            if (_jumpBufferCounter > 0f)
            {
                _jumpBufferCounter -= Time.deltaTime;

                if (_coyoteTimeCounter > 0f)
                {
                    ExecuteJump();
                    _jumpBufferCounter = 0f;
                }
            }
        }

        private void ApplyMovement()
        {
            float targetVelocityX = _moveInput.x * _moveSpeed;
            _rigidbody2D.linearVelocity = new Vector2(targetVelocityX, _rigidbody2D.linearVelocity.y);
        }

        private void ExecuteJump()
        {
            _rigidbody2D.linearVelocity = new Vector2(_rigidbody2D.linearVelocity.x, _jumpForce);
            _coyoteTimeCounter = 0f;
            _isGrounded = false;
            OnJumped?.Invoke();
        }

        private void ApplyBetterJumpGravity()
        {
            if (_rigidbody2D.linearVelocity.y < 0f)
            {
                _rigidbody2D.linearVelocity += Vector2.up * (Physics2D.gravity.y * (_fallMultiplier - 1f) * Time.fixedDeltaTime);
            }
            else if (_rigidbody2D.linearVelocity.y > 0f && _jumpBufferCounter <= 0f)
            {
                _rigidbody2D.linearVelocity += Vector2.up * (Physics2D.gravity.y * (_lowJumpMultiplier - 1f) * Time.fixedDeltaTime);
            }
        }

        private void HandleFlip()
        {
            if (_moveInput.x > 0f && !_isFacingRight || _moveInput.x < 0f && _isFacingRight)
            {
                Flip();
            }
        }

        private void Flip()
        {
            _isFacingRight = !_isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }

        private void UpdateAnimator()
        {
            if (_animator == null) return;

            _animator.SetFloat(AnimHash_Speed, Mathf.Abs(_moveInput.x));
            _animator.SetBool(AnimHash_IsGrounded, _isGrounded);
        }

        #endregion
    }
}
```

---

## 2. ScriptableObject Template

```csharp
// ============================================================================
// [DataClassName].cs
// Purpose: [One-line description of this data definition]
// Usage: Create via Assets → Create → [MenuPath]
// Unity Version: 6000.3.18f1
// ============================================================================

using UnityEngine;

namespace GameName.Data
{
    /// <summary>
    /// Defines the configuration data for [entity/system].
    /// Used as a design-time asset to drive [system] behavior.
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> Data-driven configuration for [system].
    /// Designers can create and tweak assets without modifying code.</para>
    /// <para><b>Creation:</b> Assets → Create → GameName → Data → [Asset Name]</para>
    /// <para><b>Usage:</b> Reference from MonoBehaviour components via [SerializeField].
    /// Do NOT modify at runtime — treat as read-only design data.</para>
    /// </remarks>
    [CreateAssetMenu(
        fileName = "New_EnemyData",
        menuName = "GameName/Data/Enemy Data",
        order = 0)]
    public class EnemyData : ScriptableObject
    {
        #region Identity

        [Header("Identity")]
        [Tooltip("Display name of this enemy type.")]
        [SerializeField]
        private string _displayName = "New Enemy";

        [Tooltip("Unique identifier for save/load and registry purposes.")]
        [SerializeField]
        private string _enemyId = "";

        [Tooltip("Description shown in bestiary or debug info.")]
        [SerializeField, TextArea(2, 5)]
        private string _description = "";

        #endregion

        #region Stats

        [Space(10)]
        [Header("Health")]
        [Tooltip("Maximum health points.")]
        [SerializeField, Min(1)]
        private int _maxHealth = 50;

        [Space(10)]
        [Header("Combat")]
        [Tooltip("Base contact damage dealt to the player.")]
        [SerializeField, Min(0)]
        private int _contactDamage = 10;

        [Tooltip("Time between attacks in seconds.")]
        [SerializeField, Min(0.1f)]
        private float _attackCooldown = 1.5f;

        [Tooltip("Attack range in units.")]
        [SerializeField, Min(0f)]
        private float _attackRange = 1.5f;

        [Space(10)]
        [Header("Movement")]
        [Tooltip("Movement speed in units per second.")]
        [SerializeField, Min(0f)]
        private float _moveSpeed = 3f;

        [Tooltip("Detection range for spotting the player.")]
        [SerializeField, Min(0f)]
        private float _detectionRange = 8f;

        #endregion

        #region Rewards

        [Space(10)]
        [Header("Rewards")]
        [Tooltip("Experience points awarded on kill.")]
        [SerializeField, Min(0)]
        private int _experienceReward = 25;

        [Tooltip("Currency dropped on death.")]
        [SerializeField, Min(0)]
        private int _currencyDrop = 10;

        #endregion

        #region Visual

        [Space(10)]
        [Header("Visual")]
        [Tooltip("Prefab to instantiate for this enemy.")]
        [SerializeField]
        private GameObject _prefab;

        [Tooltip("Icon for UI display.")]
        [SerializeField]
        private Sprite _icon;

        #endregion

        #region Pooling

        [Space(10)]
        [Header("Pooling")]
        [Tooltip("Default number of pre-instantiated instances.")]
        [SerializeField, Min(1)]
        private int _defaultPoolSize = 5;

        [Tooltip("Maximum pool size before objects are destroyed.")]
        [SerializeField, Min(1)]
        private int _maxPoolSize = 20;

        #endregion

        #region Properties

        /// <summary>Gets the display name.</summary>
        public string DisplayName => _displayName;

        /// <summary>Gets the unique identifier.</summary>
        public string EnemyId => _enemyId;

        /// <summary>Gets the description.</summary>
        public string Description => _description;

        /// <summary>Gets the maximum health.</summary>
        public int MaxHealth => _maxHealth;

        /// <summary>Gets the contact damage.</summary>
        public int ContactDamage => _contactDamage;

        /// <summary>Gets the attack cooldown in seconds.</summary>
        public float AttackCooldown => _attackCooldown;

        /// <summary>Gets the attack range in units.</summary>
        public float AttackRange => _attackRange;

        /// <summary>Gets the movement speed.</summary>
        public float MoveSpeed => _moveSpeed;

        /// <summary>Gets the player detection range.</summary>
        public float DetectionRange => _detectionRange;

        /// <summary>Gets the experience reward.</summary>
        public int ExperienceReward => _experienceReward;

        /// <summary>Gets the currency drop amount.</summary>
        public int CurrencyDrop => _currencyDrop;

        /// <summary>Gets the enemy prefab.</summary>
        public GameObject Prefab => _prefab;

        /// <summary>Gets the enemy icon.</summary>
        public Sprite Icon => _icon;

        /// <summary>Gets the default pool size.</summary>
        public int DefaultPoolSize => _defaultPoolSize;

        /// <summary>Gets the maximum pool size.</summary>
        public int MaxPoolSize => _maxPoolSize;

        #endregion
    }
}
```

---

## 3. Interface Template

```csharp
// ============================================================================
// I[InterfaceName].cs
// Purpose: [One-line description of this contract]
// Unity Version: 6000.3.18f1
// ============================================================================

using UnityEngine;

namespace GameName.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for [what this interface represents].
    /// </summary>
    /// <remarks>
    /// <para><b>Implementors:</b> <see cref="HealthSystem"/>, <see cref="EnemyHealth"/>.</para>
    /// <para><b>Usage:</b> Use with TryGetComponent for safe interaction:
    /// <code>
    /// if (collision.TryGetComponent(out IDamageable damageable))
    /// {
    ///     damageable.TakeDamage(10);
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    public interface IDamageable
    {
        /// <summary>Gets the current health.</summary>
        int CurrentHealth { get; }

        /// <summary>Gets the maximum health.</summary>
        int MaxHealth { get; }

        /// <summary>Gets a value indicating whether this entity is alive.</summary>
        bool IsAlive { get; }

        /// <summary>
        /// Applies damage to this entity.
        /// </summary>
        /// <param name="amount">The damage amount. Must be non-negative.</param>
        /// <param name="source">The source of the damage. Can be null for environmental damage.</param>
        /// <returns><c>true</c> if the damage was applied; <c>false</c> if blocked or invincible.</returns>
        bool TakeDamage(int amount, GameObject source = null);
    }
}
```

---

## 4. Event Channel Template

```csharp
// ============================================================================
// [Type]EventChannel.cs
// Purpose: ScriptableObject event channel carrying [type] data
// Unity Version: 6000.3.18f1
// ============================================================================

using System;
using UnityEngine;

namespace GameName.Core.Events
{
    /// <summary>
    /// ScriptableObject-based event channel that carries [description] data.
    /// Used for decoupled communication between systems.
    /// </summary>
    /// <remarks>
    /// <para><b>Creation:</b> Assets → Create → Events → [Type] Event Channel</para>
    /// <para><b>Raiser:</b> Assign to the component that produces the event.
    /// Call <see cref="RaiseEvent"/> when the event occurs.</para>
    /// <para><b>Listener:</b> Assign to the component that consumes the event.
    /// Subscribe to <see cref="OnEventRaised"/> in OnEnable, unsubscribe in OnDisable.</para>
    /// </remarks>
    [CreateAssetMenu(
        fileName = "New_IntEventChannel",
        menuName = "Events/Int Event Channel",
        order = 1)]
    public class IntEventChannel : ScriptableObject
    {
        /// <summary>Raised when this event channel is invoked.</summary>
        public event Action<int> OnEventRaised;

        /// <summary>
        /// Raises the event, notifying all subscribers.
        /// </summary>
        /// <param name="value">The integer value to pass to listeners.</param>
        public void RaiseEvent(int value)
        {
            if (OnEventRaised == null)
            {
                Debug.LogWarning($"[{name}] Event raised but no listeners are subscribed.", this);
                return;
            }

            OnEventRaised.Invoke(value);
        }
    }
}
```

---

## 5. State Template (for State Machine)

```csharp
// ============================================================================
// [StateName]State.cs
// Purpose: [State name] state for [entity] state machine
// Unity Version: 6000.3.18f1
// ============================================================================

using UnityEngine;

namespace GameName.Gameplay.Player.States
{
    /// <summary>
    /// Represents the [state description] state for the player.
    /// </summary>
    /// <remarks>
    /// <para><b>Entry conditions:</b> [When this state is entered]</para>
    /// <para><b>Exit conditions:</b> [When this state transitions out]</para>
    /// <para><b>Behavior:</b> [What happens during this state]</para>
    /// </remarks>
    public class IdleState : IState
    {
        #region Private Fields

        private readonly PlayerController _player;
        private readonly Animator _animator;

        private static readonly int AnimHash_Speed = Animator.StringToHash("Speed");

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="IdleState"/> class.
        /// </summary>
        /// <param name="player">The player controller that owns this state.</param>
        /// <param name="animator">The animator component for animation control.</param>
        public IdleState(PlayerController player, Animator animator)
        {
            _player = player;
            _animator = animator;
        }

        #endregion

        #region IState Implementation

        /// <inheritdoc/>
        public void Enter()
        {
            _animator.SetFloat(AnimHash_Speed, 0f);
        }

        /// <inheritdoc/>
        public void Tick()
        {
            // Idle behavior — check for input transitions
        }

        /// <inheritdoc/>
        public void FixedTick()
        {
            // No physics needed in idle state
        }

        /// <inheritdoc/>
        public void Exit()
        {
            // Clean up idle state
        }

        #endregion
    }
}
```

---

## 6. Editor Script Template

```csharp
// ============================================================================
// [ClassName]Editor.cs
// Purpose: Custom Inspector for [component name]
// Unity Version: 6000.3.18f1
// ============================================================================

using UnityEditor;
using UnityEngine;

namespace GameName.Editor.CustomInspectors
{
    /// <summary>
    /// Custom Inspector for <see cref="HealthSystem"/> providing
    /// debug controls and visual feedback in the editor.
    /// </summary>
    [CustomEditor(typeof(HealthSystem))]
    public class HealthSystemEditor : UnityEditor.Editor
    {
        #region Serialized Properties

        private SerializedProperty _maxHealthProp;
        private SerializedProperty _invincibilityDurationProp;

        #endregion

        #region Unity Editor Lifecycle

        private void OnEnable()
        {
            _maxHealthProp = serializedObject.FindProperty("_maxHealth");
            _invincibilityDurationProp = serializedObject.FindProperty("_invincibilityDuration");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Debug Controls", EditorStyles.boldLabel);

            if (Application.isPlaying)
            {
                HealthSystem health = (HealthSystem)target;

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.IntField("Current Health", health.CurrentHealth);
                EditorGUILayout.Toggle("Is Alive", health.IsAlive);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space(5);

                if (GUILayout.Button("Deal 10 Damage"))
                {
                    health.TakeDamage(10);
                }

                if (GUILayout.Button("Heal Full"))
                {
                    health.Heal(health.MaxHealth);
                }

                if (GUILayout.Button("Kill"))
                {
                    health.TakeDamage(health.CurrentHealth);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Debug controls available in Play Mode.", MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}
```

---

## 7. Test Script Template

```csharp
// ============================================================================
// [ClassName]Tests.cs
// Purpose: Unit tests for [system name]
// Unity Version: 6000.3.18f1
// ============================================================================

using NUnit.Framework;
using UnityEngine;

namespace GameName.Tests.EditMode
{
    /// <summary>
    /// Unit tests for <see cref="HealthSystem"/>.
    /// Tests cover damage, healing, death, and edge cases.
    /// </summary>
    [TestFixture]
    public class HealthSystemTests
    {
        #region Setup

        private GameObject _testObject;
        private HealthSystem _healthSystem;

        [SetUp]
        public void SetUp()
        {
            _testObject = new GameObject("TestHealthEntity");
            _healthSystem = _testObject.AddComponent<HealthSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_testObject);
        }

        #endregion

        #region Damage Tests

        [Test]
        public void TakeDamage_WithPositiveAmount_ReducesHealth()
        {
            // Arrange
            int initialHealth = _healthSystem.CurrentHealth;
            int damageAmount = 10;

            // Act
            _healthSystem.TakeDamage(damageAmount);

            // Assert
            Assert.AreEqual(initialHealth - damageAmount, _healthSystem.CurrentHealth);
        }

        [Test]
        public void TakeDamage_ReducingToZero_TriggersDeathEvent()
        {
            // Arrange
            bool deathEventFired = false;
            _healthSystem.OnDied += () => deathEventFired = true;

            // Act
            _healthSystem.TakeDamage(_healthSystem.MaxHealth);

            // Assert
            Assert.IsTrue(deathEventFired);
            Assert.IsFalse(_healthSystem.IsAlive);
        }

        [Test]
        public void TakeDamage_WithZeroAmount_DoesNotReduceHealth()
        {
            // Arrange
            int initialHealth = _healthSystem.CurrentHealth;

            // Act
            _healthSystem.TakeDamage(0);

            // Assert
            Assert.AreEqual(initialHealth, _healthSystem.CurrentHealth);
        }

        #endregion

        #region Heal Tests

        [Test]
        public void Heal_WithPositiveAmount_IncreasesHealth()
        {
            // Arrange
            _healthSystem.TakeDamage(20);
            int healthAfterDamage = _healthSystem.CurrentHealth;

            // Act
            _healthSystem.Heal(10);

            // Assert
            Assert.AreEqual(healthAfterDamage + 10, _healthSystem.CurrentHealth);
        }

        [Test]
        public void Heal_AboveMaxHealth_ClampsToMax()
        {
            // Arrange
            _healthSystem.TakeDamage(10);

            // Act
            _healthSystem.Heal(999);

            // Assert
            Assert.AreEqual(_healthSystem.MaxHealth, _healthSystem.CurrentHealth);
        }

        #endregion
    }
}
```
