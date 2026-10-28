namespace ArrowSwarm.UI.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using K = UiRollout01Kit;

    /// <summary>Refreshes only Boot's atmosphere, preserving the reviewed logo and loading hierarchy.</summary>
    internal static partial class UiRollout01BootAuthoring
    {
        private static void RefreshBackground()
        {
            if (Application.isPlaying || SceneManager.GetActiveScene().path != "Assets/_Project/Scenes/BootScene.unity")
                throw new InvalidOperationException("Open BootScene in Edit Mode before refreshing the backdrop.");
            var canvas = GameObject.Find("Canvas_Boot");
            var old = canvas.transform.Find("SharedBackdrop");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            K.BootBackdrop(canvas.transform);
            var background = canvas.transform.Find("SharedBackdrop").GetComponent<Image>();
            Bind(canvas.GetComponent<BootLoadingUI>(), "_backgroundImage", background);
            var tip = canvas.transform.Find("BootDesign/TipCard").GetComponent<Image>();
            K.Theme(tip, Color.white, Color.white, K.Material("BootTipDark"));
            EditorUtility.SetDirty(canvas.GetComponent<BootLoadingUI>());
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}
