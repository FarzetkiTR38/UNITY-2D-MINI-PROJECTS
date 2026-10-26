namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Mob;
    using ArrowSwarm.Path;
    using ArrowSwarm.UI;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using Arrow = ArrowSwarm.Arrow.Arrow;
    using ArrowMovement = ArrowSwarm.Arrow.ArrowMovement;
    using ArrowSpawner = ArrowSwarm.Arrow.ArrowSpawner;

    public static partial class Quality4GameplayVerification
    {
        private static IEnumerator Defense()
        {
            yield return DefenseRound(true);
            yield return DefenseRound(false);
            GameManager.Instance.GoToMainMenu(); yield return (Func<bool>)MenuReady;
            int menuWins = 0; Action onMenuWin = () => menuWins++;
            GameManager.OnLevelWon += onMenuWin;
            try
            {
                GameManager.Instance.HandleAllArrowsFired(); GameManager.Instance.HandleAllArrowsFired();
                Check(GameManager.Instance.CurrentState == GameState.Menu && menuWins == 0,
                    "Public completion callbacks cannot replace the actual Menu state or publish a win");
            }
            finally { GameManager.OnLevelWon -= onMenuWin; }
        }

        private static IEnumerator DefenseRound(bool forceLoss)
        {
            string label = forceLoss ? "last-flight final-life loss" : "normal successful final flight";
            Status = "Defense101: " + label;
            DataManager.Instance.PlayerData.currentLevel = 101; DataManager.Instance.PlayerData.highestLevel = 101;
            SceneManager.LoadScene("GameScene");
            yield return (Func<bool>)(() => SceneManager.GetActiveScene().name == "GameScene" &&
                GameManager.Instance.CurrentState == GameState.Playing && LevelManager.HasInstance &&
                LevelManager.Instance.CurrentParams.Level == 101 && ArrowSpawner.Instance.RemainingArrows > 0);
            yield return (Func<bool>)(() => MobSpawner.Instance.ActiveMobs.Count > 0);
            var spawner = MobSpawner.Instance; spawner.StopSpawning();
            Mob target = null;
            foreach (var mob in spawner.ActiveMobs)
            {
                if (mob == null || !mob.IsAlive || !mob.gameObject.activeInHierarchy) continue;
                mob.GetComponent<MobMovement>().SetFrozen(true); target = mob;
            }
            Require(target != null && GameManager.Instance.CurrentLives == 3, label + ": real living mob and three lives");
            int lost = 0, won = 0, completed = 0, mobExits = 0;
            Arrow last = null; bool lossHadFlight = false; int lossLives = -1; GameState lossState = GameState.Loading;
            Action onLost = () =>
            {
                lost++; lossLives = GameManager.Instance.CurrentLives; lossState = GameManager.Instance.CurrentState;
                lossHadFlight = last != null && last.GetComponent<ArrowMovement>().IsMoving;
            };
            Action onWon = () => won++;
            Action<Arrow> onCompleted = arrow => completed++;
            Action onMobExit = () => mobExits++;
            GameManager.OnLevelLost += onLost; GameManager.OnLevelWon += onWon;
            Arrow.OnArrowCompleted += onCompleted; GameManager.OnMobReachedFinish += onMobExit;
            try
            {
                if (forceLoss)
                {
                    var bounced = new List<Arrow>();
                    for (int penalty = 0; penalty < 2; penalty++)
                    {
                        Arrow blocked = null;
                        foreach (var arrow in ArrowSpawner.Instance.ActiveArrows)
                            if (!arrow.IsFired && !arrow.IsMarkedBlocked && !arrow.IsBlockedAnimating &&
                                !GridManager.Instance.IsPathClear(arrow.HeadPoint, arrow.HeadDirection)) { blocked = arrow; break; }
                        Require(blocked != null, "Real generated Defense board has another previously unmarked blocked arrow");
                        blocked.OnPlayerClick(); bounced.Add(blocked);
                        Require(blocked.IsBlockedAnimating, "Blocked production click starts its normal bounce");
                    }
                    Require(GameManager.Instance.CurrentLives == 1, "Two real blocked-arrow clicks leave exactly one life");
                    yield return (Func<bool>)(() => !bounced[0].IsBlockedAnimating && !bounced[1].IsBlockedAnimating);
                }
                int highest = DataManager.Instance.PlayerData.highestLevel;
                int stars = DataManager.Instance.GetLevelStars(101);
                var arrows = new List<Arrow>(ArrowSpawner.Instance.ActiveArrows);
                last = FireLegalBoard(arrows);
                Require(last != null && last.GetComponent<ArrowMovement>().IsMoving && completed == 0 &&
                    ArrowSpawner.Instance.RemainingArrows == 0, "All real pooled arrows launched; final arrow is outstanding before any completion");
                if (forceLoss)
                {
                    var movement = target.GetComponent<MobMovement>();
                    movement.SetDistance(PathManager.Instance.TotalPathLength);
                    movement.SetSpeed(1f); movement.SetFrozen(false);
                    yield return (Func<bool>)(() => lost == 1);
                    Require(mobExits == 1 && lossLives == 0 && lossState == GameState.Lose,
                        "Actual PathFollower end -> MobMovement -> Mob -> GameManager consumes the final life and emits Lose");
                    Require(lossHadFlight, "Actual loss event captures the last real arrow still in flight");
                    var losePanel = UnityEngine.Object.FindFirstObjectByType<GameOverUI>(FindObjectsInactive.Include);
                    Require(losePanel != null, "The authored Game Over overlay exists");
                    var loseGroup = losePanel.GetComponent<CanvasGroup>();
                    yield return (Func<bool>)(() => losePanel.gameObject.activeInHierarchy && loseGroup.alpha > .999f && loseGroup.interactable);
                    Require(true, "Actual loss callback completes the authored Game Over entrance animation");
                }
                yield return (Func<bool>)(() => completed == arrows.Count);
                yield return NextFrames(2);
                if (forceLoss)
                {
                    Check(GameManager.Instance.CurrentState == GameState.Lose && lost == 1 && won == 0,
                        "Late real arrow completions preserve Lose and never publish Win");
                    var winPanel = UnityEngine.Object.FindFirstObjectByType<LevelCompleteUI>(FindObjectsInactive.Include);
                    Check(winPanel != null && (!winPanel.gameObject.activeInHierarchy || !winPanel.GetComponent<CanvasGroup>().blocksRaycasts),
                        "Late flight does not open the success overlay on top of Game Over");
                    Check(DataManager.Instance.PlayerData.highestLevel == highest && DataManager.Instance.GetLevelStars(101) == stars,
                        "A lost Defense board cannot unlock another level or change its stars");
                    GameManager.Instance.HandleAllArrowsFired(); GameManager.Instance.HandleAllArrowsFired();
                    Check(GameManager.Instance.CurrentState == GameState.Lose && lost == 1 && won == 0,
                        "Repeated public completion callbacks preserve an actual Lose without another terminal event");
                }
                else
                {
                    Check(GameManager.Instance.CurrentState == GameState.Win && won == 1 && lost == 0,
                        "Positive control: real legal last flight still wins exactly once");
                    Check(DataManager.Instance.GetLevelStars(101) == 3,
                        "Positive control: successful three-life Defense flight retains its normal three-star result");
                    GameManager.Instance.HandleAllArrowsFired(); GameManager.Instance.HandleAllArrowsFired();
                    Check(GameManager.Instance.CurrentState == GameState.Win && won == 1 && lost == 0,
                        "Repeated public completion callbacks preserve an actual Win without publishing or rewarding it again");
                }
                Results.Add("OBSERVED: " + label + "; completed=" + completed + "; exits=" + mobExits +
                    "; losses=" + lost + "; wins=" + won + "; terminal=" + GameManager.Instance.CurrentState);
            }
            finally
            {
                GameManager.OnLevelLost -= onLost; GameManager.OnLevelWon -= onWon;
                Arrow.OnArrowCompleted -= onCompleted; GameManager.OnMobReachedFinish -= onMobExit;
            }
        }

        private static Arrow FireLegalBoard(List<Arrow> arrows)
        {
            Arrow last = null; int remaining = arrows.Count;
            while (remaining > 0)
            {
                bool progress = false;
                foreach (var arrow in arrows)
                {
                    if (arrow.IsFired || arrow.IsBlockedAnimating || !GridManager.Instance.IsPathClear(arrow.HeadPoint, arrow.HeadDirection)) continue;
                    arrow.OnPlayerClick(); Require(arrow.IsFired, "Legal production click launches a pooled Defense arrow");
                    last = arrow; remaining--; progress = true;
                }
                Require(progress, "Generated board retains a legal clearing sequence");
            }
            return last;
        }
    }
}
