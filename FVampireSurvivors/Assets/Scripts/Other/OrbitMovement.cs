using UnityEngine;

public class OrbitMovement : MonoBehaviour
{
    public float rotationSpeed = 180f;
    public float radius = 1.5f;

    private Transform anchor;

    void Start()
    {
        anchor = transform.parent;
        transform.localPosition = new Vector3(radius, 0, 0);
    }

    void Update()
    {
        if (anchor == null) return;

        transform.RotateAround(anchor.position, Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}
