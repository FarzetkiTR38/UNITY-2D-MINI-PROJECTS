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
            if (_noInternetText != null) _noInternetText.gameObject.SetActive(false);

            var entries = LeaderboardManager.Instance?.GetTopPlayers(_rankTexts.Length);
            if (entries != null)
            {
                for (int i = 0; i < _rankTexts.Length && i < entries.Count; i++)
                {
                    if (_rankTexts[i] != null)
                    {
                        string colorHex = entries[i].IsPlayer ? "#FFD700" : "#FFFFFF"; // Gold color for player
                        _rankTexts[i].text = $"<color={colorHex}>#{i + 1}  {entries[i].PlayerName}  Lv.{entries[i].HighestLevel} ({entries[i].TotalStars}★)</color>";
                    }
                }
            }

            if (_playerRankText != null)
            {
                int playerRank = LeaderboardManager.Instance?.GetPlayerRank() ?? 999;
                string playerName = DataManager.Instance?.PlayerData?.playerName ?? "Player";
                _playerRankText.text = $"You: {playerName} - Rank: #{playerRank}";
            }
        }

        private void OnDestroy()
        {
            _backButton?.onClick.RemoveListener(Hide);
        }
    }
}
