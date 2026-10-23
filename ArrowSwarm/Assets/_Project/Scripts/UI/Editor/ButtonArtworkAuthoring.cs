namespace ArrowSwarm.UI.Editor
{
    using System;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Authors transparent rounded masks for menu illustrations with baked cream padding.</summary>
    public static class ButtonArtworkAuthoring
    {
        private const string Root = "Assets/_Project/Art/Materials/";

        /// <summary>Updates only the five illustrated main menu buttons; preserves their text and feedback.</summary>
        public static void Apply()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode before authoring.");
            var shader = Shader.Find("ArrowSwarm/UI/ButtonArtwork");
            if (shader == null) throw new InvalidOperationException("Button artwork shader has not imported.");
            foreach (var button in UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (button.image == null || button.image.sprite == null) continue;
                string path = AssetDatabase.GetAssetPath(button.image.sprite);
                if (!path.Contains("/LocalizedSlices/")) continue;
                Vector4 bounds, corner;
                switch (button.name)
                {
                    case "PlayButton":
                        bounds = new Vector4(4f/687,0,681f/687,216f/218);
                        corner = new Vector4(108f/687,107f/218,0,0); break;
                    case "GameModeButton":
                        bounds = new Vector4(4f/688,1f/164,681f/688,163f/164);
                        corner = new Vector4(82f/688,80f/164,0,0); break;
                    case "LevelsButton":
                        bounds = new Vector4(9f/641,3f/132,631f/641,128f/132);
                        corner = new Vector4(69f/641,61f/132,0,0); break;
                    case "LeaderboardButton":
                        bounds = new Vector4(9f/641,7f/133,630f/641,132f/133);
                        corner = new Vector4(68f/641,61f/133,0,0); break;
                    case "SettingsButton":
                        bounds = new Vector4(9f/641,3f/133,630f/641,132f/133);
                        corner = new Vector4(68f/641,63f/133,0,0); break;
                    default: continue;
                }
                string materialPath = Root + "Mat_" + button.name + "Artwork.mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material == null)
                {
                    material = new Material(shader) { name = button.name + "Artwork" };
                    AssetDatabase.CreateAsset(material, materialPath);
                }
                material.SetVector("_Bounds",bounds); material.SetVector("_Corner",corner);
                button.image.material = material;
                if (button.GetComponent<UIThemeCoordinates>() == null) button.gameObject.AddComponent<UIThemeCoordinates>();
                button.GetComponentInParent<Canvas>().additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
                EditorUtility.SetDirty(material); EditorUtility.SetDirty(button.image);
            }
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
        }
    }
}
