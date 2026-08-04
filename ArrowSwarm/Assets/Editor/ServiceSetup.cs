using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using ArrowSwarm.Data;
using ArrowSwarm.Tips;
using ArrowSwarm.Ads;
using ArrowSwarm.UI;
using UnityEditor.SceneManagement;

public static class ServiceSetup
{
    [MenuItem("ArrowSwarm/Setup Services/1. Setup MainMenu Services")]
    public static void SetupMainMenuServices()
    {
        GameObject cloudObj = GameObject.Find("MockCloudService");
        if (cloudObj == null)
        {
            cloudObj = new GameObject("MockCloudService");
            cloudObj.AddComponent<MockCloudService>();
        }
        
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[ArrowSwarm] MainMenu Services Setup Complete!");
    }

    [MenuItem("ArrowSwarm/Setup Services/2. Setup GameScene Services")]
    public static void SetupGameSceneServices()
    {
        // 1. TipManager & Highlighter & MockAdService
        GameObject servicesObj = GameObject.Find("Services");
        if (servicesObj == null)
        {
            servicesObj = new GameObject("Services");
            servicesObj.transform.SetParent(GameObject.Find("Managers")?.transform);
        }
        
        var tipMgr = servicesObj.GetComponent<TipManager>();
        if (tipMgr == null) tipMgr = servicesObj.AddComponent<TipManager>();
        
        var highlighter = servicesObj.GetComponent<TipHighlighter>();
        if (highlighter == null) highlighter = servicesObj.AddComponent<TipHighlighter>();
        
        var adService = servicesObj.GetComponent<MockAdService>();
        if (adService == null) adService = servicesObj.AddComponent<MockAdService>();
        
        SerializedObject soTip = new SerializedObject(tipMgr);
        soTip.FindProperty("_highlighter").objectReferenceValue = highlighter;
        soTip.ApplyModifiedProperties();

        // 2. TipPopupUI
        GameObject overlay = GameObject.Find("Canvas_Overlay");
        if (overlay != null)
        {
            Transform existingTip = overlay.transform.Find("TipPopupPanel");
            if (existingTip != null) Object.DestroyImmediate(existingTip.gameObject);

            GameObject tipPanel = new GameObject("TipPopupPanel");
            tipPanel.transform.SetParent(overlay.transform, false);
            tipPanel.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
            tipPanel.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            tipPanel.GetComponent<RectTransform>().anchorMax = Vector2.one;
            
            Image img = tipPanel.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.9f);
            
            TipPopupUI tipUI = tipPanel.AddComponent<TipPopupUI>();
            tipPanel.AddComponent<CanvasGroup>();
            
            GameObject msgTxt = CreateText(tipPanel.transform, "MessageText", "No tips left!\nWatch an ad to get +1 tip?", 60, new Vector2(0, 200));
            GameObject watchBtn = CreateButton(tipPanel.transform, "WatchAdBtn", "WATCH AD", new Vector2(0, -100));
            GameObject closeBtn = CreateButton(tipPanel.transform, "CloseBtn", "CLOSE", new Vector2(0, -300));
            
            SerializedObject soTipUI = new SerializedObject(tipUI);
            soTipUI.FindProperty("_messageText").objectReferenceValue = msgTxt.GetComponent<TextMeshProUGUI>();
            soTipUI.FindProperty("_watchAdButton").objectReferenceValue = watchBtn.GetComponent<Button>();
            soTipUI.FindProperty("_closeButton").objectReferenceValue = closeBtn.GetComponent<Button>();
            soTipUI.ApplyModifiedProperties();
            
            tipPanel.SetActive(true); // Should be true, TipPopupUI.Start calls Hide()
        }
        else
        {
            Debug.LogError("Canvas_Overlay not found! Open GameScene first.");
            return;
        }
        
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[ArrowSwarm] GameScene Services Setup Complete!");
    }
    
    // Helpers
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
        rt.sizeDelta = new Vector2(800, 300);
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
