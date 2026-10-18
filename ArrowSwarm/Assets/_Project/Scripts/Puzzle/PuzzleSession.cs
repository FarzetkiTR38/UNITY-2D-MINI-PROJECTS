namespace ArrowSwarm.Puzzle
{
    using System;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>Scene-owned puzzle lifecycle; never participates in Defense static combat events.</summary>
    public sealed partial class PuzzleSession : MonoBehaviour
    {
        [SerializeField] private PuzzleCatalog _catalog;
        [SerializeField] private PuzzleConfig _config;
        [SerializeField] private GameModeId _mode;
        [SerializeField] private PuzzleArrowView _arrowPrefab;
        [SerializeField] private Transform _arrowRoot;
        [SerializeField] private PuzzleFrame _frame;
        [SerializeField] private ArrowSwarm.Camera.CameraController _cameraController;
        private ObjectPool<PuzzleArrowView> _pool;
        private PuzzleArrowView[] _views;
        private bool[] _removed, _inFlight;
        private bool _busy, _awarded, _continued;
        private int _version, _activeShots;
        public static PuzzleSession Active { get; private set; }
        public static event Action OnChanged;
        public PuzzleLevel Level { get; private set; }
        public int LevelNumber { get; private set; }
        public int Lives { get; private set; }
        public int Turns { get; private set; }
        public int Orientation { get; private set; }
        public int Remaining { get; private set; }
        public bool Won { get; private set; }
        public bool Lost { get; private set; }
        public bool Paused { get; private set; }
        public bool Preview { get; private set; }
        public bool Ready => Level != null && Lives > 0 && !_busy && !Won && !Lost && !Paused && !HintPromptOpen &&
            (_mode == GameModeId.Shapes || !HasActiveShots);
        /// <summary>Whether normal arrows are still escaping or returning from a collision.</summary>
        public bool HasActiveShots => _activeShots > 0;
        /// <summary>Whether an exclusive skill can safely inspect or change stationary arrow visuals.</summary>
        public bool Idle => Ready && !HasActiveShots;
        public bool LastLevel => LevelNumber >= _catalog.Count;
        private string _messageKey, _messageFallback;
        public string Message => ArrowSwarm.Localization.LocalizationManager.Instance.GetText(_messageKey, _messageFallback);
        public int AttemptVersion => _version;
        public bool HintPromptOpen { get; private set; }
        public bool CanContinue => Lost && !_continued && !Preview;
        public PuzzleConfig Config => _config;
        public GameModeId Mode => _mode;

        private void Awake() { Active = this; }
        private void Start()
        {
            if (LevelManager.HasInstance) LevelManager.Instance.CleanupLevel();
            Preview = PuzzleLaunch.Preview && PuzzleLaunch.PreviewMode == _mode;
            LevelNumber = Preview ? PuzzleLaunch.PreviewLevel : DataManager.Instance.GetCurrentLevel(_mode);
            _pool = new ObjectPool<PuzzleArrowView>(_arrowPrefab, _arrowRoot, _config.PoolSize);
            LoadLevel(LevelNumber);
        }

        /// <summary>Loads catalog data, restores three lives and resets all transient actions.</summary>
        public void LoadLevel(int number)
        {
            StopAllCoroutines(); _activeShots = 0; _version++; Time.timeScale = 1f;
            if (_views != null) foreach (var view in _views) if (view != null && view.gameObject.activeSelf) _pool.Release(view);
            LevelNumber = number; Level = _catalog.Get(number);
            var solution = PuzzleSolver.Solve(Level);
            if (!solution.Solvable) { Level = null; SetMessage("", solution.Error); Debug.LogError("[ArrowSwarm] Invalid puzzle: " + Message); Notify(); return; }
            Lives = _config.Lives; Turns = Level.TurnBudget; Orientation = Level.InitialOrientation;
            Remaining = Level.Arrows.Count; Won = Lost = Paused = _busy = _awarded = _continued = false;
            HintPromptOpen = false;
            ResetRocket();
            _removed = new bool[Remaining]; _inFlight = new bool[Remaining]; _views = new PuzzleArrowView[Remaining];
            var origin = new Vector3(-(Level.Width - 1) * .5f, -(Level.Height - 1) * .5f, 0);
            for (int i = 0; i < _views.Length; i++) { _views[i] = _pool.Get(); _views[i].Bind(Level.Arrows[i], i, origin, _config); }
            _frame.Setup(Level, _config);
            _cameraController.FitToPuzzle(Level.Width,Level.Height);
            GameManager.Instance.SetState(GameState.Playing);
            SetMessage(number == 1 ? (_mode == GameModeId.Shapes ? "puzzle_shapes_help" : "puzzle_color_help") : "", "");
            Notify();
        }

        /// <summary>Retries without changing saved progress.</summary>
        public void Retry() => LoadLevel(LevelNumber);

        /// <summary>Pauses both movement and frame animation.</summary>
        public void Pause(bool value)
        {
            if (Won || Lost) return;
            RocketArmed = false;
            Paused = value; Time.timeScale = value ? 0f : 1f;
            GameManager.Instance.SetState(value ? GameState.Paused : GameState.Playing); Notify();
        }

        /// <summary>Returns to menu, cancelling all outstanding callbacks and preview state.</summary>
        public void Menu()
        {
            StopAllCoroutines(); _activeShots = 0; _busy = true;
            if (_inFlight != null) Array.Clear(_inFlight, 0, _inFlight.Length);
            ResetRocket();
            _version++; Time.timeScale = 1f; PuzzleLaunch.Preview = false;
            GameManager.Instance.GoToMainMenu();
        }

        /// <summary>Advances only inside this catalog; completing the MVP returns to its level list.</summary>
        public void Next()
        {
            if (!Won || _busy) return;
            if (Preview) { Advance(); return; }
            _busy=true;int version=_version;
            ArrowSwarm.Ads.AdManager.Instance.ShowInterstitialWithPacing(LevelNumber,()=>
            { if(this!=null && version==_version) { _busy=false;Advance(); } });
        }

        private void Advance()
        {
            if (LastLevel) { ArrowSwarm.UI.MainMenuUI.OpenLevelsOnLoad = true; Menu(); return; }
            if (!Preview) DataManager.Instance.SetCurrentLevel(_mode, LevelNumber + 1);
            LoadLevel(LevelNumber + 1);
        }

        private static void Notify() => OnChanged?.Invoke();
        private void SetMessage(string key, string fallback = "") { _messageKey = key; _messageFallback = fallback; }
        private void OnDestroy()
        {
            _version++; Time.timeScale = 1f;
            if (Active == this) Active = null;
        }
    }
}
