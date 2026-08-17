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
        [SerializeField] private TextMeshProUGUI _starsText;
        [SerializeField] private TextMeshProUGUI _titleText;

        [Header("Panels")]
        [SerializeField] private GameObject _leaderboardPanel;
        [SerializeField] private GameObject _levelsPanel;
        [SerializeField] private GameObject _settingsPanel;

        [Header("Animation")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeSpeed = 5f;

        private void OnEnable()
        {
            DataManager.OnPlayerDataChanged += HandlePlayerDataChanged;
        }

        private void OnDisable()
        {
            DataManager.OnPlayerDataChanged -= HandlePlayerDataChanged;
        }

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

        private void HandlePlayerDataChanged(PlayerData data)
        {
            SetupUI();
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

            // Look directly under GameObject button group first
            var buttonGroup = transform.Find("GameObject");
            var searchRoot = buttonGroup != null ? buttonGroup : transform;

            if (_playButton == null)
            {
                var t = searchRoot.Find("PlayButton") ?? transform.Find("PlayButton");
                if (t != null) _playButton = t.GetComponent<Button>();
            }

            if (_levelsButton == null)
            {
                var t = searchRoot.Find("LevelsButton") ?? transform.Find("LevelsButton");
                if (t != null) _levelsButton = t.GetComponent<Button>();
            }

            if (_leaderboardButton == null)
            {
                var t = searchRoot.Find("LeaderboardButton") ?? transform.Find("LeaderboardButton");
                if (t != null) _leaderboardButton = t.GetComponent<Button>();
            }

            if (_settingsButton == null)
            {
                var t = searchRoot.Find("SettingsButton") ?? transform.Find("SettingsButton");
                if (t != null) _settingsButton = t.GetComponent<Button>();
            }

            // Fallback: If any are still null, search direct children of searchRoot
            if (_playButton == null || _levelsButton == null || _leaderboardButton == null || _settingsButton == null)
            {
                var buttons = searchRoot.GetComponentsInChildren<Button>(true);
                foreach (var btn in buttons)
                {
                    if (btn == null) continue;
                    // Ignore buttons inside child panels
                    if (btn.transform.IsChildOf(transform.Find("LevelsPanel") ?? transform) && btn.transform != transform)
                    {
                        var lp = transform.Find("LevelsPanel");
                        if (lp != null && btn.transform.IsChildOf(lp)) continue;
                    }
                    if (btn.transform.IsChildOf(transform.Find("LeaderboardPanel") ?? transform) && btn.transform != transform)
                    {
                        var lb = transform.Find("LeaderboardPanel");
                        if (lb != null && btn.transform.IsChildOf(lb)) continue;
                    }
                    if (btn.transform.IsChildOf(transform.Find("SettingsPanel") ?? transform) && btn.transform != transform)
                    {
                        var sp = transform.Find("SettingsPanel");
                        if (sp != null && btn.transform.IsChildOf(sp)) continue;
                    }

                    string goName = btn.gameObject.name.ToLower();
                    if (_playButton == null && (goName.Contains("play") || btn.transform.Find("PlayImg") != null))
                    {
                        _playButton = btn;
                    }
                    else if (_levelsButton == null && goName.Contains("level"))
                    {
                        _levelsButton = btn;
                    }
                    else if (_settingsButton == null && goName.Contains("setting"))
                    {
                        _settingsButton = btn;
                    }
                    else if (_leaderboardButton == null && goName.Contains("leader"))
                    {
                        _leaderboardButton = btn;
                    }
                }
            }

            // Auto-wire panels if null
            if (_levelsPanel == null)
            {
                var levelSelect = GetComponentInChildren<LevelSelectUI>(true);
                if (levelSelect != null) _levelsPanel = levelSelect.gameObject;
                else
                {
                    var t = transform.Find("LevelsPanel");
                    if (t != null) _levelsPanel = t.gameObject;
                }
            }

            if (_leaderboardPanel == null)
            {
                var leaderboard = GetComponentInChildren<LeaderboardUI>(true);
                if (leaderboard != null) _leaderboardPanel = leaderboard.gameObject;
                else
                {
                    var t = transform.Find("LeaderboardPanel");
                    if (t != null) _leaderboardPanel = t.gameObject;
                }
            }

            if (_settingsPanel == null)
            {
                var settings = GetComponentInChildren<SettingsUI>(true);
                if (settings != null) _settingsPanel = settings.gameObject;
                else
                {
                    var t = transform.Find("SettingsPanel");
                    if (t != null) _settingsPanel = t.gameObject;
                }
            }

            // Auto-wire star text if present
            if (_starsText == null)
            {
                var texts = GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in texts)
                {
                    if (t.gameObject.name.ToLower().Contains("star"))
                    {
                        _starsText = t;
                        break;
                    }
                }
            }
        }

        private void SetupUI()
        {
            int level = DataManager.Instance?.PlayerData?.highestLevel ?? 1;
            int totalStars = DataManager.Instance?.GetTotalStars() ?? 0;

            if (_levelText != null)
            {
                if (_starsText != null)
                {
                    _levelText.text = $"Level: {level}";
                    _starsText.text = $"{totalStars} ★";
                }
                else
                {
                    _levelText.text = $"Level: {level}   <color=#FFD700>★ {totalStars}</color>";
                }
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
