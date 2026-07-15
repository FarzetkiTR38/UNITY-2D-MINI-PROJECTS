using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

namespace NeonGalaxy.Boot
{
    /// <summary>
    /// Controls the loading screen UI in the Boot scene.
    /// Manages fill bar animation via Image.fillAmount, percentage text,
    /// and fade-out transition when loading completes.
    /// 
    /// Attach to the LoadingScreenCanvas root object in Boot scene.
    /// Requires a CanvasGroup on the same GameObject for fade-out.
    /// </summary>
    public class LoadingScreenController : MonoBehaviour
    {
        [Header("Loading Bar")]
        [Tooltip("The fill image (Image Type = Filled, Horizontal, Left). Shows loading progress.")]
        [SerializeField] private Image fillImage;

        [Tooltip("The background bar image (always fully visible behind the fill).")]
        [SerializeField] private Image backgroundImage;

        [Header("Text")]
        [Tooltip("Displays the current loading percentage (e.g. '12%').")]
        [SerializeField] private TextMeshProUGUI percentText;

        [Header("Fade Out")]
        [Tooltip("CanvasGroup on this object — used for alpha fade-out when loading completes.")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Tooltip("Duration of the fade-out animation in seconds.")]
        [SerializeField] private float fadeOutDuration = 1f;

        [Header("Animation")]
        [Tooltip("How fast the fill bar lerps toward its target value. Higher = faster.")]
        [SerializeField] private float lerpSpeed = 3f;

        // ── Internal State ──────────────────────────────────────────
        private float _targetProgress;
        private float _currentProgress;
        private bool _isFading;

        private void Awake()
        {
            Debug.Log($"[LoadingScreen] Awake — fillImage={fillImage != null}, bgImage={backgroundImage != null}, percentText={percentText != null}, canvasGroup={canvasGroup != null}");

            // Ensure fill image is configured correctly
            if (fillImage != null)
            {
                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Horizontal;
                fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                fillImage.fillAmount = 0f;
                Debug.Log($"[LoadingScreen] fillImage configured: type={fillImage.type}, fillMethod={fillImage.fillMethod}, sprite={fillImage.sprite?.name ?? "NULL"}");
            }
            else
            {
                Debug.LogError("[LoadingScreen] fillImage is NULL! Assign it in Inspector.");
            }

            // Ensure canvas group is fully visible at start
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
            }

            _currentProgress = 0f;
            _targetProgress = 0f;
            UpdateUI(0f);
        }

        private void Update()
        {
            if (_isFading) return;

            // Smoothly lerp current progress toward target
            if (!Mathf.Approximately(_currentProgress, _targetProgress))
            {
                float prevProgress = _currentProgress;
                _currentProgress = Mathf.MoveTowards(
                    _currentProgress,
                    _targetProgress,
                    lerpSpeed * Time.deltaTime
                );

                // Snap if very close
                if (Mathf.Abs(_currentProgress - _targetProgress) < 0.001f)
                    _currentProgress = _targetProgress;

                // Log every ~10% change to avoid spam
                int prevPercent = Mathf.RoundToInt(prevProgress * 10f);
                int curPercent = Mathf.RoundToInt(_currentProgress * 10f);
                if (prevPercent != curPercent)
                    Debug.Log($"[LoadingScreen] Update lerp: {_currentProgress:F2} → target {_targetProgress:F2} (fillAmount will be {_currentProgress:F2})");

                UpdateUI(_currentProgress);
            }
        }

        /// <summary>
        /// Sets the target progress for the loading bar.
        /// The bar will smoothly animate toward this value.
        /// </summary>
        /// <param name="normalizedTarget">Progress value between 0f and 1f.</param>
        public void SetProgress(float normalizedTarget)
        {
            _targetProgress = Mathf.Clamp01(normalizedTarget);
            Debug.Log($"[LoadingScreen] SetProgress({normalizedTarget:F2}) → target={_targetProgress:F2}, current={_currentProgress:F2}");
        }

        /// <summary>
        /// Returns true if the visual progress has caught up to the target.
        /// Useful for waiting until the bar animation finishes before fading out.
        /// </summary>
        public bool HasReachedTarget()
        {
            return Mathf.Approximately(_currentProgress, _targetProgress);
        }

        /// <summary>
        /// Fades out the entire loading screen via CanvasGroup alpha,
        /// then invokes the onComplete callback.
        /// </summary>
        /// <param name="onComplete">Called after fade-out finishes.</param>
        public void FadeOutAndComplete(Action onComplete)
        {
            if (canvasGroup == null)
            {
                Debug.LogWarning("[LoadingScreenController] No CanvasGroup assigned. Skipping fade.");
                onComplete?.Invoke();
                return;
            }

            StartCoroutine(FadeOutCoroutine(onComplete));
        }

        // ── Private Helpers ─────────────────────────────────────────

        private void UpdateUI(float progress)
        {
            // Update fill bar
            if (fillImage != null)
                fillImage.fillAmount = progress;

            // Update percentage text
            if (percentText != null)
            {
                int percent = Mathf.RoundToInt(progress * 100f);
                percentText.text = $"{percent}%";
            }
        }

        private IEnumerator FadeOutCoroutine(Action onComplete)
        {
            _isFading = true;

            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeOutDuration);
                // Ease out quad for a smooth feel
                float easedT = 1f - (1f - t) * (1f - t);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, easedT);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;

            onComplete?.Invoke();

            // Destroy loading screen after transition — it's no longer needed
            Destroy(gameObject);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Preview in editor
            if (fillImage != null)
            {
                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Horizontal;
                fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            }
        }
#endif
    }
}
