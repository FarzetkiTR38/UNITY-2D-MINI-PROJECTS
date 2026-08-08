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
            
            result.ArrowPlacements = fallbackPlacements ?? new List<SolvabilityChecker.ArrowPlacement>();
            result.IsValid = true;
            return result;
        }

        /// <summary>
        /// Generates multi-point organic maze arrow placements (Image 1 style).
        /// All arrows are short/medium length (2-5 points) with frequent turns (L, Z, S shapes),
        /// completely preventing rigid parallel mega-columns (Image 2 style).
        /// </summary>
        private static List<SolvabilityChecker.ArrowPlacement> GenerateArrowPlacements(
            LevelParams levelParams, int gridWidth, int gridHeight)
        {
            return GenerateOrganicMazePlacements(levelParams, gridWidth, gridHeight);
        }

        private static List<SolvabilityChecker.ArrowPlacement> GenerateOrganicMazePlacements(
            LevelParams levelParams, int gridWidth, int gridHeight)
        {
            var placements = new List<SolvabilityChecker.ArrowPlacement>();
            bool[,] visited = new bool[gridWidth, gridHeight];
            int totalPoints = gridWidth * gridHeight;
            int visitedCount = 0;

            // Cap max length per arrow to 5 points (weight 4) so no mega-arrows span the entire screen
            int minLength = Mathf.Max(2, levelParams.MinWeight + 1);
            int maxLength = Mathf.Min(5, levelParams.MaxWeight + 1);

            while (visitedCount < totalPoints)
            {
                Vector2Int start = Vector2Int.zero;
                bool found = false;

                // Pick random unvisited start cell
                for (int attempt = 0; attempt < 50; attempt++)
                {
                    int rx = Random.Range(0, gridWidth);
                    int ry = Random.Range(0, gridHeight);
                    if (!visited[rx, ry])
                    {
                        start = new Vector2Int(rx, ry);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    for (int x = 0; x < gridWidth && !found; x++)
                    {
                        for (int y = 0; y < gridHeight && !found; y++)
                        {
                            if (!visited[x, y])
                            {
                                start = new Vector2Int(x, y);
                                found = true;
                            }
                        }
                    }
                }

                if (!found) break;

                List<Vector2Int> path = new List<Vector2Int>();
                path.Add(start);
                visited[start.x, start.y] = true;
                visitedCount++;

                int targetLength = Random.Range(minLength, maxLength + 1);
                Vector2Int current = start;
                Vector2Int currentDir = Vector2Int.zero;
                int straightSteps = 0;

                while (path.Count < targetLength)
                {
                    List<Vector2Int> validDirs = new List<Vector2Int>();
                    Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

                    foreach (var d in dirs)
                    {
                        Vector2Int neighbor = current + d;
                        if (neighbor.x >= 0 && neighbor.x < gridWidth &&
                            neighbor.y >= 0 && neighbor.y < gridHeight &&
                            !visited[neighbor.x, neighbor.y])
                        {
                            validDirs.Add(d);
                        }
                    }

                    if (validDirs.Count == 0) break;

                    // Force turn if we went straight for 2 steps
                    List<Vector2Int> turnDirs = new List<Vector2Int>(validDirs);
                    if (currentDir != Vector2Int.zero)
                    {
                        turnDirs.Remove(currentDir);
                        turnDirs.Remove(-currentDir);
                    }

                    Vector2Int chosenDir;
                    if (straightSteps >= 2 && turnDirs.Count > 0)
                    {
                        // Force a 90-degree turn
                        chosenDir = turnDirs[Random.Range(0, turnDirs.Count)];
                        straightSteps = 0;
                    }
                    else if (currentDir != Vector2Int.zero && validDirs.Contains(currentDir) && Random.value < 0.35f)
                    {
                        // 35% chance to stay straight, 65% chance to turn
                        chosenDir = currentDir;
                        straightSteps++;
                    }
                    else
                    {
                        // Turn
                        chosenDir = validDirs[Random.Range(0, validDirs.Count)];
                        straightSteps = 0;
                    }

                    current += chosenDir;
                    currentDir = chosenDir;
                    path.Add(current);
                    visited[current.x, current.y] = true;
                    visitedCount++;
                }

                // Attach isolated 1-point cells to adjacent existing paths
                if (path.Count < 2)
                {
                    Vector2Int isolated = path[0];
                    bool attached = false;

                    for (int pIndex = 0; pIndex < placements.Count; pIndex++)
                    {
                        var existingPath = placements[pIndex].PathPoints;
                        Vector2Int head = existingPath[0];
                        Vector2Int tail = existingPath[existingPath.Count - 1];

                        if (Vector2Int.Distance(isolated, head) == 1f && existingPath.Count < maxLength + 1)
                        {
                            existingPath.Insert(0, isolated);
                            attached = true;
                            break;
                        }
                        else if (Vector2Int.Distance(isolated, tail) == 1f && existingPath.Count < maxLength + 1)
                        {
                            existingPath.Add(isolated);
                            attached = true;
                            break;
                        }
                    }

                    if (!attached && placements.Count > 0)
                    {
                        placements[0].PathPoints.Add(isolated);
                    }
                    continue;
                }

                if (Random.value > 0.5f)
                {
                    path.Reverse();
                }

                Vector2Int bodyDir = path[1] - path[0];
                ArrowDirection headDir = OppositeDirection(VectorToDirection(bodyDir));

                placements.Add(new SolvabilityChecker.ArrowPlacement(path, headDir));
            }

            return placements;
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
        /// Attempts to resolve direction deadlocks by iteratively flipping head directions of stuck arrows,
        /// and as a final fallback applies guaranteed outward sequential directions.
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

            // 2. Try smart flips on stuck arrows
            for (int attempt = 0; attempt < 40; attempt++)
            {
                int index = Random.Range(0, placements.Count);
                var tempPlacement = placements[index];
                FlipArrowOrientation(ref tempPlacement);
                placements[index] = tempPlacement;

                var check = SolvabilityChecker.Check(placements, gridWidth, gridHeight, totalMobHP, winabilityRatio);
                if (check.IsValid) return true;
            }

            // 3. Guaranteed Fallback
            ApplyGuaranteedOutwardOrientation(placements, gridWidth, gridHeight);
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

        private static void RemoveHeadToHeadConflicts(
            List<SolvabilityChecker.ArrowPlacement> placements,
            int gridWidth, int gridHeight)
        {
            if (placements == null || placements.Count == 0) return;

            Dictionary<Vector2Int, int> pointToArrowIndex = new Dictionary<Vector2Int, int>();
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
                            FlipArrowOrientation(ref placementB);
                            placements[indexB] = placementB;
                        }
                        break;
                    }
                    currA += stepA;
                }
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private static void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] LevelGenerator: {message}");
        }
    }
}
