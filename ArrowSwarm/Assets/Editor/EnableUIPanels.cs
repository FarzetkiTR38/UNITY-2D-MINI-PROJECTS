using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class EnableUIPanels
{
    static EnableUIPanels()
    {
        EditorApplication.delayCall += Execute;
    }

    public static void Execute()
    {
        if (Application.isPlaying) return;
        
        string scenePath = "Assets/_Project/Scenes/GameScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        bool changed = false;
        
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == "Canvas_Overlay")
            {
                Transform wp = root.transform.Find("WinPanel");
                if (wp != null && !wp.gameObject.activeSelf)
                {
                    wp.gameObject.SetActive(true);
                    Debug.Log("[ArrowSwarm] Enabled WinPanel in GameScene!");
                    changed = true;
                }
                
                Transform gop = root.transform.Find("LosePanel");
                if (gop != null && !gop.gameObject.activeSelf)
                {
                    gop.gameObject.SetActive(true);
                    Debug.Log("[ArrowSwarm] Enabled LosePanel in GameScene!");
                    changed = true;
                }
            }
        }
        
        if (changed)
        {
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[ArrowSwarm] Saved GameScene with active UI Panels!");
        }
    }
}
