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

        [Header("Player Highlight")]
        [SerializeField] private TextMeshProUGUI playerRankText;
        [SerializeField] private TextMeshProUGUI playerScoreText;

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

            // Clear existing entries
            foreach (Transform child in entryListParent)
            {
                Destroy(child.gameObject);
            }

            if (data == null || data.entries == null || data.entries.Count == 0)
            {
                SetStatusText("No leaderboard data available.");
                return;
            }

            // Populate entries
            foreach (var entry in data.entries)
            {
                var entryGO = Instantiate(entryPrefab, entryListParent);
                entryGO.SetActive(true);

                // Try to find text components in the prefab
                var texts = entryGO.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 3)
                {
                    texts[0].text = $"#{entry.rank}";
                    texts[1].text = entry.playerName;
                    texts[2].text = entry.score.ToString("N0");
                }
                else if (texts.Length >= 1)
                {
                    texts[0].text = $"#{entry.rank}  {entry.playerName}  —  {entry.score:N0}";
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

            // Update player summary
            if (data.playerEntry != null)
            {
                if (playerRankText != null)
                    playerRankText.text = $"Your Rank: #{data.playerEntry.rank}";

                if (playerScoreText != null)
                    playerScoreText.text = $"Your Best: {data.playerEntry.score:N0}";
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

        // ── Button Handlers ──────────────────────────────────────

        private void OnRefreshClicked()
        {
            FetchAndDisplay();
        }
    }
}
