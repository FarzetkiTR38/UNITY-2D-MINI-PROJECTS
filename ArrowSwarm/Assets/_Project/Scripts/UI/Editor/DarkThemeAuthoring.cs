namespace ArrowSwarm.UI.Editor
{
    using TMPro;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Authors theme bindings into scenes/prefabs; never discovers or styles UI at runtime.</summary>
    public static class DarkThemeAuthoring
    {
        /// <summary>Migrates the project's UI while preserving every authored light color and sprite.</summary>
        [MenuItem("ArrowSwarm/Theme/Author Dark Variants")]
        public static void Apply()
        {
            if (Application.isPlaying) throw new System.InvalidOperationException("Stop Play Mode before authoring themes.");
            DarkThemeMaterials.Create();
            var setup = EditorSceneManager.GetSceneManagerSetup();
            int bindings = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project/Prefabs/UI", "Assets/_Project/Prefabs/Core" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    bindings += Configure(root);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            foreach (var guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/_Project/Scenes" }))
            {
                var scene = EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guid));
                foreach (var root in scene.GetRootGameObjects()) bindings += Configure(root);
                DarkThemeSelectorAuthoring.Apply();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.RestoreSceneManagerSetup(setup);
            Debug.Log($"[ArrowSwarm] Authored {bindings} dark theme bindings. Light visuals preserved.");
        }

        private static int Configure(GameObject root)
        {
            int count = 0;
            ThemeLayoutRepairs.Apply(root);
            foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
                canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
            foreach (var graphic in root.GetComponentsInChildren<Graphic>(true))
            {
                if (graphic.transform.GetComponentInParent<DarkThemeChoiceVisual>() != null && graphic.name != "ThemeSelector") continue;
                var image = graphic as Image;
                var text = graphic as TextMeshProUGUI;
                var existing = graphic.GetComponent<ThemedUIElement>();
                Material material = null;
                Color dark = graphic.color;
                bool materialOnly = false;
                if (text != null)
                {
                    // 3D title layers have their own authored face/rim/extrusion materials.
                    if (text.GetComponentInParent<Casual3DTitle>() != null) continue;
                    if (existing == null && Mathf.Min(dark.r, dark.g, dark.b) > 0.65f) continue;
                    dark = Parse("E6F0FF");
                }
                else if (image != null)
                {
                    if (graphic.name == "TransitionOverlay") continue; // Wipe shader owns its palette.
                    if (image.sprite == null)
                    {
                        if (graphic.color.a <= 0.01f || graphic.color.maxColorComponent < 0.35f) continue;
                        dark = Parse("354B6C");
                    }
                    else
                    {
                        material = PickMaterial(image, existing);
                        if (material == null) continue; // Colored icons and effect textures are already theme-neutral.
                        materialOnly = material == DarkThemeMaterials.Control || image.GetComponent<LevelButtonUI>() != null;
                        dark = Color.white;
                        if (image.GetComponent<UIThemeCoordinates>() == null) image.gameObject.AddComponent<UIThemeCoordinates>();
                    }
                }
                else continue;
                var themed = existing != null ? existing : graphic.gameObject.AddComponent<ThemedUIElement>();
                var so = new SerializedObject(themed);
                so.FindProperty("_role").enumValueIndex = (int)ThemeRole.Custom;
                so.FindProperty("_customLightColor").colorValue = graphic.color;
                dark.a = graphic.color.a;
                so.FindProperty("_customDarkColor").colorValue = dark;
                so.FindProperty("_materialOnly").boolValue = materialOnly;
                so.FindProperty("_preserveAlpha").boolValue = true;
                so.FindProperty("_darkMaterial").objectReferenceValue = material;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(themed);
                count++;
            }
            foreach (var grid in root.GetComponentsInChildren<ArrowSwarm.Grid.GridManager>(true))
                if (grid.GetComponent<ArrowSwarm.Grid.MapContainerVisualizer>() == null)
                    grid.gameObject.AddComponent<ArrowSwarm.Grid.MapContainerVisualizer>();
            foreach(var field in root.GetComponentsInChildren<TMP_InputField>(true))
            {
                var original=field.selectionColor;
                var binding=field.GetComponent<ThemedInputField>();
                if(binding==null) binding=field.gameObject.AddComponent<ThemedInputField>();
                var data=new SerializedObject(binding);
                data.FindProperty("_lightSelectionColor").colorValue=original;
                data.ApplyModifiedPropertiesWithoutUndo();
            }
            foreach (var map in root.GetComponentsInChildren<ArrowSwarm.Grid.MapContainerVisualizer>(true))
            {
                var so = new SerializedObject(map);
                so.FindProperty("_darkCameraBackgroundColor").colorValue = Parse("0E1423");
                so.FindProperty("_darkOuterContainerColor").colorValue = Parse("131F34");
                so.FindProperty("_darkInnerGridColor").colorValue = Parse("20324F");
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            return count;
        }

        private static Material PickMaterial(Image image, ThemedUIElement existing)
        {
            string name = image.name;
            if (name == "Background" && image.transform.parent.name == "ZoomSlider")
                return FooterThemeAuthoring.ForSprite(image.sprite);
            if ((name == "BG" || name == "Background") && image.transform.parent.GetComponent<Canvas>() != null)
                return DarkThemeMaterials.Background;
            if (name == "ThemeSelector") return DarkThemeMaterials.Selector;
            if (name == "LockIcon") return DarkThemeMaterials.Lock;
            if (name == "PauseButton") return DarkThemeMaterials.Pause;
            if (name == "Handle") return DarkThemeMaterials.Thumb;
            if (name == "LoadingBadge") return DarkThemeMaterials.Control;
            if (name == "Image" && image.transform.parent.name == "NameInput") return DarkThemeMaterials.Control;
            if (image.GetComponent<TMP_InputField>() != null || name == "CountrySelector" || name == "TextBG") return DarkThemeMaterials.Field;
            if (image.GetComponent<LevelButtonUI>() != null) return DarkThemeMaterials.Control;
            if (image.GetComponent<LeaderboardEntryUI>() != null)
                return name == "Entry_1" ? DarkThemeMaterials.Gold : name == "Entry_2" ? DarkThemeMaterials.Silver : name == "Entry_3" ? DarkThemeMaterials.Bronze : DarkThemeMaterials.Row;
            if (name.StartsWith("SettingRow")) return DarkThemeMaterials.Row;
            if (name == "Footer" || name == "BottomBadge" || name == "ButtomIMG")
                return FooterThemeAuthoring.ForSprite(image.sprite) ?? DarkThemeMaterials.Ornament;
            if (name == "Bg" || name == "ProgressBar_Capsule" || name == "Banner" || name == "ActionBubble") return DarkThemeMaterials.Bar;
            if (image.GetComponent<Selectable>() != null || name.EndsWith("Toggle") || name == "LanguageSelector")
                return DarkThemeMaterials.Control;
            if (existing != null || name == "Footer" || name == "BottomBadge" || name == "ButtomIMG" || name == "TextBG" || name == "Badge" || name == "AdBadge" || name == "LoadingBadge" || name == "ProgressBar_Capsule" || name == "Banner" || name == "ActionBubble")
                return DarkThemeMaterials.Surface;
            // Baked cream ornaments in the boot footer and profile input.
            if (name == "Logo" && image.GetComponentInParent<BootLoadingUI>() != null && image.transform.parent.name == "Footer_Branding")
                return DarkThemeMaterials.Surface;
            if (name == "Image" && image.transform.parent.name == "NameInput") return DarkThemeMaterials.Surface;
            return null;
        }

        private static Color Parse(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color color);
            return color;
        }
    }
}
