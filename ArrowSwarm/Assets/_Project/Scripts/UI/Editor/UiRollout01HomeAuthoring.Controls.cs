namespace ArrowSwarm.UI.Editor
{
    using TMPro;
    using UnityEditor;
    using UnityEditor.Events;
    using UnityEngine;
    using UnityEngine.UI;
    using K = UiRollout01Kit;

    /// <summary>Builds shared-asset controls and retains the original navigation and click-feedback tuning.</summary>
    internal static partial class UiRollout01HomeAuthoring
    {
        private static string Feedback(Button source)
        { return source != null && source.GetComponent<UIButtonFeedback>() != null ? JsonUtility.ToJson(source.GetComponent<UIButtonFeedback>()) : null; }

        private static Button Button(Transform parent, string name, string sprite, Vector2 position, Vector2 size, string feedback, ColorBlock colors)
        {
            var face = K.Image(parent, name, K.Sprite(sprite), position, size);
            face.type = Image.Type.Sliced; face.raycastTarget = true;
            if (sprite != "primary_plate" && sprite != "profile_tile") K.RoundedPlate(face, true);
            var button = face.gameObject.AddComponent<Button>(); button.targetGraphic = face;
            button.colors = colors; button.navigation = new Navigation { mode = Navigation.Mode.None };
            var squeeze = face.gameObject.AddComponent<UIButtonFeedback>();
            if (!string.IsNullOrEmpty(feedback)) JsonUtility.FromJsonOverwrite(feedback, squeeze);
            var authored = new SerializedObject(squeeze);
            authored.FindProperty("_targetTransform").objectReferenceValue = face.transform;
            authored.ApplyModifiedPropertiesWithoutUndo();
            return button;
        }

        private static void Header(Transform parent, MainMenuUI menu, out TextMeshProUGUI level, out TextMeshProUGUI stars)
        {
            var left = K.Image(parent, "LevelCounter", K.Sprite("light_plate"), new Vector2(-270, 822), new Vector2(420, 112));
            K.RoundedPlate(left, true);
            var cup = K.Image(left.transform, "TrophyIcon", K.Sprite("icon_trophy"), new Vector2(-150, 0), new Vector2(96, 108)); cup.preserveAspect = true;
            level = K.Text(left.transform, "SelectedLevel", "Lv.102", new Vector2(55, 0), new Vector2(276, 85), 62, Ink);
            AutoSize(level, 28, 62); K.Theme(level, Ink, Ice);
            var right = K.Image(parent, "StarCounter", K.Sprite("light_plate"), new Vector2(150, 822), new Vector2(330, 112));
            K.RoundedPlate(right, true);
            var star = K.Image(right.transform, "StarIcon", K.Sprite("star"), new Vector2(-108, 0), new Vector2(100, 100)); star.preserveAspect = true;
            stars = K.Text(right.transform, "SharedStars", "31", new Vector2(43, 0), new Vector2(205, 85), 62, Ink);
            AutoSize(stars, 28, 62); K.Theme(stars, Ink, Ice);
            var profile = Button(parent, "ProfileButton", "profile_tile", new Vector2(437, 822), new Vector2(128, 128), null, ColorBlock.defaultColorBlock);
            UnityEventTools.AddPersistentListener(profile.onClick, menu.OpenProfileModal);
            var mask = K.Image(profile.transform, "AvatarMask", K.Sprite("profile_tile"), Vector2.zero, new Vector2(94, 94));
            mask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            var avatar = K.Image(mask.transform, "StarAvatar", K.Sprite("profile_avatar"), Vector2.zero, new Vector2(100, 100)); avatar.preserveAspect = true;
        }

        private static void ModeCard(Transform parent, GameModeSelectUI selector, TMP_Text level, string feedback, ColorBlock colors)
        {
            var card = Button(parent, "GameModeButton", "light_plate", new Vector2(0, -435), new Vector2(930, 180), feedback, colors);
            var icon = K.Image(card.transform, "SelectedModeIcon", K.Sprite("icon_defense"), new Vector2(-330, 0), new Vector2(170, 150)); icon.preserveAspect = true;
            var visual = icon.gameObject.AddComponent<ModeButtonVisual>();
            var variants = new SerializedObject(visual); variants.FindProperty("_target").objectReferenceValue = icon;
            foreach (var field in new[] { "_lightModes", "_darkModes" })
            {
                var array = variants.FindProperty(field); array.arraySize = 3;
                for (int i = 0; i < 3; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = K.Sprite(new[] { "icon_defense", "icon_shapes", "icon_color_shift" }[i]);
            }
            variants.ApplyModifiedPropertiesWithoutUndo();
            var title = K.Text(card.transform, "ModeTitle", "GAME MODE", new Vector2(52, 30), new Vector2(550, 73), 53, Ink, "menu_game_mode");
            AutoSize(title, 28, 53); K.Theme(title, Ink, Ice);
            var label = K.Text(card.transform, "ModeLabel", "DEFENSE", new Vector2(52, -43), new Vector2(540, 55), 37, new Color(.02f, .46f, .9f), "mode_defense");
            AutoSize(label, 24, 37); K.Theme(label, new Color(.02f, .46f, .9f), new Color(.35f, .82f, 1));
            var chevron = K.Image(card.transform, "Chevron", K.Sprite("icon_play"), new Vector2(376, 0), new Vector2(50, 72));
            chevron.preserveAspect = true; chevron.color = new Color(.12f, .6f, 1);
            Bind(selector, "_open", card); Bind(selector, "_modeLabel", label); Bind(selector, "_levelLabel", level);
            var settings = new SerializedObject(selector); settings.FindProperty("_menuModeIcons").arraySize = 0; settings.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Button Tile(Transform parent, string name, string iconName, string key, string fallback, float x, string feedback, ColorBlock colors)
        {
            var tile = Button(parent, name, "nav_tile", new Vector2(x, -730), new Vector2(280, 305), feedback, colors);
            var icon = K.Image(tile.transform, "Icon", K.Sprite(iconName), new Vector2(0, 53), new Vector2(162, 166)); icon.preserveAspect = true;
            var text = K.Text(tile.transform, "Label", fallback, new Vector2(0, -93), new Vector2(246, 96), 42, Ink, key);
            AutoSize(text, 22, 42); text.maxVisibleLines = 2; K.Theme(text, Ink, Ice);
            return tile;
        }
    }
}
