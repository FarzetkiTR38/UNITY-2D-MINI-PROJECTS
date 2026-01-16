using UnityEngine;

public class OrbitMovement : MonoBehaviour
{
    [Header("Orbit Settings")]
    public float rotationSpeed = 180f;
    public float radius = 1.5f;

    [Tooltip("Starting angle in degrees (0 = right, 90 = up, 180 = left, 270 = down)")]
    public float initialAngle = 0f;

    [Header("Visual Settings")]
    [Tooltip("If true, the object rotates to face its movement direction")]
    public bool faceMovementDirection = true;

    [Tooltip("Additional rotation offset in degrees (adjust if sprite faces wrong way)")]
    public float rotationOffset = -90f;

    private Transform anchor;
    private float currentAngle;

    void Start()
    {
        anchor = transform.parent;
        currentAngle = initialAngle;
        UpdatePosition();
    }

    void Update()
    {
        if (anchor == null) return;

        // Increment angle based on rotation speed
        currentAngle += rotationSpeed * Time.deltaTime;

        // Keep angle in 0-360 range
        if (currentAngle >= 360f) currentAngle -= 360f;
        if (currentAngle < 0f) currentAngle += 360f;

        UpdatePosition();
    }

    void UpdatePosition()
    {
        if (anchor == null) return;

        float radians = currentAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(
            Mathf.Cos(radians) * radius,
            Mathf.Sin(radians) * radius,
            0f
        );

        transform.position = anchor.position + offset;

        // Rotate to face movement direction (tangent to orbit)
        if (faceMovementDirection)
        {
            // Movement direction is perpendicular to radius (tangent)
            // For clockwise rotation: tangent = 90° behind the radius angle
            float tangentAngle = currentAngle + 90f + rotationOffset;
            transform.rotation = Quaternion.Euler(0f, 0f, tangentAngle);
        }
    }

    /// <summary>
    /// Set the initial angle for this orbiting object
    /// </summary>
    public void SetInitialAngle(float angle)
    {
        initialAngle = angle;
        currentAngle = angle;
        UpdatePosition();
    }
}
