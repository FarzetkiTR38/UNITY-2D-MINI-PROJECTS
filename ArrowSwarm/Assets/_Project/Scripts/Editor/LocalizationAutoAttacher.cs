namespace ArrowSwarm.Editor
{
    using System;
    using System.Collections.Generic;
    using ArrowSwarm.Localization;
    using TMPro;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Editor utility to scan all UI TextMeshProUGUI elements in prefabs and scenes,
    /// attach LocalizedText components, and set the appropriate localization keys.
    /// </summary>
    [InitializeOnLoad]
    public static class LocalizationAutoAttacher
    {
        private const string PrefsKey = "ArrowSwarm_LocalizationAttached_v2";

        // Mapping rules: path pattern / name / text -> localization key
        private static readonly (string matchKey, string locKey)[] KeyMappings = new[]
        {
            // Main Menu
            ("PlayButton/Text", "menu_play"),
            ("LeaderboardButton/Text", "menu_leaderboard"),
            ("LevelsButton/Text", "menu_levels"),
            ("SettingsButton/Text", "menu_settings"),
            ("SettingsPanel/BoardFrame/Header/TitleText", "settings_title"),
            ("SettingRow_SFX/LabelText", "settings_sfx"),
            ("SettingRow_VFX/LabelText", "settings_vfx"),
            ("SettingRow_Vibration/LabelText", "settings_vibration"),
            ("SettingRow_Language/LabelText", "settings_language"),
            ("SettingRow_Theme/LabelText", "settings_theme"),
            ("LeaderboardPanel/BoardFrame/Header/TitleText", "leaderboard_title"),
            ("ProfileSetupModal/Card/PlayerName/NameLabel", "profile_name_label"),
            ("ProfileSetupModal/Card/Country/CountryLabel", "settings_country"),
            ("ProfileSetupModal/Card/Header/SubtitleText", "profile_desc"),
            ("ProfileSetupModal/Card/SaveButton/Text", "profile_save_play"),
            ("ProfileSetupModal/Card/SkipButton/Text", "profile_skip"),

            // Overlays (Pause, Win, Lose, Tips)
            ("PausePanel/DialogBox/HeaderTitle/TitleText", "pause_title"),
            ("PausePanel/DialogBox/ContentContainer/ContinueBtn/Text", "pause_resume"),
            ("PausePanel/DialogBox/ContentContainer/RetryBtn/Text", "pause_retry"),
            ("PausePanel/DialogBox/ContentContainer/LevelsBtn/Text", "pause_levels"),
            ("PausePanel/DialogBox/ContentContainer/MainMenuBtn/Text", "pause_main_menu"),
            ("SettingRow_Sound/LabelText", "settings_sfx"),
            ("WinPanel/DialogBox/HeaderTitle/TitleText", "win_title"),
            ("WinPanel/DialogBox/ButtonsContainer/NextLevelBtn/Text", "win_next_level"),
            ("WinPanel/DialogBox/ButtonsContainer/LevelsBtn/Text", "win_levels"),
            ("WinPanel/DialogBox/ButtonsContainer/MainMenuBtn/Text", "win_main_menu"),
            ("LosePanel/DialogBox/HeaderTitle/TitleText", "lose_title"),
            ("LosePanel/DialogBox/ButtonsContainer/RetryBtn/Text", "lose_retry"),
            ("LosePanel/DialogBox/ButtonsContainer/MainMenuBtn/Text", "lose_main_menu"),
            ("TipPopupPanel/HeaderTitle/TitleText", "tip_popup_title"),
            ("TipPopupPanel/MessageText", "tip_popup_desc"),
            ("TipPopupPanel/WatchAdBtn/Text", "tip_popup_watch_ad"),
            ("TipPopupPanel/CloseBtn/Text", "tip_popup_close"),

            // Tutorial
            ("TutorialHand/ActionBubble/Text", "tutorial_tap"),
            ("TutorialOverlay/Banner/InstructionText", "tutorial_welcome"),
            ("TutorialOverlay/CompleteCard/TitleText", "tutorial_complete"),
            ("TutorialOverlay/CompleteCard/SubtitleText2", "tutorial_sub2"),
            ("TutorialOverlay/CompleteCard/SubtitleText", "tutorial_sub1"),
            ("TutorialOverlay/CompleteCard/ContinueButton/Text", "win_main_menu")
        };

        static LocalizationAutoAttacher()
        {
            EditorApplication.delayCall += CheckAndRun;
        }

        private static void CheckAndRun()
        {
            if (!EditorPrefs.GetBool(PrefsKey, false))
            {
                RunAttachment();
                EditorPrefs.SetBool(PrefsKey, true);
            }
        }

        [MenuItem("Tools/ArrowSwarm/Attach LocalizedText Components")]
        public static void RunAttachment()
        {
            int totalAttached = 0;
            Debug.Log("[ArrowSwarm] Starting UI LocalizedText attachment scan...");

            // 1. Process UI Prefabs
            string[] prefabPaths = new[]
            {
                "Assets/_Project/Prefabs/UI/Canvas_Overlay.prefab",
                "Assets/_Project/Prefabs/UI/Canvas_HUD.prefab",
                "Assets/_Project/Prefabs/UI/Tutorial_Root.prefab"
            };

            foreach (var path in prefabPaths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                int attached = ProcessGameObjectHierarchy(prefab);
                if (attached > 0)
                {
                    EditorUtility.SetDirty(prefab);
                    PrefabUtility.SavePrefabAsset(prefab);
                    Debug.Log($"[ArrowSwarm] Attached {attached} LocalizedText components to prefab: {path}");
                    totalAttached += attached;
                }
            }

            // 2. Process Scenes
            string[] scenePaths = new[]
            {
                "Assets/_Project/Scenes/MainMenuScene.unity",
                "Assets/_Project/Scenes/BootScene.unity",
                "Assets/_Project/Scenes/GameScene.unity"
            };

            string originalScenePath = SceneManager.GetActiveScene().path;

            foreach (var sPath in scenePaths)
            {
                Scene scene;
                bool wasLoaded = false;

                if (SceneManager.GetActiveScene().path == sPath)
                {
                    scene = SceneManager.GetActiveScene();
                    wasLoaded = true;
                }
                else
                {
                    scene = EditorSceneManager.OpenScene(sPath, OpenSceneMode.Single);
                }

                int sceneAttached = 0;
                var rootObjects = scene.GetRootGameObjects();
                foreach (var root in rootObjects)
                {
                    sceneAttached += ProcessGameObjectHierarchy(root);
                }

                if (sceneAttached > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[ArrowSwarm] Attached {sceneAttached} LocalizedText components to scene: {sPath}");
                    totalAttached += sceneAttached;
                }
            }

            // Restore initial scene if needed
            if (!string.IsNullOrEmpty(originalScenePath) && SceneManager.GetActiveScene().path != originalScenePath)
            {
                EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ArrowSwarm] LocalizedText attachment complete! Total attached/updated: {totalAttached}");
        }

        private static int ProcessGameObjectHierarchy(GameObject root)
        {
            int count = 0;
            var tmpComponents = root.GetComponentsInChildren<TextMeshProUGUI>(true);

            foreach (var tmp in tmpComponents)
            {
                string path = GetHierarchyPath(tmp.transform);
                string text = tmp.text != null ? tmp.text.Trim() : string.Empty;

                string matchedKey = FindMatchingKey(path, text);
                if (!string.IsNullOrEmpty(matchedKey))
                {
                    var loc = tmp.GetComponent<LocalizedText>();
                    if (loc == null)
                    {
                        loc = Undo.AddComponent<LocalizedText>(tmp.gameObject);
                    }

                    if (loc != null && loc.LocalizationKey != matchedKey)
                    {
                        Undo.RecordObject(loc, "Set Localization Key");
                        loc.SetKey(matchedKey);
                        EditorUtility.SetDirty(loc);
                        EditorUtility.SetDirty(tmp.gameObject);
                        count++;
                    }
                }
            }

            return count;
        }

        private static string FindMatchingKey(string hierarchyPath, string currentText)
        {
            // 1. Path pattern match
            foreach (var (pattern, key) in KeyMappings)
            {
                if (hierarchyPath.EndsWith(pattern, StringComparison.OrdinalIgnoreCase) ||
                    hierarchyPath.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    return key;
                }
            }

            // 2. Direct exact text match fallback
            if (currentText.Equals("PLAY", StringComparison.OrdinalIgnoreCase)) return "menu_play";
            if (currentText.Equals("LEADERBOARD", StringComparison.OrdinalIgnoreCase)) return "menu_leaderboard";
            if (currentText.Equals("SETTINGS", StringComparison.OrdinalIgnoreCase)) return "settings_title";
            if (currentText.Equals("LEVELS", StringComparison.OrdinalIgnoreCase)) return "menu_levels";
            if (currentText.Equals("RESUME", StringComparison.OrdinalIgnoreCase) || currentText.Equals("CONTINUE", StringComparison.OrdinalIgnoreCase)) return "pause_resume";
            if (currentText.Equals("RETRY", StringComparison.OrdinalIgnoreCase)) return "pause_retry";
            if (currentText.Equals("MAIN MENU", StringComparison.OrdinalIgnoreCase)) return "pause_main_menu";
            if (currentText.Equals("NEXT LEVEL", StringComparison.OrdinalIgnoreCase)) return "win_next_level";
            if (currentText.Equals("CLOSE", StringComparison.OrdinalIgnoreCase)) return "tip_popup_close";
            if (currentText.Equals("WATCH AD", StringComparison.OrdinalIgnoreCase)) return "tip_popup_watch_ad";
            if (currentText.Equals("SAVE & PLAY", StringComparison.OrdinalIgnoreCase)) return "profile_save_play";
            if (currentText.Equals("SKIP", StringComparison.OrdinalIgnoreCase)) return "profile_skip";
            if (currentText.Equals("VIBRATION", StringComparison.OrdinalIgnoreCase)) return "settings_vibration";
            if (currentText.Equals("SFX", StringComparison.OrdinalIgnoreCase) || currentText.Equals("SOUND", StringComparison.OrdinalIgnoreCase)) return "settings_sfx";
            if (currentText.Equals("VFX", StringComparison.OrdinalIgnoreCase)) return "settings_vfx";
            if (currentText.Equals("LANGUAGE", StringComparison.OrdinalIgnoreCase)) return "settings_language";
            if (currentText.Equals("THEME", StringComparison.OrdinalIgnoreCase)) return "settings_theme";

            return null;
        }

        private static string GetHierarchyPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = $"{t.name}/{path}";
            }
            return path;
        }
    }
}
