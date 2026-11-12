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

    /// <summary>Restores only the original compact joined theme control and flush Settings ornament.</summary>
    internal static partial class UiRollout02SettingsAuthoring
    {
        internal static void ApplyLowerReference()
        {
            if (Application.isPlaying || SceneManager.GetActiveScene().path != "Assets/_Project/Scenes/MainMenuScene.unity")
                throw new InvalidOperationException("Open MainMenuScene in Edit Mode.");
            var owner = UnityEngine.Object.FindFirstObjectByType<SettingsUI>(FindObjectsInactive.Include);
            var fields = new SerializedObject(owner);
            var row = (Image)fields.FindProperty("_themeRowCard").objectReferenceValue;
            var old = row.transform.Find("ThemeSelector");
            var feedback = old.GetComponentInChildren<UIButtonFeedback>(true);
            string feedbackData = feedback != null ? JsonUtility.ToJson(feedback) : null;
            UnityEngine.Object.DestroyImmediate(old.gameObject);
            var light = UiRollout02SettingsSelectorArt.Get(false); var dark = UiRollout02SettingsSelectorArt.Get(true);
            var art = K.Image(row.transform, "ThemeSelector", light, new Vector2(245, 0), new Vector2(350, 150));
            art.type = Image.Type.Simple; art.useSpriteMesh = true;
            art.material = UiRollout02SettingsSelectorArt.Paint(false);
            K.Theme(art, Color.white, Color.white, UiRollout02SettingsSelectorArt.Paint(true));
            var buttons = new Button[2];
            for (int i = 0; i < 2; i++)
            {
                var button = U.Button(art.transform, i == 0 ? "LightButton" : "DarkButton", null,
                    new Vector2(i == 0 ? -87.5f : 87.5f, 0), new Vector2(175, 150), null, false);
                ((Image)button.GetComponent<Image>()).color = Color.clear;
                button.targetGraphic = art;
                var squeeze = button.GetComponent<UIButtonFeedback>();
                if (feedbackData != null) JsonUtility.FromJsonOverwrite(feedbackData, squeeze);
                U.Bind(squeeze, "_targetTransform", art.transform);
                var label = U.Text(button.transform, "Label", i == 0 ? "LIGHT" : "DARK", new Vector2(0, -40),
                    new Vector2(146, 39), 29, i == 0 ? "settings_theme_light" : "settings_theme_dark");
                label.textWrappingMode = TextWrappingModes.NoWrap;
                buttons[i] = button;
            }
            U.Bind(owner, "_themeSelectorImage", art); U.Bind(owner, "_lightThemeSprite", light); U.Bind(owner, "_darkThemeSprite", dark);
            U.Bind(owner, "_themeToggleButton", null); U.Bind(owner, "_lightThemeButton", buttons[0]); U.Bind(owner, "_darkThemeButton", buttons[1]);
            U.BindArray(owner, "_themeHighlights", Array.Empty<UnityEngine.Object>());
            var board = (Image)fields.FindProperty("_boardImage").objectReferenceValue;
            board.sprite = UiRollout02SettingsBoardArt.Get(); board.type = Image.Type.Simple; board.useSpriteMesh = true;
            var footer = (Image)fields.FindProperty("_footerImage").objectReferenceValue;
            footer.rectTransform.anchorMin = footer.rectTransform.anchorMax = new Vector2(.5f, 0);
            footer.rectTransform.anchoredPosition = new Vector2(0, 83);
            footer.rectTransform.sizeDelta = new Vector2(337, 166); footer.preserveAspect = true;
            K.Theme(footer, Color.white, Color.white, AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Theme/DarkFooterSettingsGear.mat"));
            EditorUtility.SetDirty(owner);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}
