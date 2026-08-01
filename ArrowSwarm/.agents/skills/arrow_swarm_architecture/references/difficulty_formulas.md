# Arrow Swarm — Difficulty & Level Generation Formulas

## Difficulty Tier
```
difficultyTier = floor((level - 1) / 5) + 1
```
Every 5 levels, difficulty tier increases by 1.
- Level 1-5: Tier 1
- Level 6-10: Tier 2
- Level 996-1000: Tier 200

## Map Selection
```
mapIndex = (level - 1) % 5    // 0-indexed, cycles through 5 maps
```

## Arrow Count
```
arrowCount = min(gridWidth * gridHeight * 0.8, 15 + difficultyTier * 1.5)
```
- Minimum: 15 arrows (Level 1)
- Maximum: 80% of grid area
- Growth: ~1.5 arrows per tier

## Arrow Outward-Facing Chance
"Outward" = arrow's facing direction has a clear path to grid edge (no blocking arrows).
This controls the probability during generation that an arrow faces outward (making it immediately fireable).
```
outwardChance = max(0.20, 0.65 - (difficultyTier - 1) * 0.01)
```
- Level 1: 65% chance arrow faces outward
- Level 250: 20% (minimum floor)
- This makes puzzles harder at higher levels

## Mob HP
```
mobHP = 5 + (difficultyTier - 1) * 2 + floor(difficultyTier / 10) * 5
```
| Level | Tier | Mob HP |
|-------|------|--------|
| 1     | 1    | 5      |
| 50    | 10   | 28     |
| 250   | 50   | 128    |
| 1000  | 200  | 503    |

## Mob Speed
```
mobSpeed = min(maxMobSpeed, 1.0 + (difficultyTier - 1) * 0.1 + floor(difficultyTier / 20) * 0.5)
```
- maxMobSpeed = 20.0 (configurable via GameConfig)
- Level 1: 1.0 units/sec
- Level 50: 1.9
- Level 250: capped at 20.0

## Mob Spawn Interval
```
spawnInterval = max(0.4, 3.0 - (difficultyTier - 1) * 0.05)
```
- Level 1: 3.0 seconds between spawns
- Level 250: 0.55 seconds
- Minimum: 0.4 seconds

## Total Mob Count
```
totalMobs = floor(5 + difficultyTier * 1.2 + floor(difficultyTier / 10) * 3)
```
| Level | Tier | Mobs |
|-------|------|------|
| 1     | 1    | 6    |
| 50    | 10   | 20   |
| 250   | 50   | 80   |
| 1000  | 200  | 305  |

## Arrow Weight Range
```
minWeight = min(5, 1 + floor(difficultyTier / 20))
maxWeight = min(10, 3 + floor(difficultyTier / 10))
```
| Level | Tier | Min Weight | Max Weight |
|-------|------|------------|------------|
| 1     | 1    | 1          | 3          |
| 50    | 10   | 1          | 4          |
| 250   | 50   | 3          | 8          |
| 1000  | 200  | 5          | 10         |

## Solvability Check Algorithm
After level generation, validate that all arrows CAN be fired:

```
1. Create a copy of the grid state.
2. Find all arrows with clear forward path (fireable arrows).
3. Remove those arrows from the grid.
4. Repeat: find newly fireable arrows (removing previous arrows may clear paths).
5. Continue until no more arrows can be fired.
6. If grid is empty → SOLVABLE.
7. If grid still has arrows but none are fireable → UNSOLVABLE → regenerate.
```

Maximum regeneration attempts: 100. If all fail, reduce difficulty by 10% and retry.

## Winnability Check
```
totalArrowWeight = sum of all arrow weights
totalMobHP = totalMobs * mobHP

Condition: totalArrowWeight >= totalMobHP * 0.6
```
This ensures that if the player fires all arrows optimally, they can kill enough mobs to survive.

## Level Generation Pipeline
```
1. Get currentLevel
2. Calculate difficultyTier
3. Select map (mapIndex)
4. Calculate all parameters (arrow count, mob stats, etc.)
5. Place arrows on grid with random positions
6. Assign directions (using outwardChance probability)
7. Assign weights (random within minWeight-maxWeight)
8. Run solvability check → regenerate if failed
9. Run winnability check → adjust weights if failed
10. Return LevelData struct
```
