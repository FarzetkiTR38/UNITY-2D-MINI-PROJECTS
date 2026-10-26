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
    public partial class LeaderboardUI : MonoBehaviour
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
        [SerializeField] private RectTransform _dialogBox;
        [SerializeField] private float _fadeSpeed = 5f;

        private void OnEnable()
        {
            LeaderboardManager.OnLeaderboardUpdated += HandleLeaderboardUpdated;
            ArrowSwarm.Localization.LocalizationManager.OnLanguageChanged += RefreshLeaderboardData;
            DataManager.OnPlayerDataChanged += HandlePlayerDataChanged;
        }

        private void OnDisable()
        {
            LeaderboardManager.OnLeaderboardUpdated -= HandleLeaderboardUpdated;
            ArrowSwarm.Localization.LocalizationManager.OnLanguageChanged -= RefreshLeaderboardData;
            DataManager.OnPlayerDataChanged -= HandlePlayerDataChanged;
        }

        private void HandleLeaderboardUpdated()
        {
            RefreshLeaderboardData();
        }

        private void HandlePlayerDataChanged(PlayerData data) => RefreshLeaderboardData();

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
            _retryButton?.onClick.AddListener(Retry);

            if (_titleText != null && string.IsNullOrEmpty(_titleText.text))
            {
                _titleText.text = "LEADERBOARD";
            }
        }

        /// <summary>
        /// Shows the leaderboard panel with elastic pop-in and refreshes data.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            AutoWire();

            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
                StopAllCoroutines();
                StartCoroutine(UIPopupAnimator.AnimateOpen(_dialogBox, _canvasGroup));
            }

            // Opening the board is the first intentional request for the lazy leaderboard service.
            var leaderboard = LeaderboardManager.Instance;
            RefreshLeaderboardData();
            if (leaderboard != null)
            {
                _ = leaderboard.RefreshFromCloudAsync();
            }
        }

        /// <summary>
        /// Hides the leaderboard panel with elastic pop-out.
        /// </summary>
        public void Hide()
        {
            if (!gameObject.activeInHierarchy) return;

            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                StopAllCoroutines();
                StartCoroutine(UIPopupAnimator.AnimateClose(_dialogBox, _canvasGroup, onComplete: () => gameObject.SetActive(false)));
            }
            else
            {
                gameObject.SetActive(false);
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
            _retryButton?.onClick.RemoveListener(Retry);
        }
    }
}
