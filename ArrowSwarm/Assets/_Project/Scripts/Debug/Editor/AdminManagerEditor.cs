#if UNITY_EDITOR
namespace ArrowSwarm.Debug.Editor
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Custom Inspector for AdminManager providing easy-to-use visual controls
    /// to view, modify, test, and launch gameplay with any player statistics.
    /// </summary>
    [CustomEditor(typeof(AdminManager))]
    public class AdminManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var admin = (AdminManager)target;

            EditorGUILayout.Space(6);
            DrawHeaderBanner();

            EditorGUILayout.Space(6);

            // Pull Button
            if (GUILayout.Button("📥 Pull Current Saved Data from Storage", GUILayout.Height(26)))
            {
                admin.PullFromPlayerData();
                EditorUtility.SetDirty(admin);
            }

            EditorGUILayout.Space(8);

            // 1. Profile Section
            DrawSectionHeader("👤 Player Profile & Identity", new Color(0.25f, 0.55f, 0.9f));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetPlayerName"), new GUIContent("Player Nickname"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetPlayerCountry"), new GUIContent("Country Code (e.g. TR, US, DE)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetProfileCompleted"), new GUIContent("Profile Setup Done"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetTutorialCompleted"), new GUIContent("Tutorial Completed (Skip Lv 0)"));

            EditorGUILayout.Space(8);

            // 2. Progress & Stars Section
            DrawSectionHeader("🏆 Progress & Stars", new Color(1f, 0.75f, 0.2f));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetLevel"), new GUIContent("Current Level (To Play)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetHighestLevel"), new GUIContent("Highest Unlocked Level"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetTotalStars"), new GUIContent("Total Stars"));

            EditorGUILayout.Space(8);

            // 3. Resources & Skills Section
            DrawSectionHeader("⚡ Resources & Abilities", new Color(0.2f, 0.8f, 0.6f));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetTips"), new GUIContent("Tip Tokens Count"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetFreeze"), new GUIContent("Freeze Charges Count"));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("💡 +10 Tips"))
            {
                var p = serializedObject.FindProperty("_targetTips");
                p.intValue += 10;
            }
            if (GUILayout.Button("❄️ +5 Freeze"))
            {
                var p = serializedObject.FindProperty("_targetFreeze");
                p.intValue += 5;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);

            // 4. Settings Section
            DrawSectionHeader("⚙️ Settings & Audio", new Color(0.6f, 0.5f, 0.8f));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetLanguage"), new GUIContent("Language (TURKISH, ENGLISH...)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetTheme"), new GUIContent("Theme Mode"));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetSfx"), new GUIContent("SFX"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetVfx"), new GUIContent("VFX"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetVibration"), new GUIContent("Vibe"));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(12);

            // 5. Main Action Buttons
            DrawSectionHeader("🎮 Actions & Launchers", new Color(0.4f, 0.8f, 0.4f));

            GUI.backgroundColor = new Color(0.2f, 0.85f, 0.35f);
            if (GUILayout.Button("💾 APPLY & SAVE ALL STATS", GUILayout.Height(32)))
            {
                serializedObject.ApplyModifiedProperties();
                admin.ApplyAllEdits();
            }

            GUI.backgroundColor = new Color(0.2f, 0.65f, 1f);
            int curLvl = serializedObject.FindProperty("_targetLevel").intValue;
            if (GUILayout.Button($"🚀 SAVE & PLAY LEVEL {curLvl} (GAME SCENE)", GUILayout.Height(34)))
            {
                serializedObject.ApplyModifiedProperties();
                admin.SaveAndStartGame();
            }

            GUI.backgroundColor = new Color(0.8f, 0.45f, 0.9f);
            if (GUILayout.Button("🏠 SAVE & LAUNCH MAIN MENU", GUILayout.Height(28)))
            {
                serializedObject.ApplyModifiedProperties();
                admin.SaveAndStartMainMenu();
            }

            GUI.backgroundColor = new Color(1f, 0.35f, 0.35f);
            if (GUILayout.Button("🗑️ RESET ALL DATA TO DEFAULT", GUILayout.Height(26)))
            {
                if (EditorUtility.DisplayDialog("Reset Player Data", "Are you sure you want to reset all player data to default?", "Yes, Reset", "Cancel"))
                {
                    admin.ResetAllData();
                }
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(12);

            // 6. Live Monitor Section
            DrawSectionHeader("📊 Live Data Monitor (Read-Only)", Color.gray);
            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_liveProfile"), new GUIContent("Profile"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_liveProgress"), new GUIContent("Progress"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_liveResources"), new GUIContent("Resources"));
            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeaderBanner()
        {
            var rect = EditorGUILayout.GetControlRect(false, 38);
            EditorGUI.DrawRect(rect, new Color(0.12f, 0.16f, 0.24f));

            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
                normal = { textColor = new Color(0.9f, 0.95f, 1f) }
            };
            EditorGUI.LabelField(rect, "ARROW SWARM — BOOT STATS & ADMIN CONTROLLER", style);
        }

        private void DrawSectionHeader(string title, Color barColor)
        {
            var rect = EditorGUILayout.GetControlRect(false, 22);
            EditorGUI.DrawRect(rect, new Color(barColor.r * 0.2f, barColor.g * 0.2f, barColor.b * 0.2f, 0.85f));

            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 11,
                normal = { textColor = barColor }
            };
            rect.x += 6;
            EditorGUI.LabelField(rect, title, style);
        }
    }
}
#endif
