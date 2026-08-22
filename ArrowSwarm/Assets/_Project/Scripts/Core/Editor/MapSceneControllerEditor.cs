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
            "🌲 Forest (6×8)",
            "🌊 Ocean (8×10)",
            "🏜️ Desert (10×12)",
            "🏔️ Mountain (12×15)",
            "🌌 Space (15×20)"
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
                $"Active Map: {mapName} ({gridInfo})\nCurrent Level: {controller.DefaultLevel} (Range: {controller.CurrentLevelRange.x}-{controller.CurrentLevelRange.y})",
                MessageType.Info);

            EditorGUILayout.Space(6);
            DrawDefaultInspector();

            // 1. Map Selection Buttons
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("🗺️ Map Selector (1-Click Switch)", EditorStyles.boldLabel);

            for (int i = 0; i < MapButtonLabels.Length; i++)
            {
                bool isCurrent = (i == controller.ActiveMapIndex);
                GUI.backgroundColor = isCurrent ? new Color(0.3f, 0.9f, 0.5f) : Color.white;

                if (GUILayout.Button(MapButtonLabels[i], GUILayout.Height(30)))
                {
                    controller.SelectMap(i);
                    EditorUtility.SetDirty(controller);
                    if (!Application.isPlaying) SceneView.RepaintAll();
                }
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
