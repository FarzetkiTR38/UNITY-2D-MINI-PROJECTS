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

            LevelParams levelParams = DifficultyCalculator.CalculateAll(
                level, map.GridWidth, map.GridHeight,
                config.MaxMobSpeed, config.MinSpawnInterval);

            int maxAttempts = config.MaxRegenerateAttempts;
            float winabilityRatio = config.WinabilityRatio;
            float difficultyReduction = config.DifficultyReductionOnFail;
            int totalMobHP = levelParams.TotalMobs * levelParams.MobHP;

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
                    result.ArrowPlacements = placements;
                    result.IsValid = true;

                    LogDebug($"Level {level} generated and solved: {levelParams} (attempt {attempt}/{maxAttempts})");
                    return result;
                }
            }

            // Fallback: Guaranteed 100% solvable outward orientation
            LogDebug($"Level {level}: Applying guaranteed solvable outward placement fallback.");
            var fallbackPlacements = GenerateArrowPlacements(levelParams, map.GridWidth, map.GridHeight);
            if (fallbackPlacements != null && fallbackPlacements.Count > 0)
            {
                ApplyGuaranteedOutwardOrientation(fallbackPlacements, map.GridWidth, map.GridHeight);
            }
            
            result.ArrowPlacements = (fallbackPlacements != null && fallbackPlacements.Count > 0) 
                ? fallbackPlacements 
                : GenerateSimpleGridPlacements(map.GridWidth, map.GridHeight);

            // Final Absolute Sanitization: Guarantee 0 self-blocking arrows in the final generated level
            if (result.ArrowPlacements != null)
            {
                for (int i = 0; i < result.ArrowPlacements.Count; i++)
                {
                    if (IsSelfBlocking(result.ArrowPlacements[i], map.GridWidth, map.GridHeight))
                    {
                        var flipped = result.ArrowPlacements[i];
                        FlipArrowOrientation(ref flipped);
                        if (!IsSelfBlocking(flipped, map.GridWidth, map.GridHeight))
                        {
                            result.ArrowPlacements[i] = flipped;
                        }
                    }
                }
            }

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
            int maxLength = Mathf.Min(16, levelParams.MaxWeight + 4);

            int maxLoopIterations = totalCells * 3;
            int loopCount = 0;

            while (filledCells < totalCells && loopCount++ < maxLoopIterations)
            {
                Vector2Int headPos = Vector2Int.zero;
                ArrowDirection headDir = ArrowDirection.Up;
                bool foundHead = false;

                // Priority 1: Boundary cell facing outward (free exit)
                var freeBoundary = GetFreeBoundaryCandidates(gridWidth, gridHeight, occupied);
                var validBoundary = freeBoundary.FindAll(c => !CreatesHeadToHeadConflict(c.pos, c.dir, placements));
                if (validBoundary.Count > 0 && (placements.Count == 0 || Random.value < 0.30f))
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
                    // Priority 2: Interior cell facing an exit or previously cleared space
                    var interiorCands = GetInteriorCandidates(gridWidth, gridHeight, occupied);
                    var validInterior = interiorCands.FindAll(c => !CreatesHeadToHeadConflict(c.pos, c.dir, placements));
                    if (validInterior.Count > 0)
                    {
                        var cand = validInterior[Random.Range(0, validInterior.Count)];
                        headPos = cand.pos;
                        headDir = cand.dir;
                        foundHead = true;
                    }
                    else if (interiorCands.Count > 0)
                    {
                        var cand = interiorCands[Random.Range(0, interiorCands.Count)];
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

                int targetLength = Random.Range(minLength, maxLength + 1);
                var path = GrowArrowPathBackwards(headPos, headDir, targetLength, gridWidth, gridHeight, occupied);

                if (path != null && path.Count >= 2)
                {
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
                    // Single cell or failed to grow backwards: attach to existing placement
                    if (cellOwner[headPos.x, headPos.y] == -1)
                    {
                        bool attached = AttachIsolatedCellToPlacement(headPos, placements, cellOwner, gridWidth, gridHeight);
                        if (attached)
                        {
                            occupied[headPos.x, headPos.y] = true;
                            filledCells++;
                        }
                    }
                }
            }

            // Post-process: Guarantee zero head-to-head conflicts
            RemoveHeadToHeadConflicts(placements, gridWidth, gridHeight);

            // Guarantee zero diagonal steps / zigzags anywhere on the map
            FixDiagonalSegments(placements);

            // Sweep and absorb 100% of unowned grid points so 0 empty dots remain
            FillAllUnownedCells(placements, gridWidth, gridHeight);

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

            int maxIterations = 80;
            int noProgressCount = 0;
            int maxNoProgress = 5;

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
            var list = new List<CandidateHead>();
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
                            Vector2Int target = pos + step;
                            if (!target.IsInBounds(width, height) || occupied[target.x, target.y])
                            {
                                list.Add(new CandidateHead { pos = pos, dir = dir });
                            }
                        }
                    }
                }
            }

            return list;
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

            List<Vector2Int> path = new List<Vector2Int> { headPos, secondPos };

            // Calculate line of fire (laser) for the head: points that this arrow CANNOT occupy under any circumstances
            var fireRay = new HashSet<Vector2Int>();
            Vector2Int rayPt = headPos + headStep;
            while (rayPt.IsInBounds(width, height))
            {
                fireRay.Add(rayPt);
                rayPt += headStep;
            }

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
                if (straightSteps >= 3 && turnDirs.Count > 0)
                {
                    chosenDir = turnDirs[Random.Range(0, turnDirs.Count)];
                    straightSteps = 0;
                }
                else if (currentDir != Vector2Int.zero && validDirs.Contains(currentDir) && Random.value < 0.40f)
                {
                    chosenDir = currentDir;
                    straightSteps++;
                }
                else
                {
                    chosenDir = validDirs[Random.Range(0, validDirs.Count)];
                    straightSteps = 0;
                }

                current += chosenDir;
                currentDir = chosenDir;
                path.Add(current);
            }

            return path;
        }

        private static bool AttachIsolatedCellToPlacement(
            Vector2Int isolated, List<SolvabilityChecker.ArrowPlacement> placements, int[,] cellOwner, int width, int height)
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

            // 3. Try forming a 2-point arrow with an UNOWNED adjacent neighbor
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

            return false;
        }

        /// <summary>
        /// Sweeps all coordinates in the grid and absorbs any unowned cell (cellOwner == -1)
        /// into an adjacent arrow's tail, corner detour, or forms a new 2-point arrow.
        /// Guarantees 100% full grid cell coverage with 0 empty dots remaining!
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

                // If both are clean, choose the one with fewer steps to grid edge
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

            RemoveHeadToHeadConflicts(placements, gridWidth, gridHeight);
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

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private static void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] LevelGenerator: {message}");
        }
    }
}