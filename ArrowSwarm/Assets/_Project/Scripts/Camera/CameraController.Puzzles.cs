namespace ArrowSwarm.Camera
{
    using ArrowSwarm.Core;
    using UnityEngine;

    /// <summary>Fits isolated puzzle bounds while reusing the existing zoom and pan implementation.</summary>
    public partial class CameraController
    {
        /// <summary>Initializes zoom limits without consulting persistent Defense grid state.</summary>
        public void FitToPuzzle(int width, int height)
        {
            _mapCenter = Vector2.zero;
            _mapExtents = new Vector2((width + 2f)*.5f,(height + 2f)*.5f);
            _gridMin = -_mapExtents; _gridMax = _mapExtents;
            float aspect = Screen.height > 0 ? (float)Screen.width/Screen.height : 9f/16f;
            _defaultOrthoSize = Mathf.Max(_mapExtents.x/(aspect*.88f),_mapExtents.y/.63f);
            _maxOrthoSize = _defaultOrthoSize/(GameManager.Instance.Config?.MinZoom ?? 1f);
            _minOrthoSize = _defaultOrthoSize/(GameManager.Instance.Config?.MaxZoom ?? 3f);
            _targetOrthoSize = _defaultOrthoSize;
            Cam.orthographicSize = _defaultOrthoSize;
            transform.position = new Vector3(0,0,-10);
        }
    }
}
