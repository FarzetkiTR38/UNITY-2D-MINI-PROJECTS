using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates a pie/wedge shaped segment for the spin wheel.
/// Attach to a UI GameObject with Image component.
/// This generates a pizza-slice shaped mesh!
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class PieSlice : Graphic
{
    [Header("Pie Slice Settings")]
    [Range(0f, 360f)]
    public float fillAngle = 45f;  // How wide the slice is (degrees)
    
    [Range(0f, 360f)]
    public float rotationAngle = 0f;  // Starting angle of the slice
    
    [Range(3, 100)]
    public int segments = 20;  // Smoothness - higher = smoother curve
    
    [Range(0f, 1f)]
    public float innerRadius = 0f;  // For donut shape (0 = full pie)
    
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;
        float outerRadius = Mathf.Min(width, height) * 0.5f;
        float inner = outerRadius * innerRadius;
        
        float angleStart = rotationAngle - (fillAngle / 2f);
        float angleStep = fillAngle / segments;
        
        // Create vertices
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;
        
        // Center point (for full pie) or inner ring points (for donut)
        if (innerRadius <= 0f)
        {
            // Full pie - single center vertex
            vertex.position = Vector3.zero;
            vh.AddVert(vertex);
            
            // Outer edge vertices
            for (int i = 0; i <= segments; i++)
            {
                float angle = (angleStart + (angleStep * i)) * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * outerRadius;
                float y = Mathf.Sin(angle) * outerRadius;
                
                vertex.position = new Vector3(x, y, 0);
                vh.AddVert(vertex);
            }
            
            // Create triangles (fan from center)
            for (int i = 0; i < segments; i++)
            {
                vh.AddTriangle(0, i + 1, i + 2);
            }
        }
        else
        {
            // Donut shape - inner and outer ring
            for (int i = 0; i <= segments; i++)
            {
                float angle = (angleStart + (angleStep * i)) * Mathf.Deg2Rad;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);
                
                // Inner vertex
                vertex.position = new Vector3(cos * inner, sin * inner, 0);
                vh.AddVert(vertex);
                
                // Outer vertex
                vertex.position = new Vector3(cos * outerRadius, sin * outerRadius, 0);
                vh.AddVert(vertex);
            }
            
            // Create triangles (quads between rings)
            for (int i = 0; i < segments; i++)
            {
                int idx = i * 2;
                vh.AddTriangle(idx, idx + 1, idx + 3);
                vh.AddTriangle(idx, idx + 3, idx + 2);
            }
        }
    }
    
    /// <summary>
    /// Setup the pie slice with given parameters
    /// </summary>
    public void SetSlice(float angle, float rotation, Color sliceColor)
    {
        fillAngle = angle;
        rotationAngle = rotation;
        color = sliceColor;
        
        Debug.Log($"[PieSlice] fillAngle={fillAngle}, rotationAngle={rotationAngle}");
        
        // Force complete refresh
        SetAllDirty();
    }
    
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }
#endif
}
