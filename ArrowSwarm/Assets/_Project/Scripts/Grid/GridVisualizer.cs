namespace ArrowSwarm.Grid
{
    using ArrowSwarm.Core;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Draws the point grid visually using small dot sprites at each intersection.
    /// Subscribes to GridManager events to know when to draw.
    /// Also callable directly via DrawPointGrid() for immediate rendering.
    /// Manages its own dedicated child container to prevent destroying sibling objects.
    /// </summary>
    public class GridVisualizer : MonoBehaviour
    {
        [SerializeField] private float _dotScaleMultiplier = 0.14f;
        [SerializeField] private Color _dotColor = new Color(0.3f, 0.3f, 0.5f, 0.5f);
        [SerializeField] private Color _edgeDotColor = new Color(0.4f, 0.4f, 0.6f, 0.7f);
        [SerializeField] private int _sortingOrder = -1;

        private GridManager _gridManager;
        private Transform _dotsContainer;

        private void OnEnable()
        {
            GridManager.OnGridInitialized += HandleGridInitialized;
        }

        private void OnDisable()
        {
            GridManager.OnGridInitialized -= HandleGridInitialized;
        }

        private void HandleGridInitialized(int width, int height)
        {
            _gridManager = GridManager.Instance;
            DrawPointGrid();
        }

        /// <summary>
        /// Creates dot sprites at every grid intersection point.
        /// Includes null safety for GridManager reference.
        /// </summary>
        public void DrawPointGrid()
        {
            if (_gridManager == null)
            {
                _gridManager = GridManager.Instance;
                if (_gridManager == null)
                {
                    _gridManager = GetComponent<GridManager>();
                }
            }
            if (_gridManager == null) return;

            EnsureDotsContainer();

            // Clear only existing dot children inside the dedicated container
            for (int i = _dotsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_dotsContainer.GetChild(i).gameObject);
            }

            int width = _gridManager.Width;
            int height = _gridManager.Height;
            float spacing = _gridManager.PointSpacing;
            Vector2 origin = _gridManager.Origin;
            float dotSize = Mathf.Clamp(spacing * _dotScaleMultiplier, 0.04f, 0.10f);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    Vector2 worldPos = pos.PointToWorld(spacing, origin);
                    bool isEdge = pos.IsEdge(width, height);

                    CreateDot(worldPos, isEdge, dotSize, $"Dot_{x}_{y}");
                }
            }
        }

        private void EnsureDotsContainer()
        {
            if (_dotsContainer != null) return;
            Transform existing = transform.Find("GridDotsContainer");
            if (existing != null)
            {
                _dotsContainer = existing;
            }
            else
            {
                var obj = new GameObject("GridDotsContainer");
                obj.transform.SetParent(transform, false);
                _dotsContainer = obj.transform;
            }
        }

        /// <summary>
        /// Creates a single dot sprite at the given world position.
        /// </summary>
        private void CreateDot(Vector2 position, bool isEdge, float dotSize, string name)
        {
            var dotObj = new GameObject(name);
            dotObj.transform.SetParent(_dotsContainer, false);
            dotObj.transform.position = new Vector3(position.x, position.y, 0f);

            var sr = dotObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = isEdge ? _edgeDotColor : _dotColor;
            sr.sortingOrder = _sortingOrder;
            dotObj.transform.localScale = Vector3.one * dotSize;
        }

        /// <summary>
        /// Creates a simple circle texture for dot sprites.
        /// Cached after first creation.
        /// </summary>
        private static Sprite _cachedCircleSprite;

        private static Sprite CreateCircleSprite()
        {
            if (_cachedCircleSprite != null) return _cachedCircleSprite;

            int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size * 0.5f;
            float radius = center - 1f;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float alpha = dist <= radius ? 1f : 0f;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;

            _cachedCircleSprite = Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                size
            );

            return _cachedCircleSprite;
        }
    }
}
