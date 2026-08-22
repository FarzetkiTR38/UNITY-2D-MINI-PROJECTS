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
    /// Editor utility to generate and configure the 5 standalone map test/preview scenes.
    /// Accessible via Unity menu: ArrowSwarm > Generate 5 Map Scenes.
    /// </summary>
    public static class MapSceneGenerator
    {
        private struct MapSceneDef
        {
            public int MapIndex;
            public string MapName;
            public string SceneName;
            public int DefaultLevel;
            public Vector2Int LevelRange;
        }

        private static readonly MapSceneDef[] MapDefs = new MapSceneDef[]
        {
            new MapSceneDef { MapIndex = 0, MapName = "Forest", SceneName = "Map1_ForestScene", DefaultLevel = 1, LevelRange = new Vector2Int(1, 5) },
            new MapSceneDef { MapIndex = 1, MapName = "Ocean", SceneName = "Map2_OceanScene", DefaultLevel = 6, LevelRange = new Vector2Int(6, 10) },
            new MapSceneDef { MapIndex = 2, MapName = "Desert", SceneName = "Map3_DesertScene", DefaultLevel = 11, LevelRange = new Vector2Int(11, 15) },
            new MapSceneDef { MapIndex = 3, MapName = "Mountain", SceneName = "Map4_MountainScene", DefaultLevel = 16, LevelRange = new Vector2Int(16, 20) },
            new MapSceneDef { MapIndex = 4, MapName = "Space", SceneName = "Map5_SpaceScene", DefaultLevel = 21, LevelRange = new Vector2Int(21, 25) }
        };

        [MenuItem("ArrowSwarm/Generate 5 Map Scenes")]
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
