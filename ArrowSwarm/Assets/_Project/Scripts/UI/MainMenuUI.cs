namespace ArrowSwarm.UI
{
    using System.Collections.Generic;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using TMPro;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>
    /// Controls the Main Menu UI navigation and panel transitions.
    /// Manages the main panel and sub-panels (Settings, Levels, Leaderboard, etc.),
    /// automatically switching between them and returning to main menu on exit.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Main Navigation Panels")]
        [Tooltip("The main menu panel (home screen) containing the primary buttons.")]
        [SerializeField] private GameObject _mainPanel;

        [Tooltip("Sub-panels opened from the main menu.")]
        [SerializeField] private GameObject _leaderboardPanel;
        [SerializeField] private GameObject _levelsPanel;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject[] _extraPanels;

        [Header("Main Menu Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _leaderboardButton;
        [SerializeField] private Button _levelsButton;
        [SerializeField] private Button _settingsButton;

        [Header("Exit / Back Buttons (Auto-detected if empty)")]
        [SerializeField] private Button[] _exitButtons;

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _starsText;
        [SerializeField] private TextMeshProUGUI _titleText;

        [Header("Profile Modal")]
        [SerializeField] private ProfileSetupModalUI _profileModal;

        [Header("Animation")]
        [SerializeField] private CanvasGroup _canvasGroup;

        private GameObject _activeSubPanel;

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
            OpenMainPanel();
        }

        /// <summary>Flag to open the Levels sub-panel directly upon loading MainMenuScene.</summary>
        public static bool OpenLevelsOnLoad { get; set; } = false;

        private void Start()
        {
            SetupUI();
            SetupButtons();

            if (OpenLevelsOnLoad)
            {
                OpenLevelsOnLoad = false;
                OpenLevels();
            }
            else
            {
                OpenMainPanel();
            }

            CheckInitialProfileSetup();
        }

        private void CheckInitialProfileSetup()
        {
            if (DataManager.Instance != null && DataManager.Instance.IsTutorialCompleted)
            {
                var data = DataManager.Instance.PlayerData;
                if (data != null && !data.isProfileSetupCompleted)
                {
                    if (_profileModal == null) _profileModal = GetComponentInChildren<ProfileSetupModalUI>(true);
                    if (_profileModal != null)
                    {
                        _profileModal.Show();
                    }
                }
            }
        }

        /// <summary>
        /// Opens the Profile setup / edit modal.
        /// </summary>
        public void OpenProfileModal()
        {
            if (_profileModal == null) _profileModal = GetComponentInChildren<ProfileSetupModalUI>(true);
            _profileModal?.Show();
        }

        private void HandlePlayerDataChanged(PlayerData data)
        {
            SetupUI();
        }

        /// <summary>
        /// Automatically discovers and links unassigned references and back buttons.
        /// </summary>
        public void AutoWireUIReferences()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            }

            // Auto-wire main panel if not assigned
            if (_mainPanel == null)
            {
                var main = transform.Find("MainPanel") ?? transform.Find("MainMenu") ??
                           transform.Find("HomePanel") ?? transform.Find("GameObject");
                if (main != null) _mainPanel = main.gameObject;
            }

            // Auto-wire sub-panels if not assigned
            if (_levelsPanel == null)
            {
                var comp = GetComponentInChildren<LevelSelectUI>(true);
                _levelsPanel = comp != null ? comp.gameObject : transform.Find("LevelsPanel")?.gameObject;
            }
            if (_leaderboardPanel == null)
            {
                var comp = GetComponentInChildren<LeaderboardUI>(true);
                _leaderboardPanel = comp != null ? comp.gameObject : transform.Find("LeaderboardPanel")?.gameObject;
            }
            if (_settingsPanel == null)
            {
                var comp = GetComponentInChildren<SettingsUI>(true);
                _settingsPanel = comp != null ? comp.gameObject : transform.Find("SettingsPanel")?.gameObject;
            }

            // Auto-wire main buttons if not assigned
            var searchRoot = _mainPanel != null ? _mainPanel.transform : transform;
            if (_playButton == null) _playButton = FindButton(searchRoot, "play");
            if (_levelsButton == null) _levelsButton = FindButton(searchRoot, "level");
            if (_leaderboardButton == null) _leaderboardButton = FindButton(searchRoot, "leader");
            if (_settingsButton == null) _settingsButton = FindButton(searchRoot, "setting");
            if (_levelText == null) _levelText = FindText(searchRoot, "level");
            if (_starsText == null) _starsText = FindText(searchRoot, "star");
        }

        private Button FindButton(Transform root, string keyword)
        {
            foreach (var btn in root.GetComponentsInChildren<Button>(true))
            {
                if (btn != null && btn.gameObject.name.ToLower().Contains(keyword))
                    return btn;
            }
            return null;
        }

        private TextMeshProUGUI FindText(Transform root, string keyword)
        {
            foreach (var txt in root.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (txt != null && txt.gameObject.name.ToLower().Contains(keyword))
                    return txt;
            }
            return null;
        }

        private void SetupUI()
        {
            int level = DataManager.Instance?.PlayerData?.highestLevel ?? 1;
            int totalStars = DataManager.Instance?.GetTotalStars() ?? 0;

            if (_levelText != null)
            {
                _levelText.text = $"Lv.{level}";
            }

            if (_starsText != null)
            {
                _starsText.text = totalStars.ToString();
            }

            if (_titleText != null)
            {
                _titleText.text = "ARROW SWARM";
            }
        }

        private void SetupButtons()
        {
            if (_playButton != null)
            {
                _playButton.onClick.RemoveAllListeners();
                _playButton.onClick.AddListener(OnPlayClicked);
            }
            if (_leaderboardButton != null)
            {
                _leaderboardButton.onClick.RemoveAllListeners();
                _leaderboardButton.onClick.AddListener(OpenLeaderboard);
            }
            if (_levelsButton != null)
            {
                _levelsButton.onClick.RemoveAllListeners();
                _levelsButton.onClick.AddListener(OpenLevels);
            }
            if (_settingsButton != null)
            {
                _settingsButton.onClick.RemoveAllListeners();
                _settingsButton.onClick.AddListener(OpenSettings);
            }

            WireExitButtons();
        }

        private void WireExitButtons()
        {
            if (_exitButtons != null)
            {
                foreach (var btn in _exitButtons)
                {
                    if (btn == null) continue;
                    btn.onClick.RemoveListener(BackToMain);
                    btn.onClick.AddListener(BackToMain);
                }
            }

            var allSubPanels = GetAllSubPanels();
            foreach (var panel in allSubPanels)
            {
                if (panel == null) continue;
                foreach (var btn in panel.GetComponentsInChildren<Button>(true))
                {
                    string btnName = btn.gameObject.name.ToLower();
                    if (btnName.Contains("back") || btnName.Contains("exit") || btnName.Contains("close") || btnName.Contains("return"))
                    {
                        btn.onClick.RemoveListener(BackToMain);
                        btn.onClick.AddListener(BackToMain);
                    }
                }
            }
        }

        /// <summary>
        /// Opens the specified panel and hides the main panel and any other sub-panels.
        /// </summary>
        /// <param name="targetPanel">The sub-panel to open.</param>
        public void OpenPanel(GameObject targetPanel)
        {
            if (targetPanel == null) return;

            SetPanelState(_mainPanel, false);

            if (_activeSubPanel != null && _activeSubPanel != targetPanel)
            {
                SetPanelState(_activeSubPanel, false);
            }

            SetPanelState(targetPanel, true);
            _activeSubPanel = targetPanel;
        }

        /// <summary>
        /// Closes all active sub-panels and brings the main panel back into view.
        /// </summary>
        public void OpenMainPanel()
        {
            var subPanels = GetAllSubPanels();
            foreach (var panel in subPanels)
            {
                SetPanelState(panel, false);
            }

            SetPanelState(_mainPanel, true);
            _activeSubPanel = null;
        }

        /// <summary>
        /// Alias for OpenMainPanel to bind to Back/Exit buttons.
        /// </summary>
        public void BackToMain()
        {
            OpenMainPanel();
        }

        /// <summary>Opens the Leaderboard panel.</summary>
        public void OpenLeaderboard() => OpenPanel(_leaderboardPanel);

        /// <summary>Opens the Levels panel.</summary>
        public void OpenLevels() => OpenPanel(_levelsPanel);

        /// <summary>Opens the Settings panel.</summary>
        public void OpenSettings() => OpenPanel(_settingsPanel);

        private void SetPanelState(GameObject panel, bool isVisible)
        {
            if (panel == null) return;

            if (isVisible)
            {
                if (panel.TryGetComponent<SettingsUI>(out var settings))
                    settings.Show();
                else if (panel.TryGetComponent<LeaderboardUI>(out var leaderboard))
                    leaderboard.Show();
                else if (panel.TryGetComponent<LevelSelectUI>(out var levelSelect))
                    levelSelect.Show();
                else
                    panel.SetActive(true);

                if (panel.TryGetComponent<CanvasGroup>(out var cg))
                {
                    cg.alpha = 1f;
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                }
            }
            else
            {
                if (panel.TryGetComponent<CanvasGroup>(out var cg))
                {
                    cg.alpha = 0f;
                    cg.interactable = false;
                    cg.blocksRaycasts = false;
                }
                panel.SetActive(false);
            }
        }

        private List<GameObject> GetAllSubPanels()
        {
            var list = new List<GameObject>();
            if (_leaderboardPanel != null) list.Add(_leaderboardPanel);
            if (_levelsPanel != null) list.Add(_levelsPanel);
            if (_settingsPanel != null) list.Add(_settingsPanel);

            if (_extraPanels != null)
            {
                foreach (var extra in _extraPanels)
                {
                    if (extra != null && !list.Contains(extra))
                        list.Add(extra);
                }
            }
            return list;
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

        private void OnDestroy()
        {
            _playButton?.onClick.RemoveListener(OnPlayClicked);
            _leaderboardButton?.onClick.RemoveListener(OpenLeaderboard);
            _levelsButton?.onClick.RemoveListener(OpenLevels);
            _settingsButton?.onClick.RemoveListener(OpenSettings);

            if (_exitButtons != null)
            {
                foreach (var btn in _exitButtons)
                    btn?.onClick.RemoveListener(BackToMain);
            }
        }
    }
}
