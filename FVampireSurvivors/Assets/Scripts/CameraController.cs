using UnityEngine;

public class CameraController : MonoBehaviour
{
    
    private Transform target;

    void Start()
    {
        target = FindAnyObjectByType<PlayerController>().transform;
    }

    // Update yerine LateUpdate kullanma sebebimiz PlayerController dan oyuncunun hareketi 
    // Update ile güncellendikten sonra LateUpdate ile camerayı takip ettireceğiz
    void LateUpdate()
    {
        transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
    }
} 
