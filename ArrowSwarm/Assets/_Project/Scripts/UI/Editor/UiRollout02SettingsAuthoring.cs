namespace ArrowSwarm.UI.Editor
{
    using System;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using K = UiRollout01Kit;
    using U = UiRollout02Kit;

    /// <summary>Reauthors only Settings visuals; the existing controller retains every setting and callback.</summary>
    internal static partial class UiRollout02SettingsAuthoring
    {
        private sealed class Sources
        {
            internal Sprite Board, Footer, Close, Back;
            internal Sprite[] Icons = new Sprite[5];
            internal string Feedback;
        }

        internal static void Apply()
        {
            if (Application.isPlaying || SceneManager.GetActiveScene().path != "Assets/_Project/Scenes/MainMenuScene.unity")
                throw new InvalidOperationException("Open MainMenuScene in Edit Mode before authoring Settings.");
            var owner = UnityEngine.Object.FindFirstObjectByType<SettingsUI>(FindObjectsInactive.Include);
            if (owner == null) throw new InvalidOperationException("The existing Settings controller is missing.");
            var source = Capture(owner);
            CreateControlMaterials();
            K.Material("SelectionGold"); // Created by the preceding mode-selection authoring step.
            for (int i = owner.transform.childCount - 1; i >= 0; i--)
                UnityEngine.Object.DestroyImmediate(owner.transform.GetChild(i).gameObject);
            K.Backdrop(owner.transform);
            var safe = U.SafeWrapper(owner.transform, "SafeAreaFit", new Vector2(1000, 1840));
            U.PopupCrest(safe, new Vector2(0, 785), new Vector2(740, 270));
            var board = K.Image(safe, "BoardFrame", source.Board, new Vector2(0, -80), new Vector2(1000, 1580));
            U.Plate(board);
            var header = K.Rect(board.transform, "Header", new Vector2(0, 650), new Vector2(940, 130));
            Control(header, "BackButton", source.Back, new Vector2(-390, 0), new Vector2(110, 110), source.Feedback, false);
            var close = Control(header, "CloseButton", source.Close, new Vector2(390, 0), new Vector2(110, 110), source.Feedback, false);
            var title = U.Text(header, "TitleText", "SETTINGS", Vector2.zero, new Vector2(630, 122), 105, "settings_title");
            title.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/Farzetki_Title_Font_H1_SDF.asset");
            title.fontSharedMaterial = K.Material("TitleBlue");
            K.Theme(title, Color.white, Color.white);
            var rows = K.Rect(board.transform, "SettingsContainer", Vector2.zero, new Vector2(950, 1200));
            var rowNames = new[] { "SFX", "VFX", "Vibration", "Language", "Theme" };
            var keys = new[] { "settings_sfx", "settings_vfx", "settings_vibration", "settings_language", "settings_theme" };
            var fallbacks = new[] { "SFX", "VFX", "VIBRATION", "LANGUAGE", "THEME" };
            var rowCards = new Image[5]; var rowIcons = new Image[5];
            for (int i = 0; i < rowNames.Length; i++)
            {
                rowCards[i] = K.Image(rows, "SettingRow_" + rowNames[i], K.Sprite("light_plate"),
                    new Vector2(0, 410 - i * 230), new Vector2(910, 215));
                U.Plate(rowCards[i]);
                rowIcons[i] = K.Image(rowCards[i].transform, "Icon", source.Icons[i], new Vector2(-350, 0), new Vector2(155, 160));
                rowIcons[i].preserveAspect = true;
                var label = U.Text(rowCards[i].transform, "LabelText", fallbacks[i], new Vector2(-115, 0),
                    new Vector2(250, 150), 51, keys[i]);
                label.alignment = TextAlignmentOptions.MidlineLeft;
            }
            var sfx = Switch(rowCards[0].transform, source.Feedback);
            var vfx = Switch(rowCards[1].transform, source.Feedback);
            var vibration = Switch(rowCards[2].transform, source.Feedback);
            Language(rowCards[3].transform, source.Feedback, out Button previous, out Button next, out TextMeshProUGUI language);
            Theme(rowCards[4].transform, source.Feedback, out Button light, out Button dark, out GameObject[] highlights);
            var footer = K.Image(board.transform, "Footer", source.Footer, new Vector2(0, -690), new Vector2(380, 140));
            footer.preserveAspect = true;
            K.Theme(footer, Color.white, Color.white, K.Material("DarkPanel"));
            U.Bind(owner, "_canvasGroup", owner.GetComponent<CanvasGroup>());
            U.Bind(owner, "_dialogBox", board.rectTransform); U.Bind(owner, "_closeButton", close); U.Bind(owner, "_titleText", title);
            U.Bind(owner, "_sfxToggle", sfx); U.Bind(owner, "_vfxToggle", vfx); U.Bind(owner, "_vibrationToggle", vibration);
            U.Bind(owner, "_languageText", language); U.Bind(owner, "_prevLanguageButton", previous); U.Bind(owner, "_nextLanguageButton", next);
            U.Bind(owner, "_languageButton", null); U.Bind(owner, "_themeToggleButton", null); U.Bind(owner, "_themeSelectorImage", null);
            U.Bind(owner, "_lightThemeSprite", null); U.Bind(owner, "_darkThemeSprite", null);
            U.Bind(owner, "_lightThemeButton", light); U.Bind(owner, "_darkThemeButton", dark); U.BindArray(owner, "_themeHighlights", highlights);
            U.Bind(owner, "_boardImage", board); U.Bind(owner, "_footerImage", footer);
            for (int i = 0; i < rowNames.Length; i++)
            {
                string prefix = "_" + rowNames[i].ToLowerInvariant();
                U.Bind(owner, prefix + "RowCard", rowCards[i]); U.Bind(owner, prefix + "Icon", rowIcons[i]);
            }
            ApplyReview();
            EditorUtility.SetDirty(owner);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static Sources Capture(SettingsUI owner)
        {
            var data = new SerializedObject(owner);
            var oldBoard = (Image)data.FindProperty("_boardImage").objectReferenceValue;
            var oldClose = (Button)data.FindProperty("_closeButton").objectReferenceValue;
            var oldFooter = (Image)data.FindProperty("_footerImage").objectReferenceValue;
            if (oldBoard == null || oldClose == null || oldFooter == null) throw new InvalidOperationException("Original Settings artwork bindings are incomplete.");
            var source = new Sources { Board = oldBoard.sprite, Footer = oldFooter.sprite, Close = (oldClose.targetGraphic as Image)?.sprite };
            var oldBack = oldBoard.transform.Find("Header/BackButton");
            source.Back = oldBack != null ? oldBack.GetComponent<Image>().sprite : source.Close;
            var feedback = oldClose.GetComponent<UIButtonFeedback>();
            source.Feedback = feedback != null ? JsonUtility.ToJson(feedback) : null;
            string[] rowNames = { "SFX", "VFX", "Vibration", "Language", "Theme" };
            for (int i = 0; i < rowNames.Length; i++)
            {
                var icon = oldBoard.transform.Find("SettingsContainer/SettingRow_" + rowNames[i] + "/Icon");
                if (icon == null || icon.GetComponent<Image>().sprite == null) throw new InvalidOperationException("Original Settings icon missing: " + rowNames[i]);
                source.Icons[i] = icon.GetComponent<Image>().sprite;
            }
            return source;
        }
    }
}
