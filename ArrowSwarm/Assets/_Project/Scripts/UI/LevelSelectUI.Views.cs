namespace ArrowSwarm.UI
{
    using System;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;

    /// <summary>Selects optional scene-authored campaign views while retaining legacy fields for other scenes.</summary>
    public partial class LevelSelectUI
    {
        /// <summary>References one authored view; multiple modes may share its root and grid.</summary>
        [Serializable]
        private sealed class ModeView
        {
            [SerializeField] private GameModeId _mode;
            [SerializeField] private GameObject _root;
            [SerializeField] private RectTransform _dialogBox;
            [SerializeField] private Button _close;
            [SerializeField] private Button _previous;
            [SerializeField] private Button _next;
            [SerializeField] private TextMeshProUGUI _page;
            [SerializeField] private Transform _grid;
            [SerializeField] private GameObject _identityRoot;
            internal GameModeId Mode => _mode;
            internal GameObject Root => _root;
            internal RectTransform Dialog => _dialogBox;
            internal Button Close => _close;
            internal Button Previous => _previous;
            internal Button Next => _next;
            internal TextMeshProUGUI Page => _page;
            internal Transform Grid => _grid;
            internal GameObject Identity => _identityRoot;
            internal bool Complete => _root != null && _dialogBox != null && _close != null &&
                _previous != null && _next != null && _page != null && _grid != null;
        }

        [Header("Optional Campaign Views")]
        [SerializeField] private ModeView[] _modeViews;
        private ModeView _boundModeView;
        private bool _navigationAttached;

        private void ResolveModeView()
        {
            if (_modeViews == null || _modeViews.Length == 0) return;
            ModeView chosen = null;
            foreach (var view in _modeViews)
                if (view != null && view.Mode == SelectedMode) { chosen = view; break; }
            if (chosen == null || !chosen.Complete || ReferenceEquals(chosen, _boundModeView)) return;
            bool listening = _navigationAttached;
            DetachNavigation();
            if (_levelGridContainer != chosen.Grid) _buttonPool.Clear();
            foreach (var view in _modeViews)
            {
                if (view == null) continue;
                if (view.Root != null) view.Root.SetActive(false);
                if (view.Identity != null) view.Identity.SetActive(false);
            }
            _dialogBox = chosen.Dialog; _closeButton = chosen.Close;
            _prevPageButton = chosen.Previous; _nextPageButton = chosen.Next;
            _pageText = chosen.Page; _levelGridContainer = chosen.Grid;
            chosen.Root.SetActive(true);
            if (chosen.Identity != null) chosen.Identity.SetActive(true);
            _boundModeView = chosen;
            if (listening) AttachNavigation();
        }

        private void AttachNavigation()
        {
            DetachNavigation();
            _closeButton?.onClick.AddListener(Close);
            _prevPageButton?.onClick.AddListener(PrevPage);
            _nextPageButton?.onClick.AddListener(NextPage);
            _navigationAttached = true;
        }

        private void DetachNavigation()
        {
            _closeButton?.onClick.RemoveListener(Close);
            _prevPageButton?.onClick.RemoveListener(PrevPage);
            _nextPageButton?.onClick.RemoveListener(NextPage);
            _navigationAttached = false;
        }

        private void UpdatePage()
        {
            if (_levelGridContainer == null) return;

            int highestLevel = SelectedHighest;
            int maxPage = Mathf.Max(1, (highestLevel - 1) / LEVELS_PER_PAGE + 1);

            _currentPage = Mathf.Clamp(_currentPage, 1, maxPage);

            if (_prevPageButton != null) _prevPageButton.interactable = _currentPage > 1;
            if (_nextPageButton != null) _nextPageButton.interactable = _currentPage < maxPage;

            int startLevel = (_currentPage - 1) * LEVELS_PER_PAGE + 1;

            RefreshPageLabel();

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
                int stars = DataManager.Instance?.GetLevelStars(SelectedMode, levelNum) ?? 0;

                _buttonPool[i].gameObject.SetActive(levelNum <= CampaignCount);
                _buttonPool[i].Setup(levelNum, isUnlocked, stars, OnLevelClicked);
            }
        }
    }
}
