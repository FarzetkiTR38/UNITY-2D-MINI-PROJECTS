using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ArrowSwarm.Core;

public static class SetupManagers
{
    [MenuItem("Tools/Setup Managers Prefab")]
    public static void Execute()
    {
        string scenePath = "Assets/_Project/Scenes/GameScene.unity";
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // Find all the manager objects
        var gameManager = Object.FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in GameScene!");
            return;
        }

        Transform managersRoot = gameManager.transform.parent != null ? gameManager.transform.parent : gameManager.transform;
        
        // If GameManager is not inside a parent called "Managers", create one
        if (managersRoot.name != "Managers")
        {
            GameObject newRoot = new GameObject("Managers");
            gameManager.transform.SetParent(newRoot.transform);
            
            // Try finding others and moving them
            var dataManager = Object.FindFirstObjectByType<ArrowSwarm.Data.DataManager>();
            if (dataManager != null) dataManager.transform.SetParent(newRoot.transform);
            
            var inputManager = Object.FindFirstObjectByType<ArrowSwarm.Core.InputManager>();
            if (inputManager != null) inputManager.transform.SetParent(newRoot.transform);
            
            var levelManager = Object.FindFirstObjectByType<LevelManager>();
            if (levelManager != null) levelManager.transform.SetParent(newRoot.transform);
            
            var gridManager = Object.FindFirstObjectByType<ArrowSwarm.Grid.GridManager>();
            if (gridManager != null) gridManager.transform.SetParent(newRoot.transform);
            
            var pathManager = Object.FindFirstObjectByType<ArrowSwarm.Path.PathManager>();
            if (pathManager != null) pathManager.transform.SetParent(newRoot.transform);
            
            var arrowSpawner = Object.FindFirstObjectByType<ArrowSwarm.Arrow.ArrowSpawner>();
            if (arrowSpawner != null) arrowSpawner.transform.SetParent(newRoot.transform);
            
            var mobSpawner = Object.FindFirstObjectByType<ArrowSwarm.Mob.MobSpawner>();
            if (mobSpawner != null) mobSpawner.transform.SetParent(newRoot.transform);

            managersRoot = newRoot.transform;
        }

        // Make sure the Prefabs folder exists
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
        }
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/Core"))
        {
            AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Core");
        }

        // Save as prefab
        string prefabPath = "Assets/_Project/Prefabs/Core/CoreManagers.prefab";
        PrefabUtility.SaveAsPrefabAsset(managersRoot.gameObject, prefabPath);
        Debug.Log("Saved CoreManagers prefab at " + prefabPath);

        // Delete from GameScene
        Object.DestroyImmediate(managersRoot.gameObject);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Deleted Managers from GameScene and saved.");
    }
}
