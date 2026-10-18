namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using UnityEngine;

    /// <summary>Loads the immutable Revision3 measurements so rebuilding Revision4 remains idempotent.</summary>
    public static partial class ShapesBalanceVerification
    {
        private static string[] _baselineCellHashes;
        private static int[] _baselineWidths, _baselineHeights;

        private static ShapesDifficultyResult[] LoadBaseline()
        {
            string path = ReportFolder + "/balance-baseline.csv";
            Require(File.Exists(path), "missing committed Revision3 balance baseline: " + path);
            string[] lines = File.ReadAllLines(path); Require(lines.Length == 51, "expected fifty frozen baseline rows");
            string[] headers = CsvValues(lines[0]); var columns = new Dictionary<string, int>();
            for (int i = 0; i < headers.Length; i++) columns.Add(headers[i], i);
            var baseline = new ShapesDifficultyResult[50];
            _baselineCellHashes = new string[50]; _baselineWidths = new int[50]; _baselineHeights = new int[50];
            for (int row = 1; row < lines.Length; row++)
            {
                string[] values = CsvValues(lines[row]); int index = (int)Value(values, columns, "level") - 1;
                Require(index >= 0 && index < baseline.Length && baseline[index] == null, "invalid frozen baseline order");
                _baselineCellHashes[index] = values[columns["cell_color_sha256"]];
                _baselineWidths[index] = (int)Value(values, columns, "width");
                _baselineHeights[index] = (int)Value(values, columns, "height");
                string[] widths = values[columns["layer_widths"]].Split(';'); var layers = new int[widths.Length];
                for (int i = 0; i < widths.Length; i++) layers[i] = int.Parse(widths[i], CultureInfo.InvariantCulture);
                baseline[index] = new ShapesDifficultyResult
                {
                    Solvable = true, DirectionCount = 4, LayerWidths = layers,
                    ArrowCount = (int)Value(values, columns, "arrows"), CoveredCells = (int)Value(values, columns, "cells"),
                    InitialLegal = (int)Value(values, columns, "initial_legal"),
                    ChoiceArea = Value(values, columns, "choice_area"), EarlyChoiceArea = Value(values, columns, "early_choice_area"),
                    MidChoiceArea = Value(values, columns, "mid_choice_area"), InwardRatio = Value(values, columns, "inward_ratio"),
                    CrossRegionRatio = Value(values, columns, "cross_region_ratio"), CrossColorRatio = Value(values, columns, "cross_color_ratio"),
                    AverageBends = Value(values, columns, "average_bends"), CellWeightedBends = Value(values, columns, "cell_weighted_bends"),
                    MaximumBends = (int)Value(values, columns, "max_bends"), MaximumStraightSpan = (int)Value(values, columns, "max_straight_span"),
                    AverageStraightMaximum = Value(values, columns, "average_straight_max"), ShortArrowRatio = Value(values, columns, "short_arrow_ratio")
                };
            }
            return baseline;
        }

        private static void AssertPinnedArtwork(PuzzleLevel level, int number)
        {
            int index = number - 1;
            Require(level.Width == _baselineWidths[index] && level.Height == _baselineHeights[index],
                "dimensions differ from frozen Revision3 artwork at " + number);
            var colors = CellColors(level); var cells = new List<Vector2Int>(colors.Keys);
            cells.Sort((a, b) => a.x == b.x ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
            var canonical = new StringBuilder();
            foreach (var cell in cells)
            {
                if (canonical.Length > 0) canonical.Append(';'); var color = colors[cell];
                canonical.Append(cell.x).Append(',').Append(cell.y).Append(':').Append(color.r).Append(',')
                    .Append(color.g).Append(',').Append(color.b).Append(',').Append(color.a);
            }
            byte[] hash; using (var sha = SHA256.Create()) hash = sha.ComputeHash(Encoding.UTF8.GetBytes(canonical.ToString()));
            var hex = new StringBuilder(); foreach (byte item in hash) hex.Append(item.ToString("x2", CultureInfo.InvariantCulture));
            Require(hex.ToString() == _baselineCellHashes[index], "cell palette/silhouette differs from frozen Revision3 at " + number);
        }

        private static float Value(string[] values, Dictionary<string, int> columns, string name) =>
            float.Parse(values[columns[name]], CultureInfo.InvariantCulture);

        private static string[] CsvValues(string line)
        {
            var values = new List<string>(); var value = new StringBuilder(); bool quoted = false;
            for (int i = 0; i < line.Length; i++)
            {
                char item = line[i];
                if (item == '"')
                {
                    if (quoted && i + 1 < line.Length && line[i + 1] == '"') { value.Append('"'); i++; }
                    else quoted = !quoted;
                }
                else if (item == ',' && !quoted) { values.Add(value.ToString()); value.Length = 0; }
                else value.Append(item);
            }
            values.Add(value.ToString()); return values.ToArray();
        }
    }
}
