namespace ArrowSwarm.UI.Editor
{
    using System.IO;
    using UnityEngine;

    /// <summary>Reuses a procedural palette, sparse kit motifs and one shared cloud cutout in both scenes.</summary>
    internal static partial class UiRollout01Kit
    {
        internal static void Backdrop(Transform parent)
        {
            var background = Image(parent, "SharedBackdrop", null, Vector2.zero, Vector2.zero);
            var rect = background.rectTransform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero; rect.SetAsFirstSibling();
            background.material = Material("BackdropLight");
            Theme(background, Color.white, Color.white, Material("BackdropDark"));
            var positions = new[] { new Vector2(.07f, .91f), new Vector2(.92f, .92f), new Vector2(.12f, .71f),
                new Vector2(.93f, .66f), new Vector2(.055f, .46f), new Vector2(.94f, .31f), new Vector2(.08f, .15f) };
            for (int i = 0; i < positions.Length; i++)
            {
                var motif = Image(rect, "BackdropRoute" + i, Sprite("icon_play"), Vector2.zero, new Vector2(72, 72));
                motif.rectTransform.anchorMin = motif.rectTransform.anchorMax = positions[i];
                motif.rectTransform.localEulerAngles = new Vector3(0, 0, 45 + i * 53);
                Theme(motif, new Color(.25f, .75f, 1f, .18f), new Color(.22f, .65f, 1f, .23f));
            }
            for (int i = 0; i < 6; i++)
            {
                var star = Image(rect, "BackdropStar" + i, StarSprite, Vector2.zero, new Vector2(32, 32));
                star.rectTransform.anchorMin = star.rectTransform.anchorMax = new Vector2(i % 2 == 0 ? .05f : .96f, .08f + i * .145f);
                Theme(star, new Color(1, .86f, .55f, .35f), new Color(1, .87f, .59f, .42f));
            }
            if (File.Exists(ArtFolder + "/cloud_footer.png"))
            {
                var clouds = Image(rect, "SharedCloudFooter", Sprite("cloud_footer"), new Vector2(0, 80), new Vector2(1260, 410));
                clouds.rectTransform.anchorMin = clouds.rectTransform.anchorMax = new Vector2(.5f, 0);
                clouds.preserveAspect = true;
                Theme(clouds, Color.white, new Color(.12f, .30f, .59f, 1));
            }
        }
    }
}
