namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using UnityEditor;
    using UnityEngine;
    using U = ModeAuthoringUtility;

    /// <summary>Independent dense-mask, geometry, dependency, budget and catalog regressions.</summary>
    public static class DenseCampaignVerification
    {
        /// <summary>Requires a bijection between intended mask cells and arrow path cells.</summary>
        public static void AssertCoverage(IReadOnlyList<PuzzleArrowData> arrows, int width, int height, bool[,] mask)
        {
            var used = new bool[width, height];
            foreach (var arrow in arrows) foreach (var p in arrow.Points)
            {
                Require(p.x >= 0 && p.y >= 0 && p.x < width && p.y < height, "off-board path cell");
                Require(mask[p.x, p.y] && !used[p.x, p.y], "overlap or arrow outside silhouette");
                used[p.x, p.y] = true;
            }
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
                Require(used[x, y] == mask[x, y], "uncovered silhouette cell at " + x + "," + y);
        }

        /// <summary>Proves complete silhouette occupancy and an independent gameplay solution.</summary>
        public static PuzzleSolution AssertLevel(PuzzleLevel level, int index)
        {
            var mask = new bool[level.Width, level.Height];
            for (int y = 0; y < level.Height; y++) for (int x = 0; x < level.Width; x++)
                mask[x, y] = level.Mode == GameModeId.ColorShift ||
                    DenseCampaignGeneration.SampleShape(index, x, y, level.Width, level.Height).a > .5f;
            AssertCoverage(level.Arrows, level.Width, level.Height, mask);
            var solution = PuzzleSolver.Solve(level);
            Require(solution.Solvable, level.Title + ": " + solution.Error);
            var graph = DenseDependencyGraph.Analyze(level.Arrows, level.Width, level.Height);
            Require(graph.Solvable && graph.MaximumDepth > 1, level.Title + ": missing dependencies");
            Require(graph.DirectionCount >= 3 && graph.InwardHeads > 0, level.Title + ": insufficient head variety");
            if (level.Mode == GameModeId.Shapes) Require(graph.DirectionCount == 4, level.Title + ": expected all four head directions");
            if (level.Mode == GameModeId.ColorShift)
            {
                Require(solution.RequiredTurns > 0 && solution.RequiredTurns <= level.TurnBudget, "invalid rotation budget");
                for (int i = 0; i < level.Arrows.Count; i++)
                    Require(level.Arrows[i].Color == level.WallColors[level.Arrows[i].ColorId], "logical/display palette mismatch");
            }
            return solution;
        }

        /// <summary>Returns a stable geometry identity, independent of title and palette.</summary>
        public static string GeometrySignature(PuzzleLevel level)
        {
            var text = new StringBuilder(); text.Append(level.Width).Append('x').Append(level.Height);
            foreach (var arrow in level.Arrows)
            {
                text.Append('|'); foreach (var p in arrow.Points) text.Append(p.x).Append(',').Append(p.y).Append(';');
            }
            return text.ToString();
        }

        /// <summary>Checks all one hundred campaign assets and reports dense coverage and difficulty metrics.</summary>
        [MenuItem("Arrow Swarm/Puzzles/Verify Dense 50-Level Campaigns")]
        public static void ValidateCatalogs()
        {
            foreach (string name in new[] { "ShapesCatalog", "ColorShiftCatalog" })
            {
                var catalog = AssetDatabase.LoadAssetAtPath<PuzzleCatalog>(U.Data + name + ".asset");
                Require(catalog != null && catalog.Count == DenseShapeLibrary.Count, name + ": expected 50 levels");
                var signatures = new HashSet<string>(); var titles = new HashSet<string>();
                for (int i = 0; i < catalog.Count; i++)
                {
                    var level = catalog.Get(i + 1); var solved = AssertLevel(level, i);
                    Require(signatures.Add(GeometrySignature(level)), name + ": repeated layout");
                    Require(titles.Add(level.Title), name + ": repeated title");
                    var graph = DenseDependencyGraph.Analyze(level.Arrows, level.Width, level.Height);
                    int cells = 0; foreach (var arrow in level.Arrows) cells += arrow.Points.Count;
                    Debug.Log($"[ArrowSwarm] {name} {i + 1}: {level.Title}, {cells} covered cells, {level.Arrows.Count} arrows, " +
                        $"{graph.MaximumDepth} dependency depth, {graph.InwardHeads} inward heads, {solved.RequiredTurns}/{level.TurnBudget} turns.");
                }
            }
            Debug.Log("[ArrowSwarm] PASS: 100 unique, fully occupied, independently solvable campaign levels.");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("Dense campaign verification: " + message);
        }
    }
}
