namespace ArrowSwarm.UI.Editor
{
    using System;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Authors painted night cards without recoloring character highlights or modifying Light artwork.</summary>
    public static class ModalNightArtworkAuthoring
    {
        private const string Art="Assets/_Project/Art/Sprites/ModeMenu/";

        /// <summary>Imports the night atlas and stores three paired sprite references on the existing scene cards.</summary>
        [MenuItem("Arrow Swarm/UI/Author Painted Night Mode Cards")]
        public static void Apply()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Author in Edit Mode.");
            string path=Art+"ModeModalNightPainted.png";AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=2048;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            if(!AssetDatabase.IsValidFolder(Art+"ModalNightSlices"))AssetDatabase.CreateFolder(Art.TrimEnd('/'),"ModalNightSlices");
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            float[] rows={920.8971f,614.26434f,307.63153f};var sprites=new Sprite[3];
            for(int i=0;i<3;i++)
            {
                var rect=new Rect(58.061638f/942*texture.width,rows[i]/1670*texture.height,
                    827.8789f/942*texture.width,296.64474f/1670*texture.height);
                var created=Sprite.Create(texture,rect,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
                created.name="NightCard"+i;string asset=Art+"ModalNightSlices/Card"+i+".asset";
                sprites[i]=AssetDatabase.LoadAssetAtPath<Sprite>(asset);
                if(sprites[i]==null){sprites[i]=created;AssetDatabase.CreateAsset(created,asset);}
                else{EditorUtility.CopySerialized(created,sprites[i]);UnityEngine.Object.DestroyImmediate(created);}
                EditorUtility.SetDirty(sprites[i]);AssetDatabase.SaveAssetIfDirty(sprites[i]);
            }
            var setup=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainMenuScene.unity");
                var modal=GameObject.Find("Canvas_MainMenu/ReferenceMainMenu/GameModeOverlay/ModalDesign").transform;
                string[] modes={"Defense","Shapes","ColorShift"};
                for(int i=0;i<modes.Length;i++)
                {
                    var card=modal.Find(modes[i]+"Card");
                    var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Theme/Dark"+modes[i]+"Card.mat");
                    material.SetFloat("_SurfaceStrength",0);EditorUtility.SetDirty(material);AssetDatabase.SaveAssetIfDirty(material);
                    var so=new SerializedObject(card.GetComponent<ThemedUIElement>());
                    so.FindProperty("_darkSprite").objectReferenceValue=sprites[i];
                    so.FindProperty("_darkMaterial").objectReferenceValue=material;so.ApplyModifiedPropertiesWithoutUndo();
                }
                EditorSceneManager.SaveScene(scene);
            }
            finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
        }
    }
}
