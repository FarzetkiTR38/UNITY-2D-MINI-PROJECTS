namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using ArrowSwarm.Data;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Focused in-memory regressions; never mutates the player's real save.</summary>
    public static class PuzzleVerification
    {
        [MenuItem("Arrow Swarm/Puzzles/Verify Rules and Save Compatibility")]
        public static void Run()
        {
            var p = JsonUtility.FromJson<PlayerData>("{\"currentLevel\":41,\"highestLevel\":42,\"levelStars\":[{\"level\":1,\"stars\":2}]}");
            p.GetModeProgress(GameModeId.Shapes).Complete(1, 3, 5);
            p.GetModeProgress(GameModeId.Shapes).Complete(1, 1, 5);
            p.GetModeProgress(GameModeId.ColorShift).Complete(1, 2, 5);
            var q = JsonUtility.FromJson<PlayerData>(JsonUtility.ToJson(p));
            Require(q.currentLevel == 41 && q.highestLevel == 42 && q.GetTotalStars() == 7, "old save, best stars and round trip");
            Require(q.GetModeProgress(GameModeId.Shapes).HighestLevel == 2 && q.GetModeProgress(GameModeId.ColorShift).HighestLevel == 2, "separate unlocks");
            var level = ScriptableObject.CreateInstance<PuzzleLevel>();
            try
            {
                var arrows = new List<PuzzleArrowData> { Make(1, 2, Vector2Int.up, 1) };
                level.Author(GameModeId.ColorShift, "Test", 5, 5, arrows, null, 1);
                var solved = PuzzleSolver.Solve(level);
                Require(solved.Solvable && solved.RequiredTurns == 1, "minimum rotation witness");
                Require(!PuzzleSolver.Solve(level, null, 0, 0).Solvable, "zero-turn wall deadlock");
                Require(PuzzleSolver.Solve(level, null, 1, 0).Solvable, "zero-turn matching exit");
                arrows.Add(Make(1, 4, Vector2Int.up, 0));
                Require(!PuzzleRules.CanRemove(level, new bool[2], 0, 1), "arrow collision precedes wall");
                Require(PuzzleRules.CanRemove(level, new[] { false, true }, 0, 1), "removal releases obstruction");
                arrows.Clear();
                arrows.Add(Make(1, 1, Vector2Int.right, 0));
                arrows.Add(Make(3, 1, Vector2Int.left, 1));
                Require(!PuzzleSolver.Solve(level).Solvable, "facing deadlock rejected");
                arrows.Add(Make(1, 1, Vector2Int.up, 0));
                Require(PuzzleRules.Validate(level) != null, "overlap rejected");
            }
            finally { UnityEngine.Object.DestroyImmediate(level); }
            Debug.Log("[ArrowSwarm] PASS: save compatibility, independent progress, best-star total, wall orientations, turns, obstruction release, facing deadlock and overlap. PlayerPrefs unchanged.");
        }

        private static PuzzleArrowData Make(int x, int y, Vector2Int direction, int color) =>
            new PuzzleArrowData(new List<Vector2Int> { new Vector2Int(x, y), new Vector2Int(x, y) - direction }, direction, Color.cyan, color);
        private static void Require(bool value, string name) { if (!value) throw new Exception("[ArrowSwarm] FAILED: " + name); }

        /// <summary>Validates every project puzzle and logs its witness length and minimum turns.</summary>
        [MenuItem("Arrow Swarm/Puzzles/Validate All Levels")]
        public static void ValidateAll()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:PuzzleLevel"))
            {
                var level = AssetDatabase.LoadAssetAtPath<PuzzleLevel>(AssetDatabase.GUIDToAssetPath(guid));
                var solution = PuzzleSolver.Solve(level);
                Require(solution.Solvable, level.name + ": " + solution.Error);
                Debug.Log($"[ArrowSwarm] {level.name}: {level.Arrows.Count} arrows, {solution.RequiredTurns}/{level.TurnBudget} turns.");
            }
        }
    }
}
