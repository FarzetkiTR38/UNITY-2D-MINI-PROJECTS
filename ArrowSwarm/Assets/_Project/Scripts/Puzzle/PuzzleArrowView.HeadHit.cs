namespace ArrowSwarm.Puzzle
{
    using UnityEngine;

    /// <summary>Keeps the whole centered triangle selectable, including its enlarged hint tip.</summary>
    public sealed partial class PuzzleArrowView
    {
        private float HeadDistanceTo(Vector2 world)
        {
            Bounds bounds = _filledHead.sprite.bounds;
            Vector3 local = _filledHeadTransform.InverseTransformPoint(world);
            float height = Mathf.Max(.0001f, bounds.size.y);
            float progress = (local.y - bounds.min.y) / height;
            float halfWidth = bounds.extents.x * (1f - progress);
            if (progress >= 0f && progress <= 1f && Mathf.Abs(local.x - bounds.center.x) <= halfWidth) return 0f;
            Vector2 a = _filledHeadTransform.TransformPoint(new Vector3(bounds.center.x, bounds.max.y));
            Vector2 b = _filledHeadTransform.TransformPoint(new Vector3(bounds.min.x, bounds.min.y));
            Vector2 c = _filledHeadTransform.TransformPoint(new Vector3(bounds.max.x, bounds.min.y));
            return Mathf.Min(HeadEdgeDistance(world, a, b), Mathf.Min(HeadEdgeDistance(world, b, c), HeadEdgeDistance(world, c, a)));
        }

        private static float HeadEdgeDistance(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 edge = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(point - a, edge) / Mathf.Max(.00000001f, edge.sqrMagnitude));
            return Vector2.Distance(point, a + edge * t);
        }
    }
}
