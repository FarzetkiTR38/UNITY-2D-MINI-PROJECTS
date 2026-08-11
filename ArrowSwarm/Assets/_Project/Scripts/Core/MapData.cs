namespace ArrowSwarm.Core
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// ScriptableObject that defines a map's layout: point grid dimensions,
    /// path waypoints, spawn/finish points, and visual properties.
    /// 5 different MapData assets will be created (one per map theme).
    /// </summary>
    [CreateAssetMenu(fileName = "MapData", menuName = "ArrowSwarm/MapData")]
    public class MapData : ScriptableObject
    {
        [Header("Map Info")]
        [SerializeField] private string _mapName;
        [SerializeField] private int _mapIndex;

        [Header("Point Grid Configuration")]
        [SerializeField] private int _gridWidth = 6;
        [SerializeField] private int _gridHeight = 8;
        [SerializeField] private float _pointSpacing = 0.8f;
        [SerializeField] private Vector2 _gridOrigin = Vector2.zero;

        [Header("Path")]
        [SerializeField] private List<Vector2> _pathWaypoints = new List<Vector2>();
        [SerializeField] private Vector2 _spawnPoint;
        [SerializeField] private Vector2 _finishPoint;

        [Header("Visuals")]
        [SerializeField] private Sprite _backgroundSprite;
        [SerializeField] private Color _pathColor = new Color(0.55f, 0.48f, 0.41f, 1f); // #8C7A68 High Contrast Dark Taupe
        [SerializeField] private Color _gridLineColor = new Color(0.85f, 0.81f, 0.75f, 1f); // #D9CFBF
        [SerializeField] private Color _cameraBackgroundColor = new Color(0.96f, 0.94f, 0.90f, 1f); // #F5EFE6
        [SerializeField] private Color _outerContainerColor = new Color(0.92f, 0.89f, 0.85f, 1f); // #EBE4D8
        [SerializeField] private Color _innerGridColor = new Color(0.99f, 0.98f, 0.97f, 1f); // #FDFBF7

        // --- Properties ---
        /// <summary>Display name of this map.</summary>
        public string MapName => _mapName;

        /// <summary>Index of this map (0-4).</summary>
        public int MapIndex => _mapIndex;

        /// <summary>Grid width in point columns.</summary>
        public int GridWidth => _gridWidth;

        /// <summary>Grid height in point rows.</summary>
        public int GridHeight => _gridHeight;

        /// <summary>Distance between adjacent points in world units.</summary>
        public float PointSpacing => _pointSpacing;

        /// <summary>Bottom-left origin point of the grid in world space.</summary>
        public Vector2 GridOrigin => _gridOrigin;

        /// <summary>Ordered path waypoints (counter-clockwise).</summary>
        public IReadOnlyList<Vector2> PathWaypoints => _pathWaypoints;

        /// <summary>World position where mobs spawn.</summary>
        public Vector2 SpawnPoint => _spawnPoint;

        /// <summary>World position mobs try to reach.</summary>
        public Vector2 FinishPoint => _finishPoint;

        /// <summary>Optional background sprite for this map.</summary>
        public Sprite BackgroundSprite => _backgroundSprite;

        /// <summary>Color used to render the mob path.</summary>
        public Color PathColor => _pathColor;

        /// <summary>Color used to render grid dots/lines.</summary>
        public Color GridLineColor => _gridLineColor;

        /// <summary>Camera background color (Layer 1).</summary>
        public Color CameraBackgroundColor => _cameraBackgroundColor;

        /// <summary>Outer container card color (Layer 2).</summary>
        public Color OuterContainerColor => _outerContainerColor;

        /// <summary>Inner grid card surface color (Layer 3).</summary>
        public Color InnerGridColor => _innerGridColor;

        /// <summary>Top gradient color for the background (legacy fallback to CameraBackgroundColor).</summary>
        public Color BackgroundGradientTop => _cameraBackgroundColor;

        /// <summary>Bottom gradient color for the background (legacy fallback to OuterContainerColor).</summary>
        public Color BackgroundGradientBottom => _outerContainerColor;

        /// <summary>
        /// Total number of points in this map's grid.
        /// </summary>
        public int TotalPoints => _gridWidth * _gridHeight;

        // --- Backward Compatibility ---
        /// <summary>Cell size — maps to PointSpacing for backward compatibility.</summary>
        public float CellSize => _pointSpacing;
    }
}
