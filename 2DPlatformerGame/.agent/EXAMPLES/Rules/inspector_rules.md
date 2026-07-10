# Inspector Rules

## Overview

Rules for creating Inspector-friendly Unity components that are easy to configure, validate, and debug.

---

## 1. Required Attributes

Every serialized field MUST have appropriate attributes:

| Attribute | When to Use | Example |
|-----------|------------|---------|
| `[Header("Section")]` | Group related fields | `[Header("Movement")]` |
| `[Tooltip("Description")]` | EVERY serialized field | `[Tooltip("Speed in units/sec")]` |
| `[SerializeField]` | ALL private fields exposed to Inspector | `[SerializeField] private float _speed;` |
| `[Min(value)]` | Non-negative values | `[Min(0f)]` |
| `[Range(min, max)]` | Bounded values | `[Range(0f, 1f)]` |
| `[Space(pixels)]` | Visual separation between groups | `[Space(10)]` |
| `[TextArea(min, max)]` | Multi-line string fields | `[TextArea(2, 5)]` |
| `[FormerlySerializedAs]` | When renaming serialized fields | `[FormerlySerializedAs("oldName")]` |

---

## 2. Field Organization Order

```csharp
// 1. Movement settings
[Header("Movement")]
[Tooltip("...")] [SerializeField, Min(0f)] private float _speed;
[Tooltip("...")] [SerializeField, Min(0f)] private float _jumpForce;

[Space(10)]

// 2. Combat settings
[Header("Combat")]
[Tooltip("...")] [SerializeField, Min(1)] private int _damage;
[Tooltip("...")] [SerializeField, Range(0f, 5f)] private float _cooldown;

[Space(10)]

// 3. References (transforms, other components)
[Header("References")]
[Tooltip("...")] [SerializeField] private Transform _groundCheck;
[Tooltip("...")] [SerializeField] private LayerMask _groundLayer;

[Space(10)]

// 4. Audio/VFX references
[Header("Audio")]
[Tooltip("...")] [SerializeField] private AudioClip _jumpSound;

[Space(10)]

// 5. Events/Channels
[Header("Events")]
[Tooltip("...")] [SerializeField] private VoidEventChannel _onDied;

[Space(10)]

// 6. Debug settings (last)
[Header("Debug")]
[Tooltip("...")] [SerializeField] private bool _showGizmos;
```

---

## 3. Validation Rules

### 3.1 OnValidate

Every component with serialized references MUST implement `OnValidate()`:

```csharp
private void OnValidate()
{
    // Warn for missing optional references
    if (_groundCheck == null)
        Debug.LogWarning($"[{name}] Ground Check not assigned.", this);

    // Error for required references
    if (_playerConfig == null)
        Debug.LogError($"[{name}] PlayerConfig is REQUIRED!", this);

    // Clamp values
    _maxHealth = Mathf.Max(1, _maxHealth);
    _attackCooldown = Mathf.Max(0.1f, _attackCooldown);

    // Validate layer masks
    if (_groundLayer == 0)
        Debug.LogWarning($"[{name}] Ground Layer mask is empty.", this);
}
```

### 3.2 RequireComponent

Use `[RequireComponent]` for mandatory components:

```csharp
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public class PlayerMovement : MonoBehaviour { }
```

---

## 4. Context Menu Debug Actions

Every gameplay component SHOULD have debug actions:

```csharp
[ContextMenu("Debug/Log State")]
private void DebugLogState() { }

[ContextMenu("Debug/Reset")]
private void DebugReset() { }

[ContextMenu("Debug/Test Damage")]
private void DebugTestDamage() { }
```

---

## 5. Read-Only Inspector Fields

For displaying runtime state without allowing modification:

```csharp
// Custom attribute
[AttributeUsage(AttributeTargets.Field)]
public class ReadOnlyAttribute : PropertyAttribute { }

// Property drawer
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
#endif

// Usage
[Header("Runtime State (Read-Only)")]
[ReadOnly, SerializeField] private int _currentHealth;
[ReadOnly, SerializeField] private bool _isGrounded;
[ReadOnly, SerializeField] private string _currentState;
```

---

## 6. Best Practices Summary

| Practice | Details |
|----------|---------|
| Always use `[Tooltip]` | Designers should never guess what a field does |
| Group with `[Header]` | Logical sections are easier to scan |
| Separate with `[Space]` | Visual breathing room between groups |
| Use `[Range]` for bounded values | Prevents out-of-range values |
| Use `[Min]` for non-negative | Prevents negative where inappropriate |
| Use `[TextArea]` for descriptions | Multi-line strings are easier to read |
| Implement `OnValidate` | Catches config errors before play mode |
| Add `[ContextMenu]` debug actions | Quick testing without code changes |
| Use `[RequireComponent]` | Prevents missing component errors |
| Use `[DisallowMultipleComponent]` | Prevents duplicate component errors |
| Never use public fields | Always `[SerializeField] private` |
