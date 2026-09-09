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
    public class LevelSelectUI : MonoBehaviour
    {
        public const int LEVELS_PER_PAGE = 20;

        [Header("UI References")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _prevPageButton;
        [SerializeField] private Button _nextPageButton;
        [SerializeField] private TextMeshProUGUI _pageText;
        [SerializeField] private Transform _levelGridContainer;
        [SerializeField] private LevelButtonUI _levelButtonPrefab;

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
            AutoWire();
            UpdatePageToHighest();
        }

        private void Start()
        {
            _closeButton?.onClick.AddListener(Close);
            _prevPageButton?.onClick.AddListener(PrevPage);
            _nextPageButton?.onClick.AddListener(NextPage);
        }

        public void AutoWire()
        {
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
            int highestLevel = DataManager.Instance?.PlayerData?.highestLevel ?? 1;
            int maxPage = Mathf.Max(1, (highestLevel - 1) / LEVELS_PER_PAGE + 1);
            _currentPage = maxPage;
            UpdatePage();
        }

        public void PrevPage()
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                UpdatePage();
            }
        }

        public void NextPage()
        {
            int highestLevel = DataManager.Instance?.PlayerData?.highestLevel ?? 1;
            int maxPage = Mathf.Max(1, (highestLevel - 1) / LEVELS_PER_PAGE + 1);

            if (_currentPage < maxPage)
            {
                _currentPage++;
                UpdatePage();
            }
        }

        private void UpdatePage()
        {
            if (_levelGridContainer == null) return;

            int highestLevel = DataManager.Instance?.PlayerData?.highestLevel ?? 1;
            int maxPage = Mathf.Max(1, (highestLevel - 1) / LEVELS_PER_PAGE + 1);

            _currentPage = Mathf.Clamp(_currentPage, 1, maxPage);

            if (_prevPageButton != null) _prevPageButton.interactable = _currentPage > 1;
            if (_nextPageButton != null) _nextPageButton.interactable = _currentPage < maxPage;

            int startLevel = (_currentPage - 1) * LEVELS_PER_PAGE + 1;
            int endLevel = _currentPage * LEVELS_PER_PAGE;

            if (_pageText != null)
            {
                _pageText.text = $"Levels {startLevel} - {endLevel}";
            }

            // Collect existing child buttons in container if pool is empty
            if (_buttonPool.Count == 0)
            {
                var existing = _levelGridContainer.GetComponentsInChildren<LevelButtonUI>(true);
                _buttonPool.AddRange(existing);
            }

            // Ensure we have 20 buttons in pool
            while (_buttonPool.Count < LEVELS_PER_PAGE && _levelButtonPrefab != null)
            {
                LevelButtonUI newBtn = Instantiate(_levelButtonPrefab, _levelGridContainer);
                _buttonPool.Add(newBtn);
            }

            // Update each button for current page
            for (int i = 0; i < LEVELS_PER_PAGE && i < _buttonPool.Count; i++)
            {
                int levelNum = startLevel + i;
                bool isUnlocked = levelNum <= highestLevel;
                int stars = DataManager.Instance?.GetLevelStars(levelNum) ?? 0;

                _buttonPool[i].gameObject.SetActive(true);
                _buttonPool[i].Setup(levelNum, isUnlocked, stars, OnLevelClicked);
            }
        }

        private void OnLevelClicked(int level)
        {
            DataManager.Instance?.SetCurrentLevel(level);
            GameManager.Instance?.StartGame();
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

        private void OnDestroy()
        {
            _closeButton?.onClick.RemoveListener(Close);
            _prevPageButton?.onClick.RemoveListener(PrevPage);
            _nextPageButton?.onClick.RemoveListener(NextPage);
        }
    }
}
