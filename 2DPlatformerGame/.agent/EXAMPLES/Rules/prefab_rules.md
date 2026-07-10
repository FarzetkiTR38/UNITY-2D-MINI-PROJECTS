# Prefab Rules

## Overview

Rules for creating, organizing, and maintaining prefabs in Unity 6 2D projects.

---

## 1. Prefab Creation Rules

| Rule | Details |
|------|---------|
| One prefab per entity type | `Player.prefab`, `Slime.prefab`, `Coin.prefab` |
| Use Prefab Variants for skins/types | `Slime_Fire.prefab` is a variant of `Slime.prefab` |
| Use Nested Prefabs for sub-components | Weapon prefab nested inside Player prefab |
| Keep hierarchy depth ≤ 3 levels | Root → Children → Grandchildren (max) |
| Name root GameObject same as prefab | `Player.prefab` root object is named `Player` |
| Never use Find in prefab scripts | All references must be serialized or self-discovered via GetComponent |

---

## 2. Prefab Organization

```
Prefabs/
├── Characters/
│   ├── Player/           ← Player and variants
│   ├── Enemies/          ← All enemy types
│   └── NPCs/             ← Non-player characters
├── Environment/
│   ├── Platforms/         ← Moving, crumbling, etc.
│   ├── Hazards/           ← Spikes, saws, fire traps
│   ├── Interactables/     ← Chests, doors, levers
│   └── Pickups/           ← Coins, health, keys
├── Projectiles/           ← Bullets, arrows, fireballs
├── VFX/                   ← Particle effects
├── UI/                    ← World-space UI elements
└── Systems/               ← Manager prefabs, camera rig
```

---

## 3. Prefab Hierarchy Standards

### 3.1 Player Prefab

```
Player (Root)
├── Components: PlayerController, Rigidbody2D, CapsuleCollider2D
├── Sprite
│   └── Components: SpriteRenderer, Animator
├── GroundCheck
│   └── Components: Transform (position only)
├── WallCheck
│   └── Components: Transform (position only)
├── AttackPoint
│   └── Components: Transform (position only)
├── InteractionZone
│   └── Components: CircleCollider2D (trigger)
└── VFX
    ├── DustParticles (ParticleSystem)
    └── HitFlash (ParticleSystem)
```

### 3.2 Enemy Prefab

```
Slime (Root)
├── Components: EnemyController, Rigidbody2D, BoxCollider2D, HealthSystem, DamageDealer
├── Sprite
│   └── Components: SpriteRenderer, Animator
├── DetectionZone
│   └── Components: CircleCollider2D (trigger) — for player detection
├── AttackPoint
│   └── Components: Transform
├── HealthBar
│   └── WorldHealthBar prefab (nested)
└── LootDropPoint
    └── Components: Transform
```

---

## 4. Prefab Documentation

Every prefab MUST have its main script document:
- What the prefab represents
- Required Inspector assignments
- Child objects and their purpose
- Sorting Layer requirements
- Physics Layer requirements
- Tag requirements

Example in script XML docs:

```csharp
/// <remarks>
/// <para><b>Prefab Structure:</b></para>
/// <list type="bullet">
///   <item>Root: PlayerController, Rigidbody2D, CapsuleCollider2D</item>
///   <item>Sprite (child): SpriteRenderer, Animator</item>
///   <item>GroundCheck (child): Transform at feet position</item>
///   <item>AttackPoint (child): Transform at attack origin</item>
/// </list>
/// <para><b>Layer:</b> Player (7)</para>
/// <para><b>Tag:</b> Player</para>
/// <para><b>Sorting Layer:</b> Player</para>
/// </remarks>
```

---

## 5. Prefab Variant Rules

| Rule | Details |
|------|---------|
| Use for visual variations | Fire Slime, Ice Slime (same behavior, different art) |
| Use for stat variations | Elite enemy (same prefab, different ScriptableObject data) |
| Do NOT use for different behaviors | Create a new base prefab instead |
| Name format | `BaseName_VariantName.prefab` → `Slime_Fire.prefab` |
| Override only what changes | Don't modify shared components unnecessarily |

---

## 6. Prefab Performance Rules

| Rule | Details |
|------|---------|
| Disable inactive features | Components not needed at spawn should be disabled |
| Use pools for spawned prefabs | Never Instantiate/Destroy frequently |
| Minimize component count | Each component has overhead |
| Use shared materials | Avoid per-instance materials |
| Set correct Sorting Layer | Prevents unnecessary sorting recalculation |
