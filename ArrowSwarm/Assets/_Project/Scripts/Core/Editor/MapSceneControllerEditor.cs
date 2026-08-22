#if UNITY_EDITOR
namespace ArrowSwarm.Core.Editor
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Custom Inspector for MapSceneController providing 1-click map switching,
    /// in-map level buttons, and live camera fit.
    /// </summary>
    [CustomEditor(typeof(MapSceneController))]
    public class MapSceneControllerEditor : Editor
    {
        private static readonly string[] MapButtonLabels = new string[]
        {
            "Map 1 (6×8)",
            "Map 2 (8×10)",
            "Map 3 (10×12)",
            "Map 4 (12×15)",
            "Map 5 (15×20)",
            "Map 6 (15×25)",
            "Map 7 (15×30)",
            "Map 8 (20×25)",
            "Map 9 (20×30)",
            "Map 10 (20×35)",
            "Map 11 (20×40)",
            "Map 12 (25×40)"
        };

        public override void OnInspectorGUI()
        {
            if (target == null) return;

            serializedObject.Update();
            var controller = (MapSceneController)target;
            if (controller == null) return;

            EditorGUILayout.Space(4);
            string mapName = controller.MapName;
            MapData activeMap = controller.GetActiveMap();
            string gridInfo = activeMap != null ? $"{activeMap.GridWidth}×{activeMap.GridHeight}" : "?×?";
            EditorGUILayout.HelpBox(
                $"Active: {mapName} ({gridInfo}) | Default Level: {controller.DefaultLevel}",
                MessageType.Info);

            EditorGUILayout.Space(6);
            DrawDefaultInspector();

            // 1. Map Selection Buttons (2 columns)
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("🗺️ Map Selector (1-Click Switch)", EditorStyles.boldLabel);

            for (int i = 0; i < MapButtonLabels.Length; i += 2)
            {
                EditorGUILayout.BeginHorizontal();
                for (int col = 0; col < 2; col++)
                {
                    int index = i + col;
                    if (index < MapButtonLabels.Length)
                    {
                        bool isCurrent = (index == controller.ActiveMapIndex);
                        GUI.backgroundColor = isCurrent ? new Color(0.3f, 0.9f, 0.5f) : Color.white;

                        if (GUILayout.Button(MapButtonLabels[index], GUILayout.Height(28)))
                        {
                            controller.SelectMap(index);
                            EditorUtility.SetDirty(controller);
                            if (!Application.isPlaying) SceneView.RepaintAll();
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            GUI.backgroundColor = Color.white;

            // 2. In-Map Level Selector
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"🎮 Levels for {mapName}", EditorStyles.boldLabel);

            Vector2Int range = controller.CurrentLevelRange;
            EditorGUILayout.BeginHorizontal();
            for (int lvl = range.x; lvl <= range.y; lvl++)
            {
                bool isCurrentLvl = (lvl == controller.DefaultLevel);
                GUI.backgroundColor = isCurrentLvl ? new Color(0.3f, 0.8f, 1f) : Color.white;

                if (GUILayout.Button($"Lv.{lvl}", GUILayout.Height(28)))
                {
                    controller.LoadLevel(lvl);
                    EditorUtility.SetDirty(controller);
                    if (!Application.isPlaying) SceneView.RepaintAll();
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            // 3. Navigation Controls
            EditorGUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("◀ Prev Level", GUILayout.Height(28)))
            {
                controller.PreviousMapLevel();
                EditorUtility.SetDirty(controller);
            }
            if (GUILayout.Button("🔄 Restart", GUILayout.Height(28)))
            {
                controller.RestartCurrentLevel();
            }
            if (GUILayout.Button("Next Level ▶", GUILayout.Height(28)))
            {
                controller.NextMapLevel();
                EditorUtility.SetDirty(controller);
            }
            EditorGUILayout.EndHorizontal();

            // 4. Camera Preview Fit
            EditorGUILayout.Space(8);
            if (GUILayout.Button("📷 Fit Camera to Preview (9:16)", GUILayout.Height(26)))
            {
                controller.FitCameraToPreview();
                SceneView.RepaintAll();
            }

            if (serializedObject != null && serializedObject.targetObject != null)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
#endif
