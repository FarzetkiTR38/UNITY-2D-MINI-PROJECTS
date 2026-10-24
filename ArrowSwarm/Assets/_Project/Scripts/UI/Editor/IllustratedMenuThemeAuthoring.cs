namespace ArrowSwarm.UI.Editor
{
    using System;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Authors narrow theme variants for the illustrated menu while retaining its Light assets and press masks.</summary>
    public static class IllustratedMenuThemeAuthoring
    {
        private const string Folder="Assets/_Project/Art/Materials/Theme/";

        /// <summary>Updates only the illustrated MainMenu scene and its shared Dark materials.</summary>
        [MenuItem("Arrow Swarm/Theme/Author Illustrated Menu Dark")]
        public static void Apply()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Author in Edit Mode.");
            var setup=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainMenuScene.unity");
                var root=GameObject.Find("Canvas_MainMenu/ReferenceMainMenu").transform;
                Bind(root.GetComponent<Graphic>(),null,false,Hex("0E1423"));
                var art=root.Find("MenuArt");
                var backdrop=Material("DarkIllustratedBackdrop","ArrowSwarm/UI/IllustratedMenuDark");
                backdrop.SetVector("_LeftStat",new Vector4(.111f,.899f,.435f,.947f));
                backdrop.SetVector("_RightStat",new Vector4(.570f,.899f,.886f,.947f));
                backdrop.SetVector("_StatCorner",new Vector4(.041f,.024f,0,0));EditorUtility.SetDirty(backdrop);
                Bind(art.GetComponent<Graphic>(),backdrop,true,Color.white);
                var artBinding=new SerializedObject(art.GetComponent<ThemedUIElement>());
                artBinding.FindProperty("_darkSprite").objectReferenceValue=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/ModeMenu/MainMenuNightCutout.png");
                artBinding.ApplyModifiedPropertiesWithoutUndo();
                foreach(string name in new[]{"Play","GameMode","Levels","Leaderboard","Settings"})
                {
                    var button=art.Find(name+"Button"); var image=button.GetComponent<Image>();
                    if(button.GetComponent<ModeButtonVisual>()==null)
                    {
                        var dark=Material("Dark"+name+"Artwork","ArrowSwarm/UI/ButtonArtwork");
                        dark.CopyPropertiesFromMaterial(image.material);
                        dark.SetFloat("_SurfaceStrength",name=="GameMode"?1:0);
                        dark.SetColor("_SurfaceColor",Hex("20324F"));EditorUtility.SetDirty(dark);
                        Bind(image,dark,true,Color.white);
                    }
                    Bind(button.Find("Label").GetComponent<Graphic>(),null,false,Hex("F0F6FF"));
                    if(name=="GameMode")Bind(button.Find("SelectedMode").GetComponent<Graphic>(),null,false,Hex("91CDFF"));
                }
                foreach(string name in new[]{"SelectedLevel","SharedStars"})Bind(art.Find(name).GetComponent<Graphic>(),null,false,Hex("E6F0FF"));
                var modal=root.Find("GameModeOverlay/ModalDesign");
                Bind(modal.Find("ModalFrame").GetComponent<Graphic>(),DarkThemeMaterials.Surface,true,Color.white);
                Bind(modal.Find("Title").GetComponent<Graphic>(),null,false,Hex("F0F6FF"));
                var close=Material("DarkCloseArtwork","ArrowSwarm/UI/ButtonArtwork");
                close.SetVector("_Bounds",new Vector4(.06f,.06f,.95f,.95f));
                close.SetVector("_Corner",new Vector4(.19f,.19f,0,0));close.SetFloat("_SurfaceStrength",0);
                EditorUtility.SetDirty(close);Bind(modal.Find("Close").GetComponent<Graphic>(),close,true,Color.white);
                foreach(string mode in new[]{"Defense","Shapes","ColorShift"})
                {
                    var card=modal.Find(mode+"Card");
                    var material=Material("Dark"+mode+"Card","ArrowSwarm/UI/ButtonArtwork");
                    material.SetVector("_Bounds",new Vector4(4f/827,4f/297,823f/827,293f/297));
                    material.SetVector("_Corner",new Vector4(65f/827,65f/297,0,0));
                    material.SetFloat("_SurfaceStrength",1);material.SetColor("_SurfaceColor",Hex("20324F"));
                    EditorUtility.SetDirty(material);Bind(card.GetComponent<Graphic>(),material,true,Color.white);
                    Bind(card.Find("ModeTitle").GetComponent<Graphic>(),null,false,Hex("E6F0FF"));
                    Bind(card.Find("SelectionLabel").GetComponent<Graphic>(),null,false,Hex("F0F6FF"));
                }
                foreach(var canvas in scene.GetRootGameObjects())foreach(var c in canvas.GetComponentsInChildren<Canvas>(true))
                    c.additionalShaderChannels|=AdditionalCanvasShaderChannels.TexCoord1;
                EditorSceneManager.SaveScene(scene);
            }
            finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
        }

        private static Material Material(string name,string shaderName)
        {
            string path=Folder+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(Shader.Find(shaderName)){name=name};AssetDatabase.CreateAsset(material,path);}
            return material;
        }
        private static void Bind(Graphic graphic,Material material,bool materialOnly,Color dark)
        {
            if(graphic==null)throw new InvalidOperationException("Missing authored menu graphic.");
            var binding=graphic.GetComponent<ThemedUIElement>()??graphic.gameObject.AddComponent<ThemedUIElement>();
            var so=new SerializedObject(binding);
            so.FindProperty("_customLightColor").colorValue=graphic.color;
            dark.a=graphic.color.a;so.FindProperty("_customDarkColor").colorValue=dark;
            so.FindProperty("_materialOnly").boolValue=materialOnly;so.FindProperty("_preserveAlpha").boolValue=true;
            so.FindProperty("_darkMaterial").objectReferenceValue=material;so.ApplyModifiedPropertiesWithoutUndo();
            if(material!=null&&graphic.GetComponent<UIThemeCoordinates>()==null)graphic.gameObject.AddComponent<UIThemeCoordinates>();
            if(material!=null)AssetDatabase.SaveAssetIfDirty(material);
        }
        private static Color Hex(string value){Color color;ColorUtility.TryParseHtmlString("#"+value,out color);return color;}
    }
}
