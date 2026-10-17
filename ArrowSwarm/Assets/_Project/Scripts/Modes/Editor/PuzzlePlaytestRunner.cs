namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Data;
    using ArrowSwarm.Puzzle;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Plays real pooled animations through both campaigns in non-saving preview mode.</summary>
    public static class PuzzlePlaytestRunner
    {
        private static int _modeIndex,_level,_step,_stage;
        private static double _deadline,_pauseUntil,_captureUntil;
        private static bool _initializing;
        private static PuzzleSolution _solution;
        private static string _savedData;
        private static PuzzleConfig _testConfig;
        private static readonly List<string> Results=new List<string>();
        public static string Status { get; private set; }="Idle";
        private static GameModeId Mode=>_modeIndex==0?GameModeId.Shapes:GameModeId.ColorShift;

        /// <summary>Starts only in Play Mode; never writes real campaign rewards.</summary>
        [MenuItem("Arrow Swarm/Puzzles/Playtest All Levels (Play Mode)")]
        public static void Begin()
        {
            if(!Application.isPlaying)throw new InvalidOperationException("Enter Play Mode first.");
            EditorApplication.update-=Tick;Results.Clear();_modeIndex=0;_level=1;_stage=0;
            _savedData=JsonUtility.ToJson(DataManager.Instance.PlayerData);
            Directory.CreateDirectory("Captures/ThreeModes");
            LoadCampaign();EditorApplication.update+=Tick;
        }
        private static void LoadCampaign()
        {
            PuzzleLaunch.Preview=true;PuzzleLaunch.PreviewMode=Mode;PuzzleLaunch.PreviewLevel=1;
            _initializing=true;_solution=null;_deadline=EditorApplication.timeSinceStartup+180;
            SceneManager.LoadScene(Mode==GameModeId.Shapes?"ShapesScene":"ColorShiftScene");
        }
        private static void Tick()
        {
            try
            {
                if(!Application.isPlaying){Finish("Stopped before completion");return;}
                if(EditorApplication.timeSinceStartup>_deadline)throw new Exception("Playtest timed out: "+Status);
                var s=PuzzleSession.Active;
                if(s==null || s.Level==null || s.Mode!=Mode)return;
                Require(s.Preview,"playthrough must never award progress");
                if(_initializing)
                {
                    _initializing=false;Time.timeScale=10;Application.runInBackground=true;
                    if(_testConfig)UnityEngine.Object.DestroyImmediate(_testConfig);
                    _testConfig=UnityEngine.Object.Instantiate(s.Config);
                    var config=new SerializedObject(_testConfig);config.FindProperty("_speed").floatValue=1000;config.FindProperty("_rotationDuration").floatValue=.01f;config.ApplyModifiedPropertiesWithoutUndo();
                    var session=new SerializedObject(s);session.FindProperty("_config").objectReferenceValue=_testConfig;session.ApplyModifiedPropertiesWithoutUndo();
                    Require(s.LevelNumber==1 && s.Lives==3,"fresh campaign state");
                }
                if(_stage==1)
                {
                    if(EditorApplication.timeSinceStartup<_pauseUntil)return;
                    Require(s.Paused && Time.timeScale==0,"pause stops time");s.Pause(false);Time.timeScale=10;_stage=2;return;
                }
                if(s.Lost)throw new Exception("Unexpected loss in solution witness");
                if(s.Won)
                {
                    Require(s.Lives==3 && s.Remaining==0,"perfect witness clears all arrows");
                    Require(JsonUtility.ToJson(DataManager.Instance.PlayerData)==_savedData,"preview changed player progress");
                    Results.Add(Mode+" "+_level+": PASS actual animations, 3 lives, "+s.Turns+" turns left, no save writes");
                    if(_level<5){_level++;s.LoadLevel(_level);_solution=null;_deadline=EditorApplication.timeSinceStartup+180;return;}
                    if(_modeIndex==0){_modeIndex=1;_level=1;_stage=0;LoadCampaign();return;}
                    Finish("PASS all ten runtime playthroughs");return;
                }
                if(!s.Ready)return;
                Time.timeScale=10;
                if(_solution==null)
                {
                    _solution=PuzzleSolver.Solve(s.Level);Require(_solution.Solvable,"level witness");_step=0;
                    ScreenCapture.CaptureScreenshot("Captures/ThreeModes/"+Mode.ToString().ToLowerInvariant()+"-"+_level+".png");
                    _captureUntil=EditorApplication.timeSinceStartup+.15;return;
                }
                if(EditorApplication.timeSinceStartup<_captureUntil)return;
                Status=Mode+" "+_level+" action "+_step+"/"+_solution.Steps.Count;
                if(_step>=_solution.Steps.Count)return;
                int action=_solution.Steps[_step++];
                if(action<0)s.Rotate(1);else s.Fire(action);
                if(_stage==0){_stage=1;s.Pause(true);_pauseUntil=EditorApplication.timeSinceStartup+.2;}
            }
            catch(Exception ex){Finish("FAIL: "+ex.Message);Debug.LogException(ex);}
        }
        private static void Require(bool value,string message){if(!value)throw new Exception(message);}
        private static void Finish(string status)
        {
            EditorApplication.update-=Tick;Time.timeScale=1;Status=status;
            File.WriteAllText("Captures/ThreeModes/runtime-verification.txt",status+"\n"+string.Join("\n",Results));
            Debug.Log("[ArrowSwarm] "+status);
        }
    }
}
