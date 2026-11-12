namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Puzzle;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.SceneManagement;

    /// <summary>Checks scene-owned Pause bindings, unchanged geometry and actual native mouse pause/resume.</summary>
    public static partial class Revision6PauseVerification
    {
        private static IEnumerator VerifyScene(string scene)
        {
            Status = "Loading " + scene; Time.timeScale = 1;
            bool puzzle = scene == "ShapesScene" || scene == "ColorShiftScene";
            PuzzleLaunch.Preview = puzzle; PuzzleLaunch.PreviewLevel = 15;
            PuzzleLaunch.PreviewMode = scene == "ShapesScene" ? GameModeId.Shapes : GameModeId.ColorShift;
            SceneManager.LoadScene(scene);
            yield return (Func<bool>)(() => puzzle ? PuzzleSession.Active != null && PuzzleSession.Active.Ready : DefenseReady());
            if (ArrowSwarm.Mob.MobSpawner.HasInstance) ArrowSwarm.Mob.MobSpawner.Instance.StopSpawning();
            var hud = UnityEngine.Object.FindFirstObjectByType<GameHUD>(FindObjectsInactive.Include);
            if (scene == "MapTestScene" && hud != null) hud.SetBarsVisible(true, true);
            Button pause = puzzle ? Field<Button>(UnityEngine.Object.FindFirstObjectByType<PuzzleHUD>(), "_pause") : Field<Button>(hud, "_pauseButton");
            Check(pause != null && pause.gameObject.activeInHierarchy && pause.interactable, scene + ": actual authored Pause button is available");
            DataManager.Instance.SetTheme(ThemeMode.Light); yield return Settled();
            var original = new ButtonState(pause);
            using (var mouse = new NativeMouse())
                foreach (ThemeMode theme in new[] { ThemeMode.Light, ThemeMode.Dark })
                {
                    Status = scene + " / " + theme + " / actual Pause input";
                    DataManager.Instance.SetTheme(theme); yield return Settled();
                    Check(original.Matches(pause), Status + ": source sprite, tint, rect, colors and feedback tuning are unchanged");
                    var image = pause.GetComponent<Image>();
                    Check(theme != ThemeMode.Dark || image.material.name == "DarkPause", Status + ": dedicated authored material is active");
                    yield return Capture(scene + "-" + theme.ToString().ToLowerInvariant());
                    Vector3 scale = pause.transform.localScale;
                    yield return Click(mouse, pause, true);
                    yield return (Func<bool>)(() => GameManager.Instance.CurrentState == GameState.Paused && Time.timeScale == 0);
                    var group = PauseGroup(puzzle);
                    yield return (Func<bool>)(() => group.alpha >= .99f && group.interactable && group.blocksRaycasts);
                    yield return Settled();
                    Check(!puzzle || PuzzleSession.Active.Paused, Status + ": the actual puzzle session also pauses");
                    Check(Vector3.Distance(pause.transform.localScale, scale) < .0001f, Status + ": press/release leaves no button scale drift");
                    Button resume = puzzle ? Field<Button>(UnityEngine.Object.FindFirstObjectByType<PuzzlePopups>(), "_resume") :
                        Field<Button>(UnityEngine.Object.FindFirstObjectByType<PauseMenuUI>(), "_continueButton");
                    yield return Click(mouse, resume, false);
                    yield return (Func<bool>)(() => GameManager.Instance.CurrentState == GameState.Playing && Time.timeScale > 0 &&
                        (!puzzle || !PuzzleSession.Active.Paused) && group.alpha <= .01f && !group.blocksRaycasts);
                    Check(original.Matches(pause), Status + ": real Resume preserves the original control contract");
                }
            DataManager.Instance.SetTheme(ThemeMode.Light); yield return Settled();
            Check(pause.GetComponent<Image>().material == original.Material, scene + ": exact original Light material returns after Dark");
        }

        private static bool DefenseReady()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing || !ArrowSwarm.Arrow.ArrowSpawner.HasInstance ||
                ArrowSwarm.Arrow.ArrowSpawner.Instance.ActiveArrows.Count == 0) return false;
            foreach (var arrow in ArrowSwarm.Arrow.ArrowSpawner.Instance.ActiveArrows)
                if (arrow != null && arrow.GetComponent<ArrowSwarm.Arrow.ArrowVisuals>().IsSpawning) return false;
            return UnityEngine.Object.FindFirstObjectByType<OverlayManager>(FindObjectsInactive.Include) != null;
        }

        private static CanvasGroup PauseGroup(bool puzzle) => puzzle
            ? Field<CanvasGroup[]>(UnityEngine.Object.FindFirstObjectByType<PuzzlePopups>(), "_panels")[0]
            : UnityEngine.Object.FindFirstObjectByType<PauseMenuUI>(FindObjectsInactive.Include).GetComponent<CanvasGroup>();

        private static IEnumerator Click(NativeMouse mouse, Button button, bool checkSqueeze)
        {
            Check(button != null && button.interactable && button.gameObject.activeInHierarchy, "Actual target button is active and interactable");
            Canvas.ForceUpdateCanvases();
            var rect = button.GetComponent<RectTransform>(); var canvas = button.GetComponentInParent<Canvas>();
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector2 center = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
            mouse.Send(new Vector2(2, 2), false); yield return mouse.Wait(); yield return Settled(.08f);
            Vector3 baseline = button.transform.localScale;
            mouse.Send(center, true); yield return mouse.Wait(); yield return Settled(.12f);
            if (checkSqueeze) Check(button.transform.localScale.x < baseline.x * .99f, "Actual native Pause press visibly squeezes its existing feedback");
            mouse.Send(center, false); yield return mouse.Wait(); yield return Settled(.22f);
        }

        /// <summary>Captures the designer-owned control contract independently of its theme material swap.</summary>
        private sealed class ButtonState
        {
            internal readonly Material Material;
            private readonly Sprite _sprite;
            private readonly Color _color;
            private readonly ColorBlock _colors;
            private readonly Vector2 _min, _max, _pivot, _size, _position;
            private readonly string _feedback;
            internal ButtonState(Button button)
            {
                var image = button.GetComponent<Image>(); var rect = image.rectTransform;
                Material = image.material; _sprite = image.sprite; _color = image.color; _colors = button.colors;
                _min = rect.anchorMin; _max = rect.anchorMax; _pivot = rect.pivot; _size = rect.sizeDelta; _position = rect.anchoredPosition;
                _feedback = JsonUtility.ToJson(button.GetComponent<UIButtonFeedback>());
            }
            internal bool Matches(Button button)
            {
                var image = button.GetComponent<Image>(); var rect = image.rectTransform;
                return image.sprite == _sprite && image.color == _color && button.colors.Equals(_colors) &&
                    rect.anchorMin == _min && rect.anchorMax == _max && rect.pivot == _pivot && rect.sizeDelta == _size &&
                    rect.anchoredPosition == _position && JsonUtility.ToJson(button.GetComponent<UIButtonFeedback>()) == _feedback;
            }
        }
    }
}
