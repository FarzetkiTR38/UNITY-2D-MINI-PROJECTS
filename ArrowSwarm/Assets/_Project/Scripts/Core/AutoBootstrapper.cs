namespace ArrowSwarm.Core
{
    using UnityEngine;

    /// <summary>
    /// Ensures that CoreManagers are loaded even if the game is started directly
    /// from GameScene or MainMenuScene in the Editor.
    /// </summary>
    public static class AutoBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureCoreManagers()
        {
            // Try to find if GameManager already exists (meaning CoreManagers is loaded)
            if (Object.FindFirstObjectByType<GameManager>() != null)
            {
                return;
            }

            // Load CoreManagers prefab from Resources or AssetDatabase
            GameObject prefab = null;

#if UNITY_EDITOR
            // In the Editor, we can load it directly from the path so we don't have to move it to a Resources folder.
            string[] guids = UnityEditor.AssetDatabase.FindAssets("CoreManagers t:GameObject");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
#else
            // In a built game, we assume BootScene is always loaded first.
            // If you want it to work in builds without BootScene, move the prefab to a Resources folder
            // and uncomment the line below.
            // prefab = Resources.Load<GameObject>("CoreManagers");
#endif

            if (prefab != null)
            {
                GameObject instance = Object.Instantiate(prefab);
                Object.DontDestroyOnLoad(instance);
                Debug.Log("[ArrowSwarm] AutoBootstrapper: CoreManagers prefab automatically instantiated.");
            }
            else
            {
                Debug.LogWarning("[ArrowSwarm] AutoBootstrapper: CoreManagers prefab not found! Make sure it exists.");
            }
        }
    }
}
