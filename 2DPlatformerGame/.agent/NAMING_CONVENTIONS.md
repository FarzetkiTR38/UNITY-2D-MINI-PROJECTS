# Naming Conventions

## Overview

This document defines the mandatory naming rules for all code elements, assets, and project artifacts. Consistent naming improves readability, searchability, and collaboration across teams and AI agents.

All naming follows **Microsoft C# Naming Guidelines** adapted for Unity development.

---

## 1. C# Code Naming

### 1.1 General Rules

| Element | Convention | Example |
|---------|-----------|---------|
| Namespace | PascalCase, dot-separated | `GameName.Gameplay.Player` |
| Class | PascalCase, noun | `PlayerController`, `HealthSystem` |
| Abstract Class | PascalCase, descriptive | `EnemyBase`, `UIScreenBase` |
| Interface | PascalCase, "I" prefix | `IDamageable`, `IInteractable` |
| Struct | PascalCase, noun | `DamageInfo`, `GridPosition` |
| Enum | PascalCase, singular noun | `PlayerState`, `DamageType` |
| Enum Value | PascalCase | `PlayerState.Idle`, `DamageType.Fire` |
| Method | PascalCase, verb phrase | `TakeDamage()`, `CalculateScore()` |
| Property | PascalCase, noun/adjective | `CurrentHealth`, `IsAlive` |
| Public Field | PascalCase (avoid; prefer property) | `MaxHealth` |
| Private Field | camelCase, underscore prefix | `_currentHealth`, `_moveSpeed` |
| Private Static Field | camelCase, underscore prefix | `_instanceCount` |
| Private Static Readonly | PascalCase or camelCase with prefix | `_animHash_Speed` |
| Constant (const) | PascalCase | `MaxRetries`, `DefaultSpeed` |
| Parameter | camelCase | `damageAmount`, `targetPosition` |
| Local Variable | camelCase | `closestEnemy`, `moveDirection` |
| Event (C#) | PascalCase, "On" prefix | `OnHealthChanged`, `OnDied` |
| Delegate | PascalCase, descriptive | `HealthChangedHandler` |
| Generic Type Parameter | Single uppercase letter or "T" prefix | `T`, `TKey`, `TValue` |

### 1.2 Private Field Naming

```csharp
// ✅ CORRECT: Underscore prefix for all private fields
private float _moveSpeed;
private int _currentHealth;
private bool _isGrounded;
private Rigidbody2D _rigidbody2D;
private Animator _animator;
private SpriteRenderer _spriteRenderer;
private Transform _groundCheck;
private readonly List<IObserver> _observers = new();

// ✅ CORRECT: SerializeField private fields
[SerializeField] private float _jumpForce = 12f;
[SerializeField] private Transform _firePoint;
[SerializeField] private LayerMask _groundLayer;

// ❌ WRONG: No prefix
private float moveSpeed;
private int health;

// ❌ WRONG: m_ prefix (outdated convention)
private float m_moveSpeed;

// ❌ WRONG: Public field
public float moveSpeed;
public Transform groundCheck;
```

### 1.3 Boolean Naming

```csharp
// ✅ CORRECT: Boolean names express state with is/has/can/should prefixes
private bool _isGrounded;
private bool _isJumping;
private bool _isDashing;
private bool _isInvincible;
private bool _hasDoubleJump;
private bool _hasKey;
private bool _canAttack;
private bool _canInteract;
private bool _shouldFlip;

// Property versions
public bool IsAlive => _currentHealth > 0;
public bool IsGrounded => _isGrounded;
public bool CanDoubleJump => _hasDoubleJump && !_usedDoubleJump;

// ❌ WRONG: Ambiguous boolean names
private bool grounded;      // Use _isGrounded
private bool jump;           // Use _isJumping or _jumpRequested
private bool dead;           // Use _isDead
private bool attack;         // Use _isAttacking or _attackRequested
```

### 1.4 Method Naming

```csharp
// ✅ CORRECT: Verb phrases that clearly describe the action
public void TakeDamage(int amount) { }
public void Heal(int amount) { }
public bool TryInteract(GameObject interactor) { }
public void SetMoveDirection(Vector2 direction) { }
public void EnableInvincibility(float duration) { }
public void ResetToDefault() { }

// Getters (when not a property)
public List<Item> GetInventoryItems() { }
public Enemy FindClosestEnemy() { }

// Checkers
public bool IsInRange(Vector2 position, float range) { }
public bool HasEnoughCurrency(int cost) { }

// Event handlers — "Handle" or "On" prefix
private void HandlePlayerDied() { }
private void HandleEnemySpawned(Transform enemy) { }
private void OnJumpPerformed(InputAction.CallbackContext context) { }

// Coroutines/Async — suffix indicates async nature
private IEnumerator FlashSpriteCoroutine() { }
private async Awaitable FadeOutAsync(CancellationToken token) { }
private async Awaitable LoadLevelAsync(string levelName) { }

// ❌ WRONG: Ambiguous or non-descriptive
public void DoStuff() { }
public void Process() { }
public void Execute() { }     // Only for Command pattern
public void Manager() { }     // Not a verb
```

### 1.5 Event Naming

```csharp
// C# Events: "On" + PastTense/Adjective
public event Action OnDied;
public event Action<int> OnDamaged;
public event Action<int, int> OnHealthChanged;
public event Action OnLanded;
public event Action<Item> OnItemCollected;
public event Action<float> OnManaConsumed;

// ScriptableObject Event Channels: Descriptive verb phrase
// Asset names: OnPlayerDamaged.asset, OnEnemyKilled.asset, OnLevelCompleted.asset

// Delegate types: Descriptive + "Handler" suffix
public delegate void HealthChangedHandler(int currentHealth, int maxHealth);
public delegate void DamageHandler(DamageInfo info);
```

---

## 2. Unity Component Naming

### 2.1 MonoBehaviour Classes

```
Pattern: [Entity][System/Aspect]

✅ CORRECT:
PlayerController        — Main player orchestrator
PlayerMovement          — Player movement logic
PlayerCombat            — Player combat logic
PlayerAnimation         — Player animation control
PlayerInputHandler      — Player input processing
EnemyPatrol             — Enemy patrol behavior
EnemyChase              — Enemy chase behavior
HealthSystem            — Health management
DamageDealer            — Applies damage on contact
InteractionDetector     — Detects interactable objects
CameraManager           — Camera system controller
AudioManager            — Audio playback system
UIManager               — UI navigation and display

❌ WRONG:
PlayerScript            — Too vague
EnemyScript             — Too vague
Manager                 — What does it manage?
Controller              — What does it control?
Helper                  — Not descriptive
Utility                 — Not a component name
```

### 2.2 ScriptableObject Classes

```
Pattern: [DataType]Data, [Config]Config, [SystemName]Settings

✅ CORRECT:
EnemyData               — Enemy stats and configuration
WeaponData              — Weapon stats and behavior config
ItemData                — Item definition
LevelData               — Level configuration
PlayerConfig            — Player tuning parameters
GameConfig              — Global game settings
AudioConfig             — Audio system configuration
VoidEventChannel        — ScriptableObject event with no data
IntEventChannel         — ScriptableObject event with int data

❌ WRONG:
EnemySO                 — Abbreviation is unclear
EnemyScriptableObject   — Too verbose
EnemyInfo               — Ambiguous (is it runtime or design-time?)
```

---

## 3. Unity Asset Naming

### 3.1 GameObjects (In-Scene and Prefabs)

| Type | Convention | Example |
|------|-----------|---------|
| Player | Descriptive, PascalCase | `Player` |
| Enemy | Type name | `Slime`, `Skeleton`, `Boss_Dragon` |
| NPC | Role name | `Shopkeeper`, `QuestGiver_01` |
| Environment | Type + number | `Platform_Moving_01`, `Spike_01` |
| Pickup | Item name | `Coin_01`, `HealthPotion_01` |
| UI Canvas | `Canvas_` prefix | `Canvas_HUD`, `Canvas_Menus` |
| Manager | Suffix `Manager` | `GameManager`, `AudioManager` |
| Separator | `--- NAME ---` | `--- ENVIRONMENT ---` |
| Spawn Point | `SpawnPoint_` prefix | `SpawnPoint_Player`, `SpawnPoint_Enemy_01` |
| Waypoint | `Waypoint_` prefix | `Waypoint_Patrol_01` |
| Trigger | `Trigger_` prefix | `Trigger_BossFight`, `Trigger_Cutscene` |
| Camera | Descriptive | `MainCamera`, `CinemachineCamera_Follow` |

### 3.2 Layers

```
Layers:
├── Default         (0)
├── TransparentFX   (1)
├── Ignore Raycast  (2)
├── Water           (4)
├── UI              (5)
├── Ground          (6)  ← Custom
├── Player          (7)  ← Custom
├── Enemy           (8)  ← Custom
├── Projectile      (9)  ← Custom
├── Interactable    (10) ← Custom
├── Platform        (11) ← Custom
├── Hazard          (12) ← Custom
└── Trigger         (13) ← Custom
```

### 3.3 Tags

```
Tags:
├── Player
├── Enemy
├── NPC
├── Checkpoint
├── Respawn
├── MainCamera
├── GameController
├── Finish
└── (Minimize tag usage — prefer TryGetComponent with interfaces)
```

### 3.4 Sorting Layers

```
Sorting Layers (back to front):
├── Background          (-100)
├── BackgroundDetail    (-90)
├── Midground           (-50)
├── Environment         (0)
├── EnvironmentDetail   (10)
├── Interactable        (20)
├── Enemy               (30)
├── Player              (40)
├── Foreground          (50)
├── ForegroundDetail    (60)
├── Particles           (70)
├── UI_World            (80)
```

---

## 4. Animation Parameter Naming

```csharp
// Always cache animation hashes as static readonly
private static readonly int AnimHash_Speed = Animator.StringToHash("Speed");
private static readonly int AnimHash_IsGrounded = Animator.StringToHash("IsGrounded");
private static readonly int AnimHash_IsJumping = Animator.StringToHash("IsJumping");
private static readonly int AnimHash_IsFalling = Animator.StringToHash("IsFalling");
private static readonly int AnimHash_Attack = Animator.StringToHash("Attack");
private static readonly int AnimHash_Hurt = Animator.StringToHash("Hurt");
private static readonly int AnimHash_Die = Animator.StringToHash("Die");
private static readonly int AnimHash_VelocityX = Animator.StringToHash("VelocityX");
private static readonly int AnimHash_VelocityY = Animator.StringToHash("VelocityY");

// Parameter naming in Animator Controller:
// Float:   Speed, VelocityX, VelocityY
// Bool:    IsGrounded, IsJumping, IsFalling, IsAttacking, IsDashing
// Trigger: Attack, Hurt, Die, Jump, Dash
// Int:     AttackCombo (for combo attack index)
```

---

## 5. Namespace Naming

```csharp
// Root namespace: ProjectName or GameName
namespace GameName { }

// Sub-namespaces by layer
namespace GameName.Core { }
namespace GameName.Core.Events { }
namespace GameName.Core.Interfaces { }
namespace GameName.Core.Patterns { }
namespace GameName.Core.Extensions { }
namespace GameName.Core.Utilities { }

namespace GameName.Gameplay { }
namespace GameName.Gameplay.Player { }
namespace GameName.Gameplay.Enemies { }
namespace GameName.Gameplay.Combat { }
namespace GameName.Gameplay.Items { }
namespace GameName.Gameplay.Environment { }
namespace GameName.Gameplay.Progression { }

namespace GameName.Systems { }
namespace GameName.Systems.Audio { }
namespace GameName.Systems.Save { }
namespace GameName.Systems.Scene { }
namespace GameName.Systems.Input { }
namespace GameName.Systems.Dialogue { }
namespace GameName.Systems.Quest { }
namespace GameName.Systems.Pooling { }
namespace GameName.Systems.Localization { }

namespace GameName.UI { }
namespace GameName.UI.Screens { }
namespace GameName.UI.Components { }
namespace GameName.UI.Base { }

namespace GameName.Editor { }
namespace GameName.Editor.CustomInspectors { }
namespace GameName.Editor.Tools { }
```

---

## 6. File Naming

| File Type | Convention | Example |
|-----------|-----------|---------|
| C# Script | PascalCase, matches class name | `PlayerController.cs` |
| Interface | PascalCase, "I" prefix | `IDamageable.cs` |
| Enum (standalone) | PascalCase | `PlayerState.cs` |
| ScriptableObject Definition | PascalCase + "Data"/"Config" | `EnemyData.cs` |
| Editor Script | PascalCase + "Editor" suffix | `HealthSystemEditor.cs` |
| Property Drawer | PascalCase + "Drawer" suffix | `ReadOnlyDrawer.cs` |
| Test Script | PascalCase + "Tests" suffix | `HealthSystemTests.cs` |
| Assembly Definition | PascalCase, dot-separated | `GameName.Runtime.Core.asmdef` |
| Input Actions | PascalCase | `GameInputActions.inputactions` |

---

## 7. Quick Reference Table

| Element | Naming | Prefix/Suffix | Example |
|---------|--------|---------------|---------|
| Private field | _camelCase | `_` prefix | `_health` |
| Serialized field | _camelCase | `_` prefix | `_moveSpeed` |
| Constant | PascalCase | None | `MaxHealth` |
| Static readonly | PascalCase or _camelCase | `_` prefix optional | `AnimHash_Speed` |
| Bool field | _camelCase | `_is`, `_has`, `_can` | `_isGrounded` |
| Bool property | PascalCase | `Is`, `Has`, `Can` | `IsAlive` |
| Event | PascalCase | `On` prefix | `OnDied` |
| Event handler | PascalCase | `Handle` prefix | `HandleDamage` |
| Async method | PascalCase | `Async` suffix | `LoadAsync` |
| Coroutine | PascalCase | `Coroutine` suffix | `FlashCoroutine` |
| Interface | PascalCase | `I` prefix | `IDamageable` |
| Abstract class | PascalCase | `Base` suffix | `EnemyBase` |
| Test method | PascalCase | None | `TakeDamage_WithZero_DoesNothing` |
