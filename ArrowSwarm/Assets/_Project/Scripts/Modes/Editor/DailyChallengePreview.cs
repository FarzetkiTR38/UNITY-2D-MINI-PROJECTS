namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using ArrowSwarm.Ads;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Puzzle;
    using ArrowSwarm.UI;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using DebugManager = ArrowSwarm.Debug.DebugManager;

    /// <summary>Plays one UTC day's existing board in an isolated Editor session.</summary>
    public static class DailyChallengePreview
    {
        private static readonly int[][] Levels = { new[] { 2, 3, 5, 10 }, new[] { 2, 5, 12, 16 }, new[] { 2, 3, 4, 5 } };
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static Session _session;

        public static Board? Active => _session?.Board;
        internal static bool IsIsolated => SessionState.GetBool("ArrowSwarm.Marketing.Armed", false);
        internal static bool Ready => Application.isPlaying && IsIsolated && DataManager.HasInstance &&
            SceneManager.GetActiveScene().name == "MainMenuScene" && !Transitioning;
        internal static bool Transitioning => SceneTransitionManager.HasInstance && SceneTransitionManager.Instance.IsTransitioning;

        public static Board Select(DateTimeOffset time)
        {
            var date = time.UtcDateTime.Date;
            int day = (date - new DateTime(2000, 1, 1)).Days;
            int mode = ((day % Levels.Length) + Levels.Length) % Levels.Length;
            var levels = Levels[mode];
            // A full cycle has one board from each mode. Each mode advances through its own selection.
            int rotation = (day - mode) / Levels.Length;
            int index = ((rotation % levels.Length) + levels.Length) % levels.Length;
            return new Board(date, (GameModeId)mode, levels[index], day);
        }

        public static void Start(DateTimeOffset time)
        {
            if (!Ready || _session != null || AdManager.Instance.IsShowingAd)
                throw new InvalidOperationException("Waiting for the main menu in isolated Play Mode.");

            var session = new Session(Select(time));
            _session = session;
            try { session.Start(); }
            catch { End(); throw; }
        }

        public static void Retry()
        {
            if (_session == null || Transitioning) return;
            if (Active.Value.Mode != GameModeId.Defense) { PuzzleSession.Active?.Retry(); return; }
            DataManager.Instance.PlayerData.currentLevel = Active.Value.Level;
            GameManager.Instance.RestartLevel();
        }

        private static void End()
        {
            var session = _session;
            _session = null;
            session?.Dispose();
        }

        public readonly struct Board
        {
            public DateTime Date { get; }
            public GameModeId Mode { get; }
            public int Level { get; }
            public int Seed { get; }
            private string Key => "ArrowSwarm.DailyPreview.v1." + Application.identifier + "." +
                Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            public int BestStars => EditorPrefs.GetInt(Key, 0);

            internal Board(DateTime date, GameModeId mode, int level, int seed)
            { Date = date; Mode = mode; Level = level; Seed = seed; }

            internal void Record(int lives)
            {
                int stars = Mathf.Clamp(lives, 0, 3);
                if (stars > BestStars) EditorPrefs.SetInt(Key, stars);
            }
        }

        private sealed class Session : IDisposable
        {
            private readonly QualityDataSnapshot _data;
            private readonly AdManager _ads = AdManager.Instance;
            private readonly IAdService _provider;
            private readonly DebugManager _debug = DebugManager.Instance;
            private readonly string _debugJson;
            private readonly bool _preview = PuzzleLaunch.Preview;
            private readonly GameModeId _previewMode = PuzzleLaunch.PreviewMode;
            private readonly int _previewLevel = PuzzleLaunch.PreviewLevel;
            private readonly bool _levelsOnLoad = MainMenuUI.OpenLevelsOnLoad;
            private readonly UnityEngine.Random.State _random = UnityEngine.Random.state;
            internal Board Board { get; }

            internal Session(Board board)
            {
                Board = board;
                _provider = (IAdService)typeof(AdManager).GetField("_adService", Private).GetValue(_ads);
                _debugJson = EditorJsonUtility.ToJson(_debug);
                _data = new QualityDataSnapshot(DataManager.Instance);
            }

            internal void Start()
            {
                var clone = JsonUtility.FromJson<PlayerData>(JsonUtility.ToJson(DataManager.Instance.PlayerData));
                clone.selectedGameMode = Board.Mode;
                clone.currentLevel = Board.Level;
                clone.isTutorialCompleted = true;
                clone.tipCount = clone.freezeCount = 1;
                _data.SetData(clone);
                _ads.SetAdService(new NoAds());
                var debug = new SerializedObject(_debug);
                debug.FindProperty("_useDebugLevel").boolValue = false;
                debug.FindProperty("_infiniteLives").boolValue = false;
                debug.FindProperty("_infiniteTips").boolValue = false;
                debug.FindProperty("_gameSpeedMultiplier").floatValue = 1f;
                debug.ApplyModifiedPropertiesWithoutUndo();

                SceneManager.sceneLoaded += SceneLoaded;
                EditorApplication.playModeStateChanged += PlayChanged;
                AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
                if (Board.Mode == GameModeId.Defense)
                {
                    PuzzleLaunch.Preview = false;
                    LevelManager.OnLevelLoading += SeedDefense;
                    GameManager.OnLevelWon += DefenseWon;
                    GameManager.Instance.StartGame();
                }
                else
                {
                    PuzzleLaunch.Preview = true;
                    PuzzleLaunch.PreviewMode = Board.Mode;
                    PuzzleLaunch.PreviewLevel = Board.Level;
                    PuzzleSession.OnChanged += PuzzleChanged;
                    GameModeSelection.Load(Board.Mode == GameModeId.Shapes ? "ShapesScene" : "ColorShiftScene");
                }
            }

            private void SeedDefense(int level)
            {
                if (level == Board.Level && SceneManager.GetActiveScene().name == "GameScene")
                    UnityEngine.Random.InitState(Board.Seed);
            }

            private void DefenseWon() => Board.Record(GameManager.Instance.CurrentLives);

            private void PuzzleChanged()
            {
                var puzzle = PuzzleSession.Active;
                if (puzzle != null && puzzle.Preview && puzzle.Mode == Board.Mode && puzzle.LevelNumber == Board.Level && puzzle.Won)
                    Board.Record(puzzle.Lives);
            }

            private static void SceneLoaded(Scene scene, LoadSceneMode mode)
            {
                if (scene.name == "MainMenuScene") { End(); return; }
                // Keep the available actions inside the daily board or return to Home.
                if (scene.name == "GameScene")
                {
                    HideButtons(UnityEngine.Object.FindFirstObjectByType<LevelCompleteUI>(FindObjectsInactive.Include),
                        "_nextLevelButton", "_levelsButton");
                    HideButtons(UnityEngine.Object.FindFirstObjectByType<PauseMenuUI>(FindObjectsInactive.Include), "_levelsButton");
                }
                else
                {
                    HideButtons(UnityEngine.Object.FindFirstObjectByType<PuzzlePopups>(FindObjectsInactive.Include),
                        "_next", "_winLevels", "_pauseLevels");
                }
            }

            private static void HideButtons(MonoBehaviour menu, params string[] names)
            {
                if (menu == null) return;
                var state = new SerializedObject(menu);
                foreach (var name in names)
                    if (state.FindProperty(name).objectReferenceValue is UnityEngine.UI.Button button) button.gameObject.SetActive(false);
            }

            private static void PlayChanged(PlayModeStateChange state)
            {
                if (state == PlayModeStateChange.ExitingPlayMode) End();
            }

            private static void BeforeReload()
            {
                EditorApplication.ExitPlaymode();
                End();
            }

            public void Dispose()
            {
                SceneManager.sceneLoaded -= SceneLoaded;
                EditorApplication.playModeStateChanged -= PlayChanged;
                AssemblyReloadEvents.beforeAssemblyReload -= BeforeReload;
                LevelManager.OnLevelLoading -= SeedDefense;
                GameManager.OnLevelWon -= DefenseWon;
                PuzzleSession.OnChanged -= PuzzleChanged;
                _ads.SetAdService(_provider);
                EditorJsonUtility.FromJsonOverwrite(_debugJson, _debug);
                PuzzleLaunch.Preview = _preview;
                PuzzleLaunch.PreviewMode = _previewMode;
                PuzzleLaunch.PreviewLevel = _previewLevel;
                MainMenuUI.OpenLevelsOnLoad = _levelsOnLoad;
                UnityEngine.Random.state = _random;
                _data.Dispose();
                _data.VerifyRestored(new List<string>());
            }
        }

        private sealed class NoAds : IAdService
        {
            public bool IsAdReady() => false;
            public bool IsInterstitialAdReady() => false;
            public void ShowRewardedAd(Action<bool> done) => done?.Invoke(false);
            public void ShowInterstitialAd(Action done) => done?.Invoke();
        }
    }
}
