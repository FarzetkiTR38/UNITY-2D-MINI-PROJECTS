namespace ArrowSwarm.UI.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Authors independent safe-area anchors while preserving the approved 1080x1920 composition.</summary>
    internal static class UiRolloutResponsiveAuthoring
    {
        internal static void Home(Transform panel)
        {
            var root = panel.Find("MenuArt"); Expand(root);
            foreach (string name in new[] { "LevelCounter", "StarCounter", "ProfileButton" }) Anchor(root, name, 1, 822);
            if (root.Find("BrandLogo") != null) Anchor(root, "BrandLogo", .70f, 330);
            else { Anchor(root, "BrandCrest", .70f, 470); Anchor(root, "Wordmark", .70f, 115); }
            Anchor(root, "PlayButton", .5f, -215); Anchor(root, "GameModeButton", .5f, -435);
            foreach (string name in new[] { "LevelsButton", "LeaderboardButton", "SettingsButton" }) Anchor(root, name, .5f, -730);
        }

        internal static void Boot(Transform canvas)
        {
            var root = canvas.Find("BootDesign"); Expand(root);
            Anchor(root, "BootLogo", 1, 500); Anchor(root, "OrbitHero", .5f, -145);
            Anchor(root, "StatusText", 0, -477); Anchor(root, "ProgressTrack", 0, -567);
            Anchor(root, "TipCard", 0, -725); Anchor(root, "StudioBrand", 0, -835);
            Anchor(root, "StudioGames", 0, -884);
            foreach (string name in new[] { "StudioStarLeft", "StudioStarRight" }) Anchor(root, name, 0, -838);
            foreach (string name in new[] { "StudioRuleLeft", "StudioRuleRight" }) Anchor(root, name, 0, -884);
        }

        private static void Expand(Transform root)
        {
            if (Application.isPlaying || root == null) throw new InvalidOperationException("Author screen anchors on an existing Edit Mode canvas.");
            var fit = root.GetComponent<ReferenceCanvasFit>();
            if (fit == null) throw new InvalidOperationException("Screen reference fitter is missing.");
            fit.enabled = false;
            var data = new SerializedObject(fit); data.FindProperty("_expandSafeHeight").boolValue = true;
            data.ApplyModifiedPropertiesWithoutUndo(); fit.enabled = true;
        }

        private static void Anchor(Transform root, string name, float y, float referenceY)
        {
            var rect = root.Find(name) as RectTransform;
            if (rect == null) throw new InvalidOperationException("Missing authored screen element: " + name);
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, y);
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, referenceY - (y - .5f) * 1920);
            EditorUtility.SetDirty(rect);
        }
    }
}
