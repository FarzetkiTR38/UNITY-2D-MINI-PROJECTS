using UnityEngine;

public class Movement : MonoBehaviour
{
    public Camera cam;
    public float speed = 3f;

    void Start()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    void Update()
    {
        Vector3 input = Input.mousePosition;

        Vector3 worldInput = cam.ScreenToWorldPoint(input);

        Vector3 newPosition = Vector3.MoveTowards(transform.position, worldInput, speed * Time.deltaTime);

        newPosition.z = transform.position.z;

        transform.position = newPosition;
    }
}
