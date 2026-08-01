namespace ArrowSwarm.Grid
{
    using ArrowSwarm.Core;
    using UnityEngine;

    /// <summary>
    /// Draws the grid lines visually using a LineRenderer or GL.
    /// Subscribes to GridManager events to know when to draw.
    /// </summary>
    public class GridVisualizer : MonoBehaviour
    {
        [SerializeField] private float _lineWidth = 0.02f;
        [SerializeField] private Material _lineMaterial;
        [SerializeField] private int _sortingOrder = -1;

        private GridManager _gridManager;

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
            DrawGrid();
        }

        /// <summary>
        /// Creates line renderers for all grid lines.
        /// </summary>
        private void DrawGrid()
        {
            // Clear existing lines
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            int width = _gridManager.Width;
            int height = _gridManager.Height;
            float cellSize = _gridManager.CellSize;
            Vector2 origin = _gridManager.Origin;

            MapData mapData = GameManager.Instance?.Config?.GetMapForLevel(
                Data.DataManager.Instance?.PlayerData?.currentLevel ?? 1);
            Color lineColor = mapData != null ? mapData.GridLineColor
                : new Color(0.16f, 0.16f, 0.29f, 1f);

            // Vertical lines
            for (int x = 0; x <= width; x++)
            {
                CreateLine(
                    new Vector2(origin.x + x * cellSize, origin.y),
                    new Vector2(origin.x + x * cellSize, origin.y + height * cellSize),
                    lineColor, $"VLine_{x}"
                );
            }

            // Horizontal lines
            for (int y = 0; y <= height; y++)
            {
                CreateLine(
                    new Vector2(origin.x, origin.y + y * cellSize),
                    new Vector2(origin.x + width * cellSize, origin.y + y * cellSize),
                    lineColor, $"HLine_{y}"
                );
            }
        }

        private void CreateLine(Vector2 start, Vector2 end, Color color, string name)
        {
            var lineObj = new GameObject(name);
            lineObj.transform.SetParent(transform, false);

            var lr = lineObj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            lr.startWidth = _lineWidth;
            lr.endWidth = _lineWidth;
            lr.startColor = color;
            lr.endColor = color;
            lr.sortingOrder = _sortingOrder;
            lr.useWorldSpace = true;

            if (_lineMaterial != null)
            {
                lr.material = _lineMaterial;
            }
            else
            {
                lr.material = new Material(Shader.Find("Sprites/Default"));
            }
        }
    }
}
