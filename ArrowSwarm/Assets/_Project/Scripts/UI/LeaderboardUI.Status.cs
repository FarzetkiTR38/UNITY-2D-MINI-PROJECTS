namespace ArrowSwarm.UI
{
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Localized connection status and retry controls use scene-authored visuals in both themes.</summary>
    public partial class LeaderboardUI
    {
        [Header("Connection & Local Progress")]
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _updatedText;
        [SerializeField] private TextMeshProUGUI _emptyText;
        [SerializeField] private TextMeshProUGUI _retryText;
        [SerializeField] private TextMeshProUGUI _playerSummaryText;
        [SerializeField] private Button _retryButton;

        /// <summary>Retries the existing refresh path; a running request retains ownership of its result.</summary>
        public void Retry()
        {
            if (!isActiveAndEnabled) return;
            var manager = LeaderboardManager.Instance;
            if (manager == null || manager.RefreshState == LeaderboardRefreshState.Loading) return;
            _ = manager.RefreshFromCloudAsync();
        }

        private void RefreshStatus(LeaderboardManager manager, bool hasRows)
        {
            var state = manager != null ? manager.RefreshState : LeaderboardRefreshState.NotRequested;
            string key = state == LeaderboardRefreshState.Loading ? "leaderboard_loading" :
                state == LeaderboardRefreshState.Ready ? "leaderboard_current" :
                state == LeaderboardRefreshState.Offline ? "leaderboard_offline" :
                state == LeaderboardRefreshState.Failed ? "leaderboard_failed" : "leaderboard_waiting";
            if (_statusText != null) _statusText.text = Text(key, "Leaderboard");
            if (_retryText != null) _retryText.text = Text("leaderboard_retry", "REFRESH");
            if (_retryButton != null) _retryButton.interactable = state != LeaderboardRefreshState.Loading;
            if (_emptyText != null)
            {
                _emptyText.gameObject.SetActive(!hasRows);
                _emptyText.text = Text(state == LeaderboardRefreshState.Ready ? "leaderboard_empty" : "leaderboard_no_cache",
                    "Global results will appear here.");
            }
            if (_updatedText != null)
            {
                var updated = manager?.LastSuccessfulRefreshUtc;
                _updatedText.text = updated.HasValue
                    ? Format("leaderboard_updated", "Saved results • {0}", updated.Value.ToLocalTime().ToString("g"))
                    : hasRows ? Text("leaderboard_cached", "Saved results") : Text("leaderboard_local", "Your progress is saved on this device.");
            }
        }

        private static string Text(string key, string fallback) => LocalizationManager.HasInstance
            ? LocalizationManager.Instance.GetText(key, fallback) : fallback;

        private static string Format(string key, string fallback, params object[] args) => string.Format(Text(key, fallback), args);
    }
}
