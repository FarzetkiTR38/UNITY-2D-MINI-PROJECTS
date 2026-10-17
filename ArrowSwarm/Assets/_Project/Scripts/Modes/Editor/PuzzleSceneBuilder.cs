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
            GameScenePuzzleUIRevision.EnsureShells();
            string path=U.Data+"PuzzleStroke.mat";
            StrokeMaterial=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!StrokeMaterial){StrokeMaterial=new Material(Shader.Find("Sprites/Default"));AssetDatabase.CreateAsset(StrokeMaterial,path);}
            var prefabAsset=AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var prefab=prefabAsset?prefabAsset.GetComponent<PuzzleArrowView>():null;
            if(!prefab)
            {
                var go=new GameObject("PuzzleArrow");go.layer=LayerMask.NameToLayer("PuzzleBoard");var view=go.AddComponent<PuzzleArrowView>();
                U.Set(view,"_body",Line("Body",go.transform));U.Set(view,"_head",Line("Head",go.transform));
                prefab=PrefabUtility.SaveAsPrefabAsset(go,PrefabPath).GetComponent<PuzzleArrowView>();Object.DestroyImmediate(go);
            }
            AssetDatabase.SaveAssets();
            prefab=AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath).GetComponent<PuzzleArrowView>();
            PuzzleArrowArtwork.Upgrade();
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
            int puzzleLayer=LayerMask.NameToLayer("PuzzleBoard");
            if(puzzleLayer<0)throw new System.InvalidOperationException("Create the PuzzleBoard layer before building scenes.");
            cam.cullingMask=1<<puzzleLayer;
            string backdropPath=U.Data+"PuzzleBackdrop.mat";
            var backdropMat=AssetDatabase.LoadAssetAtPath<Material>(backdropPath);
            if(!backdropMat){backdropMat=new Material(Shader.Find("Sprites/Default"));backdropMat.color=new Color(.99f,.975f,.94f);AssetDatabase.CreateAsset(backdropMat,backdropPath);}
            var backdrop=GameObject.CreatePrimitive(PrimitiveType.Quad);backdrop.name="Cream Backdrop";backdrop.transform.position=new Vector3(0,0,5);backdrop.transform.localScale=new Vector3(500,500,1);
            backdrop.GetComponent<Renderer>().sharedMaterial=backdropMat;backdrop.GetComponent<Renderer>().sortingOrder=-100;Object.DestroyImmediate(backdrop.GetComponent<Collider>());
            backdrop.layer=puzzleLayer;
            var zoom=camera.AddComponent<ArrowSwarm.Camera.CameraController>();
            var light=new GameObject("Global Light",typeof(Light2D));light.GetComponent<Light2D>().lightType=Light2D.LightType.Global;
            new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
            var session=new GameObject("PuzzleSession").AddComponent<PuzzleSession>();
            var arrows=new GameObject("Pooled Arrows").transform;
            var frame=new GameObject("Rotating Frame").AddComponent<PuzzleFrame>();
            var walls=new LineRenderer[4];for(int i=0;i<4;i++)walls[i]=Line("Wall "+i,frame.transform);
            foreach(var wall in walls)wall.gameObject.layer=puzzleLayer;
            U.Set(frame,"_walls",walls);U.Set(session,"_catalog",AssetDatabase.LoadAssetAtPath<PuzzleCatalog>(U.Data+(mode==GameModeId.Shapes?"ShapesCatalog":"ColorShiftCatalog")+".asset"));
            U.Set(session,"_config",AssetDatabase.LoadAssetAtPath<PuzzleConfig>(U.Data+"PuzzleConfig.asset"));U.Set(session,"_mode",(int)mode);
            U.Set(session,"_arrowPrefab",prefab);U.Set(session,"_arrowRoot",arrows);U.Set(session,"_frame",frame);U.Set(session,"_cameraController",zoom);
            var input=session.gameObject.AddComponent<PuzzleInput>();U.Set(input,"_session",session);U.Set(input,"_camera",cam);
            BuildHUD(session,zoom);
            EditorSceneManager.SaveScene(scene,path);
        }

        private static LineRenderer Line(string name,Transform parent)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.layer=parent.gameObject.layer;var line=go.AddComponent<LineRenderer>();
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
            GameScenePuzzleUIRevision.BindScene(session, zoom);
        }
    }
}
