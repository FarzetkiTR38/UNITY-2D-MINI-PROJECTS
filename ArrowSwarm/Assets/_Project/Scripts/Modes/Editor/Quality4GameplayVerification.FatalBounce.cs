namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.IO;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Mob;
    using ArrowSwarm.Puzzle;
    using ArrowSwarm.UI;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using Arrow = ArrowSwarm.Arrow.Arrow;
    using ArrowMovement = ArrowSwarm.Arrow.ArrowMovement;
    using ArrowSpawner = ArrowSwarm.Arrow.ArrowSpawner;

    public static partial class Quality4GameplayVerification
    {
        /// <summary>Exercises fatal blocked shots and their authored result popups in all three native scenes.</summary>
        public static void BeginFatalBounce(string reportName = "nov09-fatal-bounce.txt", string captureFolder = null) =>
            Begin(FatalBounce(captureFolder), reportName);

        private static IEnumerator FatalBounce(string captureFolder)
        {
            yield return DefenseFatalBounce(captureFolder);
            yield return PuzzleFatalBounce(GameModeId.Shapes, captureFolder);
            yield return PuzzleFatalBounce(GameModeId.ColorShift, captureFolder);
        }

        private static IEnumerator DefenseFatalBounce(string captureFolder)
        {
            Status = "Defense: final blocked bounce";
            var game = GameManager.Instance;
            DataManager.Instance.PlayerData.currentLevel = 101;
            DataManager.Instance.PlayerData.highestLevel = 101;
            SceneManager.LoadScene("GameScene");
            yield return (Func<bool>)(() => SceneManager.GetActiveScene().name == "GameScene" &&
                game.CurrentState == GameState.Playing && LevelManager.Instance.CurrentParams.Level == 101 &&
                ArrowSpawner.Instance.RemainingArrows > 0);
            MobSpawner.Instance.StopSpawning();
            for (int i = 0; i < 2; i++)
            {
                var blocked = FindDefenseBlockedArrow();
                blocked.OnPlayerClick();
                yield return (Func<bool>)(() => !blocked.IsBlockedAnimating);
            }
            Require(game.CurrentLives == 1, "Defense: two distinct blocked bounces consume two lives");

            var fatal = FindDefenseBlockedArrow();
            var movement = fatal.GetComponent<ArrowMovement>();
            var popup = UnityEngine.Object.FindFirstObjectByType<GameOverUI>(FindObjectsInactive.Include);
            var group = Field<CanvasGroup>(popup, "_canvasGroup");
            Vector2 home = GridManager.Instance.Origin + (Vector2)fatal.HeadPoint * GridManager.Instance.PointSpacing;
            bool PopupStarted() => popup.gameObject.activeInHierarchy && Field<bool>(popup, "_isShowing");
            int losses = 0;
            bool homeAtLoss = false;
            string lossPose = "no loss callback";
            Action lost = () =>
            {
                losses++;
                float distance = Vector2.Distance(home, fatal.transform.position);
                homeAtLoss = !movement.IsMoving && !fatal.IsBlockedAnimating && distance < .001f;
                lossPose = $"moving={movement.IsMoving}, blocked={fatal.IsBlockedAnimating}, grid error={distance:F5}";
            };
            GameManager.OnLevelLost += lost;
            try
            {
                fatal.OnPlayerClick();
                Check(game.CurrentLives == 0 && movement.IsMoving && losses == 0 && !PopupStarted(),
                    $"Defense: last life is consumed, but Game Over remains closed as the arrow starts " +
                    $"(lives={game.CurrentLives}, moving={movement.IsMoving}, loss events={losses}, popup={PopupStarted()})");
                game.PauseGame();
                Check(Time.timeScale > 0 && game.CurrentState != GameState.Paused,
                    "Defense: Pause cannot stop the fatal return");
                bool returnCaptured = false;
                bool stayedHidden = true;
                float peakDistance = 0;
                while (movement.IsMoving)
                {
                    float displacement = Vector2.Distance(home, fatal.transform.position);
                    stayedHidden &= losses == 0 && !PopupStarted();
                    if (!returnCaptured && displacement > .0001f && displacement < peakDistance - .0001f)
                    {
                        returnCaptured = true;
                        CaptureFatalStage(captureFolder, "Defense-returning");
                    }
                    peakDistance = Mathf.Max(peakDistance, displacement);
                    yield return NextFrames();
                }
                Check(stayedHidden && returnCaptured, $"Defense: Game Over stays closed through the visible outbound and return motion " +
                    $"(hidden={stayedHidden}, reverse={returnCaptured})");
                yield return (Func<bool>)(() => PopupStarted() && group.interactable && group.alpha > .999f);
                Check(losses == 1 && homeAtLoss && game.CurrentState == GameState.Lose,
                    $"Defense: restored resting visuals and cleared movement precede exactly one loss event " +
                    $"(loss events={losses}, state={game.CurrentState}, {lossPose})");
                CaptureFatalStage(captureFolder, "Defense-game-over");
                yield return NextFrames(2);
            }
            finally { GameManager.OnLevelLost -= lost; }
        }

        private static Arrow FindDefenseBlockedArrow()
        {
            var grid = GridManager.Instance;
            Arrow result = null;
            float longest = -1;
            foreach (var arrow in ArrowSpawner.Instance.ActiveArrows)
            {
                if (arrow.IsFired || arrow.IsMarkedBlocked || grid.IsPathClear(arrow.HeadPoint, arrow.HeadDirection)) continue;
                float distance = Vector2.Distance(arrow.transform.position,
                    grid.GetCollisionPoint(arrow.HeadPoint, arrow.HeadDirection, out _));
                if (distance <= longest) continue;
                result = arrow;
                longest = distance;
            }
            Require(result != null, "Defense: generated board has an unused blocked arrow");
            return result;
        }

        private static IEnumerator PuzzleFatalBounce(GameModeId mode, string captureFolder)
        {
            Status = mode + ": final blocked bounce";
            yield return OpenPuzzle(mode);
            var session = PuzzleSession.Active;
            var removed = Field<bool[]>(session, "_removed");
            var views = Field<PuzzleArrowView[]>(session, "_views");
            int index = FindPuzzleBlockedArrow(session, removed);
            var view = views[index];
            var hud = UnityEngine.Object.FindFirstObjectByType<PuzzleHUD>();
            var pause = Field<Button>(hud, "_pause");
            var popups = UnityEngine.Object.FindFirstObjectByType<PuzzlePopups>();
            var loss = Field<CanvasGroup[]>(popups, "_panels")[2];
            for (int i = 0; i < 2; i++)
            {
                session.Fire(index);
                yield return (Func<bool>)(() => !session.HasActiveShots);
            }
            Require(session.Lives == 1, mode + ": two blocked bounces leave one life");

            session.Fire(index);
            bool sawOutbound = false, checkedPause = false, sawReturn = false, stayedHidden = true;
            float peak = 0f;
            while (session.HasActiveShots)
            {
                float distance = Field<float>(view, "_travel");
                stayedHidden &= !session.Lost && !loss.blocksRaycasts;
                sawOutbound |= session.Lives == 1 && distance > 0f;
                if (session.Lives == 0 && !checkedPause)
                {
                    checkedPause = true;
                    Check(!session.Ready && !pause.IsInteractable(), mode + ": gameplay and Pause controls lock on final impact");
                    session.Pause(true);
                    Check(!session.Paused && Time.timeScale > 0, mode + ": direct Pause cannot interrupt the fatal bounce");
                    if (session.Paused) session.Pause(false);
                }
                if (distance > 0f && distance < peak - .0001f && !sawReturn)
                {
                    sawReturn = true;
                    CaptureFatalStage(captureFolder, mode + "-returning");
                }
                peak = Mathf.Max(peak, distance);
                yield return NextFrames();
            }
            Check(sawOutbound && checkedPause && sawReturn && stayedHidden, mode + ": outbound, impact and reverse occur with no result overlay");
            Check(session.Lives == 0 && session.Lost && !session.Won && Field<float>(view, "_travel") == 0f,
                mode + ": only a fully settled arrow permits loss");
            yield return (Func<bool>)(() => loss.interactable && loss.alpha > .999f);
            CaptureFatalStage(captureFolder, mode + "-game-over");
            yield return NextFrames(2);
        }

        private static int FindPuzzleBlockedArrow(PuzzleSession session, bool[] removed)
        {
            var level = session.Level;
            int chosen = -1;
            float longest = 0f;
            for (int i = 0; i < removed.Length; i++)
            {
                if (PuzzleRules.CanRemove(level, removed, i, session.Orientation)) continue;
                var arrow = level.Arrows[i];
                var dir = arrow.Direction;
                var head = arrow.Head;
                float wall = dir.x > 0 ? level.Width - 1 - head.x : dir.x < 0 ? head.x :
                    dir.y > 0 ? level.Height - 1 - head.y : head.y;
                float travel = Mathf.Min(wall + session.Config.WallOffset, PuzzleRules.ObstacleDistance(level, removed, i));
                if (travel <= longest) continue;
                longest = travel;
                chosen = i;
            }
            Require(chosen >= 0, session.Mode + ": the real board has a blocked shot");
            return chosen;
        }

        private static void CaptureFatalStage(string folder, string name)
        {
            if (string.IsNullOrEmpty(folder)) return;
            Directory.CreateDirectory(folder);
            Canvas.ForceUpdateCanvases();
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folder, name + ".png"));
        }
    }
}
