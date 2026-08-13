using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using ArrowSwarm.UI;
using UnityEditor.SceneManagement;

public static class FixAllUIScenes
{
    [MenuItem("ArrowSwarm/UI/Fix All UI Scenes (MainMenu + GameScene)")]
    public static void FixAllUI()
    {
        FixMainMenuScene();
        FixGameSceneUI();
        Debug.Log("<color=green>[ArrowSwarm] ALL UI Scenes (MainMenu + GameScene) fixed successfully!</color>");
    }

    [MenuItem("ArrowSwarm/UI/Fix MainMenu Scene UI")]
    public static void FixMainMenuScene()
    {
        string scenePath = "Assets/_Project/Scenes/MainMenuScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);

        // 1. Ensure InputSystem EventSystem
        var es = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (es == null)
        {
            GameObject esGo = new GameObject("EventSystem");
            es = esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
        else
        {
            var oldModule = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (oldModule != null) Object.DestroyImmediate(oldModule);

            var inputModule = es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            if (inputModule == null) es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // 2. Find or Create Main Canvas
        GameObject canvasGo = GameObject.Find("Canvas_MainMenu");
        if (canvasGo == null) canvasGo = GameObject.Find("Canvas");
        if (canvasGo == null)
        {
            canvasGo = new GameObject("Canvas_MainMenu");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            canvasGo.AddComponent<GraphicRaycaster>();
        }
        else
        {
            if (canvasGo.GetComponent<GraphicRaycaster>() == null)
                canvasGo.AddComponent<GraphicRaycaster>();
        }

        // 3. Ensure MainMenuUI component
        MainMenuUI menuUI = canvasGo.GetComponent<MainMenuUI>();
        if (menuUI == null) menuUI = canvasGo.AddComponent<MainMenuUI>();

        CanvasGroup cg = canvasGo.GetComponent<CanvasGroup>();
        if (cg == null) cg = canvasGo.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        // Clean up messy old layout container if present
        Transform oldBtnContainer = canvasGo.transform.Find("GameObject");
        if (oldBtnContainer != null)
        {
            // Move buttons up to main canvas or recreate nicely
            Object.DestroyImmediate(oldBtnContainer.gameObject);
        }

        // 4. Create / Fix Main Menu UI Elements
        // Title Text
        Transform titleTrans = canvasGo.transform.Find("TitleText");
        GameObject titleObj = titleTrans != null ? titleTrans.gameObject : CreateText(canvasGo.transform, "TitleText", "ARROW SWARM", 90, new Vector2(0, 500));
        var titleTmp = titleObj.GetComponent<TextMeshProUGUI>();
        titleTmp.raycastTarget = false;

        // Level Text
        Transform levelTrans = canvasGo.transform.Find("LevelText");
        GameObject levelObj = levelTrans != null ? levelTrans.gameObject : CreateText(canvasGo.transform, "LevelText", "Level: 1", 50, new Vector2(0, 350));
        var levelTmp = levelObj.GetComponent<TextMeshProUGUI>();
        levelTmp.raycastTarget = false;

        // Play Button
        Transform playTrans = canvasGo.transform.Find("PlayButton");
        if (playTrans != null) Object.DestroyImmediate(playTrans.gameObject);
        GameObject playBtnObj = CreateButton(canvasGo.transform, "PlayButton", "PLAY", new Vector2(0, 100), new Vector2(500, 140), new Color(0.2f, 0.6f, 0.9f));

        // Levels Button
        Transform levelsTrans = canvasGo.transform.Find("LevelsButton");
        if (levelsTrans != null) Object.DestroyImmediate(levelsTrans.gameObject);
        GameObject levelsBtnObj = CreateButton(canvasGo.transform, "LevelsButton", "LEVELS", new Vector2(0, -80), new Vector2(500, 120), new Color(0.3f, 0.7f, 0.4f));

        // Leaderboard Button
        Transform leaderTrans = canvasGo.transform.Find("LeaderboardButton");
        if (leaderTrans != null) Object.DestroyImmediate(leaderTrans.gameObject);
        GameObject leaderBtnObj = CreateButton(canvasGo.transform, "LeaderboardButton", "LEADERBOARD", new Vector2(0, -240), new Vector2(500, 120), new Color(0.8f, 0.6f, 0.2f));

        // Settings Button
        Transform settingsTrans = canvasGo.transform.Find("SettingsButton");
        if (settingsTrans != null) Object.DestroyImmediate(settingsTrans.gameObject);
        GameObject settingsBtnObj = CreateButton(canvasGo.transform, "SettingsButton", "SETTINGS", new Vector2(0, -400), new Vector2(500, 120), new Color(0.6f, 0.4f, 0.8f));

        // 5. Wire References to MainMenuUI
        SerializedObject soMenu = new SerializedObject(menuUI);
        soMenu.FindProperty("_playButton").objectReferenceValue = playBtnObj.GetComponent<Button>();
        soMenu.FindProperty("_levelsButton").objectReferenceValue = levelsBtnObj.GetComponent<Button>();
        soMenu.FindProperty("_leaderboardButton").objectReferenceValue = leaderBtnObj.GetComponent<Button>();
        soMenu.FindProperty("_settingsButton").objectReferenceValue = settingsBtnObj.GetComponent<Button>();
        soMenu.FindProperty("_levelText").objectReferenceValue = levelTmp;
        soMenu.FindProperty("_titleText").objectReferenceValue = titleTmp;
        soMenu.FindProperty("_canvasGroup").objectReferenceValue = cg;
        soMenu.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[ArrowSwarm] MainMenuScene UI rebuilt clean and saved!");
    }

    [MenuItem("ArrowSwarm/UI/Fix GameScene UI")]
    public static void FixGameSceneUI()
    {
        string scenePath = "Assets/_Project/Scenes/GameScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);

        // 1. Delete stray Mob if any
        GameObject strayMob = GameObject.Find("Mob");
        if (strayMob != null) Object.DestroyImmediate(strayMob);

        // 2. Fix EventSystem
        var es = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (es == null)
        {
            GameObject esGo = new GameObject("EventSystem");
            es = esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
        else
        {
            var oldModule = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (oldModule != null) Object.DestroyImmediate(oldModule);

            var inputModule = es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            if (inputModule == null) es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // 3. Find Canvas_HUD and Setup
        GameObject canvasHudObj = GameObject.Find("Canvas_HUD");
        if (canvasHudObj == null)
        {
            canvasHudObj = new GameObject("Canvas_HUD");
            var canvas = canvasHudObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1;
            var scaler = canvasHudObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            canvasHudObj.AddComponent<GraphicRaycaster>();
        }
        else
        {
            if (canvasHudObj.GetComponent<GraphicRaycaster>() == null)
                canvasHudObj.AddComponent<GraphicRaycaster>();
        }

        GameHUD hud = canvasHudObj.GetComponent<GameHUD>();
        if (hud == null) hud = canvasHudObj.AddComponent<GameHUD>();

        // Recreate TopBar and BottomBar
        Transform oldTop = canvasHudObj.transform.Find("TopBar");
        if (oldTop != null) Object.DestroyImmediate(oldTop.gameObject);
        Transform oldBot = canvasHudObj.transform.Find("BottomBar");
        if (oldBot != null) Object.DestroyImmediate(oldBot.gameObject);

        GameObject topBar = CreatePanel(canvasHudObj.transform, "TopBar", new Vector2(0, 800), new Vector2(1080, 200));
        topBar.GetComponent<Image>().color = new Color(0, 0, 0, 0.4f);
        topBar.GetComponent<Image>().raycastTarget = false;

        GameObject levelText = CreateText(topBar.transform, "LevelText", "Lv.1", 60, new Vector2(-400, 0));
        GameObject pauseBtn = CreateButton(topBar.transform, "PauseButton", "||", new Vector2(400, 0), new Vector2(120, 120), new Color(0.8f, 0.3f, 0.3f));
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
            hearts[i].raycastTarget = false;
            hearts[i].rectTransform.anchoredPosition = new Vector2(i * 60, 0);
            hearts[i].rectTransform.sizeDelta = new Vector2(50, 50);
        }

        GameObject botBar = CreatePanel(canvasHudObj.transform, "BottomBar", new Vector2(0, -800), new Vector2(1080, 200));
        botBar.GetComponent<Image>().color = new Color(0, 0, 0, 0.4f);
        botBar.GetComponent<Image>().raycastTarget = false;
        GameObject arrowText = CreateText(botBar.transform, "ArrowText", "Arrows: 0/16", 60, new Vector2(0, 0));

        SerializedObject soHud = new SerializedObject(hud);
        soHud.FindProperty("_levelText").objectReferenceValue = levelText.GetComponent<TextMeshProUGUI>();
        soHud.FindProperty("_pauseButton").objectReferenceValue = pauseBtn.GetComponent<Button>();
        soHud.FindProperty("_arrowCountText").objectReferenceValue = arrowText.GetComponent<TextMeshProUGUI>();
        soHud.FindProperty("_tipCountText").objectReferenceValue = tipText.GetComponent<TextMeshProUGUI>();
        SerializedProperty hpArray = soHud.FindProperty("_heartIcons");
        hpArray.arraySize = 3;
        for (int i = 0; i < 3; i++) hpArray.GetArrayElementAtIndex(i).objectReferenceValue = hearts[i];

        Transform zoomSlider = canvasHudObj.transform.Find("ZoomSlider");
        if (zoomSlider != null) soHud.FindProperty("_zoomSlider").objectReferenceValue = zoomSlider.GetComponent<Slider>();
        soHud.ApplyModifiedProperties();

        // 4. Setup Canvas_Overlay
        GameObject overlayObj = GameObject.Find("Canvas_Overlay");
        if (overlayObj != null) Object.DestroyImmediate(overlayObj);

        overlayObj = new GameObject("Canvas_Overlay");
        Canvas overlay = overlayObj.AddComponent<Canvas>();
        overlay.renderMode = RenderMode.ScreenSpaceOverlay;
        overlay.sortingOrder = 10;
        CanvasScaler scalerOverlay = overlayObj.AddComponent<CanvasScaler>();
        scalerOverlay.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scalerOverlay.referenceResolution = new Vector2(1080, 1920);
        overlayObj.AddComponent<GraphicRaycaster>();

        // Pause Panel
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

        // Win Panel
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

        // Lose Panel
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

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[ArrowSwarm] GameScene UI setup complete & saved!");
    }

    // --- Helpers ---
    private static GameObject CreatePanel(Transform parent, string name, Vector2 pos = default, Vector2 size = default)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.8f);
        img.raycastTarget = true;
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
        tmp.raycastTarget = false; // Never block raycasts with text
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(800, fontSize + 20);
        return go;
    }

    private static GameObject CreateButton(Transform parent, string name, string text, Vector2 pos, Vector2 size = default, Color color = default)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        Image img = go.AddComponent<Image>();
        img.color = color == default ? new Color(0.2f, 0.5f, 0.8f) : color;
        img.raycastTarget = true; // Button image MUST receive raycast

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.interactable = true;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size == default ? new Vector2(400, 100) : size;

        CreateText(go.transform, "Text", text, 50, Vector2.zero);
        return go;
    }
}
