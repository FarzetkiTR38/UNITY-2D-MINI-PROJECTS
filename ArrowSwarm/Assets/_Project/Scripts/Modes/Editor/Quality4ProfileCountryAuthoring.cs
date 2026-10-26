namespace ArrowSwarm.Modes.Editor
{
    using System;
    using ArrowSwarm.UI;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>Repairs only the two authored profile country hit targets while preserving their existing artwork and hierarchy.</summary>
    public static class Quality4ProfileCountryAuthoring
    {
        /// <summary>Adds transparent child hit graphics, routes tint and feedback to existing art, and leaves scene saving to the caller.</summary>
        public static string RepairOpenMenuScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (Application.isPlaying || scene.name != "MainMenuScene")
                throw new InvalidOperationException("Open MainMenuScene in Edit Mode before repairing authored country controls.");
            var profile = UnityEngine.Object.FindFirstObjectByType<ProfileSetupModalUI>(FindObjectsInactive.Include);
            if (profile == null || profile.gameObject.scene != scene)
                throw new InvalidOperationException("The active menu's authored profile modal is required.");
            var profileData = new SerializedObject(profile);
            string left = Repair(profileData.FindProperty("_prevCountryButton").objectReferenceValue as Button);
            string right = Repair(profileData.FindProperty("_nextCountryButton").objectReferenceValue as Button);
            EditorSceneManager.MarkSceneDirty(scene);
            return "Prepared authored country pointer repair (not saved): " + left + "; " + right;
        }

        private static string Repair(Button button)
        {
            if (button == null || button.transform.parent == null)
                throw new InvalidOperationException("Existing country arrow Button and parent artwork are required.");
            var artwork = button.transform.parent.GetComponent<Image>();
            if (artwork == null) throw new InvalidOperationException(button.name + " has no parent Image artwork.");
            var hit = button.GetComponent<Image>();
            if (hit != null && (hit.sprite != null || hit.color.a > .001f))
                throw new InvalidOperationException("Refusing to replace an existing visible country Button graphic.");
            if (hit == null) hit = Undo.AddComponent<Image>(button.gameObject);
            Undo.RecordObject(hit, "Repair profile country hit target");
            hit.sprite = null; hit.color = new Color(1f, 1f, 1f, 0f); hit.raycastTarget = true;
            Undo.RecordObject(artwork, "Route country arrow artwork raycasts"); artwork.raycastTarget = false;
            Undo.RecordObject(button, "Bind country arrow tint artwork"); button.targetGraphic = artwork;
            var feedback = button.GetComponent<UIButtonFeedback>();
            if (feedback == null) feedback = Undo.AddComponent<UIButtonFeedback>(button.gameObject);
            var serializedFeedback = new SerializedObject(feedback);
            serializedFeedback.FindProperty("_targetTransform").objectReferenceValue = artwork.transform;
            serializedFeedback.ApplyModifiedProperties();
            EditorUtility.SetDirty(hit); EditorUtility.SetDirty(artwork); EditorUtility.SetDirty(button); EditorUtility.SetDirty(feedback);
            PrefabUtility.RecordPrefabInstancePropertyModifications(hit);
            PrefabUtility.RecordPrefabInstancePropertyModifications(artwork);
            PrefabUtility.RecordPrefabInstancePropertyModifications(button);
            PrefabUtility.RecordPrefabInstancePropertyModifications(feedback);
            return button.name + " owns transparent hit, with tint and squeeze on " + artwork.name;
        }
    }
}
