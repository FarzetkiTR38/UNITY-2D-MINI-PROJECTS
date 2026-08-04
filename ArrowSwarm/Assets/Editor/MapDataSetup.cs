using UnityEngine;
using UnityEditor;
using ArrowSwarm.Core;
using System.Collections.Generic;

/// <summary>
/// Editor utility to setup Phase 4 MapData defaults.
/// Run via menu: ArrowSwarm → Setup Map Data (Phase 4)
/// </summary>
public static class MapDataSetup
{
    [MenuItem("ArrowSwarm/Setup Map Data (Phase 4)")]
    public static void Setup()
    {
        string[] paths = new string[]
        {
            "Assets/_Project/ScriptableObjects/MapData/Map1_Forest.asset",
            "Assets/_Project/ScriptableObjects/MapData/Map2_Ocean.asset",
            "Assets/_Project/ScriptableObjects/MapData/Map3_Desert.asset",
            "Assets/_Project/ScriptableObjects/MapData/Map4_Mountain.asset",
            "Assets/_Project/ScriptableObjects/MapData/Map5_Space.asset"
        };
        
        string[] names = { "Forest", "Ocean", "Desert", "Mountain", "Space" };
        int[] widths = { 6, 7, 8, 9, 10 };
        int[] heights = { 8, 9, 10, 7, 8 };
        Color[] pathColors = { 
            new Color(0.3f, 0.6f, 0.3f, 1f), // Forest green
            new Color(0.2f, 0.4f, 0.8f, 1f), // Ocean blue
            new Color(0.8f, 0.6f, 0.2f, 1f), // Desert yellow
            new Color(0.6f, 0.6f, 0.6f, 1f), // Mountain gray
            new Color(0.5f, 0.2f, 0.8f, 1f)  // Space purple
        };

        for (int i = 0; i < paths.Length; i++)
        {
            MapData map = AssetDatabase.LoadAssetAtPath<MapData>(paths[i]);
            if (map != null)
            {
                SerializedObject so = new SerializedObject(map);
                
                so.FindProperty("_mapName").stringValue = names[i];
                so.FindProperty("_mapIndex").intValue = i;
                so.FindProperty("_gridWidth").intValue = widths[i];
                so.FindProperty("_gridHeight").intValue = heights[i];
                so.FindProperty("_cellSize").floatValue = 0.8f;
                so.FindProperty("_gridOrigin").vector2Value = new Vector2(- (widths[i] * 0.8f) / 2f + 0.4f, - (heights[i] * 0.8f) / 2f + 0.4f); // Center grid roughly
                
                // Set path color
                so.FindProperty("_pathColor").colorValue = pathColors[i];
                so.FindProperty("_gridLineColor").colorValue = new Color(0.2f, 0.2f, 0.3f, 1f);

                // Setup basic path around the edge of the grid
                Vector2 spawn = new Vector2(- (widths[i] * 0.8f) / 2f - 1f, - (heights[i] * 0.8f) / 2f);
                Vector2 finish = new Vector2((widths[i] * 0.8f) / 2f + 1f, (heights[i] * 0.8f) / 2f);
                
                so.FindProperty("_spawnPoint").vector2Value = spawn;
                so.FindProperty("_finishPoint").vector2Value = finish;
                
                SerializedProperty waypoints = so.FindProperty("_pathWaypoints");
                waypoints.arraySize = 4;
                waypoints.GetArrayElementAtIndex(0).vector2Value = spawn;
                waypoints.GetArrayElementAtIndex(1).vector2Value = new Vector2(spawn.x, finish.y);
                waypoints.GetArrayElementAtIndex(2).vector2Value = new Vector2(finish.x, finish.y);
                waypoints.GetArrayElementAtIndex(3).vector2Value = finish;
                
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(map);
            }
            else
            {
                Debug.LogError($"[ArrowSwarm] MapData not found at {paths[i]}");
            }
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log("[ArrowSwarm] MapData assets updated successfully!");
    }
}
