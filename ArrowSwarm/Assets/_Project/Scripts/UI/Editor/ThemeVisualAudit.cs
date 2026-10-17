namespace ArrowSwarm.UI.Editor
{
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Editor-only visual inspection staging. Does not award stars, unlock levels or save player data.</summary>
    public static class ThemeVisualAudit
    {
        /// <summary>Opens an actual menu screen and completes its presentation animation for comparison.</summary>
        public static string Menu(string screen, ThemeMode theme)
        {
            SetTheme(theme);
            var menu = Object.FindFirstObjectByType<MainMenuUI>();
            if (menu == null) throw new System.InvalidOperationException("MainMenuScene is required.");
            var profile = Object.FindFirstObjectByType<ProfileSetupModalUI>(FindObjectsInactive.Include);
            if (profile != null) profile.gameObject.SetActive(false);
            switch (screen)
            {
                case "Main": menu.OpenMainPanel(); break;
                case "Settings": menu.OpenSettings(); break;
                case "Levels": menu.OpenLevels(); break;
                case "Leaderboard": menu.OpenLeaderboard(); break;
                case "Profile": menu.OpenMainPanel(); profile.Show(); break;
                default: throw new System.ArgumentException(screen);
            }
            string selected = screen == "Main" ? "MainPanel" : screen == "Profile" ? "ProfileSetupModal" : screen+"Panel";
            foreach (Transform child in menu.transform)
            {
                if (!child.name.EndsWith("Panel") && child.name != "ProfileSetupModal") continue;
                bool visible = child.name == selected;
                foreach (var behaviour in child.GetComponents<MonoBehaviour>()) behaviour.StopAllCoroutines();
                var group = child.GetComponent<CanvasGroup>();
                if (group != null) { group.alpha = visible ? 1 : 0; group.interactable=visible; group.blocksRaycasts=visible; }
                if (!visible) continue;
                foreach (Transform box in child)
                    if (box.name == "BoardFrame" || box.name == "WindowCard" || box.name == "Card")
                        box.localScale = screen == "Profile" ? Vector3.one*0.73f : Vector3.one;
            }
            Canvas.ForceUpdateCanvases();
            return screen+"/"+theme;
        }

        /// <summary>Stages game UI without invoking win/lose handlers that mutate player progression.</summary>
        public static string Game(string screen, ThemeMode theme)
        {
            SetTheme(theme);
            GameManager.Instance.SetState(GameState.Paused);
            ArrowSwarm.Mob.MobSpawner.Instance.StopSpawning();
            Time.timeScale = 0;
            Object.FindFirstObjectByType<GameHUD>().SetBarsVisible(!screen.StartsWith("Tutorial"),!screen.StartsWith("Tutorial"));
            var overlay = Object.FindFirstObjectByType<OverlayManager>();
            if (overlay == null) throw new System.InvalidOperationException("GameScene is required.");
            string selected = screen == "Win" ? "WinPanel" : screen == "Lose" ? "LosePanel" : screen == "Hint" ? "TipPopupPanel" : screen == "Freeze" ? "FreezePopupPanel" : screen+"Panel";
            foreach (Transform panel in overlay.transform)
            {
                if (!panel.name.EndsWith("Panel")) continue;
                bool visible = panel.name == selected;
                foreach (var behaviour in panel.GetComponents<MonoBehaviour>())
                {
                    var flag = behaviour.GetType().GetField("_isShowing", System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
                    if (flag != null) flag.SetValue(behaviour,visible);
                    behaviour.StopAllCoroutines();
                }
                panel.gameObject.SetActive(visible);
                var group = panel.GetComponent<CanvasGroup>();
                if (group != null) { group.alpha = visible ? 1 : 0; group.interactable=visible; group.blocksRaycasts = false; }
                var box = panel.Find("DialogBox");
                if (box != null) box.localScale = Vector3.one;
                if (visible && screen == "Win")
                    for (int i=0;i<3;i++)
                    {
                        var star = box.Find("StarsContainer/Star_"+i);
                        if (star != null) { star.gameObject.SetActive(true); star.localScale=Vector3.one; }
                    }
            }
            var tutorial = Object.FindFirstObjectByType<ArrowSwarm.Tutorial.TutorialOverlayUI>(FindObjectsInactive.Include);
            if (tutorial != null)
            {
                tutorial.gameObject.SetActive(screen.StartsWith("Tutorial"));
                if (screen.StartsWith("Tutorial"))
                {
                    tutorial.transform.parent.gameObject.SetActive(true);
                    tutorial.Show();
                    tutorial.SetInstruction("Tap an arrow to hit the enemies!");
                    if (screen == "TutorialComplete") tutorial.ShowCompletionCard("TUTORIAL COMPLETE", "You mastered all the basics!", "MAIN MENU");
                    tutorial.StopAllCoroutines();
                    foreach (var group in tutorial.GetComponentsInChildren<CanvasGroup>(true))
                        group.alpha = group.name == "Banner" && screen == "TutorialComplete" || group.name == "CompletionBackdrop" && screen != "TutorialComplete" ? 0 : 1;
                    var card = tutorial.transform.Find("CompleteCard");
                    if (card != null && screen == "TutorialComplete")
                    {
                        card.gameObject.SetActive(true);
                        card.localScale = (Vector3)typeof(ArrowSwarm.Tutorial.TutorialOverlayUI).GetField("_originalCardScale",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(tutorial);
                        card.GetComponent<CanvasGroup>().alpha=1;
                    }
                }
            }
            var hand = Object.FindFirstObjectByType<ArrowSwarm.Tutorial.TutorialHandUI>(FindObjectsInactive.Include);
            if (hand != null)
            {
                hand.gameObject.SetActive(screen == "TutorialHand");
                if (screen == "TutorialHand") { hand.PointToLocalPosition(Vector2.zero,false); hand.SetActionTag("TAP!"); }
            }
            Canvas.ForceUpdateCanvases();
            return screen+"/"+theme;
        }

        private static void SetTheme(ThemeMode theme)
        {
            if (!Application.isPlaying) throw new System.InvalidOperationException("Use Play Mode for the visual audit.");
            DataManager.Instance.PlayerData.theme = theme;
            ThemeManager.Instance.ApplyTheme(theme);
        }
    }
}
