namespace ArrowSwarm.Modes
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>Ordered grid path, first point is the head; color is visual and a wall key.</summary>
    [Serializable]
    public sealed class PuzzleArrowData
    {
        [SerializeField] private List<Vector2Int> _points = new List<Vector2Int>();
        [SerializeField] private Vector2Int _direction = Vector2Int.up;
        [SerializeField] private Color _color = Color.cyan;
        [SerializeField] private int _colorId;
        public List<Vector2Int> Points => _points;
        public Vector2Int Direction => _direction;
        public Color Color => _color;
        public int ColorId => _colorId;
        public Vector2Int Head => _points[0];

        /// <summary>Constructs editable authoring data without runtime objects.</summary>
        public PuzzleArrowData(List<Vector2Int> points, Vector2Int direction, Color color, int colorId = 0)
        { _points = points; _direction = direction; _color = color; _colorId = colorId; }
    }

    /// <summary>Offline authored and validated puzzle, independent of the Defense generator.</summary>
    [CreateAssetMenu(menuName = "Arrow Swarm/Puzzle Level")]
    public sealed class PuzzleLevel : ScriptableObject
    {
        [SerializeField] private GameModeId _mode = GameModeId.Shapes;
        [SerializeField] private string _title = "New Shape";
        [SerializeField] private int _width = 24;
        [SerializeField] private int _height = 24;
        [SerializeField] private List<PuzzleArrowData> _arrows = new List<PuzzleArrowData>();
        [SerializeField] private Color[] _wallColors = { new Color(.1f,.7f,.75f), new Color(.95f,.62f,.2f) };
        [SerializeField, Min(0)] private int _turnBudget = 5;
        [SerializeField, Range(0, 1)] private int _initialOrientation;
        [SerializeField] private Texture2D _sourceImage;
        public GameModeId Mode => _mode;
        public string Title => _title;
        public int Width => _width;
        public int Height => _height;
        public List<PuzzleArrowData> Arrows => _arrows;
        public Color[] WallColors => _wallColors;
        public int TurnBudget => _turnBudget;
        public int InitialOrientation => _initialOrientation;
        public Texture2D SourceImage => _sourceImage;

        /// <summary>Sets generated content; editor owns all authored assets.</summary>
        public void Author(GameModeId mode, string title, int width, int height, List<PuzzleArrowData> arrows,
            Color[] palette = null, int turns = 5, Texture2D source = null)
        {
            _mode = mode; _title = title; _width = width; _height = height; _arrows = arrows;
            if (palette != null) _wallColors = palette;
            _turnBudget = turns; _initialOrientation = 0; _sourceImage = source;
        }
    }
}
