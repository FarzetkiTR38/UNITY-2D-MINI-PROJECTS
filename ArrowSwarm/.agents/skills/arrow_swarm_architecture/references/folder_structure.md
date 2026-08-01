# Arrow Swarm — Folder & File Naming Rules

## Script File Rules

### Naming
- File name MUST match class name exactly: `GridManager.cs` → `public class GridManager`
- One class per file (exceptions: nested private classes, small data structs)
- Interfaces get their own file: `ICloudService.cs`
- Enums get their own file if used across multiple scripts: `GameState.cs`

### Placement
| Script Type | Folder | Namespace |
|------------|--------|-----------|
| Managers (Singleton) | `Scripts/Core/` | `ArrowSwarm.Core` |
| Grid system | `Scripts/Grid/` | `ArrowSwarm.Grid` |
| Arrow scripts | `Scripts/Arrow/` | `ArrowSwarm.Arrow` |
| Mob scripts | `Scripts/Mob/` | `ArrowSwarm.Mob` |
| Path scripts | `Scripts/Path/` | `ArrowSwarm.Path` |
| Camera | `Scripts/Camera/` | `ArrowSwarm.Camera` |
| UI scripts | `Scripts/UI/` | `ArrowSwarm.UI` |
| Data/Save | `Scripts/Data/` | `ArrowSwarm.Data` |
| Tip system | `Scripts/Tips/` | `ArrowSwarm.Tips` |
| Ad system | `Scripts/Ads/` | `ArrowSwarm.Ads` |
| Audio | `Scripts/Audio/` | `ArrowSwarm.Audio` |
| VFX | `Scripts/Effects/` | `ArrowSwarm.Effects` |
| Debug tools | `Scripts/Debug/` | `ArrowSwarm.Debug` |
| Utilities | `Scripts/Utils/` | `ArrowSwarm.Utils` |

## Asset Naming Conventions

### Prefabs
- PascalCase matching the primary script: `Arrow.prefab`, `Mob.prefab`
- UI prefabs prefixed: `UI_PauseMenu.prefab`, `UI_GameHUD.prefab`
- Particle prefabs: `FX_ArrowTrail.prefab`, `FX_MobDeath.prefab`

### Sprites
- lowercase_snake_case: `arrow_blue.png`, `mob_caterpillar.png`
- Organized by type: `Sprites/Arrows/`, `Sprites/Mobs/`, `Sprites/UI/`

### Audio
- lowercase_snake_case: `sfx_arrow_fire.wav`, `bgm_menu.mp3`
- SFX in `Audio/SFX/`, Music in `Audio/Music/`

### ScriptableObjects
- PascalCase with type suffix: `GameConfig.asset`, `Map1_Forest.asset`
- All in `ScriptableObjects/` folder, subfolder by type

### Scenes
- PascalCase with "Scene" suffix: `BootScene.unity`, `MainMenuScene.unity`, `GameScene.unity`

### Animations
- PascalCase describing the action: `ArrowPulse.anim`, `MobWalk.anim`
- Controllers: `ArrowAnimator.controller`, `MobAnimator.controller`

### Materials
- PascalCase with "Mat" suffix: `ArrowGlowMat.mat`, `RainbowMat.mat`

## Scene Hierarchy Organization
```
GameScene hierarchy:
├── --- MANAGERS ---
│   ├── GameManager
│   ├── LevelManager
│   ├── AudioManager
│   └── ParticleManager
├── --- CAMERA ---
│   └── Main Camera (+ CameraController + ZoomController)
├── --- GAMEPLAY ---
│   ├── Grid (parent for GridManager + GridVisualizer)
│   │   └── [GridCells spawned here]
│   ├── Arrows (parent for ArrowSpawner)
│   │   └── [Arrow instances spawned here]
│   ├── Path (parent for PathManager + PathVisualizer)
│   └── Mobs (parent for MobSpawner)
│       └── [Mob instances spawned here]
├── --- UI ---
│   ├── Canvas_HUD (Screen Space Overlay)
│   │   ├── TopBar (Level, Lives, Tips)
│   │   ├── BottomBar (Zoom slider, Arrow count)
│   │   └── PauseButton
│   ├── Canvas_Overlay (for popups, pause menu)
│   │   ├── PauseMenu
│   │   ├── LevelComplete
│   │   ├── GameOver
│   │   └── TipPopup
│   └── Canvas_World (for in-game UI like HP text)
└── --- ENVIRONMENT ---
    └── Background
```

## Assembly Definitions (Optional but Recommended)
```
ArrowSwarm.Core.asmdef       → References: ArrowSwarm.Utils
ArrowSwarm.Grid.asmdef       → References: ArrowSwarm.Core, ArrowSwarm.Utils
ArrowSwarm.Arrow.asmdef      → References: ArrowSwarm.Core, ArrowSwarm.Grid, ArrowSwarm.Path, ArrowSwarm.Utils
ArrowSwarm.Mob.asmdef        → References: ArrowSwarm.Core, ArrowSwarm.Path, ArrowSwarm.Utils
ArrowSwarm.UI.asmdef         → References: ArrowSwarm.Core, ArrowSwarm.Data
ArrowSwarm.Utils.asmdef      → No project references (standalone)
```
Assembly definitions speed up compilation — only recompile changed assemblies.
