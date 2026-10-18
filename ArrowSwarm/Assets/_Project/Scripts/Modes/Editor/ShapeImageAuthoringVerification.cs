namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>In-memory RGBA import and persistent original-mask regressions; never touches scene or asset files.</summary>
    public static class ShapeImageAuthoringVerification
    {
        /// <summary>Checks transparent white foreground, opaque background removal, edited holes and serialized mask retention.</summary>
        [MenuItem("Arrow Swarm/Puzzles/Verify Image Mask and Dense Editing")]
        public static void Run()
        {
            var transparent = new[] { new Color32(0, 0, 0, 0), new Color32(255, 255, 255, 255) };
            var alpha = ShapeImageMask.ResolveMode(ShapeImageMaskMode.Auto, transparent);
            Require(alpha == ShapeImageMaskMode.AlphaOnly, "transparent PNG pixels select alpha-only mode");
            Require(ShapeImageMask.Includes(Color.white, alpha, Color.white, .15f, .2f), "opaque white foreground survives");
            Require(!ShapeImageMask.Includes(Color.clear, alpha, Color.white, .15f, .2f), "transparent background is removed");
            var opaque = new[] { new Color32(255, 255, 255, 255), new Color32(255, 0, 0, 255) };
            var keyed = ShapeImageMask.ResolveMode(ShapeImageMaskMode.Auto, opaque);
            Require(keyed == ShapeImageMaskMode.ColorKey &&
                !ShapeImageMask.Includes(Color.white, keyed, Color.white, .15f, .2f) &&
                ShapeImageMask.Includes(Color.red, keyed, Color.white, .15f, .2f), "opaque image removes only the chosen background color");
            Require(ShapeImageMask.ResolveMode(ShapeImageMaskMode.ColorKey, transparent) == ShapeImageMaskMode.ColorKey &&
                ShapeImageMask.ResolveMode(ShapeImageMaskMode.AlphaOnly, opaque) == ShapeImageMaskMode.AlphaOnly, "explicit modes override detection");
            var ring = new bool[8, 8];
            for (int y = 2; y < 6; y++) for (int x = 2; x < 6; x++) ring[x, y] = x == 2 || x == 5 || y == 2 || y == 5;
            ShapeImageMask.FillEnclosedHoles(ring);
            Require(ring[3, 3] && ring[4, 4] && !ring[0, 0] && !ring[7, 7], "interior white/transparent gaps fill without changing the outline");
            ring[3, 2] = ring[4, 2] = ring[3, 3] = ring[4, 3] = false;
            ShapeImageMask.FillEnclosedHoles(ring);
            Require(!ring[3, 3], "open exterior notches remain outside the shape");
            var level = ScriptableObject.CreateInstance<PuzzleLevel>();
            var restored = ScriptableObject.CreateInstance<PuzzleLevel>();
            var legacy = ScriptableObject.CreateInstance<PuzzleLevel>();
            try
            {
                var points = new List<Vector2Int> { new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(0, 0), new Vector2Int(0, 1) };
                level.Author(GameModeId.Shapes, "White foreground", 2, 2,
                    new List<PuzzleArrowData> { new PuzzleArrowData(points, Vector2Int.up, Color.white) }, null, 0);
                Require(level.CoverageMask.Count == 4 && ShapeImageMask.CoverageError(level) == null, "author captures the complete original mask");
                level.Arrows[0].Points.RemoveAt(3);
                Require(ShapeImageMask.CoverageError(level)?.Contains("uncovered") == true &&
                    !level.CaptureCoverageIfMissing() && level.CoverageMask.Count == 4, "manual deletion cannot redefine the original mask");
                EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(level), restored);
                Require(restored.CoverageMask.Count == 4 && ShapeImageMask.CoverageError(restored)?.Contains("uncovered") == true,
                    "asset serialization retains the original mask despite edited holes");
                restored.Arrows[0].Points.Add(new Vector2Int(0, 1));
                Require(ShapeImageMask.CoverageError(restored) == null, "restoring exact coverage permits further editing");
                restored.Arrows.Add(new PuzzleArrowData(new List<Vector2Int> { new Vector2Int(0, 1), new Vector2Int(0, 0) }, Vector2Int.up, Color.red));
                Require(ShapeImageMask.CoverageError(restored)?.Contains("overlap") == true, "overlapping edits are rejected");
                EditorJsonUtility.FromJsonOverwrite("{\"_coverageMask\":null}", legacy);
                legacy.Arrows.Add(new PuzzleArrowData(new List<Vector2Int>(points), Vector2Int.up, Color.cyan));
                Require(legacy.CaptureCoverageIfMissing() && !legacy.CaptureCoverageIfMissing(), "legacy baseline is captured exactly once");
                level.Author(GameModeId.ColorShift, "Color mask", 2, 2, restored.Arrows);
                Require(level.CoverageMask.Count == 4, "Color Shift Author also retains the original cell union");
            }
            finally { UnityEngine.Object.DestroyImmediate(level); UnityEngine.Object.DestroyImmediate(restored); UnityEngine.Object.DestroyImmediate(legacy); }
            Debug.Log("[ArrowSwarm] PASS: automatic/explicit image masks, white PNG foreground, opaque color key, original coverage, " +
                "holes, overlaps, serialization and legacy one-time capture. In-memory objects only.");
        }

        private static void Require(bool condition, string message)
        { if (!condition) throw new InvalidOperationException("Image authoring verification: " + message); }
    }
}
