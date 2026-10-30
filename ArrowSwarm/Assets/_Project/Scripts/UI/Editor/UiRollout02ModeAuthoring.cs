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

    /// <summary>Authors board02's localized mode modal while retaining the existing Home and mode controller.</summary>
    internal static partial class UiRollout02ModeAuthoring
    {
        private const string Art = "Assets/_Project/Art/Sprites/ModeMenu/";
        private static readonly string[] Names = { "Defense", "Shapes", "ColorShift" };
        private static readonly string[] Keys = { "mode_defense", "mode_shapes", "mode_color_shift" };
        private static readonly string[] Fallbacks = { "DEFENSE", "SHAPES", "COLOR SHIFT" };

        internal static void Apply()
        {
            if (Application.isPlaying || SceneManager.GetActiveScene().path != "Assets/_Project/Scenes/MainMenuScene.unity")
                throw new InvalidOperationException("Open MainMenuScene in Edit Mode before authoring board02 modes.");
            var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>(FindObjectsInactive.Include);
            if (menu == null) throw new InvalidOperationException("Existing MainMenuUI is missing.");
            var menuFields = new SerializedObject(menu);
            var home = menuFields.FindProperty("_mainPanel").objectReferenceValue as GameObject;
            var selector = home != null ? home.GetComponent<GameModeSelectUI>() : null;
            if (selector == null) throw new InvalidOperationException("Existing Home mode controller is missing.");
            var fields = new SerializedObject(selector);
            var group = fields.FindProperty("_group").objectReferenceValue as CanvasGroup;
            var closeSource = fields.FindProperty("_close").objectReferenceValue as Button;
            if (group == null || fields.FindProperty("_selectionHighlights") == null)
                throw new InvalidOperationException("Mode overlay and selection-highlight binding must exist.");
            var sources = new Button[3]; var light = new Sprite[3]; var dark = new Sprite[3];
            for (int i = 0; i < 3; i++)
            {
                if (fields.FindProperty("_choices").arraySize > i)
                    sources[i] = fields.FindProperty("_choices").GetArrayElementAtIndex(i).objectReferenceValue as Button;
                light[i] = RequiredSprite(Art + "LocalizedSlices/Card" + i + ".asset");
                dark[i] = RequiredSprite(Art + "ModalNightSlices/Card" + i + ".asset");
            }
            var closeSprite = RequiredSprite(Art + "LocalizedSlices/Close.asset");
            var ring = SelectionMaterial();
            var overlay = group.transform;
            var previous = new GameObject[overlay.childCount];
            for (int i = 0; i < previous.Length; i++) previous[i] = overlay.GetChild(i).gameObject;

            var dim = K.Image(overlay, "ModeDimmer", null, Vector2.zero, Vector2.zero);
            dim.rectTransform.anchorMin = Vector2.zero; dim.rectTransform.anchorMax = Vector2.one;
            dim.rectTransform.offsetMin = dim.rectTransform.offsetMax = Vector2.zero;
            dim.color = new Color(.015f, .035f, .08f, .58f); dim.raycastTarget = true;
            var fit = U.SafeWrapper(overlay, "ModeSafeFit", new Vector2(1060, 1900));
            var motion = K.Rect(fit, "ModeDialogMotion", Vector2.zero, new Vector2(1060, 1900));
            U.PopupCrest(motion, new Vector2(0, 795), new Vector2(790, 290));
            var frame = K.Image(motion, "ModalFrame", K.Sprite("light_plate"), new Vector2(0, -70), new Vector2(1000, 1590));
            U.Plate(frame);
            var title = U.Text(motion, "Title", "GAME MODES", new Vector2(-35, 614), new Vector2(780, 132), 94, "modes_title");
            title.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/LilitaOne_SDF.asset");
            title.fontSharedMaterial = K.Material("TitleBlue");
            K.Theme(title, Color.white, Color.white);
            var star = K.Image(motion, "HeaderStar", K.Sprite("star"), new Vector2(-438, 614), new Vector2(52, 58));
            star.preserveAspect = true;
            var close = U.Button(motion, "Close", closeSprite, new Vector2(425, 619), new Vector2(112, 112), closeSource, false);
            close.GetComponent<Image>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Theme/DarkCloseArtwork.mat");
            close.gameObject.AddComponent<UIThemeCoordinates>();
            var choices = new UnityEngine.Object[3]; var labels = new UnityEngine.Object[3]; var highlights = new UnityEngine.Object[3];
            for (int i = 0; i < 3; i++)
            {
                var card = Card(motion, i, light[i], dark[i], sources[i], ring, out TMP_Text label, out GameObject selected);
                choices[i] = card; labels[i] = label; highlights[i] = selected;
            }
            U.Bind(selector, "_group", group); U.Bind(selector, "_dialog", motion); U.Bind(selector, "_close", close);
            U.BindArray(selector, "_choices", choices); U.BindArray(selector, "_choiceLabels", labels);
            U.BindArray(selector, "_selectionHighlights", highlights);
            // Preserve Home's open button, labels and icon bindings. Only replace the modal's own children.
            foreach (var old in previous) UnityEngine.Object.DestroyImmediate(old);
            var oldDim = overlay.GetComponent<Image>();
            if (oldDim != null) { oldDim.enabled = false; oldDim.raycastTarget = false; }
            group.alpha = 0; group.interactable = group.blocksRaycasts = false;
            EditorUtility.SetDirty(selector);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static Sprite RequiredSprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path)
            ?? throw new InvalidOperationException("Required reusable mode sprite missing: " + path);

        /// <summary>One small shared outline material also serves Settings' authored selection highlights.</summary>
        internal static Material SelectionMaterial()
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Project/Art/Shaders/SharedSelectionRing.shader");
            if (shader == null) throw new InvalidOperationException("Import SharedSelectionRing.shader before authoring board02.");
            string path = K.MaterialFolder + "/SelectionGold.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(shader) { name = "SelectionGold" }; AssetDatabase.CreateAsset(material, path); }
            material.shader = shader; material.SetColor("_Color", Color.white);
            material.SetColor("_TopColor", new Color(1, .97f, .42f));
            material.SetColor("_BottomColor", new Color(1, .53f, .015f));
            material.SetFloat("_Radius", .20f); material.SetFloat("_Thickness", .018f);
            EditorUtility.SetDirty(material); AssetDatabase.SaveAssetIfDirty(material);
            return material;
        }
    }
}
