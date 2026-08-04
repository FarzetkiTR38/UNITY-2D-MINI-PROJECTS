using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using ArrowSwarm.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using ArrowSwarm.Camera;

/// <summary>
/// Editor utility to setup Phase 6 UI System.
/// </summary>
public static class UISetup
{
    [MenuItem("ArrowSwarm/Setup UI/1. Setup MainMenu Scene")]
    public static void SetupMainMenu()
    {
        string scenePath = "Assets/_Project/Scenes/MainMenuScene.unity";
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // Main Camera
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
        
        // Canvas
        GameObject canvasObj = new GameObject("Canvas_MainMenu");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // EventSystem
        GameObject esObj = new GameObject("EventSystem");
        esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // MainMenuUI Script
        MainMenuUI menuUI = canvasObj.AddComponent<MainMenuUI>();
        
        // Title
        GameObject titleObj = CreateText(canvasObj.transform, "TitleText", "ARROW SWARM", 100, new Vector2(0, 400));
        
        // Buttons
        GameObject playBtn = CreateButton(canvasObj.transform, "PlayButton", "PLAY", new Vector2(0, 100));
        GameObject settingsBtn = CreateButton(canvasObj.transform, "SettingsButton", "SETTINGS", new Vector2(0, -100));
        GameObject leaderBtn = CreateButton(canvasObj.transform, "LeaderboardButton", "LEADERBOARD", new Vector2(0, -300));
        
        // Fake panels for Settings and Leaderboard
        GameObject settingsPanel = CreatePanel(canvasObj.transform, "SettingsPanel");
        SettingsUI settingsUI = settingsPanel.AddComponent<SettingsUI>();
        GameObject closeSettings = CreateButton(settingsPanel.transform, "CloseBtn", "X", new Vector2(400, 800), new Vector2(100, 100));
        settingsPanel.SetActive(false);
        
        GameObject leaderPanel = CreatePanel(canvasObj.transform, "LeaderboardPanel");
        LeaderboardUI leaderUI = leaderPanel.AddComponent<LeaderboardUI>();
        GameObject closeLeader = CreateButton(leaderPanel.transform, "CloseBtn", "X", new Vector2(400, 800), new Vector2(100, 100));
        leaderPanel.SetActive(false);
        
        // Link references
        SerializedObject so = new SerializedObject(menuUI);
        so.FindProperty("_playButton").objectReferenceValue = playBtn.GetComponent<Button>();
        so.FindProperty("_settingsButton").objectReferenceValue = settingsBtn.GetComponent<Button>();
        so.FindProperty("_leaderboardButton").objectReferenceValue = leaderBtn.GetComponent<Button>();
        so.FindProperty("_settingsPanel").objectReferenceValue = settingsPanel;
        so.FindProperty("_leaderboardPanel").objectReferenceValue = leaderPanel;
        so.ApplyModifiedProperties();
        
        SerializedObject soSet = new SerializedObject(settingsUI);
        soSet.FindProperty("_backButton").objectReferenceValue = closeSettings.GetComponent<Button>();
        soSet.ApplyModifiedProperties();
        
        SerializedObject soLead = new SerializedObject(leaderUI);
        soLead.FindProperty("_backButton").objectReferenceValue = closeLeader.GetComponent<Button>();
        soLead.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[ArrowSwarm] MainMenuScene created and saved!");
    }

    [MenuItem("ArrowSwarm/Setup UI/2. Setup GameScene UI")]
    public static void SetupGameSceneUI()
    {
        // Assume GameScene is open
        Canvas canvasHud = Object.FindFirstObjectByType<Canvas>();
        if (canvasHud == null || canvasHud.name != "Canvas_HUD")
        {
            Debug.LogError("Please open GameScene and ensure Canvas_HUD exists (from Phase 5).");
            return;
        }

        // Setup GameHUD
        GameHUD hud = canvasHud.gameObject.GetComponent<GameHUD>();
        if (hud == null) hud = canvasHud.gameObject.AddComponent<GameHUD>();
        
        GameObject topBar = CreatePanel(canvasHud.transform, "TopBar", new Vector2(0, 800), new Vector2(1080, 200));
        topBar.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
        GameObject levelText = CreateText(topBar.transform, "LevelText", "Lv.1", 60, new Vector2(-400, 0));
        GameObject pauseBtn = CreateButton(topBar.transform, "PauseButton", "||", new Vector2(400, 0), new Vector2(120, 120));
        GameObject tipText = CreateText(topBar.transform, "TipText", "x3", 40, new Vector2(200, 0));
        
        GameObject heartsContainer = new GameObject("Hearts");
        heartsContainer.transform.SetParent(topBar.transform, false);
        heartsContainer.AddComponent<RectTransform>().anchoredPosition = new Vector2(-150, 0);
        Image[] hearts = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject h = new GameObject($"Heart_{i}");
            h.transform.SetParent(heartsContainer.transform, false);
            hearts[i] = h.AddComponent<Image>();
            hearts[i].rectTransform.anchoredPosition = new Vector2(i * 60, 0);
            hearts[i].rectTransform.sizeDelta = new Vector2(50, 50);
        }
        
        GameObject botBar = CreatePanel(canvasHud.transform, "BottomBar", new Vector2(0, -800), new Vector2(1080, 200));
        botBar.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
        GameObject arrowText = CreateText(botBar.transform, "ArrowText", "Arrows: 0/16", 60, new Vector2(0, 0));
        
        SerializedObject soHud = new SerializedObject(hud);
        soHud.FindProperty("_levelText").objectReferenceValue = levelText.GetComponent<TextMeshProUGUI>();
        soHud.FindProperty("_pauseButton").objectReferenceValue = pauseBtn.GetComponent<Button>();
        soHud.FindProperty("_arrowCountText").objectReferenceValue = arrowText.GetComponent<TextMeshProUGUI>();
        soHud.FindProperty("_tipCountText").objectReferenceValue = tipText.GetComponent<TextMeshProUGUI>();
        SerializedProperty hpArray = soHud.FindProperty("_heartIcons");
        hpArray.arraySize = 3;
        for (int i=0; i<3; i++) hpArray.GetArrayElementAtIndex(i).objectReferenceValue = hearts[i];
        
        // Bind ZoomSlider from Phase 5 if exists
        Transform zoomSlider = canvasHud.transform.Find("ZoomSlider");
        if (zoomSlider != null) soHud.FindProperty("_zoomSlider").objectReferenceValue = zoomSlider.GetComponent<Slider>();
        soHud.ApplyModifiedProperties();

        // Canvas_Overlay
        GameObject overlayObj = new GameObject("Canvas_Overlay");
        Canvas overlay = overlayObj.AddComponent<Canvas>();
        overlay.renderMode = RenderMode.ScreenSpaceOverlay;
        overlay.sortingOrder = 10; // Above HUD
        CanvasScaler scaler = overlayObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        overlayObj.AddComponent<GraphicRaycaster>();
        
        // Panels
        GameObject pausePanel = CreatePanel(overlayObj.transform, "PausePanel");
        PauseMenuUI pauseUI = pausePanel.AddComponent<PauseMenuUI>();
        pausePanel.AddComponent<CanvasGroup>();
        CreateText(pausePanel.transform, "Title", "PAUSED", 100, new Vector2(0, 300));
        GameObject resumeBtn = CreateButton(pausePanel.transform, "ResumeBtn", "RESUME", new Vector2(0, 50));
        GameObject pMenuBtn = CreateButton(pausePanel.transform, "MenuBtn", "MAIN MENU", new Vector2(0, -100));
        SerializedObject soPause = new SerializedObject(pauseUI);
        soPause.FindProperty("_resumeButton").objectReferenceValue = resumeBtn.GetComponent<Button>();
        soPause.FindProperty("_mainMenuButton").objectReferenceValue = pMenuBtn.GetComponent<Button>();
        soPause.ApplyModifiedProperties();
        pausePanel.SetActive(false);

        GameObject winPanel = CreatePanel(overlayObj.transform, "WinPanel");
        LevelCompleteUI winUI = winPanel.AddComponent<LevelCompleteUI>();
        winPanel.AddComponent<CanvasGroup>();
        CreateText(winPanel.transform, "Title", "LEVEL CLEARED!", 100, new Vector2(0, 300));
        GameObject nextBtn = CreateButton(winPanel.transform, "NextBtn", "NEXT LEVEL", new Vector2(0, 50));
        GameObject wMenuBtn = CreateButton(winPanel.transform, "MenuBtn", "MAIN MENU", new Vector2(0, -100));
        SerializedObject soWin = new SerializedObject(winUI);
        soWin.FindProperty("_nextLevelButton").objectReferenceValue = nextBtn.GetComponent<Button>();
        soWin.FindProperty("_mainMenuButton").objectReferenceValue = wMenuBtn.GetComponent<Button>();
        soWin.ApplyModifiedProperties();
        winPanel.SetActive(false);
        
        GameObject losePanel = CreatePanel(overlayObj.transform, "LosePanel");
        GameOverUI loseUI = losePanel.AddComponent<GameOverUI>();
        losePanel.AddComponent<CanvasGroup>();
        CreateText(losePanel.transform, "Title", "GAME OVER", 100, new Vector2(0, 300));
        GameObject restartBtn = CreateButton(losePanel.transform, "RestartBtn", "RETRY", new Vector2(0, 50));
        GameObject lMenuBtn = CreateButton(losePanel.transform, "MenuBtn", "MAIN MENU", new Vector2(0, -100));
        SerializedObject soLose = new SerializedObject(loseUI);
        soLose.FindProperty("_retryButton").objectReferenceValue = restartBtn.GetComponent<Button>();
        soLose.FindProperty("_mainMenuButton").objectReferenceValue = lMenuBtn.GetComponent<Button>();
        soLose.ApplyModifiedProperties();
        losePanel.SetActive(false);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[ArrowSwarm] GameScene UI setup complete!");
    }
    
    // --- Helpers ---
    private static GameObject CreatePanel(Transform parent, string name, Vector2 pos = default, Vector2 size = default)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.8f);
        RectTransform rt = go.GetComponent<RectTransform>();
        if (size == default)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
        }
        else
        {
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }
        return go;
    }

    private static GameObject CreateText(Transform parent, string name, string text, int fontSize, Vector2 pos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(800, fontSize + 20);
        return go;
    }

    private static GameObject CreateButton(Transform parent, string name, string text, Vector2 pos, Vector2 size = default)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.5f, 0.8f);
        Button btn = go.AddComponent<Button>();
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size == default ? new Vector2(400, 100) : size;
        
        GameObject txtGo = CreateText(go.transform, "Text", text, 50, Vector2.zero);
        return go;
    }
}
