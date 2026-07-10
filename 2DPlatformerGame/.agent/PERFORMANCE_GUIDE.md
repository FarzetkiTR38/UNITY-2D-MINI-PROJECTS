# Performance Guide

## Overview

This document defines mandatory performance optimization rules for Unity 6 2D game development. Every code change must consider CPU, GPU, memory, and GC impact. Follow these rules to maintain consistent frame rates across all target platforms.

---

## 1. Garbage Collection (GC) Avoidance

### 1.1 Zero-Allocation Hot Path Rule

**CRITICAL**: `Update()`, `FixedUpdate()`, `LateUpdate()`, and any method called every frame MUST produce ZERO GC allocations.

#### Banned in Hot Paths

```csharp
// ❌ String concatenation
string label = "Score: " + score; // Allocates new string

// ❌ String.Format
string label = string.Format("HP: {0}/{1}", current, max); // Allocates

// ❌ Boxing value types
object boxed = 42; // Boxes int to heap
Debug.Log(transform.position); // Boxes Vector3

// ❌ LINQ queries
var closest = enemies.Where(e => e.IsAlive).OrderBy(e => e.Distance).First();

// ❌ Lambda allocations
list.ForEach(item => item.Process());
list.Sort((a, b) => a.Priority.CompareTo(b.Priority)); // Allocates delegate

// ❌ yield return new
yield return new WaitForSeconds(1f); // Allocates every call

// ❌ foreach on non-optimized collections
foreach (var kvp in dictionary) { } // Allocates enumerator for Dictionary

// ❌ Params arrays
void Log(params object[] args) { } // Allocates array

// ❌ Closures over local variables
int threshold = 10;
list.RemoveAll(x => x.Value < threshold); // Closure allocates
```

#### Safe Alternatives

```csharp
// ✅ TMP SetText (no allocation)
_scoreText.SetText("Score: {0}", score);

// ✅ StringBuilder for complex strings (reuse the builder)
private readonly StringBuilder _sb = new(128);
_sb.Clear();
_sb.Append("HP: ").Append(current).Append('/').Append(max);
_text.text = _sb.ToString();

// ✅ Cached WaitForSeconds
private static readonly WaitForSeconds WaitHalf = new(0.5f);
yield return WaitHalf;

// ✅ For loop with cached count
int count = _enemies.Count;
for (int i = 0; i < count; i++)
{
    _enemies[i].Process();
}

// ✅ Non-allocating comparison
private static readonly Comparison<Enemy> CompareByDistance =
    (a, b) => a.Distance.CompareTo(b.Distance);
_enemies.Sort(CompareByDistance);

// ✅ Cached delegates
private Action<int> _cachedHandler;
private void Awake() { _cachedHandler = HandleEvent; }
```

### 1.2 Common Allocation Sources and Fixes

| Source | Allocation | Fix |
|--------|-----------|-----|
| `string + string` | New string on heap | Use `StringBuilder` or `TMP_Text.SetText` |
| `new WaitForSeconds()` | New object each call | Cache as `static readonly` |
| `foreach (Dictionary)` | Enumerator allocation | Use `for` loop or `.Keys`/`.Values` with index |
| `LINQ` | Multiple enumerators | Use manual loops |
| `GetComponent<T>()` every frame | Internal boxing | Cache in `Awake()` |
| `Camera.main` | Internal `FindObjectOfType` | Cache in `Awake()` |
| `tag == "Player"` | String comparison (alloc) | Use `CompareTag("Player")` |
| `Instantiate()` / `Destroy()` | Object creation/GC | Use `ObjectPool<T>` |
| `new List<T>()` in methods | List allocation | Reuse static/field lists |
| `Debug.Log()` with args | String formatting | Wrap in `#if UNITY_EDITOR` or `[Conditional]` |

---

## 2. Object Pooling

### 2.1 When to Pool

Pool ANY object that is:
- Created and destroyed frequently (projectiles, VFX, enemies, pickups)
- Created during gameplay (not scene setup)
- A prefab instantiation

### 2.2 Unity ObjectPool<T>

```csharp
using UnityEngine;
using UnityEngine.Pool;

public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private Projectile _prefab;
    [SerializeField, Min(1)] private int _defaultCapacity = 20;
    [SerializeField, Min(1)] private int _maxSize = 100;

    private ObjectPool<Projectile> _pool;

    private void Awake()
    {
        _pool = new ObjectPool<Projectile>(
            createFunc: CreateProjectile,
            actionOnGet: OnGetProjectile,
            actionOnRelease: OnReleaseProjectile,
            actionOnDestroy: OnDestroyProjectile,
            collectionCheck: false,
            defaultCapacity: _defaultCapacity,
            maxSize: _maxSize
        );
    }

    public Projectile Get(Vector2 position, Vector2 direction)
    {
        Projectile proj = _pool.Get();
        proj.transform.position = position;
        proj.Initialize(direction, _pool);
        return proj;
    }

    public void Return(Projectile projectile) => _pool.Release(projectile);

    private Projectile CreateProjectile()
    {
        Projectile proj = Instantiate(_prefab);
        proj.gameObject.SetActive(false);
        return proj;
    }

    private void OnGetProjectile(Projectile proj) => proj.gameObject.SetActive(true);
    private void OnReleaseProjectile(Projectile proj) => proj.gameObject.SetActive(false);
    private void OnDestroyProjectile(Projectile proj) => Destroy(proj.gameObject);
}
```

### 2.3 Pooling Guidelines

| Rule | Details |
|------|---------|
| Pre-warm pools | Instantiate default capacity during loading |
| Set max pool size | Prevent unbounded memory growth |
| Disable on release | `SetActive(false)` — don't destroy |
| Reset state on get | Clear velocity, reset position, restore health |
| Pool VFX particles | Use `ParticleSystem.Stop()` + return to pool |
| Pool audio sources | Reuse `AudioSource` components |

---

## 3. Physics Optimization

### 3.1 2D Physics Rules

```csharp
// ✅ Use layer-based collision matrix
// Configure in: Edit → Project Settings → Physics 2D → Layer Collision Matrix
// Disable unnecessary collision pairs

// ✅ Use OverlapCircle/BoxCast non-alloc variants
private readonly RaycastHit2D[] _hitBuffer = new RaycastHit2D[10];
private readonly Collider2D[] _overlapBuffer = new Collider2D[10];

private int CheckEnemiesInRange(Vector2 center, float radius, LayerMask mask)
{
    return Physics2D.OverlapCircleNonAlloc(center, radius, _overlapBuffer, mask);
}

private int CastRay(Vector2 origin, Vector2 direction, float distance, LayerMask mask)
{
    return Physics2D.RaycastNonAlloc(origin, direction, _hitBuffer, distance, mask);
}

// ✅ Use CompositeCollider2D with Tilemap
// TilemapCollider2D → Enable "Used By Composite"
// Add CompositeCollider2D to same GameObject
// This merges individual tile colliders into optimized shapes
```

### 3.2 Rigidbody2D Rules

| Rule | Details |
|------|---------|
| Use Kinematic for non-physics entities | NPCs, platforms that move via script |
| Use Static for immovable objects | Walls, ground tilemap |
| Disable Simulated when inactive | Sleeping bodies still cost CPU |
| Use Collision Detection: Discrete | Use Continuous only for very fast objects |
| Freeze rotation Z | For most 2D characters |
| Set reasonable mass values | Don't use extreme mass ratios |

---

## 4. Rendering Optimization

### 4.1 Sprite Atlas

```
Rules:
1. Create Sprite Atlases per category:
   - Characters_Atlas (player + enemy sprites)
   - Environment_Atlas (tilemap + background sprites)
   - UI_Atlas (all UI sprites)
   - VFX_Atlas (particle + effect sprites)

2. Atlas settings:
   - Max Texture Size: 2048 or 4096
   - Padding: 2 (prevents bleeding)
   - Allow Rotation: false (prevents visual issues in 2D)
   - Tight Packing: true (saves atlas space)

3. Include in Addressable Groups for on-demand loading
```

### 4.2 Rendering Rules

| Rule | Details |
|------|---------|
| Use Sorting Layers | Proper layering avoids overdraw |
| Minimize overlapping sprites | Overlapping transparent sprites cause overdraw |
| Use Sprite Atlas | Batch draw calls for same-atlas sprites |
| Disable unused cameras | Each camera costs a full render pass |
| Use pixel-perfect for pixel art | `PixelPerfectCamera` component |
| Limit 2D Lights | Each Light2D adds render passes |
| Avoid runtime material creation | Use `MaterialPropertyBlock` |
| Set proper sprite Pixels Per Unit | Consistent scale, no runtime resizing |

### 4.3 Material and Shader Rules

```csharp
// ❌ WRONG: Creating runtime materials (memory leak)
renderer.material.color = Color.red; // Creates a material instance!

// ✅ CORRECT: Use MaterialPropertyBlock (no allocation, no leak)
private MaterialPropertyBlock _mpb;
private static readonly int ColorID = Shader.PropertyToID("_Color");

private void Awake()
{
    _mpb = new MaterialPropertyBlock();
}

private void SetColor(Color color)
{
    _spriteRenderer.GetPropertyBlock(_mpb);
    _mpb.SetColor(ColorID, color);
    _spriteRenderer.SetPropertyBlock(_mpb);
}
```

---

## 5. Memory Optimization

### 5.1 Texture Import Settings

| Setting | Pixel Art | HD 2D Art |
|---------|-----------|-----------|
| Filter Mode | Point | Bilinear |
| Compression | None | Crunch (quality 75-100) |
| Generate Mip Maps | OFF | OFF (2D games) |
| Max Size | 1024-2048 | 2048-4096 |
| Read/Write | OFF | OFF |
| sRGB | ON | ON |

### 5.2 Audio Memory Rules

| Audio Type | Load Type | Compression | Quality |
|-----------|-----------|-------------|---------|
| Music (long) | Streaming | Vorbis | 70% |
| SFX (short, <1s) | Decompress On Load | PCM / ADPCM | N/A |
| SFX (medium, 1-5s) | Compressed In Memory | Vorbis | 85% |
| Ambient (long) | Compressed In Memory | Vorbis | 70% |

### 5.3 Asset Loading

```csharp
// ✅ Use Addressables for on-demand loading
// Load when entering a level, unload when leaving
private AsyncOperationHandle<GameObject> _levelHandle;

private async Awaitable LoadLevelAssetsAsync(string levelAddress)
{
    _levelHandle = Addressables.LoadAssetAsync<GameObject>(levelAddress);
    await _levelHandle.Task;
}

private void UnloadLevelAssets()
{
    if (_levelHandle.IsValid())
    {
        Addressables.Release(_levelHandle);
    }
}
```

---

## 6. CPU Optimization

### 6.1 Update Optimization

```csharp
// ❌ WRONG: Heavy computation every frame
private void Update()
{
    FindClosestEnemy(); // O(n) every frame
    RecalculatePath();  // Expensive pathfinding every frame
}

// ✅ CORRECT: Stagger expensive operations
private const float PathUpdateInterval = 0.25f;
private float _pathUpdateTimer;

private void Update()
{
    _pathUpdateTimer -= Time.deltaTime;
    if (_pathUpdateTimer <= 0f)
    {
        _pathUpdateTimer = PathUpdateInterval;
        RecalculatePath();
    }
}

// ✅ CORRECT: Use events instead of polling
private void OnEnable()
{
    TargetTracker.OnTargetChanged += HandleTargetChanged;
}

private void HandleTargetChanged(Transform newTarget)
{
    _currentTarget = newTarget;
    RecalculatePath();
}
```

### 6.2 Caching Rules

```csharp
// ✅ Cache component references
private Transform _cachedTransform;
private Rigidbody2D _rb;
private SpriteRenderer _sr;

private void Awake()
{
    _cachedTransform = transform; // Cache transform reference
    _rb = GetComponent<Rigidbody2D>();
    _sr = GetComponent<SpriteRenderer>();
}

// ✅ Cache Camera.main
private Camera _mainCamera;
private void Awake()
{
    _mainCamera = Camera.main;
}

// ✅ Cache Animator string hashes
private static readonly int AnimSpeed = Animator.StringToHash("Speed");
private static readonly int AnimJump = Animator.StringToHash("Jump");

// ✅ Cache Shader property IDs
private static readonly int ShaderColor = Shader.PropertyToID("_Color");
private static readonly int ShaderAlpha = Shader.PropertyToID("_Alpha");

// ✅ Cache frequently used math
private float _inverseMoveSpeed; // 1f / _moveSpeed, calculated once
```

### 6.3 Collection Best Practices

```csharp
// ✅ Pre-allocate collections with expected capacity
private readonly List<Enemy> _activeEnemies = new(32);
private readonly Dictionary<string, ItemData> _itemLookup = new(64);
private readonly HashSet<int> _visitedNodes = new(128);
private readonly Queue<ICommand> _commandQueue = new(16);

// ✅ Use array for fixed-size buffers
private readonly RaycastHit2D[] _raycastBuffer = new RaycastHit2D[8];
private readonly Collider2D[] _overlapBuffer = new Collider2D[16];

// ✅ Clear and reuse instead of creating new
_activeEnemies.Clear(); // Reuse the list
// NOT: _activeEnemies = new List<Enemy>(); // Creates garbage
```

---

## 7. Build Size Optimization

| Technique | Impact |
|-----------|--------|
| Use Sprite Atlas | Reduces draw calls and texture memory |
| Compress textures (Crunch) | 50-80% size reduction |
| Strip unused code | Player Settings → Managed Stripping Level: Medium/High |
| Use Addressables | Load assets on demand, not all at startup |
| Remove unused packages | Packages → Remove unused dependencies |
| Compress audio (Vorbis) | Significant size reduction for music |
| Use ADPCM for short SFX | Good compression for short clips |
| Limit font character sets | TMP: Only include needed Unicode ranges |
| Optimize mesh/sprite complexity | Fewer vertices = smaller builds |

---

## 8. Profiling Checklist

### 8.1 Unity Profiler Usage

```
Before submitting code, verify:

1. Open Profiler: Window → Analysis → Profiler
2. Play the game and monitor:
   - CPU Usage: Frame time should be <16.6ms (60 FPS)
   - GC Alloc: Should be 0 bytes in gameplay frames
   - Rendering: Draw calls and batch count
   - Physics 2D: Collision check time
   - Memory: Total used memory and peak

3. Check for:
   □ GC spikes (GC.Alloc column > 0 in hot frames)
   □ CPU spikes (frame time > 16.6ms)
   □ Excessive draw calls (> 100 for mobile)
   □ Memory leaks (steadily increasing memory)
   □ Physics overhead (too many active colliders)
```

### 8.2 Performance Targets

| Metric | Mobile | Desktop |
|--------|--------|---------|
| Target FPS | 60 | 60+ |
| Frame Budget | 16.6ms | 16.6ms |
| GC Alloc/Frame | 0 bytes | 0 bytes |
| Draw Calls | < 100 | < 200 |
| SetPass Calls | < 30 | < 50 |
| Active Rigidbody2D | < 100 | < 200 |
| Memory (Total) | < 512MB | < 1GB |
| Audio Channels | < 32 | < 64 |

---

## 9. Debug Logging Performance

```csharp
// ❌ WRONG: Debug.Log in builds (string allocation + IO)
private void Update()
{
    Debug.Log($"Position: {transform.position}"); // Allocates every frame!
}

// ✅ CORRECT: Conditional compilation
#if UNITY_EDITOR
    Debug.Log($"Position: {transform.position}");
#endif

// ✅ CORRECT: Use [Conditional] attribute for debug methods
[System.Diagnostics.Conditional("UNITY_EDITOR")]
private static void DebugLog(string message)
{
    Debug.Log(message);
}

// ✅ CORRECT: Use Debug.unityLogger.logEnabled
// Set to false in release builds via bootstrap
#if !UNITY_EDITOR
    Debug.unityLogger.logEnabled = false;
#endif
```
