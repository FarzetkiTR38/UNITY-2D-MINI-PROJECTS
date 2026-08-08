using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ArrowSwarm.Core;
using ArrowSwarm.Data;
using ArrowSwarm.Grid;
using ArrowSwarm.Path;
using ArrowSwarm.Arrow;
using ArrowSwarm.Mob;

[InitializeOnLoad]
public static class BuildCoreManagers
{
    static BuildCoreManagers()
    {
        EditorApplication.delayCall += Execute;
    }

    [MenuItem("Tools/Build Core Managers")]
    public static void Execute()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        // 1. Build Prefab
        string prefabPath = "Assets/_Project/Prefabs/Core/CoreManagers.prefab";
        
        GameObject root = new GameObject("CoreManagers");
        var gameManager = root.AddComponent<GameManager>();
        var dataManager = root.AddComponent<DataManager>();
        var inputManager = root.AddComponent<InputManager>();
        var levelManager = root.AddComponent<LevelManager>();
        var gridManager = root.AddComponent<GridManager>();
        var pathManager = root.AddComponent<PathManager>();
        var arrowSpawner = root.AddComponent<ArrowSpawner>();
        var mobSpawner = root.AddComponent<MobSpawner>();

        var configAsset = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/_Project/ScriptableObjects/GameConfig.asset");
        if (configAsset != null)
        {
            SerializedObject so = new SerializedObject(gameManager);
            so.Update();
            so.FindProperty("_gameConfig").objectReferenceValue = configAsset;
            so.ApplyModifiedProperties();
        }

        var arrowPrefab = AssetDatabase.LoadAssetAtPath<Arrow>("Assets/_Project/Prefabs/Arrow.prefab");
        if (arrowPrefab != null)
        {
            SerializedObject so = new SerializedObject(arrowSpawner);
            so.Update();
            so.FindProperty("_arrowPrefab").objectReferenceValue = arrowPrefab;
            so.ApplyModifiedProperties();
        }

        var mobPrefab = AssetDatabase.LoadAssetAtPath<Mob>("Assets/_Project/Prefabs/Mob.prefab");
        if (mobPrefab != null)
        {
            SerializedObject so = new SerializedObject(mobSpawner);
            so.Update();
            so.FindProperty("_mobPrefab").objectReferenceValue = mobPrefab;
            so.ApplyModifiedProperties();
        }

        if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
        }
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/Core"))
        {
            AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Core");
        }

        GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        Debug.Log("[ArrowSwarm] CoreManagers.prefab created at " + prefabPath);

        // 2. Inject into GameScene
        InjectPrefabIntoScene("Assets/_Project/Scenes/GameScene.unity", prefabAsset);

        // 3. Inject into BootScene
        InjectPrefabIntoScene("Assets/_Project/Scenes/BootScene.unity", prefabAsset);

        AssetDatabase.SaveAssets();
    }

    private static void InjectPrefabIntoScene(string scenePath, GameObject prefab)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var existing = Object.FindFirstObjectByType<GameManager>();
        if (existing == null)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[ArrowSwarm] CoreManagers prefab injected into {scenePath}");
        }
    }
}
