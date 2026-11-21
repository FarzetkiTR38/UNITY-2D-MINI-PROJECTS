namespace ArrowSwarm.Marketing.Editor
{
    using System;
    using System.Collections;
    using System.IO;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Puzzle;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using Arrow = ArrowSwarm.Arrow.Arrow;

    /// <summary>Production actions and rendered frames for missing marketing source witnesses.</summary>
    public static partial class MarketingCapture
    {
        private static IEnumerator RecordBacklogWitness(string kind)
        {
            _scope = new CaptureScope();
            bool defense = kind == "Wrong" || kind == "Tutorial";
            int seed = _defenseSeed, reserved = _defenseReservedArrow;
            bool ready = false;
            try
            {
                ArrowSwarm.Localization.LocalizationManager.Instance.SetLanguage("tr", false);
                PuzzleLaunch.Preview = kind == "Match"; PuzzleLaunch.PreviewMode = GameModeId.ColorShift; PuzzleLaunch.PreviewLevel = 1;
                if (defense)
                {
                    PrepareDefenseDebug(); _defenseSeed = 20261117; _defenseReservedArrow = -1;
                    DataManager.Instance.PlayerData.currentLevel = kind == "Tutorial" ? 1 : 3;
                    DataManager.Instance.PlayerData.isTutorialCompleted = kind != "Tutorial";
                    DataManager.Instance.PlayerData.selectedGameMode = GameModeId.Defense;
                    LevelManager.OnLevelLoading += SeedDefenseLevel;
                }
                SceneManager.LoadScene(kind == "Menu" ? "MainMenuScene" : defense ? "GameScene" : "ColorShiftScene");
                yield return null; yield return null; yield return StoreWait(60);
                if (Screen.width != 1080 || Screen.height != 1920) throw new InvalidOperationException("Use native1080x1920.");
                if (defense)
                {
                    StoreFrameDefense(); ExportDefenseGeometry(DataManager.Instance.PlayerData.currentLevel);
                    // Keep the existing HUD visible in this recording, including the native first-run tutorial.
                    UnityEngine.Object.FindFirstObjectByType<ArrowSwarm.UI.GameHUD>().SetBarsVisible(true, true);
                }
                var puzzle = kind == "Match" ? PuzzleSession.Active : null;
                if (puzzle != null) ExportGeometry(puzzle, PuzzleSolver.Solve(puzzle.Level));
                int totalFrames = kind == "Menu" ? 600 : kind == "Match" ? 600 : kind == "Wrong" ? 450 : 750;
                int shots = 0, returns = 0, nextShot = 60, matchStep = 0;
                Arrow blocked = null; bool awaiting = false, impact = false;
                Time.captureFramerate = 30; var end = new WaitForEndOfFrame();
                for (_frame = 0; _frame < totalFrames; _frame++)
                {
                    if (kind == "Menu") BacklogMenuAction(_frame);
                    else if (kind == "Match") BacklogMatchAction(puzzle, ref shots, ref returns, ref awaiting, ref nextShot, ref matchStep);
                    else if (kind == "Tutorial" && (_frame == 90 || _frame == 240))
                    {
                        var tutorial = ArrowSwarm.Tutorial.TutorialManager.Instance;
                        if (tutorial == null || !tutorial.IsTutorialActive) throw new InvalidOperationException("Genuine tutorial required.");
                        foreach (var arrow in ArrowSwarm.Arrow.ArrowSpawner.Instance.ActiveArrows)
                            if (!arrow.IsFired && arrow.HeadPoint == tutorial.CurrentTargetGridPos)
                            { arrow.OnPlayerClick(); shots++; BacklogEvent("fire", DefenseArrowIndex(arrow), null); break; }
                    }
                    else if (kind == "Wrong")
                    {
                        if (awaiting && blocked != null)
                        {
                            var movement = blocked.GetComponent<ArrowSwarm.Arrow.ArrowMovement>();
                            var state = movement == null ? "" : typeof(ArrowSwarm.Arrow.ArrowMovement).GetField("_blockedState", Private).GetValue(movement).ToString();
                            if (!impact && state == "ImpactShake") { BacklogEvent("collision", DefenseArrowIndex(blocked), null); impact = true; }
                            if (!blocked.IsBlockedAnimating) { returns++; awaiting = false; nextShot = _frame + 35; BacklogEvent("returned", DefenseArrowIndex(blocked), null); }
                        }
                        if (_frame >= nextShot && shots < 3 && !awaiting && GameManager.Instance.CurrentState == GameState.Playing)
                        {
                            blocked = BacklogBlockedDefense();
                            if (blocked == null) throw new InvalidOperationException("Three distinct blocked arrows required.");
                            blocked.OnPlayerClick(); shots++; awaiting = true; impact = false; BacklogEvent("wrong_fire", DefenseArrowIndex(blocked), null);
                        }
                    }
                    yield return end;
                    var image = ScreenCapture.CaptureScreenshotAsTexture();
                    try
                    {
                        if (image.width != 1080 || image.height != 1920) throw new InvalidOperationException("Native dimensions changed.");
                        File.WriteAllBytes(_folder + "/frame_" + _frame.ToString("D5") + ".jpg", image.EncodeToJPG(92));
                        if (_frame == 0) File.WriteAllBytes(_folder + "/source.png", image.EncodeToPNG());
                    }
                    finally { UnityEngine.Object.Destroy(image); }
                    if (_frame % 30 == 0) Status = "Recording backlog " + kind + " " + _frame + "/" + totalFrames;
                }
                if (kind == "Wrong" && (shots != 3 || returns != 3 || GameManager.Instance.CurrentLives != 0))
                    throw new InvalidOperationException("Three genuine bounced wrong shots and natural loss required.");
                if (kind == "Tutorial" && shots < 1) throw new InvalidOperationException("Native tutorial shot missing.");
                if (kind == "Match" && (returns != 1 || shots < 2)) throw new InvalidOperationException("Mismatch return and matching exit required.");
                if (kind == "Wrong") BacklogEvent("lost", -1, null);
                File.WriteAllLines(_folder + "/events.csv", Events);
                File.WriteAllText(_folder + "/provenance.json", JsonUtility.ToJson(new BacklogProof { scenario = kind,
                    scene = SceneManager.GetActiveScene().path, mode = kind == "Menu" ? "Menu" : defense ? "Defense" : "ColorShift",
                    level = kind == "Menu" ? 0 : kind == "Wrong" ? 3 : 1, frames = totalFrames, fps = 30,
                    width = Screen.width, height = Screen.height, shots = shots, returnedShots = returns, forcedOutcome = false }, true));
                SceneManager.LoadScene("MainMenuScene"); yield return null; ready = true; Status = "PASS: " + _folder;
            }
            finally
            {
                if (defense) { LevelManager.OnLevelLoading -= SeedDefenseLevel; RestoreDefenseDebug(); }
                _defenseSeed = seed; _defenseReservedArrow = reserved;
                if (!ready && Application.isPlaying) { SceneManager.LoadScene("MainMenuScene"); }
            }
        }
        private static void BacklogEvent(string kind, int arrow, PuzzleSession puzzle)
        {
            int remaining = puzzle != null ? puzzle.Remaining : ArrowSwarm.Arrow.ArrowSpawner.HasInstance ? ArrowSwarm.Arrow.ArrowSpawner.Instance.RemainingArrows : 0;
            int lives = puzzle != null ? puzzle.Lives : GameManager.Instance.CurrentLives;
            Events.Add(_frame + "," + (_frame / 30f).ToString("F5", System.Globalization.CultureInfo.InvariantCulture) + "," + kind + "," + arrow + "," + remaining + "," + lives);
        }
        [Serializable] private sealed class BacklogProof
        {
            public string scenario, scene, mode, language = "tr";
            public int level, frames, fps, width, height, shots, returnedShots;
            public bool forcedOutcome;
        }
    }
}
