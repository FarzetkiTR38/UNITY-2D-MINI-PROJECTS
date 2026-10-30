namespace ArrowSwarm.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Draws a transparent crescent as a strip between intersecting circular arcs, using the inherited UI tint and masking material.</summary>
    [AddComponentMenu("UI/Arrow Swarm/Crescent")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform), typeof(CanvasRenderer))]
    public sealed class UICrescentGraphic : MaskableGraphic
    {
        [Header("Normalized Geometry")]
        [SerializeField, Range(.05f, .5f)] private float _outerRadius = .48f;
        [SerializeField, Range(.05f, .5f)] private float _innerRadius = .39f;
        [SerializeField] private Vector2 _innerOffset = new Vector2(.22f, .12f);
        [SerializeField, Range(8, 128)] private int _segments = 32;

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect bounds = rectTransform.rect;
            float size = Mathf.Min(bounds.width, bounds.height);
            float distance = _innerOffset.magnitude;
            // A crescent has two distinct intersections; invalid Inspector values never generate NaN vertices.
            if (size <= 0f || _outerRadius <= 0f || _innerRadius <= 0f ||
                distance <= Mathf.Abs(_outerRadius - _innerRadius) + .00001f ||
                distance >= _outerRadius + _innerRadius - .00001f) return;

            Vector2 axis = _innerOffset / distance;
            Vector2 perpendicular = new Vector2(-axis.y, axis.x);
            float along = (_outerRadius * _outerRadius - _innerRadius * _innerRadius + distance * distance) / (2f * distance);
            float height = Mathf.Sqrt(Mathf.Max(0f, _outerRadius * _outerRadius - along * along));
            Vector2 start = axis * along + perpendicular * height;
            Vector2 end = axis * along - perpendicular * height;
            float direction = Mathf.Atan2(axis.y, axis.x);
            float outerHalf = Mathf.Acos(Mathf.Clamp(along / _outerRadius, -1f, 1f));
            float innerHalf = Mathf.Acos(Mathf.Clamp((along - distance) / _innerRadius, -1f, 1f));
            float outerStart = direction + outerHalf;
            float innerStart = direction + innerHalf;
            float outerSweep = 2f * Mathf.PI - 2f * outerHalf;
            float innerSweep = 2f * Mathf.PI - 2f * innerHalf;
            int segments = Mathf.Clamp(_segments, 8, 128);
            Color32 tint = color;

            for (int i = 0; i <= segments; i++)
            {
                Vector2 outer;
                Vector2 inner;
                if (i == 0) outer = inner = start;
                else if (i == segments) outer = inner = end;
                else
                {
                    float fraction = (float)i / segments;
                    float outerAngle = outerStart + outerSweep * fraction;
                    float innerAngle = innerStart + innerSweep * fraction;
                    outer = new Vector2(Mathf.Cos(outerAngle), Mathf.Sin(outerAngle)) * _outerRadius;
                    inner = _innerOffset + new Vector2(Mathf.Cos(innerAngle), Mathf.Sin(innerAngle)) * _innerRadius;
                }
                AddVertex(mesh, bounds.center + outer * size, tint);
                AddVertex(mesh, bounds.center + inner * size, tint);
            }
            for (int i = 0; i < segments; i++)
            {
                int first = i * 2;
                // Both endpoint pairs meet analytically. Omit the two zero-area endpoint triangles.
                if (i > 0) mesh.AddTriangle(first, first + 1, first + 3);
                if (i < segments - 1) mesh.AddTriangle(first, first + 3, first + 2);
            }
        }

        private static void AddVertex(VertexHelper mesh, Vector2 position, Color32 tint)
        {
            mesh.AddVert(new Vector3(position.x, position.y, 0f), tint, Vector2.zero);
        }
    }
}
