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
    public static partial class ShapesRevision3Authoring
    {
        /// <summary>Regenerates only Shapes, retaining catalog order and every existing level GUID.</summary>
        [MenuItem("Arrow Swarm/Puzzles/Revision 3/Build 50 Shapes")]
        public static void Build() => BuildAt(false);

        /// <summary>Validates balance and visual identity before persisting all fifty revised puzzles.</summary>
        [MenuItem("Arrow Swarm/Puzzles/Revision 4/Build Balanced 50 Shapes")]
        public static void BuildRevision4() => BuildAt(true);

        private static void BuildAt(bool balanced)
        {
            if (Application.isPlaying || _building) throw new InvalidOperationException("Stop Play Mode and any existing Shapes build first.");
            AssetDatabase.ReleaseCachedFileHandles();
            string folder = balanced ? "Captures/Revision4" : "Captures/Revision3";
            var generated = new PuzzleLevel[50]; var targets = new PuzzleLevel[50];
            try
            {
                var catalog = AssetDatabase.LoadAssetAtPath<PuzzleCatalog>(U.Data + "ShapesCatalog.asset");
                if (catalog == null || catalog.Count != 50) throw new InvalidOperationException("Expected the existing 50-level Shapes catalog.");
                for (int i = 0; i < 50; i++)
                {
                    EditorUtility.DisplayProgressBar("Shapes " + (balanced ? "Revision 4" : "Revision 3"), "Validating " + (i + 1) + "/50", i / 50f);
                    targets[i] = AssetDatabase.LoadAssetAtPath<PuzzleLevel>(U.Data + "Shapes_" + (i + 1) + ".asset");
                    if (targets[i] == null || catalog.Get(i + 1) != targets[i])
                        throw new InvalidOperationException("Existing Shapes catalog identity mismatch at " + (i + 1));
                    generated[i] = DenseCampaignGeneration.CreateShape(i);
                }
                PersistValidated(targets, generated, balanced, folder);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                foreach (var level in generated) if (level != null) UnityEngine.Object.DestroyImmediate(level);
            }
        }

        private static void PersistValidated(PuzzleLevel[] targets, PuzzleLevel[] generated, bool balanced, string folder)
        {
            var signatures = new HashSet<string>();
            var report = new StringBuilder("level,title,width,height,arrows,cells,depth,inward,directions,colors\n");
            for (int i = 0; i < generated.Length; i++)
            {
                var level = generated[i];
                if (!signatures.Add(DenseCampaignVerification.GeometrySignature(level)))
                    throw new InvalidOperationException("Duplicate Shapes geometry at " + (i + 1));
                var graph = DenseDependencyGraph.Analyze(level.Arrows, level.Width, level.Height);
                var colors = new HashSet<Color32>(); int cells = 0;
                foreach (var arrow in level.Arrows) { colors.Add((Color32)arrow.Color); cells += arrow.Points.Count; }
                report.AppendLine((i + 1) + "," + level.Title + "," + level.Width + "," + level.Height + "," +
                    level.Arrows.Count + "," + cells + "," + graph.MaximumDepth + "," + graph.InwardHeads + "," + graph.DirectionCount + "," + colors.Count);
            }
            Directory.CreateDirectory(folder);
            if (balanced) File.WriteAllText(folder + "/balance-verification.txt",
                ShapesBalanceVerification.ValidateCandidates(targets, generated));
            // No authored level is changed until the entire candidate campaign passes.
            for (int i = 0; i < 50; i++)
            {
                var level = generated[i];
                targets[i].Author(level.Mode, level.Title, level.Width, level.Height, level.Arrows,
                    level.WallColors, level.TurnBudget, level.SourceImage);
                EditorUtility.SetDirty(targets[i]); AssetDatabase.SaveAssetIfDirty(targets[i]);
            }
            File.WriteAllText(folder + "/authoring-metrics.csv", report.ToString());
            Debug.Log("[ArrowSwarm] PASS: 50 unique Shapes rebuilt in place; exact coverage, four directions, acyclic dependencies and solver witnesses. Color Shift assets unchanged.");
        }
    }
}
