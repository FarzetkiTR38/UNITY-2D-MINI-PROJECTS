using UnityEngine;
using UnityEditor;
using ArrowSwarm.UI;
using UnityEditor.SceneManagement;

public static class TipPopupFix
{
    [MenuItem("ArrowSwarm/Setup Services/3. Fix TipPopupUI")]
    public static void FixTipPopup()
    {
        GameObject tipPanel = GameObject.Find("TipPopupPanel");
        if (tipPanel != null)
        {
            TipPopupUI tipUI = tipPanel.GetComponent<TipPopupUI>();
            CanvasGroup group = tipPanel.GetComponent<CanvasGroup>();
            
            SerializedObject soTipUI = new SerializedObject(tipUI);
            soTipUI.FindProperty("_canvasGroup").objectReferenceValue = group;
            soTipUI.ApplyModifiedProperties();
            
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[ArrowSwarm] TipPopupUI CanvasGroup reference fixed!");
        }
    }
}
