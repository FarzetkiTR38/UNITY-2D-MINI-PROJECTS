namespace ArrowSwarm.Modes.Editor
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>Explicit clean-image masking policy, independent of any scene or saved player progress.</summary>
    public enum ShapeImageMaskMode { Auto, AlphaOnly, ColorKey }

    /// <summary>Pure pixel selection and original-cell coverage checks for the Shapes authoring window.</summary>
    public static partial class ShapeImageMask
    {
        /// <summary>Auto uses alpha when any pixel is translucent; fully opaque images use the selected background color.</summary>
        public static ShapeImageMaskMode ResolveMode(ShapeImageMaskMode mode, IReadOnlyList<Color32> pixels)
        {
            if (mode != ShapeImageMaskMode.Auto) return mode;
            if (pixels != null) foreach (var pixel in pixels) if (pixel.a < 255) return ShapeImageMaskMode.AlphaOnly;
            return ShapeImageMaskMode.ColorKey;
        }

        /// <summary>Selects opaque foreground pixels, preserving white artwork in an alpha-based image.</summary>
        public static bool Includes(Color pixel, ShapeImageMaskMode resolvedMode, Color background, float tolerance, float alphaThreshold)
        {
            if (pixel.a <= alphaThreshold) return false;
            if (resolvedMode == ShapeImageMaskMode.AlphaOnly) return true;
            float r = pixel.r - background.r, g = pixel.g - background.g, b = pixel.b - background.b;
            return r * r + g * g + b * b > tolerance * tolerance;
        }

        /// <summary>Rejects missing, overlapping or newly added cells relative to the retained original silhouette.</summary>
        public static string CoverageError(PuzzleLevel level)
        {
            if (level == null || level.CoverageMask == null || level.CoverageMask.Count == 0)
                return "Original silhouette mask is missing. Reopen the level before editing.";
            var expected = new HashSet<Vector2Int>(level.CoverageMask);
            var actual = new HashSet<Vector2Int>();
            if (level.Arrows == null) return "Arrow paths are missing.";
            foreach (var arrow in level.Arrows)
            {
                if (arrow?.Points == null) return "An arrow path is missing.";
                foreach (var cell in arrow.Points)
                {
                    if (!actual.Add(cell)) return "Arrow paths overlap at " + cell + ".";
                    if (!expected.Contains(cell)) return "Arrow path leaves the original silhouette at " + cell + ".";
                }
            }
            if (actual.Count != expected.Count)
                return "The original silhouette has " + (expected.Count - actual.Count) + " uncovered cell(s). Restore these paths before saving.";
            return null;
        }
    }
}
