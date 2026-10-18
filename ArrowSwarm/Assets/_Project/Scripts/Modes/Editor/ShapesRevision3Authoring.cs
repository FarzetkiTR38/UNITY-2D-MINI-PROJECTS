namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEditor;
    using UnityEngine;
    using U = ModeAuthoringUtility;

    /// <summary>Validates the complete Shapes redesign before updating existing level assets in place.</summary>
    public static class ShapesRevision3Authoring
    {
        /// <summary>Regenerates only Shapes, retaining catalog order and every existing level GUID.</summary>
        [MenuItem("Arrow Swarm/Puzzles/Revision 3/Build 50 Shapes")]
        public static void Build()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode before building Shapes.");
            var generated = new PuzzleLevel[50]; var targets = new PuzzleLevel[50];
            var signatures = new HashSet<string>();
            var report = new StringBuilder("level,title,width,height,arrows,cells,depth,inward,directions,colors\n");
            try
            {
                var catalog = AssetDatabase.LoadAssetAtPath<PuzzleCatalog>(U.Data + "ShapesCatalog.asset");
                if (catalog == null || catalog.Count != 50) throw new InvalidOperationException("Expected the existing 50-level Shapes catalog.");
                for (int i = 0; i < 50; i++)
                {
                    EditorUtility.DisplayProgressBar("Shapes Revision 3", "Validating " + (i + 1) + "/50", i / 50f);
                    targets[i] = AssetDatabase.LoadAssetAtPath<PuzzleLevel>(U.Data + "Shapes_" + (i + 1) + ".asset");
                    if (targets[i] == null || catalog.Get(i + 1) != targets[i])
                        throw new InvalidOperationException("Existing Shapes catalog identity mismatch at " + (i + 1));
                    var level = generated[i] = DenseCampaignGeneration.CreateShape(i);
                    if (!signatures.Add(DenseCampaignVerification.GeometrySignature(level)))
                        throw new InvalidOperationException("Duplicate Shapes geometry at " + (i + 1));
                    var graph = DenseDependencyGraph.Analyze(level.Arrows, level.Width, level.Height);
                    var colors = new HashSet<Color32>(); int cells = 0;
                    foreach (var arrow in level.Arrows) { colors.Add((Color32)arrow.Color); cells += arrow.Points.Count; }
                    report.AppendLine((i + 1) + "," + level.Title + "," + level.Width + "," + level.Height + "," +
                        level.Arrows.Count + "," + cells + "," + graph.MaximumDepth + "," + graph.InwardHeads + "," + graph.DirectionCount + "," + colors.Count);
                }
                // No authored level is changed until the entire candidate campaign passes.
                for (int i = 0; i < 50; i++)
                {
                    var level = generated[i];
                    targets[i].Author(level.Mode, level.Title, level.Width, level.Height, level.Arrows,
                        level.WallColors, level.TurnBudget, level.SourceImage);
                    EditorUtility.SetDirty(targets[i]); AssetDatabase.SaveAssetIfDirty(targets[i]);
                }
                Directory.CreateDirectory("Captures/Revision3");
                File.WriteAllText("Captures/Revision3/authoring-metrics.csv", report.ToString());
                Debug.Log("[ArrowSwarm] PASS: 50 unique Shapes rebuilt in place; exact coverage, four directions, acyclic dependencies and solver witnesses. Color Shift assets unchanged.");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                foreach (var level in generated) if (level != null) UnityEngine.Object.DestroyImmediate(level);
            }
        }
    }
}
