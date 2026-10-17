namespace ArrowSwarm.UI.Editor
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>Shared palettes with separate interior masks for each UI artwork family.</summary>
    internal static class DarkThemeMaterials
    {
        private const string Folder = "Assets/_Project/Art/Materials/Theme";
        internal static Material Surface => Load("DarkSurface");
        internal static Material Background => Load("DarkBackground");
        internal static Material Control => Load("DarkControl");
        internal static Material Selector => Load("DarkSelector");
        internal static Material Medal => Load("DarkMedal");
        internal static Material Gold => Load("DarkMedalGold");
        internal static Material Silver => Load("DarkMedalSilver");
        internal static Material Bronze => Load("DarkMedalBronze");
        internal static Material Lock => Load("DarkLock");
        internal static Material Row => Load("DarkRow");
        internal static Material Field => Load("DarkField");
        internal static Material Bar => Load("DarkBar");
        internal static Material Ornament => Load("DarkOrnament");
        internal static Material Pause => Load("DarkPause");
        internal static Material Thumb => Load("DarkThumb");

        internal static void Create()
        {
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Project/Art/Materials", "Theme");
            Configure("DarkSurface", "20324F", 0.94f, 1, 0, 0, new Vector2(0.022f,0.022f), new Vector2(0.06f,0.04f));
            Configure("DarkBackground", "142138", 0.9f, 1, 1, 0, Vector2.zero, Vector2.zero);
            Configure("DarkControl", "20324F", 0.96f, 0, 0, 0, Vector2.zero, Vector2.zero);
            Configure("DarkSelector", "233853", 0.96f, 1, 0, 3, new Vector2(0.023f,0.06f), new Vector2(0.03f,0.12f));
            Selector.SetFloat("_LiftInk",1);
            Configure("DarkMedal", "233853", 0.96f, 1, 0, 1, new Vector2(0.012f,0.10f), new Vector2(0.028f,0.15f));
            foreach(var name in new[]{"DarkMedalGold","DarkMedalSilver","DarkMedalBronze"})
                Configure(name,"233853",0.96f,1,0,1,Vector2.zero,new Vector2(0.022f,0.13f));
            Gold.SetVector("_ContentBounds",new Vector4(0.187f,0.078f,0.984f,0.714f));
            Silver.SetVector("_ContentBounds",new Vector4(0.187f,0.082f,0.984f,0.791f));
            Bronze.SetVector("_ContentBounds",new Vector4(0.187f,0.080f,0.984f,0.752f));
            Configure("DarkLock", "2B4160", 0.96f, 1, 0, 2, new Vector2(0.065f,0.065f), new Vector2(0.095f,0.095f));
            Configure("DarkRow", "263C5C", 0.94f, 1, 0, 0, new Vector2(0.018f,0.09f), new Vector2(0.035f,0.18f));
            Configure("DarkField", "2B4160", 0.94f, 1, 0, 0, new Vector2(0.018f,0.10f), new Vector2(0.035f,0.23f));
            Configure("DarkBar", "20324F", 0.94f, 1, 0, 0, new Vector2(0.018f,0.105f), new Vector2(0.04f,0.28f));
            Configure("DarkOrnament", "233853", 0.96f, 1, 0, 4, Vector2.zero, Vector2.zero);
            Configure("DarkPause", "20324F", 0.96f, 1, 0, 5, new Vector2(0.06f,0.06f),new Vector2(0.12f,0.12f));
            Configure("DarkThumb", "6E96BF", 0.96f, 0.75f, 0, 0, new Vector2(0.015f,0.02f),new Vector2(0.1f,0.15f));
        }

        private static Material Load(string name) => AssetDatabase.LoadAssetAtPath<Material>($"{Folder}/{name}.mat");

        private static void Configure(string name, string hex, float accent, float surface, float background, float artwork, Vector2 inset, Vector2 corner)
        {
            var material = Load(name);
            if (material == null)
            {
                material = new Material(AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Project/Art/Shaders/DarkUI.shader")) { name = name };
                AssetDatabase.CreateAsset(material, $"{Folder}/{name}.mat");
            }
            ColorUtility.TryParseHtmlString("#"+hex, out Color color);
            material.SetColor("_SurfaceColor",color);
            material.SetFloat("_AccentStrength",accent);
            material.SetFloat("_SurfaceStrength",surface);
            material.SetFloat("_Background",background);
            material.SetFloat("_Artwork",artwork);
            bool panelBevel = name == "DarkSurface" || name == "DarkRow" || name == "DarkField" || name == "DarkBar" || name == "DarkPause";
            material.SetFloat("_EdgeFill", panelBevel ? 1f : name == "DarkSelector" ? 0.9f : artwork > 0 && artwork < 3 ? 0f : 0.5f);
            material.SetFloat("_BevelStrength", panelBevel ? 1f : 0f);
            material.SetVector("_BevelShade", new Vector4(0.72f,1.10f,0,0));
            material.SetFloat("_OpaqueInterior", surface > 0 && artwork != 4 && background == 0 ? 1 : 0);
            material.SetFloat("_SolidFill", surface > 0 && artwork < 2 && background == 0 && name != "DarkThumb" ? 1 : 0);
            material.SetFloat("_LiftInk",0);
            material.SetVector("_SurfaceInset",new Vector4(inset.x,inset.y,0,0));
            material.SetVector("_Corner",new Vector4(corner.x,corner.y,0,0));
            EditorUtility.SetDirty(material);
        }
    }
}
