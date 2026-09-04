namespace ArrowSwarm.UI
{
    using System.Collections.Generic;
    using ArrowSwarm.Data;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Leaderboard screen showing top 10 players matching the popup dialog visual design.
    /// Handles animated transitions, data loading, username display/editing, and back/close buttons.
    /// </summary>
    public class LeaderboardUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Transform _entriesContainer;
        [SerializeField] private LeaderboardEntryUI[] _entryRows; // 10 entries (1, 2, 3 scene objects + 4..10 prefab instances)

        [Header("Current Player Bar (Optional)")]
        [SerializeField] private TextMeshProUGUI _currentPlayerNameText;
        [SerializeField] private TextMeshProUGUI _currentPlayerRankText;
        [SerializeField] private TextMeshProUGUI _currentPlayerLevelText;
        [SerializeField] private TextMeshProUGUI _currentPlayerStarsText;
        [SerializeField] private TMP_InputField _nameInputField;
        [SerializeField] private Button _saveNameButton;

        [Header("Decoration & Board")]
        [SerializeField] private Image _boardImage;
        [SerializeField] private Image _footerTrophyImage;

        [Header("Animation")]
        [SerializeField] private float _fadeSpeed = 5f;

        private void OnEnable()
        {
            LeaderboardManager.OnLeaderboardUpdated += HandleLeaderboardUpdated;
        }

        private void OnDisable()
        {
            LeaderboardManager.OnLeaderboardUpdated -= HandleLeaderboardUpdated;
        }

        private void HandleLeaderboardUpdated()
        {
            RefreshLeaderboardData();
        }

        private void Awake()
        {
            AutoWire();
        }

        private void Start()
        {
            _backButton?.onClick.AddListener(Hide);
            _closeButton?.onClick.AddListener(Hide);
            _nameInputField?.onEndEdit.AddListener(OnNameInputEndEdit);
            _saveNameButton?.onClick.AddListener(OnSaveNameClicked);

            if (_titleText != null && string.IsNullOrEmpty(_titleText.text))
            {
                _titleText.text = "LEADERBOARD";
            }
        }

        /// <summary>
        /// Automatically discovers and connects required UI references.
        /// </summary>
        public void AutoWire()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

            if (_backButton == null)
            {
                var btn = transform.Find("BoardFrame/Header/BackButton") ?? transform.Find("BackButton");
                if (btn != null) _backButton = btn.GetComponent<Button>();
            }

            if (_closeButton == null)
            {
                var btn = transform.Find("BoardFrame/Header/CloseButton") ?? transform.Find("CloseBtn") ?? transform.Find("CloseButton");
                if (btn != null) _closeButton = btn.GetComponent<Button>();
            }

            if (_titleText == null)
            {
                var txt = transform.Find("BoardFrame/Header/TitleText") ?? transform.Find("TitleText");
                if (txt != null) _titleText = txt.GetComponent<TextMeshProUGUI>();
            }

            if (_entriesContainer == null)
            {
                _entriesContainer = transform.Find("BoardFrame/EntriesContainer") ?? transform.Find("EntriesContainer");
            }

            if (_entryRows == null || _entryRows.Length == 0 || _entryRows[0] == null)
            {
                _entryRows = GetComponentsInChildren<LeaderboardEntryUI>(true);
            }

            if (_boardImage == null)
            {
                var b = transform.Find("BoardFrame");
                if (b != null) _boardImage = b.GetComponent<Image>();
            }

            if (_footerTrophyImage == null)
            {
                var f = transform.Find("BoardFrame/Footer") ?? transform.Find("BoardFrame/FooterArea/TrophyBadge") ?? transform.Find("FooterArea/TrophyBadge");
                if (f != null) _footerTrophyImage = f.GetComponent<Image>();
            }

            // AutoWire Current Player bar if present
            if (_currentPlayerNameText == null)
            {
                var t = transform.Find("BoardFrame/PlayerBar/NameText") 
                     ?? transform.Find("BoardFrame/Footer/PlayerNameText")
                     ?? transform.Find("PlayerNameText");
                if (t != null) _currentPlayerNameText = t.GetComponent<TextMeshProUGUI>();
            }

            if (_currentPlayerRankText == null)
            {
                var t = transform.Find("BoardFrame/PlayerBar/RankText")
                     ?? transform.Find("BoardFrame/Footer/RankText");
                if (t != null) _currentPlayerRankText = t.GetComponent<TextMeshProUGUI>();
            }

            if (_nameInputField == null)
            {
                var input = transform.Find("BoardFrame/PlayerBar/NameInputField")
                         ?? transform.Find("NameInputField");
                if (input != null) _nameInputField = input.GetComponent<TMP_InputField>();
            }

            if (_saveNameButton == null)
            {
                var btn = transform.Find("BoardFrame/PlayerBar/SaveNameButton")
                       ?? transform.Find("SaveNameButton");
                if (btn != null) _saveNameButton = btn.GetComponent<Button>();
            }
        }

        /// <summary>
        /// Shows the leaderboard panel with smooth fade in and refreshes data.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            AutoWire();

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
                StopAllCoroutines();
                StartCoroutine(FadeTo(1f));
            }

            RefreshLeaderboardData();
            if (LeaderboardManager.HasInstance)
            {
                _ = LeaderboardManager.Instance.RefreshFromCloudAsync();
            }
        }

        /// <summary>
        /// Hides the leaderboard panel with smooth fade out.
        /// </summary>
        public void Hide()
        {
            if (!gameObject.activeInHierarchy) return;

            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                StopAllCoroutines();
                StartCoroutine(FadeTo(0f, true));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Loads and displays top player entries from LeaderboardManager or local player.
        /// </summary>
        public void RefreshLeaderboardData()
        {
            AutoWire();

            int rowCount = _entryRows != null && _entryRows.Length > 0 ? _entryRows.Length : 10;
            List<LeaderboardEntry> entries = null;

            if (LeaderboardManager.HasInstance)
            {
                entries = LeaderboardManager.Instance.GetTopPlayers(rowCount);
            }
            else
            {
                var currentData = DataManager.HasInstance ? DataManager.Instance.PlayerData : null;
                int playerLevel = currentData?.highestLevel ?? 1;
                int playerStars = currentData?.GetTotalStars() ?? 0;
                string playerName = currentData?.playerName ?? "Player";
                string country = currentData?.playerCountry ?? "TR";

                entries = new List<LeaderboardEntry>
                {
                    new LeaderboardEntry
                    {
                        PlayerName = playerName,
                        HighestLevel = playerLevel,
                        TotalStars = playerStars,
                        IsPlayer = true,
                        CountryCode = country
                    }
                };
            }

            if (_entryRows != null)
            {
                for (int i = 0; i < _entryRows.Length; i++)
                {
                    if (_entryRows[i] == null) continue;

                    int rank = i + 1;
                    _entryRows[i].gameObject.SetActive(true);

                    if (entries != null && i < entries.Count)
                    {
                        var entry = entries[i];
                        _entryRows[i].Setup(
                            rank: rank,
                            playerName: entry.PlayerName,
                            level: entry.HighestLevel,
                            stars: entry.TotalStars,
                            isPlayer: entry.IsPlayer,
                            countryCode: entry.CountryCode
                        );
                    }
                    else
                    {
                        // Active empty slot: Lv.1, 0 Stars, blank username
                        _entryRows[i].Setup(
                            rank: rank,
                            playerName: "",
                            level: 1,
                            stars: 0,
                            isPlayer: false,
                            countryCode: ""
                        );
                    }
                }
            }

            // Update current player info bar if assigned
            var playerData = DataManager.Instance?.PlayerData;
            if (playerData != null)
            {
                if (_currentPlayerNameText != null)
                {
                    string tag = GetCountryTag(playerData.playerCountry);
                    _currentPlayerNameText.text = string.IsNullOrEmpty(tag) ? playerData.playerName : $"{tag} {playerData.playerName}";
                }

                if (_currentPlayerRankText != null && LeaderboardManager.Instance != null)
                {
                    _currentPlayerRankText.text = $"#{LeaderboardManager.Instance.GetPlayerRank()}";
                }

                if (_currentPlayerLevelText != null)
                {
                    _currentPlayerLevelText.text = $"Lv.{playerData.highestLevel}";
                }

                if (_currentPlayerStarsText != null)
                {
                    _currentPlayerStarsText.text = playerData.GetTotalStars().ToString();
                }

                if (_nameInputField != null && !_nameInputField.isFocused)
                {
                    _nameInputField.text = playerData.playerName;
                }
            }
        }

        /// <summary>
        /// Updates player username and refreshes ranking.
        /// </summary>
        public void SetPlayerName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) return;
            LeaderboardManager.Instance?.SetPlayerName(newName);
            RefreshLeaderboardData();
        }

        private void OnNameInputEndEdit(string text)
        {
            SetPlayerName(text);
        }

        private void OnSaveNameClicked()
        {
            if (_nameInputField != null)
            {
                SetPlayerName(_nameInputField.text);
            }
        }

        private System.Collections.IEnumerator FadeTo(float target, bool disableOnComplete = false)
        {
            while (Mathf.Abs(_canvasGroup.alpha - target) > 0.01f)
            {
                _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, target, Time.unscaledDeltaTime * _fadeSpeed);
                yield return null;
            }
            _canvasGroup.alpha = target;

            if (disableOnComplete)
            {
                if (_canvasGroup != null) _canvasGroup.blocksRaycasts = false;
                gameObject.SetActive(false);
            }
        }

        private static string GetCountryTag(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return "";
            string clean = code.Trim().ToUpperInvariant();
            if (clean.Length > 3) clean = clean.Substring(0, 3);
            return $"[{clean}]";
        }

        private void OnDestroy()
        {
            _backButton?.onClick.RemoveListener(Hide);
            _closeButton?.onClick.RemoveListener(Hide);
            _nameInputField?.onEndEdit.RemoveListener(OnNameInputEndEdit);
            _saveNameButton?.onClick.RemoveListener(OnSaveNameClicked);
        }
    }
}
