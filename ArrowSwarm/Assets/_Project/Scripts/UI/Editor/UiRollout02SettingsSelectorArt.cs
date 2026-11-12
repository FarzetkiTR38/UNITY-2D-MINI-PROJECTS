namespace ArrowSwarm.UI.Editor
{
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Reuses original joined art and caption-cleaning materials for live localized TMP.</summary>
    internal static class UiRollout02SettingsSelectorArt
    {
        internal static Sprite Get(bool dark) => AssetDatabase.LoadAllAssetsAtPath(
            "Assets/_Project/Art/_FarzetkiArts/SettingsPanel/ChatGPT Image 26 Ağu 2026 01_03_39 (" + (dark ? 10 : 9) + ").png")
            .OfType<Sprite>().OrderByDescending(s => s.rect.width * s.rect.height).First();

        internal static Material Paint(bool dark)
        {
            string name = "SettingsJoinedSelector" + (dark ? "Dark" : "Light");
            string path = UiRollout01Kit.MaterialFolder + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Theme/DarkSelector.mat")) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Project/Art/Shaders/SettingsSelectorArtwork.shader");
            material.SetFloat("_LiftInk", 0);
            material.SetFloat("_SurfaceStrength", dark ? 1 : 0);
            material.SetFloat("_AccentStrength", dark ? .96f : 1);
            material.SetVector("_Caption1", new Vector4(550f / 2172, 160f / 724, 915f / 2172, 274f / 724));
            material.SetVector("_Caption2", new Vector4(1250f / 2172, 160f / 724, 1645f / 2172, 274f / 724));
            EditorUtility.SetDirty(material); AssetDatabase.SaveAssetIfDirty(material);
            return material;
        }
    }
}
