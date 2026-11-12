namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Puzzle;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>Proves that the restyled Play button still enters each actual campaign without completing a level.</summary>
    public static partial class UiRollout01Verification
    {
        private static IEnumerator VerifyPlayRoutes()
        {
            foreach (var mode in new[] { GameModeId.Defense, GameModeId.Shapes, GameModeId.ColorShift })
            {
                Status = "Native Play route / " + mode;
                DataManager.Instance.SelectMode(mode); DataManager.Instance.SetCurrentLevel(mode, 1); yield return Settled();
                var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
                string scene = mode == GameModeId.Defense ? "GameScene" : mode == GameModeId.Shapes ? "ShapesScene" : "ColorShiftScene";
                using (var mouse = new NativeMouse()) yield return Click(mouse, Field<Button>(menu, "_playButton"), true);
                yield return (Func<bool>)(() => SceneManager.GetActiveScene().name == scene && !SceneTransitionManager.Instance.IsTransitioning &&
                    GameManager.Instance.CurrentState == GameState.Playing && (mode == GameModeId.Defense ?
                    LevelManager.HasInstance && LevelManager.Instance.CurrentLevelData.IsValid : PuzzleSession.Active != null && PuzzleSession.Active.Ready));
                if (ArrowSwarm.Mob.MobSpawner.HasInstance) ArrowSwarm.Mob.MobSpawner.Instance.StopSpawning();
                Check(SceneManager.GetActiveScene().name == scene, "Real Play pointer input reaches the existing " + scene);
                if (mode != GameModeId.Defense) Check(!PuzzleSession.Active.Preview && PuzzleSession.Active.Mode == mode && PuzzleSession.Active.LevelNumber == 1,
                    "Play starts the chosen real campaign, without replacing it with a preview");
                ReturnToMenu(); yield return Settled();
            }
        }

        private static void ReturnToMenu()
        {
            if (!Application.isPlaying) return;
            if (ArrowSwarm.Skills.FreezeManager.HasInstance) ArrowSwarm.Skills.FreezeManager.Instance.EndFreezeSilently();
            if (LevelManager.HasInstance) LevelManager.Instance.CleanupLevel();
            var transition = SceneTransitionManager.Instance;
            if (transition.IsTransitioning) transition.StopAllCoroutines();
            typeof(SceneTransitionManager).GetField("_isTransitioning", Private).SetValue(transition, false);
            var image = Field<Image>(transition, "_overlayImage");
            if (image != null)
            {
                if (image.material != null && image.material.HasProperty("_Progress")) image.material.SetFloat("_Progress", 0);
                image.raycastTarget = false; image.gameObject.SetActive(false);
            }
            Time.timeScale = 1; GameManager.Instance.SetState(GameState.Menu);
            if (SceneManager.GetActiveScene().name != "MainMenuScene") SceneManager.LoadScene("MainMenuScene");
        }
    }
}
