namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Puzzle;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Captures real paired theme states without awarding progress or persisting settings.</summary>
    public static partial class DarkThemeCapture
    {
        private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
        private static readonly List<string> Results = new List<string>();
        private static Func<bool> _wait;
        private static Scope _scope;
        private static bool _running;
        private static double _deadline;
        private static string _folder;
        private static bool _supplement;
        private static bool _tail;
        private static bool _menusOnly;
        /// <summary>Current capture stage or final result.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Starts from an idle main menu and records every player-facing UI family in both themes.</summary>
        public static void Begin(string folder)
        { _supplement=false; _tail=false; _menusOnly=false; StartCapture(folder); }

        /// <summary>Captures gameplay/boot separately when completing or reviewing the visual baseline.</summary>
        public static void BeginSupplement(string folder)
        { _supplement=true; _tail=false; _menusOnly=false; StartCapture(folder); }

        /// <summary>Completes the optional map-preview and boot portion of an audit.</summary>
        public static void BeginTail(string folder)
        { _supplement=true; _tail=true; _menusOnly=false; StartCapture(folder); }

        /// <summary>Captures all menu families for another resolution without restarting gameplay fixtures.</summary>
        public static void BeginMenus(string folder)
        { _supplement=false; _tail=false; _menusOnly=true; StartCapture(folder); }

        private static void StartCapture(string folder)
        {
            if (_running || !Application.isPlaying || SceneManager.GetActiveScene().name != "MainMenuScene" ||
                SceneTransitionManager.Instance.IsTransitioning) throw new InvalidOperationException("Start once in idle MainMenuScene Play Mode.");
            _folder = folder; Directory.CreateDirectory(folder); Results.Clear(); Steps.Clear(); _wait = null;
            _scope = new Scope(); _running = true; _deadline = EditorApplication.timeSinceStartup+420;
            Steps.Push(Run()); Status="Capturing paired themes"; EditorApplication.update+=Tick;
            AssemblyReloadEvents.beforeAssemblyReload+=Cancel; EditorApplication.playModeStateChanged+=ModeChanged;
            SceneManager.sceneLoaded+=ConfigurePreview;
        }

        /// <summary>Cancels work and restores all test-owned runtime state.</summary>
        public static void Cancel() { if (_running) Finish("ABORT: theme capture interrupted"); }

        private static IEnumerator Run()
        {
            foreach (string screen in new[] { "Main", "ModeSelect", "Settings", "Profile", "Leaderboard", "LevelsDefense", "LevelsShapes", "LevelsColorShift", "Transition" })
                if(!_supplement) yield return Pair("menu-"+screen, theme=>StageMenu(screen,theme));
            if(_menusOnly)yield break;
            foreach (string scene in new[] { "GameScene", "ShapesScene", "ColorShiftScene", "MapTestScene" })
            {
                if(_tail&&scene!="MapTestScene")continue;
                Status="Loading "+scene; Time.timeScale=1;
                bool puzzle=scene=="ShapesScene"||scene=="ColorShiftScene";
                PuzzleLaunch.Preview=puzzle; PuzzleLaunch.PreviewLevel=15;
                PuzzleLaunch.PreviewMode=scene=="ShapesScene"?GameModeId.Shapes:GameModeId.ColorShift;
                SceneManager.LoadScene(scene);
                yield return (Func<bool>)(()=>puzzle ? PuzzleSession.Active!=null&&PuzzleSession.Active.Level!=null&&PuzzleSession.Active.Ready : DefenseReady());
                yield return Settled(); Time.timeScale=0;
                if (ArrowSwarm.Mob.MobSpawner.HasInstance) ArrowSwarm.Mob.MobSpawner.Instance.StopSpawning();
                string[] screens=puzzle?new[]{"Game","Pause","Win","Lose","Hint"}:
                    scene=="MapTestScene"?new[]{"Game","Pause"}:new[]{"Game","Pause","Win","Lose","Hint","Freeze","TutorialHand","TutorialComplete"};
                foreach(string screen in screens) yield return Pair(scene+"-"+screen,theme=>StageGame(screen,theme,puzzle));
            }
            Time.timeScale=0; SceneManager.LoadScene("BootScene");
            yield return Settled();
            var boot=UnityEngine.Object.FindFirstObjectByType<BootLoadingUI>();
            boot.StopAllCoroutines(); boot.SetProgress(.65f);
            typeof(BootLoadingUI).GetField("_animateCenterHero",Flags).SetValue(boot,false);
            yield return Pair("boot",theme=>DataManager.Instance.SetTheme(theme));
            Time.timeScale=1; SceneManager.LoadScene("MainMenuScene"); yield return Settled();
        }

        private static IEnumerator Pair(string label, Action<ThemeMode> stage)
        {
            Status=label+" / Light"; stage(ThemeMode.Light); yield return Settled();
            FreezeDecorations(); yield return Capture(label+"-light");
            var before=new GraphicState();
            Status=label+" / Dark"; stage(ThemeMode.Dark); yield return Settled();
            FreezeDecorations(); yield return Capture(label+"-dark");
            stage(ThemeMode.Light); yield return Settled();
            before.Verify(Results,label);
            if(label=="menu-Transition")
            {
                var manager=SceneTransitionManager.Instance;
                var image=(UnityEngine.UI.Image)typeof(SceneTransitionManager).GetField("_overlayImage",Flags).GetValue(manager);
                image.material.SetFloat("_Progress",0);image.gameObject.SetActive(false);
            }
        }

        private static IEnumerator Capture(string label)
        {
            string path=Path.Combine(_folder,label+"-"+DateTime.UtcNow.Ticks+".png"); int frame=Time.frameCount;
            Canvas.ForceUpdateCanvases(); ScreenCapture.CaptureScreenshot(path);
            yield return (Func<bool>)(()=>Time.frameCount>frame+2&&File.Exists(path)&&new FileInfo(path).Length>0);
            Results.Add("CAPTURE: "+path);
        }

        private static Func<bool> Settled()
        { float until=Time.unscaledTime+.5f; int frame=Time.frameCount; return ()=>Time.unscaledTime>until&&Time.frameCount>frame+3; }
        private static void Tick()
        {
            try
            {
                if (!_running) return;
                if (!Application.isPlaying||EditorApplication.timeSinceStartup>_deadline) throw new Exception("Timeout or Play Mode ended: "+Status);
                EditorApplication.QueuePlayerLoopUpdate(); if(_wait!=null){if(!_wait())return;_wait=null;}
                while(Steps.Count>0)
                {
                    var step=Steps.Peek(); if(!step.MoveNext()){Steps.Pop();continue;}
                    if(step.Current is IEnumerator nested){Steps.Push(nested);continue;}
                    _wait=step.Current as Func<bool>;return;
                }
                Finish("PASS: paired UI capture and exact Light restoration");
            }
            catch(Exception ex){Finish("FAIL: "+ex);}
        }
        private static void ModeChanged(PlayModeStateChange state)
        {if(state==PlayModeStateChange.ExitingPlayMode)Cancel();}
        private static void ConfigurePreview(Scene scene,LoadSceneMode mode)
        {
            if(scene.name!="MapTestScene")return;
            var controller=UnityEngine.Object.FindFirstObjectByType<MapSceneController>();
            var data=new SerializedObject(controller);data.FindProperty("_previewMode").enumValueIndex=0;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void Finish(string result)
        {
            _running=false;EditorApplication.update-=Tick;AssemblyReloadEvents.beforeAssemblyReload-=Cancel;
            EditorApplication.playModeStateChanged-=ModeChanged;
            SceneManager.sceneLoaded-=ConfigurePreview;
            try{while(Steps.Count>0)(Steps.Pop() as IDisposable)?.Dispose();_scope?.Dispose();}
            catch(Exception ex){result+=" / RESTORE FAILED: "+ex.Message;}
            _scope=null;_wait=null;Status=result;File.WriteAllText(Path.Combine(_folder,"capture-report.txt"),result+"\n"+string.Join("\n",Results));
        }
    }
}
