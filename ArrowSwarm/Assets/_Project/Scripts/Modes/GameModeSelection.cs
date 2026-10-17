namespace ArrowSwarm.Modes
{
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using UnityEngine.SceneManagement;

    /// <summary>Routes explicit menu choices without altering legacy combat navigation.</summary>
    public static class GameModeSelection
    {
        public static GameModeId Selected
        {
            get
            {
                var value = DataManager.Instance?.PlayerData.selectedGameMode ?? GameModeId.Defense;
                return System.Enum.IsDefined(typeof(GameModeId), value) ? value : GameModeId.Defense;
            }
        }
        public static string DisplayName(GameModeId mode) => mode == GameModeId.ColorShift ? "COLOR SHIFT" : mode.ToString().ToUpperInvariant();

        /// <summary>Starts the chosen campaign; Defense retains its original start method.</summary>
        public static void PlaySelected()
        {
            PuzzleLaunch.Preview = false;
            if (Selected == GameModeId.Defense) { GameManager.Instance.StartGame(); return; }
            Load(Selected == GameModeId.Shapes ? "ShapesScene" : "ColorShiftScene");
        }

        /// <summary>Loads a scene using the existing transition service.</summary>
        public static void Load(string scene)
        {
            UnityEngine.Time.timeScale = 1f;
            if (SceneTransitionManager.Instance != null) SceneTransitionManager.Instance.LoadScene(scene);
            else SceneManager.LoadScene(scene);
        }
    }

    /// <summary>Transient editor preview context, deliberately excluded from saved player data.</summary>
    public static class PuzzleLaunch
    {
        public static bool Preview { get; set; }
        public static GameModeId PreviewMode { get; set; }
        public static int PreviewLevel { get; set; } = 1;
    }
}
