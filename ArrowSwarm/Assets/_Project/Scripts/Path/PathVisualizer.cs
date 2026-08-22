namespace ArrowSwarm.Path
{
    using ArrowSwarm.Core;
    using UnityEngine;

    /// <summary>
    /// Draws the mob path visually using LineRenderer.
    /// Instantiates portal sprites at SpawnPoint and FinishPoint.
    /// Portals render above arrows (SortingOrder 15) so arrows visually
    /// enter the portal, while mobs render above portals (SortingOrder 20).
    /// </summary>
    public class PathVisualizer : MonoBehaviour
    {
        [Header("Path Line")]
        [SerializeField] private float _lineWidth = 0.25f;
        [SerializeField] private Material _pathMaterial;
        [SerializeField] private int _sortingOrder = -6;

        [Header("Portal Sprites")]
        [Tooltip("Sprite for the spawn portal (where mobs appear).")]
        [SerializeField] private Sprite _spawnPortalSprite;
        [Tooltip("Sprite for the finish portal (where mobs try to reach).")]
        [SerializeField] private Sprite _finishPortalSprite;
        [SerializeField] private float _portalScale = 1.5f;
        [SerializeField] private int _portalSortingOrder = 15;

        private LineRenderer _lineRenderer;
        private Transform _portalsContainer;
        private static Material _sharedPathMaterial;

        private void OnEnable()
        {
            PathManager.OnPathInitialized += HandlePathInitialized;
        }

        private void OnDisable()
        {
            PathManager.OnPathInitialized -= HandlePathInitialized;
            ClearPath();
        }

        private void HandlePathInitialized()
        {
            DrawPath();
        }

        /// <summary>
        /// Draws the path using LineRenderer and instantiates portal sprites
        /// at the spawn and finish positions.
        /// </summary>
        public void DrawPath()
        {
            PathManager pm = PathManager.Instance;
            if (pm == null) return;
            if (pm.Waypoints == null) return;
            if (pm.Waypoints.Count < 2) return;

            if (_lineRenderer == null)
            {
                _lineRenderer = gameObject.GetComponent<LineRenderer>();
                if (_lineRenderer == null)
                {
                    _lineRenderer = gameObject.AddComponent<LineRenderer>();
                }
            }

            // Get path color from current map (High contrast #8C7A68 default)
            Color pathColor = new Color(0.55f, 0.48f, 0.41f, 1f);
            GameManager gmInstance = GameManager.Instance;
            if (gmInstance != null)
            {
                GameConfig config = gmInstance.Config;
                if (config != null)
                {
                    int currentLevel = 1;
                    if (Data.DataManager.HasInstance)
                    {
                        Data.DataManager dm = Data.DataManager.Instance;
                        if (dm != null && dm.PlayerData != null)
                        {
                            currentLevel = dm.PlayerData.currentLevel;
                        }
                    }
                    MapData mapData = config.GetMapForLevel(currentLevel);
                    if (mapData != null)
                    {
                        pathColor = mapData.PathColor;
                    }
                }
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
                    Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                    if (shader == null)
                    {
                        shader = Shader.Find("Sprites/Default");
                    }
                    if (shader != null)
                    {
                        _sharedPathMaterial = new Material(shader);
                        _sharedPathMaterial.mainTexture = Texture2D.whiteTexture;
                    }
                }
                _lineRenderer.sharedMaterial = _sharedPathMaterial;
            }

            // Clear any previous portal objects first
            ClearPortals();

            // Create portal sprites at spawn and finish points
            CreatePortal(pm.SpawnPoint, _spawnPortalSprite, "SpawnPortal",
                new Color(0.30f, 0.69f, 0.31f, 1f));

            // Only create separate finish portal if it doesn't overlap spawn point
            if (Vector2.Distance(pm.SpawnPoint, pm.FinishPoint) > 0.1f)
            {
                CreatePortal(pm.FinishPoint, _finishPortalSprite, "FinishPortal",
                    new Color(0.96f, 0.26f, 0.21f, 1f));
            }
        }

        /// <summary>
        /// Clears all visual path elements (line and portals).
        /// </summary>
        public void ClearPath()
        {
            if (_lineRenderer != null)
            {
                _lineRenderer.positionCount = 0;
            }
            ClearPortals();
        }

        /// <summary>
        /// Removes all portal GameObjects.
        /// </summary>
        public void ClearPortals()
        {
            EnsurePortalsContainer();

            for (int i = _portalsContainer.childCount - 1; i >= 0; i--)
            {
                GameObject child = _portalsContainer.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }

            // Also check for legacy loose portal children on root transform
            Transform oldSpawn = transform.Find("SpawnPortal");
            if (oldSpawn != null)
            {
                if (Application.isPlaying) Destroy(oldSpawn.gameObject);
                else DestroyImmediate(oldSpawn.gameObject);
            }
            Transform oldFinish = transform.Find("FinishPortal");
            if (oldFinish != null)
            {
                if (Application.isPlaying) Destroy(oldFinish.gameObject);
                else DestroyImmediate(oldFinish.gameObject);
            }
        }

        private void EnsurePortalsContainer()
        {
            if (_portalsContainer != null) return;
            Transform existing = transform.Find("PathPortalsContainer");
            if (existing != null)
            {
                _portalsContainer = existing;
            }
            else
            {
                var obj = new GameObject("PathPortalsContainer");
                obj.transform.SetParent(transform, false);
                _portalsContainer = obj.transform;
            }
        }

        /// <summary>
        /// Creates a portal sprite inside the dedicated portals container.
        /// </summary>
        private void CreatePortal(Vector2 position, Sprite portalSprite, string name, Color fallbackColor)
        {
            EnsurePortalsContainer();

            var portal = new GameObject(name);
            portal.transform.SetParent(_portalsContainer, false);
            portal.transform.position = new Vector3(position.x, position.y, 0f);
            portal.transform.localScale = Vector3.one * _portalScale;

            var sr = portal.AddComponent<SpriteRenderer>();
            sr.sortingOrder = _portalSortingOrder;

            if (portalSprite != null)
            {
                sr.sprite = portalSprite;
                sr.color = Color.white;
            }
            else
            {
                sr.sprite = CreateCircleSprite();
                sr.color = fallbackColor;
            }
        }

        /// <summary>
        /// Creates a simple circle sprite for fallback portal markers.
        /// Cached after first creation.
        /// </summary>
        private static Sprite _cachedCircleSprite;

        private static Sprite CreateCircleSprite()
        {
            if (_cachedCircleSprite != null) return _cachedCircleSprite;

            int size = 64;
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
