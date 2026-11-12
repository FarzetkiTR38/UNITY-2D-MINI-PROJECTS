namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Puzzle;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using DebugManager = ArrowSwarm.Debug.DebugManager;

    /// <summary>Enters a selected unlocked tile through real input; never completes a level or grants a reward.</summary>
    public static partial class UiRollout01Verification
    {
        private static IEnumerator Verify04LevelRoutes()
        {
            var player = DataManager.Instance.PlayerData; string original = JsonUtility.ToJson(player);
            var debug = DebugManager.Instance; bool useDebug = debug.UseDebugLevel;
            Action<int> loading = null;
            try
            {
                // Confirm the selected Defense level, rather than the outer fixture's level101 override.
                Set03(debug, "_useDebugLevel", false);
                foreach (var mode in new[] { GameModeId.Defense, GameModeId.Shapes })
                {
                    int highest = mode == GameModeId.Defense ? 102 : 3, target = mode == GameModeId.Defense ? 102 : 2;
                    Apply04Progress(player, mode, highest, mode == GameModeId.Defense ? 101 : 1);
                    DataManager.Instance.SelectMode(mode);
                    var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
                    int totalStars = player.GetTotalStars(); string color = JsonUtility.ToJson(player.colorShiftProgress);
                    string other = Other04Progress(player, mode);
                    int loaded = -1; loading = number => loaded = number; LevelManager.OnLevelLoading += loading;
                    Status = "Native level-tile route / " + mode + " / " + target;
                    using (var mouse = new NativeMouse())
                    {
                        yield return Click(mouse, Field<Button>(menu, "_levelsButton"), true); yield return Settled(.65f);
                        var levels = UnityEngine.Object.FindFirstObjectByType<LevelSelectUI>();
                        Check(Field<GameObject>(menu, "_activeSubPanel") == levels.gameObject,
                            "Level tile route retains MainMenu panel ownership / " + mode);
                        var tile = Field<List<LevelButtonUI>>(levels, "_buttonPool").Single(item => Field<int>(item, "_level") == target);
                        yield return Click(mouse, Field<Button>(tile, "_button"), true);
                    }
                    string scene = mode == GameModeId.Defense ? "GameScene" : "ShapesScene";
                    yield return (Func<bool>)(() => SceneManager.GetActiveScene().name == scene && !SceneTransitionManager.Instance.IsTransitioning &&
                        GameManager.Instance.CurrentState == GameState.Playing && (mode == GameModeId.Defense ?
                        LevelManager.HasInstance && LevelManager.Instance.CurrentLevelData.IsValid : PuzzleSession.Active != null && PuzzleSession.Active.Ready));
                    if (ArrowSwarm.Mob.MobSpawner.HasInstance) ArrowSwarm.Mob.MobSpawner.Instance.StopSpawning();
                    Check(DataManager.Instance.GetCurrentLevel(mode) == target && DataManager.Instance.GetHighestLevel(mode) == highest &&
                        player.GetTotalStars() == totalStars && JsonUtility.ToJson(player.colorShiftProgress) == color && Other04Progress(player, mode) == other,
                        "Actual level tile changes only its selected campaign/current level, with no stars or Color Shift progress change / " + mode);
                    Check(mode == GameModeId.Defense ? loaded == target : !PuzzleSession.Active.Preview && PuzzleSession.Active.Mode == mode && PuzzleSession.Active.LevelNumber == target,
                        "Actual unlocked tile starts exactly the requested non-preview level / " + mode + " / " + target);
                    LevelManager.OnLevelLoading -= loading; loading = null;
                    ReturnToMenu(); yield return Settled(.65f);
                }
            }
            finally
            {
                if (loading != null) LevelManager.OnLevelLoading -= loading;
                Set03(debug, "_useDebugLevel", useDebug);
                ReturnToMenu(); JsonUtility.FromJsonOverwrite(original, player);
            }
            yield return Settled(.65f);
            Check(JsonUtility.ToJson(player) == original, "Real level routes restore the exact outer cloned player after leaving gameplay");
        }

        private static string Other04Progress(PlayerData player, GameModeId mode) => mode == GameModeId.Defense
            ? JsonUtility.ToJson(player.shapesProgress)
            : player.currentLevel + "|" + player.highestLevel + "|" + string.Join(",", player.levelStars.Select(item => item.level + ":" + item.stars));
    }
}
