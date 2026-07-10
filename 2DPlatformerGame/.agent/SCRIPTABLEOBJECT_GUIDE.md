# ScriptableObject Guide

## Overview

ScriptableObjects are the backbone of data-driven Unity 2D architecture. They serve as shared configuration containers, event channels, databases, and runtime variables — all without requiring MonoBehaviours or scene dependencies. This guide covers every ScriptableObject use case for 2D game development.

---

## 1. Core Principles

### 1.1 When to Use ScriptableObjects

| Use Case | Example |
|----------|---------|
| **Configuration data** | Player stats, enemy stats, weapon parameters |
| **Item definitions** | Swords, potions, keys, armor |
| **Event channels** | OnPlayerDied, OnScoreChanged, OnLevelCompleted |
| **Level data** | Level layouts, wave configurations, spawn tables |
| **Audio configuration** | Sound collections per entity |
| **Database/Registry** | ItemDatabase, EnemyDatabase, AchievementList |
| **Runtime variables** | Shared FloatVariable for HP bar binding |
| **Enum-like sets** | Difficulty presets, color palettes |

### 1.2 When NOT to Use ScriptableObjects

| Avoid For | Use Instead |
|-----------|-------------|
| Runtime mutable state that must persist | Save system (JSON file) |
| Per-instance data | MonoBehaviour fields |
| Scene-specific setup | Scene references, Prefabs |
| Large binary data | Addressables, AssetBundles |

### 1.3 Golden Rules

1. **Treat as read-only at runtime** — Do not modify SO fields during gameplay. If you need runtime state, copy data to a runtime struct/class.
2. **Always expose via properties** — Never access SO fields directly from outside.
3. **Use [CreateAssetMenu]** — Every SO must be creatable from the Project window.
4. **Validate data** — Implement `OnValidate()` to catch designer errors.
5. **Document creation path** — Include the menu path in XML docs.

---

## 2. ScriptableObject Patterns

### 2.1 Configuration Pattern

For tunable game parameters that designers adjust without touching code.

```csharp
// ============================================================================
// PlayerConfig.cs
// Purpose: Player tuning parameters accessible from Inspector
// ============================================================================
using UnityEngine;

namespace GameName.Data
{
    /// <summary>
    /// Configuration asset for player movement, combat, and gameplay parameters.
    /// </summary>
    /// <remarks>
    /// <para><b>Creation:</b> Assets → Create → GameName → Config → Player Config</para>
    /// <para><b>Usage:</b> Reference from PlayerController via [SerializeField].
    /// Treat as read-only during gameplay.</para>
    /// </remarks>
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "GameName/Config/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("Maximum horizontal speed.")]
        [SerializeField, Min(0f)] private float _moveSpeed = 8f;

        [Tooltip("Jump force.")]
        [SerializeField, Min(0f)] private float _jumpForce = 14f;

        [Tooltip("Coyote time duration in seconds.")]
        [SerializeField, Range(0f, 0.5f)] private float _coyoteTime = 0.12f;

        [Tooltip("Jump buffer duration in seconds.")]
        [SerializeField, Range(0f, 0.5f)] private float _jumpBuffer = 0.15f;

        [Tooltip("Fall gravity multiplier for snappier falls.")]
        [SerializeField, Range(1f, 10f)] private float _fallMultiplier = 2.5f;

        [Space(10)]
        [Header("Combat")]
        [Tooltip("Base attack damage.")]
        [SerializeField, Min(1)] private int _attackDamage = 10;

        [Tooltip("Attack cooldown in seconds.")]
        [SerializeField, Min(0.1f)] private float _attackCooldown = 0.5f;

        [Tooltip("Invincibility frames duration after taking damage.")]
        [SerializeField, Range(0f, 5f)] private float _invincibilityDuration = 1.5f;

        [Space(10)]
        [Header("Health")]
        [Tooltip("Starting and maximum health.")]
        [SerializeField, Min(1)] private int _maxHealth = 100;

        [Tooltip("Number of extra lives at game start.")]
        [SerializeField, Min(0)] private int _startingLives = 3;

        // Properties
        public float MoveSpeed => _moveSpeed;
        public float JumpForce => _jumpForce;
        public float CoyoteTime => _coyoteTime;
        public float JumpBuffer => _jumpBuffer;
        public float FallMultiplier => _fallMultiplier;
        public int AttackDamage => _attackDamage;
        public float AttackCooldown => _attackCooldown;
        public float InvincibilityDuration => _invincibilityDuration;
        public int MaxHealth => _maxHealth;
        public int StartingLives => _startingLives;
    }
}
```

### 2.2 Item Definition Pattern

For defining items, weapons, consumables, and equipment.

```csharp
// ============================================================================
// ItemData.cs
// Purpose: Base item definition for the inventory/loot system
// ============================================================================
using UnityEngine;

namespace GameName.Data
{
    /// <summary>
    /// Base ScriptableObject for all item definitions in the game.
    /// Subclass for specific item types (Weapon, Consumable, Equipment).
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "GameName/Items/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique item identifier for save/load.")]
        [SerializeField] private string _itemId = "";

        [Tooltip("Display name shown in UI.")]
        [SerializeField] private string _displayName = "New Item";

        [Tooltip("Description shown in inventory tooltip.")]
        [SerializeField, TextArea(2, 5)] private string _description = "";

        [Space(10)]
        [Header("Visual")]
        [Tooltip("Item icon for inventory/shop UI.")]
        [SerializeField] private Sprite _icon;

        [Tooltip("Prefab spawned in the world as a pickup.")]
        [SerializeField] private GameObject _worldPrefab;

        [Space(10)]
        [Header("Properties")]
        [Tooltip("Item rarity tier.")]
        [SerializeField] private ItemRarity _rarity = ItemRarity.Common;

        [Tooltip("Maximum stack count. 1 = non-stackable.")]
        [SerializeField, Min(1)] private int _maxStackSize = 1;

        [Tooltip("Base sell value in currency.")]
        [SerializeField, Min(0)] private int _sellValue = 10;

        [Tooltip("Can this item be sold?")]
        [SerializeField] private bool _isSellable = true;

        [Tooltip("Can this item be dropped?")]
        [SerializeField] private bool _isDroppable = true;

        // Properties
        public string ItemId => _itemId;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public GameObject WorldPrefab => _worldPrefab;
        public ItemRarity Rarity => _rarity;
        public int MaxStackSize => _maxStackSize;
        public int SellValue => _sellValue;
        public bool IsSellable => _isSellable;
        public bool IsDroppable => _isDroppable;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(_itemId))
            {
                _itemId = name.ToLowerInvariant().Replace(" ", "_");
            }
        }
    }

    /// <summary>Defines item rarity tiers.</summary>
    public enum ItemRarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4
    }
}
```

```csharp
// ============================================================================
// WeaponData.cs
// Purpose: Weapon-specific item definition
// ============================================================================
using UnityEngine;

namespace GameName.Data
{
    /// <summary>
    /// Weapon data extending base ItemData with combat stats.
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "GameName/Items/Weapon Data")]
    public class WeaponData : ItemData
    {
        [Space(10)]
        [Header("Weapon Stats")]
        [Tooltip("Base damage per hit.")]
        [SerializeField, Min(1)] private int _baseDamage = 15;

        [Tooltip("Attack speed in attacks per second.")]
        [SerializeField, Range(0.1f, 10f)] private float _attackSpeed = 2f;

        [Tooltip("Weapon range in units.")]
        [SerializeField, Min(0.1f)] private float _range = 1.5f;

        [Tooltip("Critical hit chance (0.0 to 1.0).")]
        [SerializeField, Range(0f, 1f)] private float _critChance = 0.1f;

        [Tooltip("Critical hit damage multiplier.")]
        [SerializeField, Range(1f, 5f)] private float _critMultiplier = 2f;

        [Tooltip("Knockback force applied to targets.")]
        [SerializeField, Min(0f)] private float _knockbackForce = 5f;

        [Space(10)]
        [Header("Weapon Type")]
        [Tooltip("The category of this weapon.")]
        [SerializeField] private WeaponType _weaponType = WeaponType.Sword;

        [Tooltip("Damage type dealt by this weapon.")]
        [SerializeField] private DamageType _damageType = DamageType.Physical;

        // Properties
        public int BaseDamage => _baseDamage;
        public float AttackSpeed => _attackSpeed;
        public float Range => _range;
        public float CritChance => _critChance;
        public float CritMultiplier => _critMultiplier;
        public float KnockbackForce => _knockbackForce;
        public WeaponType WeaponType => _weaponType;
        public DamageType DamageType => _damageType;
    }

    /// <summary>Defines weapon categories.</summary>
    public enum WeaponType
    {
        Sword = 0,
        Axe = 1,
        Spear = 2,
        Bow = 3,
        Staff = 4,
        Dagger = 5
    }
}
```

### 2.3 Database Pattern

For collections of items, enemies, or achievements that can be queried.

```csharp
// ============================================================================
// ItemDatabase.cs
// Purpose: Centralized registry of all item definitions
// ============================================================================
using System.Collections.Generic;
using UnityEngine;

namespace GameName.Data
{
    /// <summary>
    /// Central database containing all item definitions in the game.
    /// Used for lookup by ID, filtering by type, and iteration.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "GameName/Database/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [Tooltip("All registered item definitions.")]
        [SerializeField] private List<ItemData> _items = new();

        // Cached lookup dictionary — built on first access
        private Dictionary<string, ItemData> _lookupCache;

        /// <summary>Gets the total number of registered items.</summary>
        public int Count => _items.Count;

        /// <summary>
        /// Retrieves an item by its unique identifier.
        /// </summary>
        /// <param name="itemId">The item's unique ID.</param>
        /// <returns>The item data, or null if not found.</returns>
        public ItemData GetItemById(string itemId)
        {
            BuildCacheIfNeeded();

            if (_lookupCache.TryGetValue(itemId, out ItemData item))
            {
                return item;
            }

            Debug.LogWarning($"[ItemDatabase] Item not found: {itemId}", this);
            return null;
        }

        /// <summary>
        /// Attempts to retrieve an item by its unique identifier.
        /// </summary>
        /// <param name="itemId">The item's unique ID.</param>
        /// <param name="item">The found item, or null.</param>
        /// <returns><c>true</c> if the item was found.</returns>
        public bool TryGetItem(string itemId, out ItemData item)
        {
            BuildCacheIfNeeded();
            return _lookupCache.TryGetValue(itemId, out item);
        }

        /// <summary>Returns all items of a specific rarity.</summary>
        public List<ItemData> GetItemsByRarity(ItemRarity rarity)
        {
            List<ItemData> result = new();
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Rarity == rarity)
                {
                    result.Add(_items[i]);
                }
            }
            return result;
        }

        /// <summary>Returns all items as a read-only list.</summary>
        public IReadOnlyList<ItemData> GetAllItems() => _items;

        private void BuildCacheIfNeeded()
        {
            if (_lookupCache != null) return;

            _lookupCache = new Dictionary<string, ItemData>(_items.Count);
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] == null) continue;

                if (!_lookupCache.TryAdd(_items[i].ItemId, _items[i]))
                {
                    Debug.LogWarning(
                        $"[ItemDatabase] Duplicate item ID: {_items[i].ItemId}. " +
                        $"Item '{_items[i].DisplayName}' will be skipped.", this);
                }
            }
        }

        private void OnValidate()
        {
            _lookupCache = null; // Force rebuild after editor changes

            // Check for duplicates in editor
            HashSet<string> seen = new();
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] == null)
                {
                    Debug.LogWarning($"[ItemDatabase] Null entry at index {i}.", this);
                    continue;
                }

                if (!seen.Add(_items[i].ItemId))
                {
                    Debug.LogError(
                        $"[ItemDatabase] Duplicate ID '{_items[i].ItemId}' " +
                        $"at index {i}: '{_items[i].DisplayName}'.", this);
                }
            }
        }
    }
}
```

### 2.4 Runtime Variable Pattern

For shared runtime values that multiple systems observe (e.g., player HP for the health bar).

```csharp
// ============================================================================
// FloatVariable.cs
// Purpose: Shared runtime float value observable by UI and systems
// ============================================================================
using System;
using UnityEngine;

namespace GameName.Core.Variables
{
    /// <summary>
    /// A shared float variable stored as a ScriptableObject.
    /// Multiple systems can read/write this value and subscribe to changes.
    /// </summary>
    /// <remarks>
    /// <para><b>Usage:</b> Create an asset (e.g., PlayerHealth).
    /// The gameplay system writes to it; the UI reads from it.
    /// Neither system knows about the other.</para>
    /// <para><b>Important:</b> Call <see cref="ResetToDefault"/> at game start
    /// since SO values persist between play sessions in the Editor.</para>
    /// </remarks>
    [CreateAssetMenu(fileName = "NewFloatVariable", menuName = "GameName/Variables/Float Variable")]
    public class FloatVariable : ScriptableObject
    {
        [Tooltip("The initial/default value.")]
        [SerializeField] private float _defaultValue;

        [Tooltip("Current runtime value. Reset this at game start.")]
        [SerializeField] private float _runtimeValue;

        /// <summary>Raised when the value changes. Parameters: (newValue, previousValue).</summary>
        public event Action<float, float> OnValueChanged;

        /// <summary>Gets or sets the current runtime value.</summary>
        public float Value
        {
            get => _runtimeValue;
            set
            {
                if (Mathf.Approximately(_runtimeValue, value)) return;
                float previous = _runtimeValue;
                _runtimeValue = value;
                OnValueChanged?.Invoke(_runtimeValue, previous);
            }
        }

        /// <summary>Gets the default value configured in the Inspector.</summary>
        public float DefaultValue => _defaultValue;

        /// <summary>Resets the runtime value to the default. Call at game start.</summary>
        public void ResetToDefault()
        {
            _runtimeValue = _defaultValue;
        }

        private void OnEnable()
        {
            // Reset on domain reload (entering play mode)
            _runtimeValue = _defaultValue;
        }
    }
}
```

### 2.5 Audio Collection Pattern

For grouping related sound effects per entity or system.

```csharp
// ============================================================================
// AudioCollection.cs
// Purpose: Groups related audio clips for an entity or system
// ============================================================================
using UnityEngine;

namespace GameName.Data
{
    /// <summary>
    /// Collection of audio clips for a specific entity or system.
    /// Supports random selection from clip arrays for variety.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAudioCollection", menuName = "GameName/Audio/Audio Collection")]
    public class AudioCollection : ScriptableObject
    {
        [System.Serializable]
        public struct AudioEntry
        {
            [Tooltip("Descriptive name for this sound.")]
            public string Name;

            [Tooltip("Audio clips to randomly select from.")]
            public AudioClip[] Clips;

            [Tooltip("Volume multiplier for this sound.")]
            [Range(0f, 1f)]
            public float Volume;

            [Tooltip("Pitch range (random between min and max).")]
            [Range(0.5f, 2f)]
            public float PitchMin;

            [Range(0.5f, 2f)]
            public float PitchMax;
        }

        [Tooltip("All audio entries in this collection.")]
        [SerializeField] private AudioEntry[] _entries;

        /// <summary>
        /// Gets a random clip from the specified entry index.
        /// </summary>
        /// <param name="index">The entry index.</param>
        /// <param name="clip">The selected audio clip.</param>
        /// <param name="volume">The configured volume.</param>
        /// <param name="pitch">A random pitch within the configured range.</param>
        /// <returns><c>true</c> if a valid clip was found.</returns>
        public bool TryGetClip(int index, out AudioClip clip, out float volume, out float pitch)
        {
            clip = null;
            volume = 1f;
            pitch = 1f;

            if (index < 0 || index >= _entries.Length) return false;

            AudioEntry entry = _entries[index];
            if (entry.Clips == null || entry.Clips.Length == 0) return false;

            clip = entry.Clips[Random.Range(0, entry.Clips.Length)];
            volume = entry.Volume;
            pitch = Random.Range(entry.PitchMin, entry.PitchMax);
            return clip != null;
        }

        /// <summary>Gets the number of audio entries.</summary>
        public int EntryCount => _entries?.Length ?? 0;
    }
}
```

### 2.6 Level Data Pattern

For defining level configurations, spawn waves, and progression.

```csharp
// ============================================================================
// LevelData.cs
// Purpose: Level configuration and spawn wave definitions
// ============================================================================
using System.Collections.Generic;
using UnityEngine;

namespace GameName.Data
{
    /// <summary>
    /// Configuration data for a game level including enemy waves,
    /// completion conditions, and reward definitions.
    /// </summary>
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "GameName/Levels/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Level Info")]
        [Tooltip("Display name of this level.")]
        [SerializeField] private string _levelName = "New Level";

        [Tooltip("Scene name to load for this level.")]
        [SerializeField] private string _sceneName = "";

        [Tooltip("Recommended player level.")]
        [SerializeField, Min(1)] private int _recommendedLevel = 1;

        [Tooltip("Level description for level select screen.")]
        [SerializeField, TextArea(2, 4)] private string _description = "";

        [Space(10)]
        [Header("Spawn Waves")]
        [Tooltip("Enemy waves for this level.")]
        [SerializeField] private List<WaveData> _waves = new();

        [Space(10)]
        [Header("Completion")]
        [Tooltip("Time limit in seconds. 0 = no limit.")]
        [SerializeField, Min(0f)] private float _timeLimit;

        [Tooltip("Par time for star rating in seconds.")]
        [SerializeField, Min(0f)] private float _parTime = 120f;

        [Space(10)]
        [Header("Rewards")]
        [Tooltip("Currency rewarded on first completion.")]
        [SerializeField, Min(0)] private int _firstClearReward = 100;

        [Tooltip("Experience rewarded on completion.")]
        [SerializeField, Min(0)] private int _experienceReward = 50;

        // Properties
        public string LevelName => _levelName;
        public string SceneName => _sceneName;
        public int RecommendedLevel => _recommendedLevel;
        public string Description => _description;
        public IReadOnlyList<WaveData> Waves => _waves;
        public float TimeLimit => _timeLimit;
        public float ParTime => _parTime;
        public int FirstClearReward => _firstClearReward;
        public int ExperienceReward => _experienceReward;
        public bool HasTimeLimit => _timeLimit > 0f;
    }

    /// <summary>Defines a spawn wave within a level.</summary>
    [System.Serializable]
    public class WaveData
    {
        [Tooltip("Delay before this wave starts (seconds from level start or previous wave end).")]
        [Min(0f)] public float Delay;

        [Tooltip("Enemy spawn entries for this wave.")]
        public List<SpawnEntry> Spawns = new();

        [Tooltip("Condition to trigger the next wave.")]
        public WaveEndCondition EndCondition = WaveEndCondition.AllEnemiesDefeated;
    }

    /// <summary>Defines a single enemy spawn within a wave.</summary>
    [System.Serializable]
    public class SpawnEntry
    {
        [Tooltip("Enemy type to spawn.")]
        public EnemyData EnemyData;

        [Tooltip("Number of enemies to spawn.")]
        [Min(1)] public int Count = 1;

        [Tooltip("Delay between individual spawns in seconds.")]
        [Min(0f)] public float SpawnInterval = 0.5f;

        [Tooltip("Spawn point index (matches SpawnPoint order in scene).")]
        [Min(0)] public int SpawnPointIndex;
    }

    /// <summary>Defines when a wave is considered complete.</summary>
    public enum WaveEndCondition
    {
        AllEnemiesDefeated = 0,
        TimerExpired = 1,
        ReachCheckpoint = 2,
        Manual = 3
    }
}
```

---

## 3. ScriptableObject Best Practices Summary

| Practice | Details |
|----------|---------|
| Always use `[CreateAssetMenu]` | Every SO must be creatable from Project window |
| Expose data via properties only | Never use public fields on SOs |
| Implement `OnValidate()` | Validate data integrity in the editor |
| Document the creation path | Include menu path in XML documentation |
| Do NOT mutate at runtime | Copy data to runtime containers if needed |
| Use `OnEnable()` for reset | Reset runtime state on domain reload |
| Cache computed values | Rebuild lookup tables on first access |
| Use `[System.Serializable]` for nested data | Keep nested classes as `[Serializable]` structs/classes |
| Name assets descriptively | `SwordData.asset`, `SlimeConfig.asset`, not `Data1.asset` |
| Organize in folders | `ScriptableObjects/Items/`, `ScriptableObjects/Enemies/` |
