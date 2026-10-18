namespace ArrowSwarm.Modes.Editor
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;

    /// <summary>Exports reproducible balance measurements and independently checks small known dependency fixtures.</summary>
    public static partial class ShapesBalanceVerification
    {
        private static void WriteCsv(string path, PuzzleLevel[] levels, ShapesDifficultyResult[] metrics)
        {
            var text = new StringBuilder("level,title,width,height,arrows,cells,initial_legal,initial_legal_ratio,dependency_layers,layer_widths,choice_area,early_choice_area,mid_choice_area,inward_ratio,cross_region_ratio,cross_color_ratio,average_bends,cell_weighted_bends,max_bends,max_straight_span,average_straight_max,short_arrow_ratio\n");
            for (int i = 0; i < levels.Length; i++)
            {
                var m = metrics[i]; var widths = new string[m.LayerWidths.Length];
                for (int j = 0; j < widths.Length; j++) widths[j] = m.LayerWidths[j].ToString();
                text.Append(i + 1).Append(",\"").Append(levels[i].Title.Replace("\"", "\"\"")).Append("\",")
                    .Append(levels[i].Width).Append(',').Append(levels[i].Height).Append(',').Append(m.ArrowCount).Append(',')
                    .Append(m.CoveredCells).Append(',').Append(m.InitialLegal).Append(',').Append(F(m.InitialLegalRatio)).Append(',')
                    .Append(m.DependencyLayers).Append(',').Append(string.Join(";", widths)).Append(',').Append(F(m.ChoiceArea)).Append(',')
                    .Append(F(m.EarlyChoiceArea)).Append(',').Append(F(m.MidChoiceArea)).Append(',').Append(F(m.InwardRatio)).Append(',')
                    .Append(F(m.CrossRegionRatio)).Append(',').Append(F(m.CrossColorRatio)).Append(',').Append(F(m.AverageBends)).Append(',')
                    .Append(F(m.CellWeightedBends)).Append(',').Append(m.MaximumBends).Append(',').Append(m.MaximumStraightSpan).Append(',')
                    .Append(F(m.AverageStraightMaximum)).Append(',').Append(F(m.ShortArrowRatio)).Append('\n');
            }
            File.WriteAllText(path, text.ToString());
        }

        private static void VerifyMetricFixtures()
        {
            var a = new PuzzleArrowData(new List<Vector2Int> { new Vector2Int(1, 1), new Vector2Int(0, 1) }, Vector2Int.right, Color.red);
            var b = new PuzzleArrowData(new List<Vector2Int> { new Vector2Int(3, 1), new Vector2Int(3, 0) }, Vector2Int.up, Color.green);
            var chain = ShapesDifficultyMetrics.Analyze(new[] { a, b }, 4, 3);
            Require(chain.Solvable && chain.InitialLegal == 1 && chain.DependencyLayers == 2, "two-arrow chain fixture");
            Require(chain.LayerWidths[0] == 1 && chain.LayerWidths[1] == 1 && Mathf.Abs(chain.ChoiceArea - .75f) < .0001f,
                "known frontier widths and normalized choice area");
            Require(chain.CellWeightedBends == 0 && chain.MaximumStraightSpan == 1 && chain.CrossColorRatio == 1,
                "known straight paths and cross-color blocker fixture");
            var c = new PuzzleArrowData(new List<Vector2Int> { new Vector2Int(1, 0), new Vector2Int(0, 0) }, Vector2Int.right, Color.red);
            var d = new PuzzleArrowData(new List<Vector2Int> { new Vector2Int(2, 0), new Vector2Int(3, 0), new Vector2Int(3, 1) }, Vector2Int.left, Color.blue);
            Require(!ShapesDifficultyMetrics.Analyze(new[] { c, d }, 4, 2).Solvable, "mutual-lock fixture must fail");
        }
    }
}
