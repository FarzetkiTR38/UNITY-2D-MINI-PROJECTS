namespace ArrowSwarm.UI
{
    using System;
    using System.Collections;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Manages the Boot / Loading Screen UI visuals, animated progress bar (0-100%),
    /// rotating gameplay tips, status messages, and center hero pulse animation.
    /// Provides customizable sprite and color slots for easy designer customization.
    /// </summary>
    public class BootLoadingUI : MonoBehaviour
    {
        [Header("Visual Slots (Sprites & Graphics)")]
        [Tooltip("Background wallpaper image.")]
        [SerializeField] private Image _backgroundImage;

        [Tooltip("Game title logo (e.g. Arrow Swarm 3D Logo).")]
        [SerializeField] private Image _titleLogoImage;

        [Tooltip("Glow badge under title (e.g. LOADING badge).")]
        [SerializeField] private Image _loadingBadgeImage;

        [Tooltip("Center circular arrows wheel graphic.")]
        [SerializeField] private RectTransform _centerHeroTransform;
        [SerializeField] private Image _centerHeroImage;

        [Tooltip("Glowing center star graphic.")]
        [SerializeField] private Image _centerStarImage;

        [Tooltip("Bottom company/studio branding logo.")]
        [SerializeField] private Image _brandingLogoImage;

        [Header("Progress Bar & Status")]
        [SerializeField] private Slider _progressSlider;
        [SerializeField] private Image _progressFillImage;
        [SerializeField] private TextMeshProUGUI _percentText;
        [SerializeField] private TextMeshProUGUI _statusText;

        [Header("Tips")]
        [SerializeField] private TextMeshProUGUI _tipText;
        [SerializeField] private Image _tipIcon;
        [SerializeField] private CanvasGroup _tipCanvasGroup;

        [Header("Animation & Timing")]
        [Tooltip("Simulated load duration in seconds.")]
        [SerializeField] private float _loadingDuration = 2.4f;

        [Tooltip("Whether the center hero graphic plays an ambient breathing animation.")]
        [SerializeField] private bool _animateCenterHero = true;
        [SerializeField] private float _heroPulseSpeed = 2.5f;
        [SerializeField] private float _heroPulseAmount = 0.035f;

        [Header("Text Configuration")]
        [SerializeField] private string[] _statusSteps = new string[]
        {
            "Initializing systems...",
            "Preparing arrows...",
            "Calibrating swarm path...",
            "Almost ready!"
        };

        [SerializeField] private string[] _gameplayTips = new string[]
        {
            "TIP: Match arrow paths to guide every bot!",
            "TIP: Blocked arrows bounce back safely without losing lives!",
            "TIP: Use Freeze skill when enemies get too close to the portal!",
            "TIP: Solvable arrows always have an unblocked exit path!",
            "TIP: The final Rainbow Arrow clears the remaining wave in style!"
        };

        private Coroutine _loadingCoroutine;
        private Coroutine _tipsCoroutine;
        private Vector3 _originalHeroScale = Vector3.one;

        private void Awake()
        {
            if (_centerHeroTransform != null)
            {
                _originalHeroScale = _centerHeroTransform.localScale;
            }

            SetProgress(0f);
        }

        private void Start()
        {
            if (_gameplayTips != null && _gameplayTips.Length > 0 && _tipText != null)
            {
                _tipText.text = _gameplayTips[UnityEngine.Random.Range(0, _gameplayTips.Length)];
            }

            _tipsCoroutine = StartCoroutine(CycleTipsRoutine());
        }

        private void Update()
        {
            if (_animateCenterHero && _centerHeroTransform != null)
            {
                float pulse = 1f + Mathf.Sin(Time.time * _heroPulseSpeed) * _heroPulseAmount;
                _centerHeroTransform.localScale = _originalHeroScale * pulse;
            }
        }

        /// <summary>
        /// Starts the animated loading progression from 0% to 100%.
        /// Calls onComplete when 100% is reached and ready to transition.
        /// </summary>
        public void StartLoading(Action onComplete)
        {
            if (_loadingCoroutine != null) StopCoroutine(_loadingCoroutine);
            _loadingCoroutine = StartCoroutine(LoadingSequenceRoutine(onComplete));
        }

        /// <summary>
        /// Updates the progress bar fill and percentage text (0.0 to 1.0).
        /// </summary>
        public void SetProgress(float progress)
        {
            float clamped = Mathf.Clamp01(progress);

            if (_progressSlider != null)
            {
                _progressSlider.value = clamped;
            }

            if (_progressFillImage != null && _progressFillImage.type == Image.Type.Filled)
            {
                _progressFillImage.fillAmount = clamped;
            }

            if (_percentText != null)
            {
                _percentText.text = $"{Mathf.RoundToInt(clamped * 100f)}%";
            }

            UpdateStatusMessage(clamped);
        }

        private void UpdateStatusMessage(float progress)
        {
            if (_statusText == null || _statusSteps == null || _statusSteps.Length == 0) return;

            int stepIndex = Mathf.Clamp(Mathf.FloorToInt(progress * _statusSteps.Length), 0, _statusSteps.Length - 1);
            if (progress >= 0.98f) stepIndex = _statusSteps.Length - 1;

            _statusText.text = _statusSteps[stepIndex];
        }

        private IEnumerator LoadingSequenceRoutine(Action onComplete)
        {
            float elapsed = 0f;
            float duration = Mathf.Max(0.5f, _loadingDuration);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Smooth cubic ease-out curve for natural loading feel
                float smoothT = 1f - Mathf.Pow(1f - t, 3f);
                SetProgress(smoothT);

                yield return null;
            }

            SetProgress(1.0f);
            yield return new WaitForSeconds(0.25f);

            onComplete?.Invoke();
        }

        private IEnumerator CycleTipsRoutine()
        {
            if (_gameplayTips == null || _gameplayTips.Length <= 1) yield break;

            int tipIndex = UnityEngine.Random.Range(0, _gameplayTips.Length);
            WaitForSeconds waitInterval = new WaitForSeconds(3.5f);

            while (true)
            {
                yield return waitInterval;

                // Fade out
                if (_tipCanvasGroup != null)
                {
                    float fadeOut = 0.25f;
                    float elapsed = 0f;
                    while (elapsed < fadeOut)
                    {
                        elapsed += Time.deltaTime;
                        _tipCanvasGroup.alpha = 1f - (elapsed / fadeOut);
                        yield return null;
                    }
                    _tipCanvasGroup.alpha = 0f;
                }

                // Change tip
                tipIndex = (tipIndex + 1) % _gameplayTips.Length;
                if (_tipText != null)
                {
                    _tipText.text = _gameplayTips[tipIndex];
                }

                // Fade in
                if (_tipCanvasGroup != null)
                {
                    float fadeIn = 0.25f;
                    float elapsed = 0f;
                    while (elapsed < fadeIn)
                    {
                        elapsed += Time.deltaTime;
                        _tipCanvasGroup.alpha = elapsed / fadeIn;
                        yield return null;
                    }
                    _tipCanvasGroup.alpha = 1f;
                }
            }
        }

        private void OnDestroy()
        {
            if (_loadingCoroutine != null) StopCoroutine(_loadingCoroutine);
            if (_tipsCoroutine != null) StopCoroutine(_tipsCoroutine);
        }
    }
}
