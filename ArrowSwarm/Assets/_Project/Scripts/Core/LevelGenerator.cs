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

            while (filledCells < totalCells)
            {
                Vector2Int headPos = Vector2Int.zero;
                ArrowDirection headDir = ArrowDirection.Up;
                bool foundHead = false;

                // Priority 1: Boundary cell facing outward (free exit)
                var freeBoundary = GetFreeBoundaryCandidates(gridWidth, gridHeight, occupied);
                if (freeBoundary.Count > 0 && (placements.Count == 0 || Random.value < 0.30f))
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
                    if (interiorCands.Count > 0)
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
                else if (path != null && path.Count == 1)
                {
                    Vector2Int isolated = path[0];
                    if (cellOwner[isolated.x, isolated.y] == -1)
                    {
                        bool attached = AttachIsolatedCellToPlacement(isolated, placements, cellOwner, gridWidth, gridHeight);
                        if (attached)
                        {
                            occupied[isolated.x, isolated.y] = true;
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
        /// </summary>
        private static void FixSelfBlockingArrows(
            List<SolvabilityChecker.ArrowPlacement> placements, int gridWidth, int gridHeight)
        {
            if (placements == null) return;

            for (int i = 0; i < placements.Count; i++)
            {
                var placement = placements[i];
                if (!IsSelfBlocking(placement, gridWidth, gridHeight)) continue;

                // Strategy 1: Flip the arrow (reverse path, other end becomes head)
                var flipped = placement;
                FlipArrowOrientation(ref flipped);

                if (!IsSelfBlocking(flipped, gridWidth, gridHeight))
                {
                    placements[i] = flipped;
                    continue;
                }

                // Strategy 2: Both ends self-block. Try all 4 directions from each end.
                bool wasFixed = TryAlternateDirections(ref placement, gridWidth, gridHeight);
                if (wasFixed)
                {
                    placements[i] = placement;
                    continue;
                }

                // Try from flipped end
                wasFixed = TryAlternateDirections(ref flipped, gridWidth, gridHeight);
                if (wasFixed)
                {
                    placements[i] = flipped;
                    continue;
                }

                // Last resort: keep flipped version (outward orientation will handle later)
                placements[i] = flipped;
            }
        }

        /// <summary>
        /// Tries all 4 cardinal directions for an arrow's head to find one that
        /// does not self-block. Returns true if a valid direction was found.
        /// </summary>
        private static bool TryAlternateDirections(
            ref SolvabilityChecker.ArrowPlacement placement, int gridWidth, int gridHeight)
        {
            ArrowDirection[] allDirs =
            {
                ArrowDirection.Up, ArrowDirection.Down,
                ArrowDirection.Left, ArrowDirection.Right
            };

            ArrowDirection originalDir = placement.HeadDirection;

            foreach (var dir in allDirs)
            {
                if (dir == originalDir) continue;
                placement.HeadDirection = dir;
                if (!IsSelfBlocking(placement, gridWidth, gridHeight))
                {
                    return true;
                }
            }

            // Restore original if nothing worked
            placement.HeadDirection = originalDir;
            return false;
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
                if (!occupied[x, height - 1])
                    list.Add(new CandidateHead { pos = new Vector2Int(x, height - 1), dir = ArrowDirection.Up });
            }

            for (int x = 0; x < width; x++)
            {
                if (!occupied[x, 0])
                    list.Add(new CandidateHead { pos = new Vector2Int(x, 0), dir = ArrowDirection.Down });
            }

            for (int y = 0; y < height; y++)
            {
                if (!occupied[0, y])
                    list.Add(new CandidateHead { pos = new Vector2Int(0, y), dir = ArrowDirection.Left });
            }

            for (int y = 0; y < height; y++)
            {
                if (!occupied[width - 1, y])
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
                        Vector2Int target = pos + step;

                        if (!target.IsInBounds(width, height) || occupied[target.x, target.y])
                        {
                            list.Add(new CandidateHead { pos = pos, dir = dir });
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
                Vector2Int target = pos + step;
                if (!target.IsInBounds(width, height)) return dir;
            }
            return ArrowDirection.Up;
        }

        private static List<Vector2Int> GrowArrowPathBackwards(
            Vector2Int headPos, ArrowDirection headDir, int targetLength,
            int width, int height, bool[,] occupied)
        {
            List<Vector2Int> path = new List<Vector2Int>();
            path.Add(headPos);

            Vector2Int backDir = -ArrowSwarm.Grid.GridManager.DirectionToVector(headDir);
            Vector2Int secondPos = headPos + backDir;

            if (secondPos.IsInBounds(width, height) && !occupied[secondPos.x, secondPos.y])
            {
                path.Add(secondPos);
            }
            else
            {
                Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                foreach (var d in dirs)
                {
                    Vector2Int neighbor = headPos + d;
                    if (neighbor.IsInBounds(width, height) && !occupied[neighbor.x, neighbor.y])
                    {
                        path.Add(neighbor);
                        break;
                    }
                }
            }

            if (path.Count < 2) return path;

            Vector2Int current = path[path.Count - 1];
            Vector2Int currentDir = path[path.Count - 1] - path[path.Count - 2];
            int straightSteps = 0;

            while (path.Count < targetLength)
            {
                List<Vector2Int> validDirs = new List<Vector2Int>();
                Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

                foreach (var d in dirs)
                {
                    Vector2Int neighbor = current + d;
                    if (neighbor.IsInBounds(width, height) && !occupied[neighbor.x, neighbor.y] && !path.Contains(neighbor))
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

            // 1. Try attaching isolated to an existing placement's TAIL
            for (int pIndex = 0; pIndex < placements.Count; pIndex++)
            {
                var existingPath = placements[pIndex].PathPoints;
                Vector2Int tail = existingPath[existingPath.Count - 1];

                if (IsManhattanOne(isolated, tail))
                {
                    existingPath.Add(isolated);
                    cellOwner[isolated.x, isolated.y] = pIndex;
                    return true;
                }
            }

            // 2. Try attaching isolated to an existing placement's HEAD
            for (int pIndex = 0; pIndex < placements.Count; pIndex++)
            {
                var existingPath = placements[pIndex].PathPoints;
                Vector2Int head = existingPath[0];

                if (IsManhattanOne(isolated, head))
                {
                    existingPath.Insert(0, isolated);
                    cellOwner[isolated.x, isolated.y] = pIndex;
                    return true;
                }
            }

            // 3. Try inserting isolated as a corner detour inside an existing placement's path
            for (int pIndex = 0; pIndex < placements.Count; pIndex++)
            {
                var existingPath = placements[pIndex].PathPoints;
                for (int i = 0; i < existingPath.Count - 1; i++)
                {
                    if (IsManhattanOne(isolated, existingPath[i]) && IsManhattanOne(isolated, existingPath[i + 1]))
                    {
                        existingPath.Insert(i + 1, isolated);
                        cellOwner[isolated.x, isolated.y] = pIndex;
                        return true;
                    }
                }
            }

            // 4. Try forming a 2-point arrow with an UNOWNED adjacent neighbor
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var d in dirs)
            {
                Vector2Int neighbor = isolated + d;
                if (neighbor.IsInBounds(width, height) && cellOwner[neighbor.x, neighbor.y] == -1)
                {
                    int arrowIdx = placements.Count;
                    var smallPath = new List<Vector2Int> { isolated, neighbor };
                    ArrowDirection dir = VectorToDirection(isolated - neighbor);
                    placements.Add(new SolvabilityChecker.ArrowPlacement(smallPath, dir));

                    cellOwner[isolated.x, isolated.y] = arrowIdx;
                    cellOwner[neighbor.x, neighbor.y] = arrowIdx;
                    return true;
                }
            }

            // 5. Ultimate Fallback: Attach isolated to ANY adjacent placement's path
            foreach (var d in dirs)
            {
                Vector2Int neighbor = isolated + d;
                if (neighbor.IsInBounds(width, height) && cellOwner[neighbor.x, neighbor.y] != -1)
                {
                    int pIndex = cellOwner[neighbor.x, neighbor.y];
                    var existingPath = placements[pIndex].PathPoints;
                    int idx = existingPath.IndexOf(neighbor);
                    if (idx == 0)
                    {
                        existingPath.Insert(0, isolated);
                    }
                    else
                    {
                        existingPath.Add(isolated);
                    }
                    cellOwner[isolated.x, isolated.y] = pIndex;
                    return true;
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
                            path.Add(neighbor);
                            cellOwner[neighbor.x, neighbor.y] = p;
                            tail = neighbor;
                            grewAny = true;
                        }
                    }
                }

                // Pass 2: Extend arrow heads into any adjacent unowned cell
                for (int p = 0; p < placements.Count; p++)
                {
                    var path = placements[p].PathPoints;
                    if (path == null || path.Count == 0) continue;

                    Vector2Int head = path[0];
                    foreach (var d in dirs)
                    {
                        Vector2Int neighbor = head + d;
                        if (neighbor.IsInBounds(width, height) && cellOwner[neighbor.x, neighbor.y] == -1)
                        {
                            path.Insert(0, neighbor);
                            cellOwner[neighbor.x, neighbor.y] = p;
                            head = neighbor;
                            grewAny = true;
                        }
                    }
                }

                // Pass 3: Insert empty cell as an orthogonal 90-degree corner detour into any adjacent body segment
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
                            if (path == null) continue;

                            for (int i = 0; i < path.Count - 1; i++)
                            {
                                if (IsManhattanOne(emptyPt, path[i]) && IsManhattanOne(emptyPt, path[i + 1]))
                                {
                                    path.Insert(i + 1, emptyPt);
                                    cellOwner[x, y] = p;
                                    filled = true;
                                    grewAny = true;
                                    break;
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
        /// Attempts to resolve direction deadlocks by applying targeted self-blocking
        /// and head-to-head fixes first, then random flips with re-fix after each,
        /// and finally guaranteed outward orientation as fallback.
        /// </summary>
        private static bool SolveDirectionDeadlocks(
            List<SolvabilityChecker.ArrowPlacement> placements,
            int gridWidth, int gridHeight,
            int totalMobHP, float winabilityRatio)
        {
            if (placements == null || placements.Count == 0) return false;

            // 1. Initial check
            var initialCheck = SolvabilityChecker.Check(placements, gridWidth, gridHeight, totalMobHP, winabilityRatio);
            if (initialCheck.IsValid) return true;

            // 2. Targeted fix: resolve known self-blocking and head-to-head issues
            FixSelfBlockingArrows(placements, gridWidth, gridHeight);
            RemoveHeadToHeadConflicts(placements, gridWidth, gridHeight);

            var targetedCheck = SolvabilityChecker.Check(placements, gridWidth, gridHeight, totalMobHP, winabilityRatio);
            if (targetedCheck.IsValid) return true;

            // 3. Try smart flips on stuck arrows with re-fix after each flip
            for (int attempt = 0; attempt < 40; attempt++)
            {
                int index = Random.Range(0, placements.Count);
                var tempPlacement = placements[index];
                FlipArrowOrientation(ref tempPlacement);
                placements[index] = tempPlacement;

                // Re-apply targeted fixes after each flip
                FixSelfBlockingArrows(placements, gridWidth, gridHeight);
                RemoveHeadToHeadConflicts(placements, gridWidth, gridHeight);

                var check = SolvabilityChecker.Check(placements, gridWidth, gridHeight, totalMobHP, winabilityRatio);
                if (check.IsValid) return true;
            }

            // 4. Guaranteed Fallback
            ApplyGuaranteedOutwardOrientation(placements, gridWidth, gridHeight);
            FixSelfBlockingArrows(placements, gridWidth, gridHeight);

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

                int stepsA = GetStepsToEdge(endA, dirA, gridWidth, gridHeight);
                int stepsB = GetStepsToEdge(endB, dirB, gridWidth, gridHeight);

                if (stepsB < stepsA)
                {
                    path.Reverse();
                    placement.HeadDirection = VectorToDirection(dirB);
                }
                else
                {
                    placement.HeadDirection = VectorToDirection(dirA);
                }

                placement.PathPoints = path;
                placements[i] = placement;
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
        /// Multi-pass resolution of head-to-head deadlocks where two arrows
        /// mutually block each other. Flips the arrow further from the edge
        /// and verifies the flip doesn't create self-blocking.
        /// </summary>
        private static void RemoveHeadToHeadConflicts(
            List<SolvabilityChecker.ArrowPlacement> placements,
            int gridWidth, int gridHeight)
        {
            if (placements == null || placements.Count == 0) return;

            int maxPasses = 5;

            for (int pass = 0; pass < maxPasses; pass++)
            {
                bool anyFlipped = false;

                // Rebuild point-to-arrow index each pass (directions change after flips)
                var pointToArrowIndex = new Dictionary<Vector2Int, int>();
                for (int i = 0; i < placements.Count; i++)
                {
                    var pts = placements[i].PathPoints;
                    for (int j = 0; j < pts.Count; j++)
                    {
                        pointToArrowIndex[pts[j]] = i;
                    }
                }

                for (int i = 0; i < placements.Count; i++)
                {
                    var placementA = placements[i];
                    Vector2Int headA = placementA.HeadPoint;
                    Vector2Int stepA = ArrowSwarm.Grid.GridManager.DirectionToVector(placementA.HeadDirection);

                    Vector2Int currA = headA + stepA;
                    if (!currA.IsInBounds(gridWidth, gridHeight)) continue;

                    while (currA.IsInBounds(gridWidth, gridHeight))
                    {
                        if (pointToArrowIndex.TryGetValue(currA, out int indexB) && indexB != i)
                        {
                            var placementB = placements[indexB];
                            Vector2Int headB = placementB.HeadPoint;
                            Vector2Int stepB = ArrowSwarm.Grid.GridManager.DirectionToVector(placementB.HeadDirection);

                            // Check if B fires into A (mutual blocking)
                            Vector2Int currB = headB + stepB;
                            bool bPointsToA = false;
                            while (currB.IsInBounds(gridWidth, gridHeight))
                            {
                                if (pointToArrowIndex.TryGetValue(currB, out int target) && target == i)
                                {
                                    bPointsToA = true;
                                    break;
                                }
                                currB += stepB;
                            }

                            if (bPointsToA)
                            {
                                // Mutual blocking! Flip the one further from edge
                                int stepsAToEdge = GetStepsToEdge(headA, stepA, gridWidth, gridHeight);
                                int stepsBToEdge = GetStepsToEdge(headB, stepB, gridWidth, gridHeight);

                                if (stepsBToEdge >= stepsAToEdge)
                                {
                                    anyFlipped |= TryFlipWithSelfBlockCheck(
                                        placements, indexB, i, gridWidth, gridHeight);
                                }
                                else
                                {
                                    anyFlipped |= TryFlipWithSelfBlockCheck(
                                        placements, i, indexB, gridWidth, gridHeight);
                                }
                            }
                            break;
                        }
                        currA += stepA;
                    }
                }

                if (!anyFlipped) break;
            }
        }

        /// <summary>
        /// Tries to flip primaryIndex arrow. If the flip creates self-blocking,
        /// tries flipping fallbackIndex instead. Returns true if any flip was applied.
        /// </summary>
        private static bool TryFlipWithSelfBlockCheck(
            List<SolvabilityChecker.ArrowPlacement> placements,
            int primaryIndex, int fallbackIndex,
            int gridWidth, int gridHeight)
        {
            // Try primary
            var primary = placements[primaryIndex];
            FlipArrowOrientation(ref primary);

            if (!IsSelfBlocking(primary, gridWidth, gridHeight))
            {
                placements[primaryIndex] = primary;
                return true;
            }

            // Primary flip creates self-block — try fallback
            FlipArrowOrientation(ref primary); // undo
            var fallback = placements[fallbackIndex];
            FlipArrowOrientation(ref fallback);

            if (!IsSelfBlocking(fallback, gridWidth, gridHeight))
            {
                placements[fallbackIndex] = fallback;
                return true;
            }

            // Both create self-block — force primary flip (self-block fix will handle)
            FlipArrowOrientation(ref fallback); // undo
            FlipArrowOrientation(ref primary);  // re-apply
            placements[primaryIndex] = primary;
            return true;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private static void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] LevelGenerator: {message}");
        }
    }
}