namespace ArrowSwarm.Path
{
    using ArrowSwarm.Core;
    using UnityEngine;

    /// <summary>
    /// Draws the mob path visually using LineRenderer.
    /// Also shows spawn and finish point markers.
    /// </summary>
    public class PathVisualizer : MonoBehaviour
    {
        [SerializeField] private float _lineWidth = 0.25f;
        [SerializeField] private Material _pathMaterial;
        [SerializeField] private int _sortingOrder = -6;

        private LineRenderer _lineRenderer;
        private static Material _sharedPathMaterial;

        private void OnEnable()
        {
            PathManager.OnPathInitialized += HandlePathInitialized;
        }

        private void OnDisable()
        {
            PathManager.OnPathInitialized -= HandlePathInitialized;
        }

        private void HandlePathInitialized()
        {
            DrawPath();
        }

        /// <summary>
        /// Draws the path using LineRenderer.
        /// </summary>
        private void DrawPath()
        {
            PathManager pm = PathManager.Instance;
            if (pm == null || pm.Waypoints == null || pm.Waypoints.Count < 2) return;

            if (_lineRenderer == null)
            {
                _lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            // Get path color from current map (High contrast #8C7A68 default)
            Color pathColor = new Color(0.55f, 0.48f, 0.41f, 1f);
            MapData mapData = GameManager.Instance?.Config?.GetMapForLevel(
                Data.DataManager.Instance?.PlayerData?.currentLevel ?? 1);
            if (mapData != null)
            {
                pathColor = mapData.PathColor;
            }

            _lineRenderer.positionCount = pm.Waypoints.Count;
            for (int i = 0; i < pm.Waypoints.Count; i++)
            {
                _lineRenderer.SetPosition(i, pm.Waypoints[i]);
            }

            _lineRenderer.startWidth = _lineWidth;
            _lineRenderer.endWidth = _lineWidth;
            _lineRenderer.startColor = pathColor;
            _lineRenderer.endColor = pathColor;
            _lineRenderer.sortingOrder = _sortingOrder;
            _lineRenderer.useWorldSpace = true;

            if (_pathMaterial != null)
            {
                _lineRenderer.sharedMaterial = _pathMaterial;
            }
            else
            {
                if (_sharedPathMaterial == null)
                {
                    Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Sprites/Default");
                    if (shader != null)
                    {
                        _sharedPathMaterial = new Material(shader);
                        _sharedPathMaterial.mainTexture = Texture2D.whiteTexture;
                    }
                }
                _lineRenderer.sharedMaterial = _sharedPathMaterial;
            }

            // Create spawn and finish markers
            CreateMarker(pm.SpawnPoint, new Color(0.30f, 0.69f, 0.31f, 1f), "SpawnMarker");
            CreateMarker(pm.FinishPoint, new Color(0.96f, 0.26f, 0.21f, 1f), "FinishMarker");
        }

        private void CreateMarker(Vector2 position, Color color, string name)
        {
            // Check if marker already exists
            Transform existing = transform.Find(name);
            if (existing != null) Destroy(existing.gameObject);

            var marker = new GameObject(name);
            marker.transform.SetParent(transform, false);
            marker.transform.position = position;

            var sr = marker.AddComponent<SpriteRenderer>();
            sr.color = color;
            sr.sortingOrder = _sortingOrder + 1;

            // Use a simple circle sprite or create a quad
            // Placeholder — actual sprite assigned in Unity Editor
        }
    }
}
