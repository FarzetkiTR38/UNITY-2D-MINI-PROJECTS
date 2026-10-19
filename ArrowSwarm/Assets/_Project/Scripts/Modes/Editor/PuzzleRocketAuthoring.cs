namespace ArrowSwarm.Modes.Editor
{
    using ArrowSwarm.Puzzle;
    using UnityEditor;
    using UnityEngine;
    using U = ModeAuthoringUtility;

    /// <summary>Imports the generated skill sprite and authors one reusable scene effect.</summary>
    public static class PuzzleRocketAuthoring
    {
        public const string ArtPath = "Assets/_Project/Art/Sprites/UI/RescueRocket.png";

        /// <summary>Sets explicit mobile Sprite import settings without changing generated alpha.</summary>
        public static void Import()
        {
            AssetDatabase.ImportAsset(ArtPath, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(ArtPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f; importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false; importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = 512; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        /// <summary>Binds an editable fixed SpriteRenderer; pooled arrows remain the only extracted paths.</summary>
        public static void Bind(PuzzleSession session)
        {
            var old = session.transform.Find("Rescue Rocket FX");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            var root = new GameObject("Rescue Rocket FX"); root.transform.SetParent(session.transform, false);
            root.layer = LayerMask.NameToLayer("PuzzleBoard");
            var icon = root.AddComponent<SpriteRenderer>();
            icon.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtPath); icon.sortingOrder = 30; icon.enabled = false;
            icon.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(U.Data + "PuzzleStroke.mat");
            var effect = root.AddComponent<PuzzleRocketEffect>(); U.Set(effect, "_icon", icon);
            U.Set(session, "_rocketEffect", effect);
        }
    }
}
