namespace ArrowSwarm.Modes.Editor
{
    using System.Globalization;
    using System.Reflection;
    using ArrowSwarm.Puzzle;
    using UnityEngine;

    /// <summary>Measures the straight neck actually exposed behind the triangle, excluding covered stroke.</summary>
    public static partial class ShapesNeckVerification
    {
        private static readonly FieldInfo StrokePoints = typeof(PuzzleArrowView).GetField("_strokePoints",
            BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo StrokeDistances = typeof(PuzzleArrowView).GetField("_strokeDistances",
            BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo StrokeCount = typeof(PuzzleArrowView).GetField("_strokeCount",
            BindingFlags.Instance | BindingFlags.NonPublic);
        private static float _minimumNormalStem, _minimumHintStem;

        private static void CheckVisibleStem(PuzzleArrowView view, PuzzleArrowData data, SpriteRenderer head,
            Vector3 logicalHead, Vector3 direction, bool hint)
        {
            int straightRun = 0;
            for (int i = 1; i < data.Points.Count; i++)
            {
                if (data.Points[i - 1] - data.Points[i] != data.Direction) break;
                straightRun++;
            }
            float start = view.Length - straightRun;
            var points = (Vector3[])StrokePoints.GetValue(view);
            var distances = (float[])StrokeDistances.GetValue(view);
            int count = (int)StrokeCount.GetValue(view);
            Vector3 firstAxisPoint = Vector3.zero; bool found = false;
            for (int i = 0; i < count; i++)
            {
                if (distances[i] < start - .0001f) continue;
                Vector3 fromHead = points[i] - logicalHead; fromHead.z = 0f;
                Vector3 perpendicular = fromHead - direction * Vector3.Dot(fromHead, direction);
                if (perpendicular.sqrMagnitude > .00000001f) continue;
                firstAxisPoint = points[i]; found = true; break;
            }
            Check(found, "rounded final corner has a tangent on the head axis");
            if (!found) return;
            Bounds bounds = head.sprite.bounds;
            Vector3 triangleBase = head.transform.TransformPoint(new Vector3(bounds.center.x, bounds.min.y, 0f));
            triangleBase.z = firstAxisPoint.z = 0f;
            float exposed = Vector3.Dot(triangleBase - firstAxisPoint, direction);
            Check(exposed > .0001f, "straight shaft is visibly exposed behind the triangle base in " + (hint ? "hint" : "normal") + " state");
            if (hint) _minimumHintStem = Mathf.Min(_minimumHintStem, exposed);
            else _minimumNormalStem = Mathf.Min(_minimumNormalStem, exposed);
        }

        private static string StemLength(float length) => length.ToString("F6", CultureInfo.InvariantCulture);
    }
}
