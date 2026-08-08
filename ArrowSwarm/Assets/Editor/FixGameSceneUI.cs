using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public static class FixGameSceneUI
{
    [MenuItem("Tools/Fix Game Scene UI")]
    public static void Execute()
    {
        string scenePath = "Assets/_Project/Scenes/GameScene.unity";
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 1. Fix PauseMenuPanel Active State
        var pauseMenu = Object.FindFirstObjectByType<ArrowSwarm.UI.PauseMenuUI>(FindObjectsInactive.Include);
        if (pauseMenu != null)
        {
            pauseMenu.gameObject.SetActive(true);
            var cg = pauseMenu.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
            Debug.Log("PauseMenuPanel activated and CanvasGroup reset.");
        }

        // 2. Fix Zoom Slider (FOV)
        var zoomSlider = Object.FindFirstObjectByType<Slider>(FindObjectsInactive.Include);
        if (zoomSlider != null)
        {
            // Assuming it's the only slider, or it's named ZoomSlider
            RectTransform rt = zoomSlider.GetComponent<RectTransform>();
            if (rt != null)
            {
                zoomSlider.direction = Slider.Direction.LeftToRight; // Make it horizontal

                // Anchor to bottom center
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                
                // Set position and size
                rt.anchoredPosition = new Vector2(0f, 150f); // 150 pixels from bottom
                rt.sizeDelta = new Vector2(600f, 60f); // Wide and thin

                // Also we need to fix the RectTransforms of its children (Background, Fill Area)
                // because vertical sliders have different child rect settings than horizontal.
                // It's actually tricky to convert via script because of the internal handles.
                // We'll try our best:
                var fillArea = zoomSlider.fillRect;
                if (fillArea != null)
                {
                    fillArea.anchorMin = Vector2.zero;
                    fillArea.anchorMax = Vector2.one;
                    fillArea.offsetMin = Vector2.zero;
                    fillArea.offsetMax = Vector2.zero;
                }
            }
            Debug.Log("Zoom Slider moved to bottom and made horizontal.");
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("GameScene UI fixed and saved.");
    }
}
