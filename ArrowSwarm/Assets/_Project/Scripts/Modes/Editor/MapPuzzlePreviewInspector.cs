namespace ArrowSwarm.Modes.Editor
{
    using ArrowSwarm.Core;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Mode selector retains the original Defense inspector and adds safe puzzle previews.</summary>
    public static class MapPuzzlePreviewInspector
    {
        /// <summary>Returns true when the puzzle inspector replaces Defense-only map controls.</summary>
        public static bool Draw(MapSceneController controller,SerializedObject so)
        {
            EditorGUILayout.LabelField("Game Mode Preview",EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("_previewMode"));
            so.ApplyModifiedProperties();
            if(controller.PreviewMode==GameModeId.Defense){PuzzleEditorPreview.Clear();return false;}
            EditorGUILayout.PropertyField(so.FindProperty("_puzzleLevel"));so.ApplyModifiedProperties();
            var catalog=AssetDatabase.LoadAssetAtPath<PuzzleCatalog>(ModeAuthoringUtility.Data+(controller.PreviewMode==GameModeId.Shapes?"ShapesCatalog":"ColorShiftCatalog")+".asset");
            var level=catalog?catalog.Get(controller.PuzzlePreviewLevel):null;
            if(!level){EditorGUILayout.HelpBox("Create MVP catalogs first.",MessageType.Warning);return true;}
            var solution=PuzzleSolver.Solve(level);
            EditorGUILayout.HelpBox($"{level.Title}\n{level.Arrows.Count} arrows · {solution.RequiredTurns}/{level.TurnBudget} rotations\nPreview does not award stars or change campaign progress.",MessageType.Info);
            if(GUILayout.Button("Show arrows / frame in Scene View"))PuzzleEditorPreview.Show(level);
            if(GUILayout.Button("Play selected mode / level (preview)"))
            {
                if(Application.isPlaying)controller.StartPuzzlePreview();
                else
                {
                    // Enter the existing MapTestScene; its LevelManager routes to the isolated preview.
                    EditorUtility.SetDirty(controller);
                    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(controller.gameObject.scene);
                    EditorApplication.isPlaying=true;
                }
            }
            if(GUILayout.Button("Open level in Shapes editor"))
            {Selection.activeObject=level;ShapeLevelEditorWindow.Open();}
            return true;
        }
    }
}
