namespace ArrowSwarm.Modes.Editor
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>Measures simultaneous removal waves and legal choices across a deterministic greedy solution.</summary>
    public static partial class ShapesDifficultyMetrics
    {
        private static void MeasureChoices(ShapesDifficultyResult result)
        {
            int count = result.ArrowCount;
            var dependents = new List<int>[count]; var degrees = new int[count];
            for (int i = 0; i < count; i++) dependents[i] = new List<int>();
            for (int i = 0; i < count; i++)
            {
                degrees[i] = result.Blockers[i].Count;
                foreach (int other in result.Blockers[i]) dependents[other].Add(i);
            }
            MeasureWaves(result, degrees, dependents);
            var removed = new bool[count];
            int earlyEnd = Mathf.Max(1, Mathf.FloorToInt(count * .8f));
            int middleStart = Mathf.FloorToInt(count * .2f), middleEnd = Mathf.Max(middleStart + 1, earlyEnd);
            float all = 0, early = 0, middle = 0;
            for (int step = 0; step < count; step++)
            {
                int legal = 0, selected = -1;
                for (int i = 0; i < count; i++) if (!removed[i] && degrees[i] == 0)
                { legal++; if (selected < 0) selected = i; }
                float choice = (float)legal / (count - step); all += choice;
                if (step < earlyEnd) early += choice;
                if (step >= middleStart && step < middleEnd) middle += choice;
                removed[selected] = true;
                foreach (int dependent in dependents[selected]) degrees[dependent]--;
            }
            result.ChoiceArea = all / count; result.EarlyChoiceArea = early / earlyEnd;
            result.MidChoiceArea = middle / (middleEnd - middleStart);
        }

        private static void MeasureWaves(ShapesDifficultyResult result, int[] degrees, List<int>[] dependents)
        {
            var remainingDegrees = (int[])degrees.Clone(); var removed = new bool[degrees.Length];
            var widths = new List<int>(); var ratios = new List<float>(); var frontier = new List<int>();
            int remaining = degrees.Length;
            while (remaining > 0)
            {
                frontier.Clear();
                for (int i = 0; i < degrees.Length; i++) if (!removed[i] && remainingDegrees[i] == 0) frontier.Add(i);
                widths.Add(frontier.Count); ratios.Add((float)frontier.Count / remaining); remaining -= frontier.Count;
                foreach (int index in frontier)
                {
                    removed[index] = true;
                    foreach (int dependent in dependents[index]) remainingDegrees[dependent]--;
                }
            }
            result.LayerWidths = widths.ToArray(); result.LayerFrontierRatios = ratios.ToArray();
        }
    }
}
