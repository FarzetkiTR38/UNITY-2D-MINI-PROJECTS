namespace ArrowSwarm.UI
{
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Puzzle;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Routes the original GameScene HUD to the isolated puzzle session.</summary>
    public sealed class PuzzleHUD : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _levelText, _arrowCountText, _hintCount, _turns, _instruction;
        [SerializeField] private Image[] _heartIcons;
        [SerializeField] private Button _pause, _hint, _rocket, _rotationButton;
        [SerializeField] private TextMeshProUGUI _rocketCount;
        [SerializeField] private GameObject _hintAdBadge, _rotationBar;
        [SerializeField] private HUDFeedbackEffects _feedback;
        [SerializeField] private CanvasGroup _lessonGroup;
        [SerializeField] private TextMeshProUGUI _lessonText;
        private int _attempt = -1, _lives = -1, _remaining = -1;

        private void Awake() { _feedback.Initialize(_heartIcons, _arrowCountText); }
        private void OnEnable()
        {
            PuzzleSession.OnChanged += Refresh;
            DataManager.OnPlayerDataChanged += DataChanged;
            LocalizationManager.OnLanguageChanged += Refresh;
        }
        private void OnDisable()
        {
            PuzzleSession.OnChanged -= Refresh;
            DataManager.OnPlayerDataChanged -= DataChanged;
            LocalizationManager.OnLanguageChanged -= Refresh;
        }
        private void Start()
        {
            _pause.onClick.AddListener(Pause);
            _hint.onClick.AddListener(Hint);
            if (_rocket != null) _rocket.onClick.AddListener(Rocket);
            Refresh();
        }
        private void Pause() => PuzzleSession.Active?.Pause(true);
        private void Hint() => PuzzleSession.Active?.Hint();
        private void Rocket() => PuzzleSession.Active?.ToggleRocket();
        private void DataChanged(PlayerData data) => Refresh();
        private static string Text(string key, string fallback) => LocalizationManager.Instance.GetText(key, fallback);
        private void Refresh()
        {
            var s = PuzzleSession.Active;
            if (s == null || s.Level == null) return;
            bool reset = _attempt != s.AttemptVersion || s.Lives > _lives;
            if (reset) _feedback.ResetFeedback(s.Lives);
            if (reset || s.Lives != _lives) _feedback.UpdateLives(s.Lives);
            if (!reset && s.Remaining < _remaining) _feedback.PunchArrowCount();
            _attempt = s.AttemptVersion; _lives = s.Lives; _remaining = s.Remaining;
            _levelText.text = Text("menu_level_short", "Lv.") + s.LevelNumber;
            _arrowCountText.text = s.RocketArmed ? Text("puzzle_rocket_pick", "Choose an arrow") :
                Text("hud_arrows", "Arrows") + ": " + (s.Level.Arrows.Count - s.Remaining) + "/" + s.Level.Arrows.Count;
            int count = DataManager.Instance.PlayerData.tipCount;
            _hintCount.text = count.ToString();
            _hintCount.transform.parent.gameObject.SetActive(count > 0);
            _hintAdBadge.SetActive(count <= 0 && !s.Preview);
            _hint.interactable = s.Idle;
            _pause.interactable = s.Lives > 0 && !s.Won && !s.Lost && !s.HintPromptOpen && !s.RocketPromptOpen;
            _rotationBar.SetActive(s.Mode == GameModeId.ColorShift);
            _turns.text = s.Turns.ToString();
            if (_rocket != null) _rocket.interactable = s.Idle && (s.RocketCharges > 0 || !s.Preview);
            if (_rocketCount != null) _rocketCount.text = s.RocketCharges.ToString();
            if (_rotationButton != null) _rotationButton.interactable = s.Ready && s.Turns > 0;
            _instruction.text = s.RocketNeeded ? Text("puzzle_rocket_needed", "Use the rocket to rescue an arrow") :
                Text("puzzle_swipe", "Tap or swipe either way · 1 turn");
            RefreshBoardNotice(s);
        }

        private void RefreshBoardNotice(PuzzleSession session)
        {
            if (_lessonGroup == null || _lessonText == null) return;
            bool onBoard = !session.Paused && !session.Won && !session.Lost &&
                !session.HintPromptOpen && !session.RocketPromptOpen;
            bool showFeedback = onBoard && (session.HasFailureMessage || session.HasRotationLesson);
            bool firstLesson = session.LevelNumber == 1 && !session.Preview && session.Ready &&
                session.Remaining == session.Level.Arrows.Count && session.Turns == session.Level.TurnBudget;
            _lessonGroup.alpha = showFeedback || firstLesson ? 1f : 0f;
            if (showFeedback) _lessonText.text = session.Message;
            else if (firstLesson)
            {
                _lessonText.text = session.Mode == GameModeId.Shapes
                    ? Text("puzzle_shapes_help", "Clear the shape. Blocked arrows cost a life.")
                    : Text("puzzle_color_help", "Match the arrow’s symbol to the exit wall.");
            }
        }
        private void OnDestroy()
        {
            _pause?.onClick.RemoveListener(Pause); _hint?.onClick.RemoveListener(Hint);
            _rocket?.onClick.RemoveListener(Rocket);
        }
    }
}
