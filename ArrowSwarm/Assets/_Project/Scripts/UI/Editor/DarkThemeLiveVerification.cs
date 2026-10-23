namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes.Editor;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Checks saved-theme binding refresh and live palette changes during both halves of a real paused wipe.</summary>
    public static class DarkThemeLiveVerification
    {
        private static readonly List<string> Results=new List<string>();
        private static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        private static IEnumerator _steps;
        private static Func<bool> _wait;
        private static double _deadline;
        private static string _folder;
        private static int _failed;
        /// <summary>Live verification progress or final result.</summary>
        public static string Status{get;private set;}="Idle";

        /// <summary>Starts with initialized managers and the real idle MainMenu in Play Mode.</summary>
        public static void Begin(string folder)
        {
            if(!Application.isPlaying||_steps!=null||UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="MainMenuScene"||
                !SceneTransitionManager.HasInstance||SceneTransitionManager.Instance.IsTransitioning||
                Field<Material>(SceneTransitionManager.Instance,"_customMaterialOverride")==null)throw new InvalidOperationException("Requires idle main menu reached through BootScene.");
            _folder=folder;Directory.CreateDirectory(folder);Results.Clear();_failed=0;_deadline=EditorApplication.timeSinceStartup+90;
            _steps=Run();Status="Testing live theme bindings and wipes";
            EditorApplication.update+=Tick;AssemblyReloadEvents.beforeAssemblyReload+=Cancel;
            EditorApplication.playModeStateChanged+=ModeChanged;
        }
        /// <summary>Restores owned state if the fixture is interrupted.</summary>
        public static void Cancel(){if(_steps!=null)Finish("ABORT: interrupted");}

        private static IEnumerator Run()
        {
            var data=DataManager.Instance;var original=data.PlayerData;
            float time=Time.timeScale;bool background=Application.runInBackground;
            var snapshot=new QualityDataSnapshot(data);
            var manager=SceneTransitionManager.Instance;
            var image=Field<Image>(manager,"_overlayImage");var runtime=image.material;
            var source=Field<Material>(manager,"_customMaterialOverride");string json=EditorJsonUtility.ToJson(source);
            try
            {
                snapshot.SetData(JsonUtility.FromJson<PlayerData>(JsonUtility.ToJson(original)));
                Application.runInBackground=true;Time.timeScale=0;
                CheckDynamicMenus(data);
                foreach(var binding in UnityEngine.Object.FindObjectsByType<ThemedUIElement>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                {
                    if(!binding.transform.root.name.StartsWith("Canvas_MainMenu")||!binding.transform.IsChildOf(GameObject.Find("Canvas_MainMenu/ReferenceMainMenu").transform))continue;
                    var go=binding.gameObject;bool active=go.activeSelf;var graphic=binding.GetComponent<Graphic>();
                    var so=new SerializedObject(binding);var dark=so.FindProperty("_darkMaterial").objectReferenceValue as Material;
                    go.SetActive(false);data.SetTheme(ThemeMode.Light);data.SetTheme(ThemeMode.Dark);go.SetActive(true);
                    if(dark!=null)Check(graphic.material==dark,go.name+" re-enables directly in saved Dark");
                    else Check(graphic.color==so.FindProperty("_customDarkColor").colorValue,go.name+" reapplies readable Dark text when re-enabled");
                    go.SetActive(active);
                }
                foreach(var theme in new[]{ThemeMode.Dark,ThemeMode.Light})
                {
                    data.SetTheme(theme);bool midpoint=false;int calls=0;
                    manager.PlayTransition(()=>{midpoint=true;calls++;},4f,new Vector2(.37f,.63f));
                    yield return (Func<bool>)(()=>!midpoint&&runtime.GetFloat("_Progress")>.2f&&runtime.GetFloat("_Progress")<.8f);
                    SwitchAndVerify(data,manager,image,theme==ThemeMode.Dark?ThemeMode.Light:ThemeMode.Dark,"closing");
                    SwitchAndVerify(data,manager,image,theme,"closing return");
                    yield return (Func<bool>)(()=>midpoint&&runtime.GetFloat("_Progress")>.2f&&runtime.GetFloat("_Progress")<.8f);
                    SwitchAndVerify(data,manager,image,theme==ThemeMode.Dark?ThemeMode.Light:ThemeMode.Dark,"opening");
                    SwitchAndVerify(data,manager,image,theme,"opening return");
                    yield return (Func<bool>)(()=>!manager.IsTransitioning);
                    Check(calls==1&&Time.timeScale==0,"Paused "+theme+" wipe executes once using real time");
                    Check(!image.gameObject.activeSelf&&!image.raycastTarget&&runtime.GetFloat("_Progress")==0,"Completed "+theme+" wipe releases overlay and input");
                    Check(image.material==runtime&&EditorJsonUtility.ToJson(source)==json,"Live "+theme+" changes retain one runtime instance and exact source asset");
                }
            }
            finally
            {
                manager.StopAllCoroutines();snapshot.Dispose();snapshot.VerifyRestored(Results);
                ThemeManager.Instance.ApplyCurrentTheme();Time.timeScale=time;Application.runInBackground=background;
                var menu=UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
                if(menu!=null)typeof(MainMenuUI).GetMethod("SetupUI",Flags).Invoke(menu,null);
                var modes=UnityEngine.Object.FindFirstObjectByType<GameModeSelectUI>(FindObjectsInactive.Include);
                if(modes!=null)typeof(GameModeSelectUI).GetMethod("Refresh",Flags).Invoke(modes,null);
            }
        }
        private static void CheckDynamicMenus(DataManager data)
        {
            var menu=UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
            var selector=UnityEngine.Object.FindFirstObjectByType<GameModeSelectUI>(FindObjectsInactive.Include);
            var settings=UnityEngine.Object.FindFirstObjectByType<SettingsUI>(FindObjectsInactive.Include);
            foreach(var theme in new[]{ThemeMode.Light,ThemeMode.Dark})
            {
                menu.OpenSettings();settings.SetTheme(theme);
                Check(Field<Image>(settings,"_themeSelectorImage").sprite==Field<Sprite>(settings,theme==ThemeMode.Dark?"_darkThemeSprite":"_lightThemeSprite"),"Settings owns the correct selected "+theme+" sprite");
            }
            menu.OpenMainPanel();data.SetTheme(ThemeMode.Dark);
            var labels=Field<TMPro.TMP_Text[]>(selector,"_choiceLabels");
            string selected=ArrowSwarm.Localization.LocalizationManager.Instance.GetText("modes_selected","SELECTED");
            foreach(ArrowSwarm.Modes.GameModeId mode in Enum.GetValues(typeof(ArrowSwarm.Modes.GameModeId)))
            {
                data.SelectMode(mode);selector.Show(true);int selectedCount=0;
                for(int i=0;i<labels.Length;i++)if(labels[i].text==selected){selectedCount++;Check(i==(int)mode,mode+" owns the selected localized label");}
                Check(selectedCount==1,mode+" has exactly one selected choice in Dark");selector.Show(false);
                if(mode!=ArrowSwarm.Modes.GameModeId.Defense)
                    JsonUtility.FromJsonOverwrite("{\"_highestLevel\":50}",data.PlayerData.GetModeProgress(mode));
                menu.OpenLevels();var levels=UnityEngine.Object.FindFirstObjectByType<LevelSelectUI>();
                var pool=Field<List<LevelButtonUI>>(levels,"_buttonPool");var originals=pool.ToArray();
                levels.PrevPage();levels.PrevPage();levels.NextPage();levels.PrevPage();
                bool stable=pool.Count==originals.Length,dark=true,colors=true;
                var tint=new Color[pool.Count];for(int i=0;i<pool.Count;i++)
                {stable&=pool[i]==originals[i];var graphic=pool[i].GetComponent<Image>();tint[i]=graphic.color;dark&=graphic.material.name=="DarkControl";}
                data.SetTheme(ThemeMode.Light);
                for(int i=0;i<pool.Count;i++)colors&=pool[i].GetComponent<Image>().color==tint[i];
                data.SetTheme(ThemeMode.Dark);
                Check(stable&&pool.Count==20,mode+" reuses all twenty level buttons across page changes");
                Check(dark&&colors,mode+" recycled buttons retain Dark materials and dynamic unlocked/locked tints");
                menu.OpenMainPanel();
            }
            menu.OpenLeaderboard();var leaderboard=UnityEngine.Object.FindFirstObjectByType<LeaderboardUI>();
            leaderboard.RefreshLeaderboardData();leaderboard.RefreshLeaderboardData();
            bool themed=true;int count=0;
            foreach(var binding in leaderboard.GetComponentsInChildren<ThemedUIElement>(true))
            {var so=new SerializedObject(binding);var material=so.FindProperty("_darkMaterial").objectReferenceValue as Material;
                if(material!=null){count++;themed&=binding.GetComponent<Graphic>().material==material;}}
            Check(themed&&count>0,"Leaderboard refresh retains all bound Dark materials");menu.OpenMainPanel();
        }
        private static void SwitchAndVerify(DataManager data,SceneTransitionManager manager,Image image,ThemeMode theme,string context)
        {
            var material=image.material;float progress=material.GetFloat("_Progress"),aspect=material.GetFloat("_AspectRatio");
            var center=material.GetVector("_Center");bool active=image.gameObject.activeSelf,block=image.raycastTarget;
            data.SetTheme(theme);
            Check(manager.IsTransitioning&&image.material==material&&material.GetFloat("_Progress")==progress&&
                material.GetFloat("_AspectRatio")==aspect&&material.GetVector("_Center")==center&&
                image.gameObject.activeSelf==active&&image.raycastTarget==block,context+" switch preserves animation geometry and input ownership");
            Check(material.GetColor("_Color")==Field<Color>(manager,theme==ThemeMode.Dark?"_darkBackgroundColor":"_backgroundColor"),context+" applies "+theme+" immediately");
        }
        private static T Field<T>(object owner,string name)=>(T)owner.GetType().GetField(name,Flags).GetValue(owner);
        private static void Check(bool condition,string label){Results.Add((condition?"PASS: ":"FAIL: ")+label);if(!condition)_failed++;}
        private static void ModeChanged(PlayModeStateChange state){if(state==PlayModeStateChange.ExitingPlayMode)Cancel();}
        private static void Tick()
        {
            try
            {
                if(!Application.isPlaying||EditorApplication.timeSinceStartup>_deadline)throw new TimeoutException(Status);
                EditorApplication.QueuePlayerLoopUpdate();if(_wait!=null){if(!_wait())return;_wait=null;}
                if(!_steps.MoveNext()){Finish((_failed==0?"PASS":"FAIL")+": "+Results.Count+" live theme checks; failures="+_failed);return;}
                _wait=_steps.Current as Func<bool>;
            }
            catch(Exception ex){Finish("FAIL: "+ex);}
        }
        private static void Finish(string result)
        {
            EditorApplication.update-=Tick;AssemblyReloadEvents.beforeAssemblyReload-=Cancel;EditorApplication.playModeStateChanged-=ModeChanged;
            (_steps as IDisposable)?.Dispose();_steps=null;_wait=null;Status=result;
            File.WriteAllText(Path.Combine(_folder,"live-theme.txt"),result+"\n"+string.Join("\n",Results));
        }
    }
}
