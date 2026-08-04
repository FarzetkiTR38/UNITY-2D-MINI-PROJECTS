using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class AddLevelTester
{
    [MenuItem("ArrowSwarm/Setup Final Phase/4. Add LevelTester")]
    public static void AddTester()
    {
        GameObject managers = GameObject.Find("Managers");
        if (managers != null)
        {
            if (managers.GetComponent<LevelTester>() == null)
            {
                managers.AddComponent<LevelTester>();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Debug.Log("LevelTester added!");
            }
        }
    }
}
