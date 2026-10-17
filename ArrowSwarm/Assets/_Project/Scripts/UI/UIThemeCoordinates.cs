namespace ArrowSwarm.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Supplies rect-relative theme coordinates independently of sprite slicing and atlas UVs.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public class UIThemeCoordinates : BaseMeshEffect
    {
        /// <summary>Writes normalized positions only when Unity rebuilds the UI mesh.</summary>
        public override void ModifyMesh(VertexHelper vertices)
        {
            if (!IsActive() || graphic == null) return;
            Rect rect = graphic.rectTransform.rect;
            if (rect.width <= 0f || rect.height <= 0f) return;
            UIVertex vertex = default;
            for (int i = 0; i < vertices.currentVertCount; i++)
            {
                vertices.PopulateUIVertex(ref vertex, i);
                vertex.uv1 = new Vector4((vertex.position.x - rect.xMin) / rect.width,
                    (vertex.position.y - rect.yMin) / rect.height, 0f, 0f);
                vertices.SetUIVertex(vertex, i);
            }
        }
    }
}
