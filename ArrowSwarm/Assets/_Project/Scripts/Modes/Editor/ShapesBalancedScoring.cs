namespace ArrowSwarm.Modes.Editor
{
    using UnityEngine;

    /// <summary>Difficulty objectives combine available choices, inward dependencies and useful path bends.</summary>
    public static class ShapesBalancedScoring
    {
        /// <summary>Cheap mutation objective; full frontier simulation is reserved for complete candidates.</summary>
        public static float Fast(DenseDependencyMetrics graph, int count, float weightedBends, int index)
        {
            if (!graph.Solvable) return float.NegativeInfinity;
            float blocked = graph.BlockedArrows / (float)count, inward = graph.InwardHeads / (float)count;
            float progress = Mathf.Clamp01(index / 49f);
            return -Mathf.Abs(blocked - Mathf.Lerp(.73f, .80f, progress)) * 180f -
                Mathf.Abs(inward - Mathf.Lerp(.33f, .43f, progress)) * 160f +
                Mathf.Min(weightedBends, 6f) * 6f + Mathf.Min(graph.MaximumDepth, 14) * .3f + graph.DirectionCount * 12f;
        }

        /// <summary>Scores opening and sustained midgame choice ratios so one long chain cannot hide a free board.</summary>
        public static float Final(ShapesDifficultyResult metrics, int index)
        {
            float progress = Mathf.Clamp01(index / 49f);
            float countPenalty = Mathf.Max(0f, metrics.ArrowCount - 110) * 1.5f + Mathf.Max(0f, 45 - metrics.ArrowCount) * .2f;
            return -Mathf.Abs(metrics.InitialLegalRatio - Mathf.Lerp(.27f, .20f, progress)) * 150f -
                Mathf.Abs(metrics.MidChoiceArea - Mathf.Lerp(.25f, .15f, progress)) * 240f -
                Mathf.Abs(metrics.EarlyChoiceArea - Mathf.Lerp(.27f, .18f, progress)) * 100f -
                Mathf.Abs(metrics.InwardRatio - Mathf.Lerp(.33f, .43f, progress)) * 120f - Mathf.Abs(metrics.CellWeightedBends - 5.8f) * 5f -
                Mathf.Max(0f, metrics.MaximumStraightSpan - 16) * .4f - countPenalty + metrics.CrossColorRatio * 10f;
        }
    }
}
