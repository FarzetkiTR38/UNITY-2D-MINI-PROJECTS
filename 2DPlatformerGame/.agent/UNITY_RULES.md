# Unity 6 (6000.3.18f1) — Rules and Constraints

## Version-Specific Rules

This document defines the mandatory rules for Unity 6 (6000.3.18f1) development. Every code generation, review, and suggestion MUST comply with these rules.

---

## 1. API Usage Rules

### 1.1 Deprecated API Blacklist

The following APIs are **BANNED**. Never use them under any circumstance:

```
❌ FindObjectOfType()              → Use dependency injection, SerializeField, or ServiceLocator
❌ FindObjectsOfType()             → Use cached lists, events, or registry pattern
❌ FindObjectOfType<T>()           → Use TryGetComponent, GetComponentInChildren, or injection
❌ FindObjectsByType<T>()          → Only if absolutely necessary with FindObjectsSortMode
❌ Object.FindFirstObjectByType()  → Use injection or registry pattern instead
❌ GameObject.Find()               → Use SerializeField references
❌ GameObject.FindWithTag()        → Use SerializeField references or tag-based registry
❌ SendMessage()                   → Use events, interfaces, or direct method calls
❌ BroadcastMessage()              → Use events or the observer pattern
❌ SendMessageUpwards()            → Use events or direct references
❌ Input.GetKey()                  → Use Input System (New)
❌ Input.GetKeyDown()              → Use Input System (New)
❌ Input.GetKeyUp()                → Use Input System (New)
❌ Input.GetAxis()                 → Use Input System (New)
❌ Input.GetButton()               → Use Input System (New)
❌ Input.GetMouseButton()          → Use Input System (New)
❌ Input.mousePosition             → Use Input System (New)
❌ Input.touches                   → Use Input System (New) EnhancedTouch
❌ OnGUI()                         → Use UI Toolkit or TextMeshPro
❌ GUI.*                           → Use UI Toolkit or TextMeshPro
❌ GUILayout.*                     → Use UI Toolkit or TextMeshPro (runtime) or EditorGUILayout (editor)
❌ WWW                             → Use UnityWebRequest
❌ Resources.Load()                → Use Addressables or direct SerializeField references
❌ Resources.LoadAll()             → Use Addressables
❌ PlayerPrefs (for game data)     → Use JSON/binary save system
❌ Application.LoadLevel()         → Use SceneManager.LoadSceneAsync()
❌ Application.loadedLevel         → Use SceneManager.GetActiveScene()
❌ renderer.material               → Use renderer.sharedMaterial or MaterialPropertyBlock
❌ camera.main (uncached)          → Cache Camera.main reference in Awake
```

### 1.2 Required API Replacements

| Deprecated / Bad Practice | Correct Unity 6 Alternative |
|--------------------------|----------------------------|
| `GetComponent<T>()` | `TryGetComponent(out T component)` |
| `FindObjectOfType<T>()` | Dependency injection or `[SerializeField]` |
| `Input.GetKey*()` | `InputAction` from Input System |
| `OnGUI()` | UI Toolkit `VisualElement` or TextMeshPro |
| `Resources.Load()` | `Addressables.LoadAssetAsync<T>()` |
| `Instantiate()` in loops | Object Pooling with `ObjectPool<T>` |
| `Destroy()` pooled objects | Return to pool via `ObjectPool<T>.Release()` |
| `Camera.main` (repeated) | Cache in `Awake()`: `_mainCamera = Camera.main;` |
| `new Material()` at runtime | `MaterialPropertyBlock` |
| `string` concatenation in Update | `StringBuilder` or cached strings |
| `foreach` on non-List collections in hot path | `for` loop with cached `.Count` |
| `StartCoroutine(string)` | `StartCoroutine(IEnumerator)` |
| `Invoke(string)` | Coroutine or async/await |
| `tag == "string"` | `CompareTag("string")` |

### 1.3 Unity 6 Specific Features to Use

```csharp
// ✅ Use Awaitable instead of Coroutines where appropriate (Unity 6 feature)
private async Awaitable FadeOutAsync(CancellationToken token)
{
    float elapsed = 0f;
    while (elapsed < _fadeDuration)
    {
        token.ThrowIfCancellationRequested();
        elapsed += Time.deltaTime;
        _canvasGroup.alpha = 1f - (elapsed / _fadeDuration);
        await Awaitable.NextFrameAsync(token);
    }
}

// ✅ Use ObjectPool<T> from UnityEngine.Pool
private ObjectPool<GameObject> _bulletPool;

private void Awake()
{
    _bulletPool = new ObjectPool<GameObject>(
        createFunc: () => Instantiate(_bulletPrefab),
        actionOnGet: obj => obj.SetActive(true),
        actionOnRelease: obj => obj.SetActive(false),
        actionOnDestroy: obj => Destroy(obj),
        collectionCheck: false,
        defaultCapacity: 20,
        maxSize: 100
    );
}

// ✅ Use TryGetComponent
if (collision.gameObject.TryGetComponent(out IDamageable damageable))
{
    damageable.TakeDamage(_damage);
}
```

---

## 2. Input System Rules

### 2.1 Mandatory Input System Usage

ALL input handling MUST use the **New Input System**. Never reference `UnityEngine.Input`.

```csharp
// ✅ CORRECT: Input System with InputAction
using UnityEngine.InputSystem;

[SerializeField] private InputActionReference _moveAction;
[SerializeField] private InputActionReference _jumpAction;

private void OnEnable()
{
    _moveAction.action.Enable();
    _jumpAction.action.Enable();
    _jumpAction.action.performed += OnJumpPerformed;
}

private void OnDisable()
{
    _jumpAction.action.performed -= OnJumpPerformed;
    _moveAction.action.Disable();
    _jumpAction.action.Disable();
}

private void OnJumpPerformed(InputAction.CallbackContext context)
{
    HandleJump();
}

// Reading continuous input
private void Update()
{
    Vector2 moveInput = _moveAction.action.ReadValue<Vector2>();
    HandleMovement(moveInput);
}
```

### 2.2 Input Action Asset Rules

- Create one `InputActionAsset` per project
- Organize actions into Action Maps: `Player`, `UI`, `Vehicle`, `Dialogue`
- Use `InputActionReference` for Inspector binding
- Enable/Disable action maps when switching contexts
- Support multiple control schemes: `Keyboard&Mouse`, `Gamepad`, `Touch`

---

## 3. Rendering Rules (URP 2D)

### 3.1 URP Configuration

- Use **Universal Render Pipeline** with 2D Renderer
- Configure Renderer2D Data asset properly
- Use **Sprite-Lit-Default** or **Sprite-Unlit-Default** shaders
- Use **2D Lights** (Light2D) for dynamic lighting
- Use **Shadow Caster 2D** for 2D shadows
- Configure **Sorting Layers** properly for depth ordering
- Use **Sprite Atlas** for batching optimization

### 3.2 Sorting Layer Guidelines

```
Sorting Layers (back to front):
├── Background          (Parallax backgrounds, sky)
├── BackgroundDetail    (Background decorations)
├── Midground           (Mid-layer parallax)
├── Environment         (Tilemap ground, walls)
├── EnvironmentDetail   (Decorations on environment)
├── Interactable        (Pickups, switches, doors)
├── Enemy               (Enemy sprites)
├── Player              (Player sprite)
├── Foreground          (Foreground decorations)
├── ForegroundDetail    (Close foreground)
├── Particles           (Particle effects)
├── UI_World            (World-space UI elements)
```

---

## 4. Cinemachine Rules

### 4.1 Camera Setup

```csharp
// Use Cinemachine for all camera behavior
// Unity 6 uses the updated Cinemachine 3.x namespace:
using Unity.Cinemachine;

// ✅ Use CinemachineCamera (Unity 6 / Cinemachine 3.x)
// ❌ Do NOT use CinemachineVirtualCamera (legacy Cinemachine 2.x)
```

### 4.2 2D Camera Configuration

- Use `CinemachineCamera` with `CinemachinePositionComposer` for 2D follow
- Use `CinemachineConfiner2D` for camera bounds
- Use `CinemachineImpulseSource` for screen shake
- Set `Orthographic` projection on the brain camera
- Configure dead zone and soft zone for smooth follow

---

## 5. Tilemap Rules

### 5.1 Tilemap Organization

```
Tilemaps (as child GameObjects of Grid):
├── Ground              (Main walkable terrain)
├── Walls               (Collision walls)
├── Platforms           (One-way platforms)
├── Hazards             (Spike tiles, lava)
├── Background          (Decorative background tiles)
├── Foreground          (Decorative foreground tiles)
```

### 5.2 Tilemap Code Rules

- Use `Tilemap` component reference, not string-based lookups
- Use `TilemapCollider2D` with `CompositeCollider2D` for performance
- Use `RuleTile` or `AnimatedTile` for intelligent tile placement
- Cache `Tilemap` references in `Awake()`
- Use `WorldToCell()` and `CellToWorld()` for coordinate conversion

---

## 6. Physics 2D Rules

### 6.1 Rigidbody2D Configuration

- Set `Body Type` appropriately: `Dynamic`, `Kinematic`, or `Static`
- Set `Interpolation` to `Interpolate` for smooth visual movement
- Set `Collision Detection` to `Continuous` for fast-moving objects
- Use `Gravity Scale` instead of custom gravity in most cases
- Freeze rotation Z for side-scrollers: `constraints = RigidbodyConstraints2D.FreezeRotation`

### 6.2 Physics Code Rules

```csharp
// ✅ CORRECT: Physics in FixedUpdate
private void FixedUpdate()
{
    ApplyMovement();
    ApplyGravityModifiers();
}

// ✅ CORRECT: Use Rigidbody2D.linearVelocity in Unity 6
_rigidbody2D.linearVelocity = new Vector2(moveSpeed, _rigidbody2D.linearVelocity.y);

// ❌ WRONG in Unity 6: .velocity is deprecated, use .linearVelocity
// _rigidbody2D.velocity = new Vector2(moveSpeed, _rigidbody2D.velocity.y);

// ✅ CORRECT: Ground check with OverlapCircle
private bool IsGrounded()
{
    return Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
}

// ✅ CORRECT: Raycast with layer mask
private bool CheckWall(Vector2 direction)
{
    RaycastHit2D hit = Physics2D.Raycast(
        transform.position,
        direction,
        _wallCheckDistance,
        _wallLayer
    );
    return hit.collider != null;
}
```

### 6.3 Collision Handling

```csharp
// ✅ CORRECT: Use interfaces for collision response
private void OnCollisionEnter2D(Collision2D collision)
{
    if (collision.gameObject.TryGetComponent(out IDamageable damageable))
    {
        damageable.TakeDamage(_contactDamage);
    }
}

private void OnTriggerEnter2D(Collider2D other)
{
    if (other.TryGetComponent(out ICollectible collectible))
    {
        collectible.Collect(gameObject);
    }
}
```

---

## 7. Addressables Rules

### 7.1 Asset Loading

```csharp
// ✅ CORRECT: Async loading with Addressables
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

private async Awaitable<T> LoadAssetAsync<T>(string address) where T : Object
{
    AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
    await handle.Task;

    if (handle.Status == AsyncOperationStatus.Succeeded)
    {
        return handle.Result;
    }

    Debug.LogError($"Failed to load asset: {address}");
    return null;
}

// ✅ CORRECT: Release when done
private void OnDestroy()
{
    if (_loadedHandle.IsValid())
    {
        Addressables.Release(_loadedHandle);
    }
}
```

### 7.2 Addressables Organization

- Group assets by usage pattern (scenes, characters, UI, audio)
- Use labels for cross-group queries
- Pre-download critical assets during loading screens
- Release handles when assets are no longer needed
- Use `AssetReference` for Inspector-assignable Addressable references

---

## 8. Localization Rules

### 8.1 Text Localization

```csharp
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

// ✅ CORRECT: Use LocalizedString for all user-facing text
[SerializeField] private LocalizedString _localizedTitle;
[SerializeField] private LocalizedString _localizedDescription;

private async void Start()
{
    string title = await _localizedTitle.GetLocalizedStringAsync().Task;
    _titleText.text = title;
}
```

### 8.2 Localization Rules

- NEVER hardcode user-facing strings in scripts
- Use `LocalizedString` for all text displayed to users
- Use String Tables in the Localization package
- Support at minimum: English, Turkish (for this project)
- Use Smart Strings for parameterized text
- Test with longest expected translation to verify UI layout

---

## 9. TextMeshPro Rules

### 9.1 Usage

```csharp
using TMPro;

// ✅ CORRECT: Use TMP_Text as base type for flexibility
[SerializeField] private TMP_Text _scoreText;

// ✅ CORRECT: Use SetText to avoid GC allocation
_scoreText.SetText("Score: {0}", score);

// ❌ WRONG: String concatenation causes GC allocation
// _scoreText.text = "Score: " + score.ToString();
```

### 9.2 TMP Best Practices

- Use `TMP_Text` as the field type (works with both `TextMeshPro` and `TextMeshProUGUI`)
- Use `SetText()` with format parameters to avoid string allocations
- Pre-create TMP font assets for all needed character sets
- Use Font Asset fallbacks for multi-language support
- Use Rich Text tags for inline formatting

---

## 10. Assembly Definition Rules

### 10.1 Required Assembly Definitions

```
Assembly Structure:
├── GameName.Runtime.asmdef              (Main game code)
│   ├── GameName.Runtime.Core.asmdef     (Core systems, no gameplay deps)
│   ├── GameName.Runtime.Gameplay.asmdef (Gameplay systems)
│   ├── GameName.Runtime.UI.asmdef       (UI systems)
│   └── GameName.Runtime.Systems.asmdef  (Shared systems)
├── GameName.Editor.asmdef               (Editor tools and inspectors)
└── GameName.Tests.asmdef                (Unit and integration tests)
```

### 10.2 Assembly Definition Rules

- Every folder under `Scripts/` MUST have an Assembly Definition
- Dependencies must flow downward: Gameplay → Core, UI → Core
- Core assembly MUST NOT reference Gameplay or UI assemblies
- Editor assembly references Runtime but is excluded from builds
- Test assembly references Runtime for testing
- Use `Auto Referenced: false` for non-root assemblies to enforce explicit dependencies

---

## 11. Scene Management Rules

### 11.1 Scene Structure

```
Required Scenes:
├── Bootstrap          (Entry point, initializes core services)
├── MainMenu           (Title screen and menus)
├── Loading            (Loading screen, async operations)
├── Gameplay           (Main game scene, loaded additively)
└── [Level_XX]         (Individual level scenes, loaded additively)
```

### 11.2 Scene Loading

```csharp
// ✅ CORRECT: Async scene loading
using UnityEngine.SceneManagement;

public async Awaitable LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
{
    AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);
    operation.allowSceneActivation = false;

    while (operation.progress < 0.9f)
    {
        OnLoadingProgress?.Invoke(operation.progress);
        await Awaitable.NextFrameAsync();
    }

    operation.allowSceneActivation = true;
    await Awaitable.NextFrameAsync();
    OnSceneLoaded?.Invoke(sceneName);
}
```

### 11.3 Bootstrap Pattern

- Use a `Bootstrap` scene as the entry point (Build Index 0)
- Initialize all core services in Bootstrap
- Never assume a specific scene is loaded first during development
- Use additive scene loading for gameplay + UI layering

---

## 12. Serialization Rules

### 12.1 Save Data

- Use JSON serialization (`JsonUtility` or `Newtonsoft.Json`) for save data
- Save to `Application.persistentDataPath`
- Implement save data versioning for forward compatibility
- Encrypt sensitive save data if needed
- Never use `PlayerPrefs` for game state (only for non-critical preferences like volume)

### 12.2 ScriptableObject Serialization

- Use `[CreateAssetMenu]` attribute for easy creation
- Use `[System.Serializable]` for nested data classes
- Prefer flat data structures over deep nesting
- Use `ScriptableObject` for design-time data, not runtime state

---

## 13. MonoBehaviour Lifecycle Rules

### 13.1 Lifecycle Method Usage

```
Awake()       → Self-initialization. Cache own components. Set up internal state.
              → Called even if the component is disabled.
              → Order: DO NOT depend on other objects being initialized.

OnEnable()    → Subscribe to events. Enable input actions.
              → Called after Awake() and each time the object is re-enabled.

Start()       → Cross-object initialization. Find references that depend on other objects.
              → Called only once, only if the component is enabled.

FixedUpdate() → ONLY physics calculations. Rigidbody2D movement. Force application.
              → Fixed timestep. Do NOT read input here.

Update()      → Input reading. Non-physics game logic. Timer counting.
              → Variable timestep. Minimize work here.

LateUpdate()  → Camera follow. Any logic that must run after all Update() calls.

OnDisable()   → Unsubscribe from events. Disable input actions.
              → Called when the component is disabled or the object is destroyed.

OnDestroy()   → Final cleanup. Release Addressable handles. Dispose native collections.
```

### 13.2 Anti-Patterns

```csharp
// ❌ WRONG: Heavy work in Update
private void Update()
{
    var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None); // GC + slow
    foreach (var enemy in enemies) { /* ... */ }
}

// ✅ CORRECT: Event-driven
private void OnEnable()
{
    EnemySpawner.OnEnemySpawned += HandleEnemySpawned;
    EnemySpawner.OnEnemyDespawned += HandleEnemyDespawned;
}
```
