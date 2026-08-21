namespace ArrowSwarm.Core
{
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Controls a standalone Map preview/gameplay scene (Map1..Map5).
    /// Sets the active level range, allows quick level jumping within this map,
    /// and provides scene switching shortcuts in Inspector.
    /// </summary>
    public class MapSceneController : MonoBehaviour
    {
        [Header("Map Configuration")]
        [Tooltip("The map index (0: Forest, 1: Ocean, 2: Desert, 3: Mountain, 4: Space).")]
        [SerializeField] private int _mapIndex = 0;

        [Tooltip("Name of the map theme.")]
        [SerializeField] private string _mapName = "Forest";

        [Tooltip("Default level to load when starting this scene.")]
        [SerializeField] private int _defaultLevel = 1;

        [Tooltip("Level range for this map (e.g. 1-5, 6-10, 11-15, 16-20, 21-25).")]
        [SerializeField] private Vector2Int _levelRange = new Vector2Int(1, 5);

        /// <summary>The map index (0 to 4).</summary>
        public int MapIndex { get => _mapIndex; set => _mapIndex = value; }

        /// <summary>Display name of the map theme.</summary>
        public string MapName { get => _mapName; set => _mapName = value; }

        /// <summary>Default starting level for this scene.</summary>
        public int DefaultLevel { get => _defaultLevel; set => _defaultLevel = value; }

        /// <summary>Min and max level range for this map.</summary>
        public Vector2Int LevelRange { get => _levelRange; set => _levelRange = value; }

        /// <summary>
        /// Loads a specific level within this map's range.
        /// </summary>
        public void LoadLevel(int level)
        {
            level = Mathf.Clamp(level, _levelRange.x, _levelRange.y);
            _defaultLevel = level;
            if (LevelManager.Instance != null)
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
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadLevel(_defaultLevel);
            }
        }

        /// <summary>
        /// Loads the next level within this map's range.
        /// </summary>
        [ContextMenu("▶ Next Level In This Map")]
        public void NextMapLevel()
        {
            int next = Mathf.Min(_defaultLevel + 1, _levelRange.y);
            LoadLevel(next);
        }

        /// <summary>
        /// Loads the previous level within this map's range.
        /// </summary>
        [ContextMenu("◀ Previous Level In This Map")]
        public void PreviousMapLevel()
        {
            int prev = Mathf.Max(_defaultLevel - 1, _levelRange.x);
            LoadLevel(prev);
        }

        /// <summary>
        /// Switch to another map scene by index (0-4).
        /// </summary>
        public void SwitchToMapScene(int targetMapIndex)
        {
            string[] sceneNames = new string[]
            {
                "Map1_ForestScene",
                "Map2_OceanScene",
                "Map3_DesertScene",
                "Map4_MountainScene",
                "Map5_SpaceScene"
            };

            if (targetMapIndex >= 0 && targetMapIndex < sceneNames.Length)
            {
                SceneManager.LoadScene(sceneNames[targetMapIndex]);
            }
        }
    }
}
