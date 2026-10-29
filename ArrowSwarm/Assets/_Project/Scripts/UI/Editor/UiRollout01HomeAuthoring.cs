namespace ArrowSwarm.UI.Editor
{
    using System;
    using ArrowSwarm.Localization;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using K = UiRollout01Kit;

    /// <summary>Authors only the Home composition, retaining existing controllers and later-stage dialogs.</summary>
    internal static partial class UiRollout01HomeAuthoring
    {
        private static readonly Color Ink = new Color(.025f, .16f, .57f);
        private static readonly Color Ice = new Color(.83f, .93f, 1f);

        internal static void Apply()
        {
            if (Application.isPlaying || SceneManager.GetActiveScene().path != "Assets/_Project/Scenes/MainMenuScene.unity")
                throw new InvalidOperationException("Open MainMenuScene in Edit Mode before authoring Home.");
            var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>(FindObjectsInactive.Include);
            var settings = new SerializedObject(menu);
            var panel = (GameObject)settings.FindProperty("_mainPanel").objectReferenceValue;
            var selector = panel.GetComponent<GameModeSelectUI>();
            if (selector == null || panel.transform.Find("MenuArt") == null)
                throw new InvalidOperationException("Existing Home and mode modal bindings must be present.");
            foreach (string key in new[] { "brand_crest", "primary_plate", "light_plate", "nav_tile", "profile_avatar", "star" }) K.Sprite(key);
            var oldPlay = (Button)settings.FindProperty("_playButton").objectReferenceValue;
            var oldLevels = (Button)settings.FindProperty("_levelsButton").objectReferenceValue;
            var oldBoard = (Button)settings.FindProperty("_leaderboardButton").objectReferenceValue;
            var oldSettings = (Button)settings.FindProperty("_settingsButton").objectReferenceValue;
            string[] feedback = { Feedback(oldPlay), Feedback(oldLevels), Feedback(oldBoard), Feedback(oldSettings) };
            ColorBlock colors = oldPlay.colors;
            var oldArt = panel.transform.Find("MenuArt");
            UnityEngine.Object.DestroyImmediate(oldArt.gameObject);
            var oldBackground = panel.transform.Find("SharedBackdrop");
            if (oldBackground != null) UnityEngine.Object.DestroyImmediate(oldBackground.gameObject);
            K.Backdrop(panel.transform);
            var art = Content(panel.transform);
            Header(art, menu, out TextMeshProUGUI level, out TextMeshProUGUI stars);
            var crest = K.Image(art, "BrandCrest", K.Sprite("brand_crest"), new Vector2(0, 470), new Vector2(970, 610));
            crest.preserveAspect = true;
            K.Wordmark(art, new Vector2(0, 115), 1010);
            var play = Button(art, "PlayButton", "primary_plate", new Vector2(0, -215), new Vector2(930, 235), feedback[0], colors);
            var triangle = K.Image(play.transform, "PlayIcon", K.Sprite("icon_play"), new Vector2(-307, 0), new Vector2(150, 160));
            triangle.preserveAspect = true;
            var playText = K.Text(play.transform, "Label", "PLAY", new Vector2(90, 2), new Vector2(600, 175), 130, Color.white, "menu_play");
            AutoSize(playText, 72, 130);
            ModeCard(art, selector, level, feedback[1], colors);
            var levels = Tile(art, "LevelsButton", "icon_map", "menu_levels", "LEVELS", -332, feedback[1], colors);
            var board = Tile(art, "LeaderboardButton", "icon_trophy", "menu_leaderboard", "LEADERBOARD", 0, feedback[2], colors);
            var options = Tile(art, "SettingsButton", "icon_settings", "menu_settings", "SETTINGS", 332, feedback[3], colors);
            Bind(menu, "_playButton", play); Bind(menu, "_levelsButton", levels);
            Bind(menu, "_leaderboardButton", board); Bind(menu, "_settingsButton", options);
            Bind(menu, "_levelText", level); Bind(menu, "_starsText", stars);
            // The brand has two authored TMP lines; SetupUI must not replace either with a single label.
            Bind(menu, "_titleText", null);
            UiRolloutResponsiveAuthoring.Home(panel.transform);
            foreach (var canvas in panel.GetComponentsInParent<Canvas>(true))
                canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
            EditorUtility.SetDirty(menu); EditorUtility.SetDirty(selector);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static RectTransform Content(Transform parent)
        {
            var rect = new GameObject("MenuArt", typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.SetSiblingIndex(1);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.sizeDelta = new Vector2(1080, 1920);
            var fitter = rect.gameObject.AddComponent<ReferenceCanvasFit>(); fitter.enabled = false;
            var serialized = new SerializedObject(fitter);
            serialized.FindProperty("_referenceSize").vector2Value = new Vector2(1080, 1920);
            serialized.ApplyModifiedPropertiesWithoutUndo(); fitter.enabled = true;
            return rect;
        }

        private static void Bind(UnityEngine.Object owner, string field, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(owner);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AutoSize(TMP_Text text, float minimum, float maximum)
        {
            text.enableAutoSizing = true; text.fontSizeMin = minimum; text.fontSizeMax = maximum;
            text.textWrappingMode = TextWrappingModes.Normal;
        }
    }
}
