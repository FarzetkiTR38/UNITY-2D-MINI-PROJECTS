namespace ArrowSwarm.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Restores authored feedback baselines when a puzzle attempt resets.</summary>
    public partial class HUDFeedbackEffects
    {
        private void CacheOriginalHeartTransforms()
        {
            if (_heartIcons == null || _heartIcons.Length == 0) return;
            if (_cachedHeartIcons != null && _cachedHeartIcons.Length == _heartIcons.Length)
            {
                bool unchanged = true;
                for (int i = 0; i < _heartIcons.Length; i++)
                    if (_cachedHeartIcons[i] != _heartIcons[i]) { unchanged = false; break; }
                if (unchanged) return;
            }
            // Initialize can replace the references after Awake. Keep snapshots for unchanged icons
            // so an in-progress feedback animation can never become the new authored baseline.
            var previousIcons = _cachedHeartIcons;
            var previousPositions = _heartOriginalPositions;
            var previousColors = _heartOriginalColors;
            var previousScales = _heartOriginalScales;
            var previousRotations = _heartOriginalRotations;
            _cachedHeartIcons = (Image[])_heartIcons.Clone();
            _heartOriginalPositions = new Vector3[_heartIcons.Length];
            _heartOriginalColors = new Color[_heartIcons.Length];
            _heartOriginalScales = new Vector3[_heartIcons.Length];
            _heartOriginalRotations = new Quaternion[_heartIcons.Length];

            for (int i = 0; i < _heartIcons.Length; i++)
            {
                if (_heartIcons[i] != null)
                {
                    int previous = -1;
                    if (previousIcons != null)
                        for (int j = 0; j < previousIcons.Length; j++)
                            if (previousIcons[j] == _heartIcons[i]) { previous = j; break; }
                    _heartOriginalPositions[i] = previous >= 0 ? previousPositions[previous] : _heartIcons[i].rectTransform.anchoredPosition3D;
                    _heartOriginalColors[i] = previous >= 0 ? previousColors[previous] : _heartIcons[i].color;
                    _heartOriginalScales[i] = previous >= 0 ? previousScales[previous] : _heartIcons[i].transform.localScale;
                    _heartOriginalRotations[i] = previous >= 0 ? previousRotations[previous] : _heartIcons[i].transform.localRotation;
                }
            }
        }

        private void CacheOriginalArrowTransform()
        {
            if (_arrowCountText == null || _cachedArrowCountText == _arrowCountText) return;
            _cachedArrowCountText = _arrowCountText;
            _arrowOriginalScale = _arrowCountText.transform.localScale;
        }

        /// <summary>
        /// Silently restores a new attempt or rewarded continuation. Stops outstanding heart breaks,
        /// low-life pulses and counter punches before restoring the authored transforms and colors.
        /// A subsequent UpdateLives call may resume the normal one-life heartbeat.
        /// </summary>
        public void ResetFeedback(int lives)
        {
            StopAllCoroutines();
            _heartbeatCoroutine = null;
            _arrowPunchCoroutine = null;
            CacheOriginalHeartTransforms();
            CacheOriginalArrowTransform();
            _previousLives = Mathf.Max(0, lives);
            if (_heartIcons != null)
                for (int i = 0; i < _heartIcons.Length; i++)
                {
                    if (_heartIcons[i] == null) continue;
                    var rt = _heartIcons[i].rectTransform;
                    rt.anchoredPosition3D = _heartOriginalPositions[i];
                    rt.localScale = _heartOriginalScales[i];
                    rt.localRotation = _heartOriginalRotations[i];
                    _heartIcons[i].color = _heartOriginalColors[i];
                    _heartIcons[i].gameObject.SetActive(i < _previousLives);
                }
            if (_arrowCountText != null) _arrowCountText.transform.localScale = _arrowOriginalScale;
        }

    }
}
