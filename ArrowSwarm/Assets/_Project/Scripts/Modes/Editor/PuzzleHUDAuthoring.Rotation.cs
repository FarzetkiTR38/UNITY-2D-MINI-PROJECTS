namespace ArrowSwarm.Modes.Editor
{
    using ArrowSwarm.Localization;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;
    using U = ModeAuthoringUtility;

    public static partial class PuzzleHUDAuthoring
    {
        private static RectTransform BuildTrack(Transform root, TextMeshProUGUI template)
        {
            var previous = root.Find("RotationTrack");
            if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
            var bottom = root.Find("BottomBar").GetComponent<RectTransform>();
            var track = UnityEngine.Object.Instantiate(bottom, root, false); track.name = "RotationTrack";
            for (int i = track.childCount - 1; i >= 0; i--)
                if (track.GetChild(i).name != "Bg") UnityEngine.Object.DestroyImmediate(track.GetChild(i).gameObject);
            track.anchorMin = track.anchorMax = new Vector2(.5f, 0f); track.pivot = new Vector2(.5f, .5f);
            track.anchoredPosition = new Vector2(0f, 345f); track.sizeDelta = new Vector2(940f, 140f);
            var background = track.Find("Bg").GetComponent<RectTransform>();
            background.anchorMin = Vector2.zero; background.anchorMax = Vector2.one;
            background.offsetMin = background.offsetMax = Vector2.zero; background.localScale = Vector3.one;
            var image = background.GetComponent<Image>(); image.preserveAspect = false; image.raycastTarget = false;
            // The original transparent root Image captures the complete pill. The visible plate receives tint.
            track.GetComponent<Image>().raycastTarget = true;
            var button = track.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = root.Find("TopBar/PauseButton").GetComponent<Button>().colors;
            button.onClick = new Button.ButtonClickedEvent();
            var feedback = track.gameObject.AddComponent<UIButtonFeedback>();
            U.Set(feedback, "_targetTransform", track); U.Set(feedback, "_pressedScale", .96f); U.Set(feedback, "_punchScale", 1.025f);
            var swipe = track.gameObject.AddComponent<FrameSwipeUI>();
            U.Set(swipe, "_tapToRotate", true); U.Set(swipe, "_tapCenterDeadZoneFraction", .14f);

            Circle("TurnGlow", track, 116f, new Color(.16f, .8f, 1f), new Color(.15f, .76f, 1f));
            Circle("TurnRim", track, 105f, Color.white, new Color(.64f, .94f, 1f));
            Circle("TurnFace", track, 94f, new Color(.96f, .99f, 1f), new Color(.055f, .16f, .29f));
            Label("Turns", track, template, Vector2.zero, new Vector2(86f, 80f), 62f, "5",
                new Color(.04f, .22f, .65f), Color.white);
            for (int side = -1; side <= 1; side += 2)
                for (int index = 0; index < 3; index++)
                {
                    float alpha = .38f + .25f * index;
                    Label((side < 0 ? "Left" : "Right") + index, track, template, new Vector2(side * (160f + index * 79f), 0f),
                        new Vector2(100f, 110f), 100f, side < 0 ? "‹" : "›",
                        new Color(.12f, .44f, .86f, alpha), new Color(.58f, .85f, 1f, alpha));
                }
            var instruction = Label("Instruction", track, template, new Vector2(0f, -95f), new Vector2(900f, 34f), 25f,
                "Swipe left / right to rotate", new Color(.12f, .27f, .54f), new Color(.72f, .87f, 1f));
            var localized = instruction.gameObject.AddComponent<LocalizedText>();
            U.Set(localized, "_localizationKey", "puzzle_swipe");
            return track;
        }

        private static void Circle(string name, Transform parent, float size, Color light, Color dark)
        {
            var rect = U.Rect(name, parent, 0, 0, size, size);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f); rect.anchoredPosition = Vector2.zero;
            var image = U.Image(rect, AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"), light);
            image.preserveAspect = true; AuthorTheme(image, light, dark);
        }

        private static TextMeshProUGUI Label(string name, Transform parent, TextMeshProUGUI template, Vector2 position,
            Vector2 size, float fontSize, string value, Color light, Color dark)
        {
            var label = UnityEngine.Object.Instantiate(template, parent, false); label.name = name;
            RemoveLocalization(label); label.text = value; label.fontSize = fontSize; label.color = light;
            label.enableAutoSizing = true; label.fontSizeMin = fontSize * .55f; label.fontSizeMax = fontSize;
            label.textWrappingMode = TextWrappingModes.NoWrap; label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false;
            var rect = label.rectTransform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = position; rect.sizeDelta = size; rect.localScale = Vector3.one;
            AuthorTheme(label, light, dark);
            return label;
        }

        private static void AuthorTheme(Graphic graphic, Color light, Color dark)
        {
            var theme = graphic.GetComponent<ThemedUIElement>() ?? graphic.gameObject.AddComponent<ThemedUIElement>();
            var serialized = new SerializedObject(theme);
            serialized.FindProperty("_role").enumValueIndex = (int)ThemeRole.Custom;
            serialized.FindProperty("_customLightColor").colorValue = light;
            serialized.FindProperty("_customDarkColor").colorValue = dark;
            serialized.FindProperty("_materialOnly").boolValue = false;
            serialized.FindProperty("_preserveAlpha").boolValue = true;
            serialized.FindProperty("_darkMaterial").objectReferenceValue = null;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
