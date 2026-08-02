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
        [SerializeField] private float _highlightDuration = 3f;
        [SerializeField] private float _pulseSpeed = 4f;

        private Arrow _highlightedArrow;
        private float _timer;
        private bool _isHighlighting;

        /// <summary>
        /// Highlights the given arrow with a glowing pulse effect.
        /// </summary>
        public void Highlight(Arrow arrow)
        {
            // Clear previous highlight
            ClearHighlight();

            _highlightedArrow = arrow;
            _timer = _highlightDuration;
            _isHighlighting = true;

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
            _isHighlighting = false;
        }

        private void Update()
        {
            if (!_isHighlighting) return;

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                ClearHighlight();
            }
        }

        private void OnDisable()
        {
            ClearHighlight();
        }
    }
}
