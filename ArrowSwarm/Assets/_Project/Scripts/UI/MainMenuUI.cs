namespace ArrowSwarm.UI
{
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using TMPro;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>
    /// Controls the Main Menu screen: Play button, Leaderboard button,
    /// settings icon, and current level display.
    /// Features robust auto-wiring so missing references never break button clicks!
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _leaderboardButton;
        [SerializeField] private Button _levelsButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _titleText;

        [Header("Panels")]
        [SerializeField] private GameObject _leaderboardPanel;
        [SerializeField] private GameObject _levelsPanel;
        [SerializeField] private GameObject _settingsPanel;

        [Header("Animation")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeSpeed = 5f;

        private void Awake()
        {
            AutoWireUIReferences();
        }

        private void Start()
        {
            SetupUI();
            SetupButtons();
            AnimateIn();
        }

        /// <summary>
        /// Automatically finds and binds missing UI button references in hierarchy.
        /// </summary>
        public void AutoWireUIReferences()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            }

            var buttons = GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                if (btn == null) continue;
                string goName = btn.gameObject.name.ToLower();
                var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
                string txt = tmp != null ? tmp.text.ToLower() : "";

                if (goName.Contains("play") || txt.Contains("play") || btn.transform.Find("PlayImg") != null)
                {
                    _playButton = btn;
                    Debug.Log($"[ArrowSwarm] MainMenuUI: Auto-wired _playButton -> {btn.gameObject.name}");
                }
                else if (goName.Contains("level") || txt.Contains("level"))
                {
                    _levelsButton = btn;
                }
                else if (goName.Contains("setting") || txt.Contains("setting"))
                {
                    _settingsButton = btn;
                }
                else if (goName.Contains("leader") || txt.Contains("leader"))
                {
                    _leaderboardButton = btn;
                }
            }

            // Fallback: If _playButton is still null, pick the first button in hierarchy
            if (_playButton == null && buttons.Length > 0)
            {
                _playButton = buttons[0];
                Debug.Log($"[ArrowSwarm] MainMenuUI: Fallback assigned first button to _playButton -> {buttons[0].gameObject.name}");
            }
        }

        private void SetupUI()
        {
            int level = DataManager.Instance?.PlayerData?.currentLevel ?? 1;
            if (_levelText != null)
            {
                _levelText.text = $"Level: {level}";
            }

            if (_titleText != null)
            {
                _titleText.text = "ARROW SWARM";
            }
        }

        private void SetupButtons()
        {
            AutoWireUIReferences();

            if (_playButton != null)
            {
                _playButton.onClick.RemoveAllListeners();
                _playButton.onClick.AddListener(OnPlayClicked);
            }

            if (_leaderboardButton != null)
            {
                _leaderboardButton.onClick.RemoveAllListeners();
                _leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
            }

            if (_levelsButton != null)
            {
                _levelsButton.onClick.RemoveAllListeners();
                _levelsButton.onClick.AddListener(OnLevelsClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.RemoveAllListeners();
                _settingsButton.onClick.AddListener(OnSettingsClicked);
            }
        }

        private void OnPlayClicked()
        {
            Debug.Log("[ArrowSwarm] MainMenuUI: Play button clicked! Transitioning to GameScene...");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
            else
            {
                SceneManager.LoadScene("GameScene");
            }
        }

        private void OnLeaderboardClicked()
        {
            _leaderboardPanel?.SetActive(true);
        }

        private void OnLevelsClicked()
        {
            _levelsPanel?.SetActive(true);
        }

        private void OnSettingsClicked()
        {
            _settingsPanel?.SetActive(true);
        }

        private void AnimateIn()
        {
            if (_canvasGroup == null) return;
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        private void OnDestroy()
        {
            _playButton?.onClick.RemoveListener(OnPlayClicked);
            _leaderboardButton?.onClick.RemoveListener(OnLeaderboardClicked);
            _levelsButton?.onClick.RemoveListener(OnLevelsClicked);
            _settingsButton?.onClick.RemoveListener(OnSettingsClicked);
        }
    }
}
