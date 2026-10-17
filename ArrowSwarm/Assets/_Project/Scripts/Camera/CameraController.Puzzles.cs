namespace ArrowSwarm.Camera
{
    using ArrowSwarm.Core;
    using UnityEngine;

    /// <summary>Fits isolated puzzle bounds while reusing the existing zoom and pan implementation.</summary>
    public partial class CameraController
    {
        [Header("Puzzle Board Fit")]
        [SerializeField] private ArrowSwarm.Puzzle.PuzzleBoardSurface _puzzleSurface;
        [SerializeField, Range(.5f, 1f)] private float _puzzleWidthCoverage = .92f;
        [SerializeField, Range(0f, .15f)] private float _puzzleVerticalInset = .025f;

        /// <summary>Initializes zoom limits without consulting persistent Defense grid state.</summary>
        public void FitToPuzzle(int width, int height)
        {
            Canvas.ForceUpdateCanvases();
            AutoFindHudPanels();
            GetHudRatios(out float topRatio, out float bottomRatio);
            if (_puzzleSurface == null)
                _puzzleSurface = FindFirstObjectByType<ArrowSwarm.Puzzle.PuzzleBoardSurface>();

            Vector2 visualSize = _puzzleSurface != null
                ? _puzzleSurface.GetVisualSize(width, height)
                : new Vector2(width + 2f, height + 2f);
            _mapExtents = visualSize * .5f;
            _gridMin = -_mapExtents; _gridMax = _mapExtents;

            float aspect = Screen.height > 0 ? (float)Screen.width / Screen.height : 9f / 16f;
            if (float.IsNaN(aspect) || float.IsInfinity(aspect) || aspect <= .01f) aspect = 9f / 16f;
            float playableHeight = Mathf.Max(.1f, 1f - topRatio - bottomRatio - 2f * _puzzleVerticalInset);
            _defaultOrthoSize = Mathf.Max(_mapExtents.x / (aspect * _puzzleWidthCoverage),
                _mapExtents.y / playableHeight);
            var config = GameManager.Instance?.Config;
            float minZoom = Mathf.Max(.01f, config?.MinZoom ?? 1f);
            float maxZoom = Mathf.Max(minZoom, config?.MaxZoom ?? 3f);
            _maxOrthoSize = _defaultOrthoSize / minZoom;
            _minOrthoSize = _defaultOrthoSize / maxZoom;
            _targetOrthoSize = _defaultOrthoSize;
            _zoomVelocity = 0f;
            Cam.orthographicSize = _defaultOrthoSize;

            // ResetCamera uses this fitted center; the actual board bounds remain centered on zero.
            _mapCenter = new Vector2(0f, _defaultOrthoSize * (topRatio - bottomRatio));
            float cameraZ = transform.position.z;
            if (float.IsNaN(cameraZ) || Mathf.Abs(cameraZ) < .1f) cameraZ = -10f;
            transform.position = new Vector3(_mapCenter.x, _mapCenter.y, cameraZ);
            _isPanning = _isDragging = _isPinching = false;
            _isTouchDragValid = _isMouseDragValid = false;
            OnZoomChanged?.Invoke(ZoomNormalized);
        }
    }
}
