#if UNITY_EDITOR
namespace ArrowSwarm.Core.Editor
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Editor utility to generate and configure standalone map test/preview scenes.
    /// Accessible via Unity menu: ArrowSwarm > Generate Map Scenes.
    /// </summary>
    public static class MapSceneGenerator
    {
        private struct MapSceneDef
        {
            public int MapIndex;
            public string MapName;
            public string SceneName;
            public int DefaultLevel;
        }

        private static readonly MapSceneDef[] MapDefs = new MapSceneDef[]
        {
            new MapSceneDef { MapIndex = 0, MapName = "Map 1", SceneName = "Map1Scene", DefaultLevel = 1 },
            new MapSceneDef { MapIndex = 1, MapName = "Map 2", SceneName = "Map2Scene", DefaultLevel = 6 },
            new MapSceneDef { MapIndex = 2, MapName = "Map 3", SceneName = "Map3Scene", DefaultLevel = 11 },
            new MapSceneDef { MapIndex = 3, MapName = "Map 4", SceneName = "Map4Scene", DefaultLevel = 16 },
            new MapSceneDef { MapIndex = 4, MapName = "Map 5", SceneName = "Map5Scene", DefaultLevel = 21 },
            new MapSceneDef { MapIndex = 5, MapName = "Map 6", SceneName = "Map6Scene", DefaultLevel = 30 },
            new MapSceneDef { MapIndex = 6, MapName = "Map 7", SceneName = "Map7Scene", DefaultLevel = 26 },
            new MapSceneDef { MapIndex = 7, MapName = "Map 8", SceneName = "Map8Scene", DefaultLevel = 27 },
            new MapSceneDef { MapIndex = 8, MapName = "Map 9", SceneName = "Map9Scene", DefaultLevel = 28 },
            new MapSceneDef { MapIndex = 9, MapName = "Map 10", SceneName = "Map10Scene", DefaultLevel = 29 },
            new MapSceneDef { MapIndex = 10, MapName = "Map 11", SceneName = "Map11Scene", DefaultLevel = 50 },
            new MapSceneDef { MapIndex = 11, MapName = "Map 12", SceneName = "Map12Scene", DefaultLevel = 100 }
        };

        [MenuItem("ArrowSwarm/Generate Map Scenes")]
        public static void GenerateAllMapScenes()
        {
            string gameScenePath = "Assets/_Project/Scenes/GameScene.unity";
            if (!File.Exists(gameScenePath))
            {
                Debug.LogError($"[ArrowSwarm] MapSceneGenerator: Source GameScene not found at {gameScenePath}");
                return;
            }

            // Save currently active scene before generating
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            List<string> generatedScenePaths = new List<string>();

            for (int i = 0; i < MapDefs.Length; i++)
            {
                var def = MapDefs[i];
                string targetPath = $"Assets/_Project/Scenes/{def.SceneName}.unity";

                // Duplicate GameScene to create standalone map scene
                AssetDatabase.CopyAsset(gameScenePath, targetPath);
                AssetDatabase.Refresh();

                // Open the copied scene to configure MapSceneController
                Scene scene = EditorSceneManager.OpenScene(targetPath, OpenSceneMode.Single);

                // Find or create MapSceneController
                var controllerObj = GameObject.Find("MapSceneController");
                if (controllerObj == null)
                {
                    controllerObj = new GameObject("MapSceneController");
                }

                var controller = controllerObj.GetComponent<MapSceneController>();
                if (controller == null)
                {
                    controller = controllerObj.AddComponent<MapSceneController>();
                }

                controller.ActiveMapIndex = def.MapIndex;
                controller.DefaultLevel = def.DefaultLevel;

                EditorUtility.SetDirty(controller);
                EditorUtility.SetDirty(controllerObj);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);

                generatedScenePaths.Add(targetPath);
                Debug.Log($"[ArrowSwarm] Generated Map Scene: {def.SceneName} (Map {def.MapIndex}: {def.MapName}, Default Level: {def.DefaultLevel})");
            }

            // Update Build Settings
            UpdateBuildSettings(generatedScenePaths);

            // Re-open Map1 as starting preview scene
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Map1_ForestScene.unity", OpenSceneMode.Single);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[ArrowSwarm] ✅ Successfully generated and configured all 5 Map Scenes in Assets/_Project/Scenes/!");
        }

        private static void UpdateBuildSettings(List<string> mapScenePaths)
        {
            var currentScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            var scenePathSet = new HashSet<string>();

            foreach (var s in currentScenes)
            {
                scenePathSet.Add(s.path);
            }

            // Ensure base scenes exist
            string[] baseScenes = new string[]
            {
                "Assets/_Project/Scenes/BootScene.unity",
                "Assets/_Project/Scenes/MainMenuScene.unity",
                "Assets/_Project/Scenes/GameScene.unity"
            };

            foreach (var b in baseScenes)
            {
                if (!scenePathSet.Contains(b) && File.Exists(b))
                {
                    currentScenes.Add(new EditorBuildSettingsScene(b, true));
                    scenePathSet.Add(b);
                }
            }

            // Add new map scenes
            foreach (var mapPath in mapScenePaths)
            {
                if (!scenePathSet.Contains(mapPath) && File.Exists(mapPath))
                {
                    currentScenes.Add(new EditorBuildSettingsScene(mapPath, true));
                    scenePathSet.Add(mapPath);
                }
            }

            EditorBuildSettings.scenes = currentScenes.ToArray();
            Debug.Log($"[ArrowSwarm] Updated EditorBuildSettings with {EditorBuildSettings.scenes.Length} total scenes.");
        }
    }
}
#endif
