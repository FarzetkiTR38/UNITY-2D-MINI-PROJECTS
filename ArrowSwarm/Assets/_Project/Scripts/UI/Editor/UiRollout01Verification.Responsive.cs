namespace ArrowSwarm.UI.Editor
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>Checks edge attachment and real screen-space separation, rather than containment alone.</summary>
    public static partial class UiRollout01Verification
    {
        private static Rect ScreenRect(RectTransform rect)
        {
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            var min = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
            var max = RectTransformUtility.WorldToScreenPoint(null, corners[2]);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private static float VerifyScreenRoot(Transform root)
        {
            Canvas.ForceUpdateCanvases();
            var fit = root.GetComponent<ReferenceCanvasFit>();
            Check(fit != null && new SerializedObject(fit).FindProperty("_expandSafeHeight").boolValue,
                "Screen uses independent safe-area anchors: " + root.name);
            var rect = (RectTransform)root; var bounds = ScreenRect(rect);
            Check(Mathf.Abs(bounds.yMin - Screen.safeArea.yMin) < 1 && Mathf.Abs(bounds.yMax - Screen.safeArea.yMax) < 1,
                "Screen reference fills the entire safe height: " + root.name);
            return bounds.width / rect.rect.width;
        }

        private static void VerifyBootAnchors(Transform canvas)
        {
            var root = canvas.Find("BootDesign"); float scale = VerifyScreenRoot(root);
            var logo = ScreenRect((RectTransform)root.Find("BootLogo"));
            Check(Mathf.Abs(Screen.safeArea.yMax - logo.yMax - 40 * scale) < 1,
                "Boot logo remains attached to its authored top inset after resize");
            var credit = ScreenRect((RectTransform)root.Find("StudioGames"));
            Check(Mathf.Abs(credit.yMin - Screen.safeArea.yMin - 58 * scale) < 1,
                "Entire Boot loading/credit group remains attached to the bottom safe inset");
        }

        private static void VerifyHomeAnchors(Transform panel)
        {
            var root = panel.Find("MenuArt"); float scale = VerifyScreenRoot(root);
            var profile = ScreenRect((RectTransform)root.Find("ProfileButton"));
            Check(Mathf.Abs(Screen.safeArea.yMax - profile.yMax - 74 * scale) < 1,
                "Home top HUD keeps its actual top safe-area inset on every aspect ratio");
            var wordmark = ScreenRect((RectTransform)root.Find("Wordmark"));
            var play = ScreenRect((RectTransform)root.Find("PlayButton"));
            Check(wordmark.yMin >= play.yMax + 10 * scale,
                "Home branding stays clear of the center-anchored Play control after resize");
        }
    }
}
