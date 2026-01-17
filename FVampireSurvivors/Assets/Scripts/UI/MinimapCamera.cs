using UnityEngine;

/// <summary>
/// Minimap Camera Controller
/// Follows the player from above, renders to a RenderTexture
/// </summary>
public class MinimapCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // Player

    [Header("Camera Settings")]
    public float height = 50f;      // How high above the target
    public float zoomLevel = 20f;   // Orthographic size

    [Header("Minimap Icons")]
    public GameObject playerIconPrefab;  // Player marker
    public GameObject enemyIconPrefab;   // Enemy markers (optional)

    private Camera minimapCam;
    private GameObject playerIcon;

    void Start()
    {
        minimapCam = GetComponent<Camera>();
        if (minimapCam == null)
        {
            minimapCam = gameObject.AddComponent<Camera>();
        }

        // Set camera to orthographic for top-down view
        minimapCam.orthographic = true;
        minimapCam.orthographicSize = zoomLevel;

        // Find player if not assigned
        if (target == null)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player != null)
                target = player.transform;
        }

        // Create player icon
        if (playerIconPrefab != null)
        {
            playerIcon = Instantiate(playerIconPrefab);
            playerIcon.transform.SetParent(transform);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Follow player - top-down view
        Vector3 newPos = target.position;
        newPos.z = -height; // For 2D, we use z for the camera height
        transform.position = newPos;

        // Update player icon position (if using separate icons)
        if (playerIcon != null)
        {
            playerIcon.transform.position = target.position + Vector3.back * 0.1f;
        }
    }

    /// <summary>
    /// Adjust zoom level at runtime
    /// </summary>
    public void SetZoom(float zoom)
    {
        zoomLevel = zoom;
        if (minimapCam != null)
            minimapCam.orthographicSize = zoomLevel;
    }
}
