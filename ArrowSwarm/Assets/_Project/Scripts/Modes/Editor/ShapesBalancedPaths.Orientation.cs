namespace ArrowSwarm.Modes.Editor
{
    using System.Collections.Generic;
    using UnityEngine;

    public static partial class ShapesBalancedPaths
    {
        private static void Orient(List<PuzzleArrowData> arrows, int width, int height, System.Random random, int levelIndex)
        {
            int cells = 0; float weighted = 0f;
            foreach (var arrow in arrows) { cells += arrow.Points.Count; weighted += Bends(arrow.Points) * arrow.Points.Count; }
            var graph = DenseDependencyGraph.Analyze(arrows, width, height);
            float score = ShapesBalancedScoring.Fast(graph, arrows.Count, weighted / cells, levelIndex);
            for (int pass = 0; pass < 4; pass++)
                for (int n = 0, offset = random.Next(arrows.Count); n < arrows.Count; n++)
                {
                    int index = (n + offset) % arrows.Count; var original = arrows[index];
                    var best = original; float bestScore = score, bestWeighted = weighted;
                    var variants = Variants(original);
                    for (int v = 0, shift = random.Next(variants.Count); v < variants.Count; v++)
                    {
                        var points = variants[(v + shift) % variants.Count];
                        if (!OwnRayClear(points, width, height)) continue;
                        float candidateWeighted = weighted + (Bends(points) - Bends(original.Points)) * points.Count;
                        arrows[index] = new PuzzleArrowData(points, points[0] - points[1], original.Color);
                        var candidate = DenseDependencyGraph.Analyze(arrows, width, height);
                        float candidateScore = ShapesBalancedScoring.Fast(candidate, arrows.Count, candidateWeighted / cells, levelIndex);
                        if (candidateScore > bestScore + .001f)
                        { best = arrows[index]; bestScore = candidateScore; bestWeighted = candidateWeighted; }
                    }
                    arrows[index] = best; score = bestScore; weighted = bestWeighted;
                }
        }
    }
}
