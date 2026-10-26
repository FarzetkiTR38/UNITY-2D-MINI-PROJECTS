namespace ArrowSwarm.Modes.Editor
{
    using System;
    using ArrowSwarm.UI;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>Authors standard, cancel-safe feedback on the existing profile Dice button without runtime styling.</summary>
    public static class Quality4ProfileFeedbackAuthoring
    {
        /// <summary>Preserves all dice art and tint bindings, adds authored squeeze feedback, and leaves saving to the caller.</summary>
        public static string RepairOpenMenuDiceFeedback()
        {
            var scene = SceneManager.GetActiveScene();
            if (Application.isPlaying || scene.name != "MainMenuScene")
                throw new InvalidOperationException("Open MainMenuScene in Edit Mode before authoring Dice feedback.");
            var profile = UnityEngine.Object.FindFirstObjectByType<ProfileSetupModalUI>(FindObjectsInactive.Include);
            if (profile == null || profile.gameObject.scene != scene) throw new InvalidOperationException("The actual profile modal is required.");
            var serializedProfile = new SerializedObject(profile);
            var dice = serializedProfile.FindProperty("_diceButton").objectReferenceValue as Button;
            if (dice == null) throw new InvalidOperationException("The authored Dice Button is required.");
            var feedback = dice.GetComponent<UIButtonFeedback>();
            if (feedback == null) feedback = Undo.AddComponent<UIButtonFeedback>(dice.gameObject);
            var serialized = new SerializedObject(feedback);
            serialized.FindProperty("_targetTransform").objectReferenceValue = dice.transform;
            serialized.FindProperty("_enableIdleBreath").boolValue = false;
            serialized.ApplyModifiedProperties(); EditorUtility.SetDirty(feedback);
            PrefabUtility.RecordPrefabInstancePropertyModifications(feedback); EditorSceneManager.MarkSceneDirty(scene);
            return "Prepared standard Dice feedback (not saved); target=" + dice.name + "; scale=" + dice.transform.localScale;
        }
    }
}
