# Arrow Swarm — Project Rules

## Project Info
- **Game**: Arrow Swarm
- **Engine**: Unity 6 (6000.3.18f1), URP 2D
- **Platform**: Android, Portrait (1080x1920)
- **Theme**: Minimalist / Modern (dark background, soft pastel accents, glassmorphism UI)

## Mandatory Rules

### Architecture
- All scripts MUST be under `Assets/_Project/Scripts/` with proper subfolder and namespace.
- Use the Singleton<T> base class from `ArrowSwarm.Utils` for all managers.
- Systems communicate via **C# static events** — no direct cross-references between unrelated managers.
- Use **ObjectPool<T>** for Mob and Arrow instantiation — never raw Instantiate/Destroy at runtime.
- All tunable values go in **GameConfig** or **MapData** ScriptableObjects — zero magic numbers.

### Level Generation
- Levels are generated **procedurally at runtime** when Play is pressed.
- No pre-made level data — everything is calculated from the level number.
- **DifficultyCalculator** is a static utility class with pure functions.
- **LevelGenerator** calls DifficultyCalculator, builds the grid, checks solvability.
- Every generated level MUST pass the solvability checker before being playable.

### Code Quality
- Every script must have XML summary comments on the class and public methods.
- Follow the naming conventions in the global Unity skill.
- Max 200 lines per script — split if larger.
- No `Update()` allocations (cache WaitForSeconds, avoid string concat, no LINQ).

### UI
- Use TextMeshPro for ALL text — never legacy Text.
- Two Canvas setup: Canvas_HUD (always visible) + Canvas_Overlay (popups/menus).
- CanvasGroup for show/hide animations — not SetActive.

### Testing
- DebugManager must allow jumping to any level via Inspector.
- Console logs must be prefixed with `[ArrowSwarm]`.
- Use `[System.Diagnostics.Conditional("UNITY_EDITOR")]` for debug-only logging.

## Color Palette (Reference)
```
Background:    #1A1A2E, #16213E
Accent:        #0F3460, #E94560
Text:          #EAEAEA, #A0A0B0
Arrow colors:  #64B5F6, #81C784, #FFB74D, #BA68C8, #F06292
```

## Don't Do
- Don't create scripts outside `Assets/_Project/Scripts/`.
- Don't use `public` fields — use `[SerializeField] private` + properties.
- Don't hardcode level parameters — always derive from DifficultyCalculator.
- Don't use `Resources.Load` — use direct references or addressables.
- Don't use legacy Input system — use the new Input System or EventSystem for touch.
