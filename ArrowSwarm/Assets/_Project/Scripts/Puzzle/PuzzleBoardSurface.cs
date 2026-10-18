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

        private PuzzleLevel _displayedLevel;

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
                Resize(_innerCard, gridSize + Vector2.one * (2f * margin), radius);
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

        private void ApplyTheme(ThemeMode theme)
        {
            bool dark = theme == ThemeMode.Dark;
            if (_innerCard != null) _innerCard.color = dark ? _darkInnerGridColor : _innerGridColor;
        }
    }
}
