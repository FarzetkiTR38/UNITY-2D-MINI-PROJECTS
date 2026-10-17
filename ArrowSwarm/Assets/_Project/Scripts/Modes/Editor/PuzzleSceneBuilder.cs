namespace ArrowSwarm.Modes.Editor
{
    using System.Linq;
    using ArrowSwarm.Puzzle;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem.UI;
    using UnityEngine.Rendering.Universal;
    using UnityEngine.UI;
    using U=ModeAuthoringUtility;

    /// <summary>Builds isolated, editable puzzle scenes and the pooled arrow prefab.</summary>
    public static class PuzzleSceneBuilder
    {
        private const string PrefabPath="Assets/_Project/Prefabs/PuzzleArrow.prefab";
        private static Material StrokeMaterial;

        [MenuItem("Arrow Swarm/Puzzles/Build Puzzle Scenes")]
        public static void Build()
        {
            if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new System.InvalidOperationException("Save the current scene before building puzzle scenes.");
            string path=U.Data+"PuzzleStroke.mat";
            StrokeMaterial=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!StrokeMaterial){StrokeMaterial=new Material(Shader.Find("Sprites/Default"));AssetDatabase.CreateAsset(StrokeMaterial,path);}
            var prefabAsset=AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var prefab=prefabAsset?prefabAsset.GetComponent<PuzzleArrowView>():null;
            if(!prefab)
            {
                var go=new GameObject("PuzzleArrow");var view=go.AddComponent<PuzzleArrowView>();
                U.Set(view,"_body",Line("Body",go.transform));U.Set(view,"_head",Line("Head",go.transform));
                prefab=PrefabUtility.SaveAsPrefabAsset(go,PrefabPath).GetComponent<PuzzleArrowView>();Object.DestroyImmediate(go);
            }
            AssetDatabase.SaveAssets();
            prefab=AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath).GetComponent<PuzzleArrowView>();
            BuildScene(GameModeId.Shapes,"ShapesScene",prefab);
            BuildScene(GameModeId.ColorShift,"ColorShiftScene",prefab);
            var list=EditorBuildSettings.scenes.ToList();
            foreach(var name in new[]{"ShapesScene","ColorShiftScene"})
            {string scenePath="Assets/_Project/Scenes/"+name+".unity";if(!list.Any(s=>s.path==scenePath))list.Add(new EditorBuildSettingsScene(scenePath,true));}
            EditorBuildSettings.scenes=list.ToArray();AssetDatabase.SaveAssets();
        }

        private static void BuildScene(GameModeId mode,string name,PuzzleArrowView prefab)
        {
            string path="Assets/_Project/Scenes/"+name+".unity";
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(path))return;
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("Main Camera",typeof(UnityEngine.Camera),typeof(AudioListener));camera.tag="MainCamera";camera.transform.position=new Vector3(0,0,-10);
            var cam=camera.GetComponent<UnityEngine.Camera>();cam.orthographic=true;cam.orthographicSize=20;cam.backgroundColor=new Color(.99f,.97f,.92f);cam.clearFlags=CameraClearFlags.SolidColor;
            string backdropPath=U.Data+"PuzzleBackdrop.mat";
            var backdropMat=AssetDatabase.LoadAssetAtPath<Material>(backdropPath);
            if(!backdropMat){backdropMat=new Material(Shader.Find("Sprites/Default"));backdropMat.color=new Color(.99f,.975f,.94f);AssetDatabase.CreateAsset(backdropMat,backdropPath);}
            var backdrop=GameObject.CreatePrimitive(PrimitiveType.Quad);backdrop.name="Cream Backdrop";backdrop.transform.position=new Vector3(0,0,5);backdrop.transform.localScale=new Vector3(500,500,1);
            backdrop.GetComponent<Renderer>().sharedMaterial=backdropMat;backdrop.GetComponent<Renderer>().sortingOrder=-100;Object.DestroyImmediate(backdrop.GetComponent<Collider>());
            var zoom=camera.AddComponent<ArrowSwarm.Camera.CameraController>();
            var light=new GameObject("Global Light",typeof(Light2D));light.GetComponent<Light2D>().lightType=Light2D.LightType.Global;
            new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
            var session=new GameObject("PuzzleSession").AddComponent<PuzzleSession>();
            var arrows=new GameObject("Pooled Arrows").transform;
            var frame=new GameObject("Rotating Frame").AddComponent<PuzzleFrame>();
            var walls=new LineRenderer[4];for(int i=0;i<4;i++)walls[i]=Line("Wall "+i,frame.transform);
            U.Set(frame,"_walls",walls);U.Set(session,"_catalog",AssetDatabase.LoadAssetAtPath<PuzzleCatalog>(U.Data+(mode==GameModeId.Shapes?"ShapesCatalog":"ColorShiftCatalog")+".asset"));
            U.Set(session,"_config",AssetDatabase.LoadAssetAtPath<PuzzleConfig>(U.Data+"PuzzleConfig.asset"));U.Set(session,"_mode",(int)mode);
            U.Set(session,"_arrowPrefab",prefab);U.Set(session,"_arrowRoot",arrows);U.Set(session,"_frame",frame);U.Set(session,"_cameraController",zoom);
            var input=session.gameObject.AddComponent<PuzzleInput>();U.Set(input,"_session",session);U.Set(input,"_camera",cam);
            BuildHUD(session,zoom);
            EditorSceneManager.SaveScene(scene,path);
        }

        private static LineRenderer Line(string name,Transform parent)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);var line=go.AddComponent<LineRenderer>();
            line.sharedMaterial=StrokeMaterial;line.useWorldSpace=true;line.numCornerVertices=6;line.numCapVertices=6;line.sortingOrder=5;line.positionCount=0;return line;
        }

        private static RectTransform Canvas(string name,int order)
        {
            var rect=new GameObject(name,typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)).GetComponent<RectTransform>();
            var canvas=rect.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=order;
            var scaler=rect.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(941,1672);scaler.matchWidthOrHeight=.5f;return rect;
        }

        private static void BuildHUD(PuzzleSession session,ArrowSwarm.Camera.CameraController zoom)
        {
            var canvas=Canvas("Canvas_HUD",10);var root=U.Rect("SafeHUD",canvas,0,0,941,1672);root.gameObject.AddComponent<ReferenceCanvasFit>();
            var hud=canvas.gameObject.AddComponent<PuzzleHUD>();Color ink=new Color(.11f,.2f,.42f);
            U.Set(hud,"_title",U.Text(U.Rect("Title",root,140,60,660,130),"SHAPES / 1",35,ink));
            U.Set(hud,"_lives",U.Text(U.Rect("Lives",root,260,190,420,65),"♥♥♥",48,new Color(.93f,.2f,.35f)));
            U.Set(hud,"_remaining",U.Text(U.Rect("Remaining",root,270,265,400,50),"ARROWS",24,ink));
            U.Set(hud,"_pause",U.Button(U.Rect("Pause",root,790,70,105,85),"II"));
            U.Set(hud,"_hint",U.Button(U.Rect("Hint",root,315,1465,310,85),"HINT"));
            U.Set(hud,"_message",U.Text(U.Rect("Instruction",root,110,1555,720,90),"",23,ink));
            var bar=U.Rect("RotationTrack",root,110,1335,720,95);U.Image(bar,null,new Color(.82f,.91f,1f),true);bar.gameObject.AddComponent<FrameSwipeUI>();
            U.Set(hud,"_turns",U.Text(U.Stretch("RotationLabel",bar),"SWIPE LEFT / RIGHT",27,ink));U.Set(hud,"_rotationBar",bar.gameObject);
            var slider=U.Rect("Zoom",root,220,1245,500,25).gameObject.AddComponent<Slider>();
            U.Image(slider.GetComponent<RectTransform>(),null,new Color(.83f,.85f,.9f),true);
            var handle=U.Rect("Handle",slider.transform,0,0,35,35);U.Image(handle,null,new Color(.1f,.5f,.95f));slider.handleRect=handle;slider.targetGraphic=handle.GetComponent<Image>();slider.direction=Slider.Direction.LeftToRight;
            var controller=slider.gameObject.AddComponent<ArrowSwarm.Camera.ZoomController>();U.Set(controller,"_zoomSlider",slider);
            var overlayCanvas=Canvas("Canvas_Overlay",30);var shade=U.Stretch("PuzzleOverlay",overlayCanvas);U.Image(shade,null,new Color(.05f,.1f,.2f,.65f),true);
            var group=shade.gameObject.AddComponent<CanvasGroup>();group.alpha=0;group.interactable=group.blocksRaycasts=false;U.Set(hud,"_overlay",group);
            var panel=U.Rect("ResultCard",shade,0,0,941,1672);panel.gameObject.AddComponent<ReferenceCanvasFit>();
            U.Image(U.Rect("Card",panel,100,400,741,900),null,new Color(.99f,.98f,.95f),true);
            U.Set(hud,"_result",U.Text(U.Rect("ResultTitle",panel,140,455,661,100),"LEVEL CLEARED!",52,ink));
            U.Set(hud,"_resultStars",U.Text(U.Rect("ResultStars",panel,155,580,631,100),"★★★",40,new Color(.94f,.61f,.08f)));
            U.Set(hud,"_resume",U.Button(U.Rect("Resume",panel,210,725,520,85),"RESUME"));
            var next=U.Button(U.Rect("Next",panel,210,725,520,85),"NEXT LEVEL");U.Set(hud,"_next",next);U.Set(hud,"_nextLabel",next.GetComponentInChildren<TMP_Text>());
            U.Set(hud,"_continue",U.Button(U.Rect("RewardedContinue",panel,210,725,520,85),"CONTINUE · AD"));
            U.Set(hud,"_retry",U.Button(U.Rect("Retry",panel,210,855,520,85),"RETRY"));
            U.Set(hud,"_menu",U.Button(U.Rect("Menu",panel,210,985,520,85),"MAIN MENU"));
        }
    }
}
