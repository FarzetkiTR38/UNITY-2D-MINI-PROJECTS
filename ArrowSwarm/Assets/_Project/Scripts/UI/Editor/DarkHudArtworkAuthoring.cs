namespace ArrowSwarm.UI.Editor
{
    using System;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Authors clean Dark-only stat plaques while preserving the original Light backdrop and TMP labels.</summary>
    public static class DarkHudArtworkAuthoring
    {
        private const string Art="Assets/_Project/Art/Sprites/ModeMenu/";
        private const string TexturePath=Art+"DarkHudPlaques.png";
        private const string SliceFolder=Art+"HudNightSlices";
        private const string MaterialPath="Assets/_Project/Art/Materials/Theme/DarkIllustratedBackdrop.mat";
        private const string PlaqueMaterialPath="Assets/_Project/Art/Materials/Mat_DarkHudPlaqueArtwork.mat";
        private const string ScenePath="Assets/_Project/Scenes/MainMenuScene.unity";

        /// <summary>Imports two bottom-origin source pixel rectangles, creates persistent sprites and authors the menu in Edit Mode.</summary>
        /// <param name="sourceRects">Trophy first, star second; rectangles use the source PNG's unscaled bottom-origin pixels.</param>
        public static void Apply(Rect[] sourceRects)
        {
            if(Application.isPlaying)throw new InvalidOperationException("Author HUD artwork in Edit Mode.");
            if(sourceRects==null||sourceRects.Length!=2)throw new ArgumentException("Provide trophy and star source rectangles.",nameof(sourceRects));
            var setup=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var texture=ImportTexture();
                foreach(var rect in sourceRects)Validate(rect,texture);
                if(!AssetDatabase.IsValidFolder(SliceFolder))AssetDatabase.CreateFolder(Art.TrimEnd('/'),"HudNightSlices");
                var trophy=Slice(texture,sourceRects[0],"Trophy");
                var star=Slice(texture,sourceRects[1],"Star");
                var plaqueMaterial=PlaqueMaterial();
                var material=AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
                if(material==null)throw new InvalidOperationException("Missing authored Dark backdrop material.");
                material.SetVector("_LeftStat",new Vector4(.087f,.885f,.463f,.965f));
                material.SetVector("_RightStat",new Vector4(.548f,.885f,.916f,.965f));
                material.SetVector("_StatCorner",new Vector4(66f/941,66f/1672,0,0));
                EditorUtility.SetDirty(material);AssetDatabase.SaveAssetIfDirty(material);
                var scene=EditorSceneManager.OpenScene(ScenePath);
                var owner=GameObject.Find("Canvas_MainMenu/ReferenceMainMenu/MenuArt");
                if(owner==null)throw new InvalidOperationException("Missing illustrated menu artwork root.");
                foreach(var canvas in owner.GetComponentsInParent<Canvas>(true))
                    canvas.additionalShaderChannels|=AdditionalCanvasShaderChannels.TexCoord1;
                AuthorImage(owner.transform,"DarkLevelPlaque",trophy,plaqueMaterial,new Vector2(258.8f,-125.4f),0);
                AuthorImage(owner.transform,"DarkStarPlaque",star,plaqueMaterial,new Vector2(688.8f,-125.4f),1);
                if(!EditorSceneManager.SaveScene(scene))throw new InvalidOperationException("Could not save authored HUD scene.");
            }
            finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
        }

        private static Texture2D ImportTexture()
        {
            AssetDatabase.ImportAsset(TexturePath,ImportAssetOptions.ForceSynchronousImport|ImportAssetOptions.ForceUpdate);
            var importer=AssetImporter.GetAtPath(TexturePath) as TextureImporter;
            if(importer==null)throw new InvalidOperationException("Import DarkHudPlaques.png before authoring the HUD.");
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=4096;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            if(texture==null)throw new InvalidOperationException("HUD texture import did not produce a texture asset.");
            return texture;
        }

        private static void Validate(Rect rect,Texture2D texture)
        {
            if(!(rect.width>0&&rect.height>0&&rect.xMin>=0&&rect.yMin>=0&&rect.xMax<=texture.width&&rect.yMax<=texture.height))
                throw new ArgumentException("Each HUD slice must be a non-empty rectangle inside the imported texture.");
        }

        private static Sprite Slice(Texture2D texture,Rect rect,string name)
        {
            string path=SliceFolder+"/"+name+".asset";
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if(sprite!=null&&sprite.texture==texture&&sprite.rect==rect)return sprite;
            var replacement=Sprite.Create(texture,rect,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,Vector4.zero,false);
            replacement.name="DarkHud"+name;
            if(sprite==null){AssetDatabase.CreateAsset(replacement,path);return replacement;}
            EditorUtility.CopySerialized(replacement,sprite);UnityEngine.Object.DestroyImmediate(replacement);
            EditorUtility.SetDirty(sprite);AssetDatabase.SaveAssetIfDirty(sprite);return sprite;
        }

        private static Material PlaqueMaterial()
        {
            var shader=Shader.Find("ArrowSwarm/UI/ButtonArtwork");
            if(shader==null)throw new InvalidOperationException("Missing rounded artwork shader.");
            var material=AssetDatabase.LoadAssetAtPath<Material>(PlaqueMaterialPath);
            if(material==null)
            {
                material=new Material(shader){name="Mat_DarkHudPlaqueArtwork"};AssetDatabase.CreateAsset(material,PlaqueMaterialPath);
            }
            else material.shader=shader;
            // The generated PNG retains an almost-opaque blue exterior (alpha max 254).
            // Clip the capsule geometrically, preserving every source RGB value and its original alpha.
            material.SetVector("_Bounds",new Vector4(.002f,.002f,.998f,.998f));
            material.SetVector("_Corner",new Vector4(.131f,.498f,0,0));
            material.SetFloat("_SurfaceStrength",0);material.SetColor("_Color",Color.white);
            EditorUtility.SetDirty(material);AssetDatabase.SaveAssetIfDirty(material);return material;
        }

        private static void AuthorImage(Transform parent,string name,Sprite sprite,Material material,Vector2 center,int index)
        {
            var child=parent.Find(name);bool created=child==null;
            if(created)
            {
                var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
                go.layer=parent.gameObject.layer;go.transform.SetParent(parent,false);child=go.transform;
                var rect=(RectTransform)child;rect.anchorMin=rect.anchorMax=new Vector2(0,1);
                rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=center;rect.sizeDelta=new Vector2(344,102);
            }
            var image=child.GetComponent<Image>();
            if(image==null)throw new InvalidOperationException("Existing HUD plaque is not an Image: "+name);
            image.sprite=sprite;image.material=material;image.type=Image.Type.Simple;image.preserveAspect=false;
            image.color=new Color(1,1,1,0);image.raycastTarget=false;
            if(child.GetComponent<UIThemeCoordinates>()==null)child.gameObject.AddComponent<UIThemeCoordinates>();
            child.SetSiblingIndex(index);
            var binding=child.GetComponent<ThemedUIElement>()??child.gameObject.AddComponent<ThemedUIElement>();
            var so=new SerializedObject(binding);
            so.FindProperty("_role").enumValueIndex=(int)ThemeRole.Custom;
            so.FindProperty("_customLightColor").colorValue=new Color(1,1,1,0);
            so.FindProperty("_customDarkColor").colorValue=Color.white;
            so.FindProperty("_materialOnly").boolValue=false;so.FindProperty("_preserveAlpha").boolValue=false;
            so.FindProperty("_lightSprite").objectReferenceValue=null;so.FindProperty("_darkSprite").objectReferenceValue=null;
            so.FindProperty("_darkMaterial").objectReferenceValue=null;so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
