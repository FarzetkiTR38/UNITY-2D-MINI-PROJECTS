namespace ArrowSwarm.Modes.Editor
{
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.UI;
    using U = ModeAuthoringUtility;

    /// <summary>Non-runtime menu authoring using the supplied artwork and real interactive controls.</summary>
    public static class ModeMenuBuilder
    {
        [MenuItem("Arrow Swarm/Puzzles/Build Reference Main Menu")]
        public static void Build()
        {
            if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new System.InvalidOperationException("Save the current scene before rebuilding the menu.");
            U.Folder(U.Art+"Slices");
            var scene=EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainMenuScene.unity");
            var menu=Object.FindFirstObjectByType<MainMenuUI>(FindObjectsInactive.Include);
            var so=new SerializedObject(menu);
            var old=(GameObject)so.FindProperty("_mainPanel").objectReferenceValue;
            if(old.name=="ReferenceMainMenu")throw new System.InvalidOperationException("Reference menu already exists; edit its serialized objects in place.");
            old.name="LegacyMainPanel";old.SetActive(false);
            var main=U.Stretch("ReferenceMainMenu",old.transform.parent);
            U.Image(main,null,new Color(1f,.975f,.91f));
            var art=U.Rect("MenuArt",main,0,0,941,1672);art.gameObject.AddComponent<ReferenceCanvasFit>();
            U.Image(art,U.Full("MainMenuLive.png"),Color.white);
            var level=U.Text(U.Rect("SelectedLevel",art,220,94,185,70),"Lv.1",49,new Color(0,.2f,.95f));
            var stars=U.Text(U.Rect("SharedStars",art,645,94,185,70),"0",49,new Color(0,.2f,.95f));
            var play=Hotspot("PlayButton",art,135,815,675,203);
            var mode=Hotspot("GameModeButton",art,135,1037,675,150);
            var levels=Hotspot("LevelsButton",art,159,1207,625,120);
            var leader=Hotspot("LeaderboardButton",art,159,1340,625,120);
            var settings=Hotspot("SettingsButton",art,159,1473,625,120);
            var modeText=U.Text(U.Rect("SelectedMode",art,420,1123,280,40),"DEFENSE",25,new Color(.16f,.48f,.91f));
            U.Set(menu,"_mainPanel",main.gameObject);U.Set(menu,"_playButton",play);U.Set(menu,"_levelsButton",levels);
            U.Set(menu,"_leaderboardButton",leader);U.Set(menu,"_settingsButton",settings);U.Set(menu,"_levelText",level);U.Set(menu,"_starsText",stars);
            var selector=main.gameObject.AddComponent<GameModeSelectUI>();
            var overlay=U.Stretch("GameModeOverlay",main);var group=overlay.gameObject.AddComponent<CanvasGroup>();
            U.Image(overlay,null,new Color(0,0,0,.56f),true);
            var design=U.Rect("ModalDesign",overlay,0,0,941,1672);design.gameObject.AddComponent<ReferenceCanvasFit>();
            U.Image(U.Rect("ModalArtwork",design,22,270,900,1144),U.Slice("ModeModalReference.png","ModalPanel",22,270,900,1144),Color.white);
            var close=Hotspot("Close",design,790,315,90,90);
            var selected=U.Slice("ModeModalReference.png","Selected",572,1236,290,110);
            var select=U.Slice("ModeModalReference.png","Select",587,621,275,110);
            var choices=new Button[3];var states=new Image[3];var selectedStates=new Image[3];
            for(int i=0;i<3;i++)
            {
                int y=621+i*308;
                choices[i]=Hotspot(((GameModeId)i).ToString()+"Card",design,62,458+i*307,818,285);
                // Cover each baked selection button with its dynamic state artwork.
                states[i]=U.Image(U.Rect("SelectionState"+i,design,572,y,290,110),select,Color.white);
                selectedStates[i]=U.Image(U.Rect("SelectedState"+i,design,572,y,290,110),selected,Color.white);
            }
            U.Set(selector,"_group",group);U.Set(selector,"_open",mode);U.Set(selector,"_close",close);
            U.Set(selector,"_choices",choices);U.Set(selector,"_states",states);U.Set(selector,"_selectedStates",selectedStates);
            U.Set(selector,"_modeLabel",modeText);U.Set(selector,"_levelLabel",level);U.Set(selector,"_menuModeIcons",new Image[0]);
            group.alpha=0;group.interactable=group.blocksRaycasts=false;
            var levelUI=Object.FindFirstObjectByType<LevelSelectUI>(FindObjectsInactive.Include);
            U.Set(levelUI,"_shapesCatalog",AssetDatabase.LoadAssetAtPath<PuzzleCatalog>(U.Data+"ShapesCatalog.asset"));
            U.Set(levelUI,"_colorShiftCatalog",AssetDatabase.LoadAssetAtPath<PuzzleCatalog>(U.Data+"ColorShiftCatalog.asset"));
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("[ArrowSwarm] Reference menu authored; original subpanels preserved.");
        }
        private static Button Hotspot(string name,Transform parent,float x,float y,float w,float h)
        {var b=U.Button(U.Rect(name,parent,x,y,w,h),null);b.image.color=Color.clear;return b;}
    }
}
