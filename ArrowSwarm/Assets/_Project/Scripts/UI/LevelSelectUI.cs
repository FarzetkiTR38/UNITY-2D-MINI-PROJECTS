namespace ArrowSwarm.UI
{
    using System.Collections.Generic;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Controls the Level Selection panel with multi-page pagination.
    /// Displays 20 levels per page, starts at the highest reached level's page,
    /// and lets players jump into any unlocked level.
    /// </summary>
    public partial class LevelSelectUI : MonoBehaviour
    {
        public const int LEVELS_PER_PAGE = 20;

        [Header("UI References")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _prevPageButton;
        [SerializeField] private Button _nextPageButton;
        [SerializeField] private TextMeshProUGUI _pageText;
        [SerializeField] private Transform _levelGridContainer;
        [SerializeField] private LevelButtonUI _levelButtonPrefab;
        [SerializeField] private ArrowSwarm.Modes.PuzzleCatalog _shapesCatalog;
        [SerializeField] private ArrowSwarm.Modes.PuzzleCatalog _colorShiftCatalog;

        private ArrowSwarm.Modes.GameModeId SelectedMode => ArrowSwarm.Modes.GameModeSelection.Selected;
        private int SelectedHighest => DataManager.Instance?.GetHighestLevel(SelectedMode) ?? 1;
        private int CampaignCount => SelectedMode == ArrowSwarm.Modes.GameModeId.Shapes ? (_shapesCatalog?.Count ?? 5)
            : SelectedMode == ArrowSwarm.Modes.GameModeId.ColorShift ? (_colorShiftCatalog?.Count ?? 5) : int.MaxValue;

        [Header("Animation")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _dialogBox;

        private int _currentPage = 1;
        private readonly List<LevelButtonUI> _buttonPool = new List<LevelButtonUI>();

        private void Awake()
        {
            AutoWire();
        }

        private void OnEnable()
        {
            ArrowSwarm.Localization.LocalizationManager.OnLanguageChanged += RefreshPageLabel;
            AutoWire();
            AttachNavigation();
            UpdatePageToHighest();
        }

        /// <summary>Discovers authored controls without replacing their styling.</summary>
        public void AutoWire()
        {
            ResolveModeView();
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            if (_dialogBox == null)
            {
                var card = transform.Find("WindowCard") ?? transform.Find("DialogBox") ?? transform.Find("Card");
                if (card != null) _dialogBox = card.GetComponent<RectTransform>();
            }
            if (_closeButton == null) _closeButton = (transform.Find("WindowCard/CloseButton") ?? transform.Find("Header/CloseButton") ?? transform.Find("CloseButton"))?.GetComponent<Button>();
            if (_prevPageButton == null) _prevPageButton = (transform.Find("WindowCard/Nav/PrevButton") ?? transform.Find("Nav/PrevButton") ?? transform.Find("PrevButton"))?.GetComponent<Button>();
            if (_nextPageButton == null) _nextPageButton = (transform.Find("WindowCard/Nav/NextButton") ?? transform.Find("Nav/NextButton") ?? transform.Find("NextButton"))?.GetComponent<Button>();
            if (_pageText == null) _pageText = (transform.Find("WindowCard/Nav/PageText") ?? transform.Find("Nav/PageText") ?? transform.Find("PageText"))?.GetComponent<TextMeshProUGUI>();
            if (_levelGridContainer == null) _levelGridContainer = transform.Find("WindowCard/GridContainer") ?? transform.Find("GridContainer") ?? transform.Find("Content/GridContainer");
        }

        private void UpdatePageToHighest()
        {
            int highestLevel = SelectedHighest;
            int maxPage = Mathf.Max(1, (highestLevel - 1) / LEVELS_PER_PAGE + 1);
            _currentPage = maxPage;
            UpdatePage();
        }

        /// <summary>Moves back one unlocked page.</summary>
        public void PrevPage()
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                UpdatePage();
            }
        }

        /// <summary>Moves forward one unlocked page.</summary>
        public void NextPage()
        {
            int highestLevel = SelectedHighest;
            int maxPage = Mathf.Max(1, (highestLevel - 1) / LEVELS_PER_PAGE + 1);

            if (_currentPage < maxPage)
            {
                _currentPage++;
                UpdatePage();
            }
        }

        private void OnLevelClicked(int level)
        {
            DataManager.Instance?.SetCurrentLevel(SelectedMode, level);
            ArrowSwarm.Modes.GameModeSelection.PlaySelected();
        }

        /// <summary>Shows the level select panel with elastic pop-in.</summary>
        public void Show()
        {
            gameObject.SetActive(true);
            AutoWire();
            UpdatePageToHighest();

            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
                StopAllCoroutines();
                StartCoroutine(UIPopupAnimator.AnimateOpen(_dialogBox, _canvasGroup));
            }
        }

        /// <summary>Closes the level select panel with elastic pop-out.</summary>
        public void Close()
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

        private void OnDestroy() => DetachNavigation();
    }
}
