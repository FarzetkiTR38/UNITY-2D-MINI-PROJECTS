namespace ArrowSwarm.UI
{
    using ArrowSwarm.Data;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Leaderboard screen showing top 10 players and current player rank.
    /// Uses ICloudService for data (mock in first phase).
    /// </summary>
    public class LeaderboardUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI[] _rankTexts; // Top 10 entries
        [SerializeField] private TextMeshProUGUI _playerRankText;
        [SerializeField] private TextMeshProUGUI _noInternetText;
        [SerializeField] private Button _backButton;

        private void Start()
        {
            _backButton?.onClick.AddListener(Hide);
            if (_titleText != null) _titleText.text = "LEADERBOARD";
            Hide();
        }

        /// <summary>
        /// Shows the leaderboard panel and loads data.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            LoadLeaderboardData();
        }

        /// <summary>
        /// Hides the leaderboard panel.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void LoadLeaderboardData()
        {
            // Mock data — will be replaced with ICloudService in Phase 7
            if (_noInternetText != null) _noInternetText.gameObject.SetActive(false);

            // Show mock entries
            string[] mockNames = { "ProPlayer", "ArrowKing", "SwarmMaster", "PathFinder",
                                   "GridLord", "MobSlayer", "QuickShot", "TipMaster",
                                   "LevelCrusher", "BugHunter" };
            int[] mockLevels = { 892, 756, 643, 521, 478, 412, 367, 298, 245, 189 };

            for (int i = 0; i < _rankTexts.Length && i < mockNames.Length; i++)
            {
                if (_rankTexts[i] != null)
                {
                    _rankTexts[i].text = $"#{i + 1}  {mockNames[i]}  Lv.{mockLevels[i]}";
                }
            }

            if (_playerRankText != null)
            {
                int playerLevel = DataManager.Instance?.PlayerData?.currentLevel ?? 1;
                string playerName = DataManager.Instance?.PlayerData?.playerName ?? "Player";
                _playerRankText.text = $"You: {playerName}  Lv.{playerLevel}";
            }
        }

        private void OnDestroy()
        {
            _backButton?.onClick.RemoveListener(Hide);
        }
    }
}
