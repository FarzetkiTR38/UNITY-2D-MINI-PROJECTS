namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using Object = UnityEngine.Object;

    /// <summary>Checks native Defense popup transitions and authored close controls without requesting ads.</summary>
    public static partial class AudioRevisionVerification
    {
        private static IEnumerator Popups()
        {
            var player = DataManager.Instance.PlayerData;
            player.isTutorialCompleted = player.isProfileSetupCompleted = true;
            player.currentLevel = player.highestLevel = 101;
            float originalTimeScale = Time.timeScale;
            bool generated = false;
            Action<LevelParams> ready = _ => generated = true;
            LevelManager.OnLevelReady += ready;
            try
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("GameScene");
                float deadline = Time.realtimeSinceStartup + 20f;
                bool stable = false;
                while (Time.realtimeSinceStartup < deadline)
                {
                    if (ArrowSwarm.Mob.MobSpawner.HasInstance)
                        ArrowSwarm.Mob.MobSpawner.Instance.StopSpawning();
                    foreach (var mob in Object.FindObjectsByType<ArrowSwarm.Mob.Mob>(FindObjectsSortMode.None))
                        mob.Movement?.StopMoving();
                    stable = generated && GameManager.HasInstance && GameManager.Instance.CurrentState == GameState.Playing &&
                        LevelManager.HasInstance && LevelManager.Instance.TotalArrows > 0 && ArrowSwarm.Arrow.ArrowSpawner.HasInstance &&
                        ArrowSwarm.Arrow.ArrowSpawner.Instance.ActiveArrows.Count == LevelManager.Instance.TotalArrows;
                    if (stable)
                    {
                        foreach (var arrow in ArrowSwarm.Arrow.ArrowSpawner.Instance.ActiveArrows)
                        {
                            var visuals = arrow != null ? arrow.GetComponent<ArrowSwarm.Arrow.ArrowVisuals>() : null;
                            if (visuals == null || visuals.IsSpawning || !arrow.gameObject.activeInHierarchy)
                            { stable = false; break; }
                        }
                    }
                    if (stable) break;
                    yield return null;
                }
                Check(stable, "Fresh actual Defense level is generated with all native arrows ready");
                yield return Settled(.1f);
                var win = Object.FindFirstObjectByType<LevelCompleteUI>(FindObjectsInactive.Include);
                var lose = Object.FindFirstObjectByType<GameOverUI>(FindObjectsInactive.Include);
                var pause = Object.FindFirstObjectByType<PauseMenuUI>(FindObjectsInactive.Include);
                var tip = Object.FindFirstObjectByType<TipPopupUI>(FindObjectsInactive.Include);
                var freeze = Object.FindFirstObjectByType<FreezePopupUI>(FindObjectsInactive.Include);
                Check(win != null && lose != null && pause != null && tip != null && freeze != null,
                    "All five actual Defense popup controllers exist");
                var modals = new MonoBehaviour[] { win, lose, pause, tip, freeze };
                var show = new Action[] { win.Show, lose.Show, pause.Show, tip.Show, freeze.Show };
                var hide = new Action[] { () => win.Hide(true), () => lose.Hide(true), () => pause.Hide(true),
                    () => tip.Hide(true), () => freeze.Hide(true) };
                string[][] controls = {
                    new[] { "_nextLevelButton", "_mainMenuButton", "_levelsButton" },
                    new[] { "_retryButton", "_mainMenuButton" },
                    new[] { "_continueButton", "_retryButton", "_levelsButton", "_mainMenuButton" },
                    new[] { "_closeButton", "_watchAdButton" }, new[] { "_closeButton", "_watchAdButton" }
                };
                for (int i = 0; i < modals.Length; i++)
                {
                    string label = modals[i].GetType().Name;
                    Check(!modals[i].gameObject.activeSelf, label + " startup hide leaves the authored popup inactive");
                    int before = Plays(); hide[i](); hide[i]();
                    ExpectPlays(before, 0, label + " startup/already-hidden Hide(true) is silent");
                    before = Plays(); show[i](); show[i]();
                    ExpectPlays(before, 1, label + " repeated Show produces exactly one popup opening");
                    yield return Settled();
                    var group = Field<CanvasGroup>(modals[i], "_canvasGroup");
                    Check(group != null && group.alpha >= .99f && group.interactable && group.blocksRaycasts &&
                        modals[i].gameObject.activeInHierarchy, label + " native popup open animation settles fully visible");
                    foreach (string field in controls[i])
                    {
                        var control = Field<Button>(modals[i], field);
                        bool essential = field == controls[i][0] || field == "_watchAdButton";
                        if (!essential && control == null)
                        { Results.Add("NOT AUTHORED: optional " + label + " control " + field); continue; }
                        Check(control != null && control.IsActive() && control.IsInteractable(),
                            label + " authored action is available: " + field);
                    }
                    before = Plays(); hide[i](); hide[i]();
                    ExpectPlays(before, 1, label + " repeated Hide(true) produces exactly one popup close");
                    Check(!modals[i].gameObject.activeSelf && group.alpha <= .01f && !group.interactable && !group.blocksRaycasts,
                        label + " instant close removes visibility and input blocking");
                    yield return Settled(.2f);
                }
                foreach (var modal in new MonoBehaviour[] { tip, freeze })
                {
                    bool isTip = modal == tip;
                    if (isTip) tip.Show(); else freeze.Show();
                    yield return Settled();
                    int before = Plays(); Click(Field<Button>(modal, "_closeButton"));
                    ExpectPlays(before, 2, modal.GetType().Name + " real pointer close produces one click plus one closing cue");
                    yield return Settled();
                    var group = Field<CanvasGroup>(modal, "_canvasGroup");
                    Check(!modal.gameObject.activeSelf && group.alpha <= .01f && !group.interactable && !group.blocksRaycasts,
                        modal.GetType().Name + " actual Close finishes its native animation and releases input blocking");
                }
                Results.Add("NOT EXERCISED: rewarded ad buttons are inspected only; this phase requests no ads");
            }
            finally
            {
                LevelManager.OnLevelReady -= ready;
                Time.timeScale = originalTimeScale;
                if (SceneManager.GetActiveScene().name != "MainMenuScene") SceneManager.LoadScene("MainMenuScene");
            }
            yield return Settled();
            Check(SceneManager.GetActiveScene().name == "MainMenuScene" && Object.FindFirstObjectByType<MainMenuUI>() != null,
                "Defense popup fixture returns to the actual Main Menu");
        }
    }
}
