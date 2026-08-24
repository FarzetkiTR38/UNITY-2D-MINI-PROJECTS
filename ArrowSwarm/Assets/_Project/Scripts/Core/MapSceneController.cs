namespace ArrowSwarm.Core
{
    using ArrowSwarm.Grid;
    using UnityEngine;

    /// <summary>
    /// Controls the unified Map Test Scene. Allows switching between all 12 maps
    /// from a single scene via Inspector buttons. Automatically synchronizes level numbers
    /// to load the correct map procedurally.
    /// </summary>
    public class MapSceneController : MonoBehaviour
    {
        [Header("Map Assets")]
        [Tooltip("Array of 12 MapData ScriptableObjects.")]
        [SerializeField] private MapData[] _mapDataAssets;

        [Header("Active Map & Level")]
        [Tooltip("Currently selected map index (0-11).")]
        [SerializeField] private int _activeMapIndex = 0;

        [Tooltip("Active level to load when starting, switching maps, or clicking Restart.")]
        [SerializeField] private int _defaultLevel = 1;

        /// <summary>Currently selected map index (0 to 11).</summary>
        public int ActiveMapIndex
        {
            get => _activeMapIndex;
            set
            {
                _activeMapIndex = Mathf.Clamp(value, 0, Mathf.Max(0, MapCount - 1));
                _defaultLevel = GetDefaultLevelForMapIndex(_activeMapIndex);
            }
        }

        /// <summary>Display name of the currently selected map.</summary>
        public string MapName => GetActiveMap()?.MapName ?? $"Map {_activeMapIndex + 1}";

        /// <summary>Default starting level for this scene.</summary>
        public int DefaultLevel
        {
            get => _defaultLevel;
            set
            {
                _defaultLevel = Mathf.Max(1, value);
                _activeMapIndex = DifficultyCalculator.GetMapIndex(_defaultLevel);
            }
        }

        /// <summary>Number of available map assets.</summary>
        public int MapCount => _mapDataAssets != null ? _mapDataAssets.Length : 0;

        /// <summary>Level range or sample levels for the current map.</summary>
        public Vector2Int CurrentLevelRange
        {
            get
            {
                if (_activeMapIndex < 5)
                {
                    return new Vector2Int(_activeMapIndex * 5 + 1, _activeMapIndex * 5 + 5);
                }
                return new Vector2Int(_defaultLevel, _defaultLevel + 4);
            }
        }

        /// <summary>
        /// Gets the representative level for each map index.
        /// </summary>
        public static int GetDefaultLevelForMapIndex(int mapIndex)
        {
            return mapIndex switch
            {
                0 => 1,   // Map 1 (Lv 1-5)
                1 => 6,   // Map 2 (Lv 6-10)
                2 => 11,  // Map 3 (Lv 11-15)
                3 => 16,  // Map 4 (Lv 16-20)
                4 => 21,  // Map 5 (Lv 21-25)
                5 => 30,  // Map 6 (Lv 30, 35, 40...)
                6 => 26,  // Map 7 (Lv 26, 31, 36...)
                7 => 27,  // Map 8 (Lv 27, 32, 37...)
                8 => 28,  // Map 9 (Lv 28, 33, 38...)
                9 => 29,  // Map 10 (Lv 29, 34, 39...)
                10 => 50, // Map 11 (Lv 50, 75, 125, 150...)
                11 => 100,// Map 12 (Lv 100, 200, 300...)
                _ => 1
            };
        }

        private void OnEnable()
        {
            EnsureMapAssets();
        }

        private void Start()
        {
            EnsureMapAssets();
            if (Application.isPlaying)
            {
                FitCameraToPreview();
            }
        }

        /// <summary>
        /// Ensures all 12 MapData assets are referenced.
        /// </summary>
        public void EnsureMapAssets()
        {
            if (_mapDataAssets != null && _mapDataAssets.Length == 12 && _mapDataAssets[0] != null)
            {
                return;
            }

            if (GameManager.HasInstance && GameManager.Instance.Config != null && GameManager.Instance.Config.Maps != null && GameManager.Instance.Config.Maps.Length >= 12)
            {
                _mapDataAssets = GameManager.Instance.Config.Maps;
                return;
            }

#if UNITY_EDITOR
            _mapDataAssets = new MapData[12];
            for (int i = 0; i < 12; i++)
            {
                string assetName = $"Map{i + 1}";
                string[] guids = UnityEditor.AssetDatabase.FindAssets($"{assetName} t:MapData");
                if (guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    _mapDataAssets[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<MapData>(path);
                }
            }
#endif
        }

        /// <summary>
        /// Returns the currently active MapData ScriptableObject.
        /// </summary>
        public MapData GetActiveMap()
        {
            EnsureMapAssets();
            if (_mapDataAssets == null || _mapDataAssets.Length == 0) return null;
            int idx = Mathf.Clamp(_activeMapIndex, 0, _mapDataAssets.Length - 1);
            return _mapDataAssets[idx];
        }

        /// <summary>
        /// Selects a map by index (0 to 11).
        /// Automatically adjusts the active level and fits the camera.
        /// </summary>
        public void SelectMap(int index)
        {
            ActiveMapIndex = index;
            FitCameraToPreview();

            if (Application.isPlaying && LevelManager.HasInstance)
            {
                LevelManager.Instance.LoadLevel(_defaultLevel);
            }
        }

        /// <summary>
        /// Loads a specific level.
        /// </summary>
        public void LoadLevel(int level)
        {
            DefaultLevel = level;
            FitCameraToPreview();

            if (Application.isPlaying && LevelManager.HasInstance)
            {
                LevelManager.Instance.LoadLevel(level);
            }
        }

        /// <summary>
        /// Restarts the current level on this map.
        /// </summary>
        [ContextMenu("🔄 Restart Level")]
        public void RestartCurrentLevel()
        {
            if (Application.isPlaying && LevelManager.HasInstance)
            {
                LevelManager.Instance.LoadLevel(_defaultLevel);
            }
        }

        /// <summary>Loads next level within or beyond this map.</summary>
        [ContextMenu("▶ Next Level")]
        public void NextMapLevel() => LoadLevel(_defaultLevel + 1);

        /// <summary>Loads previous level.</summary>
        [ContextMenu("◀ Previous Level")]
        public void PreviousMapLevel() => LoadLevel(Mathf.Max(1, _defaultLevel - 1));

        /// <summary>
        /// Adjusts camera position and orthographic size to preview the active map cleanly.
        /// </summary>
        public void FitCameraToPreview()
        {
            MapData map = GetActiveMap();
            if (map == null) return;

            UnityEngine.Camera cam = UnityEngine.Camera.main;
            if (cam == null) cam = FindFirstObjectByType<UnityEngine.Camera>();
            if (cam == null) return;

            float aspect = (Screen.height > 0) ? ((float)Screen.width / Screen.height) : (9f / 16f);
            if (float.IsNaN(aspect) || float.IsInfinity(aspect) || aspect <= 0.01f) aspect = 9f / 16f;

            float spacing = (Application.isPlaying && GridManager.HasInstance)
                ? GridManager.Instance.PointSpacing
                : 1.0f;

            float totalWidth = (map.GridWidth - 1) * spacing;
            float totalHeight = (map.GridHeight - 1) * spacing;
            Vector2 origin = new Vector2(-totalWidth / 2f, -totalHeight / 2f);
            Vector2 center = origin + new Vector2(totalWidth * 0.5f, totalHeight * 0.5f);

            float pathOffset = 1.10f;
            float outerMargin = 0.60f;
            float boardPadding = (pathOffset + outerMargin) * spacing;
            float visualBoardWidth = totalWidth + 2f * boardPadding;
            float visualBoardHeight = totalHeight + 2f * boardPadding;

            const float targetHeightRatio = 0.620f;
            const float targetWidthRatio = 0.880f;

            float orthoHeight = visualBoardHeight / (2f * targetHeightRatio);
            float orthoWidth = visualBoardWidth / (2f * aspect * targetWidthRatio);
            float orthoSize = Mathf.Max(orthoWidth, orthoHeight);
            if (float.IsNaN(orthoSize) || float.IsInfinity(orthoSize) || orthoSize <= 0.1f)
            {
                orthoSize = 10f;
            }

            cam.orthographicSize = orthoSize;
            float camZ = float.IsNaN(cam.transform.position.z) ? -10f : cam.transform.position.z;
            if (Mathf.Abs(camZ) < 0.1f) camZ = -10f;
            cam.transform.position = new Vector3(center.x, center.y, camZ);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            MapData map = GetActiveMap();
            if (map == null) return;

            float aspect = (float)Screen.width / Screen.height;
            if (aspect <= 0f) aspect = 9f / 16f;

            float s = (Application.isPlaying && GridManager.HasInstance)
                ? GridManager.Instance.PointSpacing
                : 1.0f;

            int w = map.GridWidth;
            int h = map.GridHeight;
            float totalW = (w - 1) * s;
            float totalH = (h - 1) * s;
            Vector2 origin = new Vector2(-totalW / 2f, -totalH / 2f);
            Vector2 center = origin + new Vector2(totalW * 0.5f, totalH * 0.5f);
            float pathOffset = 1.10f;

            // Outer Card (Layer 2)
            Gizmos.color = new Color(0.92f, 0.89f, 0.85f, 0.5f);
            Gizmos.DrawWireCube(center, new Vector3(totalW + 2f * (pathOffset + 0.60f) * s, totalH + 2f * (pathOffset + 0.60f) * s, 0f));

            // Inner Card (Layer 3)
            Gizmos.color = new Color(0.99f, 0.98f, 0.97f, 0.7f);
            Gizmos.DrawWireCube(center, new Vector3(totalW + 2f * (pathOffset - 0.60f) * s, totalH + 2f * (pathOffset - 0.60f) * s, 0f));

            // Path Rectangle
            Gizmos.color = new Color(0.55f, 0.48f, 0.41f, 0.8f);
            Vector3 tl = new Vector3(origin.x - pathOffset * s, origin.y + totalH + pathOffset * s, 0f);
            Vector3 tr = new Vector3(origin.x + totalW + pathOffset * s, origin.y + totalH + pathOffset * s, 0f);
            Vector3 br = new Vector3(origin.x + totalW + pathOffset * s, origin.y - pathOffset * s, 0f);
            Vector3 bl = new Vector3(origin.x - pathOffset * s, origin.y - pathOffset * s, 0f);
            Gizmos.DrawLine(tl, tr); Gizmos.DrawLine(tr, br); Gizmos.DrawLine(br, bl); Gizmos.DrawLine(bl, tl);

            // Grid Dots
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    Vector3 pos = new Vector3(origin.x + x * s, origin.y + y * s, 0f);
                    bool isEdge = (x == 0 || x == w - 1 || y == 0 || y == h - 1);
                    Gizmos.color = isEdge ? new Color(0.4f, 0.4f, 0.6f, 0.7f) : new Color(0.3f, 0.3f, 0.5f, 0.5f);
                    Gizmos.DrawSphere(pos, s * 0.05f);
                }
            }

            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(new Vector3(center.x, center.y + totalH * 0.5f + 1.2f, 0f), $"{map.MapName} ({w}×{h}) - Level {_defaultLevel}");
        }
#endif
    }
}
