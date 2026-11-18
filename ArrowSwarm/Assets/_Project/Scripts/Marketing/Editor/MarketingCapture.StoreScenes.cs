namespace ArrowSwarm.Marketing.Editor
{
    using System;
    using System.Collections;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Puzzle;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Actual scene routes and ordinary legal actions for localized store screenshots.</summary>
    public static partial class MarketingCapture
    {
        private static void SetStoreLanguage(string language)
        {
            ArrowSwarm.Localization.LocalizationManager.Instance.SetLanguage(language, false);
        }

        private static IEnumerator StoreHome(string language)
        {
            PuzzleLaunch.Preview = false; SetStoreLanguage(language);
            SceneManager.LoadScene("MainMenuScene"); yield return StoreWait(60);
            SetStoreLanguage(language); UnityEngine.Object.FindFirstObjectByType<ArrowSwarm.UI.MainMenuUI>().OpenMainPanel();
            yield return StoreWait(20);
            yield return StoreSave("01_Home", language, "Home", 10, "Real Home, selected Defense, fictional local demonstration progress.");
        }

        private static IEnumerator StoreDefense(int level, bool freeze, string prefix, string language)
        {
            PuzzleLaunch.Preview = false; DataManager.Instance.PlayerData.currentLevel = level;
            DataManager.Instance.PlayerData.freezeCount = 1; SetStoreLanguage(language);
            SceneManager.LoadScene("GameScene"); yield return StoreWait(3);
            int wait = 0;
            while ((!LevelManager.HasInstance || LevelManager.Instance.CurrentParams.Level != level ||
                GameManager.Instance.CurrentState != GameState.Playing) && wait++ < 300) yield return null;
            if (wait >= 300) throw new InvalidOperationException("Defense store startup timed out.");
            SetStoreLanguage(language); StoreFrameDefense(); wait = 0;
            while (LeadingDefenseMob() < .5f && wait++ < 1000) yield return null;
            if (wait >= 1000) throw new InvalidOperationException("No natural Defense approach.");
            if (freeze)
            {
                InvokeDefenseFreeze();
                if (!ArrowSwarm.Skills.FreezeManager.Instance.IsFrozen || DataManager.Instance.PlayerData.freezeCount != 0)
                    throw new InvalidOperationException("Real Freeze charge was not used.");
                int reserved = _defenseReservedArrow; _defenseReservedArrow = -1;
                try
                {
                    yield return StoreWait(6);
                    var arrow = NextDefenseArrow();
                    if (arrow == null) throw new InvalidOperationException("No legal store action.");
                    arrow.OnPlayerClick(); yield return StoreWait(5);
                }
                finally { _defenseReservedArrow = reserved; }
            }
            if (GameManager.Instance.CurrentLives != 3) throw new InvalidOperationException("Store pose lost a life.");
            yield return StoreSave(prefix, language, "Defense", level, freeze
                ? "Real Freeze1→0 and one legal shot in flight before impact; capture-only12% wider camera; no stat/spawn edits."
                : "Natural half-path approach, full original grid and HUD; capture-only12% wider camera; no enemy/stat edits.");
        }

        private static void StoreFrameDefense()
        {
            var camera = UnityEngine.Camera.main;
            var controller = UnityEngine.Object.FindFirstObjectByType<ArrowSwarm.Camera.CameraController>();
            if (camera == null || controller == null) throw new InvalidOperationException("Native Defense camera required.");
            // This scene instance is unloaded after the still; no scene, config or gameplay asset is edited.
            float size = camera.orthographicSize * 1.12f;
            typeof(ArrowSwarm.Camera.CameraController).GetField("_targetOrthoSize", Private).SetValue(controller, size);
            camera.orthographicSize = size;
        }

        private static IEnumerator StorePuzzle(GameModeId mode, int level, bool rotate, string prefix, string language)
        {
            PuzzleLaunch.Preview = true; PuzzleLaunch.PreviewMode = mode; PuzzleLaunch.PreviewLevel = level;
            SetStoreLanguage(language); SceneManager.LoadScene(mode == GameModeId.Shapes ? "ShapesScene" : "ColorShiftScene");
            yield return StoreWait(60); SetStoreLanguage(language);
            var session = PuzzleSession.Active;
            if (session == null || session.Level == null || !session.Idle || session.LevelNumber != level)
                throw new InvalidOperationException("Native puzzle store board not ready.");
            int turns = session.Turns;
            if (rotate)
            {
                session.Rotate(1); yield return StoreWait(40);
                if (!session.Idle || session.Turns != turns - 1) throw new InvalidOperationException("Real settled rotation required.");
            }
            yield return StoreSave(prefix, language, mode.ToString(), level, session.Level.name + "; " + session.Level.Width + "x" +
                session.Level.Height + "; arrows=" + session.Remaining + "; orientation=" + session.Orientation +
                "; turns=" + session.Turns + (rotate ? "; actual90-degree frame rotation used one turn" : "; stationary native board"));
        }
    }
}
