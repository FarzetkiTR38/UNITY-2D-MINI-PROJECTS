namespace ArrowSwarm.UI
{
    using TMPro;
    using UnityEngine;

    /// <summary>Displays authenticated row identity through optional scene-authored country and player states.</summary>
    public partial class LeaderboardEntryUI
    {
        [Header("Country Identity (Optional)")]
        [SerializeField] private CountryFlagGraphic _countryFlag;
        [SerializeField] private TextMeshProUGUI _countryCodeText;
        [Header("Current Player State (Optional)")]
        [SerializeField] private GameObject _playerHighlight;
        [SerializeField] private GameObject _normalValuesRoot;
        [SerializeField] private GameObject _highlightedValuesRoot;
        [SerializeField] private TextMeshProUGUI _highlightRankText;
        [SerializeField] private TextMeshProUGUI _highlightNameText;
        [SerializeField] private TextMeshProUGUI _highlightLevelText;
        [SerializeField] private TextMeshProUGUI _highlightStarsText;
        [SerializeField] private TextMeshProUGUI _highlightCountryText;

        private void ApplyIdentity(string countryCode, bool isPlayer)
        {
            string code = string.IsNullOrWhiteSpace(countryCode) ? "" : countryCode.Trim().ToUpperInvariant();
            if (code.Length > 3) code = code.Substring(0, 3);
            if (_countryCodeText != null) _countryCodeText.text = code;
            if (_countryFlag != null) _countryFlag.SetCountryCode(code);
            if (_playerHighlight != null) _playerHighlight.SetActive(isPlayer);
            if (_normalValuesRoot != null && _highlightedValuesRoot != null)
            {
                _normalValuesRoot.SetActive(!isPlayer);
                _highlightedValuesRoot.SetActive(isPlayer);
            }
            Copy(_rankText, _highlightRankText); Copy(_playerNameText, _highlightNameText);
            Copy(_levelText, _highlightLevelText); Copy(_starsText, _highlightStarsText);
            if (_highlightCountryText != null) _highlightCountryText.text = code;
        }

        private static void Copy(TextMeshProUGUI source, TextMeshProUGUI target)
        {
            if (target != null) target.text = source != null ? source.text : "";
        }
    }
}
