namespace ArrowSwarm.Camera
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Connects the UI zoom slider to the CameraController.
    /// Updates slider when zoom changes, and zooms when slider moves.
    /// </summary>
    public class ZoomController : MonoBehaviour
    {
        [SerializeField] private Slider _zoomSlider;

        private bool _isUpdatingFromCamera;

        private void OnEnable()
        {
            CameraController.OnZoomChanged += HandleZoomChanged;
            if (_zoomSlider != null)
            {
                _zoomSlider.onValueChanged.AddListener(HandleSliderChanged);
            }
        }

        private void OnDisable()
        {
            CameraController.OnZoomChanged -= HandleZoomChanged;
            if (_zoomSlider != null)
            {
                _zoomSlider.onValueChanged.RemoveListener(HandleSliderChanged);
            }
        }

        /// <summary>
        /// Called by CameraController when zoom changes (e.g., pinch).
        /// Updates the slider without triggering a feedback loop.
        /// </summary>
        private void HandleZoomChanged(float normalizedZoom)
        {
            if (_zoomSlider == null) return;

            _isUpdatingFromCamera = true;
            _zoomSlider.value = normalizedZoom;
            _isUpdatingFromCamera = false;
        }

        /// <summary>
        /// Called when the player moves the slider.
        /// Updates the camera zoom.
        /// </summary>
        private void HandleSliderChanged(float value)
        {
            if (_isUpdatingFromCamera) return;
            CameraController.Instance?.SetZoomNormalized(value);
        }
    }
}
