namespace ArrowSwarm.Debug
{
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using UnityEngine;

    /// <summary>
    /// Admin and developer control panel to view and modify player progress,
    /// star counts, levels, tip tokens, and game settings in real-time.
    /// Safe MonoBehaviour that never destroys parent GameObject.
    /// </summary>
    public class AdminManager : MonoBehaviour
    {
        private static AdminManager _instance;

        /// <summary>Global static accessor for AdminManager.</summary>
        public static AdminManager Instance => _instance;

        [Header("--- Level Controls ---")]
        [Tooltip("Target current level to apply.")]
        [SerializeField] private int _targetLevel = 1;

        [Tooltip("Target highest unlocked level to apply.")]
        [SerializeField] private int _targetHighestLevel = 1;

        [Header("--- Stars Controls ---")]
        [Tooltip("Set total star count directly (distributes 3 stars per level).")]
        [SerializeField] private int _targetTotalStars = 0;

        [Tooltip("Target level for single-level star modification.")]
        [SerializeField] private int _specificLevel = 1;

        [Tooltip("Stars to assign to Specific Level (0 to 3).")]
        [Range(0, 3)]
        [SerializeField] private int _starsForSpecificLevel = 3;

        [Header("--- Resources & Cheats ---")]
        [Tooltip("Target tip token balance.")]
        [SerializeField] private int _targetTips = 10;
        [Tooltip("Target player username.")]
        [SerializeField] private string _targetPlayerName = "Player";

        [Header("--- Live Monitor (Read-Only) ---")]
        [SerializeField] private string _livePlayerName;
        [SerializeField] private int _liveCurrentLevel;
        [SerializeField] private int _liveHighestLevel;
        [SerializeField] private int _liveTotalStars;
        [SerializeField] private int _liveTipCount;

        /// <summary>Target current level property.</summary>
        public int TargetLevel { get => _targetLevel; set => _targetLevel = value; }

        /// <summary>Target highest level property.</summary>
        public int TargetHighestLevel { get => _targetHighestLevel; set => _targetHighestLevel = value; }

        /// <summary>Target total stars property.</summary>
        public int TargetTotalStars { get => _targetTotalStars; set => _targetTotalStars = value; }

        /// <summary>Target tips count property.</summary>
        public int TargetTips { get => _targetTips; set => _targetTips = value; }

        /// <summary>Specific level to edit stars property.</summary>
        public int SpecificLevel { get => _specificLevel; set => _specificLevel = value; }

        /// <summary>Stars for specific level property.</summary>
        public int StarsForSpecificLevel { get => _starsForSpecificLevel; set => _starsForSpecificLevel = value; }

        private void Awake()
        {
            _instance = this;
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

        private void Update()
        {
            RefreshLiveMonitor();
        }

        private void HandlePlayerDataChanged(PlayerData data)
        {
            RefreshLiveMonitor();
        }

        /// <summary>
        /// Pulls current PlayerData into editable fields.
        /// </summary>
        [ContextMenu("📥 Pull Current Player Data")]
        public void PullFromPlayerData()
        {
            if (DataManager.Instance?.PlayerData == null) return;
            var data = DataManager.Instance.PlayerData;

            _targetLevel = data.currentLevel;
            _targetHighestLevel = data.highestLevel;
            _targetTotalStars = data.GetTotalStars();
            _targetTips = data.tipCount;
            _targetPlayerName = data.playerName;
            _specificLevel = data.currentLevel;
            _starsForSpecificLevel = data.GetStarsForLevel(data.currentLevel);

            RefreshLiveMonitor();
            LogDebug("Pulled current PlayerData to Admin fields.");
        }

        /// <summary>
        /// Applies all edited target values to PlayerData at once.
        /// </summary>
        [ContextMenu("💾 Apply All Edits to PlayerData")]
        public void ApplyAllEdits()
        {
            ApplyLevel();
            ApplyHighestLevel();
            ApplyTips();
            ApplyPlayerName();
            ApplyTotalStars();
            LogDebug("Applied all Admin edits to PlayerData.");
        }

        /// <summary>
        /// Applies the target current level.
        /// </summary>
        [ContextMenu("Apply Current Level")]
        public void ApplyLevel()
        {
            if (DataManager.Instance == null) return;
            DataManager.Instance.SetCurrentLevel(_targetLevel);
            LogDebug($"Set Current Level to {_targetLevel}");
        }

        /// <summary>
        /// Applies the target highest level.
        /// </summary>
        [ContextMenu("Apply Highest Level")]
        public void ApplyHighestLevel()
        {
            if (DataManager.Instance == null) return;
            DataManager.Instance.SetHighestLevel(_targetHighestLevel);
            LogDebug($"Set Highest Level to {_targetHighestLevel}");
        }

        /// <summary>
        /// Applies the target total star count.
        /// </summary>
        [ContextMenu("Apply Total Stars")]
        public void ApplyTotalStars()
        {
            if (DataManager.Instance == null) return;
            DataManager.Instance.SetTotalStarsDirect(_targetTotalStars);
            LogDebug($"Set Total Stars to {_targetTotalStars}");
        }

        /// <summary>
        /// Sets stars for the specified level.
        /// </summary>
        [ContextMenu("Apply Stars for Specific Level")]
        public void ApplyStarsForSpecificLevel()
        {
            if (DataManager.Instance == null) return;
            DataManager.Instance.ForceSetLevelStars(_specificLevel, _starsForSpecificLevel);
            LogDebug($"Set Level {_specificLevel} stars to {_starsForSpecificLevel}");
        }

        /// <summary>
        /// Applies the target tip token balance.
        /// </summary>
        [ContextMenu("Apply Tips Count")]
        public void ApplyTips()
        {
            if (DataManager.Instance == null) return;
            DataManager.Instance.SetTipCount(_targetTips);
            LogDebug($"Set Tips count to {_targetTips}");
        }

        /// <summary>
        /// Adds 10 tips immediately.
        /// </summary>
        [ContextMenu("💡 Add 10 Tips")]
        public void Add10Tips()
        {
            if (DataManager.Instance == null) return;
            DataManager.Instance.ModifyTipCount(10);
            _targetTips = DataManager.Instance.PlayerData?.tipCount ?? _targetTips;
            LogDebug("Added 10 Tips.");
        }

        /// <summary>
        /// Unlocks all levels up to the target level with 3 stars each.
        /// </summary>
        [ContextMenu("🌟 Unlock All Up To Target Level (3 Stars Each)")]
        public void UnlockAllWith3Stars()
        {
            if (DataManager.Instance == null) return;
            DataManager.Instance.UnlockLevelsWithStars(_targetHighestLevel, 3);
            LogDebug($"Unlocked all levels up to {_targetHighestLevel} with 3 stars.");
        }

        /// <summary>
        /// Applies the target player username.
        /// </summary>
        [ContextMenu("Apply Player Name")]
        public void ApplyPlayerName()
        {
            if (DataManager.Instance == null) return;
            DataManager.Instance.SetPlayerName(_targetPlayerName);
            LogDebug($"Set Player Name to {_targetPlayerName}");
        }

        /// <summary>
        /// Resets all player data to factory default.
        /// </summary>
        [ContextMenu("🔄 Reset All Player Data (Default)")]
        public void ResetAllData()
        {
            if (DataManager.Instance == null) return;
            DataManager.Instance.DeleteAllData();
            PullFromPlayerData();
            LogDebug("Reset all player data to default.");
        }

        private void RefreshLiveMonitor()
        {
            if (DataManager.Instance?.PlayerData == null) return;
            var data = DataManager.Instance.PlayerData;

            _livePlayerName = data.playerName;
            _liveCurrentLevel = data.currentLevel;
            _liveHighestLevel = data.highestLevel;
            _liveTotalStars = data.GetTotalStars();
            _liveTipCount = data.tipCount;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string msg)
        {
            UnityEngine.Debug.Log($"[ArrowSwarm] AdminManager: {msg}");
        }
    }
}
