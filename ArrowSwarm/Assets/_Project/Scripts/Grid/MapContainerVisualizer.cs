namespace ArrowSwarm.Grid
{
    using ArrowSwarm.Core;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Renders the 3-layer visual theme procedurally at runtime:
    /// Layer 1: Camera Background Color (#F5EFE6)
    /// Layer 2: Outer Card Container around track (#EBE4D8)
    /// Layer 3: Inner Grid Surface beneath arrows (#FDFBF7)
    /// Card sizes are derived from PathManager.PathOffsetMultiplier so
    /// the mob path always flows through the exact center of the grey channel.
    /// 100% procedural — no external sprite assets required.
    /// </summary>
    public class MapContainerVisualizer : MonoBehaviour
    {
        [Header("Layer 1: Camera Background")]
        [SerializeField] private Color _cameraBackgroundColor = new Color(0.96f, 0.94f, 0.90f, 1f); // #F5EFE6
        [SerializeField] private Color _darkCameraBackgroundColor = new Color(0.10f, 0.10f, 0.18f, 1f); // #1A1A2E

        [Header("Layer 2: Outer Track Container Card")]
        [SerializeField] private Color _outerContainerColor = new Color(0.92f, 0.89f, 0.85f, 1f); // #EBE4D8
        [SerializeField] private Color _darkOuterContainerColor = new Color(0.065f, 0.080f, 0.135f, 1f); // #111422
        [SerializeField] private int _layer2SortingOrder = -10;

        [Header("Layer 3: Inner Grid Card Surface")]
        [SerializeField] private Color _innerGridColor = new Color(0.99f, 0.98f, 0.97f, 1f); // #FDFBF7
        [SerializeField] private Color _darkInnerGridColor = new Color(0.125f, 0.155f, 0.245f, 1f); // #20273E
        [SerializeField] private int _layer3SortingOrder = -9;

        [Header("Channel Margins")]
        [Tooltip("Distance from path center to inner card edge, in spacing units.")]
        [SerializeField] private float _innerMargin = 0.55f;
        [Tooltip("Distance from path center to outer card edge, in spacing units.")]
        [SerializeField] private float _outerMargin = 0.55f;

        private SpriteRenderer _outerContainerRenderer;
        private SpriteRenderer _innerGridRenderer;

        private void OnEnable()
        {
            GridManager.OnGridInitialized += HandleGridInitialized;
            Path.PathManager.OnPathInitialized += HandlePathInitialized;
            ThemeManager.OnThemeChanged += HandleThemeChanged;
        }

        private void OnDisable()
        {
            GridManager.OnGridInitialized -= HandleGridInitialized;
            Path.PathManager.OnPathInitialized -= HandlePathInitialized;
            ThemeManager.OnThemeChanged -= HandleThemeChanged;
        }

        private void HandleGridInitialized(int width, int height)
        {
            BuildThreeLayerTheme(width, height);
        }

        private void HandlePathInitialized()
        {
            if (GridManager.HasInstance)
            {
                BuildThreeLayerTheme(GridManager.Instance.Width, GridManager.Instance.Height);
            }
        }

        private void Start()
        {
            ApplyThemeColors(ThemeManager.IsDark);
        }

        private void HandleThemeChanged(Data.ThemeMode mode)
        {
            ApplyThemeColors(mode == Data.ThemeMode.Dark);
        }

        /// <summary>
        /// Updates camera background and card layer colors based on current theme.
        /// </summary>
        public void ApplyThemeColors(bool isDark)
        {
            UnityEngine.Camera mainCam = UnityEngine.Camera.main;
            if (mainCam != null)
            {
                mainCam.backgroundColor = isDark ? _darkCameraBackgroundColor : _cameraBackgroundColor;
            }

            if (_outerContainerRenderer != null)
            {
                _outerContainerRenderer.color = isDark ? _darkOuterContainerColor : _outerContainerColor;
            }

            if (_innerGridRenderer != null)
            {
                _innerGridRenderer.color = isDark ? _darkInnerGridColor : _innerGridColor;
            }
        }

        /// <summary>
        /// Builds the 3 visual layers procedurally based on current grid dimensions.
        /// Inner card = totalGrid + 2 * (pathOffset - innerMargin) * spacing
        /// Outer card = totalGrid + 2 * (pathOffset + outerMargin) * spacing
        /// This guarantees the mob path runs through the exact center of the grey channel.
        /// </summary>
        public void BuildThreeLayerTheme(int width, int height)
        {
            GridManager gm = GridManager.Instance;
            if (gm == null) return;

            float spacing = gm.PointSpacing;
            Vector2 origin = gm.Origin;

            float totalGridWidth = (width - 1) * spacing;
            float totalGridHeight = (height - 1) * spacing;
            Vector2 center = origin + new Vector2(totalGridWidth * 0.5f, totalGridHeight * 0.5f);

            float scaleFactor = DifficultyCalculator.GetMapScaleFactor(width, height);
            
            // Fixed tight card margin: card hugs the grid arrows directly
            float cardMargin = 0.50f * spacing;

            // Scaled track channel half-width: thick enough to fit scaled enemies
            float halfTrackWidth = 0.60f * scaleFactor * spacing;

            bool isDark = ThemeManager.IsDark;
            Color camColor = isDark ? _darkCameraBackgroundColor : _cameraBackgroundColor;
            Color outerColor = isDark ? _darkOuterContainerColor : _outerContainerColor;
            Color innerColor = isDark ? _darkInnerGridColor : _innerGridColor;

            // Layer 1: Apply Camera Background Color
            UnityEngine.Camera mainCam = UnityEngine.Camera.main;
            if (mainCam != null)
            {
                mainCam.backgroundColor = camColor;
            }

            // Layer 2: Outer Card — wraps outside the track channel
            float outerW = totalGridWidth + 2f * (cardMargin + 2f * halfTrackWidth);
            float outerH = totalGridHeight + 2f * (cardMargin + 2f * halfTrackWidth);
            EnsureCardLayer(ref _outerContainerRenderer, "Layer2_OuterCard", center,
                new Vector2(outerW, outerH), outerColor, _layer2SortingOrder, 40f);

            // Layer 3: Inner Card — wraps tightly right around the grid arrows
            float innerW = totalGridWidth + 2f * cardMargin;
            float innerH = totalGridHeight + 2f * cardMargin;
            EnsureCardLayer(ref _innerGridRenderer, "Layer3_InnerGridCard", center,
                new Vector2(innerW, innerH), innerColor, _layer3SortingOrder, 28f);
        }

        /// <summary>
        /// Creates or updates a card layer SpriteRenderer with the given parameters.
        /// </summary>
        private void EnsureCardLayer(ref SpriteRenderer renderer, string name, Vector2 center, Vector2 size, Color color, int sortingOrder, float cornerRadius)
        {
            Transform child = transform.Find(name);
            GameObject obj;
            if (child == null)
            {
                obj = new GameObject(name);
                obj.transform.SetParent(transform, false);
            }
            else
            {
                obj = child.gameObject;
            }

            if (renderer == null)
            {
                renderer = obj.GetComponent<SpriteRenderer>();
                if (renderer == null)
                {
                    renderer = obj.AddComponent<SpriteRenderer>();
                }
            }

            // Generate 100% procedural 9-sliced rounded rectangle sprite
            Sprite cardSprite = ProceduralSpriteUtility.CreateRoundedRectangleSprite(256, 256, cornerRadius, Color.white);
            renderer.sprite = cardSprite;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = size;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;

            obj.transform.position = new Vector3(center.x, center.y, 0f);
        }
    }
}
