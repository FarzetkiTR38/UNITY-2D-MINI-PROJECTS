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
            _hint.interactable = s.Ready;
            _pause.interactable = !s.Won && !s.Lost && !s.HintPromptOpen;
            _rotationBar.SetActive(s.Mode == GameModeId.ColorShift);
            _turns.text = s.Turns.ToString();
            if (_rocket != null) _rocket.interactable = s.Ready && s.RocketCharges > 0;
            if (_rocketCount != null) _rocketCount.text = s.RocketCharges.ToString();
            if (_rotationButton != null) _rotationButton.interactable = s.Ready && s.Turns > 0;
            _instruction.text = Text("puzzle_swipe", "Swipe left / right to rotate");
        }
        private void OnDestroy()
        {
            _pause?.onClick.RemoveListener(Pause); _hint?.onClick.RemoveListener(Hint);
            _rocket?.onClick.RemoveListener(Rocket);
        }
    }
}
