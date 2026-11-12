namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Rebuilds the compact campaign in Edit Mode after every candidate has passed validation.</summary>
    public static class ShapesCompactAuthoring
    {
        private static PuzzleLevel[] _originals;
        private static PuzzleLevel[] _candidates;
        private static int _next;
        private static string _folder;

        /// <summary>Latest build progress or terminal result for the editor and Unity MCP.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Starts the production compact catalog authoring pass.</summary>
        [MenuItem("Arrow Swarm/Puzzles/Build Compact 50 Shapes")]
        public static void Begin() => Begin("Captures/UIRollout/Nov10/ShapesAuthoring");

        /// <summary>Generates one in-memory candidate per editor update, then updates the existing assets.</summary>
        public static void Begin(string reportFolder)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || _candidates != null)
                throw new InvalidOperationException("Stop Play Mode and any existing compact Shapes build first.");
            if (string.IsNullOrWhiteSpace(reportFolder))
                throw new ArgumentException("Specify an authoring report folder.", nameof(reportFolder));

            var catalog = AssetDatabase.LoadAssetAtPath<PuzzleCatalog>(ModeAuthoringUtility.Data + "ShapesCatalog.asset");
            if (catalog == null || catalog.Count != DenseShapeLibrary.Count)
                throw new InvalidOperationException("Expected the existing 50-level Shapes catalog.");

            var originals = new PuzzleLevel[catalog.Count];
            for (int i = 0; i < originals.Length; i++)
            {
                originals[i] = AssetDatabase.LoadAssetAtPath<PuzzleLevel>(ModeAuthoringUtility.Data + "Shapes_" + (i + 1) + ".asset");
                if (originals[i] == null || catalog.Get(i + 1) != originals[i] || originals[i].Mode != GameModeId.Shapes)
                    throw new InvalidOperationException("Shapes catalog identity mismatch at " + (i + 1));
            }

            _originals = originals;
            _candidates = new PuzzleLevel[originals.Length];
            _next = 0;
            _folder = reportFolder;
            Status = "Generating compact Shapes 0/" + _candidates.Length;
            EditorApplication.update += GenerateNext;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += Cancel;
        }

        private static void GenerateNext()
        {
            try
            {
                _candidates[_next] = ShapesCompactGeneration.Create(_next, _originals[_next]);
                _next++;
                Status = "Generating compact Shapes " + _next + "/" + _candidates.Length;
                if (_next < _candidates.Length) return;
                PersistCampaign();
                Finish("PASS: compact Shapes campaign validated and saved with all 50 original level identities.");
            }
            catch (Exception error)
            {
                Finish("FAIL: " + error.Message);
                Debug.LogException(error);
            }
        }

        private static void PersistCampaign()
        {
            var unique = new HashSet<string>();
            var report = new StringBuilder("level,title,old_width,old_height,old_arrows,width,height,arrows,cells,dependency_depth,inward,initial_legal\n");
            var backups = new string[_originals.Length];
            for (int i = 0; i < _candidates.Length; i++)
            {
                var before = _originals[i];
                var after = _candidates[i];
                if (!unique.Add(DenseCampaignVerification.GeometrySignature(after)) ||
                    after.Title != before.Title || after.SourceImage != before.SourceImage)
                    throw new InvalidOperationException("Compact Shapes identity validation failed at " + (i + 1));
                backups[i] = EditorJsonUtility.ToJson(before);
                var metrics = ShapesDifficultyMetrics.Analyze(after.Arrows, after.Width, after.Height);
                report.AppendFormat(CultureInfo.InvariantCulture, "{0},\"{1}\",{2},{3},{4},{5},{6},{7},{8},{9},{10:F3},{11}\n",
                    i + 1, after.Title.Replace("\"", "\"\""), before.Width, before.Height, before.Arrows.Count,
                    after.Width, after.Height, after.Arrows.Count, metrics.CoveredCells,
                    metrics.DependencyLayers, metrics.InwardRatio, metrics.InitialLegal);
            }

            Directory.CreateDirectory(_folder);
            int edited = 0;
            try
            {
                for (int i = 0; i < _candidates.Length; i++)
                {
                    var candidate = _candidates[i];
                    var target = _originals[i];
                    edited++;
                    target.Author(candidate.Mode, candidate.Title, candidate.Width, candidate.Height, candidate.Arrows,
                        candidate.WallColors, candidate.TurnBudget, candidate.SourceImage);
                    EditorUtility.SetDirty(target);
                    AssetDatabase.SaveAssetIfDirty(target);
                }
                File.WriteAllText(Path.Combine(_folder, "authoring-metrics.csv"), report.ToString());
            }
            catch
            {
                for (int i = 0; i < edited; i++)
                {
                    EditorJsonUtility.FromJsonOverwrite(backups[i], _originals[i]);
                    EditorUtility.SetDirty(_originals[i]);
                    AssetDatabase.SaveAssetIfDirty(_originals[i]);
                }
                throw;
            }
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode) Cancel();
        }

        private static void Cancel() => Finish("ABORT: compact Shapes authoring interrupted.");

        private static void Finish(string result)
        {
            EditorApplication.update -= GenerateNext;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= Cancel;
            foreach (var candidate in _candidates)
                if (candidate != null) UnityEngine.Object.DestroyImmediate(candidate);
            _candidates = null;
            _originals = null;
            Status = result;
            Debug.Log("[ArrowSwarm] " + result);
        }
    }
}
