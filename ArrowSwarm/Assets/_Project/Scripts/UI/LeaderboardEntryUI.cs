namespace ArrowSwarm.UI
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Represents a single player ranking row (Top 1-10) in the Leaderboard UI.
    /// Handles rank number, crown/badge graphics, level, and star count display.
    /// </summary>
    public class LeaderboardEntryUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image _cardBackground;
        [SerializeField] private Image _rankBadge;
        [SerializeField] private Image _crownIcon;
        [SerializeField] private TextMeshProUGUI _rankText;
        [SerializeField] private Image _contentPill;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Image _divider;
        [SerializeField] private Image _starIcon;
        [SerializeField] private TextMeshProUGUI _starsText;
        [SerializeField] private TextMeshProUGUI _playerNameText;

        [Header("Rank Sprites (Assign in Inspector)")]
        [Tooltip("Outer card background sprite for Rank 1 (Gold)")]
        [SerializeField] private Sprite _rank1CardSprite;

        [Tooltip("Outer card background sprite for Rank 2 (Silver)")]
        [SerializeField] private Sprite _rank2CardSprite;

        [Tooltip("Outer card background sprite for Rank 3 (Bronze)")]
        [SerializeField] private Sprite _rank3CardSprite;

        [Tooltip("Outer card background sprite for Rank 4-10 (Blue)")]
        [SerializeField] private Sprite _normalCardSprite;

        [Header("Badge / Crown Sprites (Assign in Inspector)")]
        [SerializeField] private Sprite _rank1BadgeSprite;
        [SerializeField] private Sprite _rank2BadgeSprite;
        [SerializeField] private Sprite _rank3BadgeSprite;
        [SerializeField] private Sprite _normalBadgeSprite;

        [Header("Fallback Colors")]
        [SerializeField] private Color _rank1BgColor = new Color(0.98f, 0.76f, 0.22f, 1f); // Gold
        [SerializeField] private Color _rank2BgColor = new Color(0.75f, 0.82f, 0.90f, 1f); // Silver
        [SerializeField] private Color _rank3BgColor = new Color(0.85f, 0.53f, 0.28f, 1f); // Bronze
        [SerializeField] private Color _normalBgColor = new Color(0.18f, 0.58f, 0.96f, 1f); // Blue
        [SerializeField] private Color _playerHighlightColor = new Color(0.30f, 0.80f, 0.50f, 1f); // Green/Player

        /// <summary>
        /// Populates the leaderboard entry with rank, level, and stars.
        /// </summary>
        public void Setup(int rank, string playerName, int level, int stars, bool isPlayer)
        {
            AutoWire();

            if (_rankText != null)
            {
                _rankText.text = rank.ToString();
            }

            if (_levelText != null)
            {
                _levelText.text = $"Lv.{level}";
            }

            if (_starsText != null)
            {
                _starsText.text = stars.ToString();
            }

            if (_playerNameText != null)
            {
                _playerNameText.text = playerName;
            }

            // Crown icon is visible for top 3
            if (_crownIcon != null)
            {
                _crownIcon.gameObject.SetActive(rank <= 3);
            }

            // Apply card styling based on rank
            ApplyRankStyling(rank, isPlayer);
        }

        private void ApplyRankStyling(int rank, bool isPlayer)
        {
            if (_cardBackground == null) return;

            switch (rank)
            {
                case 1:
                    if (_rank1CardSprite != null) _cardBackground.sprite = _rank1CardSprite;
                    else _cardBackground.color = _rank1BgColor;

                    if (_rankBadge != null && _rank1BadgeSprite != null) _rankBadge.sprite = _rank1BadgeSprite;
                    break;

                case 2:
                    if (_rank2CardSprite != null) _cardBackground.sprite = _rank2CardSprite;
                    else _cardBackground.color = _rank2BgColor;

                    if (_rankBadge != null && _rank2BadgeSprite != null) _rankBadge.sprite = _rank2BadgeSprite;
                    break;

                case 3:
                    if (_rank3CardSprite != null) _cardBackground.sprite = _rank3CardSprite;
                    else _cardBackground.color = _rank3BgColor;

                    if (_rankBadge != null && _rank3BadgeSprite != null) _rankBadge.sprite = _rank3BadgeSprite;
                    break;

                default:
                    if (_normalCardSprite != null) _cardBackground.sprite = _normalCardSprite;
                    else _cardBackground.color = isPlayer ? _playerHighlightColor : _normalBgColor;

                    if (_rankBadge != null && _normalBadgeSprite != null) _rankBadge.sprite = _normalBadgeSprite;
                    break;
            }
        }

        /// <summary>
        /// Auto-wires child components by name if not assigned.
        /// </summary>
        public void AutoWire()
        {
            if (_cardBackground == null) _cardBackground = GetComponent<Image>();

            if (_rankText == null)
            {
                var t = transform.Find("LeftBadge/RankText") ?? transform.Find("RankText");
                if (t != null) _rankText = t.GetComponent<TextMeshProUGUI>();
            }

            if (_rankBadge == null)
            {
                var t = transform.Find("LeftBadge") ?? transform.Find("Badge");
                if (t != null) _rankBadge = t.GetComponent<Image>();
            }

            if (_crownIcon == null)
            {
                var t = transform.Find("LeftBadge/CrownIcon") ?? transform.Find("CrownIcon");
                if (t != null) _crownIcon = t.GetComponent<Image>();
            }

            if (_contentPill == null)
            {
                var t = transform.Find("ContentPill");
                if (t != null) _contentPill = t.GetComponent<Image>();
            }

            if (_levelText == null)
            {
                var t = transform.Find("ContentPill/LevelText") ?? transform.Find("LevelText");
                if (t != null) _levelText = t.GetComponent<TextMeshProUGUI>();
            }

            if (_divider == null)
            {
                var t = transform.Find("ContentPill/Divider");
                if (t != null) _divider = t.GetComponent<Image>();
            }

            if (_starIcon == null)
            {
                var t = transform.Find("ContentPill/StarIcon") ?? transform.Find("StarIcon");
                if (t != null) _starIcon = t.GetComponent<Image>();
            }

            if (_starsText == null)
            {
                var t = transform.Find("ContentPill/StarsText") ?? transform.Find("StarsText");
                if (t != null) _starsText = t.GetComponent<TextMeshProUGUI>();
            }
        }
    }
}
