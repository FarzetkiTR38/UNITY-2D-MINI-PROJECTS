namespace ArrowSwarm.Tips
{
    using System;
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

        /// <summary>Fired when a tip is used (remaining tip count).</summary>
        public static event Action<int> OnTipUsed;

        /// <summary>Fired when tip count is zero and user tries to use.</summary>
        public static event Action OnNoTipsAvailable;

        /// <summary>
        /// Uses a tip to highlight the best arrow.
        /// </summary>
        public void UseTip()
        {
            if (GameManager.Instance?.CurrentState != GameState.Playing) return;

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

            // Use the tip
            DataManager.Instance.ModifyTipCount(-1);
            OnTipUsed?.Invoke(data.tipCount);

            // Highlight the arrow
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
