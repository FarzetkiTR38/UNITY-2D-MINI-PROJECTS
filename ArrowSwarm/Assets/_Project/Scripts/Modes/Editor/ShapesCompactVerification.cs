namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Audits Shapes assets and compares the compact campaign against the published silhouettes.</summary>
    public static class ShapesCompactVerification
    {
        private const int MaskResolution = 48;
        private const int MaxSide = 34;
        private const int MinArrows = 40;
        private const int MaxArrows = 50;

        /// <summary>Records the original identity, occupancy and difficulty for each Shapes level.</summary>
        public static string WriteCatalogBaseline(string path)
        {
            var rows = MeasureCatalog();
            WriteJson(path, rows);
            return $"PASS: measured the original {rows.Count} Shapes levels.";
        }

        /// <summary>Verifies the saved compact catalog and writes per-level comparison and any failures.</summary>
        public static string VerifyCatalog(string baselinePath, string reportPath)
        {
            var baseline = JArray.Parse(File.ReadAllText(baselinePath));
            var revised = MeasureCatalog();
            if (baseline.Count != revised.Count)
                throw new InvalidOperationException("The saved Shapes campaign count differs from the baseline.");

            var failures = new JArray();
            for (int index = 0; index < revised.Count; index++)
            {
                var before = (JObject)baseline[index];
                var after = (JObject)revised[index];
                CompareLevel(before, after, failures);
            }

            var report = new JObject
            {
                ["status"] = failures.Count == 0 ? "PASS" : "FAIL",
                ["mask_resolution"] = MaskResolution,
                ["levels"] = revised,
                ["failures"] = failures
            };
            WriteJson(reportPath, report);
            if (failures.Count > 0)
                throw new InvalidOperationException("Compact Shapes audit: " + string.Join(", ", failures));

            return $"PASS: {revised.Count} original Shapes identities, compact boards and verified solutions.";
        }

        private static JArray MeasureCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<PuzzleCatalog>(ModeAuthoringUtility.Data + "ShapesCatalog.asset");
            if (catalog == null || catalog.Count != DenseShapeLibrary.Count)
                throw new InvalidOperationException("Expected the existing fifty-level Shapes catalog.");

            var rows = new JArray();
            var signatures = new HashSet<string>();
            for (int number = 1; number <= catalog.Count; number++)
            {
                string assetPath = ModeAuthoringUtility.Data + "Shapes_" + number + ".asset";
                var level = catalog.Get(number);
                if (level == null || level != AssetDatabase.LoadAssetAtPath<PuzzleLevel>(assetPath) ||
                    level.Mode != GameModeId.Shapes || level.Title != DenseShapeLibrary.Name(number - 1))
                    throw new InvalidOperationException("Unexpected Shapes asset at position " + number);

                string geometryError = PuzzleRules.Validate(level);
                if (geometryError != null)
                    throw new InvalidOperationException(level.Title + ": " + geometryError);
                if (!signatures.Add(DenseCampaignVerification.GeometrySignature(level)))
                    throw new InvalidOperationException(level.Title + ": repeated geometry.");

                var mask = new bool[level.Width, level.Height];
                if (level.CoverageMask == null || level.CoverageMask.Count == 0)
                    throw new InvalidOperationException(level.Title + ": no authored silhouette.");
                foreach (var cell in level.CoverageMask)
                {
                    if (!PuzzleRules.Inside(level, cell) || mask[cell.x, cell.y])
                        throw new InvalidOperationException(level.Title + ": invalid authored silhouette cell.");
                    mask[cell.x, cell.y] = true;
                }
                DenseCampaignVerification.AssertCoverage(level.Arrows, level.Width, level.Height, mask);
                rows.Add(MeasureLevel(level, number, assetPath, mask));
            }
            return rows;
        }

        private static JObject MeasureLevel(PuzzleLevel level, int number, string assetPath, bool[,] mask)
        {
            var solution = PuzzleSolver.Solve(level);
            var difficulty = ShapesDifficultyMetrics.Analyze(level.Arrows, level.Width, level.Height);
            if (!solution.Solvable || solution.Steps.Count != level.Arrows.Count || !difficulty.Solvable)
                throw new InvalidOperationException(level.Title + ": no complete, normal-arrow solution.");

            var colors = new HashSet<Color32>();
            foreach (var arrow in level.Arrows) colors.Add(arrow.Color);
            return new JObject
            {
                ["level"] = number,
                ["title"] = level.Title,
                ["asset_guid"] = AssetDatabase.AssetPathToGUID(assetPath),
                ["width"] = level.Width,
                ["height"] = level.Height,
                ["arrows"] = level.Arrows.Count,
                ["covered_cells"] = level.CoverageMask.Count,
                ["colors"] = colors.Count,
                ["directions"] = difficulty.DirectionCount,
                ["initial_legal"] = difficulty.InitialLegal,
                ["initial_legal_ratio"] = difficulty.InitialLegalRatio,
                ["inward_ratio"] = difficulty.InwardRatio,
                ["dependency_layers"] = difficulty.DependencyLayers,
                ["mid_choice_area"] = difficulty.MidChoiceArea,
                ["cell_weighted_bends"] = difficulty.CellWeightedBends,
                ["normalized_mask"] = NormalizeMask(mask, level.CoverageMask)
            };
        }

        private static void CompareLevel(JObject before, JObject after, JArray failures)
        {
            int side = MaxGridSide(after);
            int arrows = (int)after["arrows"];
            float intersection = MaskOverlap((string)before["normalized_mask"], (string)after["normalized_mask"]);
            after["previous_width"] = (int)before["width"];
            after["previous_height"] = (int)before["height"];
            after["previous_arrows"] = (int)before["arrows"];
            after["previous_colors"] = (int)before["colors"];
            after["normalized_silhouette_iou"] = intersection;
            after.Remove("normalized_mask");
            string label = $"{after["level"]} {after["title"]}";
            if ((int)before["level"] != (int)after["level"] ||
                (string)before["title"] != (string)after["title"] || (string)before["asset_guid"] != (string)after["asset_guid"])
                failures.Add(label + ": changed catalog identity.");
            if (side > MaxSide || side >= MaxGridSide(before))
                failures.Add(label + $": grid long side is {side}.");
            if (arrows < MinArrows || arrows > MaxArrows)
                failures.Add(label + $": {arrows} arrows.");
            if ((int)after["directions"] != 4 || (int)after["dependency_layers"] < 3 ||
                (float)after["initial_legal_ratio"] > .40f || (float)after["inward_ratio"] < .20f)
                failures.Add(label + ": puzzle has weak directional dependencies.");
            if (intersection < .70f)
                failures.Add(label + $": silhouette overlap {intersection:P0}.");
        }

        private static int MaxGridSide(JObject row) => Math.Max((int)row["width"], (int)row["height"]);

        private static string NormalizeMask(bool[,] mask, IReadOnlyList<Vector2Int> cells)
        {
            var min = new Vector2Int(mask.GetLength(0), mask.GetLength(1));
            var max = Vector2Int.zero;
            foreach (var cell in cells)
            {
                min = Vector2Int.Min(min, cell);
                max = Vector2Int.Max(max, cell);
            }
            var bits = new char[MaskResolution * MaskResolution];
            var size = max - min + Vector2Int.one;
            for (int y = 0; y < MaskResolution; y++)
            {
                int gridY = min.y + ((2 * y + 1) * size.y) / (2 * MaskResolution);
                for (int x = 0; x < MaskResolution; x++)
                {
                    int gridX = min.x + ((2 * x + 1) * size.x) / (2 * MaskResolution);
                    bits[y * MaskResolution + x] = mask[gridX, gridY] ? '1' : '0';
                }
            }
            return new string(bits);
        }

        private static float MaskOverlap(string before, string after)
        {
            if (before == null || after == null || before.Length != MaskResolution * MaskResolution || before.Length != after.Length)
                throw new InvalidOperationException("The Shapes baseline mask has the wrong resolution.");
            int intersection = 0, union = 0;
            for (int i = 0; i < before.Length; i++)
            {
                if (before[i] == '1' && after[i] == '1') intersection++;
                if (before[i] == '1' || after[i] == '1') union++;
            }
            return union > 0 ? (float)intersection / union : 0;
        }

        private static void WriteJson(string path, JToken data)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            File.WriteAllText(path, data.ToString() + Environment.NewLine);
        }
    }
}
