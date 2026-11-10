namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>Resamples the original painted silhouettes onto a compact grid with complete 2x2 tiles.</summary>
    internal static class ShapesCompactLayout
    {
        private const int ProbeSteps = 192;
        private static readonly Color[][,] Layouts = new Color[DenseShapeLibrary.Count][,];

        internal static Color[,] Get(int index)
        {
            if (index < 0 || index >= DenseShapeLibrary.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return Layouts[index] ?? (Layouts[index] = Rasterize(index));
        }

        private static Color[,] Rasterize(int index)
        {
            var min = new Vector2(1, 1);
            var max = new Vector2(-1, -1);
            for (int y = 0; y <= ProbeSteps; y++)
            for (int x = 0; x <= ProbeSteps; x++)
            {
                var p = new Vector2(2f * x / ProbeSteps - 1f, 2f * y / ProbeSteps - 1f);
                if (DenseShapeLibrary.Sample(index, p.x, p.y).a <= .5f) continue;
                min = Vector2.Min(min, p);
                max = Vector2.Max(max, p);
            }
            var span = max - min;
            if (span.x <= 0 || span.y <= 0)
                throw new InvalidOperationException("Empty compact Shapes source " + (index + 1));

            var center = (min + max) * .5f;
            int longSide = GridLongSide(index);
            float pitch = Mathf.Max(span.x, span.y) / (longSide / 2);
            int width = Mathf.Min(longSide / 2, Mathf.CeilToInt(span.x / pitch));
            int height = Mathf.Min(longSide / 2, Mathf.CeilToInt(span.y / pitch));
            var tiles = new Color[width, height];
            int left = width, right = -1, bottom = height, top = -1;
            var inks = new Dictionary<Color32, int>();
            float verticalOffset = VerticalTileOffset(index);
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float u = center.x + (x + .5f - width * .5f) * pitch;
                float v = center.y + (y + .5f - height * .5f + verticalOffset) * pitch;
                var color = SampleTile(index, u, v, pitch, inks);
                if (color.a <= .5f) continue;
                tiles[x, y] = color;
                left = Mathf.Min(left, x);
                right = Mathf.Max(right, x);
                bottom = Mathf.Min(bottom, y);
                top = Mathf.Max(top, y);
            }
            if (right < left || top < bottom)
                throw new InvalidOperationException("Compact sampling lost Shapes level " + (index + 1));
            var trimmed = new Color[right - left + 1, top - bottom + 1];
            for (int y = bottom; y <= top; y++)
            for (int x = left; x <= right; x++)
                trimmed[x - left, y - bottom] = tiles[x, y];
            return trimmed;
        }

        private static Color SampleTile(int index, float u, float v, float pitch, Dictionary<Color32, int> inks)
        {
            // The center retains small colored details such as eyes. Edge tiles can fill in only
            // when most of the footprint lies within the original silhouette.
            var center = DenseShapeLibrary.Sample(index, u, v);
            if (center.a > .5f) return center;
            inks.Clear();
            int filled = 0, highest = 0;
            var majority = Color.clear;
            for (int y = -1; y <= 1; y++)
            for (int x = -1; x <= 1; x++)
            {
                if (x == 0 && y == 0) continue;
                var ink = DenseShapeLibrary.Sample(index, u + x * pitch / 3f, v + y * pitch / 3f);
                if (ink.a <= .5f) continue;
                filled++;
                inks.TryGetValue((Color32)ink, out int count);
                inks[(Color32)ink] = ++count;
                if (count <= highest) continue;
                highest = count;
                majority = ink;
            }
            return filled >= 5 ? majority : Color.clear;
        }

        // Fractional tile alignment keeps the slender sunflower stem and anchor crossbar continuous.
        private static float VerticalTileOffset(int index) => index == 38 ? -.1f : index == 42 ? .2f : 0f;

        private static int GridLongSide(int index)
        {
            // Narrow or detailed subjects need two extra tiles to retain their outlines and painted details.
            switch (index)
            {
                case 4:  // Starlight Rocket
                case 12: // Blue Whale
                case 17: // Coral Seahorse
                case 27: // Emerald Dragon
                case 28: // Coral Flamingo
                case 38: // Summer Sunflower
                case 42: // Ocean Anchor
                case 46: // Triple Scoop
                case 48: // Meadow Windmill
                    return 34;
                default:
                    return index < 15 ? 30 : 32;
            }
        }
    }
}
