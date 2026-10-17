namespace ArrowSwarm.UI
{
    using System;
    using ArrowSwarm.Audio;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.Puzzle;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    /// <summary>Routes the existing gameplay popup artwork to the isolated puzzle session.</summary>
    [DisallowMultipleComponent]
    public sealed partial class PuzzlePopups : MonoBehaviour
    {
        [Header("Pause, Win, Lose, Hint")]
        [SerializeField] private CanvasGroup[] _panels;
        [SerializeField] private RectTransform[] _dialogs;
        [SerializeField] private GameObject[] _winStars;
        [SerializeField] private TMP_Text _winLevel, _loseLevel, _nextLabel;
        [Header("Pause Actions")]
        [SerializeField] private Button _resume, _pauseRetry, _pauseLevels, _pauseMenu;
        [SerializeField] private SettingsToggleUI _soundToggle, _vibrationToggle;
        [Header("Result Actions")]
        [SerializeField] private Button _next, _winLevels, _winMenu, _loseRetry, _loseMenu, _continue;
        [Header("Hint Prompt")]
        [SerializeField] private Button _watchHint, _closeHint;
        private int _attemptVersion = int.MinValue;

        private void Awake() => InitializePanels();
        private void OnEnable()
        {
            PuzzleSession.OnChanged += Refresh;
            LocalizationManager.OnLanguageChanged += Refresh;
            WireButtons(true);
            if (_soundToggle != null) _soundToggle.OnValueChanged += SoundChanged;
            if (_vibrationToggle != null) _vibrationToggle.OnValueChanged += VibrationChanged;
            Refresh();
        }
        private void Start() => Refresh();
        private void OnDisable()
        {
            PuzzleSession.OnChanged -= Refresh;
            LocalizationManager.OnLanguageChanged -= Refresh;
            WireButtons(false);
            if (_soundToggle != null) _soundToggle.OnValueChanged -= SoundChanged;
            if (_vibrationToggle != null) _vibrationToggle.OnValueChanged -= VibrationChanged;
            ResetPanels();
        }

        private void WireButtons(bool listen)
        {
            Listen(_resume, Resume, listen); Listen(_pauseRetry, Retry, listen);
            Listen(_pauseLevels, Levels, listen); Listen(_pauseMenu, Menu, listen);
            Listen(_next, Next, listen); Listen(_winLevels, Levels, listen); Listen(_winMenu, Menu, listen);
            Listen(_loseRetry, Retry, listen); Listen(_loseMenu, Menu, listen); Listen(_continue, Continue, listen);
            Listen(_watchHint, WatchHint, listen); Listen(_closeHint, CloseHint, listen);
        }
        private static void Listen(Button button, UnityAction action, bool listen)
        {
            if (button == null) return;
            if (listen) button.onClick.AddListener(action); else button.onClick.RemoveListener(action);
        }

        /// <summary>Refreshes result data and popup visibility without awarding progress.</summary>
        public void Refresh()
        {
            var session = PuzzleSession.Active;
            if (session == null || session.Level == null) return;
            if (_attemptVersion != session.AttemptVersion)
            {
                ResetPanels();
                _attemptVersion = session.AttemptVersion;
            }
            bool openingWin = session.Won && !_visible[1];
            if (session.Paused && !_visible[0]) RefreshToggles();
            SetPanelVisible(0, session.Paused && !session.Won && !session.Lost);
            SetPanelVisible(1, session.Won);
            SetPanelVisible(2, session.Lost);
            SetPanelVisible(3, session.HintPromptOpen && !session.Won && !session.Lost && !session.Paused);
            if (_winLevel != null) _winLevel.text = FormatLevel("puzzle_level_cleared", "Level {0} Cleared", session.LevelNumber);
            if (_loseLevel != null) _loseLevel.text = FormatLevel("puzzle_level_failed", "Level {0} Failed", session.LevelNumber);
            if (_nextLabel != null)
            {
                string key = session.LastLevel ? "win_levels" : "win_next_level";
                var localized = _nextLabel.GetComponent<LocalizedText>();
                if (localized != null) localized.SetKey(key);
                else _nextLabel.text = Translate(key, session.LastLevel ? "LEVELS" : "NEXT LEVEL");
            }
            if (_continue != null)
            {
                _continue.gameObject.SetActive(session.CanContinue && !session.Preview);
                _continue.interactable = session.CanContinue && !session.Preview;
            }
            if (_watchHint != null) _watchHint.interactable = session.HintPromptOpen;
            if (openingWin) AnimateWinStars(Mathf.Clamp(session.Lives, 0, 3));
        }

        private void RefreshToggles()
        {
            var data = DataManager.Instance?.PlayerData;
            if (data == null) return;
            _soundToggle?.SetIsOn(data.sfxEnabled, false);
            _vibrationToggle?.SetIsOn(data.vibrationEnabled, false);
        }
        private static string Translate(string key, string fallback) =>
            LocalizationManager.Instance != null ? LocalizationManager.Instance.GetText(key, fallback) : fallback;
        private static string FormatLevel(string key, string fallback, int level)
        {
            string template = Translate(key, fallback);
            try { return string.Format(template, level); }
            catch (FormatException) { return string.Format(fallback, level); }
        }
        private static void Click() => AudioManager.Instance?.PlayButtonClick();
        private void Resume() { Click(); PuzzleSession.Active?.Pause(false); }
        private void Retry() { Click(); PuzzleSession.Active?.Retry(); }
        private void Menu() { Click(); PuzzleSession.Active?.Menu(); }
        private void Next() { Click(); PuzzleSession.Active?.Next(); }
        private void Continue() { Click(); PuzzleSession.Active?.Continue(); }
        private void WatchHint() { Click(); PuzzleSession.Active?.WatchHintAd(); }
        private void CloseHint() { Click(); PuzzleSession.Active?.CloseHintPrompt(); }
        private void Levels()
        {
            Click(); MainMenuUI.OpenLevelsOnLoad = true; PuzzleSession.Active?.Menu();
        }
        private void SoundChanged(bool value)
        {
            AudioManager.Instance?.PlayToggle(); DataManager.Instance?.SetSFXEnabled(value);
        }
        private void VibrationChanged(bool value)
        {
            AudioManager.Instance?.PlayToggle(); DataManager.Instance?.SetVibrationEnabled(value);
        }
    }
}
