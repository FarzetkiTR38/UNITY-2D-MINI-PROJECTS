namespace ArrowSwarm.Modes.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Freezes the published cell colors so geometry revisions cannot repaint an existing silhouette.</summary>
    public static class ShapesBalancedSource
    {
        /// <summary>Reads exact published tile colors, with the normalized reference retained for fresh projects.</summary>
        public static Color[,] Read(int index, Color[,] fallback, out bool published)
        {
            var level = AssetDatabase.LoadAssetAtPath<PuzzleLevel>(ModeAuthoringUtility.Data + "Shapes_" + (index + 1) + ".asset");
            published = level != null;
            if (!published) return fallback;
            if ((level.Width & 1) != 0 || (level.Height & 1) != 0) throw new InvalidOperationException("Published Shapes grid must be even.");
            var cells = new Color[level.Width, level.Height];
            foreach (var arrow in level.Arrows) foreach (var p in arrow.Points) cells[p.x, p.y] = arrow.Color;
            var tiles = new Color[level.Width / 2, level.Height / 2];
            for (int y = 0; y < level.Height; y += 2) for (int x = 0; x < level.Width; x += 2)
            {
                var ink = cells[x, y];
                if (!SameInk(ink, cells[x + 1, y]) || !SameInk(ink, cells[x, y + 1]) || !SameInk(ink, cells[x + 1, y + 1]))
                    throw new InvalidOperationException("Published color boundary splits a Shapes tile at " + x + "," + y);
                tiles[x / 2, y / 2] = ink;
            }
            return tiles;
        }

        /// <summary>Uses the exact displayed color key rather than a tolerance that can erase a feature.</summary>
        public static bool SameInk(Color a, Color b)
        {
            Color32 x = a, y = b;
            return x.r == y.r && x.g == y.g && x.b == y.b && x.a == y.a;
        }
    }
}
