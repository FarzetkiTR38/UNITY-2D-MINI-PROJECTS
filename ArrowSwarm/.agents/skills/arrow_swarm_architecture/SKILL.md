---
name: Arrow Swarm Game Architecture
description: Project-specific architecture and rules for the Arrow Swarm Unity 2D mobile game. Covers game systems (arrow, mob, grid, path, level generation), namespace structure, folder conventions, difficulty algorithm, and inter-system communication patterns.
---

# Arrow Swarm — Project Architecture Skill

## Project Overview
Arrow Swarm is a Unity 2D mobile game (Portrait, Android) combining arrow puzzles with tower defense. Players tap arrows to fire them at mobs moving along a rectangular path. Levels are procedurally generated based on level number with infinite progression.

**Engine:** Unity 6 (6000.3.18f1) | **Language:** C# | **Render Pipeline:** URP 2D

## Namespace Structure
```
ArrowSwarm.Core        → GameManager, LevelManager, LevelGenerator, DifficultyCalculator
ArrowSwarm.Grid        → GridManager, GridCell, GridVisualizer
ArrowSwarm.Arrow       → Arrow, ArrowSpawner, ArrowMovement, ArrowVisuals
ArrowSwarm.Mob         → Mob, MobSpawner, MobMovement, MobHealth, MobVisuals
ArrowSwarm.Path        → PathManager, PathFollower, PathVisualizer
ArrowSwarm.Camera      → CameraController, ZoomController
ArrowSwarm.UI          → All UI scripts (MainMenuUI, GameHUD, etc.)
ArrowSwarm.Data        → PlayerData, DataManager, ICloudService, MockCloudService
ArrowSwarm.Tips        → TipManager, TipHighlighter
ArrowSwarm.Ads         → IAdService, MockAdService
ArrowSwarm.Audio       → AudioManager, SFXLibrary
ArrowSwarm.Effects     → ParticleManager, ScreenEffects
ArrowSwarm.Debug       → DebugManager
ArrowSwarm.Utils       → Singleton<T>, ObjectPool<T>, Extensions
```

## Folder Structure
```
Assets/_Project/
├── Scripts/          → All game scripts (mirrors namespace structure)
├── ScriptableObjects/→ GameConfig, MapData (5 maps)
├── Prefabs/          → Arrow, Mob, GridCell, UI elements
├── Art/Sprites/      → Arrows/, Mobs/, UI/, Backgrounds/
├── Art/Fonts/        → Poppins, Inter, RobotoMono (TMP)
├── Art/Materials/    → Glow materials, rainbow shader
├── Audio/Music/      → BGM tracks
├── Audio/SFX/        → Sound effects
├── Particles/        → Particle system prefabs
├── Animations/       → Animation clips and controllers
└── Scenes/           → BootScene, MainMenuScene, GameScene
```

## Core Systems & Communication

### System Dependencies (Event-Driven)
```
GameManager (state machine) ─── broadcasts GameState changes
    ↓ events
LevelManager ─── calls LevelGenerator.Generate(level)
    ↓ events
GridManager ─── places arrows on grid via ArrowSpawner
PathManager ─── defines waypoints for mob movement
MobSpawner ─── spawns mobs on schedule via ObjectPool
    ↓ events
Arrow (on click) ─── fires if path clear, damages mobs on path
    ↓ events
GameHUD ─── updates UI (lives, arrow count, etc.)
AudioManager ─── plays SFX based on events
ParticleManager ─── spawns effects based on events
```

### Key Rules
1. **All inter-system communication via C# static events** — no direct manager references between systems.
2. **LevelGenerator is pure logic** — takes level number, returns LevelData struct. No MonoBehaviour dependency.
3. **DifficultyCalculator is static** — pure math functions, no state.
4. **Object pooling mandatory** for Mob and Arrow prefabs.
5. **GameConfig ScriptableObject** holds all tunable parameters — no magic numbers in code.
6. **MapData ScriptableObject** per map (5 total) — grid size, waypoints, colors.

## Game State Flow
```
Loading → Menu → Playing → (Paused) → Win/Lose → Menu
```

## References
- **Game Mechanics Detail**: See `references/game_mechanics.md` for arrow firing, mob pathing, damage rules.
- **Difficulty Algorithm**: See `references/difficulty_formulas.md` for all mathematical formulas and scaling.
- **Folder & File Rules**: See `references/folder_structure.md` for naming and organization rules.
