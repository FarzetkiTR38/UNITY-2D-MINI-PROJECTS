namespace ArrowSwarm.UI.Editor
{
    using System;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using K = UiRollout01Kit;

    /// <summary>Authors concept 01 in the already-open Boot scene without replacing its loading logic.</summary>
    internal static partial class UiRollout01BootAuthoring
    {
        private static readonly Color Ink = new Color(.025f, .16f, .57f);
        private static readonly Color Ice = new Color(.78f, .91f, 1f);

        /// <summary>Rebuilds only the Boot canvas artwork and rebinds the existing loading component.</summary>
        internal static void Apply()
        {
            if (Application.isPlaying || SceneManager.GetActiveScene().path != "Assets/_Project/Scenes/BootScene.unity")
                throw new InvalidOperationException("Open BootScene in Edit Mode before authoring concept 01.");
            var canvasObject = GameObject.Find("Canvas_Boot");
            var loading = canvasObject != null ? canvasObject.GetComponent<BootLoadingUI>() : null;
            if (loading == null) throw new InvalidOperationException("The original Boot canvas/loading component is missing.");
            // The shared kit already contains this glyph; avoid retaining another large texture only for a tip icon.
            Sprite bulb = K.Sprite("icon_bulb");
            foreach (string asset in new[] { "boot_logo", "orbit_hero", "primary_plate", "light_plate" }) K.Sprite(asset);

            for (int i = canvasObject.transform.childCount - 1; i >= 0; i--)
                UnityEngine.Object.DestroyImmediate(canvasObject.transform.GetChild(i).gameObject);
            K.Backdrop(canvasObject.transform);
            var background = canvasObject.GetComponentInChildren<Image>();
            var content = Content(canvasObject.transform);

            var crest = BootLogo(content);
            var hero = K.Image(content, "OrbitHero", K.Sprite("orbit_hero"), new Vector2(0, -145), new Vector2(930, 700));
            hero.preserveAspect = true;

            var status = K.Text(content, "StatusText", "Initializing systems...", new Vector2(0, -477), new Vector2(920, 48), 25, Ink);
            K.Theme(status, Ink, Ice);
            var progress = Progress(content, out Image fill, out TextMeshProUGUI percentage);
            var tip = Tip(content, bulb, out Image icon, out TextMeshProUGUI tipText);
            StudioCredit(content);

            Bind(loading, "_backgroundImage", background);
            Bind(loading, "_titleLogoImage", crest);
            Bind(loading, "_loadingBadgeImage", null);
            Bind(loading, "_centerHeroTransform", hero.rectTransform);
            Bind(loading, "_centerHeroImage", hero);
            Bind(loading, "_centerStarImage", null);
            Bind(loading, "_brandingLogoImage", null);
            Bind(loading, "_progressSlider", progress);
            Bind(loading, "_progressFillImage", fill);
            Bind(loading, "_percentText", percentage);
            Bind(loading, "_statusText", status);
            Bind(loading, "_tipText", tipText);
            Bind(loading, "_tipIcon", icon);
            Bind(loading, "_tipCanvasGroup", tip);
            foreach (var graphic in canvasObject.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
            EditorUtility.SetDirty(loading);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static RectTransform Content(Transform parent)
        {
            var rect = new GameObject("BootDesign", typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.sizeDelta = new Vector2(1080, 1920);
            var fit = rect.gameObject.AddComponent<ReferenceCanvasFit>();
            fit.enabled = false;
            var settings = new SerializedObject(fit);
            settings.FindProperty("_referenceSize").vector2Value = new Vector2(1080, 1920);
            settings.ApplyModifiedPropertiesWithoutUndo();
            fit.enabled = true;
            return rect;
        }

        private static void Bind(BootLoadingUI owner, string field, UnityEngine.Object value)
        {
            var settings = new SerializedObject(owner);
            var property = settings.FindProperty(field);
            if (property == null) throw new InvalidOperationException("Missing original Boot binding: " + field);
            property.objectReferenceValue = value;
            settings.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
