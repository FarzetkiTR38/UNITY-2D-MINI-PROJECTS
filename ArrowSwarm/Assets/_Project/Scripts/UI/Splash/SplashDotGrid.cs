namespace ArrowSwarm.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// The gameplay board's dot grid as a single texture-free UI mesh, centered on its rect
    /// so dots stay symmetric on every aspect ratio. Rebuilt only on layout changes.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SplashDotGrid : MaskableGraphic
    {
        private const int CircleSegments = 10;

        [SerializeField, Min(8f)] private float _spacing = 108f;
        [SerializeField, Min(0.5f)] private float _dotRadius = 9f;

        /// <inheritdoc/>
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = rectTransform.rect;
            Vector2 c = r.center;
            int halfX = Mathf.CeilToInt(r.width * 0.5f / _spacing);
            int halfY = Mathf.CeilToInt(r.height * 0.5f / _spacing);
            Color32 tint = color;
            for (int x = -halfX; x <= halfX; x++)
            {
                for (int y = -halfY; y <= halfY; y++)
                {
                    // Half-spacing offset matches the board, where dots sit between grid lines.
                    Vector2 p = c + new Vector2((x + 0.5f) * _spacing, (y + 0.5f) * _spacing);
                    if (!r.Contains(p)) continue;
                    AddDot(vh, p, tint);
                }
            }
        }

        private void AddDot(VertexHelper vh, Vector2 p, Color32 tint)
        {
            int start = vh.currentVertCount;
            vh.AddVert(p, tint, Vector2.zero);
            for (int i = 0; i < CircleSegments; i++)
            {
                float a = i * Mathf.PI * 2f / CircleSegments;
                vh.AddVert(p + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * _dotRadius, tint, Vector2.zero);
                vh.AddTriangle(start, start + 1 + i, start + 1 + (i + 1) % CircleSegments);
            }
        }
    }
}
