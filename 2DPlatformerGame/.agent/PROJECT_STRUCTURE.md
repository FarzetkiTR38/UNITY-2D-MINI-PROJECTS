# Project Structure Guide

## Overview

This document defines the mandatory folder organization, asset placement rules, and project structure conventions for Unity 6 2D projects. A well-organized project is critical for team collaboration, asset management, build optimization, and long-term maintainability.

---

## 1. Root Folder Structure

```
Assets/
├── _Project/                        ← All project-specific assets live here
│   ├── Art/
│   │   ├── Animations/
│   │   │   ├── Characters/
│   │   │   │   ├── Player/
│   │   │   │   │   ├── Idle.anim
│   │   │   │   │   ├── Run.anim
│   │   │   │   │   ├── Jump.anim
│   │   │   │   │   ├── Fall.anim
│   │   │   │   │   ├── Attack.anim
│   │   │   │   │   └── PlayerAnimatorController.controller
│   │   │   │   └── Enemies/
│   │   │   │       ├── Slime/
│   │   │   │       ├── Skeleton/
│   │   │   │       └── Boss/
│   │   │   ├── Environment/
│   │   │   │   ├── Doors.anim
│   │   │   │   ├── Torches.anim
│   │   │   │   └── Water.anim
│   │   │   └── UI/
│   │   │       ├── ButtonHover.anim
│   │   │       └── PanelFade.anim
│   │   │
│   │   ├── Sprites/
│   │   │   ├── Characters/
│   │   │   │   ├── Player/
│   │   │   │   │   ├── player_idle_sheet.png
│   │   │   │   │   ├── player_run_sheet.png
│   │   │   │   │   └── player_attack_sheet.png
│   │   │   │   └── Enemies/
│   │   │   │       ├── Slime/
│   │   │   │       ├── Skeleton/
│   │   │   │       └── Boss/
│   │   │   ├── Environment/
│   │   │   │   ├── Tilesets/
│   │   │   │   │   ├── ground_tileset.png
│   │   │   │   │   ├── wall_tileset.png
│   │   │   │   │   └── decoration_tileset.png
│   │   │   │   ├── Backgrounds/
│   │   │   │   │   ├── parallax_layer_0.png
│   │   │   │   │   ├── parallax_layer_1.png
│   │   │   │   │   └── parallax_layer_2.png
│   │   │   │   └── Props/
│   │   │   │       ├── trees.png
│   │   │   │       ├── rocks.png
│   │   │   │       └── signs.png
│   │   │   ├── Items/
│   │   │   │   ├── weapons.png
│   │   │   │   ├── consumables.png
│   │   │   │   └── key_items.png
│   │   │   ├── UI/
│   │   │   │   ├── Icons/
│   │   │   │   ├── Buttons/
│   │   │   │   ├── Panels/
│   │   │   │   └── HUD/
│   │   │   └── VFX/
│   │   │       ├── particles.png
│   │   │       ├── impact_effects.png
│   │   │       └── magic_effects.png
│   │   │
│   │   ├── Tiles/
│   │   │   ├── RuleTiles/
│   │   │   │   ├── Ground_RuleTile.asset
│   │   │   │   ├── Wall_RuleTile.asset
│   │   │   │   └── Platform_RuleTile.asset
│   │   │   ├── AnimatedTiles/
│   │   │   └── Palettes/
│   │   │       ├── GroundPalette.prefab
│   │   │       └── DecorationPalette.prefab
│   │   │
│   │   ├── Materials/
│   │   │   ├── Sprites/
│   │   │   │   ├── Sprite-Lit-Default.mat
│   │   │   │   └── Sprite-Unlit-Default.mat
│   │   │   ├── Particles/
│   │   │   │   └── ParticleMaterial.mat
│   │   │   └── UI/
│   │   │       └── UIBlurMaterial.mat
│   │   │
│   │   └── SpriteAtlases/
│   │       ├── Characters_Atlas.spriteatlas
│   │       ├── Environment_Atlas.spriteatlas
│   │       ├── UI_Atlas.spriteatlas
│   │       └── VFX_Atlas.spriteatlas
│   │
│   ├── Audio/
│   │   ├── Music/
│   │   │   ├── MainMenu_BGM.ogg
│   │   │   ├── Level01_BGM.ogg
│   │   │   ├── Boss_BGM.ogg
│   │   │   └── GameOver_BGM.ogg
│   │   ├── SFX/
│   │   │   ├── Player/
│   │   │   │   ├── jump.wav
│   │   │   │   ├── land.wav
│   │   │   │   ├── attack_swing.wav
│   │   │   │   ├── hurt.wav
│   │   │   │   └── death.wav
│   │   │   ├── Enemies/
│   │   │   │   ├── enemy_hit.wav
│   │   │   │   ├── enemy_death.wav
│   │   │   │   └── enemy_attack.wav
│   │   │   ├── UI/
│   │   │   │   ├── button_click.wav
│   │   │   │   ├── button_hover.wav
│   │   │   │   ├── menu_open.wav
│   │   │   │   └── menu_close.wav
│   │   │   ├── Environment/
│   │   │   │   ├── door_open.wav
│   │   │   │   ├── chest_open.wav
│   │   │   │   └── checkpoint.wav
│   │   │   └── Items/
│   │   │       ├── coin_pickup.wav
│   │   │       ├── health_pickup.wav
│   │   │       └── powerup.wav
│   │   └── Mixers/
│   │       └── MainAudioMixer.mixer
│   │
│   ├── Fonts/
│   │   ├── PrimaryFont.ttf
│   │   ├── PrimaryFont SDF.asset
│   │   ├── SecondaryFont.ttf
│   │   └── SecondaryFont SDF.asset
│   │
│   ├── Prefabs/
│   │   ├── Characters/
│   │   │   ├── Player/
│   │   │   │   └── Player.prefab
│   │   │   ├── Enemies/
│   │   │   │   ├── Slime.prefab
│   │   │   │   ├── Skeleton.prefab
│   │   │   │   └── Boss.prefab
│   │   │   └── NPCs/
│   │   │       ├── Shopkeeper.prefab
│   │   │       └── QuestGiver.prefab
│   │   ├── Environment/
│   │   │   ├── Platforms/
│   │   │   │   ├── MovingPlatform.prefab
│   │   │   │   └── CrumblingPlatform.prefab
│   │   │   ├── Hazards/
│   │   │   │   ├── Spikes.prefab
│   │   │   │   ├── SawBlade.prefab
│   │   │   │   └── FireTrap.prefab
│   │   │   ├── Interactables/
│   │   │   │   ├── Chest.prefab
│   │   │   │   ├── Door.prefab
│   │   │   │   ├── Lever.prefab
│   │   │   │   └── Checkpoint.prefab
│   │   │   └── Pickups/
│   │   │       ├── Coin.prefab
│   │   │       ├── HealthPickup.prefab
│   │   │       └── KeyItem.prefab
│   │   ├── Projectiles/
│   │   │   ├── PlayerBullet.prefab
│   │   │   ├── EnemyBullet.prefab
│   │   │   └── Arrow.prefab
│   │   ├── VFX/
│   │   │   ├── HitEffect.prefab
│   │   │   ├── DeathEffect.prefab
│   │   │   ├── DustCloud.prefab
│   │   │   ├── CoinPickupVFX.prefab
│   │   │   └── LevelUpVFX.prefab
│   │   ├── UI/
│   │   │   ├── DamagePopup.prefab
│   │   │   ├── FloatingText.prefab
│   │   │   └── HealthBar_World.prefab
│   │   └── Systems/
│   │       ├── Spawner.prefab
│   │       ├── CameraRig.prefab
│   │       └── AudioSystem.prefab
│   │
│   ├── Scenes/
│   │   ├── Bootstrap.unity
│   │   ├── MainMenu.unity
│   │   ├── Loading.unity
│   │   ├── Gameplay.unity
│   │   └── Levels/
│   │       ├── Level_01.unity
│   │       ├── Level_02.unity
│   │       └── Level_03.unity
│   │
│   ├── Scripts/
│   │   ├── Runtime/
│   │   │   ├── Core/
│   │   │   │   ├── Core.asmdef
│   │   │   │   ├── Bootstrap/
│   │   │   │   │   ├── GameBootstrapper.cs
│   │   │   │   │   └── ServiceInitializer.cs
│   │   │   │   ├── Events/
│   │   │   │   │   ├── GameEvent.cs
│   │   │   │   │   ├── GameEventListener.cs
│   │   │   │   │   ├── VoidEventChannel.cs
│   │   │   │   │   ├── IntEventChannel.cs
│   │   │   │   │   ├── FloatEventChannel.cs
│   │   │   │   │   ├── StringEventChannel.cs
│   │   │   │   │   └── TransformEventChannel.cs
│   │   │   │   ├── Interfaces/
│   │   │   │   │   ├── IDamageable.cs
│   │   │   │   │   ├── IHealable.cs
│   │   │   │   │   ├── IInteractable.cs
│   │   │   │   │   ├── ICollectible.cs
│   │   │   │   │   ├── ISaveable.cs
│   │   │   │   │   ├── IPoolable.cs
│   │   │   │   │   ├── IState.cs
│   │   │   │   │   └── IService.cs
│   │   │   │   ├── Patterns/
│   │   │   │   │   ├── ServiceLocator.cs
│   │   │   │   │   ├── StateMachine.cs
│   │   │   │   │   ├── ObjectPool.cs
│   │   │   │   │   └── Command.cs
│   │   │   │   ├── Extensions/
│   │   │   │   │   ├── VectorExtensions.cs
│   │   │   │   │   ├── TransformExtensions.cs
│   │   │   │   │   ├── ComponentExtensions.cs
│   │   │   │   │   └── CollectionExtensions.cs
│   │   │   │   └── Utilities/
│   │   │   │       ├── Timer.cs
│   │   │   │       ├── MathUtils.cs
│   │   │   │       └── CoroutineHelper.cs
│   │   │   │
│   │   │   ├── Gameplay/
│   │   │   │   ├── Gameplay.asmdef
│   │   │   │   ├── Player/
│   │   │   │   │   ├── PlayerController.cs
│   │   │   │   │   ├── PlayerMovement.cs
│   │   │   │   │   ├── PlayerCombat.cs
│   │   │   │   │   ├── PlayerAnimation.cs
│   │   │   │   │   └── PlayerInputHandler.cs
│   │   │   │   ├── Enemies/
│   │   │   │   │   ├── EnemyBase.cs
│   │   │   │   │   ├── EnemyAI.cs
│   │   │   │   │   ├── EnemyPatrol.cs
│   │   │   │   │   ├── EnemyChase.cs
│   │   │   │   │   └── EnemySpawner.cs
│   │   │   │   ├── Combat/
│   │   │   │   │   ├── HealthSystem.cs
│   │   │   │   │   ├── DamageDealer.cs
│   │   │   │   │   ├── Projectile.cs
│   │   │   │   │   ├── MeleeAttack.cs
│   │   │   │   │   ├── Knockback.cs
│   │   │   │   │   └── StatusEffect.cs
│   │   │   │   ├── Items/
│   │   │   │   │   ├── ItemBase.cs
│   │   │   │   │   ├── Inventory.cs
│   │   │   │   │   ├── ItemPickup.cs
│   │   │   │   │   ├── WeaponItem.cs
│   │   │   │   │   └── ConsumableItem.cs
│   │   │   │   ├── Interaction/
│   │   │   │   │   ├── InteractionDetector.cs
│   │   │   │   │   ├── Chest.cs
│   │   │   │   │   ├── Door.cs
│   │   │   │   │   ├── Lever.cs
│   │   │   │   │   └── Checkpoint.cs
│   │   │   │   ├── Progression/
│   │   │   │   │   ├── ExperienceSystem.cs
│   │   │   │   │   ├── LevelSystem.cs
│   │   │   │   │   ├── CurrencySystem.cs
│   │   │   │   │   └── AchievementSystem.cs
│   │   │   │   ├── Environment/
│   │   │   │   │   ├── MovingPlatform.cs
│   │   │   │   │   ├── CrumblingPlatform.cs
│   │   │   │   │   ├── Hazard.cs
│   │   │   │   │   ├── ParallaxLayer.cs
│   │   │   │   │   └── DayNightCycle.cs
│   │   │   │   └── Spawning/
│   │   │   │       ├── SpawnPoint.cs
│   │   │   │       ├── WaveSpawner.cs
│   │   │   │       └── LootDropper.cs
│   │   │   │
│   │   │   ├── Systems/
│   │   │   │   ├── Systems.asmdef
│   │   │   │   ├── Audio/
│   │   │   │   │   ├── AudioManager.cs
│   │   │   │   │   ├── MusicPlayer.cs
│   │   │   │   │   └── SFXPlayer.cs
│   │   │   │   ├── Camera/
│   │   │   │   │   ├── CameraManager.cs
│   │   │   │   │   └── ScreenShake.cs
│   │   │   │   ├── Save/
│   │   │   │   │   ├── SaveManager.cs
│   │   │   │   │   ├── SaveData.cs
│   │   │   │   │   └── SaveFileHandler.cs
│   │   │   │   ├── Scene/
│   │   │   │   │   ├── SceneLoader.cs
│   │   │   │   │   └── SceneTransition.cs
│   │   │   │   ├── Input/
│   │   │   │   │   └── InputManager.cs
│   │   │   │   ├── Dialogue/
│   │   │   │   │   ├── DialogueManager.cs
│   │   │   │   │   ├── DialogueLine.cs
│   │   │   │   │   └── DialogueTrigger.cs
│   │   │   │   ├── Quest/
│   │   │   │   │   ├── QuestManager.cs
│   │   │   │   │   ├── Quest.cs
│   │   │   │   │   └── QuestObjective.cs
│   │   │   │   ├── Pooling/
│   │   │   │   │   ├── PoolManager.cs
│   │   │   │   │   └── PoolableObject.cs
│   │   │   │   └── Localization/
│   │   │   │       └── LocalizationManager.cs
│   │   │   │
│   │   │   └── UI/
│   │   │       ├── UI.asmdef
│   │   │       ├── Screens/
│   │   │       │   ├── MainMenuScreen.cs
│   │   │       │   ├── GameplayHUD.cs
│   │   │       │   ├── PauseScreen.cs
│   │   │       │   ├── GameOverScreen.cs
│   │   │       │   ├── SettingsScreen.cs
│   │   │       │   ├── InventoryScreen.cs
│   │   │       │   └── DialogueUI.cs
│   │   │       ├── Components/
│   │   │       │   ├── HealthBar.cs
│   │   │       │   ├── DamagePopup.cs
│   │   │       │   ├── FloatingText.cs
│   │   │       │   ├── MinimapUI.cs
│   │   │       │   ├── CooldownIndicator.cs
│   │   │       │   └── NotificationPopup.cs
│   │   │       ├── Base/
│   │   │       │   ├── UIScreen.cs
│   │   │       │   ├── UIManager.cs
│   │   │       │   └── UIAnimation.cs
│   │   │       └── Data/
│   │   │           └── UIThemeData.cs
│   │   │
│   │   └── Editor/
│   │       ├── Editor.asmdef
│   │       ├── CustomInspectors/
│   │       │   ├── HealthSystemEditor.cs
│   │       │   └── EnemyBaseEditor.cs
│   │       ├── PropertyDrawers/
│   │       │   └── ReadOnlyDrawer.cs
│   │       ├── Tools/
│   │       │   ├── LevelDesignTools.cs
│   │       │   └── DataValidationTool.cs
│   │       └── Windows/
│   │           └── GameDebugWindow.cs
│   │
│   ├── ScriptableObjects/
│   │   ├── Config/
│   │   │   ├── GameConfig.asset
│   │   │   ├── PlayerConfig.asset
│   │   │   ├── PhysicsConfig.asset
│   │   │   └── AudioConfig.asset
│   │   ├── Items/
│   │   │   ├── Definitions/
│   │   │   │   ├── Sword.asset
│   │   │   │   ├── Bow.asset
│   │   │   │   ├── HealthPotion.asset
│   │   │   │   └── Key.asset
│   │   │   └── Database/
│   │   │       └── ItemDatabase.asset
│   │   ├── Enemies/
│   │   │   ├── SlimeData.asset
│   │   │   ├── SkeletonData.asset
│   │   │   └── BossData.asset
│   │   ├── Weapons/
│   │   │   ├── SwordData.asset
│   │   │   ├── BowData.asset
│   │   │   └── StaffData.asset
│   │   ├── Audio/
│   │   │   ├── PlayerSounds.asset
│   │   │   ├── EnemySounds.asset
│   │   │   ├── UISounds.asset
│   │   │   └── EnvironmentSounds.asset
│   │   ├── Events/
│   │   │   ├── OnPlayerDamaged.asset
│   │   │   ├── OnPlayerDied.asset
│   │   │   ├── OnEnemyKilled.asset
│   │   │   ├── OnItemCollected.asset
│   │   │   ├── OnLevelCompleted.asset
│   │   │   ├── OnScoreChanged.asset
│   │   │   └── OnGamePaused.asset
│   │   ├── Levels/
│   │   │   ├── Level01Data.asset
│   │   │   ├── Level02Data.asset
│   │   │   └── Level03Data.asset
│   │   └── Progression/
│   │       ├── ExperienceCurve.asset
│   │       ├── SkillTree.asset
│   │       └── AchievementList.asset
│   │
│   ├── Addressables/
│   │   ├── AddressableAssetsData/
│   │   ├── Groups/
│   │   │   ├── Characters.asset
│   │   │   ├── Environment.asset
│   │   │   ├── Audio.asset
│   │   │   └── UI.asset
│   │   └── Labels/
│   │
│   ├── Settings/
│   │   ├── Input/
│   │   │   └── GameInputActions.inputactions
│   │   ├── URP/
│   │   │   ├── URP_Renderer2D.asset
│   │   │   └── URP_PipelineAsset.asset
│   │   ├── Physics/
│   │   │   └── Physics2DSettings.asset
│   │   └── Localization/
│   │       ├── LocalizationSettings.asset
│   │       ├── StringTables/
│   │       │   ├── UI_Strings_en.asset
│   │       │   └── UI_Strings_tr.asset
│   │       └── AssetTables/
│   │
│   └── Tests/
│       ├── EditMode/
│       │   ├── EditMode.asmdef
│       │   ├── HealthSystemTests.cs
│       │   ├── InventoryTests.cs
│       │   └── StateMachineTests.cs
│       └── PlayMode/
│           ├── PlayMode.asmdef
│           ├── PlayerMovementTests.cs
│           └── SaveSystemTests.cs
│
├── Plugins/                         ← Third-party plugins (DOTween, etc.)
│   └── [PluginName]/
│
├── TextMesh Pro/                    ← TMP Essential Resources
│
└── Resources/                       ← ONLY for TMP or absolutely required resources
    └── (minimize usage — prefer Addressables)
```

---

## 2. Folder Naming Rules

| Rule | Example |
|------|---------|
| Use PascalCase for folders | `Scripts/`, `Gameplay/`, `ScriptableObjects/` |
| Use PascalCase for subfolders | `Player/`, `Enemies/`, `Combat/` |
| Prefix project root with underscore | `_Project/` (sorts to top in Project window) |
| Group by feature, not by type | `Player/` contains sprites, anims, prefabs for player |
| Keep nesting to max 4 levels | `_Project/Scripts/Runtime/Gameplay/` |

---

## 3. Asset Naming Conventions

### 3.1 Sprites

| Convention | Example |
|-----------|---------|
| `character_action_frame` | `player_idle_01.png` |
| `tileset_category` | `ground_grass_tileset.png` |
| `ui_element_state` | `btn_play_normal.png` |
| `vfx_effect_type` | `vfx_explosion_01.png` |
| Use lowercase with underscores | `enemy_slime_walk_sheet.png` |

### 3.2 Animations

| Convention | Example |
|-----------|---------|
| `Character_Action` | `Player_Idle.anim` |
| `Character_AnimatorController` | `PlayerAnimatorController.controller` |
| PascalCase for animation files | `Player_Run.anim` |

### 3.3 Prefabs

| Convention | Example |
|-----------|---------|
| PascalCase, descriptive name | `Player.prefab` |
| Variant suffix | `Slime_Fire.prefab` (variant of `Slime.prefab`) |
| System prefabs | `AudioSystem.prefab`, `CameraRig.prefab` |

### 3.4 ScriptableObjects

| Convention | Example |
|-----------|---------|
| `TypeName` + descriptive suffix | `SwordData.asset`, `SlimeConfig.asset` |
| Database assets | `ItemDatabase.asset` |
| Event channels | `OnPlayerDamaged.asset` |
| Config assets | `GameConfig.asset` |

### 3.5 Audio

| Convention | Example |
|-----------|---------|
| Music: `context_BGM.ogg` | `MainMenu_BGM.ogg` |
| SFX: `action_detail.wav` | `player_jump.wav` |
| Use `.ogg` for music (compressed) | `Level01_BGM.ogg` |
| Use `.wav` for SFX (uncompressed source) | `sword_swing.wav` |

### 3.6 Scenes

| Convention | Example |
|-----------|---------|
| PascalCase, purpose-named | `Bootstrap.unity` |
| Level scenes: `Level_XX` | `Level_01.unity` |
| Test scenes: `Test_Feature` | `Test_Combat.unity` |

---

## 4. Assembly Definition Organization

### 4.1 Required Assemblies

```
GameName.Runtime.Core        ← Core systems, interfaces, events, utilities
GameName.Runtime.Gameplay    ← Player, enemies, combat, items, environment
GameName.Runtime.Systems     ← Audio, save, scene management, input, dialogue
GameName.Runtime.UI          ← All UI screens, components, managers
GameName.Editor              ← Custom editors, property drawers, tools
GameName.Tests.EditMode      ← Edit-mode unit tests
GameName.Tests.PlayMode      ← Play-mode integration tests
```

### 4.2 Dependency Rules

```
Dependency Flow (arrows mean "depends on"):

GameName.Runtime.Gameplay  ──→  GameName.Runtime.Core
GameName.Runtime.Systems   ──→  GameName.Runtime.Core
GameName.Runtime.UI        ──→  GameName.Runtime.Core
GameName.Runtime.UI        ──→  GameName.Runtime.Systems (for data binding)
GameName.Editor            ──→  GameName.Runtime.Core
GameName.Editor            ──→  GameName.Runtime.Gameplay
GameName.Tests.EditMode    ──→  GameName.Runtime.Core
GameName.Tests.EditMode    ──→  GameName.Runtime.Gameplay
GameName.Tests.PlayMode    ──→  All Runtime assemblies

FORBIDDEN:
GameName.Runtime.Core      ──✗→  GameName.Runtime.Gameplay (Core NEVER depends on Gameplay)
GameName.Runtime.Core      ──✗→  GameName.Runtime.UI       (Core NEVER depends on UI)
GameName.Runtime.Gameplay  ──✗→  GameName.Runtime.UI       (Gameplay NEVER depends on UI)
Any Runtime assembly       ──✗→  GameName.Editor           (Runtime NEVER depends on Editor)
```

---

## 5. Scene Organization

### 5.1 Scene Hierarchy (in-scene object organization)

```
Scene Hierarchy:
├── --- MANAGERS ---         ← Separator (empty GameObject)
│   ├── GameManager
│   ├── AudioManager
│   └── UIManager
│
├── --- ENVIRONMENT ---      ← Separator
│   ├── Grid
│   │   ├── Ground (Tilemap)
│   │   ├── Walls (Tilemap)
│   │   ├── Platforms (Tilemap)
│   │   └── Decoration (Tilemap)
│   ├── Backgrounds
│   │   ├── ParallaxLayer_0
│   │   ├── ParallaxLayer_1
│   │   └── ParallaxLayer_2
│   └── Props
│       ├── Trees
│       └── Rocks
│
├── --- GAMEPLAY ---         ← Separator
│   ├── Player
│   ├── Enemies
│   │   ├── Slime_01
│   │   └── Skeleton_01
│   ├── Interactables
│   │   ├── Chest_01
│   │   └── Door_01
│   ├── Pickups
│   │   ├── Coin_01
│   │   └── HealthPickup_01
│   ├── Hazards
│   │   ├── Spikes_01
│   │   └── SawBlade_01
│   └── SpawnPoints
│       ├── PlayerSpawn
│       └── EnemySpawn_01
│
├── --- CAMERAS ---          ← Separator
│   ├── MainCamera
│   └── CinemachineCamera
│
├── --- LIGHTING ---         ← Separator
│   ├── GlobalLight2D
│   └── PointLight2D_01
│
├── --- UI ---               ← Separator
│   ├── Canvas_HUD
│   ├── Canvas_Menus
│   └── EventSystem
│
└── --- DEBUG ---            ← Separator (disabled in builds)
    └── DebugCanvas
```

### 5.2 Separator Convention

Use empty GameObjects with `---` prefix/suffix as visual separators:
- Name format: `--- CATEGORY_NAME ---`
- Set Tag to `EditorOnly` so they are stripped from builds
- Do NOT attach any components to separator objects

---

## 6. Prefab Organization

### 6.1 Prefab Rules

| Rule | Details |
|------|---------|
| One prefab per entity type | `Player.prefab`, `Slime.prefab` |
| Use Prefab Variants for skins/types | `Slime_Fire.prefab` (variant of `Slime.prefab`) |
| Use Nested Prefabs for sub-components | Player prefab contains nested weapon prefab |
| Keep prefab hierarchy shallow | Max 3 levels of nesting |
| Document prefab dependencies | List required references in script XML docs |

### 6.2 Prefab Hierarchy Example

```
Player.prefab
├── Player (Root: PlayerController, Rigidbody2D, CapsuleCollider2D)
│   ├── Sprite (SpriteRenderer, Animator)
│   ├── GroundCheck (Transform — used for ground detection)
│   ├── WallCheck (Transform — used for wall detection)
│   ├── AttackPoint (Transform — melee attack origin)
│   ├── InteractionDetector (CircleCollider2D trigger)
│   └── VFX
│       ├── DustParticles (ParticleSystem)
│       └── HitEffect (ParticleSystem)
```

---

## 7. Import Settings

### 7.1 Sprite Import Settings

| Setting | Value |
|---------|-------|
| Texture Type | Sprite (2D and UI) |
| Sprite Mode | Single / Multiple (for sheets) |
| Pixels Per Unit | 16 / 32 / 64 (match your art style) |
| Filter Mode | Point (pixel art) / Bilinear (smooth art) |
| Compression | None (pixel art) / High Quality (smooth art) |
| Max Size | 2048 or 4096 |
| Generate Mip Maps | OFF for 2D games |
| Read/Write | OFF (unless needed for runtime pixel access) |

### 7.2 Audio Import Settings

| Type | Format | Load Type | Compression |
|------|--------|-----------|-------------|
| Music (BGM) | Vorbis | Streaming | Quality: 70% |
| SFX (short) | PCM / ADPCM | Decompress On Load | None / ADPCM |
| SFX (long) | Vorbis | Compressed In Memory | Quality: 85% |
| Ambient | Vorbis | Compressed In Memory | Quality: 70% |

---

## 8. Version Control Rules

### 8.1 .gitignore Essentials

```gitignore
# Unity generated
/[Ll]ibrary/
/[Tt]emp/
/[Oo]bj/
/[Bb]uild/
/[Bb]uilds/
/[Ll]ogs/
/[Uu]ser[Ss]ettings/

# IDE
/.idea/
/.vs/
/.vscode/
*.csproj
*.sln
*.slnx

# OS
.DS_Store
Thumbs.db

# Build
*.apk
*.aab
*.unitypackage

# Crashlytics
crashlytics-build.properties
```

### 8.2 Files to Always Commit

- All `.meta` files
- `ProjectSettings/` folder
- `Packages/manifest.json` and `packages-lock.json`
- All source code, assets, and ScriptableObjects
