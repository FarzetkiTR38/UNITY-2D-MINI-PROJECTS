namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary>Exercises the actual menu panels through native pointer input and captures the closing pose.</summary>
    public static partial class UiRollout01Verification
    {
        private static bool _nextMenuCloseStage, _menuCloseStage;

        /// <summary>Checks menu exits at the current Game View size from idle MainMenu reached through Boot.</summary>
        public static void MenuCloseAt(string folder)
        {
            _nextMenuCloseStage = true;
            try { Start(folder, false); }
            finally { _nextMenuCloseStage = false; }
        }

        private static IEnumerator VerifyMenuCloseStage()
        {
            var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
            var data = DataManager.Instance;
            var player = data.PlayerData;
            string original = JsonUtility.ToJson(player);
            var home = Field<GameObject>(menu, "_mainPanel");
            try
            {
                using (var pointer = new NativeMouse())
                {
                    foreach (var theme in new[] { ThemeMode.Light, ThemeMode.Dark })
                    {
                        data.SetTheme(theme);
                        foreach (var mode in new[] { GameModeId.Defense, GameModeId.Shapes, GameModeId.ColorShift })
                        {
                            data.SelectMode(mode);
                            yield return Click(pointer, Field<Button>(menu, "_levelsButton"));
                            var levels = UnityEngine.Object.FindFirstObjectByType<LevelSelectUI>();
                            yield return VerifyNativeMenuClose(pointer, menu, home, levels,
                                Field<Button>(levels, "_closeButton"), "levels-" + mode + "-" + theme);
                        }

                        yield return Click(pointer, Field<Button>(menu, "_leaderboardButton"));
                        var board = UnityEngine.Object.FindFirstObjectByType<LeaderboardUI>();
                        yield return VerifyNativeMenuClose(pointer, menu, home, board,
                            Field<Button>(board, "_closeButton"), "leaderboard-close-" + theme);
                        yield return Click(pointer, Field<Button>(menu, "_leaderboardButton"));
                        yield return VerifyNativeMenuClose(pointer, menu, home, board,
                            Field<Button>(board, "_backButton"), "leaderboard-back-" + theme);

                        yield return Click(pointer, Field<Button>(menu, "_settingsButton"));
                        var settings = UnityEngine.Object.FindFirstObjectByType<SettingsUI>();
                        yield return VerifyNativeMenuClose(pointer, menu, home, settings,
                            Field<Button>(settings, "_closeButton"), "settings-" + theme);
                    }
                }

                yield return VerifyInterruptedMenuClose(menu, home);
            }
            finally
            {
                JsonUtility.FromJsonOverwrite(original, player);
                menu.OpenMainPanel();
            }
        }

        private static IEnumerator VerifyNativeMenuClose(NativeMouse pointer, MainMenuUI menu, GameObject home,
            MonoBehaviour panel, Button close, string label)
        {
            var group = Field<CanvasGroup>(panel, "_canvasGroup");
            var dialog = Field<RectTransform>(panel, "_dialogBox");
            Vector3 restScale = dialog.localScale;
            Check(panel.gameObject.activeSelf && !home.activeSelf && group.interactable && group.alpha == 1f,
                label + " opens and settles before native close");
            Canvas.ForceUpdateCanvases();
            VerifyButton(close, label);
            var canvas = close.GetComponentInParent<Canvas>();
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            var rect = close.GetComponent<RectTransform>();
            var center = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
            int clicks = 0;
            bool startsOverHome = false;
            void ObserveClose()
            {
                clicks++;
                startsOverHome = panel.gameObject.activeSelf && !home.activeSelf && group.blocksRaycasts && !group.interactable;
            }

            close.onClick.AddListener(ObserveClose);
            string screenshot = Path.Combine(_folder, label + "-closing.png");
            if (File.Exists(screenshot)) File.Delete(screenshot);
            bool intermediate = false;
            try
            {
                pointer.Send(center, false);
                yield return pointer.Wait();
                var hits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = center }, hits);
                Check(hits.Count > 0 && ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) == close.gameObject,
                    label + " native pointer reaches the authored close control");
                pointer.Send(center, true);
                yield return pointer.Wait();
                pointer.Send(center, false);
                float deadline = Time.unscaledTime + 2f;
                while (!home.activeSelf && Time.unscaledTime < deadline)
                {
                    if (clicks == 1 && panel.gameObject.activeSelf && !intermediate &&
                        group.alpha > .05f && group.alpha < .99f && dialog.localScale.x < restScale.x)
                    {
                        Check(group.blocksRaycasts && !group.interactable,
                            label + " retains the input shield while shrinking and fading");
                        float alpha = group.alpha;
                        Vector3 scale = dialog.localScale;
                        menu.BackToMain();
                        Check(group.alpha == alpha && dialog.localScale == scale,
                            label + " ignores a second close while the same effect is playing");
                        ScreenCapture.CaptureScreenshot(screenshot);
                        intermediate = true;
                    }
                    yield return null;
                }

                yield return pointer.Wait();
                Check(clicks == 1 && startsOverHome, label + " native click starts exit before Home is shown");
                Check(intermediate, label + " displays a rendered intermediate shrinking and fading pose");
                Check(home.activeSelf && !panel.gameObject.activeSelf && group.alpha == 0f &&
                    !group.blocksRaycasts && dialog.localScale == restScale,
                    label + " reveals Home only after closing and restores the resting dialog scale");
                yield return (Func<bool>)(() => File.Exists(screenshot) && new FileInfo(screenshot).Length > 0);
            }
            finally { close.onClick.RemoveListener(ObserveClose); }
        }

        private static IEnumerator VerifyInterruptedMenuClose(MainMenuUI menu, GameObject home)
        {
            menu.OpenSettings();
            yield return Settled();
            var settings = UnityEngine.Object.FindFirstObjectByType<SettingsUI>();
            var group = Field<CanvasGroup>(settings, "_canvasGroup");
            var dialog = Field<RectTransform>(settings, "_dialogBox");
            float previousTimeScale = Time.timeScale;
            try
            {
                Time.timeScale = 0f;
                menu.BackToMain();
                yield return (Func<bool>)(() => group.alpha < .95f);
                Check(!home.activeSelf && settings.gameObject.activeSelf, "Settings exit advances with time scale zero");
                float alpha = group.alpha;
                Vector3 scale = dialog.localScale;
                menu.OpenSettings();
                Check(group.alpha == alpha && dialog.localScale == scale,
                    "Reopening during exit keeps the same pose and reverses smoothly");
                yield return Settled();
                Check(settings.gameObject.activeSelf && group.interactable && group.alpha == 1f && !home.activeSelf,
                    "Interrupted exit settles open without a stale Home callback");

                menu.BackToMain();
                yield return (Func<bool>)(() => group.alpha < .95f);
                menu.OpenLevels();
                yield return Settled();
                Check(!settings.gameObject.activeSelf && !home.activeSelf &&
                    UnityEngine.Object.FindFirstObjectByType<LevelSelectUI>() != null,
                    "Switching panels during an exit does not reveal Home over the destination");
            }
            finally { Time.timeScale = previousTimeScale; }
        }
    }
}
