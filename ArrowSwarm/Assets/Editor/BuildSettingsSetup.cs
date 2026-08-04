using UnityEditor;
using UnityEngine;

public static class BuildSettingsSetup
{
    [MenuItem("ArrowSwarm/Setup UI/4. Fix Build Settings")]
    public static void FixBuildSettings()
    {
        EditorBuildSettingsScene[] original = EditorBuildSettings.scenes;
        EditorBuildSettingsScene[] newSettings = new EditorBuildSettingsScene[2];
        
        newSettings[0] = new EditorBuildSettingsScene("Assets/_Project/Scenes/MainMenuScene.unity", true);
        newSettings[1] = new EditorBuildSettingsScene("Assets/_Project/Scenes/GameScene.unity", true);
        
        EditorBuildSettings.scenes = newSettings;
        Debug.Log("[ArrowSwarm] Build settings updated with MainMenuScene and GameScene!");
    }
}
