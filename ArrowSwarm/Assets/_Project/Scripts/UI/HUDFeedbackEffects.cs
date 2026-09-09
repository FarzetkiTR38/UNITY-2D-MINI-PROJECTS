namespace ArrowSwarm.UI
{
    using System.Collections;
    using System.Collections.Generic;
    using ArrowSwarm.Audio;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Handles micro-animations for HUD elements:
    /// - Heart shatter/break shake and pop on life loss
    /// - Low HP rhythmic heartbeat pulse on last heart
    /// - Arrow counter scale punch on fire
    /// Zero GC allocations in Update.
    /// </summary>
    public class HUDFeedbackEffects : MonoBehaviour
    {
        [Header("Heart References")]
        [SerializeField] private Image[] _heartIcons;
        [Header("Arrow Counter")]
        [SerializeField] private TextMeshProUGUI _arrowCountText;

        private int _previousLives = 3;
        private Coroutine _heartbeatCoroutine;
        private Coroutine _arrowPunchCoroutine;
        private Vector3[] _heartOriginalPositions;
        private Color[] _heartOriginalColors;

        private void Awake()
        {
            if (_heartIcons == null || _heartIcons.Length == 0)
            {
                var list = new List<Image>();
                for (int i = 0; i < 5; i++)
                {
                    var t = transform.Find($"TopBar/Heart_{i}") ?? transform.Find($"TopPanel/Heart_{i}") ?? transform.Find($"Heart_{i}");
                    if (t != null && t.TryGetComponent<Image>(out var img)) list.Add(img);
                }
                if (list.Count > 0) _heartIcons = list.ToArray();
            }
            if (_arrowCountText == null)
            {
                _arrowCountText = transform.Find("BottomBar/ArrowText")?.GetComponent<TextMeshProUGUI>()
                               ?? transform.Find("BottomPanel/ArrowText")?.GetComponent<TextMeshProUGUI>();
            }
            CacheOriginalHeartTransforms();
        }

        /// <summary>
        /// Initializes references from GameHUD.
        /// </summary>
        public void Initialize(Image[] heartIcons, TextMeshProUGUI arrowCountText)
        {
            if (heartIcons != null && heartIcons.Length > 0) _heartIcons = heartIcons;
            if (arrowCountText != null) _arrowCountText = arrowCountText;
            CacheOriginalHeartTransforms();
        }

        private void CacheOriginalHeartTransforms()
        {
            if (_heartIcons == null || _heartIcons.Length == 0) return;
            _heartOriginalPositions = new Vector3[_heartIcons.Length];
            _heartOriginalColors = new Color[_heartIcons.Length];

            for (int i = 0; i < _heartIcons.Length; i++)
            {
                if (_heartIcons[i] != null)
                {
                    _heartOriginalPositions[i] = _heartIcons[i].rectTransform.anchoredPosition;
                    _heartOriginalColors[i] = _heartIcons[i].color;
                }
            }
        }

        /// <summary>
        /// Updates the heart display with shatter animations on damage or reset.
        /// </summary>
        public void UpdateLives(int lives)
        {
            if (_heartIcons == null || _heartIcons.Length == 0) return;
            if (_heartOriginalPositions == null) CacheOriginalHeartTransforms();

            if (lives < _previousLives)
            {
                for (int i = _previousLives - 1; i >= lives && i >= 0 && i < _heartIcons.Length; i--)
                {
                    if (_heartIcons[i] != null && _heartIcons[i].gameObject.activeSelf)
                        StartCoroutine(HeartBreakRoutine(_heartIcons[i], i));
                }
                AudioManager.Instance?.PlayHeartBreak();
            }
            else
            {
                for (int i = 0; i < _heartIcons.Length; i++)
                {
                    if (_heartIcons[i] != null)
                    {
                        _heartIcons[i].gameObject.SetActive(i < lives);
                        _heartIcons[i].transform.localScale = Vector3.one;
                        if (_heartOriginalPositions != null && i < _heartOriginalPositions.Length)
                        {
                            _heartIcons[i].rectTransform.anchoredPosition = _heartOriginalPositions[i];
                            _heartIcons[i].color = _heartOriginalColors[i];
                        }
                    }
                }
            }

            _previousLives = lives;

            if (lives == 1)
            {
                if (_heartbeatCoroutine == null && _heartIcons.Length > 0 && _heartIcons[0] != null)
                    _heartbeatCoroutine = StartCoroutine(HeartbeatRoutine(_heartIcons[0]));
            }
            else if (_heartbeatCoroutine != null)
            {
                StopCoroutine(_heartbeatCoroutine);
                _heartbeatCoroutine = null;
                if (_heartIcons.Length > 0 && _heartIcons[0] != null) _heartIcons[0].transform.localScale = Vector3.one;
            }
        }

        private IEnumerator HeartBreakRoutine(Image heartImage, int index)
        {
            RectTransform rt = heartImage.rectTransform;
            Vector3 origPos = _heartOriginalPositions != null && index < _heartOriginalPositions.Length ? _heartOriginalPositions[index] : rt.anchoredPosition;
            Color origColor = _heartOriginalColors != null && index < _heartOriginalColors.Length ? _heartOriginalColors[index] : heartImage.color;
            Color flashGold = new Color(1f, 0.95f, 0.5f, 1f);
            Color amberEnd = new Color(1f, 0.62f, 0.12f, 0f);

            float elapsed = 0f, duration = 0.35f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                rt.anchoredPosition = (Vector2)origPos + Random.insideUnitCircle * ((1f - t) * 3.5f);
                rt.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(0f, 75f, t));

                float scale = t < 0.32f ? Mathf.Lerp(1f, 1.35f, t / 0.32f) : Mathf.Lerp(1.35f, 0f, (t - 0.32f) / 0.68f);
                rt.localScale = Vector3.one * scale;

                heartImage.color = t < 0.25f 
                    ? Color.Lerp(origColor, flashGold, t / 0.25f) 
                    : Color.Lerp(flashGold, amberEnd, (t - 0.25f) / 0.75f);
                yield return null;
            }

            heartImage.gameObject.SetActive(false);
            rt.anchoredPosition = origPos;
            rt.localScale = Vector3.one;
            rt.localEulerAngles = Vector3.zero;
            heartImage.color = origColor;
        }

        private IEnumerator HeartbeatRoutine(Image heartImage)
        {
            Transform tr = heartImage.transform;
            while (true)
            {
                float pulse = 1f + 0.12f * Mathf.Sin(Time.unscaledTime * 4.2f);
                tr.localScale = Vector3.one * pulse;
                yield return null;
            }
        }

        /// <summary>
        /// Punches the arrow counter scale when an arrow is fired.
        /// </summary>
        public void PunchArrowCount()
        {
            if (_arrowCountText == null) return;
            if (_arrowPunchCoroutine != null) StopCoroutine(_arrowPunchCoroutine);
            _arrowPunchCoroutine = StartCoroutine(ArrowPunchRoutine());
        }

        private IEnumerator ArrowPunchRoutine()
        {
            Transform tr = _arrowCountText.transform;
            float elapsed = 0f, duration = 0.16f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                tr.localScale = Vector3.one * (1f + 0.22f * Mathf.Sin(t * Mathf.PI));
                yield return null;
            }
            tr.localScale = Vector3.one;
            _arrowPunchCoroutine = null;
        }
    }
}
