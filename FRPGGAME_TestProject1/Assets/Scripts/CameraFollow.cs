using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0f, 6f, 0f);
    public float followSpeed = 10f;

    private Transform target;
    private Quaternion initialRotation; // kameranın ilk rotasyonunu saklıyoruz

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Kamera açısını ilk haliyle kaydet
        initialRotation = transform.rotation;
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        // Pozisyon takibi
        Vector3 desiredPos = target.position;
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);

        // Kamera açısını ASLA değiştirme
        transform.rotation = initialRotation;
    }
}
