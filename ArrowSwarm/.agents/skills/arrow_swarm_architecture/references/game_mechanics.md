# Arrow Swarm — Game Mechanics Detail

## Map Structure
- Map is a **rectangle**. The **perimeter** is the mob path. The **interior** is the arrow grid.
- 5 different maps cycle: `mapIndex = (level - 1) % 5`
- Each map has its own grid dimensions, waypoints, spawn/finish points.
- Mobs move **counter-clockwise** around the perimeter.

## Arrow Mechanics

### Placement
- Arrows sit on a regular grid inside the rectangle.
- Each grid cell holds at most 1 arrow.
- Not all cells are filled — arrow count is dynamic per level.
- Each arrow has: direction (Up/Down/Left/Right), weight (1-10), color (by weight), grid position (row, col).

### Firing Rules
1. Player taps an arrow.
2. System checks: **is the path clear** in the arrow's facing direction?
   - "Clear" means: no other arrow between this arrow and the grid edge in its facing direction.
3. **If clear**: Arrow fires.
   a. Arrow moves visually in its facing direction until it exits the grid.
   b. Upon reaching the path (perimeter), arrow turns to face **opposite to mob movement direction** (toward spawn point).
   c. Arrow travels along the path toward spawn point.
   d. Arrow damages every mob it passes **behind it** (between arrow and spawn point) for its weight value.
   e. Arrow is destroyed when it reaches the spawn point area.
4. **If blocked** (another arrow is in the way):
   a. Arrow does NOT fire.
   b. Player loses 1 life.
   c. Red flash screen effect + error SFX.

### Rainbow (Last) Arrow
- When only 1 arrow remains and is fired, it becomes a **rainbow arrow**.
- Rainbow arrow deals 999 damage to ALL remaining mobs.
- Special visual effects: rainbow color cycling trail, sparkle particles.
- Level completion triggers immediately after.

## Mob Mechanics

### Spawning
- Mobs spawn continuously at the spawn point, one at a time, at intervals.
- Spawn interval decreases with difficulty (faster spawns at higher levels).
- Total mob count is determined by level difficulty.

### Movement
- Mobs follow the rectangular path counter-clockwise via waypoints.
- Movement is smooth (interpolated between waypoints).
- Mobs can turn corners smoothly.

### Health
- Each mob has HP displayed as text above it (TextMeshPro).
- When hit by arrow: HP -= arrow weight. Text updates with shake animation.
- When HP <= 0: mob is destroyed with burst particle effect.

### Reaching Finish
- If a mob reaches the finish point: player loses 1 life, mob is removed.
- Warning SFX plays.

## Win/Lose Conditions

### WIN
- All arrows successfully fired (tapped and launched).
- Remaining mobs are destroyed by rainbow arrow effect.
- Level complete screen shows.

### LOSE (shared life pool = 3 lives)
- Wrong tap (blocked arrow): -1 life
- Mob reaches finish: -1 life
- 0 lives = Game Over

## Damage Direction Clarification
Arrow only damages mobs that are **between the arrow's current position and the spawn point** along the path. Mobs ahead of the arrow (toward finish point) are NOT damaged. The arrow moves toward the spawn point, hitting mobs in its path as it goes.

## Tip (Hint) System
- Tap tip button → highlights the best available arrow (fires and does most damage).
- Uses 1 tip charge per use.
- Default: 3 tips. Daily login: +1. Watch ad (mock): +1.
- If 0 tips: prompt to watch ad.
