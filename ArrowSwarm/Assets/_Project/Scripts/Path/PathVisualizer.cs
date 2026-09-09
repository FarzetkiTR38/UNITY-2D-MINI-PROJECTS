namespace ArrowSwarm.Path
{
    using ArrowSwarm.Core;
    using ArrowSwarm.Effects;
    using UnityEngine;

    /// <summary>
    /// Instantiates and manages visual portal sprites at SpawnPoint and FinishPoint.
    /// Portals render above arrows (SortingOrder 15) with dynamic rotation & pulse effects.
    /// </summary>
    public class PathVisualizer : MonoBehaviour
    {
        [Header("Portal Sprites")]
        [Tooltip("Sprite for the spawn portal (where mobs appear).")]
        [SerializeField] private Sprite _spawnPortalSprite;
        [Tooltip("Sprite for the finish portal (where mobs try to reach).")]
        [SerializeField] private Sprite _finishPortalSprite;
        [SerializeField] private float _portalScale = 1.5f;
        [SerializeField] private int _portalSortingOrder = 15;

        private Transform _portalsContainer;
        private static Sprite _cachedCircleSprite;

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
        /// Instantiates portal sprites at the spawn and finish positions.
        /// </summary>
        public void DrawPath()
        {
            PathManager pm = PathManager.Instance;
            if (pm == null || pm.Waypoints == null || pm.Waypoints.Count < 2) return;

            ClearPortals();

            bool isLoop = Vector2.Distance(pm.SpawnPoint, pm.FinishPoint) <= 0.1f;

            if (isLoop)
            {
                // Unified portal serving as both spawn and finish in loop tracks
                CreatePortal(pm.SpawnPoint, _spawnPortalSprite, "Portal",
                    new Color(0.30f, 0.69f, 0.31f, 1f), isSpawn: true, isFinish: true);
            }
            else
            {
                CreatePortal(pm.SpawnPoint, _spawnPortalSprite, "SpawnPortal",
                    new Color(0.30f, 0.69f, 0.31f, 1f), isSpawn: true, isFinish: false);
                CreatePortal(pm.FinishPoint, _finishPortalSprite, "FinishPortal",
                    new Color(0.96f, 0.26f, 0.21f, 1f), isSpawn: false, isFinish: true);
            }
        }

        /// <summary>
        /// Clears all visual path elements and portals.
        /// </summary>
        public void ClearPath()
        {
            ClearPortals();
        }

        /// <summary>
        /// Removes all portal GameObjects cleanly.
        /// </summary>
        public void ClearPortals()
        {
            EnsurePortalsContainer();

            if (_portalsContainer != null)
            {
                for (int i = _portalsContainer.childCount - 1; i >= 0; i--)
                {
                    Transform child = _portalsContainer.GetChild(i);
                    child.SetParent(null);
                    if (Application.isPlaying) Destroy(child.gameObject);
                    else DestroyImmediate(child.gameObject);
                }
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
            else if (gameObject.activeInHierarchy)
            {
                var obj = new GameObject("PathPortalsContainer");
                obj.transform.SetParent(transform, false);
                _portalsContainer = obj.transform;
            }
        }

        private void CreatePortal(Vector2 position, Sprite portalSprite, string name, Color fallbackColor, bool isSpawn, bool isFinish)
        {
            EnsurePortalsContainer();

            var portal = new GameObject(name);
            portal.transform.SetParent(_portalsContainer, false);
            portal.transform.position = new Vector3(position.x, position.y, 0f);

            float scaleFactor = 1.0f;
            if (ArrowSwarm.Grid.GridManager.HasInstance)
            {
                scaleFactor = DifficultyCalculator.GetMapScaleFactor(
                    ArrowSwarm.Grid.GridManager.Instance.Width,
                    ArrowSwarm.Grid.GridManager.Instance.Height);
            }
            portal.transform.localScale = Vector3.one * (_portalScale * scaleFactor);

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

            // Attach dynamic portal micro-animations (idle swirl, breathing pulse, spawn/finish burst)
            var effect = portal.AddComponent<PortalVisualEffect>();
            effect.Initialize(isSpawn, isFinish);
        }

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
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, dist <= radius ? 1f : 0f));
                }
            }

            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;

            _cachedCircleSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _cachedCircleSprite;
        }
    }
}
