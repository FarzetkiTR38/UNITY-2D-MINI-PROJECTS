namespace ArrowSwarm.Core
{
    using System.Collections.Generic;
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Generates levels procedurally based on level number.
    /// Creates multi-point arrow paths using random walk on the point grid.
    /// Validates with SolvabilityChecker before returning.
    /// Pure logic — no MonoBehaviour dependency.
    /// </summary>
    public static class LevelGenerator
    {
        /// <summary>
        /// Holds all data needed to play a generated level.
        /// </summary>
        public struct LevelData
        {
            public int Level;
            public LevelParams Params;
            public MapData Map;
            public List<SolvabilityChecker.ArrowPlacement> ArrowPlacements;
            public bool IsValid;
            public int GenerationAttempts;
        }

        // Cached direction vectors for random walk
        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        /// <summary>
        /// Probability bias for spawning arrows in the upper half of the weight range.
        /// Defaults to 0.50f (~50% upper, ~50% lower).
        /// </summary>
        public static float TargetUpperWeightRatio = 0.50f;
        public static string LastDiag = "";

        /// <summary>
        /// Generates a complete level. Returns LevelData with arrow placements.
        /// Guarantees solvability (retries up to maxAttempts).
        /// </summary>
        public static LevelData Generate(int level, GameConfig config)
        {
            MapData map = config.GetMapForLevel(level);
            if (map == null)
            {
                Debug.LogError($"[ArrowSwarm] LevelGenerator: No map found for level {level}!");
                return new LevelData { IsValid = false };
            }

            if (level <= 0)
            {
                return GenerateTutorialLevel(map, config, 0);
            }

            LevelParams levelParams = DifficultyCalculator.CalculateAll(
                level, map.GridWidth, map.GridHeight,
                config.MaxMobSpeed, config.MinSpawnInterval);

            int maxAttempts = config.MaxRegenerateAttempts;
            float winabilityRatio = config.WinabilityRatio;
            float difficultyReduction = config.DifficultyReductionOnFail;

            // Cap totalMobHP to the grid's physical damage budget so generation is always winnable
            int maxGridCapacity = map.GridWidth * map.GridHeight;
            int maxTheoreticalDamage = Mathf.FloorToInt(maxGridCapacity * 0.82f);
            int maxAllowedMobHP = Mathf.FloorToInt(maxTheoreticalDamage / winabilityRatio);
            int totalMobHP = Mathf.Min(levelParams.TotalMobs * levelParams.MobHP, maxAllowedMobHP);

            LevelData result = new LevelData
            {
                Level = level,
                Params = levelParams,
                Map = map,
                IsValid = false,
                GenerationAttempts = 0
            };

            // Try generating a valid level
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                result.GenerationAttempts = attempt;

                var placements = GenerateArrowPlacements(
                    levelParams, map.GridWidth, map.GridHeight);

                if (placements == null || placements.Count == 0) continue;

                if (SolveDirectionDeadlocks(placements, map.GridWidth, map.GridHeight, totalMobHP, winabilityRatio))
                {
                    DeepenPuzzleDependencies(placements, map.GridWidth, map.GridHeight, totalMobHP, winabilityRatio);

                    var solvedCheck = SolvabilityChecker.Check(placements, map.GridWidth, map.GridHeight, totalMobHP, winabilityRatio);
                    float maxInitialRatio = attempt < 4 ? (placements.Count > 60 ? 0.22f : 0.18f) : (attempt < 8 ? 0.28f : 0.35f);
                    int targetMaxInitial = Mathf.Max(2, Mathf.RoundToInt(placements.Count * maxInitialRatio));

                    // If quality criteria met (or on attempt >= 8), accept!
                    if (solvedCheck.IsValid && solvedCheck.InitialUnblockedCount >= 2 && (solvedCheck.InitialUnblockedCount <= targetMaxInitial + 1 || attempt >= 8 || attempt == maxAttempts))
                    {
                        AssignHarmoniousArrowColors(placements, map.GridWidth, map.GridHeight, config?.ArrowColors?.Length ?? 5);
                        result.ArrowPlacements = placements;
                        result.IsValid = true;

                        LogDebug($"Level {level} generated & deepened: {levelParams} (initialUnblocked={solvedCheck.InitialUnblockedCount}/{placements.Count}, steps={solvedCheck.FiringSteps}, attempt {attempt}/{maxAttempts})");
                        return result;
                    }
                }
            }

            // Fallback: Guaranteed 100% solvable outward orientation
            LogDebug($"Level {level}: Applying guaranteed solvable outward placement fallback.");
            for (int fbAttempt = 0; fbAttempt < 5; fbAttempt++)
            {
                var fallbackPlacements = GenerateArrowPlacements(levelParams, map.GridWidth, map.GridHeight);
                if (fallbackPlacements != null && fallbackPlacements.Count > 0)
                {
                    if (SolveDirectionDeadlocks(fallbackPlacements, map.GridWidth, map.GridHeight, totalMobHP, winabilityRatio))
                    {
                        DeepenPuzzleDependencies(fallbackPlacements, map.GridWidth, map.GridHeight, totalMobHP, winabilityRatio);
                        var fbCheck = SolvabilityChecker.Check(fallbackPlacements, map.GridWidth, map.GridHeight, totalMobHP, winabilityRatio);
                        if (fbCheck.IsValid && fbCheck.InitialUnblockedCount >= 2)
                        {
                            AssignHarmoniousArrowColors(fallbackPlacements, map.GridWidth, map.GridHeight, config?.ArrowColors?.Length ?? 5);
                            result.ArrowPlacements = fallbackPlacements;
                            result.IsValid = true;
                            return result;
                        }
                    }
                }
            }
            
            var simplePlacements = GenerateSimpleGridPlacements(map.GridWidth, map.GridHeight);
            AssignHarmoniousArrowColors(simplePlacements, map.GridWidth, map.GridHeight, config?.ArrowColors?.Length ?? 5);
            result.ArrowPlacements = simplePlacements;
            result.IsValid = true;
            return result;
        }

        private static List<SolvabilityChecker.ArrowPlacement> GenerateSimpleGridPlacements(int width, int height)
        {
            var placements = new List<SolvabilityChecker.ArrowPlacement>();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width - 1; x += 2)
                {
                    var path = new List<Vector2Int>
                    {
                        new Vector2Int(x, y),
                        new Vector2Int(x + 1, y)
                    };
                    placements.Add(new SolvabilityChecker.ArrowPlacement(path, ArrowDirection.Left));
                }
            }
            return placements;
        }

        /// <summary>
        /// Generates 100% solvable organic mazes using Reverse Disassembly (Backwards Carving).
        /// Creates interlocking U-loops, spirals, S-curves, and varied arrow lengths matching
        /// the reference games (Level 2, Level 35, Level 5).
        /// </summary>
        private static List<SolvabilityChecker.ArrowPlacement> GenerateArrowPlacements(
            LevelParams levelParams, int gridWidth, int gridHeight)
        {
            return GenerateReverseDisassemblyPlacements(levelParams, gridWidth, gridHeight);
        }

        private static List<SolvabilityChecker.ArrowPlacement> GenerateReverseDisassemblyPlacements(
            LevelParams levelParams, int gridWidth, int gridHeight)
        {
            var placements = new List<SolvabilityChecker.ArrowPlacement>();
            int[,] cellOwner = new int[gridWidth, gridHeight];
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    cellOwner[x, y] = -1;
                }
            }

            bool[,] occupied = new bool[gridWidth, gridHeight];
            int totalCells = gridWidth * gridHeight;
            int filledCells = 0;

            int minLength = Mathf.Max(2, levelParams.MinWeight + 1);
            int maxLength = Mathf.Max(minLength, levelParams.MaxWeight + 1);

            int maxLoopIterations = totalCells * 3;
            int loopCount = 0;
            int consecutiveFails = 0;

            while (filledCells < totalCells && loopCount++ < maxLoopIterations)
            {
                Vector2Int headPos = Vector2Int.zero;
                ArrowDirection headDir = ArrowDirection.Up;
                bool foundHead = false;

                // Priority 1: Boundary cell facing outward (free exit)
                var freeBoundary = GetFreeBoundaryCandidates(gridWidth, gridHeight, occupied);
                var validBoundary = freeBoundary.FindAll(c => 
                    !CreatesHeadToHeadConflict(c.pos, c.dir, placements) &&
                    !CreatesPerimeterCluster(c.pos, c.dir, placements, gridWidth, gridHeight));

                if (validBoundary.Count > 0 && (placements.Count == 0 || Random.value < 0.32f))
                {
                    var cand = validBoundary[Random.Range(0, validBoundary.Count)];
                    headPos = cand.pos;
                    headDir = cand.dir;
                    foundHead = true;
                }
                else if (freeBoundary.Count > 0 && placements.Count == 0)
                {
                    var cand = freeBoundary[Random.Range(0, freeBoundary.Count)];
                    headPos = cand.pos;
                    headDir = cand.dir;
                    foundHead = true;
                }
                else
                {
                    // Priority 2: Interior cell - strictly prioritize candidates whose fire ray to boundary has minimal collisions
                    var interiorCands = GetInteriorCandidates(gridWidth, gridHeight, occupied);
                    var validInterior = interiorCands.FindAll(c => !CreatesHeadToHeadConflict(c.pos, c.dir, placements));
                    if (validInterior.Count > 0)
                    {
                        int minCollisions = int.MaxValue;
                        var bestCands = new List<CandidateHead>();

                        for (int cIdx = 0; cIdx < validInterior.Count; cIdx++)
                        {
                            var c = validInterior[cIdx];
                            int col = CountRayCollisions(c.pos, c.dir, gridWidth, gridHeight, occupied);
                            if (col < minCollisions)
                            {
                                minCollisions = col;
                                bestCands.Clear();
                                bestCands.Add(c);
                            }
                            else if (col == minCollisions)
                            {
                                bestCands.Add(c);
                            }
                        }

                        var cand = bestCands[Random.Range(0, bestCands.Count)];
                        headPos = cand.pos;
                        headDir = cand.dir;
                        foundHead = true;
                    }
                    else if (validBoundary.Count > 0)
                    {
                        var cand = validBoundary[Random.Range(0, validBoundary.Count)];
                        headPos = cand.pos;
                        headDir = cand.dir;
                        foundHead = true;
                    }
                }

                if (!foundHead)
                {
                    var remaining = GetUnoccupiedCells(gridWidth, gridHeight, occupied);
                    if (remaining.Count == 0) break;

                    headPos = remaining[Random.Range(0, remaining.Count)];
                    headDir = GetBestOutwardDir(headPos, gridWidth, gridHeight, occupied);
                }

                int targetLength = PickTargetArrowLength(levelParams.MinWeight, levelParams.MaxWeight, placements);
                var path = GrowArrowPathBackwards(headPos, headDir, targetLength, gridWidth, gridHeight, occupied);

                int minAcceptableLength = Mathf.Max(3, levelParams.MinWeight + 1);

                if (path != null && path.Count >= minAcceptableLength)
                {
                    consecutiveFails = 0;
                    int arrowIdx = placements.Count;
                    placements.Add(new SolvabilityChecker.ArrowPlacement(path, headDir));

                    foreach (var pt in path)
                    {
                        if (cellOwner[pt.x, pt.y] == -1)
                        {
                            cellOwner[pt.x, pt.y] = arrowIdx;
                            occupied[pt.x, pt.y] = true;
                            filledCells++;
                        }
                    }
                }
                else
                {
                    consecutiveFails++;
                    if ((consecutiveFails >= 25 && filledCells >= totalCells * 0.70f) || consecutiveFails >= 40)
                    {
                        // Board is densely packed and cannot carve long paths anymore;
                        // let FillAllUnownedCells absorb all remaining cells into existing arrows!
                        break;
                    }

                    // Single cell or failed to reach min acceptable length: attach to existing placement if possible
                    if (cellOwner[headPos.x, headPos.y] == -1)
                    {
                        bool attached = AttachIsolatedCellToPlacement(headPos, placements, cellOwner, gridWidth, gridHeight, false);
                        if (attached)
                        {
                            occupied[headPos.x, headPos.y] = true;
                            filledCells++;
                        }
                    }
                }
            }

            int w1Loop = 0; foreach (var p in placements) if (p.Weight == 1) w1Loop++;

            // Post-process: Guarantee zero head-to-head conflicts
            RemoveHeadToHeadConflicts(placements, gridWidth, gridHeight);

            // Guarantee zero diagonal steps / zigzags anywhere on the map
            FixDiagonalSegments(placements);

            int w1BeforeFill = 0; foreach (var p in placements) if (p.Weight == 1) w1BeforeFill++;

            // Sweep and absorb 100% of unowned grid points so 0 empty dots remain
            FillAllUnownedCells(placements, gridWidth, gridHeight);

            int w1AfterFill = 0; foreach (var p in placements) if (p.Weight == 1) w1AfterFill++;

            // Consolidate excess short arrows (especially weight 1) into adjacent arrows
            MergeExcessShortArrows(placements, levelParams.MaxWeight, gridWidth, gridHeight);

            int w1AfterMerge = 0; foreach (var p in placements) if (p.Weight == 1) w1AfterMerge++;

            // Strictly enforce 100% head-to-body direction alignment (Golden Axiom)
            EnforceStrictHeadAlignment(placements);

            // Fix arrows whose head direction fires into their own body (self-blocking)
            FixSelfBlockingArrows(placements, gridWidth, gridHeight);

            // Re-check head-to-head after alignment and self-blocking fixes
            RemoveHeadToHeadConflicts(placements, gridWidth, gridHeight);

            // Simulation-based greedy resolver for any remaining deadlocks
            ResolveDeadlockedArrows(placements, gridWidth, gridHeight);

            // Audit zero overlaps and full grid coverage
            SolvabilityChecker.ValidateNoOverlaps(placements);

            int w1End = 0; foreach (var p in placements) if (p.Weight == 1) w1End++;
            LastDiag = $"w1Loop={w1Loop}, w1BeforeFill={w1BeforeFill}, w1AfterFill={w1AfterFill}, w1AfterMerge={w1AfterMerge}, w1End={w1End}, Total={placements.Count}";
            Debug.Log($"[ArrowSwarm DIAG] {LastDiag}");

            return placements;
        }

        /// <summary>
        /// Strictly enforces that every arrow placement's HeadDirection is 100% aligned
        /// with the vector pointing from path[1] to path[0] (the first body segment).
        /// Absolutely guarantees ZERO perpendicular or sideways arrowheads!
        /// </summary>
        private static void EnforceStrictHeadAlignment(List<SolvabilityChecker.ArrowPlacement> placements)
        {
            if (placements == null) return;
            for (int i = 0; i < placements.Count; i++)
            {
                var p = placements[i];
                if (p.PathPoints != null && p.PathPoints.Count >= 2)
                {
                    p.HeadDirection = VectorToDirection(p.PathPoints[0] - p.PathPoints[1]);
                    placements[i] = p;
                }
            }
        }

        /// <summary>
        /// Checks if an arrow's fire line passes through its own body segments.
        /// A self-blocking arrow can never be fired because it blocks itself.
        /// </summary>
        private static bool IsSelfBlocking(
            SolvabilityChecker.ArrowPlacement placement, int gridWidth, int gridHeight)
        {
            var path = placement.PathPoints;
            if (path == null || path.Count < 2) return false;

            Vector2Int head = path[0];
            Vector2Int step = ArrowSwarm.Grid.GridManager.DirectionToVector(placement.HeadDirection);
            if (step == Vector2Int.zero) return true;

            // Build set of own body points (excluding head)
            var ownBody = new HashSet<Vector2Int>();
            for (int i = 1; i < path.Count; i++)
            {
                ownBody.Add(path[i]);
            }

            // Trace fire line from head — if it hits own body, arrow is self-blocking
            Vector2Int current = head + step;
            while (current.IsInBounds(gridWidth, gridHeight))
            {
                if (ownBody.Contains(current)) return true;
                current += step;
            }

            return false;
        }

        /// <summary>
        /// Detects and fixes arrows whose head direction causes them to fire into
        /// their own body. Fix: reverse the path so the other end becomes the head.
        /// Path shapes are 100% preserved — only the head endpoint changes.
        /// Uses deep copy to avoid shared PathPoints reference corruption.
        /// </summary>
        private static void FixSelfBlockingArrows(
            List<SolvabilityChecker.ArrowPlacement> placements, int gridWidth, int gridHeight)
        {
            if (placements == null) return;

            for (int i = 0; i < placements.Count; i++)
            {
                if (!IsSelfBlocking(placements[i], gridWidth, gridHeight)) continue;

                // Save original state (deep copy path because Reverse is in-place)
                var originalPath = new List<Vector2Int>(placements[i].PathPoints);
                ArrowDirection originalDir = placements[i].HeadDirection;

                // Try flipping the arrow (reverse path, other end becomes head with natural direction)
                var flipped = placements[i];
                FlipArrowOrientation(ref flipped);

                if (!IsSelfBlocking(flipped, gridWidth, gridHeight))
                {
                    placements[i] = flipped;
                }
                else
                {
                    // Flipped is also self-blocking, restore original state
                    RestoreArrowState(placements, i, originalPath, originalDir);
                }
            }
        }

        /// <summary>
        /// Checks if placing an arrow head at headPos with direction headDir would create
        /// a mutual head-to-head deadlock on the same row or column with any existing placement.
        /// </summary>
        private static bool CreatesHeadToHeadConflict(
            Vector2Int headPos, ArrowDirection headDir,
            List<SolvabilityChecker.ArrowPlacement> placements,
            int ignoreIndex = -1)
        {
            if (placements == null) return false;

            for (int i = 0; i < placements.Count; i++)
            {
                if (i == ignoreIndex) continue;

                var p = placements[i];
                Vector2Int h = p.HeadPoint;
                ArrowDirection d = p.HeadDirection;

                // Same row check (Y axis identical)
                if (h.y == headPos.y)
                {
                    if (headPos.x < h.x && headDir == ArrowDirection.Right && d == ArrowDirection.Left) return true;
                    if (headPos.x > h.x && headDir == ArrowDirection.Left && d == ArrowDirection.Right) return true;
                }

                // Same column check (X axis identical)
                if (h.x == headPos.x)
                {
                    if (headPos.y < h.y && headDir == ArrowDirection.Up && d == ArrowDirection.Down) return true;
                    if (headPos.y > h.y && headDir == ArrowDirection.Down && d == ArrowDirection.Up) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a point lies in the line of fire of an arrow's head.
        /// </summary>
        private static bool IsInFireRay(Vector2Int pt, Vector2Int head, ArrowDirection dir, int width, int height)
        {
            Vector2Int step = ArrowSwarm.Grid.GridManager.DirectionToVector(dir);
            if (step == Vector2Int.zero) return false;

            Vector2Int curr = head + step;
            while (curr.IsInBounds(width, height))
            {
                if (curr == pt) return true;
                curr += step;
            }
            return false;
        }

        /// <summary>
        /// Checks if a single arrow can fire: path from head in HeadDirection
        /// must be clear of any occupied points all the way to the grid edge.
        /// </summary>
        private static bool CanFireArrow(
            SolvabilityChecker.ArrowPlacement arrow,
            int gridWidth, int gridHeight,
            HashSet<Vector2Int> occupied)
        {
            Vector2Int current = arrow.HeadPoint;
            Vector2Int step = ArrowSwarm.Grid.GridManager.DirectionToVector(arrow.HeadDirection);
            if (step == Vector2Int.zero) return false;

            current += step;
            while (current.IsInBounds(gridWidth, gridHeight))
            {
                if (occupied.Contains(current)) return false;
                current += step;
            }
            return true;
        }

        /// <summary>
        /// Simulates the iterative arrow firing process and returns the indices
        /// of arrows that could NOT fire (stuck in deadlock).
        /// </summary>
        private static List<int> FindStuckArrowIndices(
            List<SolvabilityChecker.ArrowPlacement> placements,
            int gridWidth, int gridHeight)
        {
            var occupied = new HashSet<Vector2Int>();
            var remainingIndices = new List<int>();

            for (int i = 0; i < placements.Count; i++)
            {
                remainingIndices.Add(i);
                var pts = placements[i].PathPoints;
                if (pts == null) continue;
                for (int j = 0; j < pts.Count; j++)
                {
                    occupied.Add(pts[j]);
                }
            }

            bool progress = true;
            while (progress && remainingIndices.Count > 0)
            {
                progress = false;
                for (int i = remainingIndices.Count - 1; i >= 0; i--)
                {
                    int idx = remainingIndices[i];
                    if (CanFireArrow(placements[idx], gridWidth, gridHeight, occupied))
                    {
                        var pts = placements[idx].PathPoints;
                        for (int j = 0; j < pts.Count; j++)
                        {
                            occupied.Remove(pts[j]);
                        }
                        remainingIndices.RemoveAt(i);
                        progress = true;
                    }
                }
            }

            return remainingIndices;
        }

        /// <summary>
        /// Simulation-based greedy resolver for deadlocked arrows.
        /// Runs the firing simulation to find stuck arrows, then tries flipping
        /// each one. Keeps flips that reduce the stuck count, undoes the rest.
        /// Uses deep copy for safe undo (PathPoints is a shared reference).
        /// Repeats until all arrows can fire or no more progress is made.
        /// Path shapes are 100% preserved — only head direction changes.
        /// </summary>
        private static void ResolveDeadlockedArrows(
            List<SolvabilityChecker.ArrowPlacement> placements,
            int gridWidth, int gridHeight)
        {
            if (placements == null || placements.Count == 0) return;

            int maxIterations = Mathf.Max(120, placements.Count * 2);
            int noProgressCount = 0;
            int maxNoProgress = Mathf.Max(15, placements.Count / 5);

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                var stuckIndices = FindStuckArrowIndices(placements, gridWidth, gridHeight);
                if (stuckIndices.Count == 0) return; // All arrows can fire!

                bool improved = false;

                for (int s = 0; s < stuckIndices.Count; s++)
                {
                    int stuckIdx = stuckIndices[s];

                    // Deep-copy the original state for safe undo
                    var originalPath = new List<Vector2Int>(placements[stuckIdx].PathPoints);
                    ArrowDirection originalDir = placements[stuckIdx].HeadDirection;

                    // Flip this stuck arrow
                    var flipped = placements[stuckIdx];
                    FlipArrowOrientation(ref flipped);

                    // Skip if flip creates self-blocking
                    if (IsSelfBlocking(flipped, gridWidth, gridHeight))
                    {
                        // Restore original state (in-place Reverse corrupted the shared list)
                        RestoreArrowState(placements, stuckIdx, originalPath, originalDir);
                        continue;
                    }

                    placements[stuckIdx] = flipped;

                    // Check if this improved the situation
                    var newStuck = FindStuckArrowIndices(placements, gridWidth, gridHeight);
                    if (newStuck.Count < stuckIndices.Count)
                    {
                        // Strict improvement! Restart with new stuck list
                        improved = true;
                        noProgressCount = 0;
                        break;
                    }

                    if (newStuck.Count == stuckIndices.Count)
                    {
                        // Same count but different arrows might be stuck — could cascade later
                        bool differentSet = false;
                        for (int n = 0; n < newStuck.Count; n++)
                        {
                            if (!stuckIndices.Contains(newStuck[n]))
                            {
                                differentSet = true;
                                break;
                            }
                        }

                        if (differentSet)
                        {
                            // Accept: different deadlock group may be easier to resolve
                            improved = true;
                            noProgressCount++;
                            break;
                        }
                    }

                    // No improvement — restore original state
                    RestoreArrowState(placements, stuckIdx, originalPath, originalDir);
                }

                if (!improved || noProgressCount >= maxNoProgress) break;
            }
        }

        /// <summary>
        /// Restores an arrow placement to its original state using a deep-copied path.
        /// Necessary because FlipArrowOrientation uses in-place List.Reverse() which
        /// corrupts the shared PathPoints reference in the struct copy.
        /// </summary>
        private static void RestoreArrowState(
            List<SolvabilityChecker.ArrowPlacement> placements,
            int index, List<Vector2Int> originalPath, ArrowDirection originalDir)
        {
            var restored = placements[index];
            restored.PathPoints.Clear();
            restored.PathPoints.AddRange(originalPath);
            restored.HeadDirection = originalDir;
            placements[index] = restored;
        }

        /// <summary>
        /// Checks if placing an outward arrow head at headPos would create an ugly parallel cluster
        /// of more than maxAllowedConsecutive (default 2) adjacent arrows firing outward on the same border.
        /// Prevents the "13 parallel arrows pointing right" issue.
        /// </summary>
        private static bool CreatesPerimeterCluster(
            Vector2Int headPos, ArrowDirection headDir,
            List<SolvabilityChecker.ArrowPlacement> placements,
            int width, int height,
            int ignoreIndex = -1, int maxAllowedConsecutive = 2)
        {
            if (placements == null) return false;

            bool isRightEdge = headPos.x == width - 1 && headDir == ArrowDirection.Right;
            bool isLeftEdge = headPos.x == 0 && headDir == ArrowDirection.Left;
            bool isTopEdge = headPos.y == height - 1 && headDir == ArrowDirection.Up;
            bool isBottomEdge = headPos.y == 0 && headDir == ArrowDirection.Down;

            if (!isRightEdge && !isLeftEdge && !isTopEdge && !isBottomEdge)
            {
                return false; // Not firing directly outward into a border
            }

            int consecutive = 1;

            if (isRightEdge || isLeftEdge)
            {
                int checkX = headPos.x;
                for (int y = headPos.y + 1; y < height; y++)
                {
                    if (HasOutwardHeadAt(checkX, y, headDir, placements, ignoreIndex))
                        consecutive++;
                    else
                        break;
                }
                for (int y = headPos.y - 1; y >= 0; y--)
                {
                    if (HasOutwardHeadAt(checkX, y, headDir, placements, ignoreIndex))
                        consecutive++;
                    else
                        break;
                }
            }
            else
            {
                int checkY = headPos.y;
                for (int x = headPos.x + 1; x < width; x++)
                {
                    if (HasOutwardHeadAt(x, checkY, headDir, placements, ignoreIndex))
                        consecutive++;
                    else
                        break;
                }
                for (int x = headPos.x - 1; x >= 0; x--)
                {
                    if (HasOutwardHeadAt(x, checkY, headDir, placements, ignoreIndex))
                        consecutive++;
                    else
                        break;
                }
            }

            return consecutive > maxAllowedConsecutive;
        }

        private static bool HasOutwardHeadAt(
            int x, int y, ArrowDirection dir,
            List<SolvabilityChecker.ArrowPlacement> placements,
            int ignoreIndex)
        {
            for (int i = 0; i < placements.Count; i++)
            {
                if (i == ignoreIndex) continue;
                if (placements[i].HeadPoint.x == x &&
                    placements[i].HeadPoint.y == y &&
                    placements[i].HeadDirection == dir)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool HasAnyPerimeterCluster(
            List<SolvabilityChecker.ArrowPlacement> placements,
            int gridWidth, int gridHeight)
        {
            if (placements == null) return false;
            for (int i = 0; i < placements.Count; i++)
            {
                if (CreatesPerimeterCluster(placements[i].HeadPoint, placements[i].HeadDirection, placements, gridWidth, gridHeight, i))
                {
                    return true;
                }
            }
            return false;
        }

        private static int CountCurrentBoundaryExits(
            List<SolvabilityChecker.ArrowPlacement> placements,
            int width, int height)
        {
            if (placements == null) return 0;
            int count = 0;
            for (int i = 0; i < placements.Count; i++)
            {
                Vector2Int h = placements[i].HeadPoint;
                ArrowDirection d = placements[i].HeadDirection;
                if ((h.x == width - 1 && d == ArrowDirection.Right) ||
                    (h.x == 0 && d == ArrowDirection.Left) ||
                    (h.y == height - 1 && d == ArrowDirection.Up) ||
                    (h.y == 0 && d == ArrowDirection.Down))
                {
                    count++;
                }
            }
            return count;
        }

        private static List<int> GetInitialUnblockedIndices(
            List<SolvabilityChecker.ArrowPlacement> placements,
            int gridWidth, int gridHeight)
        {
            var list = new List<int>();
            var occupied = new HashSet<Vector2Int>();
            for (int i = 0; i < placements.Count; i++)
            {
                var pts = placements[i].PathPoints;
                if (pts == null) continue;
                for (int j = 0; j < pts.Count; j++)
                {
                    occupied.Add(pts[j]);
                }
            }

            for (int i = 0; i < placements.Count; i++)
            {
                if (CanFireArrow(placements[i], gridWidth, gridHeight, occupied))
                {
                    list.Add(i);
                }
            }

            return list;
        }

        /// <summary>
        /// Post-solve puzzle deepener: Transforms a trivial level (where too many arrows are
        /// immediately fireable at start) into a deep, satisfying disentanglement puzzle.
        /// Iteratively flips excess unblocked arrows to point into valid blockers, ensuring:
        /// 1. InitialUnblockedCount is restricted to 2-3 (max 4 on huge maps).
        /// 2. Zero perimeter clusters (no >= 3 parallel arrows in a row on any edge).
        /// 3. Level remains 100% solvable and winnable throughout every step.
        /// </summary>
        private static void DeepenPuzzleDependencies(
            List<SolvabilityChecker.ArrowPlacement> placements,
            int gridWidth, int gridHeight,
            int totalMobHP, float winabilityRatio)
        {
            if (placements == null || placements.Count <= 3) return;

            int totalArrows = placements.Count;
            int targetMaxInitial = Mathf.Max(2, Mathf.RoundToInt(totalArrows * 0.18f));

            int maxDeepenPasses = 40;

            for (int pass = 0; pass < maxDeepenPasses; pass++)
            {
                var check = SolvabilityChecker.Check(placements, gridWidth, gridHeight, totalMobHP, winabilityRatio);
                if (!check.IsValid) break;

                bool hasCluster = HasAnyPerimeterCluster(placements, gridWidth, gridHeight);
                if (check.InitialUnblockedCount <= targetMaxInitial && !hasCluster)
                {
                    break;
                }

                var unblockedIndices = GetInitialUnblockedIndices(placements, gridWidth, gridHeight);
                if (unblockedIndices.Count <= targetMaxInitial && !hasCluster)
                {
                    break;
                }

                List<int> candidateIndices;
                if (hasCluster)
                {
                    candidateIndices = new List<int>();
                    for (int i = 0; i < placements.Count; i++)
                    {
                        if (CreatesPerimeterCluster(placements[i].HeadPoint, placements[i].HeadDirection, placements, gridWidth, gridHeight, i))
                        {
                            candidateIndices.Add(i);
                        }
                    }
                    for (int i = 0; i < unblockedIndices.Count; i++)
                    {
                        if (!candidateIndices.Contains(unblockedIndices[i]))
                        {
                            candidateIndices.Add(unblockedIndices[i]);
                        }
                    }
                }
                else
                {
                    candidateIndices = new List<int>(unblockedIndices);
                }

                ShuffleList(candidateIndices);
                bool anyFlipped = false;

                for (int c = 0; c < candidateIndices.Count; c++)
                {
                    int idx = candidateIndices[c];

                    var originalPath = new List<Vector2Int>(placements[idx].PathPoints);
                    ArrowDirection originalDir = placements[idx].HeadDirection;

                    var flipped = placements[idx];
                    FlipArrowOrientation(ref flipped);

                    if (IsSelfBlocking(flipped, gridWidth, gridHeight) ||
                        CreatesHeadToHeadConflict(flipped.HeadPoint, flipped.HeadDirection, placements, idx) ||
                        CreatesPerimeterCluster(flipped.HeadPoint, flipped.HeadDirection, placements, gridWidth, gridHeight, idx))
                    {
                        RestoreArrowState(placements, idx, originalPath, originalDir);
                        continue;
                    }

                    placements[idx] = flipped;

                    var newCheck = SolvabilityChecker.Check(placements, gridWidth, gridHeight, totalMobHP, winabilityRatio);

                    bool improvedInitial = newCheck.InitialUnblockedCount < check.InitialUnblockedCount;
                    bool improvedCluster = hasCluster && !HasAnyPerimeterCluster(placements, gridWidth, gridHeight);
                    bool deepenedSteps = newCheck.InitialUnblockedCount == check.InitialUnblockedCount && newCheck.FiringSteps > check.FiringSteps;

                    if (newCheck.IsValid && newCheck.InitialUnblockedCount >= 2 && (improvedInitial || improvedCluster || deepenedSteps))
                    {
                        anyFlipped = true;
                        break;
                    }

                    RestoreArrowState(placements, idx, originalPath, originalDir);
                }

                if (!anyFlipped) break;
            }
        }

        /// <summary>
        /// Selects a target arrow length (points = weight + 1) following the user's weight distribution:
        /// ~50% in the upper half [midWeight + 1 .. maxWeight], ~50% in the lower half [minWeight .. midWeight].
        /// Dynamically balances lower and upper counts, and strictly respects minWeight.
        /// </summary>
        private static int PickTargetArrowLength(
            int minWeight, int maxWeight,
            List<SolvabilityChecker.ArrowPlacement> existingPlacements)
        {
            if (minWeight >= maxWeight) return minWeight + 1;

            int midWeight = minWeight + (maxWeight - minWeight) / 2;

            int lowCount = 0;
            int highCount = 0;

            if (existingPlacements != null)
            {
                for (int i = 0; i < existingPlacements.Count; i++)
                {
                    int w = existingPlacements[i].Weight;
                    if (w <= midWeight) lowCount++;
                    else highCount++;
                }
            }

            int chosenWeight;

            // Target 50/50 balance between lower half [minWeight .. midWeight] and upper half [midWeight + 1 .. maxWeight]
            if (highCount < lowCount || (highCount == lowCount && Random.value < TargetUpperWeightRatio))
            {
                int lowBound = Mathf.Min(midWeight + 1, maxWeight);
                chosenWeight = Random.Range(lowBound, maxWeight + 1);
            }
            else
            {
                chosenWeight = Random.Range(minWeight, midWeight + 1);
            }

            return chosenWeight + 1;
        }

        /// <summary>
        /// Consolidates excess short arrows (especially weight 1) into adjacent arrows
        /// to ensure a balanced weight distribution (targeting ~5-8% max weight 1 arrows).
        /// Uses 3 strategies: (1) Tail append, (2) U-Detour segment insertion, (3) Pair merge of two W1 arrows into a W3 arrow.
        /// Strictly preserves all 8 invariants.
        /// </summary>
        private static void MergeExcessShortArrows(
            List<SolvabilityChecker.ArrowPlacement> placements,
            int maxWeight, int gridWidth, int gridHeight)
        {
            if (placements == null || placements.Count <= 4) return;

            int maxLength = maxWeight + 2;
            int targetMaxWeight1 = Mathf.Max(1, Mathf.RoundToInt(placements.Count * 0.05f));

            bool mergedAny = true;
            int maxPasses = 25;

            while (mergedAny && maxPasses-- > 0)
            {
                mergedAny = false;

                int weight1Count = 0;
                for (int i = 0; i < placements.Count; i++)
                {
                    if (placements[i].Weight == 1) weight1Count++;
                }

                if (weight1Count <= targetMaxWeight1) break;

                for (int i = placements.Count - 1; i >= 0; i--)
                {
                    if (placements[i].Weight != 1) continue;

                    var shortPlacement = placements[i];
                    var shortPath = shortPlacement.PathPoints;
                    if (shortPath == null || shortPath.Count != 2) continue;

                    Vector2Int ptA = shortPath[0];
                    Vector2Int ptB = shortPath[1];

                    bool mergedThis = false;

                    // Strategy 1: Tail Merge (append to adjacent arrow's tail)
                    for (int j = 0; j < placements.Count; j++)
                    {
                        if (i == j) continue;

                        var targetPlacement = placements[j];
                        var targetPath = targetPlacement.PathPoints;
                        if (targetPath == null || targetPath.Count == 0) continue;

                        if (targetPath.Count + 2 > maxLength) continue;

                        Vector2Int tail = targetPath[targetPath.Count - 1];

                        // Case 1A: tail connects to ptA
                        if (IsManhattanOne(tail, ptA))
                        {
                            if (!IsInFireRay(ptA, targetPlacement.HeadPoint, targetPlacement.HeadDirection, gridWidth, gridHeight) &&
                                !IsInFireRay(ptB, targetPlacement.HeadPoint, targetPlacement.HeadDirection, gridWidth, gridHeight))
                            {
                                var testPath = new List<Vector2Int>(targetPath) { ptA, ptB };
                                var testPlace = new SolvabilityChecker.ArrowPlacement(testPath, targetPlacement.HeadDirection);

                                if (!IsSelfBlocking(testPlace, gridWidth, gridHeight))
                                {
                                    targetPath.Add(ptA);
                                    targetPath.Add(ptB);
                                    placements.RemoveAt(i);
                                    mergedThis = true;
                                    mergedAny = true;
                                    break;
                                }
                            }
                        }
                        // Case 1B: tail connects to ptB
                        else if (IsManhattanOne(tail, ptB))
                        {
                            if (!IsInFireRay(ptA, targetPlacement.HeadPoint, targetPlacement.HeadDirection, gridWidth, gridHeight) &&
                                !IsInFireRay(ptB, targetPlacement.HeadPoint, targetPlacement.HeadDirection, gridWidth, gridHeight))
                            {
                                var testPath = new List<Vector2Int>(targetPath) { ptB, ptA };
                                var testPlace = new SolvabilityChecker.ArrowPlacement(testPath, targetPlacement.HeadDirection);

                                if (!IsSelfBlocking(testPlace, gridWidth, gridHeight))
                                {
                                    targetPath.Add(ptB);
                                    targetPath.Add(ptA);
                                    placements.RemoveAt(i);
                                    mergedThis = true;
                                    mergedAny = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (mergedThis)
                    {
                        weight1Count--;
                        if (weight1Count <= targetMaxWeight1) break;
                        continue;
                    }

                    // Strategy 2: U-Detour Merge into an adjacent segment of an existing arrow
                    for (int j = 0; j < placements.Count; j++)
                    {
                        if (i == j) continue;

                        var targetPlacement = placements[j];
                        var targetPath = targetPlacement.PathPoints;
                        if (targetPath == null || targetPath.Count < 2) continue;
                        if (targetPath.Count + 2 > maxLength) continue;

                        for (int k = 0; k < targetPath.Count - 1; k++)
                        {
                            if (IsManhattanOne(targetPath[k], ptA) && IsManhattanOne(ptB, targetPath[k + 1]))
                            {
                                if (!IsInFireRay(ptA, targetPlacement.HeadPoint, targetPlacement.HeadDirection, gridWidth, gridHeight) &&
                                    !IsInFireRay(ptB, targetPlacement.HeadPoint, targetPlacement.HeadDirection, gridWidth, gridHeight))
                                {
                                    var testPath = new List<Vector2Int>(targetPath);
                                    testPath.Insert(k + 1, ptB);
                                    testPath.Insert(k + 1, ptA);
                                    var testPlace = new SolvabilityChecker.ArrowPlacement(testPath, targetPlacement.HeadDirection);

                                    if (!IsSelfBlocking(testPlace, gridWidth, gridHeight))
                                    {
                                        targetPath.Insert(k + 1, ptB);
                                        targetPath.Insert(k + 1, ptA);
                                        placements.RemoveAt(i);
                                        mergedThis = true;
                                        mergedAny = true;
                                        break;
                                    }
                                }
                            }
                            else if (IsManhattanOne(targetPath[k], ptB) && IsManhattanOne(ptA, targetPath[k + 1]))
                            {
                                if (!IsInFireRay(ptA, targetPlacement.HeadPoint, targetPlacement.HeadDirection, gridWidth, gridHeight) &&
                                    !IsInFireRay(ptB, targetPlacement.HeadPoint, targetPlacement.HeadDirection, gridWidth, gridHeight))
                                {
                                    var testPath = new List<Vector2Int>(targetPath);
                                    testPath.Insert(k + 1, ptA);
                                    testPath.Insert(k + 1, ptB);
                                    var testPlace = new SolvabilityChecker.ArrowPlacement(testPath, targetPlacement.HeadDirection);

                                    if (!IsSelfBlocking(testPlace, gridWidth, gridHeight))
                                    {
                                        targetPath.Insert(k + 1, ptA);
                                        targetPath.Insert(k + 1, ptB);
                                        placements.RemoveAt(i);
                                        mergedThis = true;
                                        mergedAny = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (mergedThis) break;
                    }

                    if (mergedThis)
                    {
                        weight1Count--;
                        if (weight1Count <= targetMaxWeight1) break;
                        continue;
                    }

                    // Strategy 3: Pair Merge with another adjacent W1 arrow into a single W3 arrow
                    for (int j = 0; j < placements.Count; j++)
                    {
                        if (i == j || placements[j].Weight != 1) continue;
                        var pathB = placements[j].PathPoints;
                        if (pathB == null || pathB.Count != 2) continue;

                        List<Vector2Int> combined = null;
                        ArrowDirection newDir = ArrowDirection.Up;

                        if (IsManhattanOne(ptB, pathB[0]))
                        {
                            combined = new List<Vector2Int> { ptA, ptB, pathB[0], pathB[1] };
                            newDir = VectorToDirection(ptA - ptB);
                        }
                        else if (IsManhattanOne(ptB, pathB[1]))
                        {
                            combined = new List<Vector2Int> { ptA, ptB, pathB[1], pathB[0] };
                            newDir = VectorToDirection(ptA - ptB);
                        }
                        else if (IsManhattanOne(pathB[1], ptA))
                        {
                            combined = new List<Vector2Int> { pathB[0], pathB[1], ptA, ptB };
                            newDir = VectorToDirection(pathB[0] - pathB[1]);
                        }
                        else if (IsManhattanOne(pathB[1], ptB))
                        {
                            combined = new List<Vector2Int> { pathB[0], pathB[1], ptB, ptA };
                            newDir = VectorToDirection(pathB[0] - pathB[1]);
                        }

                        if (combined != null)
                        {
                            var testPlace = new SolvabilityChecker.ArrowPlacement(combined, newDir);
                            if (!IsSelfBlocking(testPlace, gridWidth, gridHeight) &&
                                !CreatesHeadToHeadConflict(testPlace.HeadPoint, newDir, placements, j))
                            {
                                placements[j] = testPlace;
                                placements.RemoveAt(i);
                                mergedThis = true;
                                mergedAny = true;
                                break;
                            }
                        }
                    }

                    if (mergedThis)
                    {
                        weight1Count -= 2; // Pair merge eliminates 2 weight 1 arrows!
                        if (weight1Count <= targetMaxWeight1) break;
                    }
                }
            }
        }

        private static int CountFreeSpaceNeighbors(
            Vector2Int pt, int width, int height, bool[,] occupied, HashSet<Vector2Int> fireRay, List<Vector2Int> path)
        {
            int count = 0;
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            for (int i = 0; i < dirs.Length; i++)
            {
                Vector2Int n = pt + dirs[i];
                if (n.IsInBounds(width, height) && !occupied[n.x, n.y] && !fireRay.Contains(n) && !path.Contains(n))
                {
                    count++;
                }
            }
            return count;
        }

        private static Vector2Int PickBestOpenDirection(
            List<Vector2Int> candidates, Vector2Int current,
            int width, int height, bool[,] occupied, HashSet<Vector2Int> fireRay, List<Vector2Int> path)
        {
            if (candidates.Count == 1) return candidates[0];

            int bestScore = -1;
            var bestList = new List<Vector2Int>();

            for (int i = 0; i < candidates.Count; i++)
            {
                Vector2Int d = candidates[i];
                int score = CountFreeSpaceNeighbors(current + d, width, height, occupied, fireRay, path);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestList.Clear();
                    bestList.Add(d);
                }
                else if (score == bestScore)
                {
                    bestList.Add(d);
                }
            }

            return bestList[Random.Range(0, bestList.Count)];
        }

        private struct CandidateHead
        {
            public Vector2Int pos;
            public ArrowDirection dir;
        }

        private static List<CandidateHead> GetFreeBoundaryCandidates(
            int width, int height, bool[,] occupied)
        {
            var list = new List<CandidateHead>();

            for (int x = 0; x < width; x++)
            {
                // Head at top, facing Up: neck must be below (x, height - 2)
                if (!occupied[x, height - 1] && (height < 2 || !occupied[x, height - 2]))
                    list.Add(new CandidateHead { pos = new Vector2Int(x, height - 1), dir = ArrowDirection.Up });
            }

            for (int x = 0; x < width; x++)
            {
                // Head at bottom, facing Down: neck must be above (x, 1)
                if (!occupied[x, 0] && (height < 2 || !occupied[x, 1]))
                    list.Add(new CandidateHead { pos = new Vector2Int(x, 0), dir = ArrowDirection.Down });
            }

            for (int y = 0; y < height; y++)
            {
                // Head at left, facing Left: neck must be right (1, y)
                if (!occupied[0, y] && (width < 2 || !occupied[1, y]))
                    list.Add(new CandidateHead { pos = new Vector2Int(0, y), dir = ArrowDirection.Left });
            }

            for (int y = 0; y < height; y++)
            {
                // Head at right, facing Right: neck must be left (width - 2, y)
                if (!occupied[width - 1, y] && (width < 2 || !occupied[width - 2, y]))
                    list.Add(new CandidateHead { pos = new Vector2Int(width - 1, y), dir = ArrowDirection.Right });
            }

            return list;
        }

        private static List<CandidateHead> GetInteriorCandidates(
            int width, int height, bool[,] occupied)
        {
            var preferredList = new List<CandidateHead>();
            var allList = new List<CandidateHead>();
            ArrowDirection[] dirs = { ArrowDirection.Up, ArrowDirection.Down, ArrowDirection.Left, ArrowDirection.Right };

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (occupied[x, y]) continue;

                    Vector2Int pos = new Vector2Int(x, y);

                    foreach (var dir in dirs)
                    {
                        Vector2Int step = ArrowSwarm.Grid.GridManager.DirectionToVector(dir);
                        Vector2Int neck = pos - step;

                        // Neck MUST be in bounds and unoccupied for growth to start
                        if (neck.IsInBounds(width, height) && !occupied[neck.x, neck.y])
                        {
                            var cand = new CandidateHead { pos = pos, dir = dir };
                            allList.Add(cand);

                            // Prioritize candidate heads whose neck can make at least 1 further step (preventing dead-end 2-cell pockets)
                            bool hasOpenExit = false;
                            foreach (var d in dirs)
                            {
                                Vector2Int n = neck + ArrowSwarm.Grid.GridManager.DirectionToVector(d);
                                if (n != pos && n.IsInBounds(width, height) && !occupied[n.x, n.y])
                                {
                                    hasOpenExit = true;
                                    break;
                                }
                            }

                            if (hasOpenExit)
                            {
                                preferredList.Add(cand);
                            }
                        }
                    }
                }
            }

            return preferredList.Count > 0 ? preferredList : allList;
        }

        private static int CountRayCollisions(
            Vector2Int pos, ArrowDirection dir, int width, int height, bool[,] occupied)
        {
            Vector2Int step = ArrowSwarm.Grid.GridManager.DirectionToVector(dir);
            Vector2Int curr = pos + step;
            int count = 0;
            while (curr.IsInBounds(width, height))
            {
                if (occupied[curr.x, curr.y]) count++;
                curr += step;
            }
            return count;
        }

        private static List<Vector2Int> GetUnoccupiedCells(int width, int height, bool[,] occupied)
        {
            var list = new List<Vector2Int>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!occupied[x, y]) list.Add(new Vector2Int(x, y));
                }
            }
            return list;
        }

        private static ArrowDirection GetBestOutwardDir(
            Vector2Int pos, int width, int height, bool[,] occupied)
        {
            ArrowDirection[] dirs = { ArrowDirection.Up, ArrowDirection.Down, ArrowDirection.Left, ArrowDirection.Right };
            foreach (var dir in dirs)
            {
                Vector2Int step = ArrowSwarm.Grid.GridManager.DirectionToVector(dir);
                Vector2Int neck = pos - step;
                if (neck.IsInBounds(width, height) && !occupied[neck.x, neck.y])
                {
                    Vector2Int target = pos + step;
                    if (!target.IsInBounds(width, height)) return dir;
                }
            }
            // Fallback
            foreach (var dir in dirs)
            {
                Vector2Int step = ArrowSwarm.Grid.GridManager.DirectionToVector(dir);
                Vector2Int neck = pos - step;
                if (neck.IsInBounds(width, height) && !occupied[neck.x, neck.y]) return dir;
            }
            return ArrowDirection.Up;
        }

        private static List<Vector2Int> GrowArrowPathBackwards(
            Vector2Int headPos, ArrowDirection headDir, int targetLength,
            int width, int height, bool[,] occupied)
        {
            Vector2Int headStep = ArrowSwarm.Grid.GridManager.DirectionToVector(headDir);
            Vector2Int secondPos = headPos - headStep;

            // By the Neck Axiom, secondPos MUST be in bounds and unoccupied
            if (!secondPos.IsInBounds(width, height) || occupied[secondPos.x, secondPos.y])
            {
                return null; // Cannot form a natural neck pointing in headDir
            }

            // Calculate line of fire (laser) for the head: points that this arrow CANNOT occupy under any circumstances
            var fireRay = new HashSet<Vector2Int>();
            Vector2Int rayPt = headPos + headStep;
            while (rayPt.IsInBounds(width, height))
            {
                fireRay.Add(rayPt);
                rayPt += headStep;
            }

            List<Vector2Int> bestPath = null;
            int attempts = targetLength >= 5 ? 3 : 1;

            for (int attempt = 0; attempt < attempts; attempt++)
            {
                List<Vector2Int> path = new List<Vector2Int> { headPos, secondPos };
                Vector2Int current = secondPos;
                Vector2Int currentDir = -headStep;
                int straightSteps = 0;

                while (path.Count < targetLength)
                {
                    List<Vector2Int> validDirs = new List<Vector2Int>();
                    Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

                    foreach (var d in dirs)
                    {
                        Vector2Int neighbor = current + d;
                        // CRITICAL: Cannot be occupied, cannot be in current path, AND CANNOT BE IN HEAD'S LASER FIRE RAY!
                        if (neighbor.IsInBounds(width, height) && 
                            !occupied[neighbor.x, neighbor.y] && 
                            !path.Contains(neighbor) &&
                            !fireRay.Contains(neighbor))
                        {
                            validDirs.Add(d);
                        }
                    }

                    if (validDirs.Count == 0) break;

                    List<Vector2Int> turnDirs = new List<Vector2Int>(validDirs);
                    if (currentDir != Vector2Int.zero)
                    {
                        turnDirs.Remove(currentDir);
                        turnDirs.Remove(-currentDir);
                    }

                    Vector2Int chosenDir;
                    int maxStraight = targetLength >= 8 ? 5 : 3;
                    float straightChance = targetLength >= 8 ? 0.65f : 0.40f;

                    if (straightSteps >= maxStraight && turnDirs.Count > 0)
                    {
                        chosenDir = PickBestOpenDirection(turnDirs, current, width, height, occupied, fireRay, path);
                        straightSteps = 0;
                    }
                    else if (currentDir != Vector2Int.zero && validDirs.Contains(currentDir) && Random.value < straightChance)
                    {
                        chosenDir = currentDir;
                        straightSteps++;
                    }
                    else
                    {
                        chosenDir = PickBestOpenDirection(validDirs, current, width, height, occupied, fireRay, path);
                        straightSteps = 0;
                    }

                    current += chosenDir;
                    currentDir = chosenDir;
                    path.Add(current);
                }

                if (bestPath == null || path.Count > bestPath.Count)
                {
                    bestPath = path;
                }

                if (bestPath.Count >= targetLength)
                {
                    break;
                }
            }

            return bestPath;
        }

        private static bool AttachIsolatedCellToPlacement(
            Vector2Int isolated, List<SolvabilityChecker.ArrowPlacement> placements, int[,] cellOwner, int width, int height, bool allowNewArrows = false)
        {
            if (placements == null || placements.Count == 0) return false;

            // 1. Try attaching isolated to an existing placement's TAIL (strictly never into head's laser ray)
            for (int pIndex = 0; pIndex < placements.Count; pIndex++)
            {
                var p = placements[pIndex];
                var existingPath = p.PathPoints;
                Vector2Int tail = existingPath[existingPath.Count - 1];

                if (IsManhattanOne(isolated, tail))
                {
                    if (!IsInFireRay(isolated, existingPath[0], p.HeadDirection, width, height))
                    {
                        existingPath.Add(isolated);
                        cellOwner[isolated.x, isolated.y] = pIndex;
                        return true;
                    }
                }
            }

            // 2. Try inserting isolated as a corner detour inside an existing placement's path
            for (int pIndex = 0; pIndex < placements.Count; pIndex++)
            {
                var p = placements[pIndex];
                var existingPath = p.PathPoints;
                for (int i = 0; i < existingPath.Count - 1; i++)
                {
                    if (IsManhattanOne(isolated, existingPath[i]) && IsManhattanOne(isolated, existingPath[i + 1]))
                    {
                        if (!IsInFireRay(isolated, existingPath[0], p.HeadDirection, width, height))
                        {
                            existingPath.Insert(i + 1, isolated);
                            cellOwner[isolated.x, isolated.y] = pIndex;
                            return true;
                        }
                    }
                }
            }

            // 3. Try Head extension: isolated becomes the new head of an adjacent arrow
            for (int pIndex = 0; pIndex < placements.Count; pIndex++)
            {
                var p = placements[pIndex];
                var existingPath = p.PathPoints;
                Vector2Int head = existingPath[0];

                if (IsManhattanOne(isolated, head))
                {
                    ArrowDirection newDir = VectorToDirection(isolated - head);
                    if (!CreatesHeadToHeadConflict(isolated, newDir, placements, pIndex))
                    {
                        var testPath = new List<Vector2Int> { isolated };
                        testPath.AddRange(existingPath);
                        var testPlacement = new SolvabilityChecker.ArrowPlacement(testPath, newDir);
                        if (!IsSelfBlocking(testPlacement, width, height))
                        {
                            existingPath.Insert(0, isolated);
                            p.HeadDirection = newDir;
                            placements[pIndex] = p;
                            cellOwner[isolated.x, isolated.y] = pIndex;
                            return true;
                        }
                    }
                }
            }

            // 4. Try forming a 2-point arrow with an UNOWNED adjacent neighbor (last resort, only if allowed)
            if (allowNewArrows)
            {
                Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                foreach (var d in dirs)
                {
                    Vector2Int neighbor = isolated + d;
                    if (neighbor.IsInBounds(width, height) && cellOwner[neighbor.x, neighbor.y] == -1)
                    {
                        ArrowDirection dir = VectorToDirection(isolated - neighbor);
                        if (!CreatesHeadToHeadConflict(isolated, dir, placements))
                        {
                            int arrowIdx = placements.Count;
                            var smallPath = new List<Vector2Int> { isolated, neighbor };
                            placements.Add(new SolvabilityChecker.ArrowPlacement(smallPath, dir));

                            cellOwner[isolated.x, isolated.y] = arrowIdx;
                            cellOwner[neighbor.x, neighbor.y] = arrowIdx;
                            return true;
                        }

                        ArrowDirection oppDir = VectorToDirection(neighbor - isolated);
                        if (!CreatesHeadToHeadConflict(neighbor, oppDir, placements))
                        {
                            int arrowIdx = placements.Count;
                            var smallPath = new List<Vector2Int> { neighbor, isolated };
                            placements.Add(new SolvabilityChecker.ArrowPlacement(smallPath, oppDir));

                            cellOwner[isolated.x, isolated.y] = arrowIdx;
                            cellOwner[neighbor.x, neighbor.y] = arrowIdx;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Sweeps all coordinates in the grid and absorbs 100% of any unowned cells (cellOwner == -1)
        /// using multi-tiered absorption (tail extension, corner detour, 2-point creation, head extension,
        /// and arrow splitting). Guarantees ZERO empty dots remain on the board!
        /// </summary>
        private static void FillAllUnownedCells(
            List<SolvabilityChecker.ArrowPlacement> placements, int width, int height)
        {
            if (placements == null || placements.Count == 0) return;

            int[,] cellOwner = new int[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    cellOwner[x, y] = -1;
                }
            }

            for (int p = 0; p < placements.Count; p++)
            {
                var path = placements[p].PathPoints;
                if (path == null) continue;
                for (int i = 0; i < path.Count; i++)
                {
                    cellOwner[path[i].x, path[i].y] = p;
                }
            }

            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            bool grewAny = true;
            int maxPasses = 50;

            while (grewAny && maxPasses-- > 0)
            {
                grewAny = false;

                // Pass 1: Extend arrow tails into any adjacent unowned cell
                for (int p = 0; p < placements.Count; p++)
                {
                    var path = placements[p].PathPoints;
                    if (path == null || path.Count == 0) continue;

                    Vector2Int tail = path[path.Count - 1];
                    foreach (var d in dirs)
                    {
                        Vector2Int neighbor = tail + d;
                        if (neighbor.IsInBounds(width, height) && cellOwner[neighbor.x, neighbor.y] == -1)
                        {
                            // Ensure neighbor is not in head's line of fire
                            if (!IsInFireRay(neighbor, path[0], placements[p].HeadDirection, width, height))
                            {
                                path.Add(neighbor);
                                cellOwner[neighbor.x, neighbor.y] = p;
                                tail = neighbor;
                                grewAny = true;
                            }
                        }
                    }
                }

                // Pass 2: Insert empty cell as an orthogonal 90-degree corner detour into any adjacent body segment
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (cellOwner[x, y] != -1) continue;

                        Vector2Int emptyPt = new Vector2Int(x, y);
                        bool filled = false;

                        for (int p = 0; p < placements.Count; p++)
                        {
                            var path = placements[p].PathPoints;
                            if (path == null || path.Count < 2) continue;

                            for (int i = 0; i < path.Count - 1; i++)
                            {
                                if (IsManhattanOne(emptyPt, path[i]) && IsManhattanOne(emptyPt, path[i + 1]))
                                {
                                    if (!IsInFireRay(emptyPt, path[0], placements[p].HeadDirection, width, height))
                                    {
                                        path.Insert(i + 1, emptyPt);
                                        cellOwner[x, y] = p;
                                        filled = true;
                                        grewAny = true;
                                        break;
                                    }
                                }
                            }
                            if (filled) break;
                        }
                    }
                }

                // Pass 2.5: Insert adjacent unowned cell pairs as a 2-point U-detour into an existing arrow segment
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (cellOwner[x, y] != -1) continue;
                        Vector2Int ptA = new Vector2Int(x, y);

                        foreach (var d in dirs)
                        {
                            Vector2Int ptB = ptA + d;
                            if (!ptB.IsInBounds(width, height) || cellOwner[ptB.x, ptB.y] != -1) continue;

                            bool absorbedPair = false;

                            // Check only arrows owning adjacent cells to ptA
                            foreach (var dOwner in dirs)
                            {
                                Vector2Int nOwner = ptA + dOwner;
                                if (!nOwner.IsInBounds(width, height) || cellOwner[nOwner.x, nOwner.y] == -1) continue;
                                int p = cellOwner[nOwner.x, nOwner.y];

                                var path = placements[p].PathPoints;
                                if (path == null || path.Count < 2) continue;

                                for (int i = 0; i < path.Count - 1; i++)
                                {
                                    if (IsManhattanOne(ptA, path[i]) && IsManhattanOne(ptB, path[i + 1]))
                                    {
                                        if (!IsInFireRay(ptA, path[0], placements[p].HeadDirection, width, height) &&
                                            !IsInFireRay(ptB, path[0], placements[p].HeadDirection, width, height))
                                        {
                                            path.Insert(i + 1, ptB);
                                            path.Insert(i + 1, ptA);
                                            cellOwner[ptA.x, ptA.y] = p;
                                            cellOwner[ptB.x, ptB.y] = p;
                                            absorbedPair = true;
                                            grewAny = true;
                                            break;
                                        }
                                    }
                                    else if (IsManhattanOne(ptB, path[i]) && IsManhattanOne(ptA, path[i + 1]))
                                    {
                                        if (!IsInFireRay(ptA, path[0], placements[p].HeadDirection, width, height) &&
                                            !IsInFireRay(ptB, path[0], placements[p].HeadDirection, width, height))
                                        {
                                            path.Insert(i + 1, ptA);
                                            path.Insert(i + 1, ptB);
                                            cellOwner[ptA.x, ptA.y] = p;
                                            cellOwner[ptB.x, ptB.y] = p;
                                            absorbedPair = true;
                                            grewAny = true;
                                            break;
                                        }
                                    }
                                }

                                if (absorbedPair) break;
                            }

                            if (absorbedPair) break;
                        }
                    }
                }

                // Pass 3: Head extension into adjacent unowned cell
                for (int p = 0; p < placements.Count; p++)
                {
                    var pPlacement = placements[p];
                    var path = pPlacement.PathPoints;
                    if (path == null || path.Count == 0) continue;

                    Vector2Int head = path[0];
                    foreach (var d in dirs)
                    {
                        Vector2Int neighbor = head + d;
                        if (neighbor.IsInBounds(width, height) && cellOwner[neighbor.x, neighbor.y] == -1)
                        {
                            ArrowDirection newDir = VectorToDirection(neighbor - head);
                            if (!CreatesHeadToHeadConflict(neighbor, newDir, placements, p))
                            {
                                var testPath = new List<Vector2Int> { neighbor };
                                testPath.AddRange(path);
                                var testPlacement = new SolvabilityChecker.ArrowPlacement(testPath, newDir);
                                if (!IsSelfBlocking(testPlacement, width, height))
                                {
                                    path.Insert(0, neighbor);
                                    pPlacement.HeadDirection = newDir;
                                    placements[p] = pPlacement;
                                    cellOwner[neighbor.x, neighbor.y] = p;
                                    grewAny = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // Pass 4: Form 2-point arrows for any adjacent pair of unowned cells (last resort fallback only when all absorption exhausted)
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (cellOwner[x, y] != -1) continue;
                    Vector2Int ptA = new Vector2Int(x, y);

                    foreach (var d in dirs)
                    {
                        Vector2Int ptB = ptA + d;
                        if (ptB.IsInBounds(width, height) && cellOwner[ptB.x, ptB.y] == -1)
                        {
                            ArrowDirection dirA = VectorToDirection(ptA - ptB);
                            if (!CreatesHeadToHeadConflict(ptA, dirA, placements))
                            {
                                int newIdx = placements.Count;
                                placements.Add(new SolvabilityChecker.ArrowPlacement(new List<Vector2Int> { ptA, ptB }, dirA));
                                cellOwner[ptA.x, ptA.y] = newIdx;
                                cellOwner[ptB.x, ptB.y] = newIdx;
                                break;
                            }

                            ArrowDirection dirB = VectorToDirection(ptB - ptA);
                            if (!CreatesHeadToHeadConflict(ptB, dirB, placements))
                            {
                                int newIdx = placements.Count;
                                placements.Add(new SolvabilityChecker.ArrowPlacement(new List<Vector2Int> { ptB, ptA }, dirB));
                                cellOwner[ptA.x, ptA.y] = newIdx;
                                cellOwner[ptB.x, ptB.y] = newIdx;
                                break;
                            }
                        }
                    }
                }
            }

            // FINAL PASS 5: Absolute guarantee sweep — Any remaining isolated unowned cells
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (cellOwner[x, y] != -1) continue;

                    Vector2Int isolated = new Vector2Int(x, y);
                    bool absorbed = false;

                    // Strategy A: Split an adjacent arrow of length >= 4 to absorb isolated
                    for (int p = 0; p < placements.Count; p++)
                    {
                        var existingPath = placements[p].PathPoints;
                        if (existingPath == null || existingPath.Count < 4) continue;

                        for (int k = 2; k < existingPath.Count - 1; k++)
                        {
                            if (IsManhattanOne(isolated, existingPath[k]))
                            {
                                var path1 = existingPath.GetRange(0, k);
                                var path2 = new List<Vector2Int> { isolated };
                                path2.AddRange(existingPath.GetRange(k, existingPath.Count - k));

                                ArrowDirection dir1 = placements[p].HeadDirection;
                                ArrowDirection dir2 = VectorToDirection(isolated - existingPath[k]);

                                var place1 = new SolvabilityChecker.ArrowPlacement(path1, dir1);
                                var place2 = new SolvabilityChecker.ArrowPlacement(path2, dir2);

                                if (!IsSelfBlocking(place1, width, height) && !IsSelfBlocking(place2, width, height))
                                {
                                    placements[p] = place1;
                                    int newIdx = placements.Count;
                                    placements.Add(place2);

                                    for (int i = 0; i < path1.Count; i++) cellOwner[path1[i].x, path1[i].y] = p;
                                    for (int i = 0; i < path2.Count; i++) cellOwner[path2[i].x, path2[i].y] = newIdx;
                                    cellOwner[isolated.x, isolated.y] = newIdx;

                                    absorbed = true;
                                    break;
                                }
                            }
                        }
                        if (absorbed) break;
                    }

                    if (absorbed) continue;

                    // Strategy B: Force attach to any adjacent arrow tail or head
                    foreach (var d in dirs)
                    {
                        Vector2Int neighbor = isolated + d;
                        if (neighbor.IsInBounds(width, height) && cellOwner[neighbor.x, neighbor.y] != -1)
                        {
                            int pIndex = cellOwner[neighbor.x, neighbor.y];
                            var path = placements[pIndex].PathPoints;
                            if (path != null && path.Count > 0)
                            {
                                if (neighbor == path[path.Count - 1])
                                {
                                    path.Add(isolated);
                                    cellOwner[isolated.x, isolated.y] = pIndex;
                                    absorbed = true;
                                    break;
                                }
                                else if (neighbor == path[0])
                                {
                                    path.Insert(0, isolated);
                                    placements[pIndex] = new SolvabilityChecker.ArrowPlacement(path, VectorToDirection(isolated - neighbor));
                                    cellOwner[isolated.x, isolated.y] = pIndex;
                                    absorbed = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (absorbed) continue;

                    // Strategy C: Pair with adjacent cell from an arrow of length >= 3
                    foreach (var d in dirs)
                    {
                        Vector2Int neighbor = isolated + d;
                        if (neighbor.IsInBounds(width, height) && cellOwner[neighbor.x, neighbor.y] != -1)
                        {
                            int pIndex = cellOwner[neighbor.x, neighbor.y];
                            var path = placements[pIndex].PathPoints;
                            if (path != null && path.Count >= 3)
                            {
                                if (neighbor == path[path.Count - 1])
                                {
                                    path.RemoveAt(path.Count - 1);
                                    int newIdx = placements.Count;
                                    var newPath = new List<Vector2Int> { isolated, neighbor };
                                    ArrowDirection newDir = VectorToDirection(isolated - neighbor);
                                    placements.Add(new SolvabilityChecker.ArrowPlacement(newPath, newDir));
                                    cellOwner[isolated.x, isolated.y] = newIdx;
                                    cellOwner[neighbor.x, neighbor.y] = newIdx;
                                    absorbed = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validates that every consecutive pair of points in every arrow path is strictly
        /// Manhattan distance 1 (100% horizontal or vertical).
        /// If any diagonal segment is found, removes the diagonal point to guarantee ZERO diagonal lines!
        /// </summary>
        private static void FixDiagonalSegments(List<SolvabilityChecker.ArrowPlacement> placements)
        {
            if (placements == null) return;

            for (int p = 0; p < placements.Count; p++)
            {
                var placement = placements[p];
                var path = placement.PathPoints;
                if (path == null || path.Count < 2) continue;

                for (int i = path.Count - 2; i >= 0; i--)
                {
                    Vector2Int p1 = path[i];
                    Vector2Int p2 = path[i + 1];

                    int dx = Mathf.Abs(p1.x - p2.x);
                    int dy = Mathf.Abs(p1.y - p2.y);

                    if (dx + dy != 1)
                    {
                        // Illegal non-orthogonal step! Remove p2 to prevent diagonal zigzag!
                        path.RemoveAt(i + 1);
                    }
                }

                if (path.Count >= 2)
                {
                    placement.HeadDirection = VectorToDirection(path[0] - path[1]);
                    placements[p] = placement;
                }
            }
        }

        private static bool IsManhattanOne(Vector2Int a, Vector2Int b)
        {
            return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y)) == 1;
        }

        private static List<SolvabilityChecker.ArrowPlacement> GenerateMergedBSPPlacements(
            LevelParams levelParams, int gridWidth, int gridHeight)
        {
            var placements = new List<SolvabilityChecker.ArrowPlacement>();
            var pieces = new List<RectInt>();
            
            PartitionGrid(0, 0, gridWidth, gridHeight, levelParams.MaxWeight + 1, pieces);

            var paths = new List<List<Vector2Int>>();
            foreach (var rect in pieces)
            {
                var path = new List<Vector2Int>();
                if (rect.width > 1)
                {
                    for (int i = 0; i < rect.width; i++) path.Add(new Vector2Int(rect.x + i, rect.y));
                }
                else
                {
                    for (int i = 0; i < rect.height; i++) path.Add(new Vector2Int(rect.x, rect.y + i));
                }
                if (path.Count >= 2) paths.Add(path);
            }

            for (int i = 0; i < paths.Count; i++)
            {
                if (paths[i] == null) continue;
                for (int j = i + 1; j < paths.Count; j++)
                {
                    if (paths[j] == null) continue;

                    if (paths[i].Count + paths[j].Count <= levelParams.MaxWeight + 1)
                    {
                        Vector2Int tailI = paths[i][paths[i].Count - 1];
                        Vector2Int headJ = paths[j][0];
                        Vector2Int tailJ = paths[j][paths[j].Count - 1];
                        Vector2Int headI = paths[i][0];

                        if (Vector2Int.Distance(tailI, headJ) == 1f)
                        {
                            paths[i].AddRange(paths[j]);
                            paths[j] = null;
                            break;
                        }
                        else if (Vector2Int.Distance(headI, tailJ) == 1f)
                        {
                            paths[j].AddRange(paths[i]);
                            paths[i] = paths[j];
                            paths[j] = null;
                            break;
                        }
                    }
                }
            }

            foreach (var path in paths)
            {
                if (path == null || path.Count < 2) continue;

                if (Random.value > 0.5f) path.Reverse();

                Vector2Int bodyDir = path[1] - path[0];
                ArrowDirection headDir = OppositeDirection(VectorToDirection(bodyDir));

                placements.Add(new SolvabilityChecker.ArrowPlacement(path, headDir));
            }

            return placements;
        }

        private static void PartitionGrid(int x, int y, int w, int h, int maxLength, List<RectInt> pieces)
        {
            if (w == 1 && h == 1) return; // Should not happen with proper splitting

            if (w == 1)
            {
                Split1D(x, y, h, true, maxLength, pieces);
                return;
            }
            
            if (h == 1)
            {
                Split1D(x, y, w, false, maxLength, pieces);
                return;
            }

            // Both w > 1 and h > 1
            bool splitVertically = Random.value > 0.5f;
            
            if (splitVertically)
            {
                int splitW = Random.Range(1, w);
                PartitionGrid(x, y, splitW, h, maxLength, pieces);
                PartitionGrid(x + splitW, y, w - splitW, h, maxLength, pieces);
            }
            else
            {
                int splitH = Random.Range(1, h);
                PartitionGrid(x, y, w, splitH, maxLength, pieces);
                PartitionGrid(x, y + splitH, w, h - splitH, maxLength, pieces);
            }
        }

        private static void Split1D(int x, int y, int length, bool isVertical, int maxLength, List<RectInt> pieces)
        {
            int currentPos = 0;
            while (currentPos < length)
            {
                int remaining = length - currentPos;
                if (remaining == 0) break;
                
                int pieceLen;
                
                if (remaining <= maxLength)
                {
                    if (remaining >= 4 && maxLength >= 2 && Random.value > 0.5f)
                    {
                        pieceLen = Random.Range(2, remaining - 1);
                    }
                    else
                    {
                        pieceLen = remaining;
                    }
                }
                else
                {
                    int maxAllowed = Mathf.Min(maxLength, remaining - 2); 
                    if (maxAllowed >= 2)
                    {
                        pieceLen = Random.Range(2, maxAllowed + 1);
                    }
                    else
                    {
                        pieceLen = 2; 
                        if (remaining - pieceLen == 1) pieceLen = 3; // Prevent leaving exactly 1
                    }
                }
                
                // Safety catch to absolutely prevent 1-length pieces
                if (pieceLen < 2) pieceLen = 2;
                if (currentPos + pieceLen > length) pieceLen = length - currentPos;

                if (isVertical)
                    pieces.Add(new RectInt(x, y + currentPos, 1, pieceLen));
                else
                    pieces.Add(new RectInt(x + currentPos, y, pieceLen, 1));
                    
                currentPos += pieceLen;
            }
        }

        /// <summary>
        /// Determines the head direction for the arrow.
        /// With outwardChance probability, the head is placed on an edge facing outward.
        /// Otherwise, a random valid direction is chosen.
        /// Path[0] = head, so we may need to reverse the path.
        /// </summary>
        private static ArrowDirection DetermineHeadDirection(
            List<Vector2Int> path, int gridWidth, int gridHeight, float outwardChance)
        {
            // Try to make the head face outward (better for solvability)
            if (Random.value < outwardChance)
            {
                // Check if first point (path[0]) is on edge
                ArrowDirection? outDir = GetOutwardDirectionForPoint(path[0], gridWidth, gridHeight);
                if (outDir.HasValue)
                {
                    return outDir.Value;
                }

                // Check if last point is on edge — if so, reverse the path
                ArrowDirection? tailOutDir = GetOutwardDirectionForPoint(
                    path[path.Count - 1], gridWidth, gridHeight);
                if (tailOutDir.HasValue)
                {
                    path.Reverse();
                    return tailOutDir.Value;
                }
            }

            // Not on edge or random: direction from path[0] opposite to path[1]
            if (path.Count >= 2)
            {
                Vector2Int headToSecond = path[1] - path[0];
                // Head direction is opposite to the path direction (pointing away from body)
                ArrowDirection bodyDir = VectorToDirection(headToSecond);
                return OppositeDirection(bodyDir);
            }

            return (ArrowDirection)Random.Range(0, 4);
        }

        /// <summary>
        /// Returns an outward direction if the point is on the grid edge, null otherwise.
        /// If multiple edge directions exist (corner), picks one randomly.
        /// </summary>
        private static ArrowDirection? GetOutwardDirectionForPoint(
            Vector2Int point, int gridWidth, int gridHeight)
        {
            var candidates = new List<ArrowDirection>(4);

            if (point.x == 0) candidates.Add(ArrowDirection.Left);
            if (point.x == gridWidth - 1) candidates.Add(ArrowDirection.Right);
            if (point.y == 0) candidates.Add(ArrowDirection.Down);
            if (point.y == gridHeight - 1) candidates.Add(ArrowDirection.Up);

            if (candidates.Count == 0) return null;
            return candidates[Random.Range(0, candidates.Count)];
        }

        private static ArrowDirection VectorToDirection(Vector2Int vec)
        {
            if (vec.y > 0) return ArrowDirection.Up;
            if (vec.y < 0) return ArrowDirection.Down;
            if (vec.x < 0) return ArrowDirection.Left;
            return ArrowDirection.Right;
        }

        private static ArrowDirection OppositeDirection(ArrowDirection dir)
        {
            return dir switch
            {
                ArrowDirection.Up => ArrowDirection.Down,
                ArrowDirection.Down => ArrowDirection.Up,
                ArrowDirection.Left => ArrowDirection.Right,
                ArrowDirection.Right => ArrowDirection.Left,
                _ => ArrowDirection.Up
            };
        }

        private static LevelParams ReduceDifficulty(LevelParams original, float reduction)
        {
            LevelParams easier = original;
            easier.OutwardChance = Mathf.Min(0.95f, original.OutwardChance + reduction * 2f);
            easier.MobHP = Mathf.Max(1, Mathf.FloorToInt(original.MobHP * (1f - reduction)));
            easier.TotalMobs = Mathf.Max(1, Mathf.FloorToInt(original.TotalMobs * (1f - reduction)));
            easier.ArrowCount = Mathf.Max(3, Mathf.FloorToInt(original.ArrowCount * (1f - reduction * 0.5f)));
            easier.MaxWeight = Mathf.Max(1, easier.MaxWeight - 1);
            return easier;
        }

        private static void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        /// <summary>
        /// Attempts to resolve direction deadlocks using a multi-layered approach:
        /// 1. Targeted pattern fixes (self-blocking, head-to-head)
        /// 2. Simulation-based greedy resolver (finds stuck arrows, flips them)
        /// 3. Guaranteed outward orientation as final fallback
        /// Path shapes are never modified — only arrow directions change.
        /// </summary>
        private static bool SolveDirectionDeadlocks(
            List<SolvabilityChecker.ArrowPlacement> placements,
            int gridWidth, int gridHeight,
            int totalMobHP, float winabilityRatio)
        {
            if (placements == null || placements.Count == 0) return false;

            // 1. Initial check — maybe already solvable
            var initialCheck = SolvabilityChecker.Check(placements, gridWidth, gridHeight, totalMobHP, winabilityRatio);
            if (initialCheck.IsValid) return true;

            // 2. Targeted pattern fixes
            FixSelfBlockingArrows(placements, gridWidth, gridHeight);
            RemoveHeadToHeadConflicts(placements, gridWidth, gridHeight);

            var targetedCheck = SolvabilityChecker.Check(placements, gridWidth, gridHeight, totalMobHP, winabilityRatio);
            if (targetedCheck.IsValid) return true;

            // 3. Simulation-based greedy resolver — the main solver
            ResolveDeadlockedArrows(placements, gridWidth, gridHeight);
            FixSelfBlockingArrows(placements, gridWidth, gridHeight);

            var resolvedCheck = SolvabilityChecker.Check(placements, gridWidth, gridHeight, totalMobHP, winabilityRatio);
            if (resolvedCheck.IsValid) return true;

            // 4. Guaranteed outward fallback + re-resolve
            ApplyGuaranteedOutwardOrientation(placements, gridWidth, gridHeight);
            FixSelfBlockingArrows(placements, gridWidth, gridHeight);
            ResolveDeadlockedArrows(placements, gridWidth, gridHeight);

            var finalCheck = SolvabilityChecker.Check(placements, gridWidth, gridHeight, totalMobHP, winabilityRatio);
            return finalCheck.IsSolvable;
        }

        private static void FlipArrowOrientation(ref SolvabilityChecker.ArrowPlacement placement)
        {
            var path = placement.PathPoints;
            if (path == null || path.Count < 2) return;

            // Reverse path so Head becomes Tail and Tail becomes Head
            path.Reverse();
            Vector2Int bodyDir = path[1] - path[0];
            ArrowDirection newHeadDir = OppositeDirection(VectorToDirection(bodyDir));
            
            placement.PathPoints = path;
            placement.HeadDirection = newHeadDir;
        }

        private static void ApplyGuaranteedOutwardOrientation(
            List<SolvabilityChecker.ArrowPlacement> placements,
            int gridWidth, int gridHeight)
        {
            // Build 2D owner grid for fast O(1) ray obstacle checks
            int[,] owner = new int[gridWidth, gridHeight];
            for (int x = 0; x < gridWidth; x++)
                for (int y = 0; y < gridHeight; y++)
                    owner[x, y] = -1;

            for (int pIdx = 0; pIdx < placements.Count; pIdx++)
            {
                var pts = placements[pIdx].PathPoints;
                if (pts == null) continue;
                for (int ptIdx = 0; ptIdx < pts.Count; ptIdx++)
                {
                    owner[pts[ptIdx].x, pts[ptIdx].y] = pIdx;
                }
            }

            for (int i = 0; i < placements.Count; i++)
            {
                var placement = placements[i];
                var path = placement.PathPoints;
                if (path == null || path.Count < 2) continue;

                Vector2Int endA = path[0];
                Vector2Int endB = path[path.Count - 1];

                Vector2Int dirA = path[0] - path[1];
                Vector2Int dirB = path[path.Count - 1] - path[path.Count - 2];

                var placeA = new SolvabilityChecker.ArrowPlacement(new List<Vector2Int>(path), VectorToDirection(dirA));
                bool selfBlockA = IsSelfBlocking(placeA, gridWidth, gridHeight);

                var reversedPath = new List<Vector2Int>(path);
                reversedPath.Reverse();
                var placeB = new SolvabilityChecker.ArrowPlacement(reversedPath, VectorToDirection(dirB));
                bool selfBlockB = IsSelfBlocking(placeB, gridWidth, gridHeight);

                // If A is clean and B self-blocks, MUST keep A
                if (!selfBlockA && selfBlockB)
                {
                    placements[i] = placeA;
                    continue;
                }
                // If B is clean and A self-blocks, MUST keep B
                if (selfBlockA && !selfBlockB)
                {
                    placements[i] = placeB;
                    continue;
                }

                // Check actual arrow obstacles along each orientation's line of fire
                int obstaclesA = GetRayObstacles(endA, dirA, gridWidth, gridHeight, owner, i);
                int obstaclesB = GetRayObstacles(endB, dirB, gridWidth, gridHeight, owner, i);

                if (obstaclesB < obstaclesA && !selfBlockB)
                {
                    placements[i] = placeB;
                }
                else if (obstaclesA < obstaclesB && !selfBlockA)
                {
                    placements[i] = placeA;
                }
                else
                {
                    // If obstacles are equal, choose the one with fewer geometric steps to grid edge
                    int stepsA = GetStepsToEdge(endA, dirA, gridWidth, gridHeight);
                    int stepsB = GetStepsToEdge(endB, dirB, gridWidth, gridHeight);

                    if (stepsB < stepsA && !selfBlockB)
                    {
                        placements[i] = placeB;
                    }
                    else
                    {
                        placements[i] = placeA;
                    }
                }
            }

            RemoveHeadToHeadConflicts(placements, gridWidth, gridHeight);
        }

        private static int GetRayObstacles(
            Vector2Int pos, Vector2Int dir, int width, int height, int[,] owner, int selfIndex)
        {
            int obstacles = 0;
            Vector2Int curr = pos + dir;
            while (curr.IsInBounds(width, height))
            {
                int o = owner[curr.x, curr.y];
                if (o != -1 && o != selfIndex)
                {
                    obstacles++;
                }
                curr += dir;
            }
            return obstacles;
        }

        private static int GetStepsToEdge(Vector2Int pos, Vector2Int dir, int width, int height)
        {
            int steps = 0;
            Vector2Int curr = pos + dir;
            while (curr.IsInBounds(width, height))
            {
                steps++;
                curr += dir;
            }
            return steps;
        }

        /// <summary>
        /// Multi-pass systematic resolution of head-to-head deadlocks across all rows and columns.
        /// Checks if any two arrow heads on the same row or column point towards each other,
        /// and flips one of them safely without creating self-blocking or new conflicts.
        /// </summary>
        private static void RemoveHeadToHeadConflicts(
            List<SolvabilityChecker.ArrowPlacement> placements,
            int gridWidth, int gridHeight)
        {
            if (placements == null || placements.Count < 2) return;

            int maxPasses = 15;
            for (int pass = 0; pass < maxPasses; pass++)
            {
                bool anyFlipped = false;

                for (int i = 0; i < placements.Count; i++)
                {
                    var pA = placements[i];
                    Vector2Int hA = pA.HeadPoint;
                    ArrowDirection dA = pA.HeadDirection;

                    for (int j = i + 1; j < placements.Count; j++)
                    {
                        var pB = placements[j];
                        Vector2Int hB = pB.HeadPoint;
                        ArrowDirection dB = pB.HeadDirection;

                        bool isConflict = false;

                        // Same row: A is to the left of B, A points Right, B points Left
                        if (hA.y == hB.y)
                        {
                            if (hA.x < hB.x && dA == ArrowDirection.Right && dB == ArrowDirection.Left) isConflict = true;
                            else if (hB.x < hA.x && dB == ArrowDirection.Right && dA == ArrowDirection.Left) isConflict = true;
                        }
                        // Same column: A is below B, A points Up, B points Down
                        else if (hA.x == hB.x)
                        {
                            if (hA.y < hB.y && dA == ArrowDirection.Up && dB == ArrowDirection.Down) isConflict = true;
                            else if (hB.y < hA.y && dB == ArrowDirection.Up && dA == ArrowDirection.Down) isConflict = true;
                        }

                        if (isConflict)
                        {
                            // Conflict found! Try flipping one of the arrows safely
                            anyFlipped |= TryFlipWithSelfBlockCheck(placements, j, i, gridWidth, gridHeight);
                        }
                    }
                }

                if (!anyFlipped) break;
            }
        }

        /// <summary>
        /// Tries to flip primaryIndex arrow. Checks that the flip does not create
        /// self-blocking or new head-to-head conflicts. If primary fails, tries fallbackIndex.
        /// Uses deep copy for safe undo of shared PathPoints references.
        /// </summary>
        private static bool TryFlipWithSelfBlockCheck(
            List<SolvabilityChecker.ArrowPlacement> placements,
            int primaryIndex, int fallbackIndex,
            int gridWidth, int gridHeight)
        {
            // Try primary with strict conflict check
            var primaryOrigPath = new List<Vector2Int>(placements[primaryIndex].PathPoints);
            ArrowDirection primaryOrigDir = placements[primaryIndex].HeadDirection;

            var primary = placements[primaryIndex];
            FlipArrowOrientation(ref primary);

            if (!IsSelfBlocking(primary, gridWidth, gridHeight) &&
                !CreatesHeadToHeadConflict(primary.HeadPoint, primary.HeadDirection, placements, primaryIndex))
            {
                placements[primaryIndex] = primary;
                return true;
            }

            RestoreArrowState(placements, primaryIndex, primaryOrigPath, primaryOrigDir);

            // Try fallback with strict conflict check
            var fallbackOrigPath = new List<Vector2Int>(placements[fallbackIndex].PathPoints);
            ArrowDirection fallbackOrigDir = placements[fallbackIndex].HeadDirection;

            var fallback = placements[fallbackIndex];
            FlipArrowOrientation(ref fallback);

            if (!IsSelfBlocking(fallback, gridWidth, gridHeight) &&
                !CreatesHeadToHeadConflict(fallback.HeadPoint, fallback.HeadDirection, placements, fallbackIndex))
            {
                placements[fallbackIndex] = fallback;
                return true;
            }

            // Relaxed check: accept flip as long as it's not self-blocking
            RestoreArrowState(placements, fallbackIndex, fallbackOrigPath, fallbackOrigDir);

            var relaxedPrimary = placements[primaryIndex];
            FlipArrowOrientation(ref relaxedPrimary);
            if (!IsSelfBlocking(relaxedPrimary, gridWidth, gridHeight))
            {
                placements[primaryIndex] = relaxedPrimary;
                return true;
            }

            RestoreArrowState(placements, primaryIndex, primaryOrigPath, primaryOrigDir);

            var relaxedFallback = placements[fallbackIndex];
            FlipArrowOrientation(ref relaxedFallback);
            if (!IsSelfBlocking(relaxedFallback, gridWidth, gridHeight))
            {
                placements[fallbackIndex] = relaxedFallback;
                return true;
            }

            RestoreArrowState(placements, fallbackIndex, fallbackOrigPath, fallbackOrigDir);
            return false;
        }

        /// <summary>
        /// Generates a handcrafted, deterministic Introductory Level (Level 1).
        /// Sets up 3 clear arrows to teach firing, unblocking, and rainbow mechanics.
        /// </summary>
        private static LevelData GenerateTutorialLevel(MapData map, GameConfig config, int level = 1)
        {
            LevelParams levelParams = new LevelParams
            {
                Level = level,
                DifficultyTier = 1,
                MapIndex = 0,
                ArrowCount = 3,
                OutwardChance = 1f,
                MobHP = 1,
                MobSpeed = 1.0f,
                SpawnInterval = 2.5f,
                TotalMobs = 3,
                MinWeight = 1,
                MaxWeight = 1,
                MapScaleFactor = 1f,
                WaveConfig = new WaveConfig
                {
                    Waves = new WaveData[]
                    {
                        new WaveData { MobCount = 3, MobHP = 1, IsBossWave = false }
                    },
                    WavePauseDuration = 2f
                }
            };

            var placements = new List<SolvabilityChecker.ArrowPlacement>
            {
                // Arrow 1 (Step 1): Direct outward arrow facing Up into the top path
                new SolvabilityChecker.ArrowPlacement(
                    new List<Vector2Int> { new Vector2Int(2, 6), new Vector2Int(2, 5) },
                    ArrowDirection.Up
                ),
                // Arrow 2 (Step 2): Blocked behind Arrow 1; path clears when Arrow 1 fires
                new SolvabilityChecker.ArrowPlacement(
                    new List<Vector2Int> { new Vector2Int(2, 4), new Vector2Int(2, 3) },
                    ArrowDirection.Up
                ),
                // Arrow 3 (Step 3): Side arrow, becomes final Rainbow arrow
                new SolvabilityChecker.ArrowPlacement(
                    new List<Vector2Int> { new Vector2Int(4, 3), new Vector2Int(3, 3) },
                    ArrowDirection.Right
                )
            };

            AssignHarmoniousArrowColors(placements, map.GridWidth, map.GridHeight, config?.ArrowColors?.Length ?? 5);

            LogDebug($"Handcrafted Introductory Level (Level {level}) generated successfully.");

            return new LevelData
            {
                Level = level,
                Params = levelParams,
                Map = map,
                ArrowPlacements = placements,
                IsValid = true,
                GenerationAttempts = 1
            };
        }

        /// <summary>
        /// Assigns harmonious palette colors (0..paletteCount-1) to all arrow placements so that:
        /// 1. At most 2 adjacent/touching arrows share the same color (max monochromatic component size <= 2).
        /// 2. No 3 adjacent arrows in a row, column, or cluster share the same color.
        /// 3. Colors are distributed evenly across the entire palette.
        /// </summary>
        public static void AssignHarmoniousArrowColors(
            List<SolvabilityChecker.ArrowPlacement> placements,
            int gridWidth, int gridHeight, int paletteCount = 5)
        {
            if (placements == null || placements.Count == 0 || paletteCount <= 1) return;

            // 1. Build 2D grid of cell ownership
            int[,] owner = new int[gridWidth, gridHeight];
            for (int x = 0; x < gridWidth; x++)
                for (int y = 0; y < gridHeight; y++)
                    owner[x, y] = -1;

            for (int i = 0; i < placements.Count; i++)
            {
                var pts = placements[i].PathPoints;
                if (pts == null) continue;
                for (int j = 0; j < pts.Count; j++)
                {
                    owner[pts[j].x, pts[j].y] = i;
                }
            }

            // 2. Build adjacency graph
            var neighbors = new List<int>[placements.Count];
            for (int i = 0; i < placements.Count; i++)
            {
                neighbors[i] = new List<int>();
            }

            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            var neighborSet = new HashSet<int>();

            for (int i = 0; i < placements.Count; i++)
            {
                neighborSet.Clear();
                var pts = placements[i].PathPoints;
                if (pts == null) continue;

                for (int pIdx = 0; pIdx < pts.Count; pIdx++)
                {
                    Vector2Int pt = pts[pIdx];
                    for (int d = 0; d < dirs.Length; d++)
                    {
                        Vector2Int n = pt + dirs[d];
                        if (n.x >= 0 && n.x < gridWidth && n.y >= 0 && n.y < gridHeight)
                        {
                            int o = owner[n.x, n.y];
                            if (o != -1 && o != i)
                            {
                                neighborSet.Add(o);
                            }
                        }
                    }
                }
                neighbors[i].AddRange(neighborSet);
            }

            // 3. Greedy balanced coloring with component size <= 2
            int[] colorAssign = new int[placements.Count];
            for (int i = 0; i < colorAssign.Length; i++) colorAssign[i] = -1;
            int[] colorCounts = new int[paletteCount];

            // Local helper: check if assigning color c to node creates component >= 3
            bool IsValidAssignment(int node, int c)
            {
                int sameColorNeighbors = 0;
                var nodeNeighbors = neighbors[node];
                for (int n = 0; n < nodeNeighbors.Count; n++)
                {
                    int nb = nodeNeighbors[n];
                    if (colorAssign[nb] == c)
                    {
                        sameColorNeighbors++;
                        var nbNeighbors = neighbors[nb];
                        for (int n2 = 0; n2 < nbNeighbors.Count; n2++)
                        {
                            int nb2 = nbNeighbors[n2];
                            if (nb2 != node && colorAssign[nb2] == c)
                            {
                                return false; // nb already has another c-neighbor -> chain of 3!
                            }
                        }
                    }
                }
                return sameColorNeighbors <= 1;
            }

            for (int i = 0; i < placements.Count; i++)
            {
                var validColors = new List<int>();
                for (int c = 0; c < paletteCount; c++)
                {
                    if (IsValidAssignment(i, c))
                    {
                        validColors.Add(c);
                    }
                }

                if (validColors.Count > 0)
                {
                    int minCount = int.MaxValue;
                    for (int v = 0; v < validColors.Count; v++)
                    {
                        if (colorCounts[validColors[v]] < minCount)
                            minCount = colorCounts[validColors[v]];
                    }

                    var leastUsed = new List<int>();
                    for (int v = 0; v < validColors.Count; v++)
                    {
                        if (colorCounts[validColors[v]] == minCount)
                            leastUsed.Add(validColors[v]);
                    }

                    int chosen = leastUsed[Random.Range(0, leastUsed.Count)];
                    colorAssign[i] = chosen;
                    colorCounts[chosen]++;
                }
                else
                {
                    // Fallback: pick color that minimizes violations
                    int bestC = 0;
                    int minV = int.MaxValue;
                    for (int c = 0; c < paletteCount; c++)
                    {
                        int v = 0;
                        var nodeNeighbors = neighbors[i];
                        for (int n = 0; n < nodeNeighbors.Count; n++)
                        {
                            if (colorAssign[nodeNeighbors[n]] == c) v++;
                        }
                        if (v < minV)
                        {
                            minV = v;
                            bestC = c;
                        }
                    }
                    colorAssign[i] = bestC;
                    colorCounts[bestC]++;
                }
            }

            // 4. Conflict Resolution Pass (breaks any remaining chains of >= 3 same-color adjacent arrows)
            for (int iter = 0; iter < 50; iter++)
            {
                bool anyFixed = false;
                for (int i = 0; i < placements.Count; i++)
                {
                    int c = colorAssign[i];
                    int sameNeighbors = 0;
                    bool connectedToAnotherChain = false;
                    var nodeNeighbors = neighbors[i];

                    for (int n = 0; n < nodeNeighbors.Count; n++)
                    {
                        int nb = nodeNeighbors[n];
                        if (colorAssign[nb] == c)
                        {
                            sameNeighbors++;
                            var nbNeighbors = neighbors[nb];
                            for (int n2 = 0; n2 < nbNeighbors.Count; n2++)
                            {
                                int nb2 = nbNeighbors[n2];
                                if (nb2 != i && colorAssign[nb2] == c)
                                {
                                    connectedToAnotherChain = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (sameNeighbors >= 2 || connectedToAnotherChain)
                    {
                        // Recolour 'i' to a color that has NO same-color neighbor
                        var cleanColors = new List<int>();
                        for (int newC = 0; newC < paletteCount; newC++)
                        {
                            if (newC == c) continue;
                            bool hasNeighbor = false;
                            for (int n = 0; n < nodeNeighbors.Count; n++)
                            {
                                if (colorAssign[nodeNeighbors[n]] == newC)
                                {
                                    hasNeighbor = true;
                                    break;
                                }
                            }
                            if (!hasNeighbor) cleanColors.Add(newC);
                        }

                        if (cleanColors.Count > 0)
                        {
                            colorCounts[c]--;
                            int chosen = cleanColors[Random.Range(0, cleanColors.Count)];
                            colorAssign[i] = chosen;
                            colorCounts[chosen]++;
                            anyFixed = true;
                        }
                        else
                        {
                            for (int newC = 0; newC < paletteCount; newC++)
                            {
                                if (newC == c) continue;
                                if (IsValidAssignment(i, newC))
                                {
                                    colorCounts[c]--;
                                    colorAssign[i] = newC;
                                    colorCounts[newC]++;
                                    anyFixed = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (!anyFixed) break;
            }

            // 5. Apply assigned colors to placements struct
            for (int i = 0; i < placements.Count; i++)
            {
                var p = placements[i];
                p.ColorIndex = colorAssign[i];
                placements[i] = p;
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private static void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] LevelGenerator: {message}");
        }
    }
}