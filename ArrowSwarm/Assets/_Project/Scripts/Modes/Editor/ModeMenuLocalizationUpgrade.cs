namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.IO;
    using System.Linq;
    using ArrowSwarm.Localization;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;
    using U = ModeAuthoringUtility;

    /// <summary>Upgrades the loaded menu in place. The caller owns opening and saving the scene.</summary>
    public static class ModeMenuLocalizationUpgrade
    {
        private const string SliceRoot = U.Art + "LocalizedSlices/";
        private const string MaterialPath = SliceRoot + "LocalizedMenuOutline.mat";
        private const string MenuFontPath = "Assets/_Project/Fonts/Fazo_Font.asset";
        private static readonly Color Ink = new Color(.015f, .12f, .55f);
        private static TMP_FontAsset MenuFont => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(MenuFontPath)
            ?? throw new InvalidOperationException("The authored gameplay menu font is missing: " + MenuFontPath);

        [MenuItem("Arrow Swarm/Puzzles/Upgrade Loaded Menu Localization And Feedback")]
        public static void UpgradeLoadedScene()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode before authoring menu UI.");
            var selector = UnityEngine.Object.FindFirstObjectByType<GameModeSelectUI>(FindObjectsInactive.Include);
            if (selector == null) throw new InvalidOperationException("Open MainMenuScene with the reference menu first.");
            var main = selector.transform;
            var menuArt = main.Find("MenuArt");
            var overlay = main.Find("GameModeOverlay");
            var design = overlay != null ? overlay.Find("ModalDesign") : null;
            if (menuArt == null || design == null) throw new InvalidOperationException("Reference menu hierarchy is incomplete.");
            Directory.CreateDirectory(SliceRoot);
            AssetDatabase.Refresh();
            RequireSource("ModeModalTextless.png"); RequireSource("ModeModalFrame.png");
            RequireSource("MainMenuTextless.png"); RequireSource("MainMenuBackdrop.png");

            var menu = main.GetComponentInParent<MainMenuUI>();
            menuArt.GetComponent<Image>().sprite = Slice("MainMenuBackdrop.png", "Backdrop", 0, 0, 941, 1672);
            Remove(menuArt, "PlayButton", "GameModeButton", "LevelsButton", "LeaderboardButton", "SettingsButton", "SelectedMode");
            var play = MainButton(menuArt, "PlayButton", 128, 809, 687, 218);
            Label(play.transform, "Label", 214, 35, 410, 133, "menu_play", "PLAY", 101, Color.white, true);
            var open = MainButton(menuArt, "GameModeButton", 128, 1030, 688, 164);
            Label(open.transform, "Label", 285, 31, 286, 60, "menu_game_mode", "GAME MODE", 44, Ink);
            var selected = Label(open.transform, "SelectedMode", 287, 89, 286, 45, "mode_defense", "DEFENSE", 27, new Color(.11f,.44f,.9f));
            var levels = MainButton(menuArt, "LevelsButton", 151, 1200, 641, 132);
            Label(levels.transform, "Label", 183, 13, 421, 94, "menu_levels", "LEVELS", 49, Color.white, true);
            var leaderboard = MainButton(menuArt, "LeaderboardButton", 151, 1334, 641, 133);
            Label(leaderboard.transform, "Label", 182, 13, 421, 94, "menu_leaderboard", "LEADERBOARD", 45, Color.white, true);
            var settings = MainButton(menuArt, "SettingsButton", 151, 1467, 641, 133);
            Label(settings.transform, "Label", 183, 13, 421, 94, "menu_settings", "SETTINGS", 49, Color.white, true);

            foreach (Transform child in design.Cast<Transform>().ToArray()) UnityEngine.Object.DestroyImmediate(child.gameObject);
            U.Image(U.Rect("ModalFrame", design, 0, 0, 941, 1672), Slice("ModeModalFrame.png", "ModalFrame", 0, 0, 941, 1672), Color.white);
            Label(design, "Title", 110, 316, 654, 119, "modes_title", "GAME MODES", 88, Color.white, true);
            var close = VisibleButton("Close", design, 786, 312, 99, 99,
                Slice("ModeModalTextless.png", "Close", 786, 312, 99, 99), .92f);
            var choices = new Button[3]; var labels = new TMP_Text[3];
            var keys = new[] { "mode_defense", "mode_shapes", "mode_color_shift" };
            var names = new[] { "DEFENSE", "SHAPES", "COLOR SHIFT" };
            for (int i = 0; i < choices.Length; i++)
            {
                float y = 453 + i * 307;
                choices[i] = VisibleButton(((GameModeId)i) + "Card", design, 58, y, 827, 297,
                    Slice("ModeModalTextless.png", "Card" + i, 58, y, 827, 297), .97f);
                Label(choices[i].transform, "ModeTitle", 41, 20, i == 2 ? 487 : 350, 83, keys[i], names[i], 63, Ink);
                labels[i] = Label(choices[i].transform, "SelectionLabel", 548, 169, 235, 96,
                    i == 0 ? "modes_selected" : "modes_select", i == 0 ? "SELECTED" : "SELECT", 40, Color.white, true);
            }
            var level = menuArt.Find("SelectedLevel")?.GetComponent<TMP_Text>();
            if (level != null) { ConfigureStat(level); level.transform.SetAsLastSibling(); }
            var sharedStars = menuArt.Find("SharedStars")?.GetComponent<TMP_Text>();
            if (sharedStars != null) { ConfigureStat(sharedStars); sharedStars.transform.SetAsLastSibling(); }
            U.Set(menu, "_playButton", play); U.Set(menu, "_levelsButton", levels);
            U.Set(menu, "_leaderboardButton", leaderboard); U.Set(menu, "_settingsButton", settings);
            U.Set(selector, "_group", overlay.GetComponent<CanvasGroup>());
            U.Set(selector, "_dialog", design);
            U.Set(selector, "_open", open); U.Set(selector, "_close", close);
            U.Set(selector, "_choices", choices); U.Set(selector, "_choiceLabels", labels);
            U.Set(selector, "_modeLabel", selected); U.Set(selector, "_levelLabel", level);
            var group = overlay.GetComponent<CanvasGroup>();
            group.alpha = 0; group.interactable = group.blocksRaycasts = false;
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(main.gameObject.scene);
            Debug.Log("[ArrowSwarm] Menu localized: selector words and menu button labels use live TMP; visible images receive existing squeeze and tint feedback.");
        }

        private static Button MainButton(Transform parent, string name, float x, float y, float width, float height)
        { return VisibleButton(name, parent, x, y, width, height, Slice("MainMenuTextless.png", name, x, y, width, height), .92f); }

        private static Button VisibleButton(string name, Transform parent, float x, float y, float width, float height, Sprite sprite, float scale)
        {
            var rect = U.Rect(name, parent, x, y, width, height);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = new Vector2(x + width * .5f, -y - height * .5f);
            var image = U.Image(rect, sprite, Color.white, true);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image; button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = Color.white; colors.highlightedColor = new Color(.94f, .98f, 1f);
            colors.pressedColor = new Color(.72f, .80f, .93f); colors.selectedColor = Color.white;
            colors.disabledColor = new Color(.65f, .65f, .65f, .55f); colors.fadeDuration = .08f;
            button.colors = colors;
            var feedback = rect.gameObject.AddComponent<UIButtonFeedback>();
            U.Set(feedback, "_targetTransform", rect); U.Set(feedback, "_pressedScale", scale);
            U.Set(feedback, "_punchScale", scale > .95f ? 1.025f : 1.06f);
            return button;
        }

        private static TextMeshProUGUI Label(Transform parent, string name, float x, float y, float width, float height,
            string key, string fallback, float size, Color color, bool outlined = false)
        {
            var text = U.Text(U.Rect(name, parent, x, y, width, height), fallback, size, color);
            text.font = MenuFont;
            text.enableAutoSizing = true; text.fontSizeMin = size * .45f; text.fontSizeMax = size;
            text.textWrappingMode = TextWrappingModes.NoWrap; text.overflowMode = TextOverflowModes.Ellipsis;
            text.fontStyle = outlined ? FontStyles.Bold : FontStyles.Normal;
            if (outlined)
            {
                text.fontSharedMaterial = OutlineMaterial(); text.enableVertexGradient = true;
                text.colorGradient = new VertexGradient(Color.white, Color.white, new Color(.67f,.88f,1f), new Color(.67f,.88f,1f));
                text.extraPadding = true;
            }
            var localized = text.gameObject.AddComponent<LocalizedText>();
            U.Set(localized, "_localizationKey", key);
            return text;
        }

        private static void ConfigureStat(TMP_Text text)
        {
            float size = text.enableAutoSizing ? text.fontSizeMax : text.fontSize;
            text.font = MenuFont; text.fontSharedMaterial = MenuFont.material;
            text.enableAutoSizing = true; text.fontSizeMax = size; text.fontSizeMin = size * .4f;
            text.textWrappingMode = TextWrappingModes.NoWrap; text.overflowMode = TextOverflowModes.Ellipsis;
        }

        private static Material OutlineMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            bool create = material == null;
            var fontMaterial = MenuFont.material;
            if (create) material = new Material(fontMaterial) { name = "LocalizedMenuOutline" };
            else
            {
                material.shader = fontMaterial.shader;
                material.CopyPropertiesFromMaterial(fontMaterial);
            }
            material.EnableKeyword("OUTLINE_ON"); material.EnableKeyword("UNDERLAY_ON");
            material.SetColor(ShaderUtilities.ID_OutlineColor, new Color(.01f,.13f,.7f));
            material.SetFloat(ShaderUtilities.ID_OutlineWidth, .15f);
            material.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(.005f,.04f,.3f,.85f));
            material.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0); material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -.7f);
            material.SetFloat(ShaderUtilities.ID_UnderlayDilate, .18f);
            if (create) AssetDatabase.CreateAsset(material, MaterialPath);
            else EditorUtility.SetDirty(material);
            return material;
        }

        private static void Remove(Transform parent, params string[] names)
        {
            foreach (string name in names)
            {
                var child = parent.Find(name);
                if (child != null) UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }

        private static void RequireSource(string file)
        {
            string path = U.Art + file;
            if (!File.Exists(path)) throw new FileNotFoundException("Localized UI source is missing", path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer.textureType == TextureImporterType.Sprite && importer.maxTextureSize >= 2048 &&
                importer.textureCompression == TextureImporterCompression.Uncompressed) return;
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
            importer.maxTextureSize = 2048; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static Sprite Slice(string source, string name, float x, float top, float width, float height)
        {
            string path = SliceRoot + name + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(U.Art + source);
            // Normalize pixel sampling if editing preserved composition with a one-pixel dimension change.
            float sx = texture.width / 941f, sy = texture.height / 1672f;
            var sprite = Sprite.Create(texture, new Rect(x * sx, texture.height - (top + height) * sy, width * sx, height * sy),
                new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
            sprite.name = name; AssetDatabase.CreateAsset(sprite, path);
            return sprite;
        }
    }
}
