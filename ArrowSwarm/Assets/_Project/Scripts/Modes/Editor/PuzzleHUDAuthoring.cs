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
    public static class PuzzleHUDAuthoring
    {
        /// <summary>Keeps original authored transforms, sprites, fonts, theme bindings and slider callbacks.</summary>
        public static void Bind(GameObject canvas, ArrowSwarm.Camera.CameraController camera, bool colorShift)
        {
            var root = canvas.transform;
            var hud = canvas.AddComponent<PuzzleHUD>();
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
            var feedback = canvas.GetComponent<HUDFeedbackEffects>() ?? canvas.AddComponent<HUDFeedbackEffects>();
            U.Set(feedback, "_heartIcons", stars); U.Set(feedback, "_arrowCountText", counter); U.Set(hud, "_feedback", feedback);
            var track = BuildTrack(root, counter);
            U.Set(hud, "_rotationBar", track.gameObject);
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

        private static RectTransform BuildTrack(Transform root, TextMeshProUGUI template)
        {
            var bottom = root.Find("BottomBar").GetComponent<RectTransform>();
            var track = UnityEngine.Object.Instantiate(bottom, root, false); track.name = "RotationTrack";
            for (int i = track.childCount - 1; i >= 0; i--)
                if (track.GetChild(i).name != "Bg") UnityEngine.Object.DestroyImmediate(track.GetChild(i).gameObject);
            track.anchorMin = track.anchorMax = track.pivot = new Vector2(.5f, 0f);
            track.anchoredPosition = new Vector2(0f, 235f); track.sizeDelta = new Vector2(940f, 125f);
            var background = track.Find("Bg").GetComponent<RectTransform>();
            background.anchorMin = Vector2.zero; background.anchorMax = Vector2.one;
            background.offsetMin = background.offsetMax = Vector2.zero;
            background.localScale = Vector3.one;
            background.GetComponent<Image>().preserveAspect = false;
            track.GetComponent<Image>().raycastTarget = true;
            track.gameObject.AddComponent<FrameSwipeUI>();
            Label("Turns", track, template, new Vector2(0f, 20f), new Vector2(730f, 48f), 36f, "Rotations: 5");
            Label("Instruction", track, template, new Vector2(0f, -22f), new Vector2(730f, 38f), 25f, "Swipe left / right to rotate");
            Label("Left", track, template, new Vector2(-420f, 0f), new Vector2(110f, 100f), 60f, "‹‹");
            Label("Right", track, template, new Vector2(420f, 0f), new Vector2(110f, 100f), 60f, "››");
            return track;
        }
        private static void Label(string name, Transform parent, TextMeshProUGUI template, Vector2 position, Vector2 size, float fontSize, string text)
        {
            var label = UnityEngine.Object.Instantiate(template, parent, false); label.name = name;
            RemoveLocalization(label); label.text = text; label.fontSize = fontSize;
            label.enableAutoSizing = true; label.fontSizeMin = fontSize * .6f; label.fontSizeMax = fontSize;
            label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false;
            var r = label.rectTransform; r.anchorMin = r.anchorMax = r.pivot = new Vector2(.5f, .5f);
            r.anchoredPosition = position; r.sizeDelta = size; r.localScale = Vector3.one;
        }
        private static void RemoveLocalization(TMP_Text text)
        {
            var component = text.GetComponent<LocalizedText>();
            if (component != null) UnityEngine.Object.DestroyImmediate(component);
        }
    }
}
