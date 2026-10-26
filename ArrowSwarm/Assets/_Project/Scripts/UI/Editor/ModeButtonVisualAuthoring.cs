namespace ArrowSwarm.UI.Editor
{
    using System;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Authors mode-specific illustrations on the existing localized button and its original input target.</summary>
    public static class ModeButtonVisualAuthoring
    {
        private const string Art="Assets/_Project/Art/Sprites/ModeMenu/";

        /// <summary>Imports the paired atlases, creates persistent sprite slices and wires the menu in Edit Mode.</summary>
        [MenuItem("Arrow Swarm/UI/Author Selected Mode Artwork")]
        public static void Apply()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Author in Edit Mode.");
            var setup=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var light=Slices("ModeMenuVariants.png", "Light");
                var dark=Slices("ModeMenuVariantsDark.png", "Dark");
                string path="Assets/_Project/Art/Materials/Mat_ModeVariantArtwork.mat";
                var material=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(material==null){material=new Material(Shader.Find("ArrowSwarm/UI/ButtonArtwork"));AssetDatabase.CreateAsset(material,path);}
                material.SetVector("_Bounds",new Vector4(.003f,.003f,.997f,.997f));
                material.SetVector("_Corner",new Vector4(.091f,.494f,0,0));
                material.SetFloat("_SurfaceStrength",0);material.SetColor("_Color",Color.white);
                EditorUtility.SetDirty(material);AssetDatabase.SaveAssetIfDirty(material);
                var scene=EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainMenuScene.unity");
                var button=GameObject.Find("Canvas_MainMenu/ReferenceMainMenu/MenuArt/GameModeButton");
                var image=button.GetComponent<Image>();image.sprite=light[0];image.material=material;
                var binding=new SerializedObject(button.GetComponent<ThemedUIElement>());
                binding.FindProperty("_lightSprite").objectReferenceValue=null;
                binding.FindProperty("_darkSprite").objectReferenceValue=null;
                binding.FindProperty("_darkMaterial").objectReferenceValue=material;
                binding.ApplyModifiedPropertiesWithoutUndo();
                var visual=button.GetComponent<ModeButtonVisual>()??button.AddComponent<ModeButtonVisual>();
                var so=new SerializedObject(visual);so.FindProperty("_target").objectReferenceValue=image;
                SetSprites(so.FindProperty("_lightModes"),light);SetSprites(so.FindProperty("_darkModes"),dark);
                so.ApplyModifiedPropertiesWithoutUndo();EditorSceneManager.SaveScene(scene);
            }
            finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
        }
        private static Sprite[] Slices(string file,string theme)
        {
            string path=Art+file;
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=2048;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if(!AssetDatabase.IsValidFolder(Art+"VariantSlices"))AssetDatabase.CreateFolder(Art.TrimEnd('/'),"VariantSlices");
            var result=new Sprite[3];string[] modes={"Defense","Shapes","ColorShift"};
            for(int i=0;i<3;i++)
            {
                string target=Art+"VariantSlices/"+theme+modes[i]+".asset";
                result[i]=AssetDatabase.LoadAssetAtPath<Sprite>(target);
                if(result[i]!=null)continue;
                result[i]=Sprite.Create(texture,new Rect(240,texture.height-268-i*270,1436,260),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
                result[i].name=theme+modes[i];AssetDatabase.CreateAsset(result[i],target);
            }
            return result;
        }
        private static void SetSprites(SerializedProperty array,Sprite[] values)
        {array.arraySize=values.Length;for(int i=0;i<values.Length;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=values[i];}
    }
}
