namespace ArrowSwarm.UI
{
    using System;
    using System.Collections;
    using ArrowSwarm.Localization;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Manages the Boot / Loading Screen UI visuals, animated progress bar (0-100%),
    /// rotating localized gameplay tips, and localized status messages during startup.
    /// </summary>
    public class BootLoadingUI : MonoBehaviour
    {
        [Header("Visual Slots (Sprites & Graphics)")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _titleLogoImage;
        [SerializeField] private Image _loadingBadgeImage;
        [SerializeField] private RectTransform _centerHeroTransform;
        [SerializeField] private Image _centerHeroImage;
        [SerializeField] private Image _centerStarImage;
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

        [Header("Hero Animation & Timing")]
        [SerializeField] private bool _animateCenterHero = true;
        [SerializeField] private float _heroPulseSpeed = 2.5f;
        [SerializeField] private float _heroPulseAmount = 0.035f;
        [SerializeField] private float _loadingDuration = 2.4f;

        private static readonly string[] StatusKeys = { "boot_status_0", "boot_status_1", "boot_status_2", "boot_status_3" };
        private static readonly string[] StatusFallbacks = { "Initializing systems...", "Preparing arrows...", "Calibrating swarm path...", "Almost ready!" };

        private static readonly string[] TipKeys = { "boot_tip_0", "boot_tip_1", "boot_tip_2", "boot_tip_3", "boot_tip_4" };
        private static readonly string[] TipFallbacks =
        {
            "TIP: Match arrow paths to guide every bot!", "TIP: Blocked arrows bounce back safely without losing lives!",
            "TIP: Use Freeze skill when enemies get too close to the portal!", "TIP: Solvable arrows always have an unblocked exit path!",
            "TIP: The final Rainbow Arrow clears the remaining wave in style!"
        };

        private Coroutine _loadingCoroutine;
        private Coroutine _tipsCoroutine;
        private Vector3 _originalHeroScale = Vector3.one;
        private int _currentStatusIndex;
        private int _currentTipIndex;

        private void Awake()
        {
            if (_centerHeroTransform != null) _originalHeroScale = _centerHeroTransform.localScale;

            DisableStaticLocalizedText(_statusText);
            DisableStaticLocalizedText(_tipText);

            _currentTipIndex = UnityEngine.Random.Range(0, TipKeys.Length);
            UpdateTipMessage(_currentTipIndex);
            SetProgress(0f);
        }

        private void DisableStaticLocalizedText(TextMeshProUGUI tmp)
        {
            if (tmp != null && tmp.TryGetComponent<LocalizedText>(out var loc))
            {
                loc.enabled = false;
            }
        }

        private void OnEnable()
        {
            LocalizationManager.OnLanguageChanged += RefreshLocalizedTexts;
            RefreshLocalizedTexts();
        }

        private void OnDisable()
        {
            LocalizationManager.OnLanguageChanged -= RefreshLocalizedTexts;
        }

        private void Start()
        {
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

        /// <summary>Starts loading sequence progression from 0% to 100%.</summary>
        public void StartLoading(Action onComplete)
        {
            if (_loadingCoroutine != null) StopCoroutine(_loadingCoroutine);
            _loadingCoroutine = StartCoroutine(LoadingSequenceRoutine(onComplete));
        }

        /// <summary>Updates progress bar fill and percentage text (0.0 to 1.0).</summary>
        public void SetProgress(float progress)
        {
            float clamped = Mathf.Clamp01(progress);
            if (_progressSlider != null) _progressSlider.value = clamped;
            if (_progressFillImage != null && _progressFillImage.type == Image.Type.Filled) _progressFillImage.fillAmount = clamped;
            if (_percentText != null) _percentText.text = $"{Mathf.RoundToInt(clamped * 100f)}%";
            UpdateStatusMessage(clamped);
        }

        private void UpdateStatusMessage(float progress)
        {
            if (_statusText == null) return;
            _currentStatusIndex = Mathf.Clamp(Mathf.FloorToInt(progress * StatusKeys.Length), 0, StatusKeys.Length - 1);
            if (progress >= 0.98f) _currentStatusIndex = StatusKeys.Length - 1;
            _statusText.text = GetLocalized(StatusKeys[_currentStatusIndex], StatusFallbacks[_currentStatusIndex]);
        }

        private void UpdateTipMessage(int index)
        {
            if (_tipText == null) return;
            _currentTipIndex = Mathf.Clamp(index, 0, TipKeys.Length - 1);
            _tipText.text = GetLocalized(TipKeys[_currentTipIndex], TipFallbacks[_currentTipIndex]);
        }

        private string GetLocalized(string key, string fallback)
        {
            return LocalizationManager.Instance != null ? LocalizationManager.Instance.GetText(key, fallback) : fallback;
        }

        private void RefreshLocalizedTexts()
        {
            if (_statusText != null) _statusText.text = GetLocalized(StatusKeys[_currentStatusIndex], StatusFallbacks[_currentStatusIndex]);
            if (_tipText != null) _tipText.text = GetLocalized(TipKeys[_currentTipIndex], TipFallbacks[_currentTipIndex]);
        }

        private IEnumerator LoadingSequenceRoutine(Action onComplete)
        {
            float elapsed = 0f;
            float duration = Mathf.Max(0.5f, _loadingDuration);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
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
            WaitForSeconds waitInterval = new WaitForSeconds(3.5f);
            while (true)
            {
                yield return waitInterval;
                if (_tipCanvasGroup != null) yield return FadeCanvasGroup(_tipCanvasGroup, 1f, 0f, 0.25f);
                _currentTipIndex = (_currentTipIndex + 1) % TipKeys.Length;
                UpdateTipMessage(_currentTipIndex);
                if (_tipCanvasGroup != null) yield return FadeCanvasGroup(_tipCanvasGroup, 0f, 1f, 0.25f);
            }
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            cg.alpha = to;
        }

        private void OnDestroy()
        {
            if (_loadingCoroutine != null) StopCoroutine(_loadingCoroutine);
            if (_tipsCoroutine != null) StopCoroutine(_tipsCoroutine);
            LocalizationManager.OnLanguageChanged -= RefreshLocalizedTexts;
        }
    }
}
