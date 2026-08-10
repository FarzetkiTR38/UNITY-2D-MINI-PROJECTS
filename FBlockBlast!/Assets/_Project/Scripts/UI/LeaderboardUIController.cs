using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonGalaxy.Boot;
using NeonGalaxy.Services;
using NeonGalaxy.Meta;
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

        [Header("Top 3 Avatars")]
        [SerializeField] private Image firstPlayerAvatarImage;
        [SerializeField] private Image secondPlayerAvatarImage;
        [SerializeField] private Image thirdPlayerAvatarImage;
        [SerializeField] private Sprite defaultLeaderboardAvatar; // Default avatar for players without custom picture

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
                    SetLeaderboardAvatar(firstPlayerAvatarImage, entry, data);
                    continue; // Skip creating prefab for top 3
                }
                else if (i == 1)
                {
                    if (secondPlayerNameText != null) secondPlayerNameText.text = entry.playerName;
                    if (secondPlayerScoreText != null) secondPlayerScoreText.text = entry.score.ToString("N0");
                    SetLeaderboardAvatar(secondPlayerAvatarImage, entry, data);
                    continue; // Skip creating prefab for top 3
                }
                else if (i == 2)
                {
                    if (thirdPlayerNameText != null) thirdPlayerNameText.text = entry.playerName;
                    if (thirdPlayerScoreText != null) thirdPlayerScoreText.text = entry.score.ToString("N0");
                    SetLeaderboardAvatar(thirdPlayerAvatarImage, entry, data);
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

                // Set avatar for list entries (4th and beyond)
                // Check for AvatarMask parent first, and pick the inner Image child inside it
                Transform maskTransform = entryGO.transform.Find("AvatarMask")
                                       ?? entryGO.transform.Find("Mask");

                Image entryAvatarImage = null;

                if (maskTransform != null)
                {
                    // If AvatarMask has a child (the actual photo image inside mask), pick the inner child
                    if (maskTransform.childCount > 0)
                    {
                        entryAvatarImage = maskTransform.GetChild(0).GetComponent<Image>();
                    }
                    else
                    {
                        entryAvatarImage = maskTransform.GetComponent<Image>();
                    }
                }

                // Fallback search if AvatarMask is not present
                if (entryAvatarImage == null)
                {
                    Transform avatarChild = entryGO.transform.Find("AvatarImage")
                                         ?? entryGO.transform.Find("Avatar")
                                         ?? entryGO.transform.Find("ProfileImage")
                                         ?? entryGO.transform.Find("Profile");

                    entryAvatarImage = avatarChild != null 
                        ? avatarChild.GetComponent<Image>() 
                        : entryGO.GetComponentInChildren<Image>();
                }

                if (entryAvatarImage != null)
                {
                    SetLeaderboardAvatar(entryAvatarImage, entry, data);
                }
            }

        }

        private readonly Dictionary<string, Sprite> _avatarCache = new Dictionary<string, Sprite>();

        /// <summary>
        /// Sets the avatar image for a leaderboard entry.
        /// Shows current player's avatar directly, or fetches other players' public avatars from UGS Cloud Save.
        /// </summary>
        private void SetLeaderboardAvatar(Image avatarImage, LeaderboardEntry entry, CachedLeaderboard leaderboardData)
        {
            if (avatarImage == null || entry == null) return;

            // Default fallback first
            if (defaultLeaderboardAvatar != null)
                avatarImage.sprite = defaultLeaderboardAvatar;

            // Check if this is the current player
            bool isCurrentPlayer = leaderboardData.playerEntry != null
                                   && entry.playerId == leaderboardData.playerEntry.playerId;

            if (isCurrentPlayer)
            {
                // Show current player's avatar (custom or built-in)
                var profileManager = ServiceLocator.Get<ProfileManager>();
                if (profileManager != null)
                {
                    var sprite = profileManager.GetCurrentAvatarSprite();
                    if (sprite != null)
                    {
                        avatarImage.sprite = sprite;
                        return;
                    }
                }
            }

            // For other players: check memory cache first
            if (_avatarCache.TryGetValue(entry.playerId, out var cachedSprite))
            {
                if (cachedSprite != null)
                {
                    avatarImage.sprite = cachedSprite;
                }
                return;
            }

            // Async fetch other player's avatar from UGS Cloud Save
            _ = LoadOtherPlayerAvatarAsync(avatarImage, entry.playerId);
        }

        private async Task LoadOtherPlayerAvatarAsync(Image avatarImage, string playerId)
        {
            if (string.IsNullOrEmpty(playerId) || avatarImage == null) return;

            var cloudSaveService = ServiceLocator.Get<ICloudSaveService>();
            if (cloudSaveService == null || !cloudSaveService.IsAvailable) return;

            string payload = await cloudSaveService.LoadPublicDataForPlayerAsync(playerId, "public_avatar_data");
            if (string.IsNullOrEmpty(payload))
            {
                _avatarCache[playerId] = null; // Mark as null to avoid repeated network calls
                return;
            }

            Sprite loadedSprite = null;

            if (payload.StartsWith("id:"))
            {
                // Built-in avatar ID
                string avatarId = payload.Substring(3);
                var profileManager = ServiceLocator.Get<ProfileManager>();
                var registry = profileManager?.AvatarRegistry;
                if (registry != null)
                {
                    loadedSprite = registry.GetAvatarSprite(avatarId);
                }
            }
            else if (payload.StartsWith("data:"))
            {
                // Base64 custom avatar PNG data
                string base64 = payload.Substring(5);
                var pictureService = ServiceLocator.Get<ProfilePictureService>();
                if (pictureService != null)
                {
                    loadedSprite = pictureService.Base64ToSprite(base64);
                }
            }

            if (loadedSprite != null)
            {
                _avatarCache[playerId] = loadedSprite;
                if (avatarImage != null)
                {
                    avatarImage.sprite = loadedSprite;
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
