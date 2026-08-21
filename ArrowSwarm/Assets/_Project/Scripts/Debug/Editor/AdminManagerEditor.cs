#if UNITY_EDITOR
namespace ArrowSwarm.Debug.Editor
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Custom Inspector for AdminManager providing one-click buttons
    /// to modify player progress, levels, stars, tips, and save data.
    /// </summary>
    [CustomEditor(typeof(AdminManager))]
    public class AdminManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var admin = (AdminManager)target;

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                "Arrow Swarm Admin Panel\nEdit target values and click buttons to apply live changes to PlayerData.",
                MessageType.Info);

            EditorGUILayout.Space(6);

            if (GUILayout.Button("📥 Pull Current Player Data", GUILayout.Height(28)))
            {
                admin.PullFromPlayerData();
                EditorUtility.SetDirty(admin);
            }

            EditorGUILayout.Space(8);

            // --- Level Section ---
            EditorGUILayout.LabelField("Level Controls", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetLevel"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetHighestLevel"));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply Current Level"))
            {
                admin.ApplyLevel();
            }
            if (GUILayout.Button("Apply Highest Level"))
            {
                admin.ApplyHighestLevel();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);

            // --- Stars Section ---
            EditorGUILayout.LabelField("Stars Controls", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetTotalStars"));
            if (GUILayout.Button("Apply Total Stars Direct"))
            {
                admin.ApplyTotalStars();
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_specificLevel"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_starsForSpecificLevel"));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Set Specific Level Stars"))
            {
                admin.ApplyStarsForSpecificLevel();
            }
            if (GUILayout.Button("🌟 Unlock Up To Target (3★)"))
            {
                admin.UnlockAllWith3Stars();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);

            // --- Resources Section ---
            EditorGUILayout.LabelField("Tips / Hints Controls", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetTips"));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply Tips Count"))
            {
                admin.ApplyTips();
            }
            if (GUILayout.Button("💡 +10 Tips"))
            {
                admin.Add10Tips();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // --- Bulk Apply / Reset ---
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f);
            if (GUILayout.Button("💾 Apply All Edits", GUILayout.Height(30)))
            {
                admin.ApplyAllEdits();
            }

            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("🔄 Reset All Data", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Reset Player Data", "Are you sure you want to reset all player data to default?", "Yes, Reset", "Cancel"))
                {
                    admin.ResetAllData();
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(12);

            // --- Live Monitor ---
            EditorGUILayout.LabelField("Live Monitor (Read Only)", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_liveCurrentLevel"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_liveHighestLevel"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_liveTotalStars"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_liveTipCount"));
            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
