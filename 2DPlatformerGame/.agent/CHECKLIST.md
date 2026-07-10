# Pre-Commit and Review Checklist

## Overview

Use this checklist before committing code or reviewing pull requests. Every item must be verified to maintain code quality and prevent common issues.

---

## 1. Code Quality Checklist

### 1.1 Before Every Commit

```
□ Code compiles with zero errors and zero warnings
□ No new compiler warnings introduced
□ All public members have XML documentation comments
□ All SerializeField fields have [Tooltip] attributes
□ No magic numbers — all constants are named
□ No public fields — use [SerializeField] private with properties
□ Private fields use underscore prefix (_fieldName)
□ Method names are descriptive verb phrases
□ Boolean names use is/has/can prefix
□ No empty catch blocks
□ No commented-out code (remove it or use version control)
□ Regions are used consistently and not hiding complexity
□ File length is under 400 lines (600 hard limit)
□ One class per file (filename matches class name)
```

### 1.2 Unity-Specific Checks

```
□ No deprecated API usage (see UNITY_RULES.md banned list)
□ No FindObjectOfType or GameObject.Find calls
□ No Resources.Load calls (use Addressables or SerializeField)
□ Input uses Input System (New), not Input.GetKey
□ Physics code is in FixedUpdate, not Update
□ Camera follow code is in LateUpdate
□ TryGetComponent used instead of GetComponent where possible
□ Component references cached in Awake, not retrieved every frame
□ OnEnable subscribes to events, OnDisable unsubscribes
□ OnValidate validates SerializeField references
□ No Instantiate/Destroy in hot paths (use object pooling)
□ CompareTag used instead of == for tag comparison
□ Animator hashes cached as static readonly
□ Shader property IDs cached as static readonly
```

### 1.3 Performance Checks

```
□ No GC allocations in Update/FixedUpdate/LateUpdate
□ No LINQ in hot paths
□ No string concatenation in hot paths
□ No new WaitForSeconds() in coroutines (cache them)
□ No foreach on Dictionary in hot paths
□ No Debug.Log without #if UNITY_EDITOR or [Conditional]
□ Collections pre-allocated with expected capacity
□ No Camera.main called every frame (cache it)
□ Physics queries use NonAlloc variants
□ Object pools used for frequently created/destroyed objects
```

---

## 2. Architecture Checklist

```
□ Class follows Single Responsibility Principle
□ No circular dependencies between classes/assemblies
□ Communication uses events, not direct references (between systems)
□ ScriptableObjects used for design-time configuration
□ No Singleton unless absolutely justified (documented reason)
□ Interfaces used for cross-system contracts (IDamageable, etc.)
□ Assembly Definition dependency flow is correct (Core ← Gameplay, Core ← UI)
□ No runtime assembly depends on editor assembly
□ New code fits the existing project architecture
□ No global state introduced without ServiceLocator pattern
```

---

## 3. Inspector Checklist

```
□ All SerializeField fields have [Header] grouping
□ All SerializeField fields have [Tooltip] descriptions
□ Numeric fields have [Range] or [Min] where appropriate
□ Reference fields validated in OnValidate with warnings
□ Required components declared with [RequireComponent]
□ [DisallowMultipleComponent] used where appropriate
□ [ContextMenu] debug actions added for complex components
□ Inspector layout is clean and organized with [Space]
□ FormerlySerializedAs used when renaming serialized fields
```

---

## 4. Safety Checklist

```
□ All public methods validate their parameters
□ Null checks present for all external references
□ Try-catch used for file I/O operations
□ Async operations have cancellation support
□ Events checked for null before invocation (?.Invoke)
□ Array/collection bounds checked before access
□ Division by zero prevented
□ Enum values validated (IsDefined or switch with default)
□ Rigidbody operations check for null
□ Scene loading checks scene exists in build settings
```

---

## 5. New Script Checklist

When creating a new script, verify:

```
□ File header comment with purpose and dependencies
□ Proper namespace (GameName.Layer.System)
□ XML documentation on class
□ XML documentation remarks with Purpose, Dependencies, Inspector Setup, Usage, Performance, Extension
□ Constants section with all magic numbers extracted
□ Regions organizing class structure
□ Awake for self-initialization
□ Start for cross-object references
□ OnEnable/OnDisable for event subscription
□ OnValidate for Inspector validation
□ OnDrawGizmosSelected for visual debugging (if applicable)
□ Interface implementations where appropriate
□ Event declarations for state changes
```

---

## 6. New ScriptableObject Checklist

```
□ [CreateAssetMenu] attribute with proper path
□ XML documentation with creation path
□ All fields private with [SerializeField]
□ All fields have [Tooltip]
□ Read-only properties for all fields
□ OnValidate for data integrity checks
□ Organized with [Header] and [Space]
□ No runtime mutation (read-only pattern)
□ Proper folder placement (ScriptableObjects/Category/)
□ Asset named descriptively (EnemyData_Slime.asset)
```

---

## 7. New Prefab Checklist

```
□ Root component has the main script
□ Component dependencies documented in script XML docs
□ All SerializeField references assigned
□ Layer set correctly
□ Tag set if needed
□ Sorting Layer and Order configured
□ Collider size matches visual
□ Rigidbody2D configured (body type, constraints, interpolation)
□ Nested prefabs used where appropriate
□ Prefab variant used for variations (not duplicates)
□ Named descriptively in PascalCase
□ Placed in correct Prefabs/ subfolder
```

---

## 8. Scene Checklist

```
□ Scene hierarchy uses separator objects (--- CATEGORY ---)
□ Required managers present (or loaded from Bootstrap)
□ Camera configured with Cinemachine
□ EventSystem present for UI
□ Tilemap colliders using CompositeCollider2D
□ Lighting configured (Global Light 2D)
□ Sorting layers used correctly
□ No broken prefab references
□ No missing script components
□ No disabled components that should be enabled (or vice versa)
□ Debug objects tagged as EditorOnly
□ Scene added to Build Settings
```

---

## 9. PR/Code Review Checklist

```
For Reviewers:

□ Does the change follow existing architecture patterns?
□ Is the code easy to understand without the PR description?
□ Are there any performance concerns in hot paths?
□ Are edge cases handled (null, zero, empty, max)?
□ Are events properly subscribed/unsubscribed?
□ Is the Inspector experience good for designers?
□ Are there unit tests for new logic?
□ Does the change break any existing tests?
□ Are ScriptableObjects used for configuration?
□ Is the naming consistent with the project conventions?
```
