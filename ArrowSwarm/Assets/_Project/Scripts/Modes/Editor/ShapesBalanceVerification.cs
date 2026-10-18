namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Blocks a Shapes rebuild unless it preserves the artwork and measurably improves puzzle structure.</summary>
    public static partial class ShapesBalanceVerification
    {
        private const string ReportFolder = "Captures/Revision4";

        /// <summary>Validates all candidates before the authoring caller persists any existing level asset.</summary>
        public static string ValidateCandidates(PuzzleLevel[] original, PuzzleLevel[] candidates)
        {
            Require(original != null && candidates != null && original.Length == 50 && candidates.Length == 50,
                "expected fifty originals and candidates");
            VerifyMetricFixtures();
            var previous = LoadBaseline(); var revised = new ShapesDifficultyResult[50];
            var failures = new List<string>(); var report = new StringBuilder();
            var signatures = new HashSet<string>();
            for (int i = 0; i < 50; i++)
            {
                var before = original[i]; var after = candidates[i];
                Require(before != null && after != null, "missing level " + (i + 1));
                AssertArtwork(before, after, i + 1);
                revised[i] = AssertPuzzle(after, i + 1);
                Require(signatures.Add(DenseCampaignVerification.GeometrySignature(after)), "repeated candidate layout");
                CheckLevel(previous[i], revised[i], i + 1, failures);
                report.AppendLine((i + 1) + ". " + after.Title + ": " + Describe(previous[i]) + " -> " + Describe(revised[i]));
            }
            CheckCampaign(previous, revised, failures, report);
            Directory.CreateDirectory(ReportFolder);
            WriteCsv(ReportFolder + "/balance-reference.csv", original, previous);
            WriteCsv(ReportFolder + "/balance-candidates.csv", candidates, revised);
            string outcome = failures.Count == 0 ? "PASS: 50 solvable Shapes preserve exact cell colors, silhouette and dimensions; stronger measured puzzle structure."
                : "FAIL: " + string.Join("; ", failures);
            File.WriteAllText(ReportFolder + "/balance-comparison.txt", outcome + "\n" + report);
            Require(failures.Count == 0, outcome);
            Debug.Log("[ArrowSwarm] " + outcome); return outcome;
        }

        /// <summary>Audits the persisted fifty-level Shapes catalog independently of the generation pipeline.</summary>
        [MenuItem("Arrow Swarm/Puzzles/Revision 4/Verify Shapes Balance")]
        public static void ValidateCatalog()
        {
            VerifyMetricFixtures(); LoadBaseline();
            var catalog = AssetDatabase.LoadAssetAtPath<PuzzleCatalog>(ModeAuthoringUtility.Data + "ShapesCatalog.asset");
            Require(catalog != null && catalog.Count == 50, "expected the existing fifty-level catalog");
            var levels = new PuzzleLevel[50]; var metrics = new ShapesDifficultyResult[50];
            var signatures = new HashSet<string>();
            for (int i = 0; i < 50; i++)
            {
                levels[i] = catalog.Get(i + 1); metrics[i] = AssertPuzzle(levels[i], i + 1);
                AssertPinnedArtwork(levels[i], i + 1);
                Require(signatures.Add(DenseCampaignVerification.GeometrySignature(levels[i])), "duplicate catalog geometry");
                Require(metrics[i].InitialLegalRatio <= .36f, "too many initial legal arrows in " + (i + 1));
                Require(metrics[i].InwardRatio >= .22f, "insufficient inward heads in " + (i + 1));
                Require(metrics[i].CellWeightedBends >= 3.5f, "insufficient area-weighted path bends in " + (i + 1));
            }
            Directory.CreateDirectory(ReportFolder); WriteCsv(ReportFolder + "/balance-current.csv", levels, metrics);
            Debug.Log("[ArrowSwarm] PASS: 50 unique balanced Shapes; exact mask coverage, four directions, acyclic dependencies and solver witnesses.");
        }

        private static ShapesDifficultyResult AssertPuzzle(PuzzleLevel level, int number)
        {
            Require(level != null && level.Mode == GameModeId.Shapes, "invalid Shapes mode at " + number);
            string error = PuzzleRules.Validate(level); Require(error == null, number + ": " + error);
            var colors = CellColors(level); var mask = new HashSet<Vector2Int>();
            Require(level.CoverageMask != null && level.CoverageMask.Count > 0, "missing source mask at " + number);
            foreach (var cell in level.CoverageMask) Require(mask.Add(cell), "duplicate source mask cell");
            Require(mask.SetEquals(colors.Keys), "silhouette coverage gap at " + number);
            var solution = PuzzleSolver.Solve(level); Require(solution.Solvable, number + ": " + solution.Error);
            var result = ShapesDifficultyMetrics.Analyze(level.Arrows, level.Width, level.Height);
            Require(result.Solvable && result.DependencyLayers > 1 && result.DirectionCount == 4,
                "missing solvable four-direction dependencies at " + number);
            return result;
        }

        private static void AssertArtwork(PuzzleLevel before, PuzzleLevel after, int number)
        {
            Require(before.Width == after.Width && before.Height == after.Height && before.Title == after.Title,
                "changed silhouette dimensions/title at " + number);
            var oldColors = CellColors(before); var newColors = CellColors(after);
            Require(oldColors.Count == newColors.Count, "changed silhouette cell count at " + number);
            foreach (var cell in oldColors)
            {
                Color32 color; Require(newColors.TryGetValue(cell.Key, out color) && color.Equals(cell.Value),
                    "changed silhouette or Color32 at " + number + " cell " + cell.Key);
            }
            AssertPinnedArtwork(after, number);
        }

        private static Dictionary<Vector2Int, Color32> CellColors(PuzzleLevel level)
        {
            var colors = new Dictionary<Vector2Int, Color32>();
            foreach (var arrow in level.Arrows) foreach (var cell in arrow.Points)
            { Require(!colors.ContainsKey(cell), "overlapping silhouette cell"); colors.Add(cell, (Color32)arrow.Color); }
            return colors;
        }

        private static void CheckLevel(ShapesDifficultyResult old, ShapesDifficultyResult current, int level, List<string> failures)
        {
            if (current.InitialLegalRatio > Mathf.Min(.36f, old.InitialLegalRatio + .05f)) failures.Add(level + ": too many initial choices");
            if (current.MidChoiceArea > Mathf.Max(.32f, old.MidChoiceArea + .03f)) failures.Add(level + ": middle game too open");
            if (current.InwardRatio < .22f) failures.Add(level + ": insufficient inward heads");
            if (current.CellWeightedBends < 3.5f) failures.Add(level + ": area-weighted paths too straight");
        }

        private static void CheckCampaign(ShapesDifficultyResult[] old, ShapesDifficultyResult[] current,
            List<string> failures, StringBuilder report)
        {
            float initialOld = Mean(old, x => x.InitialLegalRatio), initialNew = Mean(current, x => x.InitialLegalRatio);
            float choiceOld = Mean(old, x => x.MidChoiceArea), choiceNew = Mean(current, x => x.MidChoiceArea);
            float inwardOld = Mean(old, x => x.InwardRatio), inwardNew = Mean(current, x => x.InwardRatio);
            float bendsOld = Mean(old, x => x.CellWeightedBends), bendsNew = Mean(current, x => x.CellWeightedBends);
            float spanOld = Mean(old, x => x.MaximumStraightSpan), spanNew = Mean(current, x => x.MaximumStraightSpan);
            if (initialNew > initialOld * .75f) failures.Add("campaign initial choices did not fall by 25%");
            if (choiceNew > choiceOld * .8f) failures.Add("campaign middle choices did not fall by 20%");
            if (inwardNew < inwardOld + .15f) failures.Add("campaign inward heads did not increase by 15 percentage points");
            if (bendsNew < bendsOld + .8f) failures.Add("campaign area-weighted bends did not increase by .8");
            if (spanNew > spanOld * .8f) failures.Add("campaign longest straight runs did not fall by 20%");
            report.AppendLine("Means: initial=" + F(initialOld) + "->" + F(initialNew) + ", midChoices=" + F(choiceOld) + "->" + F(choiceNew) +
                ", inward=" + F(inwardOld) + "->" + F(inwardNew) + ", weightedBends=" + F(bendsOld) + "->" + F(bendsNew) +
                ", straightSpan=" + F(spanOld) + "->" + F(spanNew));
        }

        private static float Mean(ShapesDifficultyResult[] values, Func<ShapesDifficultyResult, float> select)
        { float sum = 0; foreach (var item in values) sum += select(item); return sum / values.Length; }
        private static string Describe(ShapesDifficultyResult value) => "legal=" + F(value.InitialLegalRatio) + ", mid=" + F(value.MidChoiceArea) +
            ", inward=" + F(value.InwardRatio) + ", weightedBends=" + F(value.CellWeightedBends) + ", layers=" + value.DependencyLayers;
        private static string F(float value) => value.ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
        private static void Require(bool condition, string message)
        { if (!condition) throw new InvalidOperationException("Shapes balance verification: " + message); }
    }
}
