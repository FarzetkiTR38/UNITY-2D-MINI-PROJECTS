namespace ArrowSwarm.Modes.Editor
{
    using TMPro;
    using UnityEngine;

    /// <summary>Checks intentionally overflowing rank typography against its actual visible badge, preserving authored styling.</summary>
    public static partial class Revision6LeaderboardPreviewVerification
    {
        private static void VerifyRankBadge(TMP_Text text)
        {
            // Existing fixed 60px rank labels intentionally draw 70.3px glyphs into a 108px badge.
            // TMP's line-box overflow flag alone does not establish clipping of this authored layout.
            Check(FitsBadge(text), "visible rank glyph geometry fits its authored badge without clipping");
            float size = text.fontSize;
            try
            {
                text.fontSize = size * 4; text.ForceMeshUpdate();
                Check(!FitsBadge(text), "badge fit checker rejects a deliberately oversized rank glyph");
            }
            finally { text.fontSize = size; text.ForceMeshUpdate(); }
        }

        private static bool FitsBadge(TMP_Text text)
        {
            var parent = text.transform.parent as RectTransform;
            if (parent == null || text.overflowMode != TextOverflowModes.Overflow) return false;
            var visible = parent.rect; visible.xMin -= 1; visible.xMax += 1; visible.yMin -= 1; visible.yMax += 1;
            var bounds = text.textBounds;
            for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                {
                    var local = bounds.center + new Vector3(bounds.extents.x * x, bounds.extents.y * y, 0);
                    var point = parent.InverseTransformPoint(text.transform.TransformPoint(local));
                    if (!visible.Contains(point)) return false;
                }
            return bounds.size.x > 0 && bounds.size.y > 0;
        }
    }
}
