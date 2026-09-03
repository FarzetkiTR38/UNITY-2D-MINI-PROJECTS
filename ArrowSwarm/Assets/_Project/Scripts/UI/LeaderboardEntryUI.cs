namespace ArrowSwarm.UI
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Represents a single player ranking row in the Leaderboard UI.
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

        /// <summary>
        /// Populates the leaderboard entry with rank, level, star counts, and country flag.
        /// </summary>
        public void Setup(int rank, string playerName, int level, int stars, bool isPlayer, string countryCode = "TR")
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
                if (string.IsNullOrEmpty(playerName))
                {
                    _playerNameText.text = "";
                }
                else
                {
                    string tag = GetCountryTag(countryCode);
                    _playerNameText.text = string.IsNullOrEmpty(tag) ? playerName : $"{tag} {playerName}";
                }
            }

            if (_crownIcon != null)
            {
                _crownIcon.gameObject.SetActive(rank <= 3);
            }
        }

        private static string GetCountryTag(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return "";
            string clean = code.Trim().ToUpperInvariant();
            if (clean.Length > 3) clean = clean.Substring(0, 3);
            return $"[{clean}]";
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

            if (_playerNameText == null)
            {
                var t = transform.Find("ContentPill/PlayerNameText") 
                     ?? transform.Find("ContentPill/NameText") 
                     ?? transform.Find("PlayerNameText") 
                     ?? transform.Find("NameText");
                if (t != null) _playerNameText = t.GetComponent<TextMeshProUGUI>();
            }
        }
    }
}
