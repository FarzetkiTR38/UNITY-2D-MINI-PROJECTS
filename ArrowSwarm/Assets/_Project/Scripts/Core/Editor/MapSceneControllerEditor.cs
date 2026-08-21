#if UNITY_EDITOR
namespace ArrowSwarm.Core.Editor
{
    using UnityEditor;
    using UnityEngine;
    using UnityEditor.SceneManagement;

    /// <summary>
    /// Custom Inspector for MapSceneController allowing instant level switching,
    /// map preview navigation, and jumping across Map1..Map5 scenes.
    /// </summary>
    [CustomEditor(typeof(MapSceneController))]
    public class MapSceneControllerEditor : Editor
    {
        private static readonly string[] MapNames = new string[]
        {
            "🌲 Map 1: Forest",
            "🌊 Map 2: Ocean",
            "🏜️ Map 3: Desert",
            "🏔️ Map 4: Mountain",
            "🌌 Map 5: Space"
        };

        private static readonly string[] ScenePaths = new string[]
        {
            "Assets/_Project/Scenes/Map1_ForestScene.unity",
            "Assets/_Project/Scenes/Map2_OceanScene.unity",
            "Assets/_Project/Scenes/Map3_DesertScene.unity",
            "Assets/_Project/Scenes/Map4_MountainScene.unity",
            "Assets/_Project/Scenes/Map5_SpaceScene.unity"
        };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var controller = (MapSceneController)target;
            int mapIdx = Mathf.Clamp(controller.MapIndex, 0, 4);

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                $"{MapNames[mapIdx]}\nLevels {controller.LevelRange.x} to {controller.LevelRange.y} | Active Default Level: {controller.DefaultLevel}",
                MessageType.Info);

            EditorGUILayout.Space(6);
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("🎮 In-Map Level Selector", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            for (int lvl = controller.LevelRange.x; lvl <= controller.LevelRange.y; lvl++)
            {
                bool isCurrent = (lvl == controller.DefaultLevel);
                GUI.backgroundColor = isCurrent ? new Color(0.3f, 0.8f, 1f) : Color.white;

                if (GUILayout.Button($"Lv.{lvl}", GUILayout.Height(28)))
                {
                    if (Application.isPlaying)
                    {
                        controller.LoadLevel(lvl);
                    }
                    else
                    {
                        controller.DefaultLevel = lvl;
                        EditorUtility.SetDirty(controller);
                    }
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("◀ Prev Level"))
            {
                controller.PreviousMapLevel();
            }
            if (GUILayout.Button("🔄 Restart Level"))
            {
                controller.RestartCurrentLevel();
            }
            if (GUILayout.Button("Next Level ▶"))
            {
                controller.NextMapLevel();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(14);
            EditorGUILayout.LabelField("🗺️ Jump to Map Scene", EditorStyles.boldLabel);

            for (int i = 0; i < ScenePaths.Length; i++)
            {
                bool isThisMap = (i == controller.MapIndex);
                GUI.backgroundColor = isThisMap ? new Color(0.4f, 0.9f, 0.5f) : Color.white;

                if (GUILayout.Button(MapNames[i], GUILayout.Height(26)))
                {
                    if (Application.isPlaying)
                    {
                        controller.SwitchToMapScene(i);
                    }
                    else
                    {
                        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            EditorSceneManager.OpenScene(ScenePaths[i]);
                        }
                    }
                }
            }
            GUI.backgroundColor = Color.white;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
