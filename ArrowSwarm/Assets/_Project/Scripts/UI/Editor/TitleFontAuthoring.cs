namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.TextCore.LowLevel;

    /// <summary>Builds the renamed native-weight Turkish title family and shared appearance presets in Edit Mode.</summary>
    public static class TitleFontAuthoring
    {
        private const string Root = "Assets/_Project/Fonts/";
        private const string Family = "Farzetki_Title_Font_H1";
        private const string Folder = Root + "Farzetki_Title_H1";

        /// <summary>Creates one static atlas and clones the existing six material presets without changing their styling.</summary>
        public static string Build()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Use idle Edit Mode.");
            string path = Root + Family + "_SDF.asset";
            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path) != null)
                throw new InvalidOperationException("The reviewed font already exists. Do not rebuild implicitly.");
            var source = AssetDatabase.LoadAssetAtPath<Font>(Root + Family + ".ttf");
            if (source == null) throw new InvalidOperationException("Import the renamed TTF first.");
            var characters = File.ReadAllText("Tools/Fonts/title-characters.txt");
            var font = TMP_FontAsset.CreateFontAsset(source, 90, 9, GlyphRenderMode.SDFAA, 1024, 2048,
                AtlasPopulationMode.Dynamic, false);
            string missing;
            if (!font.TryAddCharacters(characters, out missing, true))
                throw new InvalidOperationException("The single title atlas did not fit; missing count=" + missing.Length);
            font.name = Family + "_SDF";
            font.atlasPopulationMode = AtlasPopulationMode.Static;
            font.isMultiAtlasTexturesEnabled = false;
            font.fallbackFontAssetTable = new List<TMP_FontAsset>(
                AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root + "LilitaOne_SDF.asset").fallbackFontAssetTable);
            AssetDatabase.CreateAsset(font, path);
            var serialized = new SerializedObject(font);
            serialized.FindProperty("m_ClearDynamicDataOnBuild").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            font.atlasTexture.name = Family + " Atlas";
            AssetDatabase.AddObjectToAsset(font.atlasTexture, font);
            font.material.name = Family + " Material";
            AssetDatabase.AddObjectToAsset(font.material, font);
            EditorUtility.SetDirty(font); AssetDatabase.SaveAssetIfDirty(font);
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Root.TrimEnd('/'), "Farzetki_Title_H1");
            var entries = new List<string>();
            long fontId, textureId, materialId; string fontGuid, textureGuid, materialGuid;
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(font, out fontGuid, out fontId);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(font.atlasTexture, out textureGuid, out textureId);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(font.material, out materialGuid, out materialId);
            entries.Add("font=" + fontGuid + ":" + fontId);
            entries.Add("default=" + materialGuid + ":" + materialId);
            entries.Add("texture=" + textureGuid + ":" + textureId);
            foreach (string name in new[] { "Front", "Rim", "Extrusion", "Shadow", "TitleBlue", "TitleGold" })
            {
                string oldPath = name.StartsWith("Title", StringComparison.Ordinal)
                    ? "Assets/_Project/Art/Materials/SharedUI/" + name + ".mat" : Root + "LilitaOne_" + name + ".mat";
                var original = AssetDatabase.LoadAssetAtPath<Material>(oldPath);
                var copy = new Material(original) { name = Family + "_" + name };
                copy.mainTexture = font.atlasTexture;
                copy.SetFloat("_TextureHeight", 2048);
                AssetDatabase.CreateAsset(copy, Folder + "/" + name + ".mat");
                entries.Add(AssetDatabase.AssetPathToGUID(oldPath) + "=" + AssetDatabase.AssetPathToGUID(Folder + "/" + name + ".mat"));
            }
            string report = "Tools/Fonts/title-asset-map.txt";
            File.WriteAllLines(report, entries); AssetDatabase.SaveAssetIfDirty(font);
            return "PASS: " + characters.Length + " native characters, one 1024x2048 static atlas, six unchanged appearance presets; " + report;
        }
    }
}
