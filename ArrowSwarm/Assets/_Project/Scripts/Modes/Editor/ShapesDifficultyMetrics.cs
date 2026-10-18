namespace ArrowSwarm.Modes.Editor
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>Human-facing puzzle structure measurements, independent of a level's title or palette brightness.</summary>
    public sealed class ShapesDifficultyResult
    {
        public bool Solvable { get; internal set; }
        public int ArrowCount { get; internal set; }
        public int CoveredCells { get; internal set; }
        public int InitialLegal { get; internal set; }
        public float InitialLegalRatio => ArrowCount == 0 ? 0 : (float)InitialLegal / ArrowCount;
        public float BlockedRatio => 1f - InitialLegalRatio;
        public int[] LayerWidths { get; internal set; }
        public float[] LayerFrontierRatios { get; internal set; }
        public int DependencyLayers => LayerWidths == null ? 0 : LayerWidths.Length;
        public float ChoiceArea { get; internal set; }
        public float EarlyChoiceArea { get; internal set; }
        public float MidChoiceArea { get; internal set; }
        public float InwardRatio { get; internal set; }
        public float CrossRegionRatio { get; internal set; }
        public float CrossColorRatio { get; internal set; }
        public float AverageBends { get; internal set; }
        public float CellWeightedBends { get; internal set; }
        public int MaximumBends { get; internal set; }
        public int MaximumStraightSpan { get; internal set; }
        public float AverageStraightMaximum { get; internal set; }
        public float ShortArrowRatio { get; internal set; }
        public int DirectionCount { get; internal set; }
        internal List<int>[] Blockers { get; set; }
    }

    /// <summary>Measures actual head-ray choices and path complexity rather than relying on longest-chain depth.</summary>
    public static partial class ShapesDifficultyMetrics
    {
        /// <summary>Analyzes an exact Shapes layout; cyclic, overlapping or self-blocking data remains unsolvable.</summary>
        public static ShapesDifficultyResult Analyze(IReadOnlyList<PuzzleArrowData> arrows, int width, int height)
        {
            var result = new ShapesDifficultyResult { ArrowCount = arrows.Count };
            var graph = DenseDependencyGraph.Analyze(arrows, width, height);
            if (!graph.Solvable || arrows.Count == 0) return result;
            var owner = new int[width, height];
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) owner[x, y] = -1;
            for (int i = 0; i < arrows.Count; i++) foreach (var p in arrows[i].Points) owner[p.x, p.y] = i;
            result.Blockers = new List<int>[arrows.Count];
            var regions = new int[arrows.Count];
            int inward = 0, bendsTotal = 0, weightedBends = 0, spanTotal = 0, shortCount = 0;
            var center = new Vector2((width - 1) * .5f, (height - 1) * .5f);
            for (int i = 0; i < arrows.Count; i++)
            {
                var arrow = arrows[i]; var blockers = result.Blockers[i] = new List<int>();
                for (var p = arrow.Head + arrow.Direction; p.x >= 0 && p.y >= 0 && p.x < width && p.y < height; p += arrow.Direction)
                {
                    int other = owner[p.x, p.y];
                    if (other >= 0 && !blockers.Contains(other)) blockers.Add(other);
                }
                if (blockers.Count == 0) result.InitialLegal++;
                if (Vector2.Dot((Vector2)arrow.Head - center, arrow.Direction) < 0) inward++;
                int bends, span; PathComplexity(arrow, out bends, out span);
                bendsTotal += bends; weightedBends += bends * arrow.Points.Count; spanTotal += span;
                result.MaximumBends = Mathf.Max(result.MaximumBends, bends);
                result.MaximumStraightSpan = Mathf.Max(result.MaximumStraightSpan, span);
                result.CoveredCells += arrow.Points.Count; if (arrow.Points.Count <= 4) shortCount++;
                var centroid = Vector2.zero; foreach (var p in arrow.Points) centroid += p;
                centroid /= arrow.Points.Count;
                regions[i] = (centroid.x >= width * .5f ? 1 : 0) + (centroid.y >= height * .5f ? 2 : 0);
            }
            int edges = 0, crossRegion = 0, crossColor = 0;
            for (int i = 0; i < arrows.Count; i++) foreach (int other in result.Blockers[i])
            {
                edges++; if (regions[i] != regions[other]) crossRegion++;
                if (!((Color32)arrows[i].Color).Equals((Color32)arrows[other].Color)) crossColor++;
            }
            result.InwardRatio = (float)inward / arrows.Count;
            result.AverageBends = (float)bendsTotal / arrows.Count;
            result.CellWeightedBends = (float)weightedBends / result.CoveredCells;
            result.AverageStraightMaximum = (float)spanTotal / arrows.Count;
            result.ShortArrowRatio = (float)shortCount / arrows.Count;
            result.CrossRegionRatio = edges == 0 ? 0 : (float)crossRegion / edges;
            result.CrossColorRatio = edges == 0 ? 0 : (float)crossColor / edges;
            result.DirectionCount = graph.DirectionCount;
            MeasureChoices(result); result.Solvable = true; return result;
        }

        private static void PathComplexity(PuzzleArrowData arrow, out int bends, out int maximumSpan)
        {
            bends = 0; maximumSpan = 0; int span = 0; var previous = Vector2Int.zero;
            for (int i = 1; i < arrow.Points.Count; i++)
            {
                var direction = arrow.Points[i] - arrow.Points[i - 1];
                if (direction != previous) { if (i > 1) bends++; span = 0; }
                maximumSpan = Mathf.Max(maximumSpan, ++span); previous = direction;
            }
        }
    }
}
