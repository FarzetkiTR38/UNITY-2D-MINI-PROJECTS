# Common Mistakes and Anti-Patterns

## Overview

This document catalogs the most common mistakes and anti-patterns in Unity 2D development. Every item includes the mistake, why it's problematic, and the correct alternative. Agents MUST avoid every pattern listed here.

---

## 1. Missing References

### ❌ MISTAKE: Not validating Inspector references

```csharp
// Runtime NullReferenceException because designer forgot to assign
[SerializeField] private Transform _spawnPoint;

private void Start()
{
    Instantiate(_prefab, _spawnPoint.position, Quaternion.identity); // CRASH!
}
```

### ✅ FIX: Validate in OnValidate and guard in runtime

```csharp
[SerializeField] private Transform _spawnPoint;

private void OnValidate()
{
    if (_spawnPoint == null)
    {
        Debug.LogWarning($"[{name}] SpawnPoint is not assigned!", this);
    }
}

private void SpawnEnemy()
{
    if (_spawnPoint == null)
    {
        Debug.LogError($"[{name}] Cannot spawn: SpawnPoint is null!", this);
        return;
    }

    Instantiate(_prefab, _spawnPoint.position, Quaternion.identity);
}
```

---

## 2. NullReferenceException

### ❌ MISTAKE: Assuming GetComponent always succeeds

```csharp
private void OnCollisionEnter2D(Collision2D collision)
{
    collision.gameObject.GetComponent<IDamageable>().TakeDamage(10); // CRASH if no IDamageable!
}
```

### ✅ FIX: Use TryGetComponent

```csharp
private void OnCollisionEnter2D(Collision2D collision)
{
    if (collision.gameObject.TryGetComponent(out IDamageable damageable))
    {
        damageable.TakeDamage(10);
    }
}
```

---

## 3. Circular Dependencies

### ❌ MISTAKE: Systems directly reference each other

```csharp
public class Player { public Enemy Target; }
public class Enemy { public Player TargetPlayer; }
// Circular reference, tight coupling, untestable
```

### ✅ FIX: Use events or interfaces

```csharp
// Player and Enemy don't know about each other
// Both implement IDamageable
// Communication through EventChannels or collision callbacks
public interface IDamageable
{
    bool TakeDamage(int amount, GameObject source = null);
}
```

---

## 4. FindObjectOfType Usage

### ❌ MISTAKE: Using FindObjectOfType for references

```csharp
private void Start()
{
    _player = FindObjectOfType<PlayerController>(); // SLOW! Every frame search
    _audioManager = FindObjectOfType<AudioManager>(); // Creates dependency on scene
}
```

### ✅ FIX: Use SerializeField, events, or ServiceLocator

```csharp
// Option 1: SerializeField
[SerializeField] private PlayerController _player;

// Option 2: ServiceLocator
private void Start()
{
    _audioManager = ServiceLocator.Get<IAudioService>();
}

// Option 3: Event-driven (player doesn't need reference to audio)
[SerializeField] private VoidEventChannel _onPlayerJumped;
// AudioManager subscribes to this event independently
```

---

## 5. Resources Folder Usage

### ❌ MISTAKE: Using Resources.Load

```csharp
private void LoadEnemy()
{
    var prefab = Resources.Load<GameObject>("Prefabs/Enemies/Slime"); // BAD!
    // Problems: Hardcoded path, included in build even if unused, no async loading
}
```

### ✅ FIX: Use Addressables or SerializeField

```csharp
// Option 1: SerializeField (simplest)
[SerializeField] private GameObject _slimePrefab;

// Option 2: Addressables (for dynamic loading)
[SerializeField] private AssetReference _slimeReference;

private async void LoadEnemy()
{
    var handle = _slimeReference.LoadAssetAsync<GameObject>();
    await handle.Task;
    if (handle.Status == AsyncOperationStatus.Succeeded)
    {
        Instantiate(handle.Result);
    }
}
```

---

## 6. Singleton Overuse

### ❌ MISTAKE: Making everything a Singleton

```csharp
// 15 Singletons in one project = global state nightmare
GameManager.Instance.ScoreManager.Instance.AddScore(100);
AudioManager.Instance.PlaySFX(clip);
UIManager.Instance.ShowPopup("text");
InventoryManager.Instance.AddItem(item);
// Untestable, coupled, order-dependent initialization
```

### ✅ FIX: ServiceLocator for services, events for communication

```csharp
// Register services once in Bootstrap
ServiceLocator.Register<IAudioService>(audioManager);

// Use events for cross-system communication
[SerializeField] private IntEventChannel _onScoreChanged;
_onScoreChanged.RaiseEvent(100);

// Inject via SerializeField for gameplay components
[SerializeField] private HealthSystem _playerHealth;
```

---

## 7. Update Spam

### ❌ MISTAKE: Doing expensive work every frame

```csharp
private void Update()
{
    // Finding objects every frame (O(n) search, GC allocation)
    var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

    // String operations every frame
    _scoreText.text = "Score: " + _score.ToString();

    // GetComponent every frame
    var rb = GetComponent<Rigidbody2D>();

    // Unnecessary distance checks every frame
    foreach (var enemy in enemies)
    {
        float dist = Vector3.Distance(transform.position, enemy.transform.position);
        // ...
    }
}
```

### ✅ FIX: Cache, events, and staggered updates

```csharp
// Cache in Awake
private Rigidbody2D _rb;
private void Awake() => _rb = GetComponent<Rigidbody2D>();

// Use events instead of polling
private void OnEnable()
{
    _onScoreChanged.OnEventRaised += UpdateScoreDisplay;
}
private void UpdateScoreDisplay(int score)
{
    _scoreText.SetText("Score: {0}", score); // No allocation
}

// Stagger expensive checks
private const float DetectionInterval = 0.25f;
private float _detectionTimer;
private void Update()
{
    _detectionTimer -= Time.deltaTime;
    if (_detectionTimer <= 0f)
    {
        _detectionTimer = DetectionInterval;
        PerformDetection(); // Only every 0.25 seconds
    }
}
```

---

## 8. GC Allocation in Hot Paths

### ❌ MISTAKE: Allocating in Update

```csharp
private void Update()
{
    // String allocation
    _text.text = $"HP: {_health}/{_maxHealth}";

    // LINQ allocation
    var closest = _enemies.OrderBy(e => (e.transform.position - transform.position).sqrMagnitude).First();

    // New allocation
    yield return new WaitForSeconds(0.5f);

    // Lambda allocation
    _enemies.ForEach(e => e.Process());
}
```

### ✅ FIX: Zero-allocation alternatives

```csharp
// TMP SetText (no allocation)
_text.SetText("HP: {0}/{1}", _health, _maxHealth);

// Manual loop (no allocation)
Enemy closest = null;
float closestDist = float.MaxValue;
for (int i = 0; i < _enemies.Count; i++)
{
    float dist = (_enemies[i].transform.position - transform.position).sqrMagnitude;
    if (dist < closestDist)
    {
        closestDist = dist;
        closest = _enemies[i];
    }
}

// Cached WaitForSeconds
private static readonly WaitForSeconds Wait05 = new(0.5f);
yield return Wait05;

// Cached delegate
private static readonly Action<Enemy> ProcessEnemy = e => e.Process();
```

---

## 9. Hardcoded Paths and Values

### ❌ MISTAKE: Hardcoded strings and magic numbers

```csharp
private void Start()
{
    SceneManager.LoadScene("Level1");              // Hardcoded scene name
    var clip = Resources.Load<AudioClip>("Sounds/jump"); // Hardcoded path
    if (health <= 0) { Die(); }                    // Magic number
    transform.position += Vector3.right * 5f;      // Magic number
    if (_combo >= 3) { SpecialAttack(); }           // Magic number
}
```

### ✅ FIX: Constants, SerializeField, and ScriptableObjects

```csharp
// Constants for magic numbers
private const float MinHealth = 0f;
private const float DefaultMoveSpeed = 5f;
private const int ComboThresholdForSpecial = 3;

// SerializeField for scene names
[SerializeField] private string _levelSceneName = "Level_01";

// ScriptableObject for configuration
[SerializeField] private PlayerConfig _config;

private void UseConfig()
{
    float speed = _config.MoveSpeed; // From SO, not hardcoded
}
```

---

## 10. Inspector Dependency Errors

### ❌ MISTAKE: Public fields without validation

```csharp
public Transform spawnPoint;         // Public field, no validation
public float speed;                  // No default, no range, no tooltip
public GameObject enemyPrefab;       // No null check
```

### ✅ FIX: Private fields with attributes and validation

```csharp
[Header("Spawning")]
[Tooltip("Point where enemies spawn.")]
[SerializeField] private Transform _spawnPoint;

[Tooltip("Movement speed in units per second.")]
[SerializeField, Range(0f, 20f)] private float _speed = 5f;

[Tooltip("Enemy prefab to instantiate.")]
[SerializeField] private GameObject _enemyPrefab;

private void OnValidate()
{
    if (_spawnPoint == null)
        Debug.LogWarning($"[{name}] SpawnPoint is not assigned!", this);
    if (_enemyPrefab == null)
        Debug.LogError($"[{name}] EnemyPrefab is required!", this);
}
```

---

## 11. Incorrect MonoBehaviour Lifecycle Usage

### ❌ MISTAKE: Wrong method for wrong purpose

```csharp
// Physics in Update (framerate-dependent, jittery)
private void Update()
{
    _rb.AddForce(Vector2.up * jumpForce);
}

// Camera in Update (jitters because it runs before LateUpdate)
private void Update()
{
    transform.position = _target.position;
}

// Heavy initialization in Awake that depends on other objects
private void Awake()
{
    _player = FindObjectOfType<Player>(); // Other objects may not exist yet
}
```

### ✅ FIX: Correct lifecycle method for each purpose

```csharp
private void Awake()    { /* Self-init only: GetComponent, cache own refs */ }
private void Start()    { /* Cross-object refs: find other initialized objects */ }
private void FixedUpdate() { /* Physics ONLY: AddForce, velocity changes */ }
private void Update()   { /* Input reading, timers, non-physics logic */ }
private void LateUpdate() { /* Camera follow, post-Update adjustments */ }
```

---

## 12. Forgetting Event Unsubscription

### ❌ MISTAKE: Subscribe without unsubscribe

```csharp
private void Start()
{
    _onEnemyKilled.OnEventRaised += HandleEnemyKilled;
    // NEVER unsubscribed! Causes:
    // - Memory leaks
    // - Null reference after object destroyed
    // - Event called on destroyed objects
}
```

### ✅ FIX: Always pair OnEnable/OnDisable

```csharp
private void OnEnable()
{
    _onEnemyKilled.OnEventRaised += HandleEnemyKilled;
}

private void OnDisable()
{
    _onEnemyKilled.OnEventRaised -= HandleEnemyKilled;
}
```

---

## 13. Using velocity Instead of linearVelocity (Unity 6)

### ❌ MISTAKE: Using deprecated Rigidbody2D.velocity

```csharp
// In Unity 6, .velocity is deprecated for Rigidbody2D
_rigidbody2D.velocity = new Vector2(speed, _rigidbody2D.velocity.y);
```

### ✅ FIX: Use linearVelocity in Unity 6

```csharp
_rigidbody2D.linearVelocity = new Vector2(speed, _rigidbody2D.linearVelocity.y);
```

---

## 14. Creating Materials at Runtime

### ❌ MISTAKE: Accessing renderer.material

```csharp
// Creates a material instance — MEMORY LEAK!
_spriteRenderer.material.color = Color.red;

// Even worse in Update:
private void Update()
{
    _spriteRenderer.material.SetFloat("_Alpha", alpha); // New material every frame!
}
```

### ✅ FIX: Use MaterialPropertyBlock

```csharp
private MaterialPropertyBlock _mpb;
private static readonly int AlphaID = Shader.PropertyToID("_Alpha");

private void Awake()
{
    _mpb = new MaterialPropertyBlock();
}

private void SetAlpha(float alpha)
{
    _spriteRenderer.GetPropertyBlock(_mpb);
    _mpb.SetFloat(AlphaID, alpha);
    _spriteRenderer.SetPropertyBlock(_mpb);
}
```

---

## 15. Summary Table

| # | Mistake | Impact | Severity |
|---|---------|--------|----------|
| 1 | Missing reference validation | Runtime crash | 🔴 Critical |
| 2 | No TryGetComponent | NullRef crash | 🔴 Critical |
| 3 | Circular dependencies | Untestable, fragile | 🟠 High |
| 4 | FindObjectOfType | Performance, coupling | 🟠 High |
| 5 | Resources.Load | Build size, no async | 🟡 Medium |
| 6 | Singleton overuse | Global state, untestable | 🟠 High |
| 7 | Update spam | CPU waste, GC | 🔴 Critical |
| 8 | GC in hot paths | Frame drops, stuttering | 🔴 Critical |
| 9 | Hardcoded values | Unmaintainable | 🟡 Medium |
| 10 | Inspector errors | Designer confusion | 🟡 Medium |
| 11 | Wrong lifecycle method | Physics jitter, race conditions | 🟠 High |
| 12 | Missing unsubscribe | Memory leak, crashes | 🔴 Critical |
| 13 | velocity vs linearVelocity | Deprecated API warning | 🟡 Medium |
| 14 | Runtime material creation | Memory leak | 🟠 High |
