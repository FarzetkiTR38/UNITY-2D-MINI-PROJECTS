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
        [SerializeField] private float _fadeSpeed = 5f;

        private int _currentPage = 1;
        private readonly List<LevelButtonUI> _buttonPool = new List<LevelButtonUI>();

        private void Awake()
        {
            AutoWire();
        }

        private void OnEnable()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
                StopAllCoroutines();
                StartCoroutine(FadeTo(1f));
            }

            int highestLevel = DataManager.Instance?.PlayerData?.highestLevel ?? 1;
            int maxPage = Mathf.Max(1, (highestLevel - 1) / LEVELS_PER_PAGE + 1);
            _currentPage = maxPage;

            UpdatePage();
        }

        private void Start()
        {
            _closeButton?.onClick.AddListener(Close);
            _prevPageButton?.onClick.AddListener(PrevPage);
            _nextPageButton?.onClick.AddListener(NextPage);
        }

        public void AutoWire()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

            if (_closeButton == null)
            {
                var btn = transform.Find("Header/CloseButton") ?? transform.Find("CloseButton");
                if (btn != null) _closeButton = btn.GetComponent<Button>();
            }

            if (_prevPageButton == null)
            {
                var btn = transform.Find("Nav/PrevButton") ?? transform.Find("PrevButton");
                if (btn != null) _prevPageButton = btn.GetComponent<Button>();
            }

            if (_nextPageButton == null)
            {
                var btn = transform.Find("Nav/NextButton") ?? transform.Find("NextButton");
                if (btn != null) _nextPageButton = btn.GetComponent<Button>();
            }

            if (_pageText == null)
            {
                var txt = transform.Find("Nav/PageText") ?? transform.Find("PageText");
                if (txt != null) _pageText = txt.GetComponent<TextMeshProUGUI>();
            }

            if (_levelGridContainer == null)
                _levelGridContainer = transform.Find("GridContainer") ?? transform.Find("Content/GridContainer");
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

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Close()
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

        private System.Collections.IEnumerator FadeTo(float target, bool disableOnComplete = false)
        {
            while (Mathf.Abs(_canvasGroup.alpha - target) > 0.01f)
            {
                _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, target, Time.deltaTime * _fadeSpeed);
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
            _closeButton?.onClick.RemoveListener(Close);
            _prevPageButton?.onClick.RemoveListener(PrevPage);
            _nextPageButton?.onClick.RemoveListener(NextPage);
        }
    }
}
