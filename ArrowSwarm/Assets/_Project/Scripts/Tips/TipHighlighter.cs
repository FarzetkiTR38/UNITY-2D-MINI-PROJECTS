namespace ArrowSwarm.Tips
{
    using ArrowSwarm.Arrow;
    using UnityEngine;

    /// <summary>
    /// Handles the visual highlighting of a tipped arrow.
    /// Shows a glowing pulse effect on the suggested arrow.
    /// </summary>
    public class TipHighlighter : MonoBehaviour
    {
        private Arrow _highlightedArrow;

        private void OnEnable()
        {
            Arrow.OnArrowFiredEvent += HandleArrowFired;
        }

        private void OnDisable()
        {
            Arrow.OnArrowFiredEvent -= HandleArrowFired;
            ClearHighlight();
        }

        private void HandleArrowFired(Arrow arrow)
        {
            if (_highlightedArrow != null && _highlightedArrow == arrow)
            {
                ClearHighlight();
            }
        }

        /// <summary>
        /// Highlights the given arrow with a glowing pulse effect.
        /// </summary>
        public void Highlight(Arrow arrow)
        {
            if (arrow != null && arrow == _highlightedArrow && arrow.isActiveAndEnabled && !arrow.IsFired) return;
            // Clear previous highlight
            ClearHighlight();

            if (arrow == null || !arrow.isActiveAndEnabled || arrow.IsFired) return;

            _highlightedArrow = arrow;

            // Use ArrowVisuals to set highlight mode
            var visuals = arrow.GetComponent<ArrowVisuals>();
            visuals?.SetHighlight(true);
        }

        /// <summary>
        /// Clears the current highlight.
        /// </summary>
        public void ClearHighlight()
        {
            if (_highlightedArrow != null)
            {
                var visuals = _highlightedArrow.GetComponent<ArrowVisuals>();
                visuals?.SetHighlight(false);
            }

            _highlightedArrow = null;
        }

        private void Update()
        {
            // A paid hint remains readable until its arrow is used or returned to the pool.
            if (_highlightedArrow != null && (!_highlightedArrow.isActiveAndEnabled ||
                _highlightedArrow.IsFired || _highlightedArrow.PathPoints.Count < 2)) ClearHighlight();
        }
    }
}
