namespace ArrowSwarm.Debug
{
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.SceneManagement;
#endif

    /// <summary>
    /// Boot scene & editor control panel to view, modify, and test all player stats,
    /// progress, profile, skills, and settings, and launch directly into gameplay.
    /// </summary>
    public class AdminManager : MonoBehaviour
    {
        public static AdminManager Instance { get; private set; }

        [Header("👤 Player Profile")]
        [SerializeField] private string _targetPlayerName = "Player";
        [SerializeField] private string _targetPlayerCountry = "US";
        [SerializeField] private bool _targetProfileCompleted = true;
        [SerializeField] private bool _targetTutorialCompleted = true;

        [Header("🏆 Progress & Stars")]
        [SerializeField] private int _targetLevel = 1;
        [SerializeField] private int _targetHighestLevel = 1;
        [SerializeField] private int _targetTotalStars = 0;

        [Header("⚡ Resources & Skills")]
        [SerializeField] private int _targetTips = 5;
        [SerializeField] private int _targetFreeze = 5;

        [Header("⚙️ Settings & Preferences")]
        [SerializeField] private string _targetLanguage = "ENGLISH";
        [SerializeField] private ThemeMode _targetTheme = ThemeMode.Light;
        [SerializeField] private bool _targetSfx = true;
        [SerializeField] private bool _targetVfx = true;
        [SerializeField] private bool _targetVibration = true;

        [Header("📊 Live Monitor (Read-Only)")]
        [SerializeField] private string _liveProfile = "";
        [SerializeField] private string _liveProgress = "";
        [SerializeField] private string _liveResources = "";

        public int TargetLevel { get => _targetLevel; set => _targetLevel = value; }
        public int TargetHighestLevel { get => _targetHighestLevel; set => _targetHighestLevel = value; }
        public int TargetTotalStars { get => _targetTotalStars; set => _targetTotalStars = value; }
        public int TargetTips { get => _targetTips; set => _targetTips = value; }
        public int TargetFreeze { get => _targetFreeze; set => _targetFreeze = value; }
        public string TargetPlayerName { get => _targetPlayerName; set => _targetPlayerName = value; }
        public string TargetPlayerCountry { get => _targetPlayerCountry; set => _targetPlayerCountry = value; }
        public bool TargetTutorialCompleted { get => _targetTutorialCompleted; set => _targetTutorialCompleted = value; }

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            DataManager.OnPlayerDataChanged += HandlePlayerDataChanged;
        }

        private void OnDisable()
        {
            DataManager.OnPlayerDataChanged -= HandlePlayerDataChanged;
        }

        private void Start()
        {
            PullFromPlayerData();
        }

        /// <summary>Pulls saved data from DataManager into inspector fields.</summary>
        [ContextMenu("📥 Pull Current Player Data")]
        public void PullFromPlayerData()
        {
            var data = DataManager.Instance?.PlayerData;
            if (data == null) return;

            _targetPlayerName = data.playerName;
            _targetPlayerCountry = data.playerCountry;
            _targetProfileCompleted = data.isProfileSetupCompleted;
            _targetTutorialCompleted = data.isTutorialCompleted;
            _targetLevel = data.currentLevel;
            _targetHighestLevel = data.highestLevel;
            _targetTotalStars = data.GetTotalStars();
            _targetTips = data.tipCount;
            _targetFreeze = data.freezeCount;
            _targetLanguage = data.selectedLanguage;
            _targetTheme = data.theme;
            _targetSfx = data.sfxEnabled;
            _targetVfx = data.vfxEnabled;
            _targetVibration = data.vibrationEnabled;

            RefreshLiveMonitor();
        }

        /// <summary>Applies and persists all edited stats to PlayerData.</summary>
        [ContextMenu("💾 Apply All Edits to PlayerData")]
        public void ApplyAllEdits()
        {
            if (DataManager.Instance == null) return;
            var dm = DataManager.Instance;

            dm.SetPlayerProfile(_targetPlayerName, _targetPlayerCountry, _targetProfileCompleted);
            dm.SetTutorialCompleted(_targetTutorialCompleted);
            dm.SetCurrentLevel(_targetLevel);
            dm.SetHighestLevel(_targetHighestLevel);
            dm.SetTotalStarsDirect(_targetTotalStars);
            dm.SetTipCount(_targetTips);
            dm.SetFreezeCount(_targetFreeze);
            dm.SetLanguage(_targetLanguage);
            dm.SetTheme(_targetTheme);
            dm.SetSFXEnabled(_targetSfx);
            dm.SetVFXEnabled(_targetVfx);
            dm.SetVibrationEnabled(_targetVibration);
            dm.Save();

            RefreshLiveMonitor();
            UnityEngine.Debug.Log($"[ArrowSwarm] AdminManager: Saved! Name={_targetPlayerName} [{_targetPlayerCountry}], Lv={_targetLevel}/{_targetHighestLevel}, Stars={_targetTotalStars}, Tips={_targetTips}, Freeze={_targetFreeze}");
        }

        /// <summary>Saves all configured stats and immediately loads GameScene.</summary>
        [ContextMenu("🚀 Save & Start Game")]
        public void SaveAndStartGame()
        {
            ApplyAllEdits();

            if (Application.isPlaying)
            {
                if (LevelManager.Instance != null) LevelManager.Instance.LoadLevel(_targetLevel);
                if (SceneTransitionManager.Instance != null) SceneTransitionManager.Instance.LoadScene("GameScene");
                else UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
            }
#if UNITY_EDITOR
            else
            {
                EditorSceneManager.OpenScene("Assets/_Project/Scenes/GameScene.unity");
                EditorApplication.isPlaying = true;
            }
#endif
        }

        /// <summary>Saves all configured stats and loads MainMenuScene.</summary>
        [ContextMenu("🏠 Save & Start Main Menu")]
        public void SaveAndStartMainMenu()
        {
            ApplyAllEdits();

            if (Application.isPlaying)
            {
                if (SceneTransitionManager.Instance != null) SceneTransitionManager.Instance.LoadScene("MainMenuScene");
                else UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
            }
#if UNITY_EDITOR
            else
            {
                EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainMenuScene.unity");
                EditorApplication.isPlaying = true;
            }
#endif
        }

        /// <summary>Deletes all saved player data and resets fields to default.</summary>
        [ContextMenu("🔄 Reset All Data")]
        public void ResetAllData()
        {
            DataManager.Instance?.DeleteAllData();
            PullFromPlayerData();
            UnityEngine.Debug.Log("[ArrowSwarm] AdminManager: All data reset to default.");
        }

        private void HandlePlayerDataChanged(PlayerData data) => RefreshLiveMonitor();

        private void RefreshLiveMonitor()
        {
            var d = DataManager.Instance?.PlayerData;
            if (d == null) return;
            _liveProfile = $"{d.playerName} [{d.playerCountry}] | ProfileDone: {d.isProfileSetupCompleted} | TutDone: {d.isTutorialCompleted}";
            _liveProgress = $"Level: {d.currentLevel} | Highest: {d.highestLevel} | Total Stars: {d.GetTotalStars()}";
            _liveResources = $"Tips: {d.tipCount} | Freeze: {d.freezeCount} | Lang: {d.selectedLanguage} | Theme: {d.theme}";
        }
    }
}
