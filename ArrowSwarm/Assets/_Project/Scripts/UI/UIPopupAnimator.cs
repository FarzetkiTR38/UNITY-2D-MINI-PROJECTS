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
        internal const float DefaultOpenDuration = 0.24f;
        private const float DefaultCloseDuration = 0.16f;

        /// <summary>
        /// Plays an elastic pop-in animation (scale 0.75 -> 1.05 -> 1.0 with alpha fade in).
        /// </summary>
        public static IEnumerator AnimateOpen(
            RectTransform dialogBox,
            CanvasGroup canvasGroup,
            float duration = DefaultOpenDuration,
            Action onComplete = null,
            Vector3? targetScale = null)
        {
            Vector3 baseScale = targetScale ?? Vector3.one;
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
                ApplyOpenProgress(dialogBox, canvasGroup, baseScale, Mathf.Clamp01(elapsed / duration));
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

        internal static void ApplyOpenProgress(RectTransform dialogBox, CanvasGroup canvasGroup, Vector3 baseScale, float progress)
        {
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Min(1f, progress * 1.6f);
            if (dialogBox == null) return;

            // Back.Out: expand from 75%, overshoot slightly, and settle at rest.
            float p = progress - 1f;
            const float overshoot = 1.6f;
            float easeBackOut = 1f + (overshoot + 1f) * p * p * p + overshoot * p * p;
            dialogBox.localScale = baseScale * Mathf.LerpUnclamped(0.75f, 1f, easeBackOut);
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
