using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonGalaxy.Boot;
using NeonGalaxy.Services;
using NeonGalaxy.Data;

namespace NeonGalaxy.UI
{
    /// <summary>
    /// Controls the leaderboard UI panel.
    /// Fetches entries from ILeaderboardService and displays them
    /// in a scrollable list. Highlights the player's own entry.
    /// Handles offline fallback with cached data.
    /// </summary>
    public class LeaderboardUIController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Transform entryListParent;
        [SerializeField] private GameObject entryPrefab;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private GameObject loadingIndicator;

        [Header("Top 3 Players")]
        [SerializeField] private TextMeshProUGUI firstPlayerNameText;
        [SerializeField] private TextMeshProUGUI firstPlayerScoreText;
        [SerializeField] private TextMeshProUGUI secondPlayerNameText;
        [SerializeField] private TextMeshProUGUI secondPlayerScoreText;
        [SerializeField] private TextMeshProUGUI thirdPlayerNameText;
        [SerializeField] private TextMeshProUGUI thirdPlayerScoreText;


        [Header("Buttons")]
        [SerializeField] private Button refreshButton;
        [SerializeField] private Button closeButton;

        public event System.Action OnCloseClicked;

        private CachedLeaderboard _cachedData;

        private void Awake()
        {
            if (refreshButton != null)
                refreshButton.onClick.AddListener(OnRefreshClicked);

            if (closeButton != null)
                closeButton.onClick.AddListener(() =>
                {
                    OnCloseClicked?.Invoke();
                    gameObject.SetActive(false);
                });
        }

        private void OnEnable()
        {
            FetchAndDisplay();
        }

        // ── Public API ───────────────────────────────────────────

        /// <summary>
        /// Fetches leaderboard data and populates the UI.
        /// </summary>
        public void FetchAndDisplay()
        {
            StartCoroutine(FetchLeaderboardRoutine());
        }

        // ── Internal ─────────────────────────────────────────────

        private IEnumerator FetchLeaderboardRoutine()
        {
            ClearUI();
            SetLoading(true);

            var service = ServiceLocator.Get<ILeaderboardService>();
            if (service == null)
            {
                SetStatusText("Leaderboard service not available.");
                SetLoading(false);
                yield break;
            }

            var task = service.FetchLeaderboardAsync();
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted)
            {
                SetStatusText("Failed to load leaderboard.");
                Debug.LogWarning($"[LeaderboardUI] Fetch failed: {task.Exception?.Message}");
            }
            else
            {
                _cachedData = task.Result;
                PopulateList(_cachedData);

                if (!service.IsOnline)
                {
                    SetStatusText("Offline — showing cached data.");
                }
                else
                {
                    SetStatusText("");
                }
            }

            SetLoading(false);
        }

        private void PopulateList(CachedLeaderboard data)
        {
            if (entryListParent == null || entryPrefab == null) return;

            ClearUI();

            if (data == null || data.entries == null || data.entries.Count == 0)
            {
                SetStatusText("No leaderboard data available.");
                return;
            }

            // Populate entries (max 10 total)
            int count = Mathf.Min(data.entries.Count, 10);
            for (int i = 0; i < count; i++)
            {
                var entry = data.entries[i];

                // Top 3 handling
                if (i == 0)
                {
                    if (firstPlayerNameText != null) firstPlayerNameText.text = entry.playerName;
                    if (firstPlayerScoreText != null) firstPlayerScoreText.text = entry.score.ToString("N0");
                    continue; // Skip creating prefab for top 3
                }
                else if (i == 1)
                {
                    if (secondPlayerNameText != null) secondPlayerNameText.text = entry.playerName;
                    if (secondPlayerScoreText != null) secondPlayerScoreText.text = entry.score.ToString("N0");
                    continue; // Skip creating prefab for top 3
                }
                else if (i == 2)
                {
                    if (thirdPlayerNameText != null) thirdPlayerNameText.text = entry.playerName;
                    if (thirdPlayerScoreText != null) thirdPlayerScoreText.text = entry.score.ToString("N0");
                    continue; // Skip creating prefab for top 3
                }

                // 4th and beyond
                var entryGO = Instantiate(entryPrefab, entryListParent);
                entryGO.SetActive(true);

                // Try to find text components in the prefab
                var texts = entryGO.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 3)
                {
                    texts[0].text = entry.rank.ToString();
                    texts[1].text = entry.playerName;
                    texts[2].text = entry.score.ToString("N0");
                }
                else if (texts.Length >= 1)
                {
                    texts[0].text = $"{entry.rank}  {entry.playerName}  —  {entry.score:N0}";
                }

                // Highlight player entry
                bool isPlayer = data.playerEntry != null &&
                               entry.playerId == data.playerEntry.playerId;
                if (isPlayer)
                {
                    // Apply highlight color
                    foreach (var t in texts)
                    {
                        t.color = new Color(0.4f, 1f, 0.8f, 1f); // Neon cyan
                    }
                }
            }

        }

        private void SetLoading(bool loading)
        {
            if (loadingIndicator != null)
                loadingIndicator.SetActive(loading);

            if (refreshButton != null)
                refreshButton.interactable = !loading;
        }

        private void SetStatusText(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }

        private void ClearUI()
        {
            if (entryListParent != null)
            {
                foreach (Transform child in entryListParent)
                {
                    Destroy(child.gameObject);
                }
            }

            if (firstPlayerNameText != null) firstPlayerNameText.text = "---";
            if (firstPlayerScoreText != null) firstPlayerScoreText.text = "0";
            if (secondPlayerNameText != null) secondPlayerNameText.text = "---";
            if (secondPlayerScoreText != null) secondPlayerScoreText.text = "0";
            if (thirdPlayerNameText != null) thirdPlayerNameText.text = "---";
            if (thirdPlayerScoreText != null) thirdPlayerScoreText.text = "0";
        }

        // ── Button Handlers ──────────────────────────────────────

        private void OnRefreshClicked()
        {
            FetchAndDisplay();
        }
    }
}
