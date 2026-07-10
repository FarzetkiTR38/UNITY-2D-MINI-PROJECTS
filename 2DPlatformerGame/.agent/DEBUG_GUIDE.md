# Debug Guide

## Overview

This document defines debugging strategies, tools, and patterns for Unity 6 2D projects. Effective debugging catches issues early, reduces iteration time, and prevents production bugs.

---

## 1. Unity Profiler

### 1.1 Key Profiler Modules

| Module | What to Monitor |
|--------|----------------|
| **CPU Usage** | Frame time, script execution time, GC allocation |
| **GPU Usage** | Draw calls, shader complexity, overdraw |
| **Memory** | Total allocated, texture memory, mesh memory |
| **Physics 2D** | Active contacts, rigidbody count, simulation time |
| **Rendering** | Batches, SetPass calls, triangles, vertices |
| **Audio** | Active sources, channel count, memory |
| **UI** | Canvas rebuild, layout rebuild, batch breaks |

### 1.2 Profiler Usage

```
Steps:
1. Window → Analysis → Profiler (Ctrl+7)
2. Click Record
3. Play the game
4. Look for:
   - Red frames (frame time > 33ms)
   - GC.Alloc spikes in CPU module
   - Increasing memory in Memory module
   - High draw calls in Rendering module
5. Deep Profile for exact allocation source (expensive, use sparingly)
```

---

## 2. Debug Logging Standards

### 2.1 Logging Levels

```csharp
// INFORMATION: Normal game flow
Debug.Log($"[SceneLoader] Loading scene: {sceneName}", this);

// WARNING: Unexpected but recoverable
Debug.LogWarning($"[AudioManager] Clip is null for action: {actionName}. Using fallback.", this);

// ERROR: Something is broken
Debug.LogError($"[HealthSystem] Attempted to apply negative damage: {amount}", this);

// ASSERTION: Invariant violation
Debug.Assert(maxHealth > 0, $"[{name}] MaxHealth must be positive", this);
```

### 2.2 Debug Wrapper

```csharp
/// <summary>
/// Conditional debug logger that is stripped from release builds.
/// </summary>
public static class GameDebug
{
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void Log(string message, Object context = null)
    {
        Debug.Log(message, context);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void LogWarning(string message, Object context = null)
    {
        Debug.LogWarning(message, context);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void LogError(string message, Object context = null)
    {
        Debug.LogError(message, context);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void Assert(bool condition, string message, Object context = null)
    {
        Debug.Assert(condition, message, context);
    }
}
```

### 2.3 Logging Formatting Rules

```
Format: [ClassName] Action/description with context data.

Examples:
[PlayerMovement] Grounded state changed: true → false
[EnemyAI] State transition: Patrol → Chase (target: Player)
[SaveManager] Save completed: gamesave.json (2.3KB, 12ms)
[InventorySystem] Added item: HealthPotion x3 (slot 5)
[SceneLoader] Loading scene: Level_02 (async, progress: 45%)
```

---

## 3. Gizmos and Visual Debugging

### 3.1 Gizmo Patterns

```csharp
// ✅ Use OnDrawGizmosSelected for component-specific debug visuals
private void OnDrawGizmosSelected()
{
    // Ground check visualization
    if (_groundCheckPoint != null)
    {
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(_groundCheckPoint.position, _groundCheckRadius);
    }

    // Attack range visualization
    if (_attackPoint != null)
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(_attackPoint.position, _attackRadius);
    }

    // Detection range
    Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
    Gizmos.DrawWireSphere(transform.position, _detectionRange);

    // Patrol waypoints
    if (_waypoints != null && _waypoints.Length > 1)
    {
        Gizmos.color = Color.cyan;
        for (int i = 0; i < _waypoints.Length; i++)
        {
            Gizmos.DrawSphere(_waypoints[i].position, 0.15f);
            if (i < _waypoints.Length - 1)
            {
                Gizmos.DrawLine(_waypoints[i].position, _waypoints[i + 1].position);
            }
        }
    }
}

// ✅ Use OnDrawGizmos for always-visible debug (use sparingly)
private void OnDrawGizmos()
{
    // Only draw if debug toggle is enabled
    if (!_showDebugGizmos) return;

    Gizmos.color = Color.blue;
    Gizmos.DrawRay(transform.position, Vector2.right * _wallCheckDistance);
}
```

### 3.2 Debug Drawing at Runtime

```csharp
// ✅ Use Debug.DrawRay/Line for runtime visualization (Scene view only)
private void FixedUpdate()
{
    #if UNITY_EDITOR
    // Visualize movement direction
    Debug.DrawRay(transform.position, _moveDirection * 2f, Color.green);

    // Visualize velocity
    Debug.DrawRay(transform.position, _rb.linearVelocity * 0.5f, Color.yellow);

    // Visualize ground check ray
    Debug.DrawRay(_groundCheck.position, Vector2.down * _groundCheckDistance, 
        _isGrounded ? Color.green : Color.red);
    #endif
}
```

---

## 4. Context Menu Debug Actions

```csharp
// Add debug actions to components via ContextMenu
[ContextMenu("Debug/Reset Health")]
private void DebugResetHealth()
{
    _currentHealth = _maxHealth;
    OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    Debug.Log($"[{name}] Health reset to {_maxHealth}", this);
}

[ContextMenu("Debug/Kill Entity")]
private void DebugKill()
{
    TakeDamage(_currentHealth);
}

[ContextMenu("Debug/Toggle Invincibility")]
private void DebugToggleInvincibility()
{
    _isInvincible = !_isInvincible;
    Debug.Log($"[{name}] Invincibility: {_isInvincible}", this);
}

[ContextMenu("Debug/Add 100 Currency")]
private void DebugAddCurrency()
{
    AddCurrency(100);
    Debug.Log($"[{name}] Added 100 currency. Total: {_currency}", this);
}

[ContextMenu("Debug/Log Current State")]
private void DebugLogState()
{
    Debug.Log($"[{name}] HP: {_currentHealth}/{_maxHealth}, " +
              $"Position: {transform.position}, " +
              $"Grounded: {_isGrounded}, " +
              $"State: {_stateMachine.CurrentState?.GetType().Name}", this);
}
```

---

## 5. Editor Debug Window

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace GameName.Editor.Windows
{
    /// <summary>
    /// Editor window for real-time game state debugging.
    /// </summary>
    public class GameDebugWindow : EditorWindow
    {
        [MenuItem("Tools/GameName/Debug Window")]
        public static void ShowWindow()
        {
            GetWindow<GameDebugWindow>("Game Debug");
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use debug controls.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("Game Debug Controls", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            // Time controls
            EditorGUILayout.LabelField("Time", EditorStyles.boldLabel);
            Time.timeScale = EditorGUILayout.Slider("Time Scale", Time.timeScale, 0f, 3f);

            EditorGUILayout.Space(5);

            // Player controls
            EditorGUILayout.LabelField("Player", EditorStyles.boldLabel);
            if (GUILayout.Button("Heal Player Full"))
            {
                var players = FindObjectsByType<HealthSystem>(FindObjectsSortMode.None);
                foreach (var health in players)
                {
                    if (health.CompareTag("Player"))
                    {
                        health.Heal(health.MaxHealth);
                    }
                }
            }

            if (GUILayout.Button("Kill All Enemies"))
            {
                var enemies = FindObjectsByType<HealthSystem>(FindObjectsSortMode.None);
                foreach (var health in enemies)
                {
                    if (health.CompareTag("Enemy"))
                    {
                        health.TakeDamage(health.MaxHealth);
                    }
                }
            }

            EditorGUILayout.Space(5);

            // Scene controls
            EditorGUILayout.LabelField("Scene", EditorStyles.boldLabel);
            if (GUILayout.Button("Reload Current Scene"))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }

            Repaint();
        }
    }
}
#endif
```

---

## 6. Common Debugging Scenarios

### 6.1 Debugging Checklist

| Issue | Debug Steps |
|-------|-------------|
| **Object not moving** | Check Rigidbody2D exists, not Static, constraints not frozen, time scale > 0 |
| **Collision not working** | Check layers in Physics2D matrix, collider size, trigger vs collision, Rigidbody2D exists |
| **Animation not playing** | Check Animator controller assigned, parameter names match code hashes, transitions configured |
| **Null Reference** | Check Inspector assignments, Awake/Start order, use OnValidate for warnings |
| **Input not responding** | Check Input System enabled, action map active, control scheme matches device |
| **GC spike** | Profile with Deep Profile, check for LINQ/string concat/new in Update |
| **Physics jitter** | Check Interpolation setting, ensure physics in FixedUpdate, check gravity |
| **Sprite not visible** | Check Sorting Layer, Order in Layer, SpriteRenderer enabled, position in camera view |
| **Sound not playing** | Check AudioSource enabled, clip assigned, volume > 0, AudioListener exists |
| **Save not loading** | Check file path, JSON format, data version, Application.persistentDataPath |

### 6.2 Null Reference Prevention

```csharp
// In OnValidate — catches missing references before play
private void OnValidate()
{
    if (_groundCheck == null)
        Debug.LogWarning($"[{name}] GroundCheck is not assigned!", this);

    if (_attackPoint == null)
        Debug.LogWarning($"[{name}] AttackPoint is not assigned!", this);

    if (_playerConfig == null)
        Debug.LogError($"[{name}] PlayerConfig ScriptableObject is required!", this);
}

// In Awake — catches runtime component issues
private void Awake()
{
    _rb = GetComponent<Rigidbody2D>();
    Debug.Assert(_rb != null, $"[{name}] Missing Rigidbody2D!", this);

    if (!TryGetComponent(out _animator))
    {
        Debug.LogWarning($"[{name}] No Animator found. Animations disabled.", this);
    }
}
```

---

## 7. Performance Debugging

### 7.1 Frame Debugger

```
Window → Analysis → Frame Debugger

Use to:
- Identify why draw calls are not batching
- Find overdraw issues
- Debug shader/material problems
- Verify Sprite Atlas batching
- Check render order
```

### 7.2 Memory Profiler

```
Window → Analysis → Memory Profiler

Use to:
- Find memory leaks (steadily increasing allocations)
- Identify large textures that need compression
- Track audio memory usage
- Verify Addressable loading/unloading
- Check for unreleased references
```

### 7.3 Physics Debugger

```
Window → Analysis → Physics Debugger

Use to:
- Visualize all 2D colliders
- Check collision matrix settings
- Identify overlapping colliders
- Verify trigger zones
- Debug raycasts
```
