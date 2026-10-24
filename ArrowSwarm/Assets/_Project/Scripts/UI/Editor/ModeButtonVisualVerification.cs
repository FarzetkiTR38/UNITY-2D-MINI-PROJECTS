namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Modes.Editor;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Exercises production mode-selection callbacks and paired artwork without persisting player changes.</summary>
    public static class ModeButtonVisualVerification
    {
        private static readonly List<string> Results=new List<string>();
        private const BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
        private static IEnumerator _steps;
        private static Func<bool> _wait;
        private static string _folder;
        private static double _deadline;
        private static int _failures;
        /// <summary>Current fixture stage or completed result.</summary>
        public static string Status{get;private set;}="Idle";

        /// <summary>Starts in the initialized main menu and captures all six theme/mode combinations.</summary>
        public static void Begin(string folder)
        {
            if(!Application.isPlaying||_steps!=null||UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="MainMenuScene")
                throw new InvalidOperationException("Requires the idle main menu in Play Mode.");
            _folder=folder;Directory.CreateDirectory(folder);Results.Clear();_failures=0;
            _deadline=EditorApplication.timeSinceStartup+90;_steps=Run();Status="Testing mode artwork";
            EditorApplication.update+=Tick;AssemblyReloadEvents.beforeAssemblyReload+=Cancel;
            EditorApplication.playModeStateChanged+=ModeChanged;
        }
        private static IEnumerator Run()
        {
            var data=DataManager.Instance;var snapshot=new QualityDataSnapshot(data);
            var original=data.PlayerData;float time=Time.timeScale;bool background=Application.runInBackground;
            var visual=UnityEngine.Object.FindFirstObjectByType<ModeButtonVisual>();
            var image=visual.GetComponent<Image>();var button=visual.GetComponent<Button>();
            var selector=UnityEngine.Object.FindFirstObjectByType<GameModeSelectUI>();
            var choices=(Button[])typeof(GameModeSelectUI).GetField("_choices",Flags).GetValue(selector);
            var labels=(TMP_Text[])typeof(GameModeSelectUI).GetField("_choiceLabels",Flags).GetValue(selector);
            try
            {
                snapshot.SetData(JsonUtility.FromJson<PlayerData>(JsonUtility.ToJson(original)));
                Application.runInBackground=true;Time.timeScale=0;
                Check(button.targetGraphic==image&&image.raycastTarget,"Original button remains the single pointer and color-tint target");
                foreach(var theme in new[]{ThemeMode.Light,ThemeMode.Dark})
                {
                    data.SetTheme(theme);
                    for(int mode=0;mode<3;mode++)
                    {
                        Status=theme+" / "+(GameModeId)mode;selector.Show(true);yield return Settled();
                        choices[mode].onClick.Invoke();yield return Settled();
                        Check(GameModeSelection.Selected==(GameModeId)mode,"Production selection callback selects "+Status);
                        var so=new SerializedObject(visual);
                        var expected=so.FindProperty(theme==ThemeMode.Dark?"_darkModes":"_lightModes").GetArrayElementAtIndex(mode).objectReferenceValue;
                        Check(image.sprite==expected,"Matching authored mode/theme sprite appears immediately: "+Status);
                        string selected=ArrowSwarm.Localization.LocalizationManager.Instance.GetText("modes_selected","SELECTED");
                        int count=0;foreach(var label in labels)if(label.text==selected)count++;
                        Check(count==1&&labels[mode].text==selected,"Exactly one localized selected label: "+Status);
                        selector.Show(false);yield return Settled();
                        visual.enabled=false;visual.enabled=true;
                        Check(image.sprite==expected&&image.color==Color.white,"Re-enabling preserves selection and authored tint: "+Status);
                        string path=Path.Combine(_folder,theme+"-"+(GameModeId)mode+".png");
                        int frame=Time.frameCount;ScreenCapture.CaptureScreenshot(path);
                        yield return (Func<bool>)(()=>Time.frameCount>frame+2&&File.Exists(path)&&new FileInfo(path).Length>0);
                        Results.Add("CAPTURE: "+path);
                    }
                }
            }
            finally
            {
                selector.Show(false);snapshot.Dispose();snapshot.VerifyRestored(Results);
                ThemeManager.Instance.ApplyCurrentTheme();visual.Refresh();
                typeof(GameModeSelectUI).GetMethod("Refresh",Flags).Invoke(selector,null);
                Time.timeScale=time;Application.runInBackground=background;
            }
        }
        private static Func<bool> Settled()
        {float until=Time.unscaledTime+.4f;int frame=Time.frameCount;return ()=>Time.unscaledTime>=until&&Time.frameCount>frame+3;}
        private static void Check(bool result,string label)
        {Results.Add((result?"PASS: ":"FAIL: ")+label);if(!result)_failures++;}
        private static void Tick()
        {
            try
            {
                if(_steps==null)return;
                if(!Application.isPlaying||EditorApplication.timeSinceStartup>_deadline)throw new TimeoutException(Status);
                EditorApplication.QueuePlayerLoopUpdate();if(_wait!=null){if(!_wait())return;_wait=null;}
                if(!_steps.MoveNext()){Finish((_failures==0?"PASS":"FAIL")+": mode artwork; failures="+_failures);return;}
                _wait=_steps.Current as Func<bool>;
            }
            catch(Exception ex){Finish("FAIL: "+ex.Message);}
        }
        /// <summary>Cancels the fixture and restores its temporary player context.</summary>
        public static void Cancel(){if(_steps!=null)Finish("ABORT: mode artwork interrupted");}
        private static void ModeChanged(PlayModeStateChange change){if(change==PlayModeStateChange.ExitingPlayMode)Cancel();}
        private static void Finish(string result)
        {
            EditorApplication.update-=Tick;AssemblyReloadEvents.beforeAssemblyReload-=Cancel;
            EditorApplication.playModeStateChanged-=ModeChanged;
            try{(_steps as IDisposable)?.Dispose();}catch(Exception ex){result+=" / RESTORE FAILED: "+ex.Message;}
            _steps=null;_wait=null;Status=result;
            File.WriteAllText(Path.Combine(_folder,"mode-artwork.txt"),result+"\n"+string.Join("\n",Results));
        }
    }
}
