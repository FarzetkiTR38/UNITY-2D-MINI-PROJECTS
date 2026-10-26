namespace ArrowSwarm.UI
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Finds legacy optional panel bindings without replacing Inspector styling.</summary>
    public partial class LeaderboardUI
    {
        /// <summary>
        /// Automatically discovers and connects required UI references.
        /// </summary>
        public void AutoWire()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

            if (_dialogBox == null)
            {
                var b = transform.Find("BoardFrame") ?? transform.Find("DialogBox");
                if (b != null) _dialogBox = b.GetComponent<RectTransform>();
            }

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

    }
}
