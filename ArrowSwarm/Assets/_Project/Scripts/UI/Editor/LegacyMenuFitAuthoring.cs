namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections.Generic;
    using TMPro;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;

    /// <summary>Authors safe-area wrappers around legacy dialogs while preserving their serialized references, styling and animation targets.</summary>
    public static class LegacyMenuFitAuthoring
    {
        /// <summary>Fits the three tall legacy menu dialogs and prevents localized page labels from wrapping outside their badge.</summary>
        public static string Apply()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Author in Edit Mode.");
            var setup=EditorSceneManager.GetSceneManagerSetup();var report=new List<string>();
            try
            {
                var scene=EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainMenuScene.unity");
                Fit(UnityEngine.Object.FindFirstObjectByType<LevelSelectUI>(FindObjectsInactive.Include),"_dialogBox",report);
                Fit(UnityEngine.Object.FindFirstObjectByType<LeaderboardUI>(FindObjectsInactive.Include),"_dialogBox",report);
                Fit(UnityEngine.Object.FindFirstObjectByType<SettingsUI>(FindObjectsInactive.Include),"_dialogBox",report);
                var levels=new SerializedObject(UnityEngine.Object.FindFirstObjectByType<LevelSelectUI>(FindObjectsInactive.Include));
                var label=(TMP_Text)levels.FindProperty("_pageText").objectReferenceValue;
                label.fontSizeMax=label.fontSize;label.enableAutoSizing=true;label.textWrappingMode=TextWrappingModes.NoWrap;
                EditorSceneManager.SaveScene(scene);return string.Join("\n",report);
            }
            finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
        }
        private static void Fit(MonoBehaviour owner,string field,List<string> report)
        {
            var content=(RectTransform)new SerializedObject(owner).FindProperty(field).objectReferenceValue;
            if(content==null)throw new InvalidOperationException("Missing dialog: "+owner.name);
            RectTransform wrapper;
            if(content.parent.GetComponent<SafeAreaPanelFit>()!=null)wrapper=(RectTransform)content.parent;
            else
            {
                wrapper=new GameObject("SafeAreaFit",typeof(RectTransform),typeof(SafeAreaPanelFit)).GetComponent<RectTransform>();
                wrapper.SetParent(content.parent,false);wrapper.SetSiblingIndex(content.GetSiblingIndex());
                wrapper.anchorMin=Vector2.zero;wrapper.anchorMax=Vector2.one;wrapper.pivot=Vector2.one*.5f;
                wrapper.sizeDelta=Vector2.zero;wrapper.anchoredPosition=Vector2.zero;content.SetParent(wrapper,false);
            }
            wrapper.localScale=Vector3.one;wrapper.anchoredPosition=Vector2.zero;Canvas.ForceUpdateCanvases();
            var corners=new Vector3[4];Vector2 extent=Vector2.zero;
            foreach(var rect in content.GetComponentsInChildren<RectTransform>(true))
            {
                var image=rect.GetComponent<UnityEngine.UI.Image>();
                if(rect!=content&&(image==null||image.sprite==null||image.color.a<=.01f||!AuthoredActive(rect,content)))continue;
                rect.GetWorldCorners(corners);
                foreach(var corner in corners)
                {
                    Vector3 local=wrapper.InverseTransformPoint(corner);
                    extent=Vector2.Max(extent,new Vector2(Mathf.Abs(local.x),Mathf.Abs(local.y))*2);
                }
            }
            var data=new SerializedObject(wrapper.GetComponent<SafeAreaPanelFit>());
            data.FindProperty("_referenceExtent").vector2Value=extent;data.ApplyModifiedPropertiesWithoutUndo();
            report.Add(owner.name+" extent="+extent+"; preserved animation target "+content.name);
        }
        private static bool AuthoredActive(Transform child,Transform root)
        {
            for(var current=child;current!=root;current=current.parent)
                if(current==null||!current.gameObject.activeSelf)return false;
            return true;
        }
    }
}
