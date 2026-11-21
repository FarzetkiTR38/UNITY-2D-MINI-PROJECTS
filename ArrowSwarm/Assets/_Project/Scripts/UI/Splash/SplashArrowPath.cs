namespace ArrowSwarm.UI
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// A gameplay-style arrow drawn as a texture-free UI mesh: a thick polyline with round caps
    /// and joints, and a triangular head that rides the tip while <see cref="Progress"/> grows.
    /// Path points, width and color are authored in the Inspector.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SplashArrowPath : MaskableGraphic
    {
        private const int CapSegments = 10;

        [Tooltip("Local points from tail to head.")]
        [SerializeField] private Vector2[] _points = { new Vector2(0f, -200f), new Vector2(0f, 200f) };
        [SerializeField, Min(1f)] private float _width = 50f;
        [SerializeField, Min(1f)] private float _headLength = 84f;
        [SerializeField, Min(1f)] private float _headWidth = 100f;
        [SerializeField, Range(0f, 1f)] private float _progress = 1f;

        private readonly List<Vector2> _visible = new List<Vector2>(8);

        /// <summary>Drawn fraction of the path, tail first; the head follows the tip.</summary>
        public float Progress
        {
            get => _progress;
            set { value = Mathf.Clamp01(value); if (Mathf.Approximately(value, _progress)) return; _progress = value; SetVerticesDirty(); }
        }

        /// <inheritdoc/>
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (_points == null || _points.Length < 2 || _progress <= 0f) return;
            BuildVisible();
            if (_visible.Count < 2) return;
            Color32 tint = color;
            Vector2 tip = _visible[_visible.Count - 1];
            Vector2 dir = (tip - _visible[_visible.Count - 2]).normalized;

            // The body stops under the head's base so the round cap never pokes past it.
            float bodyTrim = _headLength * 0.45f;
            Vector2 bodyEnd = tip - dir * Mathf.Min(bodyTrim, (tip - _visible[_visible.Count - 2]).magnitude);
            _visible[_visible.Count - 1] = bodyEnd;
            float radius = _width * 0.5f;
            for (int i = 0; i < _visible.Count; i++)
            {
                AddDisc(vh, _visible[i], radius, tint);
                if (i > 0) AddSegment(vh, _visible[i - 1], _visible[i], radius, tint);
            }

            Vector2 normal = new Vector2(-dir.y, dir.x);
            Vector2 baseCenter = tip - dir * (_headLength * 0.5f);
            int s = vh.currentVertCount;
            vh.AddVert(baseCenter + dir * _headLength, tint, Vector2.zero);
            vh.AddVert(baseCenter + normal * (_headWidth * 0.5f), tint, Vector2.zero);
            vh.AddVert(baseCenter - normal * (_headWidth * 0.5f), tint, Vector2.zero);
            vh.AddTriangle(s, s + 1, s + 2);
        }

        private void BuildVisible()
        {
            _visible.Clear();
            float total = 0f;
            for (int i = 1; i < _points.Length; i++) total += Vector2.Distance(_points[i - 1], _points[i]);
            float remaining = total * _progress;
            _visible.Add(_points[0]);
            for (int i = 1; i < _points.Length && remaining > 0f; i++)
            {
                float length = Vector2.Distance(_points[i - 1], _points[i]);
                if (remaining >= length) { _visible.Add(_points[i]); remaining -= length; }
                else { _visible.Add(Vector2.Lerp(_points[i - 1], _points[i], remaining / length)); remaining = 0f; }
            }
        }

        private static void AddSegment(VertexHelper vh, Vector2 a, Vector2 b, float radius, Color32 tint)
        {
            Vector2 d = b - a;
            if (d.sqrMagnitude < 0.0001f) return;
            Vector2 n = new Vector2(-d.y, d.x).normalized * radius;
            int s = vh.currentVertCount;
            vh.AddVert(a + n, tint, Vector2.zero); vh.AddVert(b + n, tint, Vector2.zero);
            vh.AddVert(b - n, tint, Vector2.zero); vh.AddVert(a - n, tint, Vector2.zero);
            vh.AddTriangle(s, s + 1, s + 2); vh.AddTriangle(s, s + 2, s + 3);
        }

        private static void AddDisc(VertexHelper vh, Vector2 p, float radius, Color32 tint)
        {
            int s = vh.currentVertCount;
            vh.AddVert(p, tint, Vector2.zero);
            for (int i = 0; i < CapSegments; i++)
            {
                float a = i * Mathf.PI * 2f / CapSegments;
                vh.AddVert(p + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius, tint, Vector2.zero);
                vh.AddTriangle(s, s + 1 + i, s + 1 + (i + 1) % CapSegments);
            }
        }
    }
}
