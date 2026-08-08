using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ArrowSwarm.Core;

public static class SetupBootScene
{
    [MenuItem("Tools/Setup Boot Scene")]
    public static void Execute()
    {
        string scenePath = "Assets/_Project/Scenes/BootScene.unity";
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        var bootLoader = Object.FindFirstObjectByType<BootLoader>();
        if (bootLoader == null)
        {
            Debug.LogError("BootLoader not found in BootScene!");
            return;
        }

        string prefabPath = "Assets/_Project/Prefabs/Core/CoreManagers.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError("CoreManagers prefab not found at " + prefabPath);
            return;
        }

        SerializedObject so = new SerializedObject(bootLoader);
        so.Update();
        so.FindProperty("_coreManagersPrefab").objectReferenceValue = prefab;
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(bootLoader);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Assigned CoreManagers prefab to BootLoader and saved BootScene.");
    }
}
