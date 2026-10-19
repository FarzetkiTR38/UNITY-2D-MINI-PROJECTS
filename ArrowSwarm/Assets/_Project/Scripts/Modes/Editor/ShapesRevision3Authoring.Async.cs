namespace ArrowSwarm.Modes.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Generates one validated candidate per editor tick before committing the complete catalog.</summary>
    public static partial class ShapesRevision3Authoring
    {
        private static PuzzleLevel[] _pendingTargets, _pendingLevels;
        private static int _pendingIndex;
        private static bool _building;
        /// <summary>Current asynchronous authoring progress or final outcome for MCP polling.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Starts a responsive all-or-nothing candidate validation pass in Edit Mode.</summary>
        [MenuItem("Arrow Swarm/Puzzles/Revision 4/Build Balanced Shapes Asynchronously")]
        public static void BeginRevision4()
        {
            if (_building || Application.isPlaying) throw new InvalidOperationException("Stop Play Mode and any existing Shapes build first.");
            var catalog = AssetDatabase.LoadAssetAtPath<PuzzleCatalog>(ModeAuthoringUtility.Data + "ShapesCatalog.asset");
            if (catalog == null || catalog.Count != 50) throw new InvalidOperationException("Expected the existing fifty-level Shapes catalog.");
            AssetDatabase.ReleaseCachedFileHandles();
            _pendingTargets = new PuzzleLevel[50]; _pendingLevels = new PuzzleLevel[50];
            for (int i = 0; i < 50; i++)
            {
                _pendingTargets[i] = AssetDatabase.LoadAssetAtPath<PuzzleLevel>(ModeAuthoringUtility.Data + "Shapes_" + (i + 1) + ".asset");
                if (_pendingTargets[i] == null || catalog.Get(i + 1) != _pendingTargets[i])
                    throw new InvalidOperationException("Existing Shapes catalog identity mismatch at " + (i + 1));
            }
            _pendingIndex = 0; _building = true; Status = "Generating balanced Shapes 0/50";
            EditorApplication.update += GenerateNext;
            AssemblyReloadEvents.beforeAssemblyReload += CancelPending;
            EditorApplication.playModeStateChanged += ModeChanged;
        }

        private static void GenerateNext()
        {
            if (!_building) return;
            try
            {
                _pendingLevels[_pendingIndex] = DenseCampaignGeneration.CreateShape(_pendingIndex);
                _pendingIndex++;
                Status = "Generating balanced Shapes " + _pendingIndex + "/50";
                EditorUtility.DisplayProgressBar("Balanced Shapes", Status, _pendingIndex / 50f);
                if (_pendingIndex < 50) return;
                PersistValidated(_pendingTargets, _pendingLevels, true, "Captures/Revision4");
                FinishBuild("PASS: all 50 balanced Shapes validated before persistence; catalog identities preserved");
            }
            catch (Exception error)
            {
                FinishBuild("FAIL: " + error.Message); Debug.LogException(error);
            }
        }

        private static void ModeChanged(PlayModeStateChange value)
        { if (value == PlayModeStateChange.ExitingEditMode) CancelPending(); }
        private static void CancelPending() { if (_building) FinishBuild("ABORT: authoring interrupted before completion"); }
        private static void FinishBuild(string outcome)
        {
            _building = false; Status = outcome;
            EditorApplication.update -= GenerateNext; AssemblyReloadEvents.beforeAssemblyReload -= CancelPending;
            EditorApplication.playModeStateChanged -= ModeChanged; EditorUtility.ClearProgressBar();
            if (_pendingLevels != null) foreach (var level in _pendingLevels)
                if (level != null) UnityEngine.Object.DestroyImmediate(level);
            _pendingLevels = null; _pendingTargets = null;
            Debug.Log("[ArrowSwarm] " + outcome);
        }
    }
}
