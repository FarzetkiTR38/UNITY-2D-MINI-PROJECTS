using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ArrowSwarm.Camera;

/// <summary>
/// Editor utility to setup Phase 5 Camera and UI Input.
/// Run via menu: ArrowSwarm → Setup Camera System (Phase 5)
/// </summary>
public static class CameraSystemSetup
{
    [MenuItem("ArrowSwarm/Setup Camera System (Phase 5)")]
    public static void Setup()
    {
        // 1. Setup CameraController
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            CameraController camController = mainCam.GetComponent<CameraController>();
            if (camController == null)
            {
                camController = mainCam.gameObject.AddComponent<CameraController>();
                Debug.Log("[ArrowSwarm] CameraController added to Main Camera.");
            }
        }
        else
        {
            Debug.LogError("[ArrowSwarm] Main Camera not found in the scene!");
        }

        // 2. Setup EventSystem if missing
        EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // 3. Setup Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        GameObject canvasObj;
        if (canvas == null)
        {
            canvasObj = new GameObject("Canvas_HUD");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("[ArrowSwarm] Canvas_HUD created.");
        }
        else
        {
            canvasObj = canvas.gameObject;
        }

        // 4. Create Slider if missing
        Slider slider = canvasObj.GetComponentInChildren<Slider>();
        if (slider == null)
        {
            DefaultControls.Resources uiResources = new DefaultControls.Resources();
            // Try to load standard built-in sprite for UI
            uiResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            uiResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            uiResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            GameObject sliderObj = DefaultControls.CreateSlider(uiResources);
            sliderObj.name = "ZoomSlider";
            sliderObj.transform.SetParent(canvasObj.transform, false);
            
            RectTransform rt = sliderObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
            rt.anchoredPosition = new Vector2(-50, 0); // Margin from right edge
            rt.sizeDelta = new Vector2(400, 40);
            rt.localEulerAngles = new Vector3(0, 0, 90); // Vertical slider
            
            slider = sliderObj.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f; // Zoom out completely initially
            
            Debug.Log("[ArrowSwarm] ZoomSlider created.");
        }

        // 5. Setup ZoomController
        ZoomController zc = canvasObj.GetComponent<ZoomController>();
        if (zc == null)
        {
            zc = canvasObj.AddComponent<ZoomController>();
        }
        
        SerializedObject serializedZc = new SerializedObject(zc);
        serializedZc.FindProperty("_zoomSlider").objectReferenceValue = slider;
        serializedZc.ApplyModifiedProperties();
        
        // Save scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[ArrowSwarm] Camera & Input System setup complete!");
    }
}
