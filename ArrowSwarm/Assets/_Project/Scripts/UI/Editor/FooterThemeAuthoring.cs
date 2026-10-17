namespace ArrowSwarm.UI.Editor
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Bakes small semantic masks for footer plaques and the HUD's pale zoom track.</summary>
    public static class FooterThemeAuthoring
    {
        private const string Folder = "Assets/_Project/Art/Materials/Theme";
        private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

        /// <summary>Assigns shared footer materials to both scene instances and reusable prefabs.</summary>
        [MenuItem("ArrowSwarm/Theme/Repair Illustrated Footers")]
        public static void Apply()
        {
            if (Application.isPlaying) throw new System.InvalidOperationException("Stop Play Mode before authoring.");
            Materials.Clear();
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project/Prefabs/UI" }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var root = PrefabUtility.LoadPrefabContents(path);
                    try
                    {
                        if (Configure(root) > 0) PrefabUtility.SaveAsPrefabAsset(root, path);
                    }
                    finally { PrefabUtility.UnloadPrefabContents(root); }
                }
                foreach (var guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/_Project/Scenes" }))
                {
                    var scene = EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guid));
                    int count = 0;
                    foreach (var root in scene.GetRootGameObjects()) count += Configure(root);
                    if (count > 0) EditorSceneManager.SaveScene(scene);
                }
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
            Debug.Log($"[ArrowSwarm] Repaired illustrated footers with {Materials.Count} shared plaque masks.");
        }

        /// <summary>Returns an Inspector-authored material for a recognized footer sprite.</summary>
        internal static Material ForSprite(Sprite sprite)
        {
            if (sprite == null) return null;
            string name = sprite.name;
            string family = name.Contains("LeaderboardPanel_Assets (3)") ? "TrophyArrows" :
                name.Contains("01_07_36 (2)") ? "TrophyStars" :
                name.Contains("01_03_39 (8)_1") ? "SettingsGear" :
                name.Contains("LevelMenu_Sprites (2)-Photoroom_2") ? "LevelArrows" :
                name == "ArrowSwarm_Sprites (19)_0" ? "ZoomTrack" : null;
            if (family == null) return null;
            if (Materials.TryGetValue(family, out var cached)) return cached;
            string path = $"{Folder}/DarkFooter{family}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Project/Art/Shaders/DarkUI.shader"));
                AssetDatabase.CreateAsset(material, path);
            }
            var mask = BakePlateMask(sprite, family);
            ColorUtility.TryParseHtmlString(family == "ZoomTrack" ? "#3C5877" : "#233853", out var surface);
            material.SetColor("_SurfaceColor", surface);
            material.SetFloat("_UsePlateMask", 1);
            material.EnableKeyword("FOOTER_PLATE_MASK");
            material.SetTexture("_PlateMask", mask);
            material.SetFloat("_Artwork", 4);
            material.SetFloat("_AccentStrength", 1);
            material.SetFloat("_SurfaceStrength", 1);
            material.SetFloat("_OpaqueInterior", 0);
            material.SetFloat("_SolidFill", 0);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssetIfDirty(material);
            Materials[family] = material;
            return material;
        }

        private static int Configure(GameObject root)
        {
            int count = 0;
            foreach (var image in root.GetComponentsInChildren<Image>(true))
            {
                if (image.name != "Footer" && image.name != "BottomBadge" && image.name != "ButtomIMG" &&
                    !(image.name == "Background" && image.transform.parent.name == "ZoomSlider")) continue;
                var material = ForSprite(image.sprite);
                var binding = image.GetComponent<ThemedUIElement>();
                if (material == null || binding == null) continue;
                var data = new SerializedObject(binding);
                data.FindProperty("_darkMaterial").objectReferenceValue = material;
                data.ApplyModifiedPropertiesWithoutUndo();
                count++;
            }
            return count;
        }

        private static Texture2D BakePlateMask(Sprite sprite, string family)
        {
            // Read through the GPU; source import settings and original artwork stay untouched.
            var rect = sprite.rect;
            int width = Mathf.Min(1024, Mathf.RoundToInt(rect.width));
            int height = Mathf.RoundToInt(width * rect.height / rect.width);
            var target = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var previous = RenderTexture.active;
            Graphics.Blit(sprite.texture, target,
                new Vector2(rect.width / sprite.texture.width, rect.height / sprite.texture.height),
                new Vector2(rect.x / sprite.texture.width, rect.y / sprite.texture.height));
            RenderTexture.active = target;
            var source = new Texture2D(width, height, TextureFormat.RGBA32, false);
            source.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            source.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
            var pixels = source.GetPixels();
            Object.DestroyImmediate(source);
            var candidates = new bool[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                var c = pixels[i];
                float hi = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
                float lo = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
                candidates[i] = c.a > 0.15f && hi > 0.15f && (hi - lo) / hi < 0.48f;
            }
            var visited = new bool[pixels.Length];
            var plate = new List<int>();
            for (int i = 0; i < pixels.Length; i++)
            {
                if (!candidates[i] || visited[i]) continue;
                var component = Flood(i, candidates, visited, width, height);
                if (component.Count > plate.Count) plate = component;
            }
            // The plate visible through a gear/cup handle is also background, not a white glint.
            Vector2[] holes = family == "SettingsGear" ? new[] { new Vector2(0.5f, 0.5f) } :
                family == "TrophyArrows" ? new[] { new Vector2(0.435f, 0.616f), new Vector2(0.599f, 0.616f) } :
                family == "TrophyStars" ? new[] { new Vector2(0.389f, 0.634f), new Vector2(0.610f, 0.634f) } : new Vector2[0];
            foreach (var seed in holes)
            {
                int index = Mathf.RoundToInt(seed.y * (height - 1)) * width + Mathf.RoundToInt(seed.x * (width - 1));
                if (candidates[index]) plate.AddRange(Flood(index, candidates, new bool[pixels.Length], width, height));
            }
            var values = new Color32[pixels.Length];
            foreach (int index in plate) values[index] = new Color32(255, 255, 255, 255);
            string path = $"{Folder}/Footer{family}PlateMask.asset";
            var mask = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var generated = new Texture2D(width, height, TextureFormat.R8, false, true);
            generated.name = $"Footer{family}PlateMask";
            generated.wrapMode = TextureWrapMode.Clamp;
            generated.filterMode = FilterMode.Bilinear;
            generated.SetPixels32(values);
            generated.Apply(false, true);
            if (mask == null)
            {
                mask = generated;
                AssetDatabase.CreateAsset(mask, path);
            }
            else
            {
                EditorUtility.CopySerialized(generated, mask);
                Object.DestroyImmediate(generated);
            }
            EditorUtility.SetDirty(mask);
            AssetDatabase.SaveAssetIfDirty(mask);
            return mask;
        }

        private static List<int> Flood(int seed, bool[] candidates, bool[] visited, int width, int height)
        {
            var component = new List<int>();
            var queue = new Queue<int>();
            queue.Enqueue(seed);
            visited[seed] = true;
            while (queue.Count > 0)
            {
                int index = queue.Dequeue();
                component.Add(index);
                int x = index % width;
                int y = index / width;
                if (x > 0) Visit(index - 1, candidates, visited, queue);
                if (x + 1 < width) Visit(index + 1, candidates, visited, queue);
                if (y > 0) Visit(index - width, candidates, visited, queue);
                if (y + 1 < height) Visit(index + width, candidates, visited, queue);
            }
            return component;
        }

        private static void Visit(int index, bool[] candidates, bool[] visited, Queue<int> queue)
        {
            if (!candidates[index] || visited[index]) return;
            visited[index] = true;
            queue.Enqueue(index);
        }
    }
}
