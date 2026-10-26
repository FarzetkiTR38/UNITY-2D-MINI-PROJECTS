namespace ArrowSwarm.UI
{
    using TMPro;
    using UnityEngine;

    /// <summary>Optionally fits translated title geometry within its authored width without changing fonts, colors or arch styling.</summary>
    public partial class Casual3DTitle
    {
        [Header("Optional Localized Width Fit")]
        [Tooltip("Uniformly shrink all four warped layers together when the translated title exceeds this RectTransform's width.")]
        [SerializeField] private bool _fitToRect;
        [Tooltip("Horizontal inset on each side of the authored title rectangle, in local Canvas units.")]
        [Min(0f), SerializeField] private float _horizontalFitPadding = 12f;

        private void OnRectTransformDimensionsChange() { if (_fitToRect) MarkDirty(); }

        private void FitRenderedLayers()
        {
            if (!_fitToRect || !(transform is RectTransform rect) ||
                !TryGetRenderedBounds(out Bounds bounds) || bounds.size.x <= .001f) return;
            float available = Mathf.Max(1f, rect.rect.width - _horizontalFitPadding * 2f);
            float scale = Mathf.Min(1f, available / bounds.size.x);
            float centerShift = rect.rect.center.x - bounds.center.x * scale;
            FitLayer(_frontText, scale, centerShift); FitLayer(_rimText, scale, centerShift);
            FitLayer(_extrusionText, scale, centerShift); FitLayer(_shadowText, scale, centerShift);
        }

        private void FitLayer(TextMeshProUGUI layer, float scale, float centerShift)
        {
            if (layer == null) return;
            var layerRect = layer.rectTransform;
            Vector3 origin = transform.InverseTransformPoint(layerRect.position);
            origin *= scale; origin.x += centerShift;
            layerRect.localScale *= scale;
            layerRect.position = transform.TransformPoint(origin);
        }

        /// <summary>Gets the combined visible, already warped glyph bounds of every layer in this title's local coordinates.</summary>
        public bool TryGetRenderedBounds(out Bounds bounds)
        {
            bounds = default; bool hasVertices = false;
            AccumulateBounds(_frontText, ref bounds, ref hasVertices);
            AccumulateBounds(_rimText, ref bounds, ref hasVertices);
            AccumulateBounds(_extrusionText, ref bounds, ref hasVertices);
            AccumulateBounds(_shadowText, ref bounds, ref hasVertices);
            return hasVertices;
        }

        private void AccumulateBounds(TextMeshProUGUI layer, ref Bounds bounds, ref bool hasVertices)
        {
            if (layer == null || layer.textInfo == null) return;
            var info = layer.textInfo;
            Matrix4x4 toTitle = transform.worldToLocalMatrix * layer.rectTransform.localToWorldMatrix;
            for (int index = 0; index < info.characterCount; index++)
            {
                var character = info.characterInfo[index];
                if (!character.isVisible) continue;
                var vertices = info.meshInfo[character.materialReferenceIndex].vertices;
                for (int corner = 0; corner < 4; corner++)
                {
                    Vector3 point = toTitle.MultiplyPoint3x4(vertices[character.vertexIndex + corner]);
                    if (!hasVertices) { bounds = new Bounds(point, Vector3.zero); hasVertices = true; }
                    else bounds.Encapsulate(point);
                }
            }
        }
    }
}
