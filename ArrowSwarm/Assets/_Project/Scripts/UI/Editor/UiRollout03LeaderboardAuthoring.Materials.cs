namespace ArrowSwarm.UI.Editor
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;
    using K = UiRollout01Kit;

    /// <summary>Uses the existing rounded UI shader for rank accents without baking numbers into new textures.</summary>
    internal static partial class UiRollout03LeaderboardAuthoring
    {
        private static readonly Color Ink = new Color(.025f, .16f, .57f);
        private static readonly Color Ice = new Color(.86f, .95f, 1);

        private static void CreateMaterials()
        {
            Paint("RimBlue", new Color(.42f, .94f, 1), new Color(.025f, .42f, .95f), new Color(.2f, .83f, 1));
            Paint("RimGold", new Color(1, .95f, .32f), new Color(1, .46f, .015f), new Color(1, .88f, .15f));
            Paint("RimSilver", new Color(.97f, 1, 1), new Color(.37f, .56f, .78f), new Color(.70f, .86f, 1));
            Paint("RimBronze", new Color(1, .80f, .45f), new Color(.9f, .27f, .025f), new Color(1, .60f, .15f));
            Paint("FaceLight", new Color(1, 1, 1), new Color(.83f, .94f, 1), new Color(.75f, .94f, 1));
            Paint("FaceWarm", new Color(1, .99f, .94f), new Color(1, .93f, .79f), new Color(1, .96f, .87f));
            Paint("FaceDark", new Color(.035f, .16f, .34f), new Color(.015f, .06f, .18f), new Color(.10f, .35f, .57f));
            Paint("Player", new Color(.03f, .91f, 1), new Color(.015f, .28f, .93f), new Color(.69f, 1, 1), true);
            Paint("BadgeBlue", new Color(.52f, .91f, 1), new Color(.79f, .96f, 1), new Color(.34f, .86f, 1));
            Paint("BadgeDark", new Color(.025f, .22f, .47f), new Color(.015f, .09f, .25f), new Color(.09f, .52f, .79f));
        }

        private static Material Mat(string name) => K.Material("Ui03Leaderboard" + name);

        private static void Paint(string name, Color top, Color bottom, Color rim, bool frame = false)
        {
            string path = K.MaterialFolder + "/Ui03Leaderboard" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Project/Art/Shaders/SharedLoadingBar.shader");
                material = new Material(shader) { name = "Ui03Leaderboard" + name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_Color", Color.white);
            material.SetColor("_TopColor", top); material.SetColor("_BottomColor", bottom); material.SetColor("_RimColor", rim);
            material.SetFloat("_RimPixels", frame ? 2.5f : 1.5f);
            material.SetFloat("_Gloss", name == "FaceDark" || name == "BadgeDark" ? .025f : .17f);
            material.SetFloat("_Flow", 0); material.SetFloat("_Chevrons", 0); material.SetFloat("_Frame", frame ? 1 : 0);
            EditorUtility.SetDirty(material); AssetDatabase.SaveAssetIfDirty(material);
        }

        private static Image Surface(Transform parent, string name, Vector2 position, Vector2 size, string light, string dark = null)
        {
            var image = K.Image(parent, name, null, position, size);
            image.material = Mat(light); image.gameObject.AddComponent<UIThemeCoordinates>();
            if (dark != null) K.Theme(image, Color.white, Color.white, Mat(dark));
            return image;
        }
    }
}
