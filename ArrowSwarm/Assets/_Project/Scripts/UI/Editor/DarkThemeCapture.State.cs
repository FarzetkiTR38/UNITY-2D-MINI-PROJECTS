namespace ArrowSwarm.UI.Editor
{
    using System;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Modes.Editor;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using DebugManager=ArrowSwarm.Debug.DebugManager;

    /// <summary>Owns isolated player/debug state for the paired-theme visual audit.</summary>
    public static partial class DarkThemeCapture
    {
        private sealed class Scope:IDisposable
        {
            internal readonly PlayerData Player;
            private readonly QualityDataSnapshot _data;
            private readonly string _originalJson,_debugJson;
            private readonly DebugManager _debug;
            private readonly bool _createdDebug,_background,_preview;
            private readonly GameModeId _previewMode;
            private readonly int _previewLevel;
            private readonly float _time;
            private readonly UnityEngine.Random.State _random;
            private bool _disposed;
            internal Scope()
            {
                var data=DataManager.Instance;_originalJson=JsonUtility.ToJson(data.PlayerData);
                Player=JsonUtility.FromJson<PlayerData>(_originalJson);_data=new QualityDataSnapshot(data);_data.SetData(Player);
                Player.isTutorialCompleted=Player.isProfileSetupCompleted=true;
                _background=Application.runInBackground;_time=Time.timeScale;_random=UnityEngine.Random.state;
                _preview=PuzzleLaunch.Preview;_previewMode=PuzzleLaunch.PreviewMode;_previewLevel=PuzzleLaunch.PreviewLevel;
                _createdDebug=!DebugManager.HasInstance;_debug=DebugManager.Instance;_debugJson=JsonUtility.ToJson(_debug);
                var so=new SerializedObject(_debug);so.FindProperty("_useDebugLevel").boolValue=true;
                so.FindProperty("_debugLevel").intValue=101;so.FindProperty("_infiniteLives").boolValue=true;
                so.FindProperty("_gameSpeedMultiplier").floatValue=1;so.ApplyModifiedPropertiesWithoutUndo();
                Application.runInBackground=true;Time.timeScale=1;UnityEngine.Random.InitState(523101);
            }
            public void Dispose()
            {
                if(_disposed)return;_disposed=true;
                Time.timeScale=1;
                if(Application.isPlaying&&SceneManager.GetActiveScene().name!="MainMenuScene")SceneManager.LoadScene("MainMenuScene");
                var image=(UnityEngine.UI.Image)typeof(SceneTransitionManager).GetField("_overlayImage",Flags).GetValue(SceneTransitionManager.Instance);
                image.material.SetFloat("_Progress",0);image.gameObject.SetActive(false);image.raycastTarget=false;
                _data.Dispose();_data.VerifyRestored(Results);
                if(JsonUtility.ToJson(DataManager.Instance.PlayerData)!=_originalJson)throw new InvalidOperationException("Original player data changed.");
                if(_debug!=null){JsonUtility.FromJsonOverwrite(_debugJson,_debug);if(_createdDebug)UnityEngine.Object.DestroyImmediate(_debug.gameObject);}
                PuzzleLaunch.Preview=_preview;PuzzleLaunch.PreviewMode=_previewMode;PuzzleLaunch.PreviewLevel=_previewLevel;
                GameManager.Instance.SetState(GameState.Menu);ThemeManager.Instance.ApplyCurrentTheme();
                Time.timeScale=_time;Application.runInBackground=_background;UnityEngine.Random.state=_random;
                Results.Add("PASS: restored player, preferences, theme, debug, preview, random, time and background context");
            }
        }
    }
}
