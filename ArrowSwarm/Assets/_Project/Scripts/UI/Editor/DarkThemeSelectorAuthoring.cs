namespace ArrowSwarm.UI.Editor
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>Restores the original illustrated sun/moon selector and removes the temporary text-only cover.</summary>
    public static class DarkThemeSelectorAuthoring
    {
        /// <summary>Removes the previous cover; SettingsUI continues to own the original state sprites.</summary>
        public static void Apply()
        {
            var settings = Object.FindFirstObjectByType<SettingsUI>(FindObjectsInactive.Include);
            if (settings == null) return;
            var image = new SerializedObject(settings).FindProperty("_themeSelectorImage").objectReferenceValue as UnityEngine.UI.Image;
            if (image == null) return;
            var artwork = image.transform.Find("DarkArtwork");
            if (artwork != null) Object.DestroyImmediate(artwork.gameObject);
            var cover = image.GetComponent<DarkThemeChoiceVisual>();
            if (cover != null) Object.DestroyImmediate(cover);
        }
    }
}
