namespace ArrowSwarm.UI
{
    using System.Collections.Generic;
    using ArrowSwarm.Data;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Leaderboard screen showing top 10 players matching the popup dialog visual design.
    /// Handles animated transitions, data loading, and back/close buttons.
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

        [Header("Decoration & Board")]
        [SerializeField] private Image _boardImage;
        [SerializeField] private Image _footerTrophyImage;

        [Header("Animation")]
        [SerializeField] private float _fadeSpeed = 5f;

        private void Awake()
        {
            AutoWire();
        }

        private void Start()
        {
            _backButton?.onClick.AddListener(Hide);
            _closeButton?.onClick.AddListener(Hide);
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

            if (_entriesContainer != null && (_entryRows == null || _entryRows.Length == 0))
            {
                _entryRows = _entriesContainer.GetComponentsInChildren<LeaderboardEntryUI>(true);
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
                _canvasGroup.blocksRaycasts = false;
                StopAllCoroutines();
                StartCoroutine(FadeTo(0f, true));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Loads and displays top 10 player entries from LeaderboardManager.
        /// </summary>
        public void RefreshLeaderboardData()
        {
            int rowCount = _entryRows != null && _entryRows.Length > 0 ? _entryRows.Length : 10;
            var entries = LeaderboardManager.Instance?.GetTopPlayers(rowCount);

            if (entries != null && _entryRows != null)
            {
                for (int i = 0; i < _entryRows.Length; i++)
                {
                    if (_entryRows[i] == null) continue;

                    if (i < entries.Count)
                    {
                        int rank = i + 1;
                        var entry = entries[i];
                        _entryRows[i].gameObject.SetActive(true);
                        _entryRows[i].Setup(
                            rank: rank,
                            playerName: entry.PlayerName,
                            level: entry.HighestLevel,
                            stars: entry.TotalStars,
                            isPlayer: entry.IsPlayer
                        );
                    }
                    else
                    {
                        _entryRows[i].gameObject.SetActive(false);
                    }
                }
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
                gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            _backButton?.onClick.RemoveListener(Hide);
            _closeButton?.onClick.RemoveListener(Hide);
        }
    }
}
