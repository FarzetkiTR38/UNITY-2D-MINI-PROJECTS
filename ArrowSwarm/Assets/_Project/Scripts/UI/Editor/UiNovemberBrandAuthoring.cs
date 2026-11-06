namespace ArrowSwarm.UI.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>Shares the approved glossy Boot logo with Home, preserving all other authored controls.</summary>
    public static class UiNovemberBrandAuthoring
    {
        /// <summary>Replaces only the separate Home crest and flat wordmark, without adding a texture.</summary>
        public static void Apply()
        {
            if (Application.isPlaying || SceneManager.GetActiveScene().name != "MainMenuScene")
                throw new InvalidOperationException("Open MainMenuScene in Edit Mode.");
            var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>(FindObjectsInactive.Include);
            var data = new SerializedObject(menu);
            var panel = (GameObject)data.FindProperty("_mainPanel").objectReferenceValue;
            var root = panel.transform.Find("MenuArt");
            foreach (string name in new[] { "BrandCrest", "Wordmark", "BrandLogo" })
            {
                var old = root.Find(name);
                if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            }
            var logo = UiRollout01Kit.Image(root, "BrandLogo", UiRollout01Kit.Sprite("boot_logo"),
                new Vector2(0, -54), new Vector2(940, 940));
            logo.rectTransform.anchorMin = logo.rectTransform.anchorMax = new Vector2(.5f, .70f);
            logo.preserveAspect = true; logo.useSpriteMesh = true; logo.raycastTarget = false;
            data.FindProperty("_titleText").objectReferenceValue = null;
            data.ApplyModifiedPropertiesWithoutUndo();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}
