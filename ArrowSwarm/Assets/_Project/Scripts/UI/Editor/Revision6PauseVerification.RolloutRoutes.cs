namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using ArrowSwarm.Core;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>Exercises the three remaining original pause actions through native queued input.</summary>
    public static partial class Revision6PauseVerification
    {
        private static IEnumerator Routes0608()
        {
            using(var mouse=new NativeMouse())
            foreach(var action in new[]{"_retryButton","_levelsButton","_mainMenuButton"})
            {
                Status="Defense Pause route / "+action;
                if(SceneManager.GetActiveScene().name!="GameScene")
                {
                    SceneManager.LoadScene("GameScene");yield return (Func<bool>)DefenseReady;
                    if(ArrowSwarm.Mob.MobSpawner.HasInstance)ArrowSwarm.Mob.MobSpawner.Instance.StopSpawning();
                }
                Time.timeScale=1;
                var hud=UnityEngine.Object.FindFirstObjectByType<GameHUD>();
                var pause=UnityEngine.Object.FindFirstObjectByType<PauseMenuUI>(FindObjectsInactive.Include);
                var generated=LevelManager.Instance.CurrentLevelData.ArrowPlacements;
                int level=ArrowSwarm.Data.DataManager.Instance.PlayerData.currentLevel;
                {
                    yield return Settled(.6f);
                    yield return Click(mouse,Field<Button>(hud,"_pauseButton"),false);
                    yield return (Func<bool>)(()=>GameManager.Instance.CurrentState==GameState.Paused);
                    yield return Settled(.6f);
                    yield return Click(mouse,Field<Button>(pause,action),false);
                    if(action=="_retryButton")
                    {
                        yield return (Func<bool>)(()=>GameManager.Instance.CurrentState==GameState.Playing &&
                            !ReferenceEquals(generated,LevelManager.Instance.CurrentLevelData.ArrowPlacements));
                        yield return (Func<bool>)DefenseReady;
                        Check(ArrowSwarm.Data.DataManager.Instance.PlayerData.currentLevel==level,"Native Retry regenerates the same level through original handler");
                    }
                    else
                    {
                        yield return (Func<bool>)(()=>SceneManager.GetActiveScene().name=="MainMenuScene");
                        yield return Settled(.9f);
                        var levels=UnityEngine.Object.FindFirstObjectByType<LevelSelectUI>(FindObjectsInactive.Include);
                        if(action=="_levelsButton")Check(levels.gameObject.activeInHierarchy,"Native Levels opens the real existing campaign selector");
                        else Check(!levels.gameObject.activeInHierarchy,"Native Main Menu opens Home without the level selector");
                    }
                }
            }
        }
    }
}
