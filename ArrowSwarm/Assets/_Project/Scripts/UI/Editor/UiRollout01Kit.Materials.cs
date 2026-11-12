namespace ArrowSwarm.UI.Editor
{
    using System;
    using TMPro;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Owns compact material variants shared by every new Boot/Home control and TMP label.</summary>
    internal static partial class UiRollout01Kit
    {
        private static void CreateMaterials()
        {
            EnsureFolder(MaterialFolder);
            var panel = NewMaterial("DarkPanel", AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Project/Art/Shaders/DarkUI.shader"));
            panel.SetColor("_Color", Color.white); panel.SetColor("_SurfaceColor", new Color(.035f, .105f, .245f, 1));
            panel.SetFloat("_AccentStrength", 1); panel.SetFloat("_SurfaceStrength", 1);
            panel.SetFloat("_BevelStrength", 1); panel.SetFloat("_EdgeFill", 1);
            panel.SetFloat("_Artwork", 0); panel.SetFloat("_Background", 0); panel.SetFloat("_LiftInk", 0);
            panel.SetFloat("_OpaqueInterior", 0); panel.SetFloat("_SolidFill", 0);
            panel.SetVector("_SurfaceInset", new Vector4(.02f, .04f, 0, 0));
            panel.SetVector("_Corner", new Vector4(.06f, .12f, 0, 0));
            panel.SetVector("_BevelShade", new Vector4(.76f, 1.12f, 0, 0)); Save(panel);
            var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Project/Art/Shaders/SharedMenuBackdrop.shader");
            ConfigureBackdrop("BackdropLight", shader, new Color(.985f, .969f, .908f), new Color(.94f, .978f, 1f));
            ConfigureBackdrop("BackdropDark", shader, new Color(.012f, .038f, .105f), new Color(.028f, .10f, .235f));
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            var titleFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TitleFontPath);
            if (font == null || titleFont == null) throw new InvalidOperationException("Existing TMP fonts are missing.");
            ConfigureText("Body", font.material, .045f, .0f, .02f);
            ConfigureText("TitleBlue", titleFont.material, .20f, .13f, .16f);
            ConfigureText("TitleGold", titleFont.material, .20f, .13f, .16f);
            CreateLoadingMaterials();
        }

        private static void CreateLoadingMaterials()
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Project/Art/Shaders/SharedLoadingBar.shader");
            var track = NewMaterial("LoadingTrack", shader);
            track.SetColor("_Color", Color.white);
            track.SetColor("_TopColor", new Color(.08f, .20f, .40f));
            track.SetColor("_BottomColor", new Color(.015f, .06f, .18f));
            track.SetColor("_RimColor", new Color(.12f, .70f, 1));
            track.SetFloat("_RimPixels", 3); track.SetFloat("_Gloss", .10f); track.SetFloat("_Flow", 0);
            track.SetFloat("_Frame", 1); track.SetFloat("_Chevrons", 0); Save(track);
            var fill = NewMaterial("LoadingFill", shader);
            fill.SetColor("_Color", Color.white);
            fill.SetColor("_TopColor", new Color(.10f, .90f, 1));
            fill.SetColor("_BottomColor", new Color(.015f, .24f, .94f));
            fill.SetColor("_RimColor", new Color(.48f, .98f, 1));
            fill.SetFloat("_RimPixels", 1.5f); fill.SetFloat("_Gloss", .27f); fill.SetFloat("_Flow", .075f);
            fill.SetFloat("_Frame", 0); fill.SetFloat("_Chevrons", .18f); Save(fill);
        }

        private static void ConfigureBackdrop(string name, Shader shader, Color top, Color bottom)
        {
            var material = NewMaterial(name, shader);
            material.SetColor("_Color", Color.white); material.SetColor("_TopColor", top);
            material.SetColor("_BottomColor", bottom); Save(material);
        }

        private static void ConfigureText(string name, Material source, float outline, float dilate, float shadow)
        {
            var material = NewMaterial(name, source.shader); material.CopyPropertiesFromMaterial(source);
            material.SetColor("_FaceColor", Color.white); material.SetFloat("_FaceDilate", dilate);
            material.SetColor("_OutlineColor", new Color(.008f, .075f, .48f, 1));
            material.SetFloat("_OutlineWidth", outline); material.EnableKeyword("OUTLINE_ON");
            material.SetColor("_UnderlayColor", new Color(.01f, .035f, .22f, .9f));
            material.SetFloat("_UnderlayOffsetX", 0); material.SetFloat("_UnderlayOffsetY", -.62f);
            material.SetFloat("_UnderlayDilate", shadow); material.SetFloat("_UnderlaySoftness", .06f);
            material.EnableKeyword("UNDERLAY_ON"); Save(material);
        }

        private static Material NewMaterial(string name, Shader shader)
        {
            if (shader == null) throw new InvalidOperationException("Shared UI shader missing: " + name);
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath(name));
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, MaterialPath(name));
            }
            else material.shader = shader;
            return material;
        }

        private static void Save(Material material)
        { EditorUtility.SetDirty(material); AssetDatabase.SaveAssetIfDirty(material); }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/'); EnsureFolder(path.Substring(0, slash));
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }
    }
}
