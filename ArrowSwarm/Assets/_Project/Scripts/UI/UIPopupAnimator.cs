namespace ArrowSwarm.UI
{
    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Provides smooth, juice-rich elastic pop-in and pop-out animations for UI dialog boxes.
    /// Uses unscaled delta time so animations remain fluid when Time.timeScale is zero (e.g. PauseMenu).
    /// </summary>
    public static class UIPopupAnimator
    {
        private const float DefaultOpenDuration = 0.24f;
        private const float DefaultCloseDuration = 0.16f;

        /// <summary>
        /// Plays an elastic pop-in animation (scale 0.75 -> 1.05 -> 1.0 with alpha fade in).
        /// </summary>
        public static IEnumerator AnimateOpen(
            RectTransform dialogBox,
            CanvasGroup canvasGroup,
            float duration = DefaultOpenDuration,
            Action onComplete = null)
        {
            Vector3 baseScale = Vector3.one;
            if (dialogBox != null)
            {
                dialogBox.localScale = baseScale * 0.75f;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = true;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Alpha fade
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.Min(1f, t * 1.6f);
                }

                // Elastic Back.Out scale: 0.75 -> 1.05 -> 1.0
                if (dialogBox != null)
                {
                    float p = t - 1f;
                    float c1 = 1.6f;
                    float c3 = c1 + 1f;
                    float easeBackOut = 1f + c3 * p * p * p + c1 * p * p;
                    float scaleMult = Mathf.LerpUnclamped(0.75f, 1f, easeBackOut);
                    dialogBox.localScale = baseScale * scaleMult;
                }

                yield return null;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            if (dialogBox != null)
            {
                dialogBox.localScale = baseScale;
            }

            onComplete?.Invoke();
        }

        /// <summary>
        /// Plays a snappy pop-out animation (scale 1.0 -> 1.02 -> 0.8 with alpha fade out).
        /// </summary>
        public static IEnumerator AnimateClose(
            RectTransform dialogBox,
            CanvasGroup canvasGroup,
            float duration = DefaultCloseDuration,
            Action onComplete = null)
        {
            Vector3 baseScale = dialogBox != null ? dialogBox.localScale : Vector3.one;

            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f - t;
                }

                if (dialogBox != null)
                {
                    // Slight anticipation then smooth shrink
                    float shrink = t < 0.2f
                        ? Mathf.Lerp(1f, 1.03f, t / 0.2f)
                        : Mathf.Lerp(1.03f, 0.75f, (t - 0.2f) / 0.8f);
                    dialogBox.localScale = baseScale * shrink;
                }

                yield return null;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
            }

            if (dialogBox != null)
            {
                dialogBox.localScale = baseScale;
            }

            onComplete?.Invoke();
        }
    }
}
