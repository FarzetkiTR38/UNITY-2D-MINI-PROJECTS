# Testing Guide

## Overview

This document defines the testing strategies, patterns, and tools for Unity 6 2D game projects. Testing ensures code correctness, prevents regressions, and builds confidence for refactoring and feature additions.

---

## 1. Testing Strategy

### 1.1 Test Pyramid

```
              ┌──────────┐
              │  Manual   │  ← Playtesting, QA sessions
              │  Testing  │
            ┌─┤──────────├─┐
            │ Integration  │  ← Play Mode Tests (Unity Test Runner)
            │    Tests     │
          ┌─┤─────────────├─┐
          │    Unit Tests    │  ← Edit Mode Tests (NUnit)
          │    (Most Tests)  │
          └──────────────────┘
```

### 1.2 What to Test

| Category | What to Test | Test Type |
|----------|-------------|-----------|
| **Pure Logic** | Damage calculation, XP curves, inventory math | Unit (Edit Mode) |
| **State Machines** | State transitions, entry/exit conditions | Unit (Edit Mode) |
| **Data Validation** | ScriptableObject data integrity | Unit (Edit Mode) |
| **Serialization** | Save/load data correctness | Unit (Edit Mode) |
| **Component Behavior** | Player movement, combat, physics | Integration (Play Mode) |
| **System Integration** | Scene loading, event flow, UI binding | Integration (Play Mode) |
| **Visual/Audio** | Animations, VFX, sound playback | Manual |
| **Platform** | Touch input, performance, device-specific | Manual |

### 1.3 What NOT to Test

- Unity Engine internals (Physics, Rendering)
- Third-party plugin behavior
- Visual correctness (use screenshots instead)
- Exact floating-point values (use `Assert.AreEqual` with tolerance)

---

## 2. Test Setup

### 2.1 Assembly Definitions

```
Tests/
├── EditMode/
│   ├── GameName.Tests.EditMode.asmdef
│   │   ├── References: GameName.Runtime.Core, GameName.Runtime.Gameplay
│   │   ├── Platforms: Editor
│   │   ├── Define Constraints: UNITY_INCLUDE_TESTS
│   │   └── Test Assemblies: true
│   └── *.Tests.cs
│
└── PlayMode/
    ├── GameName.Tests.PlayMode.asmdef
    │   ├── References: GameName.Runtime.Core, GameName.Runtime.Gameplay, GameName.Runtime.Systems
    │   ├── Platforms: Any
    │   ├── Define Constraints: UNITY_INCLUDE_TESTS
    │   └── Test Assemblies: true
    └── *.Tests.cs
```

### 2.2 Test Runner

```
Window → General → Test Runner (Alt+Shift+T)

Edit Mode tab: Unit tests (fast, no scene required)
Play Mode tab: Integration tests (requires play mode, scene setup)
```

---

## 3. Unit Test Patterns (Edit Mode)

### 3.1 Test Template

```csharp
using NUnit.Framework;
using UnityEngine;

namespace GameName.Tests.EditMode
{
    /// <summary>
    /// Unit tests for the damage calculation system.
    /// </summary>
    [TestFixture]
    public class DamageCalculationTests
    {
        #region Setup

        private BaseDamageCalculator _calculator;

        [SetUp]
        public void SetUp()
        {
            _calculator = new BaseDamageCalculator();
        }

        #endregion

        #region Base Damage Tests

        [Test]
        public void Calculate_WithBaseDamage_ReturnsUnmodified()
        {
            // Arrange
            int baseDamage = 50;

            // Act
            int result = _calculator.Calculate(baseDamage);

            // Assert
            Assert.AreEqual(50, result);
        }

        [Test]
        public void Calculate_WithZeroDamage_ReturnsZero()
        {
            Assert.AreEqual(0, _calculator.Calculate(0));
        }

        [Test]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        [TestCase(999)]
        public void Calculate_WithVariousDamageValues_ReturnsCorrectResult(int damage)
        {
            Assert.AreEqual(damage, _calculator.Calculate(damage));
        }

        #endregion

        #region Armor Reduction Tests

        [Test]
        public void ArmorReduction_ReducesDamage()
        {
            // Arrange
            IDamageCalculator calc = new BaseDamageCalculator();
            calc = new ArmorReductionDecorator(calc, armorValue: 10);

            // Act
            int result = calc.Calculate(50);

            // Assert
            Assert.AreEqual(40, result);
        }

        [Test]
        public void ArmorReduction_NeverReducesBelowOne()
        {
            IDamageCalculator calc = new BaseDamageCalculator();
            calc = new ArmorReductionDecorator(calc, armorValue: 999);

            int result = calc.Calculate(5);

            Assert.GreaterOrEqual(result, 1, "Damage should never be reduced below 1.");
        }

        #endregion
    }
}
```

### 3.2 Testing Inventory Logic

```csharp
[TestFixture]
public class InventoryTests
{
    private Inventory _inventory;
    private ItemData _sword;
    private ItemData _potion;

    [SetUp]
    public void SetUp()
    {
        _inventory = new Inventory(capacity: 10);
        _sword = ScriptableObject.CreateInstance<ItemData>();
        // Configure sword as non-stackable (MaxStackSize = 1)
        _potion = ScriptableObject.CreateInstance<ItemData>();
        // Configure potion as stackable (MaxStackSize = 99)
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_sword);
        Object.DestroyImmediate(_potion);
    }

    [Test]
    public void TryAddItem_EmptyInventory_ReturnsTrue()
    {
        bool result = _inventory.TryAddItem(_sword);
        Assert.IsTrue(result);
    }

    [Test]
    public void TryAddItem_FullInventory_ReturnsFalse()
    {
        // Fill all slots
        for (int i = 0; i < 10; i++)
        {
            ItemData item = ScriptableObject.CreateInstance<ItemData>();
            _inventory.TryAddItem(item);
        }

        bool result = _inventory.TryAddItem(_sword);
        Assert.IsFalse(result);
    }

    [Test]
    public void GetItemCount_AfterAdding_ReturnsCorrectCount()
    {
        _inventory.TryAddItem(_potion, count: 5);
        Assert.AreEqual(5, _inventory.GetItemCount(_potion));
    }

    [Test]
    public void TryRemoveItem_ExistingItem_ReturnsTrue()
    {
        _inventory.TryAddItem(_sword);
        bool result = _inventory.TryRemoveItem(_sword);
        Assert.IsTrue(result);
        Assert.AreEqual(0, _inventory.GetItemCount(_sword));
    }
}
```

### 3.3 Testing State Machine

```csharp
[TestFixture]
public class StateMachineTests
{
    private StateMachine _stateMachine;
    private MockState _idleState;
    private MockState _runState;

    [SetUp]
    public void SetUp()
    {
        _stateMachine = new StateMachine();
        _idleState = new MockState("Idle");
        _runState = new MockState("Run");
    }

    [Test]
    public void SetState_CallsEnterOnNewState()
    {
        _stateMachine.SetState(_idleState);
        Assert.IsTrue(_idleState.EnterCalled);
    }

    [Test]
    public void SetState_CallsExitOnPreviousState()
    {
        _stateMachine.SetState(_idleState);
        _stateMachine.SetState(_runState);
        Assert.IsTrue(_idleState.ExitCalled);
    }

    [Test]
    public void Transition_WhenConditionMet_ChangesState()
    {
        bool shouldRun = false;
        _stateMachine.AddTransition(_idleState, _runState, () => shouldRun);
        _stateMachine.SetState(_idleState);

        shouldRun = true;
        _stateMachine.Tick();

        Assert.AreEqual(_runState, _stateMachine.CurrentState);
    }

    // Mock state for testing
    private class MockState : IState
    {
        public string Name { get; }
        public bool EnterCalled { get; private set; }
        public bool ExitCalled { get; private set; }
        public int TickCount { get; private set; }

        public MockState(string name) { Name = name; }

        public void Enter() => EnterCalled = true;
        public void Exit() => ExitCalled = true;
        public void Tick() => TickCount++;
        public void FixedTick() { }
    }
}
```

---

## 4. Integration Tests (Play Mode)

### 4.1 Play Mode Test Template

```csharp
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameName.Tests.PlayMode
{
    /// <summary>
    /// Play mode integration tests for player movement.
    /// </summary>
    [TestFixture]
    public class PlayerMovementTests
    {
        private GameObject _playerObject;
        private PlayerMovement _movement;
        private Rigidbody2D _rb;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _playerObject = new GameObject("TestPlayer");
            _rb = _playerObject.AddComponent<Rigidbody2D>();
            _rb.gravityScale = 0f; // Disable gravity for controlled testing
            _playerObject.AddComponent<CapsuleCollider2D>();
            _movement = _playerObject.AddComponent<PlayerMovement>();

            yield return null; // Wait one frame for Awake/Start
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(_playerObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SetMoveInput_RightInput_MovesPlayerRight()
        {
            // Arrange
            Vector3 startPos = _playerObject.transform.position;
            _movement.SetMoveInput(Vector2.right);

            // Act — wait several physics frames
            for (int i = 0; i < 10; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            // Assert
            Assert.Greater(_playerObject.transform.position.x, startPos.x,
                "Player should have moved to the right.");
        }
    }
}
```

---

## 5. Testing Best Practices

### 5.1 Rules

| Rule | Details |
|------|---------|
| **One assertion per concept** | Test one behavior per test method |
| **Arrange-Act-Assert** | Structure every test with clear sections |
| **Descriptive names** | `MethodName_Condition_ExpectedResult` |
| **Isolated tests** | Each test must be independent — use SetUp/TearDown |
| **No test interdependencies** | Tests must run in any order |
| **Test edge cases** | Zero, negative, max values, null inputs |
| **Test failure paths** | Verify error handling works correctly |
| **Keep tests fast** | Unit tests < 100ms, integration tests < 2s |
| **Use ScriptableObject.CreateInstance** | For SO test data, not asset loading |
| **Destroy test objects** | Always clean up in TearDown |

### 5.2 Naming Convention

```
Pattern: [MethodUnderTest]_[Scenario]_[ExpectedBehavior]

Examples:
TakeDamage_WithPositiveAmount_ReducesHealth
TakeDamage_WhenInvincible_DoesNotReduceHealth
TakeDamage_ReducingToZero_RaisesDeathEvent
TakeDamage_WithNegativeAmount_ThrowsException
Heal_AboveMaxHealth_ClampsToMax
Heal_WhenDead_DoesNothing
TryAddItem_FullInventory_ReturnsFalse
GetItemCount_AfterRemoval_ReturnsCorrectCount
```
