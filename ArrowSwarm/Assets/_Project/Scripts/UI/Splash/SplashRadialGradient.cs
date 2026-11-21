namespace ArrowSwarm.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Texture-free radial gradient that fills its rect: center color at an authored focus,
    /// fading to the edge color. Built once per layout change, so it costs nothing per frame.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SplashRadialGradient : MaskableGraphic
    {
        private const int Segments = 48;

        [SerializeField] private Color _centerColor = new Color(0.114f, 0.165f, 0.302f, 1f);
        [SerializeField] private Color _edgeColor = new Color(0.055f, 0.078f, 0.141f, 1f);
        [Tooltip("Gradient focus in normalized rect coordinates.")]
        [SerializeField] private Vector2 _focus = new Vector2(0.5f, 0.54f);

        /// <inheritdoc/>
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = rectTransform.rect;
            Vector2 center = new Vector2(r.xMin + r.width * _focus.x, r.yMin + r.height * _focus.y);
            Color32 inner = _centerColor * color, outer = _edgeColor * color;
            vh.AddVert(center, inner, Vector2.zero);

            // Ellipse reaching the farthest rect corner, so the edge color lands on the corners.
            float rx = Mathf.Max(center.x - r.xMin, r.xMax - center.x) * 1.42f;
            float ry = Mathf.Max(center.y - r.yMin, r.yMax - center.y) * 1.42f;
            for (int i = 0; i <= Segments; i++)
            {
                float a = i * Mathf.PI * 2f / Segments;
                vh.AddVert(center + new Vector2(Mathf.Cos(a) * rx, Mathf.Sin(a) * ry), outer, Vector2.zero);
                if (i > 0) vh.AddTriangle(0, i, i + 1);
            }
        }
    }
}
