# Scene Organization Rules

## Overview

Rules for organizing Unity scenes consistently across the project.

---

## 1. Scene Hierarchy Template

Every gameplay scene MUST follow this hierarchy:

```
Scene Root:
│
├── --- MANAGERS ---              (Separator)
│   ├── GameManager
│   ├── AudioManager
│   └── EventSystem               (Required for UI input)
│
├── --- CAMERA ---                (Separator)
│   ├── MainCamera                (Camera + CinemachineBrain)
│   └── CinemachineCamera         (CinemachineCamera + Confiner2D)
│
├── --- ENVIRONMENT ---           (Separator)
│   ├── Tilemaps
│   │   ├── Ground                (TilemapRenderer + TilemapCollider2D + CompositeCollider2D)
│   │   ├── Walls
│   │   ├── Background
│   │   └── Decoration            (No collider)
│   ├── Background_Parallax
│   │   ├── Layer_0_Sky
│   │   ├── Layer_1_Mountains
│   │   └── Layer_2_Trees
│   ├── Hazards
│   │   ├── Spikes_Zone01
│   │   └── LavaPit_01
│   └── LevelBounds               (Polygon/BoxCollider2D for Cinemachine Confiner)
│
├── --- ENTITIES ---              (Separator)
│   ├── Player                    (Player prefab instance)
│   ├── Enemies
│   │   ├── Slime_01
│   │   ├── Slime_02
│   │   └── Boss_FireDragon
│   └── NPCs
│       ├── Shopkeeper
│       └── QuestGiver
│
├── --- INTERACTABLES ---         (Separator)
│   ├── Doors
│   │   └── Door_ExitLevel
│   ├── Chests
│   │   ├── Chest_Common_01
│   │   └── Chest_Rare_01
│   ├── Pickups
│   │   ├── Coin_01
│   │   └── HealthPotion_01
│   └── SpawnPoints
│       ├── PlayerSpawn
│       └── EnemySpawnZone_01
│
├── --- UI ---                    (Separator)
│   ├── Canvas_HUD
│   ├── Canvas_Menus
│   └── Canvas_Transitions
│
├── --- LIGHTING ---              (Separator)
│   └── GlobalLight2D
│
└── --- DEBUG ---                 (Separator, EditorOnly tag)
    └── DebugOverlays
```

---

## 2. Scene Rules

| Rule | Details |
|------|---------|
| Use separator objects | `--- CATEGORY ---` empty GameObjects for organization |
| Tag debug objects | Use `EditorOnly` tag to exclude from builds |
| One player instance | Spawned from prefab or placed in scene |
| EventSystem required | Must exist for UI interaction |
| CinemachineCamera must have Confiner2D | Prevents camera from showing outside level bounds |
| Tilemaps use CompositeCollider2D | Mandatory for performance |
| Sorting Layers configured | Background → Environment → Entities → Foreground → UI |
| Player must have tag "Player" | Required for collision detection scripts |
| Enemies must have tag "Enemy" | Required for combat system |
| Scene in Build Settings | Every loadable scene must be added to Build Settings |

---

## 3. Sorting Layer Order

| Order | Sorting Layer | Contents |
|-------|--------------|----------|
| 0 | Background | Sky, far parallax layers |
| 1 | Environment | Tilemaps, platforms |
| 2 | BehindEntities | Back decoration, background VFX |
| 3 | Entities | Player, enemies, NPCs, pickups |
| 4 | Foreground | Front decoration, foreground objects |
| 5 | VFX | Particle effects, screen effects |
| 6 | UI | World-space UI elements |

---

## 4. Physics 2D Layer Configuration

| Layer # | Name | Purpose |
|---------|------|---------|
| 0 | Default | Generic objects |
| 6 | Ground | Tilemap ground, platforms |
| 7 | Player | Player collider |
| 8 | Enemy | Enemy colliders |
| 9 | Projectile | Player and enemy projectiles |
| 10 | Pickup | Coins, items, health pickups |
| 11 | Interactable | Doors, chests, switches |
| 12 | Hazard | Spikes, lava, saws |
| 13 | OneWayPlatform | Drop-through platforms |
| 14 | Trigger | Detection zones |

### Layer Collision Matrix

```
              Ground  Player  Enemy  Projectile  Pickup  Interact  Hazard  OneWay  Trigger
Ground          ✗       ✓       ✓       ✓          ✗       ✗        ✗       ✗       ✗
Player          ✓       ✗       ✓       ✗          ✓       ✓        ✓       ✓       ✓
Enemy           ✓       ✓       ✗       ✓          ✗       ✗        ✗       ✗       ✗
Projectile      ✓       ✗       ✓       ✗          ✗       ✗        ✗       ✗       ✗
Pickup          ✗       ✓       ✗       ✗          ✗       ✗        ✗       ✗       ✗
Interactable    ✗       ✓       ✗       ✗          ✗       ✗        ✗       ✗       ✗
Hazard          ✗       ✓       ✗       ✗          ✗       ✗        ✗       ✗       ✗
OneWay          ✗       ✓       ✗       ✗          ✗       ✗        ✗       ✗       ✗
Trigger         ✗       ✓       ✗       ✗          ✗       ✗        ✗       ✗       ✗
```

---

## 5. Bootstrap Scene Pattern

```
Build Index 0: Bootstrap (persistent scene)
├── Initializes ServiceLocator
├── Loads persistent managers (AudioManager, SaveManager)
├── Loads the first gameplay scene additively
└── Never unloaded

Build Index 1+: Gameplay scenes (additive)
├── Loaded/unloaded dynamically
├── Contain level-specific content
└── Reference services via ServiceLocator
```

---

## 6. Scene Transition Rules

| Rule | Details |
|------|---------|
| Use async scene loading | `SceneManager.LoadSceneAsync` |
| Show loading screen | Prevents jarring transitions |
| Unload previous scene | Free memory before loading new |
| Use Addressables for large scenes | Reduces initial load time |
| Pre-load next scene when possible | During gameplay near exit points |
| Fade transitions | Canvas_Transitions handles fade in/out |
