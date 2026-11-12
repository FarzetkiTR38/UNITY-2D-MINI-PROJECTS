namespace ArrowSwarm.Puzzle
{
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>Scene-authored rounded board layers using the existing gameplay card theme.</summary>
    [DisallowMultipleComponent]
    public sealed class PuzzleBoardSurface : MonoBehaviour
    {
        [Header("Card Renderers")]
        [SerializeField] private SpriteRenderer _innerCard;

        [Header("Light Theme")]
        [SerializeField] private Color _innerGridColor = new Color(.99f, .98f, .97f, 1f);

        [Header("Dark Theme")]
        [SerializeField] private Color _darkInnerGridColor = new Color(.1254902f, .19607843f, .30980393f, 1f);

        [Header("Grid Card Geometry")]
        [SerializeField, Min(.01f)] private float _cardMargin = .5f;
        [SerializeField, Range(1f, 128f)] private float _innerCornerRadius = 28f;
        [SerializeField, Min(16)] private int _spriteResolution = 256;
        [Tooltip("Use the material's inlaid card shader instead of the shared sliced sprite.")]
        [SerializeField] private bool _useInlaidCard;

        private static readonly int BoardGeometry = Shader.PropertyToID("_BoardGeometry");
        private static Sprite _inlaidCardQuad;
        private MaterialPropertyBlock _cardProperties;

        private PuzzleLevel _displayedLevel;
        private int _screenWidth, _screenHeight;

        private void OnEnable()
        {
            PuzzleSession.OnChanged += Refresh;
            ThemeManager.OnThemeChanged += ApplyTheme;
            Refresh();
        }

        private void OnDisable()
        {
            PuzzleSession.OnChanged -= Refresh;
            ThemeManager.OnThemeChanged -= ApplyTheme;
        }

        private void Start() => Refresh();

        private void LateUpdate()
        {
            if (_screenWidth == Screen.width && _screenHeight == Screen.height) return;
            var session = PuzzleSession.Active;
            if (session == null || session.Level == null) return;
            _screenWidth = Screen.width; _screenHeight = Screen.height;
            var camera = ArrowSwarm.Camera.CameraController.Instance;
            if (camera != null) camera.FitToPuzzle(session.Level.Width, session.Level.Height);
        }

        /// <summary>Returns complete card dimensions without consulting Defense grid state.</summary>
        public Vector2 GetVisualSize(int width, int height)
        {
            var session = PuzzleSession.Active;
            float padding = session != null && session.Mode == GameModeId.ColorShift
                ? session.Config.WallOffset + session.Config.WallWidth * .5f : _cardMargin;
            return new Vector2(Mathf.Max(1, width) - 1, Mathf.Max(1, height) - 1) + Vector2.one * (2f * padding);
        }

        /// <summary>Resizes only the playable grid card; puzzle modes have no Defense path channel.</summary>
        public void Refresh()
        {
            var session = PuzzleSession.Active;
            if (session == null || session.Level == null) return;
            var level = session.Level;
            if (_displayedLevel != level)
            {
                _displayedLevel = level;
                Vector2 gridSize = new Vector2(level.Width - 1, level.Height - 1);
                float margin = session.Mode == GameModeId.ColorShift ? session.Config.WallOffset : _cardMargin;
                float radius = session.Mode == GameModeId.ColorShift
                    ? Mathf.Min(.8f, session.Config.WallOffset - session.Config.WallWidth * .5f) * 100f : _innerCornerRadius;
                Vector2 size = gridSize + Vector2.one * (2f * margin);
                if (_useInlaidCard) ResizeInlaidCard(size, margin, radius);
                else Resize(_innerCard, size, radius);
            }
            ApplyTheme(ThemeManager.CurrentTheme);
        }

        private void Resize(SpriteRenderer card, Vector2 size, float radius)
        {
            if (card == null) return;
            if (card.sprite == null)
                card.sprite = ProceduralSpriteUtility.CreateRoundedRectangleSprite(
                    _spriteResolution, _spriteResolution, radius, Color.white);
            card.drawMode = SpriteDrawMode.Sliced;
            card.size = size;
            card.transform.localPosition = Vector3.zero;
        }

        private void ResizeInlaidCard(Vector2 size, float margin, float radius)
        {
            if (_innerCard == null) return;
            if (_inlaidCardQuad == null)
            {
                var texture = Texture2D.whiteTexture;
                _inlaidCardQuad = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(.5f, .5f), texture.width, 0, SpriteMeshType.FullRect);
                _inlaidCardQuad.name = "Puzzle Board Unit Quad";
            }

            _innerCard.sprite = _inlaidCardQuad;
            _innerCard.drawMode = SpriteDrawMode.Simple;
            _innerCard.transform.localPosition = Vector3.zero;
            _innerCard.transform.localScale = new Vector3(size.x, size.y, 1f);

            // Radius follows the existing sliced sprite's 100 pixels-per-unit convention.
            _cardProperties ??= new MaterialPropertyBlock();
            _innerCard.GetPropertyBlock(_cardProperties);
            _cardProperties.SetVector(BoardGeometry, new Vector4(size.x, size.y, margin, radius / 100f));
            _innerCard.SetPropertyBlock(_cardProperties);
        }

        private void ApplyTheme(ThemeMode theme)
        {
            bool dark = theme == ThemeMode.Dark;
            if (_innerCard != null) _innerCard.color = dark ? _darkInnerGridColor : _innerGridColor;
        }
    }
}
