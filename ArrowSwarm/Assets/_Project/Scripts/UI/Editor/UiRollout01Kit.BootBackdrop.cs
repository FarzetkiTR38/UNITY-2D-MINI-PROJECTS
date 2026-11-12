namespace ArrowSwarm.UI.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Boot-only atmosphere and motifs; the Home backdrop and shared source materials remain untouched.</summary>
    internal static partial class UiRollout01Kit
    {
        internal static void BootBackdrop(Transform parent)
        {
            if (Application.isPlaying) throw new InvalidOperationException("Author Boot atmosphere only in Edit Mode.");
            var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Project/Art/Shaders/BootMenuBackdrop.shader");
            var arrow = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/Arrows/arrow_placeholder.png");
            if (shader == null || arrow == null) throw new InvalidOperationException("Boot backdrop shader or existing arrow sprite is missing.");
            EnsureFolder(MaterialFolder);
            var light = ConfigureBootBackdrop("BootBackdropLight", shader, false);
            var dark = ConfigureBootBackdrop("BootBackdropDark", shader, true);
            ConfigureBootTip();
            Backdrop(parent);
            var background = parent.Find("SharedBackdrop").GetComponent<Image>();
            background.material = light;
            Theme(background, Color.white, Color.white, dark);
            RetargetBootMotifs(background.transform, arrow);
            var cloud = background.transform.Find("SharedCloudFooter");
            if (cloud != null) Theme(cloud.GetComponent<Image>(), Color.white, new Color(.16f, .40f, .82f, 1));
        }

        private static Material ConfigureBootBackdrop(string name, Shader shader, bool dark)
        {
            var material = NewMaterial(name, shader);
            material.SetColor("_Color", Color.white);
            material.SetColor("_TopColor", dark ? new Color(0, .094f, .239f) : new Color(.992f, .949f, .871f));
            material.SetColor("_BottomColor", dark ? new Color(0, .137f, .325f) : new Color(1, .965f, .906f));
            material.SetColor("_UpperGlowColor", dark ? new Color(.027f, .333f, .659f) : new Color(1, .988f, .945f));
            material.SetColor("_LowerGlowColor", dark ? new Color(0, .54f, .84f) : new Color(1, .902f, .639f));
            material.SetVector("_UpperGlow", new Vector4(.5f, .78f, .67f, .32f));
            material.SetVector("_LowerGlow", new Vector4(.5f, .42f, .62f, .29f));
            material.SetVector("_GlowStrengths", new Vector4(.28f, dark ? .16f : .11f, 0, 0));
            material.SetColor("_EdgeColor", dark ? new Color(0, .065f, .18f) : new Color(.957f, .91f, .82f));
            material.SetFloat("_EdgeStrength", dark ? .10f : .055f);
            Save(material); return material;
        }

        private static void ConfigureBootTip()
        {
            var source = Material("DarkPanel");
            var tip = NewMaterial("BootTipDark", source.shader);
            tip.CopyPropertiesFromMaterial(source);
            tip.SetFloat("_SolidFill", 1);
            tip.SetColor("_SurfaceColor", new Color(.015f, .08f, .245f, 1));
            Save(tip);
        }

        private static void RetargetBootMotifs(Transform parent, Sprite sprite)
        {
            var positions = new[] { new Vector2(.07f, .94f), new Vector2(.91f, .90f), new Vector2(.05f, .77f),
                new Vector2(.95f, .68f), new Vector2(.06f, .56f), new Vector2(.94f, .46f),
                new Vector2(.05f, .33f), new Vector2(.94f, .30f) };
            float[] sizes = { 62, 74, 48, 66, 88, 56, 50, 76 };
            float[] angles = { -35, 40, -60, 25, -10, 75, 30, -50 };
            float[] alphas = { .40f, .38f, .28f, .34f, .36f, .28f, .30f, .35f };
            for (int i = 0; i < positions.Length; i++)
            {
                var old = parent.Find("BackdropRoute" + i);
                var motif = old != null ? old.GetComponent<Image>() : Image(parent, "BackdropRoute" + i, sprite, Vector2.zero, Vector2.zero);
                motif.sprite = sprite; motif.material = null; motif.type = UnityEngine.UI.Image.Type.Simple;
                motif.preserveAspect = true; motif.raycastTarget = false;
                var rect = motif.rectTransform;
                rect.anchorMin = rect.anchorMax = positions[i]; rect.pivot = new Vector2(.5f, .5f);
                rect.anchoredPosition = Vector2.zero; rect.sizeDelta = Vector2.one * sizes[i];
                rect.localEulerAngles = new Vector3(0, 0, angles[i]); rect.SetSiblingIndex(i);
                // Equal theme alpha intentionally respects the existing preserve-alpha theme contract.
                Theme(motif, new Color(.08f, .70f, 1f, alphas[i]), new Color(.03f, .55f, 1f, alphas[i]));
            }
            float[] starSizes = { 18, 26, 21, 32, 16, 24 };
            for (int i = 0; i < starSizes.Length; i++)
            {
                var star = parent.Find("BackdropStar" + i);
                if (star == null) continue;
                star.GetComponent<RectTransform>().sizeDelta = Vector2.one * starSizes[i];
                star.localEulerAngles = new Vector3(0, 0, i * 17);
                Theme(star.GetComponent<Image>(), new Color(1, .90f, .62f, .44f), new Color(1, .91f, .62f, .44f));
            }
        }
    }
}
