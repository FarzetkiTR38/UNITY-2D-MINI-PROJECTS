using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ArrowSwarm.Core;
using ArrowSwarm.Debug;

public static class FinalPhaseSetup
{
    [MenuItem("ArrowSwarm/Setup Final Phase/1. Create Boot Scene")]
    public static void CreateBootScene()
    {
        UnityEngine.SceneManagement.Scene currentScene = EditorSceneManager.GetActiveScene();
        if (currentScene.isDirty) EditorSceneManager.SaveScene(currentScene);

        UnityEngine.SceneManagement.Scene bootScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        GameObject bootManager = new GameObject("BootManager");
        bootManager.AddComponent<BootLoader>();
        
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        camObj.tag = "MainCamera";

        string scenePath = "Assets/_Project/Scenes/BootScene.unity";
        EditorSceneManager.SaveScene(bootScene, scenePath);
        
        Debug.Log("[ArrowSwarm] BootScene created and saved!");
    }

    [MenuItem("ArrowSwarm/Setup Final Phase/2. Setup Build Settings")]
    public static void SetupBuildSettings()
    {
        EditorBuildSettingsScene[] newSettings = new EditorBuildSettingsScene[3];
        newSettings[0] = new EditorBuildSettingsScene("Assets/_Project/Scenes/BootScene.unity", true);
        newSettings[1] = new EditorBuildSettingsScene("Assets/_Project/Scenes/MainMenuScene.unity", true);
        newSettings[2] = new EditorBuildSettingsScene("Assets/_Project/Scenes/GameScene.unity", true);
        
        EditorBuildSettings.scenes = newSettings;
        Debug.Log("[ArrowSwarm] Build Settings configured for 3 scenes!");
    }

    [MenuItem("ArrowSwarm/Setup Final Phase/3. Setup GameScene DebugManager")]
    public static void SetupGameSceneDebug()
    {
        GameObject managers = GameObject.Find("Managers");
        if (managers != null)
        {
            var debugMgr = managers.GetComponent<DebugManager>();
            if (debugMgr == null) debugMgr = managers.AddComponent<DebugManager>();
            
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[ArrowSwarm] DebugManager added to GameScene Managers!");
        }
        else
        {
            Debug.LogError("Managers object not found. Are you in GameScene?");
        }
    }
}
