using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ArrowSwarm.Core;
using ArrowSwarm.UI;

public static class FixReferences
{
    [MenuItem("Tools/Fix References")]
    public static void Execute()
    {
        // 1. Fix GameManager Config in Prefab
        string prefabPath = "Assets/_Project/Prefabs/Core/CoreManagers.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null)
        {
            var gameManager = prefab.GetComponentInChildren<GameManager>(true);
            if (gameManager != null)
            {
                string configPath = "Assets/_Project/ScriptableObjects/GameConfig.asset";
                var configAsset = AssetDatabase.LoadAssetAtPath<GameConfig>(configPath);
                
                SerializedObject so = new SerializedObject(gameManager);
                so.Update();
                so.FindProperty("_gameConfig").objectReferenceValue = configAsset;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(prefab);
                Debug.Log("GameManager GameConfig reference fixed in CoreManagers prefab.");
            }
        }
        else
        {
            Debug.LogError("CoreManagers prefab not found!");
        }

        // 2. Fix PauseMenuUI CanvasGroup in GameScene
        string scenePath = "Assets/_Project/Scenes/GameScene.unity";
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        var pauseMenu = Object.FindFirstObjectByType<PauseMenuUI>(FindObjectsInactive.Include);
        if (pauseMenu != null)
        {
            var canvasGroup = pauseMenu.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = pauseMenu.gameObject.AddComponent<CanvasGroup>();
            }
            
            SerializedObject so = new SerializedObject(pauseMenu);
            so.Update();
            so.FindProperty("_canvasGroup").objectReferenceValue = canvasGroup;
            so.ApplyModifiedProperties();
            
            EditorUtility.SetDirty(pauseMenu);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("PauseMenuUI CanvasGroup reference fixed in GameScene.");
        }
        else
        {
            Debug.LogError("PauseMenuUI not found in GameScene!");
        }

        AssetDatabase.SaveAssets();
    }
}
