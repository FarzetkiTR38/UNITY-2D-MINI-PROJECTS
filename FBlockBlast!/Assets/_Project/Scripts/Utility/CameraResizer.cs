using UnityEngine;

namespace NeonGalaxy.Utility
{
    /// <summary>
    /// Dynamically resizes the Orthographic Camera size and position
    /// to ensure the board and the piece tray fit on any screen aspect ratio.
    /// Works for both ultra-tall portrait phones (e.g., 9:20 / 1080x2408) and wide tablets.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraResizer : MonoBehaviour
    {
        [Header("Target Bounds to Fit")]
        [SerializeField] private float requiredWidth = 9.2f;  // Board width (8.35) + padding
        [SerializeField] private float requiredHeight = 12.0f; // Height from board top to tray bottom + padding
        [SerializeField] private float targetCenterY = -1.15f; // Center slightly downwards to balance board & tray

        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            AdjustCamera();
        }

#if UNITY_EDITOR
        private void Update()
        {
            // Auto-updates in editor for live preview when resizing the game window
            AdjustCamera();
        }
#endif

        private void AdjustCamera()
        {
            if (_camera == null) return;

            // Get current aspect ratio (Width / Height)
            float aspect = (float)Screen.width / Screen.height;

            if (aspect <= 0f) return;

            // Calculate required orthographic sizes
            float sizeForWidth = (requiredWidth / aspect) / 2f;
            float sizeForHeight = requiredHeight / 2f;

            // Choose the max size to guarantee both width and height fit
            _camera.orthographicSize = Mathf.Max(sizeForWidth, sizeForHeight);

            // Shift camera down slightly so board and tray are perfectly centered
            Vector3 pos = transform.position;
            pos.y = targetCenterY;
            transform.position = pos;
        }
    }
}
