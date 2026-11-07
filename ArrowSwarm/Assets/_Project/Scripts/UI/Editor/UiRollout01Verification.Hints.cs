namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using System.IO;
    using ArrowSwarm.Ads;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Puzzle;
    using ArrowSwarm.Tips;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>Combines reversible hint ownership checks with real last-charge HUD input in all three campaigns.</summary>
    public static partial class UiRollout01Verification
    {
        private static bool _nextHintStage, _hintStage;

        /// <summary>Starts from fresh Boot-loaded MainMenu and writes each domain result into the chosen evidence folder.</summary>
        public static void HintAt(string folder)
        {
            if (!AdManager.HasInstance) throw new InvalidOperationException("Resolve the Boot AdManager before starting the scoped Hint fixture.");
            _nextHintStage = true;
            try { Start(folder, false); } finally { _nextHintStage = false; }
        }

        private static IEnumerator VerifyHintStage()
        {
            foreach (var mode in new[] { GameModeId.Defense, GameModeId.Shapes, GameModeId.ColorShift })
            {
                Status = "Hint ownership / " + mode;
                DataManager.Instance.SelectMode(mode); DataManager.Instance.SetCurrentLevel(mode, mode == GameModeId.Defense ? 101 : 1);
                yield return Settled();
                var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
                using (var pointer = new NativeMouse()) yield return Click(pointer, Field<Button>(menu, "_playButton"), true);
                string scene = mode == GameModeId.Defense ? "GameScene" : mode == GameModeId.Shapes ? "ShapesScene" : "ColorShiftScene";
                yield return (Func<bool>)(() => SceneManager.GetActiveScene().name == scene &&
                    !SceneTransitionManager.Instance.IsTransitioning && (mode == GameModeId.Defense ?
                    HintDefenseReady() : PuzzleSession.Active != null && PuzzleSession.Active.Idle));
                Check(SceneManager.GetActiveScene().name == scene, "Actual native Play reaches the chosen hint campaign / " + mode);
                if (mode == GameModeId.Defense)
                {
                    WriteHintReport("defense-hints", HintGuardVerification.VerifyDefense());
                    yield return (Func<bool>)HintDefenseReady;
                    if (ArrowSwarm.Mob.MobSpawner.HasInstance) ArrowSwarm.Mob.MobSpawner.Instance.StopSpawning();
                }
                else
                {
                    WriteHintReport(mode.ToString().ToLowerInvariant() + "-hints", HintGuardVerification.VerifyPuzzle());
                    yield return (Func<bool>)(() => PuzzleSession.Active != null && PuzzleSession.Active.Idle);
                    if (mode == GameModeId.ColorShift)
                    {
                        WriteHintReport("colorshift-rotation-hint", HintGuardVerification.VerifyRotationHint());
                        yield return (Func<bool>)(() => PuzzleSession.Active != null && PuzzleSession.Active.Idle);
                    }
                }
                yield return Settled(.25f);
                using (var pointer = new NativeMouse()) yield return VerifyNativeLastCharge(pointer, mode);
                Time.timeScale = 1;
                if (mode == GameModeId.Defense) GameManager.Instance.GoToMainMenu();
                else PuzzleSession.Active.Menu();
                yield return (Func<bool>)(() => SceneManager.GetActiveScene().name == "MainMenuScene" &&
                    !SceneTransitionManager.Instance.IsTransitioning && UnityEngine.Object.FindFirstObjectByType<MainMenuUI>() != null);
                yield return Settled(.15f);
            }
            Check(_scope.Ads.Calls == 0 && _scope.Cloud.Operations == 0,
                "All three native hint campaigns leave the isolated ad/cloud operation counters at zero");
        }

        private static void WriteHintReport(string name, string json)
        {
            Check(!string.IsNullOrWhiteSpace(json) && json.Contains("\"PASS\""), "Live hint ownership helper passed / " + name);
            File.WriteAllText(Path.Combine(_folder, name + ".json"), json);
        }

        private static bool HintDefenseReady()
        {
            if (!LevelManager.HasInstance || !TipManager.HasInstance || !ArrowSwarm.Arrow.ArrowSpawner.HasInstance ||
                GameManager.Instance.CurrentState != GameState.Playing || LevelManager.Instance.CurrentParams.Level != 101) return false;
            var arrows = ArrowSwarm.Arrow.ArrowSpawner.Instance.ActiveArrows;
            if (arrows.Count == 0 || arrows.Count != LevelManager.Instance.TotalArrows) return false;
            foreach (var arrow in arrows)
            {
                if (arrow == null || !arrow.gameObject.activeInHierarchy || arrow.IsFired) return false;
                var visuals = arrow.GetComponent<ArrowSwarm.Arrow.ArrowVisuals>();
                var body = arrow.GetComponent<LineRenderer>();
                if (visuals == null || visuals.IsSpawning || body == null || !body.enabled || body.positionCount < 2) return false;
            }
            return UnityEngine.Object.FindFirstObjectByType<GameHUD>() != null;
        }
    }
}
