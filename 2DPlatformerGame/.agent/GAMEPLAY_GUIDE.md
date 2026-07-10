# 2D Gameplay Systems Guide

## Overview

This document provides implementation guidelines for all major 2D gameplay systems. Each section covers the system's purpose, key components, implementation patterns, and integration points.

---

## 1. Player Controller

### 1.1 Component Composition

```
Player GameObject:
├── PlayerController.cs      — Orchestrates all player systems
├── PlayerMovement.cs         — Horizontal movement, jumping, dashing
├── PlayerCombat.cs           — Melee attacks, projectiles, combos
├── PlayerAnimation.cs        — Animator parameter management
├── PlayerInputHandler.cs     — Reads Input System actions
├── HealthSystem.cs           — Health, damage, death
├── Rigidbody2D               — Physics body
├── CapsuleCollider2D          — Physics collider
├── Animator                   — Animation state machine
└── SpriteRenderer             — Visual rendering
```

### 1.2 Input Handling Pattern

```csharp
// PlayerInputHandler reads input and delegates to systems
public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private InputActionReference _moveAction;
    [SerializeField] private InputActionReference _jumpAction;
    [SerializeField] private InputActionReference _attackAction;
    [SerializeField] private InputActionReference _dashAction;
    [SerializeField] private InputActionReference _interactAction;

    private PlayerMovement _movement;
    private PlayerCombat _combat;
    private InteractionDetector _interactionDetector;

    private void Awake()
    {
        TryGetComponent(out _movement);
        TryGetComponent(out _combat);
        TryGetComponent(out _interactionDetector);
    }

    private void OnEnable()
    {
        _jumpAction.action.performed += ctx => _movement?.RequestJump();
        _attackAction.action.performed += ctx => _combat?.Attack();
        _dashAction.action.performed += ctx => _movement?.RequestDash();
        _interactAction.action.performed += ctx => _interactionDetector?.TryInteract();
        EnableAllActions();
    }

    private void Update()
    {
        Vector2 input = _moveAction.action.ReadValue<Vector2>();
        _movement?.SetMoveInput(input);
    }

    private void OnDisable() => DisableAllActions();

    private void EnableAllActions()
    {
        _moveAction.action.Enable();
        _jumpAction.action.Enable();
        _attackAction.action.Enable();
        _dashAction.action.Enable();
        _interactAction.action.Enable();
    }

    private void DisableAllActions()
    {
        _moveAction.action.Disable();
        _jumpAction.action.Disable();
        _attackAction.action.Disable();
        _dashAction.action.Disable();
        _interactAction.action.Disable();
    }
}
```

### 1.3 Key Movement Features

| Feature | Implementation |
|---------|---------------|
| Coyote Time | Track time since leaving ground; allow jump within window |
| Jump Buffering | Track time since jump pressed; execute when grounded |
| Variable Jump Height | Apply extra gravity when jump button released early |
| Wall Sliding | Reduce gravity when touching wall + holding toward it |
| Wall Jump | Launch away from wall with horizontal + vertical force |
| Dash | Short burst of speed with invincibility frames |
| One-Way Platforms | Use Platform Effector 2D + "drop through" input |

---

## 2. Enemy AI

### 2.1 State Machine Approach

```
Enemy AI States:
├── IdleState        — Standing still, waiting for player
├── PatrolState      — Moving between waypoints
├── ChaseState       — Pursuing the player
├── AttackState      — Performing an attack
├── HurtState        — Reacting to damage
├── DeadState        — Playing death animation, dropping loot
└── StunnedState     — Temporarily incapacitated
```

### 2.2 Component Structure

```
Enemy GameObject:
├── EnemyController.cs        — State machine orchestrator
├── EnemyMovement.cs          — Movement execution
├── EnemyDetection.cs         — Player detection (raycast/overlap)
├── EnemyCombat.cs            — Attack logic
├── HealthSystem.cs           — Shared health component
├── LootDropper.cs            — Drops items on death
├── DamageDealer.cs           — Contact damage
├── EnemyData (SO reference)  — Stats from ScriptableObject
├── Rigidbody2D
├── Collider2D
├── Animator
└── SpriteRenderer
```

### 2.3 Detection System

```csharp
// Non-allocating detection with cached buffers
public class EnemyDetection : MonoBehaviour
{
    [SerializeField] private float _detectionRange = 8f;
    [SerializeField] private float _attackRange = 1.5f;
    [SerializeField] private LayerMask _playerLayer;

    private readonly Collider2D[] _detectionBuffer = new Collider2D[1];
    private Transform _detectedTarget;

    public Transform DetectedTarget => _detectedTarget;
    public bool HasTarget => _detectedTarget != null;
    public bool IsInAttackRange => HasTarget &&
        Vector2.Distance(transform.position, _detectedTarget.position) <= _attackRange;

    // Called on a timer, not every frame
    public void PerformDetection()
    {
        int count = Physics2D.OverlapCircleNonAlloc(
            transform.position, _detectionRange, _detectionBuffer, _playerLayer);

        _detectedTarget = count > 0 ? _detectionBuffer[0].transform : null;
    }
}
```

---

## 3. Combat System

### 3.1 Damage Flow

```
Damage Flow:
1. Attacker calls IDamageable.TakeDamage(amount, source)
2. Target HealthSystem checks invincibility
3. If valid, reduces health and raises OnDamaged event
4. OnDamaged triggers: VFX, SFX, screen shake, UI update
5. If health <= 0, raises OnDied event
6. OnDied triggers: death animation, loot drop, score update

All through events — no direct references between systems.
```

### 3.2 Melee Attack Pattern

```csharp
public class MeleeAttack : MonoBehaviour
{
    [SerializeField] private Transform _attackPoint;
    [SerializeField] private float _attackRadius = 0.8f;
    [SerializeField] private int _damage = 10;
    [SerializeField] private LayerMask _targetLayers;
    [SerializeField] private float _knockbackForce = 5f;

    private readonly Collider2D[] _hitBuffer = new Collider2D[8];

    /// <summary>Performs a melee attack, damaging all targets in range.</summary>
    /// <returns>Number of targets hit.</returns>
    public int PerformAttack()
    {
        int hitCount = Physics2D.OverlapCircleNonAlloc(
            _attackPoint.position, _attackRadius, _hitBuffer, _targetLayers);

        int damageCount = 0;
        for (int i = 0; i < hitCount; i++)
        {
            if (_hitBuffer[i].TryGetComponent(out IDamageable damageable))
            {
                if (damageable.TakeDamage(_damage, gameObject))
                {
                    damageCount++;

                    // Apply knockback
                    if (_hitBuffer[i].TryGetComponent(out Rigidbody2D rb))
                    {
                        Vector2 direction = (_hitBuffer[i].transform.position - transform.position).normalized;
                        rb.AddForce(direction * _knockbackForce, ForceMode2D.Impulse);
                    }
                }
            }
        }

        return damageCount;
    }
}
```

### 3.3 Projectile Pattern

```csharp
public class Projectile : MonoBehaviour, IPoolable
{
    [SerializeField] private float _speed = 15f;
    [SerializeField] private int _damage = 5;
    [SerializeField] private float _lifetime = 3f;
    [SerializeField] private LayerMask _targetLayers;
    [SerializeField] private LayerMask _environmentLayers;

    private Vector2 _direction;
    private float _timer;
    private System.Action<Projectile> _returnToPool;

    public void Initialize(Vector2 direction, System.Action<Projectile> returnToPool)
    {
        _direction = direction.normalized;
        _returnToPool = returnToPool;
        _timer = _lifetime;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Update()
    {
        transform.position += (Vector3)(_direction * _speed * Time.deltaTime);

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & _targetLayers) != 0)
        {
            if (other.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(_damage, gameObject);
            }
            ReturnToPool();
        }
        else if (((1 << other.gameObject.layer) & _environmentLayers) != 0)
        {
            ReturnToPool();
        }
    }

    public void OnGetFromPool() => gameObject.SetActive(true);
    public void OnReturnToPool() => gameObject.SetActive(false);
    private void ReturnToPool() => _returnToPool?.Invoke(this);
}
```

---

## 4. Inventory System

### 4.1 Slot-Based Inventory

```csharp
public class Inventory
{
    private readonly InventorySlot[] _slots;

    public event Action<int> OnSlotChanged;
    public int Capacity => _slots.Length;

    public Inventory(int capacity)
    {
        _slots = new InventorySlot[capacity];
        for (int i = 0; i < capacity; i++)
        {
            _slots[i] = new InventorySlot();
        }
    }

    public bool TryAddItem(ItemData item, int count = 1)
    {
        // First try to stack with existing
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].ItemData == item && _slots[i].Count < item.MaxStackSize)
            {
                int canAdd = Mathf.Min(count, item.MaxStackSize - _slots[i].Count);
                _slots[i].Count += canAdd;
                count -= canAdd;
                OnSlotChanged?.Invoke(i);
                if (count <= 0) return true;
            }
        }

        // Then try empty slots
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].IsEmpty)
            {
                int canAdd = Mathf.Min(count, item.MaxStackSize);
                _slots[i].ItemData = item;
                _slots[i].Count = canAdd;
                count -= canAdd;
                OnSlotChanged?.Invoke(i);
                if (count <= 0) return true;
            }
        }

        return count <= 0;
    }

    public bool TryRemoveItem(ItemData item, int count = 1)
    {
        for (int i = _slots.Length - 1; i >= 0; i--)
        {
            if (_slots[i].ItemData == item)
            {
                int canRemove = Mathf.Min(count, _slots[i].Count);
                _slots[i].Count -= canRemove;
                count -= canRemove;

                if (_slots[i].Count <= 0)
                {
                    _slots[i].Clear();
                }

                OnSlotChanged?.Invoke(i);
                if (count <= 0) return true;
            }
        }

        return count <= 0;
    }

    public InventorySlot GetSlot(int index)
    {
        if (index < 0 || index >= _slots.Length) return null;
        return _slots[index];
    }

    public int GetItemCount(ItemData item)
    {
        int total = 0;
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].ItemData == item)
            {
                total += _slots[i].Count;
            }
        }
        return total;
    }
}

[System.Serializable]
public class InventorySlot
{
    public ItemData ItemData;
    public int Count;
    public bool IsEmpty => ItemData == null || Count <= 0;

    public void Clear()
    {
        ItemData = null;
        Count = 0;
    }
}
```

---

## 5. Save System

### 5.1 Save Architecture

```csharp
// Save data container
[System.Serializable]
public class SaveData
{
    public int Version = 1;
    public string SaveDate;
    public PlayerSaveData Player;
    public ProgressSaveData Progress;
    public SettingsSaveData Settings;
}

[System.Serializable]
public class PlayerSaveData
{
    public int CurrentHealth;
    public int MaxHealth;
    public int Currency;
    public int Experience;
    public int Level;
    public string CurrentScene;
    public float PositionX;
    public float PositionY;
    public List<string> InventoryItemIds;
    public List<int> InventoryItemCounts;
}

[System.Serializable]
public class ProgressSaveData
{
    public List<string> CompletedLevels;
    public List<string> UnlockedAchievements;
    public List<string> CollectedItems;
    public int HighScore;
}

[System.Serializable]
public class SettingsSaveData
{
    public float MasterVolume = 1f;
    public float MusicVolume = 0.7f;
    public float SfxVolume = 1f;
    public int QualityLevel = 2;
    public string Language = "en";
    public bool Fullscreen = true;
}
```

```csharp
// File handler
public static class SaveFileHandler
{
    private const string SaveFileName = "gamesave.json";
    private const string SettingsFileName = "settings.json";

    public static void Save(SaveData data)
    {
        data.SaveDate = System.DateTime.Now.ToString("O");
        string json = JsonUtility.ToJson(data, true);
        string path = System.IO.Path.Combine(Application.persistentDataPath, SaveFileName);
        System.IO.File.WriteAllText(path, json);
    }

    public static SaveData Load()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, SaveFileName);
        if (!System.IO.File.Exists(path)) return new SaveData();

        string json = System.IO.File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static bool SaveExists()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, SaveFileName);
        return System.IO.File.Exists(path);
    }

    public static void DeleteSave()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, SaveFileName);
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
        }
    }
}
```

---

## 6. Audio System

### 6.1 Audio Manager Pattern

```csharp
public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioSource _uiSource;

    [Header("Audio Mixer")]
    [SerializeField] private UnityEngine.Audio.AudioMixer _audioMixer;

    private const string MasterVolumeParam = "MasterVolume";
    private const string MusicVolumeParam = "MusicVolume";
    private const string SfxVolumeParam = "SfxVolume";

    /// <summary>Plays a sound effect one-shot.</summary>
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        _sfxSource.PlayOneShot(clip, volume);
    }

    /// <summary>Plays a UI sound effect.</summary>
    public void PlayUI(AudioClip clip)
    {
        if (clip == null) return;
        _uiSource.PlayOneShot(clip);
    }

    /// <summary>Plays background music with crossfade.</summary>
    public async Awaitable PlayMusicAsync(AudioClip clip, float fadeDuration = 1f)
    {
        if (clip == _musicSource.clip && _musicSource.isPlaying) return;

        // Fade out
        float startVolume = _musicSource.volume;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            await Awaitable.NextFrameAsync();
        }

        // Switch track
        _musicSource.clip = clip;
        _musicSource.Play();

        // Fade in
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _musicSource.volume = Mathf.Lerp(0f, startVolume, elapsed / fadeDuration);
            await Awaitable.NextFrameAsync();
        }
    }

    /// <summary>Sets the volume for an audio group.</summary>
    public void SetVolume(string parameter, float normalizedValue)
    {
        // Convert linear 0-1 to logarithmic dB (-80 to 0)
        float dB = normalizedValue > 0.001f ? Mathf.Log10(normalizedValue) * 20f : -80f;
        _audioMixer.SetFloat(parameter, dB);
    }
}
```

---

## 7. Camera System

### 7.1 Cinemachine 2D Setup

```
Camera Setup:
├── MainCamera (Camera, CinemachineBrain)
│   └── URP Camera settings, Orthographic
└── CinemachineCamera (CinemachineCamera component)
    ├── Follow: Player Transform
    ├── CinemachinePositionComposer (2D framing)
    ├── CinemachineConfiner2D (level bounds)
    └── CinemachineImpulseListener (screen shake)
```

### 7.2 Screen Shake

```csharp
using Unity.Cinemachine;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource _impulseSource;

    public void ShakeLight()
    {
        _impulseSource.GenerateImpulseWithForce(0.3f);
    }

    public void ShakeMedium()
    {
        _impulseSource.GenerateImpulseWithForce(0.6f);
    }

    public void ShakeHeavy()
    {
        _impulseSource.GenerateImpulseWithForce(1.0f);
    }
}
```

---

## 8. Parallax Background

```csharp
public class ParallaxLayer : MonoBehaviour
{
    [Tooltip("Parallax effect strength. 0 = no movement, 1 = moves with camera.")]
    [SerializeField, Range(0f, 1f)] private float _parallaxFactor = 0.5f;

    [Tooltip("Enable infinite scrolling.")]
    [SerializeField] private bool _infiniteScroll;

    private Transform _cameraTransform;
    private Vector3 _previousCameraPosition;
    private float _spriteWidth;

    private void Start()
    {
        _cameraTransform = Camera.main.transform;
        _previousCameraPosition = _cameraTransform.position;

        if (_infiniteScroll && TryGetComponent(out SpriteRenderer sr))
        {
            _spriteWidth = sr.bounds.size.x;
        }
    }

    private void LateUpdate()
    {
        Vector3 delta = _cameraTransform.position - _previousCameraPosition;
        transform.position += new Vector3(delta.x * _parallaxFactor, delta.y * _parallaxFactor, 0f);
        _previousCameraPosition = _cameraTransform.position;

        if (_infiniteScroll && _spriteWidth > 0f)
        {
            float cameraX = _cameraTransform.position.x;
            float layerX = transform.position.x;

            if (cameraX - layerX > _spriteWidth)
            {
                transform.position += Vector3.right * _spriteWidth;
            }
            else if (layerX - cameraX > _spriteWidth)
            {
                transform.position -= Vector3.right * _spriteWidth;
            }
        }
    }
}
```

---

## 9. Status Effects (Buffs/Debuffs)

```csharp
// ScriptableObject-based status effect definition
[CreateAssetMenu(fileName = "NewStatusEffect", menuName = "GameName/Combat/Status Effect")]
public class StatusEffectData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string _effectName;
    [SerializeField] private Sprite _icon;
    [SerializeField, TextArea] private string _description;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float _duration = 5f;
    [SerializeField, Min(0f)] private float _tickInterval = 1f;

    [Header("Effect")]
    [SerializeField] private StatusEffectType _type;
    [SerializeField] private float _value = 5f;
    [SerializeField] private bool _isStackable;
    [SerializeField, Min(1)] private int _maxStacks = 1;

    public string EffectName => _effectName;
    public Sprite Icon => _icon;
    public string Description => _description;
    public float Duration => _duration;
    public float TickInterval => _tickInterval;
    public StatusEffectType Type => _type;
    public float Value => _value;
    public bool IsStackable => _isStackable;
    public int MaxStacks => _maxStacks;
}

public enum StatusEffectType
{
    DamageOverTime = 0,     // Poison, burn
    HealOverTime = 1,       // Regeneration
    SpeedBoost = 2,         // Haste
    SpeedReduction = 3,     // Slow
    DamageBoost = 4,        // Strength
    DamageReduction = 5,    // Armor
    Stun = 6,               // Cannot act
    Invincibility = 7       // Cannot take damage
}
```

---

## 10. Experience and Leveling

```csharp
[CreateAssetMenu(fileName = "ExperienceCurve", menuName = "GameName/Progression/Experience Curve")]
public class ExperienceCurveData : ScriptableObject
{
    [Tooltip("Base XP required for level 2.")]
    [SerializeField, Min(1)] private int _baseXP = 100;

    [Tooltip("XP growth multiplier per level.")]
    [SerializeField, Range(1f, 3f)] private float _growthFactor = 1.5f;

    [Tooltip("Maximum player level.")]
    [SerializeField, Min(1)] private int _maxLevel = 50;

    public int MaxLevel => _maxLevel;

    /// <summary>Calculates XP required for a specific level.</summary>
    public int GetXPForLevel(int level)
    {
        if (level <= 1) return 0;
        return Mathf.RoundToInt(_baseXP * Mathf.Pow(_growthFactor, level - 2));
    }

    /// <summary>Calculates total XP from level 1 to the specified level.</summary>
    public int GetTotalXPForLevel(int level)
    {
        int total = 0;
        for (int i = 2; i <= level; i++)
        {
            total += GetXPForLevel(i);
        }
        return total;
    }

    /// <summary>Determines the level for a given total XP amount.</summary>
    public int GetLevelForXP(int totalXP)
    {
        int accumulated = 0;
        for (int level = 2; level <= _maxLevel; level++)
        {
            accumulated += GetXPForLevel(level);
            if (totalXP < accumulated)
            {
                return level - 1;
            }
        }
        return _maxLevel;
    }
}
```

---

## 11. System Summary

| System | Key Pattern | Key Components |
|--------|------------|----------------|
| Player Controller | Component Composition | InputHandler → Movement, Combat, Animation |
| Enemy AI | State Machine + SO Data | States, Detection, EnemyData SO |
| Combat | Interface (IDamageable) | HealthSystem, MeleeAttack, Projectile |
| Inventory | Slot-Based Array | Inventory, InventorySlot, ItemData SO |
| Save | JSON Serialization | SaveData, SaveFileHandler, ISaveable |
| Audio | Service + Event | AudioManager, AudioCollection SO |
| Camera | Cinemachine 3.x | CinemachineCamera, Confiner2D, ImpulseSource |
| Parallax | LateUpdate Follow | ParallaxLayer per background layer |
| Status Effects | SO Definition + Runtime Instance | StatusEffectData SO, StatusEffectInstance |
| Progression | SO Curve + Runtime State | ExperienceCurveData SO, LevelSystem |
