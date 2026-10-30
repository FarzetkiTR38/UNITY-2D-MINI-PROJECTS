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

    internal static partial class UiRollout02SettingsAuthoring
    {
        /// <summary>Revises only the requested title, footer and theme illustrations on the existing Settings hierarchy.</summary>
        internal static void ApplyReview()
        {
            if (Application.isPlaying || SceneManager.GetActiveScene().path != "Assets/_Project/Scenes/MainMenuScene.unity")
                throw new InvalidOperationException("Open MainMenuScene in Edit Mode before revising Settings.");
            var owner = UnityEngine.Object.FindFirstObjectByType<SettingsUI>(FindObjectsInactive.Include);
            var fields = new SerializedObject(owner);
            var old = (TextMeshProUGUI)fields.FindProperty("_titleText").objectReferenceValue;
            var dimensional = old.GetComponentInParent<Casual3DTitle>();
            Transform header = dimensional != null ? dimensional.transform.parent : old.transform.parent;
            UnityEngine.Object.DestroyImmediate(dimensional != null ? dimensional.gameObject : old.gameObject);
            var front = U.Title(header, "TitleText", "SETTINGS", new Vector2(0, -10), new Vector2(630, 220), 120, "settings_title");
            U.Bind(owner, "_titleText", front);
            var footer = (Image)fields.FindProperty("_footerImage").objectReferenceValue;
            footer.rectTransform.sizeDelta = new Vector2(450, 222);
            footer.rectTransform.anchoredPosition = new Vector2(0, -745);
            footer.preserveAspect = true;
            var fit = owner.GetComponentInChildren<SafeAreaPanelFit>(true);
            var safe = new SerializedObject(fit);
            safe.FindProperty("_referenceExtent").vector2Value = new Vector2(1000, 1900);
            safe.ApplyModifiedPropertiesWithoutUndo();
            ThemeArtwork((Button)fields.FindProperty("_lightThemeButton").objectReferenceValue, false);
            ThemeArtwork((Button)fields.FindProperty("_darkThemeButton").objectReferenceValue, true);
            EditorUtility.SetDirty(owner);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static void ThemeArtwork(Button button, bool dark)
        {
            foreach (string name in new[] { "SunIcon", "MoonIcon", "ThemeArtwork" })
            {
                var previous = button.transform.Find(name);
                if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
            }
            var light = dark ? UiRollout02ReviewSettingsArt.MoonRichLight() : UiRollout02ReviewSettingsArt.SunRich();
            var night = dark ? UiRollout02ReviewSettingsArt.MoonRichDark() : light;
            var image = K.Image(button.transform, "ThemeArtwork", light, new Vector2(0, 25), new Vector2(110, 90));
            image.type = Image.Type.Simple; image.useSpriteMesh = true; image.preserveAspect = true;
            U.Variants(image, light, night);
        }
    }
}
