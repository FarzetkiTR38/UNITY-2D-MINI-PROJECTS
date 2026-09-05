#if UNITY_EDITOR
namespace ArrowSwarm.Editor
{
    using System.Collections.Generic;
    using TMPro;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Editor utility to generate Dynamic SDF font assets for Japanese & Korean,
    /// and link them as Fallback Font Assets to Fazo_Font and Fredoka fonts.
    /// </summary>
    public static class CJKFontAssetCreator
    {
        private const string JP_TTF_PATH = "Assets/_Project/Fonts/NotoSansJP.ttf";
        private const string KR_TTF_PATH = "Assets/_Project/Fonts/NotoSansKR.ttf";
        private const string JP_SDF_PATH = "Assets/_Project/Fonts/NotoSansJP_SDF.asset";
        private const string KR_SDF_PATH = "Assets/_Project/Fonts/NotoSansKR_SDF.asset";
        private const string FAZO_FONT_PATH = "Assets/_Project/Fonts/Fazo_Font.asset";

        [MenuItem("Tools/ArrowSwarm/Setup CJK Fallback Fonts")]
        public static void SetupCJKFallbackFonts()
        {
            Debug.Log("[ArrowSwarm] Setting up CJK Fallback Fonts for Fazo_Font...");

            Font jpFont = AssetDatabase.LoadAssetAtPath<Font>(JP_TTF_PATH);
            Font krFont = AssetDatabase.LoadAssetAtPath<Font>(KR_TTF_PATH);

            if (jpFont == null || krFont == null)
            {
                Debug.LogError("[ArrowSwarm] Failed to load NotoSansJP or NotoSansKR TTF font! Make sure they are in Assets/_Project/Fonts/");
                return;
            }

            TMP_FontAsset jpAsset = GetOrCreateFontAsset(jpFont, JP_SDF_PATH, "NotoSansJP_SDF");
            TMP_FontAsset krAsset = GetOrCreateFontAsset(krFont, KR_SDF_PATH, "NotoSansKR_SDF");

            if (jpAsset == null || krAsset == null)
            {
                Debug.LogError("[ArrowSwarm] Failed to create or load CJK font assets.");
                return;
            }

            // Link to Fazo_Font
            TMP_FontAsset fazoFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FAZO_FONT_PATH);
            if (fazoFont != null)
            {
                if (fazoFont.fallbackFontAssetTable == null)
                {
                    fazoFont.fallbackFontAssetTable = new List<TMP_FontAsset>();
                }

                if (!fazoFont.fallbackFontAssetTable.Contains(jpAsset))
                {
                    fazoFont.fallbackFontAssetTable.Add(jpAsset);
                }
                if (!fazoFont.fallbackFontAssetTable.Contains(krAsset))
                {
                    fazoFont.fallbackFontAssetTable.Add(krAsset);
                }

                EditorUtility.SetDirty(fazoFont);
                Debug.Log("[ArrowSwarm] Successfully attached Japanese & Korean fallback fonts to Fazo_Font!");
            }

            // Link to Fredoka font as well
            string fredokaPath = "Assets/_Project/Fonts/Fredoka-VariableFont_wdth,wght SDF.asset";
            TMP_FontAsset fredokaFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fredokaPath);
            if (fredokaFont != null)
            {
                if (fredokaFont.fallbackFontAssetTable == null) fredokaFont.fallbackFontAssetTable = new List<TMP_FontAsset>();
                if (!fredokaFont.fallbackFontAssetTable.Contains(jpAsset)) fredokaFont.fallbackFontAssetTable.Add(jpAsset);
                if (!fredokaFont.fallbackFontAssetTable.Contains(krAsset)) fredokaFont.fallbackFontAssetTable.Add(krAsset);
                EditorUtility.SetDirty(fredokaFont);
            }

            // Also link to TMP Settings global fallbacks
            if (TMP_Settings.instance != null)
            {
                if (TMP_Settings.fallbackFontAssets == null) TMP_Settings.fallbackFontAssets = new List<TMP_FontAsset>();
                if (!TMP_Settings.fallbackFontAssets.Contains(jpAsset)) TMP_Settings.fallbackFontAssets.Add(jpAsset);
                if (!TMP_Settings.fallbackFontAssets.Contains(krAsset)) TMP_Settings.fallbackFontAssets.Add(krAsset);
                EditorUtility.SetDirty(TMP_Settings.instance);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ArrowSwarm] CJK Fallback Font Setup Complete! Japanese and Korean characters will now render in Fazo_Font style.");
        }

        private static TMP_FontAsset GetOrCreateFontAsset(Font font, string sdfPath, string assetName)
        {
            TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(sdfPath);
            if (existing != null) return existing;

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(font);
            if (fontAsset == null)
            {
                Debug.LogError($"[ArrowSwarm] TMP_FontAsset.CreateFontAsset failed for {font.name}");
                return null;
            }

            fontAsset.name = assetName;
            AssetDatabase.CreateAsset(fontAsset, sdfPath);

            if (fontAsset.atlasTextures != null && fontAsset.atlasTextures.Length > 0 && fontAsset.atlasTextures[0] != null)
            {
                fontAsset.atlasTextures[0].name = assetName + " Atlas";
                AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);
            }

            if (fontAsset.material != null)
            {
                fontAsset.material.name = assetName + " Material";
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }

            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();

            return fontAsset;
        }
    }
}
#endif
