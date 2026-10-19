namespace ArrowSwarm.Modes.Editor
{
    using ArrowSwarm.Localization;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;
    using U = ModeAuthoringUtility;

    /// <summary>Binds the copied GameScene HUD and adds only the Color Shift swipe control.</summary>
    public static partial class PuzzleHUDAuthoring
    {
        /// <summary>Keeps original authored transforms, sprites, fonts, theme bindings and slider callbacks.</summary>
        public static void Bind(GameObject canvas, ArrowSwarm.Camera.CameraController camera, bool colorShift)
        {
            var root = canvas.transform;
            var hud = canvas.GetComponent<PuzzleHUD>() ?? canvas.AddComponent<PuzzleHUD>();
            var level = root.Find("TopBar/LevelText").GetComponent<TextMeshProUGUI>();
            var counter = root.Find("BottomBar/ArrowText").GetComponent<TextMeshProUGUI>();
            RemoveLocalization(level); RemoveLocalization(counter);
            foreach (var label in new[] { level, counter })
            {
                label.enableAutoSizing = true; label.fontSizeMax = label.fontSize;
                label.fontSizeMin = label.fontSize * .5f; label.textWrappingMode = TextWrappingModes.NoWrap;
            }
            var stars = new Image[3];
            for (int i = 0; i < 3; i++) stars[i] = root.Find("TopBar/Hearts/Heart_" + i).GetComponent<Image>();
            U.Set(hud, "_levelText", level); U.Set(hud, "_arrowCountText", counter); U.Set(hud, "_heartIcons", stars);
            U.Set(hud, "_pause", root.Find("TopBar/PauseButton").GetComponent<Button>());
            U.Set(hud, "_hint", root.Find("BottomBar/Skill1").GetComponent<Button>());
            U.Set(hud, "_hintCount", root.Find("BottomBar/Skill1/Badge/Text").GetComponent<TextMeshProUGUI>());
            U.Set(hud, "_hintAdBadge", root.Find("BottomBar/Skill1/AdBadge").gameObject);
            BindRocket(root, hud);
            var feedback = canvas.GetComponent<HUDFeedbackEffects>() ?? canvas.AddComponent<HUDFeedbackEffects>();
            U.Set(feedback, "_heartIcons", stars); U.Set(feedback, "_arrowCountText", counter); U.Set(hud, "_feedback", feedback);
            var track = BuildTrack(root, counter);
            U.Set(hud, "_rotationBar", track.gameObject);
            U.Set(hud, "_rotationButton", track.GetComponent<Button>());
            U.Set(hud, "_turns", track.Find("Turns").GetComponent<TextMeshProUGUI>());
            U.Set(hud, "_instruction", track.Find("Instruction").GetComponent<TextMeshProUGUI>());
            track.gameObject.SetActive(colorShift);
            U.Set(camera, "_topPanelRect", root.Find("TopBar"));
            U.Set(camera, "_bottomPanelRect", colorShift ? track : root.Find("BottomBar"));
            foreach (var button in canvas.GetComponentsInChildren<Button>(true))
            {
                button.transition = Selectable.Transition.ColorTint;
                if (button.GetComponent<UIButtonFeedback>() == null) button.gameObject.AddComponent<UIButtonFeedback>();
            }
            canvas.GetComponent<CanvasGroup>().alpha = 1f;
            canvas.GetComponent<CanvasGroup>().interactable = canvas.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }

        private static void RemoveLocalization(TMP_Text text)
        {
            var component = text.GetComponent<LocalizedText>();
            if (component != null) UnityEngine.Object.DestroyImmediate(component);
        }
    }
}
