using UnityEngine;

public class CameraController : MonoBehaviour
{

    public Transform target;
    public float speed;
    public float scalespeed;
    public Camera cam;

    void Update()
    {

        Vector3 positionLerp = Vector3.Lerp(transform.position, target.position, Time.deltaTime * speed);

        positionLerp.z = transform.position.z;

        transform.position = positionLerp;

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, 4 * target.localScale.x, scalespeed * Time.deltaTime / 25);
        //cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, 4 * target.localScale.x, scalespeed * Time.deltaTime);

    }
}
