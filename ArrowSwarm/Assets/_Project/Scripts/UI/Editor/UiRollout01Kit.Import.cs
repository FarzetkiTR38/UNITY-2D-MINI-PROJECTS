namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using UnityEditor;
    using UnityEditor.U2D.Sprites;
    using UnityEngine;

    /// <summary>Imports reusable cutouts and twelve atlas slices with stable native sprite identities.</summary>
    internal static partial class UiRollout01Kit
    {
        /// <summary>Authors import/material assets only; does not open or save any scene.</summary>
        internal static void ImportAssets()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play before importing the UI kit.");
            foreach (string name in new[] { "brand_crest", "orbit_hero", "cloud_footer" })
                if (File.Exists(ArtFolder + "/" + name + ".png")) ConfigureTexture(ArtFolder + "/" + name + ".png", false, 1024);
            ConfigureTexture(ArtFolder + "/star_gold.png", false, 256);
            ImportAtlas(); CreateProfileAvatar(); CreateMaterials();
        }

        private static TextureImporter ConfigureTexture(string path, bool multiple, int maximum)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Shared UI texture missing: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = multiple ? SpriteImportMode.Multiple : SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100; importer.alphaIsTransparency = true; importer.sRGBTexture = true;
            importer.mipmapEnabled = false; importer.isReadable = false; importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp; importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = maximum; importer.textureCompression = TextureImporterCompression.CompressedHQ;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings);
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name = "Android", overridden = true, maxTextureSize = maximum,
                format = TextureImporterFormat.ASTC_6x6, compressionQuality = 70
            });
            importer.SaveAndReimport(); return importer;
        }

        private static void ImportAtlas()
        {
            var importer = ConfigureTexture(ArtFolder + "/shared_ui_kit.png", true, 2048);
            var factories = new SpriteDataProviderFactories(); factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var previous = provider.GetSpriteRects().ToDictionary(item => item.name);
            var rectangles = new List<SpriteRect>(); var identities = new List<SpriteNameFileIdPair>();
            var measured = JArray.Parse(File.ReadAllText("Captures/UIRollout/Phase01/shared-kit-slices.json"));
            foreach (var entry in measured)
            {
                string name = (string)entry["name"]; var values = entry["rectTopLeft"].Values<float>().ToArray();
                if (name == "profile_tile") values = new[] { 638f, 224f, 248f, 252f };
                var rect = new SpriteRect
                {
                    name = name, rect = new Rect(values[0], 1536 - values[1] - values[3], values[2], values[3]),
                    pivot = new Vector2(.5f, .5f), alignment = SpriteAlignment.Center,
                    spriteID = previous.TryGetValue(name, out var existing) ? existing.spriteID : GUID.Generate(),
                    border = name == "primary_plate" || name == "light_plate" ? new Vector4(90, 54, 90, 54) :
                        name == "nav_tile" ? new Vector4(60, 60, 60, 60) : Vector4.zero
                };
                rectangles.Add(rect); identities.Add(new SpriteNameFileIdPair(name, rect.spriteID));
            }
            provider.SetSpriteRects(rectangles.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(identities);
            provider.Apply(); importer.SaveAndReimport();
        }

        private static void CreateProfileAvatar()
        {
            var texture = Sprite("brand_crest").texture;
            var rect = new Rect(texture.width * .365f, texture.height * .25f, texture.width * .32f, texture.height * .47f);
            string path = ArtFolder + "/profile_avatar.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null && existing.texture == texture && existing.rect == rect) return;
            var avatar = UnityEngine.Sprite.Create(texture, rect, new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
            avatar.name = "profile_avatar";
            if (existing == null) AssetDatabase.CreateAsset(avatar, path);
            else
            {
                EditorUtility.CopySerialized(avatar, existing); UnityEngine.Object.DestroyImmediate(avatar);
                EditorUtility.SetDirty(existing); AssetDatabase.SaveAssetIfDirty(existing);
            }
        }
    }
}
