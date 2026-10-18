namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Linq;
    using ArrowSwarm.Puzzle;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using U = ModeAuthoringUtility;

    /// <summary>Persists Shapes-only presentation tuning without modifying the shared puzzle config.</summary>
    public static class ShapesPresentationAuthoring
    {
        private const string ConfigPath = U.Data + "ShapesPuzzleConfig.asset";

        /// <summary>Returns the dedicated Shapes config, falling back for projects not yet upgraded.</summary>
        public static PuzzleConfig Config => AssetDatabase.LoadAssetAtPath<PuzzleConfig>(ConfigPath) ??
            AssetDatabase.LoadAssetAtPath<PuzzleConfig>(U.Data + "PuzzleConfig.asset");

        /// <summary>Creates independent tuning and binds only the existing Shapes scene.</summary>
        [MenuItem("Arrow Swarm/Puzzles/Revision 3/Apply Shapes Presentation")]
        public static void Apply()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode before authoring Shapes.");
            var previous = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (previous.isDirty) throw new InvalidOperationException("Save the active scene before authoring Shapes.");
            string previousPath = previous.path;
            if (AssetDatabase.LoadAssetAtPath<PuzzleConfig>(ConfigPath) == null &&
                !AssetDatabase.CopyAsset(U.Data + "PuzzleConfig.asset", ConfigPath))
                throw new InvalidOperationException("Could not create the Shapes-only config.");
            var config = AssetDatabase.LoadAssetAtPath<PuzzleConfig>(ConfigPath);
            U.Set(config, "_strokeWidth", .42f); U.Set(config, "_headSize", .72f);
            U.Set(config, "_hitRadius", .42f);
            U.Set(config, "_minimumDarkLuminance", .28f); U.Set(config, "_maximumLightLuminance", .46f);
            AssetDatabase.SaveAssetIfDirty(config);
            const string scenePath = "Assets/_Project/Scenes/ShapesScene.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            try
            {
                config = AssetDatabase.LoadAssetAtPath<PuzzleConfig>(ConfigPath);
                if (config == null || !EditorUtility.IsPersistent(config))
                    throw new InvalidOperationException("Shapes presentation requires a persistent config asset.");
                var session = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<PuzzleSession>(true)).Single();
                U.Set(session, "_config", config);
                var camera = scene.GetRootGameObjects().SelectMany(root =>
                    root.GetComponentsInChildren<ArrowSwarm.Camera.CameraController>(true)).Single();
                U.Set(camera, "_puzzleWidthCoverage", .95f); U.Set(camera, "_puzzleVerticalInset", .015f);
                Canvas.ForceUpdateCanvases();
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save Shapes presentation.");
            }
            finally
            {
                if (!string.IsNullOrEmpty(previousPath) && previousPath != scenePath)
                    EditorSceneManager.OpenScene(previousPath, OpenSceneMode.Single);
            }
            Debug.Log("[ArrowSwarm] Shapes-only stroke/head, palette and camera framing authored; shared puzzle tuning preserved.");
        }
    }
}
