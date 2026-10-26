namespace ArrowSwarm.Modes.Editor
{
    using System;
    using ArrowSwarm.UI;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Enables optional width fitting only on the active gameplay scene's authored pause title.</summary>
    public static class Quality4PauseTitleAuthoring
    {
        /// <summary>Sets the pause title's fit policy without changing any font, color, layer or other title, and does not save the scene.</summary>
        public static string EnableOpenScenePauseTitleFit()
        {
            var scene = SceneManager.GetActiveScene();
            if (Application.isPlaying || (scene.name != "GameScene" && scene.name != "MapTestScene" &&
                scene.name != "ShapesScene" && scene.name != "ColorShiftScene"))
                throw new InvalidOperationException("Open one gameplay scene in Edit Mode before enabling pause-title fitting.");
            Casual3DTitle pauseTitle = null;
            foreach (var root in scene.GetRootGameObjects())
                foreach (var title in root.GetComponentsInChildren<Casual3DTitle>(true))
                    if (title.LocalizationKey == "pause_title")
                    {
                        if (pauseTitle != null) throw new InvalidOperationException("Expected one authored pause title per scene.");
                        pauseTitle = title;
                    }
            if (pauseTitle == null) throw new InvalidOperationException("The authored pause_title Casual3DTitle was not found.");
            var serialized = new SerializedObject(pauseTitle);
            serialized.FindProperty("_fitToRect").boolValue = true;
            serialized.FindProperty("_horizontalFitPadding").floatValue = 12f;
            serialized.ApplyModifiedProperties();
            PrefabUtility.RecordPrefabInstancePropertyModifications(pauseTitle);
            EditorUtility.SetDirty(pauseTitle); EditorSceneManager.MarkSceneDirty(scene); pauseTitle.MarkDirty();
            return "Prepared optional pause-title fit (not saved): " + scene.name + "; rect width=" +
                ((RectTransform)pauseTitle.transform).rect.width + "; per-side inset=12; font, colors, arch and layer offsets preserved";
        }
    }
}
