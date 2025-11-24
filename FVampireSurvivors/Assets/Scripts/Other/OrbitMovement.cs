using UnityEngine;

public class OrbitMovement : MonoBehaviour
{
    [Header("Orbit Settings")]
    public float rotationSpeed = 180f; // derece/sn
    public float radius = 1.5f;

    private Transform anchor; // player’ın SwordAnchor'ı

    void Start()
    {
        anchor = transform.parent;
        transform.localPosition = new Vector3(radius, 0, 0);
    }

    void Update()
    {
        if (anchor == null) return;

        // Anchor etrafında döndür
        transform.RotateAround(anchor.position, Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}
