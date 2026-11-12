namespace ArrowSwarm.Tips
{
    using System;
    using System.Collections.Generic;
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Manages the tip (hint) system. When activated, highlights the best
    /// arrow to fire — the one whose head is on the edge facing outward
    /// with the highest weight.
    /// </summary>
    public class TipManager : Singleton<TipManager>
    {
        [SerializeField] private TipHighlighter _highlighter;
        private Arrow _suggestedArrow;
        private ArrowDirection _suggestedDirection;
        private readonly List<Vector2Int> _suggestedPoints = new List<Vector2Int>();

        /// <summary>Whether a paid suggestion still identifies an unused, valid arrow on the current board.</summary>
        public bool HasPendingSuggestion
        {
            get
            {
                if (!SuggestionIsValid()) ClearSuggestion();
                return _suggestedArrow != null;
            }
        }

        /// <summary>Fired when a tip is used (remaining tip count).</summary>
        public static event Action<int> OnTipUsed;

        /// <summary>Fired when tip count is zero and user tries to use.</summary>
        public static event Action OnNoTipsAvailable;

        protected override void OnSingletonAwake()
        {
            if (_highlighter == null)
            {
                _highlighter = GetComponent<TipHighlighter>() ?? gameObject.AddComponent<TipHighlighter>();
            }
        }

        private void OnEnable()
        {
            Arrow.OnArrowFiredEvent += HandleSuggestedArrowFired;
            GameManager.OnGameStateChanged += HandleGameState;
            LevelManager.OnLevelLoading += HandleLevelLoading;
        }

        private void OnDisable()
        {
            Arrow.OnArrowFiredEvent -= HandleSuggestedArrowFired;
            GameManager.OnGameStateChanged -= HandleGameState;
            LevelManager.OnLevelLoading -= HandleLevelLoading;
            ClearSuggestion();
        }

        private void HandleSuggestedArrowFired(Arrow arrow) { if (arrow == _suggestedArrow) ClearSuggestion(); }
        private void HandleLevelLoading(int level) => ClearSuggestion();
        private void HandleGameState(GameState state)
        { if (state != GameState.Playing && state != GameState.Paused) ClearSuggestion(); }

        private bool SuggestionIsValid()
        {
            if (_suggestedArrow == null || !_suggestedArrow.isActiveAndEnabled || _suggestedArrow.IsFired ||
                _suggestedArrow.HeadDirection != _suggestedDirection || _suggestedArrow.PathPoints.Count != _suggestedPoints.Count ||
                !ArrowSpawner.HasInstance || !GridManager.HasInstance) return false;
            for (int i = 0; i < _suggestedPoints.Count; i++)
                if (_suggestedArrow.PathPoints[i] != _suggestedPoints[i]) return false;
            var arrows = ArrowSpawner.Instance.ActiveArrows;
            for (int i = 0; i < arrows.Count; i++)
                if (arrows[i] == _suggestedArrow)
                    return GridManager.Instance.IsPathClear(_suggestedArrow.HeadPoint, _suggestedDirection);
            return false;
        }

        private void ClearSuggestion()
        {
            _suggestedArrow = null; _suggestedPoints.Clear();
            if (_highlighter != null) _highlighter.ClearHighlight();
        }

        /// <summary>
        /// Uses a tip to highlight the best arrow.
        /// </summary>
        public void UseTip()
        {
            if (GameManager.Instance?.CurrentState != GameState.Playing) return;
            if (HasPendingSuggestion)
            {
                _highlighter?.Highlight(_suggestedArrow);
                return;
            }

            PlayerData data = DataManager.Instance?.PlayerData;
            if (data == null) return;

            if (data.tipCount <= 0)
            {
                OnNoTipsAvailable?.Invoke();
                LogDebug("No tips available!");
                return;
            }

            Arrow bestArrow = FindBestArrow();
            if (bestArrow == null)
            {
                LogDebug("No valid arrow to highlight.");
                return;
            }

            // Record before inventory events: a re-entrant or rapid request must reuse this suggestion.
            _suggestedArrow = bestArrow; _suggestedDirection = bestArrow.HeadDirection;
            _suggestedPoints.Clear();
            for (int i = 0; i < bestArrow.PathPoints.Count; i++) _suggestedPoints.Add(bestArrow.PathPoints[i]);
            DataManager.Instance.ModifyTipCount(-1);
            OnTipUsed?.Invoke(data.tipCount);

            // Highlight the arrow
            if (_highlighter == null)
            {
                _highlighter = GetComponent<TipHighlighter>() ?? gameObject.AddComponent<TipHighlighter>();
            }
            _highlighter?.Highlight(bestArrow);

            LogDebug($"Tip used! Highlighted arrow at {bestArrow.HeadPoint}. Tips remaining: {data.tipCount}");
        }

        /// <summary>
        /// Finds the best arrow to fire: head must be on grid edge facing outward,
        /// and among those, picks the one with highest weight.
        /// </summary>
        private Arrow FindBestArrow()
        {
            var activeArrows = ArrowSpawner.Instance?.ActiveArrows;
            if (activeArrows == null || activeArrows.Count == 0) return null;

            Arrow best = null;
            int bestWeight = -1;

            for (int i = 0; i < activeArrows.Count; i++)
            {
                Arrow arrow = activeArrows[i];
                if (arrow.IsFired) continue;

                bool canFire = GridManager.Instance.IsPathClear(
                    arrow.HeadPoint, arrow.HeadDirection);

                if (canFire && arrow.Weight > bestWeight)
                {
                    best = arrow;
                    bestWeight = arrow.Weight;
                }
            }

            return best;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] TipManager: {message}");
        }
    }
}
