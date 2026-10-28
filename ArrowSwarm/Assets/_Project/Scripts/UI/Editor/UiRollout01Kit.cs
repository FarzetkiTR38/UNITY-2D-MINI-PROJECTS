namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Linq;
    using ArrowSwarm.Localization;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Editor-only building blocks shared by the Boot and Home scene-authored designs.</summary>
    internal static partial class UiRollout01Kit
    {
        internal const string ArtFolder = "Assets/_Project/Art/Sprites/SharedUI";
        internal const string MaterialFolder = "Assets/_Project/Art/Materials/SharedUI";
        internal const string BootLogoPath = "Assets/_Project/Art/_FarzetkiArts/Logos/ChatGPT Image 9 Eyl 2026 18_42_19 (3)-Photoroom.png";
        private const string FontPath = "Assets/_Project/Fonts/Fazo_Font.asset";
        private const string TitleFontPath = "Assets/_Project/Fonts/LilitaOne_SDF.asset";
        private static readonly Color Ink = new Color(.025f, .16f, .57f);
        private static readonly Color Ice = new Color(.82f, .93f, 1f);

        /// <summary>Loads one reusable sprite; callers never duplicate its underlying texture.</summary>
        internal static Sprite Sprite(string name)
        {
            if (name == "star") return StarSprite;
            string path = name == "boot_logo" ? BootLogoPath :
                name == "profile_avatar" ? ArtFolder + "/profile_avatar.asset" : ArtFolder + "/" + name + ".png";
            var direct = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderByDescending(s => s.rect.width * s.rect.height).FirstOrDefault();
            if (direct != null) return direct;
            var slice = AssetDatabase.LoadAllAssetsAtPath(ArtFolder + "/shared_ui_kit.png").OfType<Sprite>().FirstOrDefault(s => s.name == name);
            if (slice == null) throw new InvalidOperationException("Import shared UI artwork before authoring: " + name);
            return slice;
        }

        internal static Sprite StarSprite => AssetDatabase.LoadAllAssetsAtPath(ArtFolder + "/star_gold.png")
            .OfType<Sprite>().OrderByDescending(s => s.rect.width * s.rect.height).FirstOrDefault()
            ?? throw new InvalidOperationException("Shared 256px gold star is missing.");

        internal static RectTransform Rect(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = position; rect.sizeDelta = size;
            return rect;
        }

        internal static Image Image(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 size)
        {
            var image = Rect(parent, name, position, size).gameObject.AddComponent<Image>();
            image.sprite = sprite; image.color = Color.white; image.raycastTarget = false;
            return image;
        }

        /// <summary>Creates live copy using one shared TMP material, with optional existing localization binding.</summary>
        internal static TextMeshProUGUI Text(Transform parent, string name, string text, Vector2 position,
            Vector2 size, float fontSize, Color color, string localizationKey = null)
        {
            var label = Rect(parent, name, position, size).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            label.fontSharedMaterial = Material("Body");
            label.text = text; label.fontSize = fontSize; label.color = color;
            label.alignment = TextAlignmentOptions.Center; label.fontStyle = FontStyles.Normal;
            label.enableAutoSizing = false; label.richText = false; label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.Normal; label.overflowMode = TextOverflowModes.Overflow;
            if (!string.IsNullOrEmpty(localizationKey))
            {
                var localized = label.gameObject.AddComponent<LocalizedText>();
                var settings = new SerializedObject(localized);
                settings.FindProperty("_localizationKey").stringValue = localizationKey;
                settings.ApplyModifiedPropertiesWithoutUndo();
            }
            return label;
        }

        /// <summary>Stores both palettes in the scene; existing theme infrastructure owns runtime application.</summary>
        internal static void Theme(Graphic graphic, Color light, Color dark, Material darkMaterial = null)
        {
            graphic.color = light;
            var themed = graphic.GetComponent<ThemedUIElement>() ?? graphic.gameObject.AddComponent<ThemedUIElement>();
            var settings = new SerializedObject(themed);
            settings.FindProperty("_role").enumValueIndex = (int)ThemeRole.Custom;
            settings.FindProperty("_customLightColor").colorValue = light;
            settings.FindProperty("_customDarkColor").colorValue = dark;
            settings.FindProperty("_darkMaterial").objectReferenceValue = darkMaterial;
            settings.FindProperty("_preserveAlpha").boolValue = true;
            settings.FindProperty("_materialOnly").boolValue = false;
            settings.ApplyModifiedPropertiesWithoutUndo();
            if (darkMaterial != null && graphic is Image && graphic.GetComponent<UIThemeCoordinates>() == null)
                graphic.gameObject.AddComponent<UIThemeCoordinates>();
            var canvas = graphic.GetComponentInParent<Canvas>();
            if (canvas != null) canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
        }

        internal static void RoundedPlate(Image image, bool darkSurface)
        {
            image.type = UnityEngine.UI.Image.Type.Sliced; image.preserveAspect = false; image.pixelsPerUnitMultiplier = 1.35f;
            Theme(image, Color.white, Color.white, darkSurface ? Material("DarkPanel") : null);
        }

        internal static Material Material(string name) => AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/" + name + ".mat")
            ?? throw new InvalidOperationException("Import shared UI materials before authoring: " + name);
    }
}
