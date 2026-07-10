# Architecture Guide

## Overview

This document defines the architectural patterns, design principles, and system organization rules for Unity 6 2D game projects. The architecture prioritizes scalability, testability, maintainability, and performance.

---

## 1. Core Principles

### 1.1 SOLID Principles

| Principle | Application in Unity |
|-----------|---------------------|
| **S**ingle Responsibility | Each MonoBehaviour handles ONE concern. `PlayerMovement` moves, `PlayerCombat` fights, `PlayerAnimation` animates. |
| **O**pen/Closed | Use ScriptableObjects for configuration. Extend behavior via composition, not modification. |
| **L**iskov Substitution | All `IDamageable` implementations must behave consistently. Base class contracts are inviolable. |
| **I**nterface Segregation | Use focused interfaces: `IDamageable`, `IHealable`, `IInteractable` — not one `IEntity` mega-interface. |
| **D**ependency Inversion | Depend on abstractions (`IDamageable`) not concrete types (`PlayerHealth`). Use `[SerializeField]` or DI for injection. |

### 1.2 Additional Principles

| Principle | Rule |
|-----------|------|
| **DRY** | Extract repeated logic into utility classes, extension methods, or base components. |
| **KISS** | Start simple. Add complexity only when justified by a real requirement. |
| **Composition Over Inheritance** | Use multiple focused components on a GameObject instead of deep class hierarchies. |
| **Data-Driven Design** | Use ScriptableObjects for all tuneable values. Designers should never need to touch code. |
| **Event-Driven Communication** | Systems communicate through events, not direct references. Minimize coupling. |

---

## 2. Recommended Architecture: ScriptableObject Event-Driven Architecture

### 2.1 Overview

The recommended architecture for Unity 2D projects combines:
- **ScriptableObject Architecture** (Ryan Hipple, Unite 2017)
- **Event Channel Pattern** (decoupled communication)
- **Service Locator** (lightweight dependency access)
- **State Machine** (complex behavior management)
- **Command Pattern** (reversible actions)
- **Object Pool** (performance-critical allocation avoidance)

### 2.2 Layer Diagram

```
┌─────────────────────────────────────────────────────┐
│                    UI LAYER                          │
│  Screens, HUD, Menus, Popups                        │
│  Listens to events, updates visuals                 │
│  NEVER contains game logic                          │
├─────────────────────────────────────────────────────┤
│                 SYSTEMS LAYER                        │
│  AudioManager, SaveManager, SceneLoader,            │
│  InputManager, DialogueManager, QuestManager        │
│  Cross-cutting concerns, infrastructure services    │
├─────────────────────────────────────────────────────┤
│                GAMEPLAY LAYER                        │
│  Player, Enemies, Combat, Items, Environment        │
│  Game mechanics, entity behaviors                   │
│  Uses Core interfaces and events                    │
├─────────────────────────────────────────────────────┤
│                  CORE LAYER                          │
│  Interfaces, Events, Patterns, Utilities            │
│  NO dependencies on upper layers                    │
│  Pure abstractions and shared infrastructure        │
├─────────────────────────────────────────────────────┤
│             SCRIPTABLEOBJECT DATA                    │
│  Configs, Stats, Item Definitions, Event Channels   │
│  Design-time data that drives all systems           │
└─────────────────────────────────────────────────────┘
```

### 2.3 Communication Flow

```
┌──────────┐    Event     ┌──────────┐    Event     ┌──────────┐
│  Player  │──Channel───→│  Audio   │              │   UI     │
│ Combat   │              │ Manager  │              │  HUD     │
└──────────┘              └──────────┘              └──────────┘
     │                                                   ↑
     │              ┌──────────────┐                     │
     └──Event───→│ OnEnemyKilled │──Event Channel──────┘
       Channel      │  (SO Asset)  │
                    └──────────────┘

Systems do NOT know about each other.
They communicate through ScriptableObject Event Channels.
```

---

## 3. Design Patterns

### 3.1 Event Channel Pattern (ScriptableObject Events)

The primary communication mechanism between decoupled systems.

```csharp
// ============================================================================
// VoidEventChannel.cs
// Purpose: ScriptableObject-based event channel with no parameters
// ============================================================================
using System;
using UnityEngine;

namespace GameName.Core.Events
{
    /// <summary>
    /// ScriptableObject-based event channel that carries no data.
    /// Used for fire-and-forget notifications between decoupled systems.
    /// </summary>
    /// <remarks>
    /// <para><b>Usage:</b> Create as asset via Assets → Create → Events → Void Event Channel.
    /// Assign to both the raiser and listener via Inspector.</para>
    /// <para><b>Example:</b> OnGamePaused, OnPlayerDied, OnLevelCompleted</para>
    /// </remarks>
    [CreateAssetMenu(fileName = "NewVoidEventChannel", menuName = "Events/Void Event Channel")]
    public class VoidEventChannel : ScriptableObject
    {
        /// <summary>Event raised when this channel is invoked.</summary>
        public event Action OnEventRaised;

        /// <summary>Raises the event, notifying all listeners.</summary>
        public void RaiseEvent()
        {
            OnEventRaised?.Invoke();
        }
    }

    /// <summary>
    /// ScriptableObject-based event channel that carries a single typed parameter.
    /// </summary>
    /// <typeparam name="T">The type of data this event carries.</typeparam>
    public abstract class EventChannel<T> : ScriptableObject
    {
        /// <summary>Event raised when this channel is invoked with data.</summary>
        public event Action<T> OnEventRaised;

        /// <summary>Raises the event with the specified data.</summary>
        /// <param name="value">The data to pass to listeners.</param>
        public void RaiseEvent(T value)
        {
            OnEventRaised?.Invoke(value);
        }
    }
}
```

```csharp
// Typed event channels
[CreateAssetMenu(fileName = "NewIntEventChannel", menuName = "Events/Int Event Channel")]
public class IntEventChannel : EventChannel<int> { }

[CreateAssetMenu(fileName = "NewFloatEventChannel", menuName = "Events/Float Event Channel")]
public class FloatEventChannel : EventChannel<float> { }

[CreateAssetMenu(fileName = "NewStringEventChannel", menuName = "Events/String Event Channel")]
public class StringEventChannel : EventChannel<string> { }

[CreateAssetMenu(fileName = "NewTransformEventChannel", menuName = "Events/Transform Event Channel")]
public class TransformEventChannel : EventChannel<Transform> { }
```

**Usage:**

```csharp
// RAISER: Combat system raises event when enemy dies
public class EnemyCombat : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private IntEventChannel _onEnemyKilled;

    private void Die()
    {
        _onEnemyKilled.RaiseEvent(_experienceValue);
    }
}

// LISTENER: UI listens to update score display
public class ScoreUI : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private IntEventChannel _onEnemyKilled;

    private void OnEnable() => _onEnemyKilled.OnEventRaised += HandleEnemyKilled;
    private void OnDisable() => _onEnemyKilled.OnEventRaised -= HandleEnemyKilled;

    private void HandleEnemyKilled(int expValue)
    {
        UpdateScoreDisplay();
    }
}
```

### 3.2 State Machine Pattern

For managing complex entity behaviors (Player states, Enemy AI, Game flow).

```csharp
// ============================================================================
// StateMachine.cs
// Purpose: Generic finite state machine for entity behavior management
// ============================================================================
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameName.Core.Patterns
{
    /// <summary>
    /// Generic finite state machine that manages state transitions.
    /// </summary>
    /// <remarks>
    /// <para><b>Usage:</b> Create states implementing IState. Register transitions.
    /// Call Tick() from Update() and FixedTick() from FixedUpdate().</para>
    /// </remarks>
    public class StateMachine
    {
        #region Private Fields

        private IState _currentState;
        private readonly Dictionary<Type, List<Transition>> _transitions = new();
        private readonly List<Transition> _anyTransitions = new();
        private List<Transition> _currentTransitions = new();
        private static readonly List<Transition> EmptyTransitions = new(0);

        #endregion

        #region Properties

        /// <summary>Gets the currently active state.</summary>
        public IState CurrentState => _currentState;

        #endregion

        #region Public Methods

        /// <summary>Updates the state machine. Call from Update().</summary>
        public void Tick()
        {
            Transition transition = GetTriggeredTransition();
            if (transition != null)
            {
                SetState(transition.To);
            }

            _currentState?.Tick();
        }

        /// <summary>Fixed updates the state machine. Call from FixedUpdate().</summary>
        public void FixedTick()
        {
            _currentState?.FixedTick();
        }

        /// <summary>Sets the active state, calling Exit on the old and Enter on the new.</summary>
        /// <param name="state">The state to transition to.</param>
        public void SetState(IState state)
        {
            if (_currentState == state) return;

            _currentState?.Exit();
            _currentState = state;

            _transitions.TryGetValue(_currentState.GetType(), out var transitions);
            _currentTransitions = transitions ?? EmptyTransitions;

            _currentState.Enter();
        }

        /// <summary>Adds a conditional transition between two states.</summary>
        /// <param name="from">The source state.</param>
        /// <param name="to">The destination state.</param>
        /// <param name="condition">The condition that triggers the transition.</param>
        public void AddTransition(IState from, IState to, Func<bool> condition)
        {
            if (!_transitions.TryGetValue(from.GetType(), out var transitions))
            {
                transitions = new List<Transition>();
                _transitions[from.GetType()] = transitions;
            }

            transitions.Add(new Transition(to, condition));
        }

        /// <summary>Adds a transition that can trigger from any state.</summary>
        /// <param name="to">The destination state.</param>
        /// <param name="condition">The condition that triggers the transition.</param>
        public void AddAnyTransition(IState to, Func<bool> condition)
        {
            _anyTransitions.Add(new Transition(to, condition));
        }

        #endregion

        #region Private Methods

        private Transition GetTriggeredTransition()
        {
            foreach (var transition in _anyTransitions)
            {
                if (transition.Condition()) return transition;
            }

            foreach (var transition in _currentTransitions)
            {
                if (transition.Condition()) return transition;
            }

            return null;
        }

        #endregion

        #region Nested Types

        private sealed class Transition
        {
            public IState To { get; }
            public Func<bool> Condition { get; }

            public Transition(IState to, Func<bool> condition)
            {
                To = to;
                Condition = condition;
            }
        }

        #endregion
    }
}
```

### 3.3 Service Locator Pattern

Lightweight alternative to full DI containers. Use when you need global access to services without Singleton anti-patterns.

```csharp
// ============================================================================
// ServiceLocator.cs
// Purpose: Lightweight service registry for global service access
// ============================================================================
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameName.Core.Patterns
{
    /// <summary>
    /// Provides a centralized registry for service instances.
    /// Preferred over Singletons for testability and flexibility.
    /// </summary>
    /// <remarks>
    /// <para><b>Usage:</b></para>
    /// <para>Register: <c>ServiceLocator.Register&lt;IAudioService&gt;(audioManager);</c></para>
    /// <para>Resolve: <c>var audio = ServiceLocator.Get&lt;IAudioService&gt;();</c></para>
    /// <para><b>When to use:</b> For cross-cutting services (Audio, Save, Input) that many
    /// systems need access to. Prefer [SerializeField] injection for gameplay components.</para>
    /// </remarks>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> Services = new();

        /// <summary>Registers a service instance for the given interface type.</summary>
        /// <typeparam name="T">The service interface type.</typeparam>
        /// <param name="service">The service implementation instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when service is null.</exception>
        public static void Register<T>(T service) where T : class
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            Type type = typeof(T);

            if (Services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Overwriting existing service: {type.Name}");
            }

            Services[type] = service;
        }

        /// <summary>Retrieves the registered service for the given interface type.</summary>
        /// <typeparam name="T">The service interface type.</typeparam>
        /// <returns>The registered service instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no service is registered.</exception>
        public static T Get<T>() where T : class
        {
            Type type = typeof(T);

            if (Services.TryGetValue(type, out object service))
            {
                return (T)service;
            }

            throw new InvalidOperationException(
                $"[ServiceLocator] Service not registered: {type.Name}. " +
                $"Ensure it is registered in the Bootstrap scene before access.");
        }

        /// <summary>Attempts to retrieve a registered service.</summary>
        /// <typeparam name="T">The service interface type.</typeparam>
        /// <param name="service">The service instance, or null if not registered.</param>
        /// <returns><c>true</c> if the service was found.</returns>
        public static bool TryGet<T>(out T service) where T : class
        {
            Type type = typeof(T);

            if (Services.TryGetValue(type, out object obj))
            {
                service = (T)obj;
                return true;
            }

            service = null;
            return false;
        }

        /// <summary>Removes a registered service.</summary>
        /// <typeparam name="T">The service interface type.</typeparam>
        public static void Unregister<T>() where T : class
        {
            Services.Remove(typeof(T));
        }

        /// <summary>Removes all registered services. Call during scene cleanup or testing.</summary>
        public static void Clear()
        {
            Services.Clear();
        }
    }
}
```

### 3.4 Command Pattern

For reversible actions (undo/redo), input buffering, and action replay.

```csharp
// ============================================================================
// ICommand.cs
// Purpose: Command pattern interface for reversible actions
// ============================================================================
namespace GameName.Core.Patterns
{
    /// <summary>
    /// Represents a reversible action that can be executed, undone, and redone.
    /// </summary>
    public interface ICommand
    {
        /// <summary>Executes the command.</summary>
        void Execute();

        /// <summary>Reverses the command's effect.</summary>
        void Undo();
    }
}
```

### 3.5 Observer Pattern (C# Events)

For direct observer relationships within the same assembly.

```csharp
// Use C# events for tight coupling within a system
public class HealthSystem : MonoBehaviour
{
    /// <summary>Raised when health changes. Parameters: (currentHealth, maxHealth).</summary>
    public event Action<int, int> OnHealthChanged;

    /// <summary>Raised when the entity dies.</summary>
    public event Action OnDied;

    /// <summary>Raised when the entity takes damage. Parameter: damageAmount.</summary>
    public event Action<int> OnDamaged;

    /// <summary>Raised when the entity is healed. Parameter: healAmount.</summary>
    public event Action<int> OnHealed;
}

// Use ScriptableObject Event Channels for cross-system communication
// (see Event Channel Pattern above)
```

### 3.6 Factory Pattern

For creating configured instances of entities.

```csharp
// ============================================================================
// EnemyFactory.cs
// Purpose: Creates and configures enemy instances from data assets
// ============================================================================
using UnityEngine;
using UnityEngine.Pool;

namespace GameName.Gameplay.Enemies
{
    /// <summary>
    /// Factory for creating enemy instances from ScriptableObject data.
    /// Integrates with object pooling for performance.
    /// </summary>
    public class EnemyFactory : MonoBehaviour
    {
        [Header("Enemy Prefabs")]
        [Tooltip("Database of all available enemy types and their prefabs.")]
        [SerializeField] private EnemyDatabase _enemyDatabase;

        private readonly Dictionary<EnemyType, ObjectPool<GameObject>> _pools = new();

        /// <summary>Creates or retrieves a pooled enemy of the specified type.</summary>
        /// <param name="enemyType">The type of enemy to create.</param>
        /// <param name="position">The spawn position.</param>
        /// <param name="rotation">The spawn rotation.</param>
        /// <returns>The configured enemy GameObject.</returns>
        public GameObject Create(EnemyType enemyType, Vector3 position, Quaternion rotation)
        {
            if (!_pools.TryGetValue(enemyType, out var pool))
            {
                EnemyData data = _enemyDatabase.GetEnemyData(enemyType);
                pool = CreatePool(data);
                _pools[enemyType] = pool;
            }

            GameObject enemy = pool.Get();
            enemy.transform.SetPositionAndRotation(position, rotation);
            return enemy;
        }

        /// <summary>Returns an enemy to its pool.</summary>
        /// <param name="enemy">The enemy GameObject to return.</param>
        /// <param name="enemyType">The type of enemy being returned.</param>
        public void Return(GameObject enemy, EnemyType enemyType)
        {
            if (_pools.TryGetValue(enemyType, out var pool))
            {
                pool.Release(enemy);
            }
            else
            {
                Debug.LogWarning($"[EnemyFactory] No pool for type {enemyType}. Destroying.", this);
                Destroy(enemy);
            }
        }

        private ObjectPool<GameObject> CreatePool(EnemyData data)
        {
            return new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    GameObject instance = Instantiate(data.Prefab);
                    instance.SetActive(false);
                    return instance;
                },
                actionOnGet: obj => obj.SetActive(true),
                actionOnRelease: obj => obj.SetActive(false),
                actionOnDestroy: obj => Destroy(obj),
                collectionCheck: false,
                defaultCapacity: data.DefaultPoolSize,
                maxSize: data.MaxPoolSize
            );
        }
    }
}
```

### 3.7 Strategy Pattern

For interchangeable behaviors (movement strategies, attack types, AI behaviors).

```csharp
// Interface
public interface IMovementStrategy
{
    /// <summary>Calculates the movement vector for this frame.</summary>
    Vector2 CalculateMovement(Transform entity, Transform target);
}

// Implementations
public class PatrolMovement : IMovementStrategy
{
    private readonly Vector2[] _waypoints;
    private int _currentIndex;

    public PatrolMovement(Vector2[] waypoints)
    {
        _waypoints = waypoints;
    }

    public Vector2 CalculateMovement(Transform entity, Transform target)
    {
        Vector2 direction = (_waypoints[_currentIndex] - (Vector2)entity.position).normalized;

        if (Vector2.Distance(entity.position, _waypoints[_currentIndex]) < 0.1f)
        {
            _currentIndex = (_currentIndex + 1) % _waypoints.Length;
        }

        return direction;
    }
}

public class ChaseMovement : IMovementStrategy
{
    public Vector2 CalculateMovement(Transform entity, Transform target)
    {
        if (target == null) return Vector2.zero;
        return ((Vector2)target.position - (Vector2)entity.position).normalized;
    }
}

public class FleeMovement : IMovementStrategy
{
    public Vector2 CalculateMovement(Transform entity, Transform target)
    {
        if (target == null) return Vector2.zero;
        return ((Vector2)entity.position - (Vector2)target.position).normalized;
    }
}
```

### 3.8 Decorator Pattern

For stacking modifiers (buffs, debuffs, damage modifiers).

```csharp
// Base interface
public interface IDamageCalculator
{
    /// <summary>Calculates the final damage after modifications.</summary>
    int Calculate(int baseDamage);
}

// Base implementation
public class BaseDamageCalculator : IDamageCalculator
{
    public int Calculate(int baseDamage) => baseDamage;
}

// Decorators
public class CriticalHitDecorator : IDamageCalculator
{
    private readonly IDamageCalculator _inner;
    private readonly float _critMultiplier;
    private readonly float _critChance;

    public CriticalHitDecorator(IDamageCalculator inner, float critMultiplier, float critChance)
    {
        _inner = inner;
        _critMultiplier = critMultiplier;
        _critChance = critChance;
    }

    public int Calculate(int baseDamage)
    {
        int damage = _inner.Calculate(baseDamage);
        bool isCrit = UnityEngine.Random.value < _critChance;
        return isCrit ? Mathf.RoundToInt(damage * _critMultiplier) : damage;
    }
}

public class ArmorReductionDecorator : IDamageCalculator
{
    private readonly IDamageCalculator _inner;
    private readonly int _armorValue;

    public ArmorReductionDecorator(IDamageCalculator inner, int armorValue)
    {
        _inner = inner;
        _armorValue = armorValue;
    }

    public int Calculate(int baseDamage)
    {
        int damage = _inner.Calculate(baseDamage);
        return Mathf.Max(1, damage - _armorValue);
    }
}

// Usage:
// IDamageCalculator calc = new BaseDamageCalculator();
// calc = new CriticalHitDecorator(calc, 2.0f, 0.15f);
// calc = new ArmorReductionDecorator(calc, targetArmor);
// int finalDamage = calc.Calculate(baseDamage);
```

---

## 4. Bootstrap Architecture

### 4.1 Bootstrap Flow

```
Application Start
       │
       ▼
┌──────────────┐
│  Bootstrap   │    Scene Index 0 — Always loads first
│    Scene     │    Initializes core services
└──────┬───────┘
       │
       ▼
┌──────────────┐
│  Initialize  │    Register services in ServiceLocator
│   Services   │    AudioManager, SaveManager, InputManager
└──────┬───────┘
       │
       ▼
┌──────────────┐
│  Load Save   │    Load player preferences and progress
│    Data      │    Apply settings (volume, language, etc.)
└──────┬───────┘
       │
       ▼
┌──────────────┐
│  Load Main   │    Async load MainMenu scene
│    Menu      │    Show loading screen if needed
└──────────────┘
```

### 4.2 Bootstrap Script

```csharp
// ============================================================================
// GameBootstrapper.cs
// Purpose: Application entry point. Initializes all core services.
// ============================================================================
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameName.Core.Bootstrap
{
    /// <summary>
    /// Entry point for the application. Initializes all core services
    /// and loads the main menu scene.
    /// </summary>
    /// <remarks>
    /// <para><b>Setup:</b> Place in the Bootstrap scene (Build Index 0).
    /// This scene should contain ONLY this script and essential
    /// DontDestroyOnLoad managers.</para>
    /// </remarks>
    public class GameBootstrapper : MonoBehaviour
    {
        private const string MainMenuScene = "MainMenu";

        [Header("Core Services")]
        [Tooltip("Audio manager prefab to instantiate.")]
        [SerializeField] private GameObject _audioManagerPrefab;

        [Tooltip("Save manager prefab to instantiate.")]
        [SerializeField] private GameObject _saveManagerPrefab;

        private async void Awake()
        {
            // Prevent duplicate bootstrapping
            if (FindObjectsByType<GameBootstrapper>(FindObjectsSortMode.None).Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);

            InitializeCoreServices();
            await LoadInitialScene();
        }

        private void InitializeCoreServices()
        {
            // Instantiate and register core services
            var audioManager = Instantiate(_audioManagerPrefab);
            DontDestroyOnLoad(audioManager);

            var saveManager = Instantiate(_saveManagerPrefab);
            DontDestroyOnLoad(saveManager);

            // Set target frame rate
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
        }

        private async Awaitable LoadInitialScene()
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(MainMenuScene);

            while (!operation.isDone)
            {
                await Awaitable.NextFrameAsync();
            }
        }
    }
}
```

---

## 5. When to Use Which Pattern

| Situation | Recommended Pattern |
|-----------|-------------------|
| Cross-system notifications (Player died → UI + Audio + Save) | **ScriptableObject Event Channel** |
| Within-system notifications (HealthSystem → HealthBar) | **C# Events (Action/delegate)** |
| Complex entity behavior (Player states, Enemy AI) | **State Machine** |
| Global infrastructure services (Audio, Save, Input) | **Service Locator** |
| Creating configured instances (Enemies, Projectiles) | **Factory + Object Pool** |
| Interchangeable behaviors (Movement types, AI modes) | **Strategy Pattern** |
| Stacking modifiers (Buffs, damage calculation) | **Decorator Pattern** |
| Reversible actions (Undo, action replay) | **Command Pattern** |
| Design-time configuration (Stats, Levels, Items) | **ScriptableObject Config** |
| Feature toggling (Debug, cheats) | **ScriptableObject Variable** |

---

## 6. Anti-Patterns to Avoid

### 6.1 Singleton Overuse

```csharp
// ❌ WRONG: Everything is a Singleton
public class GameManager : Singleton<GameManager> { }
public class UIManager : Singleton<UIManager> { }
public class AudioManager : Singleton<AudioManager> { }
public class EnemyManager : Singleton<EnemyManager> { }
public class ItemManager : Singleton<ItemManager> { }
// This creates a web of global state that is untestable

// ✅ CORRECT: Use Service Locator for true services, events for communication
ServiceLocator.Register<IAudioService>(audioManager);
ServiceLocator.Register<ISaveService>(saveManager);
// Gameplay systems use events, not global access
```

### 6.2 God Object

```csharp
// ❌ WRONG: One class does everything
public class GameManager : MonoBehaviour
{
    // Handles input, spawning, scoring, UI, audio, saving...
    // 2000+ lines
}

// ✅ CORRECT: Split into focused components
// GameStateManager.cs - Game flow (pause, resume, game over)
// ScoreManager.cs     - Score tracking and persistence
// SpawnManager.cs     - Enemy/item spawning
// Each with its own single responsibility
```

### 6.3 Deep Inheritance

```csharp
// ❌ WRONG: Deep inheritance tree
class Entity : MonoBehaviour { }
class Character : Entity { }
class Humanoid : Character { }
class Warrior : Humanoid { }
class PlayerWarrior : Warrior { }

// ✅ CORRECT: Flat composition
// PlayerWarrior GameObject:
//   - HealthSystem component
//   - MovementSystem component
//   - CombatSystem component
//   - AnimationController component
//   - InteractionDetector component
```

### 6.4 Circular Dependencies

```csharp
// ❌ WRONG: A depends on B, B depends on A
class PlayerCombat
{
    [SerializeField] private EnemyHealth _enemyHealth; // Direct reference
}

class EnemyHealth
{
    [SerializeField] private PlayerCombat _playerCombat; // Circular!
}

// ✅ CORRECT: Use interfaces and events
class PlayerCombat
{
    private void OnAttackHit(Collider2D other)
    {
        if (other.TryGetComponent(out IDamageable target))
        {
            target.TakeDamage(_attackDamage); // Interface, no direct dependency
        }
    }
}
```
