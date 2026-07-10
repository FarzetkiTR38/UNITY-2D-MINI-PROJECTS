# Unity C# Coding Guidelines

## Overview

This document defines the mandatory C# coding standards for all Unity 6 scripts. These guidelines ensure consistency, readability, maintainability, and performance across the entire codebase.

---

## 1. Language Features

### 1.1 C# Version

Use modern C# features supported by Unity 6:

```csharp
// ✅ Pattern matching
if (collision.gameObject.TryGetComponent(out IDamageable damageable))
{
    damageable.TakeDamage(damage);
}

// ✅ Null-conditional and null-coalescing
OnHealthChanged?.Invoke(_currentHealth);
string displayName = _playerName ?? "Unknown";
int health = _healthComponent?.CurrentHealth ?? 0;

// ✅ Expression-bodied members
public int MaxHealth => _maxHealth;
public bool IsAlive => _currentHealth > 0;
public override string ToString() => $"{_name} (HP: {_currentHealth}/{_maxHealth})";

// ✅ String interpolation
Debug.Log($"[{GetType().Name}] Player {_playerName} took {damage} damage. HP: {_currentHealth}/{_maxHealth}");

// ✅ Using declarations
using var reader = new StreamReader(path);

// ✅ Switch expressions
string GetDifficultyName(Difficulty difficulty) => difficulty switch
{
    Difficulty.Easy => "Easy",
    Difficulty.Normal => "Normal",
    Difficulty.Hard => "Hard",
    Difficulty.Nightmare => "Nightmare",
    _ => throw new ArgumentOutOfRangeException(nameof(difficulty))
};

// ✅ Target-typed new
private readonly List<Enemy> _activeEnemies = new();
private readonly Dictionary<string, int> _inventory = new();

// ✅ Init-only setters for data objects
public record PlayerScore
{
    public string PlayerName { get; init; }
    public int Score { get; init; }
    public DateTime Timestamp { get; init; }
}
```

### 1.2 Banned Language Features in Hot Paths

```csharp
// ❌ NEVER in Update/FixedUpdate/LateUpdate:
// LINQ queries (causes GC allocation)
var closest = enemies.Where(e => e.IsAlive).OrderBy(e => e.Distance).First();

// ❌ String concatenation (causes GC allocation)
_debugText.text = "HP: " + health.ToString() + "/" + maxHealth.ToString();

// ❌ Lambda captures in hot paths (causes GC allocation)
enemies.ForEach(e => e.TakeDamage(damage));

// ❌ Boxing value types
object boxed = 42; // int boxed to object

// ❌ Params arrays in frequently called methods
void LogValues(params object[] values) { } // allocates array every call
```

---

## 2. Code Organization

### 2.1 File Rules

- **One class per file** (exceptions: nested private classes, small related enums)
- **Filename must match class name exactly**: `PlayerController.cs` contains `class PlayerController`
- **No partial classes** unless required by code generation (e.g., Input System)
- **Maximum file length**: 400 lines preferred, 600 lines hard limit. Refactor if larger.

### 2.2 Using Directives Order

```csharp
// 1. System namespaces
using System;
using System.Collections.Generic;
using System.Threading;

// 2. Unity namespaces
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

// 3. Third-party namespaces
using Unity.Cinemachine;
using TMPro;

// 4. Project namespaces
using ProjectName.Core;
using ProjectName.Gameplay;
using ProjectName.UI;
```

### 2.3 Class Structure Order

Every class MUST follow this internal ordering:

```csharp
public class ExampleClass : MonoBehaviour
{
    // 1. Constants
    #region Constants
    private const float DefaultSpeed = 5f;
    private const int MaxRetries = 3;
    #endregion

    // 2. Static fields
    #region Static Fields
    private static readonly int AnimHash_Speed = Animator.StringToHash("Speed");
    private static readonly int AnimHash_IsGrounded = Animator.StringToHash("IsGrounded");
    #endregion

    // 3. Serialized fields (Inspector-visible)
    #region Serialized Fields
    [Header("Movement")]
    [Tooltip("Maximum movement speed in units per second.")]
    [SerializeField, Min(0f)] private float _moveSpeed = 8f;

    [Tooltip("Force applied when jumping.")]
    [SerializeField, Min(0f)] private float _jumpForce = 12f;

    [Header("References")]
    [Tooltip("Reference to the ground check transform.")]
    [SerializeField] private Transform _groundCheck;

    [Tooltip("Layer mask for ground detection.")]
    [SerializeField] private LayerMask _groundLayer;
    #endregion

    // 4. Private fields (non-serialized)
    #region Private Fields
    private Rigidbody2D _rigidbody2D;
    private Animator _animator;
    private Vector2 _moveInput;
    private bool _isGrounded;
    private bool _jumpRequested;
    #endregion

    // 5. Properties
    #region Properties
    /// <summary>Gets a value indicating whether the player is currently grounded.</summary>
    public bool IsGrounded => _isGrounded;

    /// <summary>Gets the current movement speed.</summary>
    public float CurrentSpeed => _rigidbody2D != null ? _rigidbody2D.linearVelocity.magnitude : 0f;
    #endregion

    // 6. Events
    #region Events
    /// <summary>Raised when the player lands on the ground.</summary>
    public event Action OnLanded;

    /// <summary>Raised when the player jumps.</summary>
    public event Action OnJumped;
    #endregion

    // 7. Unity Lifecycle methods
    #region Unity Lifecycle
    private void Awake() { /* ... */ }
    private void OnEnable() { /* ... */ }
    private void Start() { /* ... */ }
    private void FixedUpdate() { /* ... */ }
    private void Update() { /* ... */ }
    private void LateUpdate() { /* ... */ }
    private void OnDisable() { /* ... */ }
    private void OnDestroy() { /* ... */ }
    private void OnValidate() { /* ... */ }
    #endregion

    // 8. Public methods
    #region Public Methods
    /// <summary>Applies damage to the player.</summary>
    public void TakeDamage(float amount) { /* ... */ }
    #endregion

    // 9. Private/Protected methods
    #region Private Methods
    private void HandleMovement() { /* ... */ }
    private void HandleJump() { /* ... */ }
    private void CheckGround() { /* ... */ }
    #endregion

    // 10. Coroutines / Async methods
    #region Coroutines
    private async Awaitable DashAsync(CancellationToken token) { /* ... */ }
    #endregion
}
```

---

## 3. XML Documentation

### 3.1 Required Documentation

ALL public and protected members MUST have XML documentation:

```csharp
/// <summary>
/// Manages player health, damage intake, healing, and death.
/// Provides events for UI binding and gameplay responses.
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> Central health management for any damageable entity.</para>
/// <para><b>Dependencies:</b> Requires no external dependencies. Optionally raises events
/// consumed by UI and VFX systems.</para>
/// <para><b>Inspector Setup:</b></para>
/// <list type="bullet">
///   <item>Set <c>_maxHealth</c> to the entity's maximum health value.</item>
///   <item>Set <c>_invincibilityDuration</c> for post-damage invincibility frames.</item>
/// </list>
/// <para><b>Performance:</b> No allocations in damage/heal paths. Event invocation is O(n)
/// where n is subscriber count.</para>
/// </remarks>
public class HealthSystem : MonoBehaviour, IDamageable, IHealable
{
    /// <summary>
    /// Applies the specified amount of damage to this entity.
    /// </summary>
    /// <param name="amount">The damage amount. Must be greater than zero.</param>
    /// <param name="source">The GameObject that caused the damage. Can be null.</param>
    /// <returns><c>true</c> if damage was applied; <c>false</c> if the entity is invincible or dead.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="amount"/> is negative.</exception>
    public bool TakeDamage(float amount, GameObject source = null)
    {
        if (amount < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Damage amount cannot be negative.");
        }
        // Implementation...
    }
}
```

### 3.2 Documentation Templates

```csharp
// For events:
/// <summary>
/// Raised when [what triggers it]. Provides [what data].
/// </summary>

// For properties:
/// <summary>Gets [what it returns]. Returns [default] if [edge case].</summary>

// For fields:
/// <summary>The [description]. Configured via Inspector.</summary>

// For enums:
/// <summary>Defines the possible [states/types/modes] for [system].</summary>

// For enum values:
/// <summary>[Description of when this value is used].</summary>
```

---

## 4. Constants and Magic Numbers

### 4.1 Rules

```csharp
// ❌ WRONG: Magic numbers
if (health <= 0) { Die(); }
transform.position += Vector3.right * 5f * Time.deltaTime;
if (_comboCount >= 3) { TriggerSpecialAttack(); }

// ✅ CORRECT: Named constants
private const float MinHealth = 0f;
private const float DefaultMoveSpeed = 5f;
private const int ComboThresholdForSpecial = 3;

if (health <= MinHealth) { Die(); }
transform.position += Vector3.right * DefaultMoveSpeed * Time.deltaTime;
if (_comboCount >= ComboThresholdForSpecial) { TriggerSpecialAttack(); }
```

### 4.2 Constant Categories

```csharp
// Physics constants
private const float GroundCheckRadius = 0.2f;
private const float WallCheckDistance = 0.5f;
private const float CoyoteTimeDuration = 0.15f;
private const float JumpBufferDuration = 0.2f;

// Animation hash caching (static readonly for Animator hashes)
private static readonly int AnimHash_Speed = Animator.StringToHash("Speed");
private static readonly int AnimHash_IsGrounded = Animator.StringToHash("IsGrounded");
private static readonly int AnimHash_Attack = Animator.StringToHash("Attack");
private static readonly int AnimHash_Die = Animator.StringToHash("Die");

// Layer/tag constants
private const string TagPlayer = "Player";
private const string TagEnemy = "Enemy";

// Timing constants
private const float InvincibilityDuration = 1.5f;
private const float RespawnDelay = 2f;
```

---

## 5. Null Safety

### 5.1 Reference Validation

```csharp
// ✅ CORRECT: Validate in Awake
private void Awake()
{
    _rigidbody2D = GetComponent<Rigidbody2D>();
    _animator = GetComponent<Animator>();

    Debug.Assert(_rigidbody2D != null, $"[{name}] Missing Rigidbody2D component.", this);
    Debug.Assert(_animator != null, $"[{name}] Missing Animator component.", this);
}

// ✅ CORRECT: Validate serialized references
private void OnValidate()
{
    if (_groundCheck == null)
    {
        Debug.LogWarning($"[{name}] Ground Check transform is not assigned.", this);
    }

    if (_groundLayer == 0)
    {
        Debug.LogWarning($"[{name}] Ground Layer mask is not set.", this);
    }
}

// ✅ CORRECT: Safe external reference access
public void ApplyEffect(StatusEffect effect)
{
    if (effect == null)
    {
        Debug.LogError($"[{name}] Attempted to apply null StatusEffect.", this);
        return;
    }

    // Safe to use effect
}

// ✅ CORRECT: TryGetComponent pattern
private void OnTriggerEnter2D(Collider2D other)
{
    if (other.TryGetComponent(out IInteractable interactable))
    {
        interactable.Interact(gameObject);
    }
}
```

### 5.2 Argument Validation

```csharp
/// <summary>Deals damage to this entity.</summary>
/// <param name="amount">Damage amount. Must be non-negative.</param>
/// <param name="damageType">The type of damage being dealt.</param>
public void TakeDamage(float amount, DamageType damageType)
{
    if (amount < 0f)
    {
        throw new ArgumentOutOfRangeException(nameof(amount),
            $"Damage amount must be non-negative. Received: {amount}");
    }

    if (!System.Enum.IsDefined(typeof(DamageType), damageType))
    {
        throw new ArgumentOutOfRangeException(nameof(damageType),
            $"Invalid DamageType: {damageType}");
    }

    // Implementation
}
```

---

## 6. Field Accessibility

### 6.1 Rules

```csharp
// ✅ CORRECT: Private field with SerializeField
[SerializeField] private float _moveSpeed = 5f;

// ✅ CORRECT: Public read-only property
public float MoveSpeed => _moveSpeed;

// ✅ CORRECT: Private field, no Inspector exposure needed
private bool _isJumping;

// ✅ CORRECT: Readonly for injected dependencies
private readonly List<IObserver> _observers = new();

// ❌ WRONG: Public field
public float moveSpeed = 5f;

// ❌ WRONG: Public field for Unity serialization
public Transform groundCheck;
```

### 6.2 Property Patterns

```csharp
// Read-only property (most common)
public int CurrentHealth => _currentHealth;

// Property with change notification
private int _currentHealth;
public int CurrentHealth
{
    get => _currentHealth;
    private set
    {
        if (_currentHealth == value) return;
        int previousHealth = _currentHealth;
        _currentHealth = Mathf.Clamp(value, 0, _maxHealth);
        OnHealthChanged?.Invoke(_currentHealth, previousHealth);
    }
}
```

---

## 7. Inspector Attributes

### 7.1 Required Attributes

```csharp
[Header("Movement Settings")]
[Tooltip("Maximum horizontal movement speed in units per second.")]
[SerializeField, Min(0f)]
private float _moveSpeed = 8f;

[Tooltip("Vertical force applied when the player jumps.")]
[SerializeField, Min(0f)]
private float _jumpForce = 14f;

[Tooltip("Duration of coyote time in seconds. Allows jumping briefly after leaving a platform.")]
[SerializeField, Range(0f, 0.5f)]
private float _coyoteTime = 0.12f;

[Space(10)]

[Header("Ground Detection")]
[Tooltip("Transform position used as the center of the ground check circle.")]
[SerializeField]
private Transform _groundCheckPoint;

[Tooltip("Radius of the ground check overlap circle.")]
[SerializeField, Range(0.01f, 1f)]
private float _groundCheckRadius = 0.2f;

[Tooltip("Physics layers considered as ground.")]
[SerializeField]
private LayerMask _groundLayer;

[Space(10)]

[Header("Combat")]
[Tooltip("Base damage dealt per attack.")]
[SerializeField, Min(1)]
private int _attackDamage = 10;

[Tooltip("Seconds of invincibility after taking damage.")]
[SerializeField, Range(0f, 5f)]
private float _invincibilityDuration = 1.5f;

[Space(10)]

[Header("Audio")]
[Tooltip("Sound effect played when the player jumps.")]
[SerializeField]
private AudioClip _jumpSound;

[Tooltip("Sound effect played when the player takes damage.")]
[SerializeField]
private AudioClip _hurtSound;

[Space(10)]

[Header("Debug")]
[Tooltip("Enable to show ground check gizmos in Scene view.")]
[SerializeField]
private bool _showDebugGizmos = true;
```

### 7.2 Context Menu Actions

```csharp
[ContextMenu("Reset Health to Max")]
private void ResetHealthToMax()
{
    _currentHealth = _maxHealth;
    Debug.Log($"[{name}] Health reset to {_maxHealth}.", this);
}

[ContextMenu("Kill (Debug)")]
private void DebugKill()
{
    TakeDamage(_currentHealth);
}

[ContextMenu("Log State")]
private void LogCurrentState()
{
    Debug.Log($"[{name}] State: HP={_currentHealth}/{_maxHealth}, " +
              $"Grounded={_isGrounded}, Velocity={_rigidbody2D.linearVelocity}", this);
}
```

---

## 8. Error Handling

### 8.1 Logging Standards

```csharp
// Information (normal flow)
Debug.Log($"[{GetType().Name}] Player spawned at {transform.position}.", this);

// Warning (unexpected but recoverable)
Debug.LogWarning($"[{GetType().Name}] No spawn point found. Using default position.", this);

// Error (something is wrong, may cause issues)
Debug.LogError($"[{GetType().Name}] Required component {typeof(Rigidbody2D).Name} is missing!", this);

// Assertion (invariant violation, should never happen)
Debug.Assert(_maxHealth > 0, $"[{name}] Max health must be positive.", this);

// ALWAYS pass 'this' as context parameter for clickable logs in Console
```

### 8.2 Error Recovery

```csharp
// Graceful degradation
private void PlaySound(AudioClip clip)
{
    if (clip == null)
    {
        Debug.LogWarning($"[{GetType().Name}] Audio clip is null. Skipping playback.", this);
        return;
    }

    if (_audioSource == null)
    {
        Debug.LogError($"[{GetType().Name}] AudioSource is missing!", this);
        return;
    }

    _audioSource.PlayOneShot(clip);
}
```

---

## 9. Region Usage

### 9.1 Guidelines

- Use regions to organize class sections (Constants, Fields, Properties, Methods)
- Do NOT use regions to hide large blocks of code (refactor instead)
- Do NOT nest regions
- Region names must be descriptive and consistent

```csharp
// ✅ CORRECT region usage
#region Constants
private const float Gravity = -9.81f;
#endregion

#region Serialized Fields
[SerializeField] private float _speed = 5f;
#endregion

// ❌ WRONG: Region hiding complexity
#region 200 Lines Of Spaghetti Code
// ... this should be refactored into separate methods/classes
#endregion
```

---

## 10. Async/Await vs Coroutines

### 10.1 When to Use What

```csharp
// ✅ Use Awaitable (Unity 6) for:
// - Timed sequences
// - Loading operations
// - Cancellable operations
// - Operations that need try/catch
private async Awaitable PerformDashAsync(CancellationToken token)
{
    _isDashing = true;
    float elapsed = 0f;

    try
    {
        while (elapsed < _dashDuration)
        {
            token.ThrowIfCancellationRequested();
            elapsed += Time.deltaTime;
            _rigidbody2D.linearVelocity = _dashDirection * _dashSpeed;
            await Awaitable.NextFrameAsync(token);
        }
    }
    finally
    {
        _isDashing = false;
        _rigidbody2D.linearVelocity = Vector2.zero;
    }
}

// ✅ Use Coroutines for:
// - Simple delays
// - Frame-by-frame sequences that don't need cancellation
// - When you need YieldInstruction caching
private IEnumerator FlashSpriteCoroutine()
{
    for (int i = 0; i < _flashCount; i++)
    {
        _spriteRenderer.color = Color.red;
        yield return _flashWaitForSeconds; // cached WaitForSeconds
        _spriteRenderer.color = Color.white;
        yield return _flashWaitForSeconds;
    }
}

// Cache WaitForSeconds to avoid GC
private readonly WaitForSeconds _flashWaitForSeconds = new(0.1f);
```

---

## 11. Interface Design

### 11.1 Common Interfaces

```csharp
/// <summary>Represents an entity that can receive damage.</summary>
public interface IDamageable
{
    /// <summary>Gets the current health of this entity.</summary>
    int CurrentHealth { get; }

    /// <summary>Gets the maximum health of this entity.</summary>
    int MaxHealth { get; }

    /// <summary>Gets a value indicating whether this entity is alive.</summary>
    bool IsAlive { get; }

    /// <summary>Applies damage to this entity.</summary>
    /// <param name="amount">The damage amount.</param>
    /// <param name="source">The source of the damage.</param>
    /// <returns><c>true</c> if damage was applied successfully.</returns>
    bool TakeDamage(int amount, GameObject source = null);
}

/// <summary>Represents an entity that can be healed.</summary>
public interface IHealable
{
    /// <summary>Heals this entity by the specified amount.</summary>
    /// <param name="amount">The heal amount.</param>
    void Heal(int amount);
}

/// <summary>Represents an object that can be interacted with by the player.</summary>
public interface IInteractable
{
    /// <summary>Gets a value indicating whether this object can currently be interacted with.</summary>
    bool CanInteract { get; }

    /// <summary>Gets the interaction prompt text to display.</summary>
    string InteractionPrompt { get; }

    /// <summary>Performs the interaction.</summary>
    /// <param name="interactor">The GameObject performing the interaction.</param>
    void Interact(GameObject interactor);
}

/// <summary>Represents an item that can be collected/picked up.</summary>
public interface ICollectible
{
    /// <summary>Collects this item.</summary>
    /// <param name="collector">The GameObject collecting this item.</param>
    void Collect(GameObject collector);
}

/// <summary>Represents an entity that can be saved and loaded.</summary>
public interface ISaveable
{
    /// <summary>Gets the unique identifier for this saveable entity.</summary>
    string SaveId { get; }

    /// <summary>Captures the current state as serializable data.</summary>
    object CaptureState();

    /// <summary>Restores state from previously captured data.</summary>
    /// <param name="state">The state data to restore.</param>
    void RestoreState(object state);
}

/// <summary>Represents an entity with a poolable lifecycle.</summary>
public interface IPoolable
{
    /// <summary>Called when the object is retrieved from the pool.</summary>
    void OnGetFromPool();

    /// <summary>Called when the object is returned to the pool.</summary>
    void OnReturnToPool();
}

/// <summary>Represents a state in a finite state machine.</summary>
public interface IState
{
    /// <summary>Called when entering this state.</summary>
    void Enter();

    /// <summary>Called every frame while in this state.</summary>
    void Tick();

    /// <summary>Called at fixed intervals while in this state (physics).</summary>
    void FixedTick();

    /// <summary>Called when exiting this state.</summary>
    void Exit();
}
```

---

## 12. Enum Design

### 12.1 Rules

```csharp
/// <summary>Defines the possible states for a player character.</summary>
public enum PlayerState
{
    /// <summary>Player is standing still on the ground.</summary>
    Idle = 0,

    /// <summary>Player is running on the ground.</summary>
    Running = 1,

    /// <summary>Player is in the air, moving upward.</summary>
    Jumping = 2,

    /// <summary>Player is in the air, moving downward.</summary>
    Falling = 3,

    /// <summary>Player is performing a wall slide.</summary>
    WallSliding = 4,

    /// <summary>Player is performing a dash.</summary>
    Dashing = 5,

    /// <summary>Player is performing an attack.</summary>
    Attacking = 6,

    /// <summary>Player has been hit and is in hurt state.</summary>
    Hurt = 7,

    /// <summary>Player is dead.</summary>
    Dead = 8
}

// ✅ Rules:
// - Always assign explicit integer values
// - Always document each value
// - Start from 0
// - Use [Flags] attribute only when values can be combined
// - Prefer specific names over generic ones

/// <summary>Defines damage types that can modify damage calculation.</summary>
[Flags]
public enum DamageType
{
    /// <summary>No special damage type.</summary>
    None = 0,

    /// <summary>Physical melee damage.</summary>
    Physical = 1 << 0,

    /// <summary>Fire-based damage.</summary>
    Fire = 1 << 1,

    /// <summary>Ice-based damage.</summary>
    Ice = 1 << 2,

    /// <summary>Electrical damage.</summary>
    Electric = 1 << 3,

    /// <summary>Poison damage over time.</summary>
    Poison = 1 << 4
}
```
