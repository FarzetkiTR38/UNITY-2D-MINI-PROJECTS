using System;
using System.Collections;
using UnityEngine;

namespace NeonGalaxy.VFX
{
    /// <summary>
    /// Static utility class for common UI animation patterns.
    /// All methods return IEnumerator for coroutine usage.
    /// Uses easing curves for premium-feeling motion.
    /// </summary>
    public static class UIAnimator
    {
        // ── Scale Animations ────────────────────────────────────

        /// <summary>
        /// Punch-scale effect: quickly scale up then return to original.
        /// Great for button presses and score updates.
        /// </summary>
        public static IEnumerator PunchScale(Transform target, float amount, float duration)
        {
            Vector3 original = target.localScale;
            Vector3 punched = original * (1f + amount);
            float half = duration * 0.5f;

            // Scale up
            float elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = EaseOutCubic(Mathf.Clamp01(elapsed / half));
                target.localScale = Vector3.LerpUnclamped(original, punched, t);
                yield return null;
            }

            // Scale back
            elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = EaseInOutQuad(Mathf.Clamp01(elapsed / half));
                target.localScale = Vector3.LerpUnclamped(punched, original, t);
                yield return null;
            }

            target.localScale = original;
        }

        /// <summary>
        /// Bounce-in effect with elastic overshoot. Premium panel entrance.
        /// </summary>
        public static IEnumerator BounceIn(Transform target, float duration, float overshoot = 1.7f)
        {
            Vector3 targetScale = target.localScale;
            target.localScale = Vector3.zero;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = EaseOutBack(t, overshoot);
                target.localScale = targetScale * ease;
                yield return null;
            }

            target.localScale = targetScale;
        }

        /// <summary>
        /// Scale out to zero with ease-in curve.
        /// </summary>
        public static IEnumerator ScaleOut(Transform target, float duration)
        {
            Vector3 original = target.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = EaseInCubic(Mathf.Clamp01(elapsed / duration));
                target.localScale = Vector3.LerpUnclamped(original, Vector3.zero, t);
                yield return null;
            }

            target.localScale = Vector3.zero;
        }

        // ── Position Animations ─────────────────────────────────

        /// <summary>
        /// Slide-in from an offset direction.
        /// </summary>
        public static IEnumerator SlideIn(Transform target, Vector3 fromOffset, float duration)
        {
            Vector3 targetPos = target.localPosition;
            Vector3 startPos = targetPos + fromOffset;
            target.localPosition = startPos;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = EaseOutCubic(Mathf.Clamp01(elapsed / duration));
                target.localPosition = Vector3.LerpUnclamped(startPos, targetPos, t);
                yield return null;
            }

            target.localPosition = targetPos;
        }

        /// <summary>
        /// Slide-out to an offset direction.
        /// </summary>
        public static IEnumerator SlideOut(Transform target, Vector3 toOffset, float duration)
        {
            Vector3 startPos = target.localPosition;
            Vector3 endPos = startPos + toOffset;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = EaseInCubic(Mathf.Clamp01(elapsed / duration));
                target.localPosition = Vector3.LerpUnclamped(startPos, endPos, t);
                yield return null;
            }

            target.localPosition = endPos;
        }

        /// <summary>
        /// Shake position effect. Good for errors and impacts.
        /// </summary>
        public static IEnumerator ShakePosition(Transform target, float intensity, float duration)
        {
            Vector3 original = target.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float dampening = 1f - (elapsed / duration);
                float x = UnityEngine.Random.Range(-intensity, intensity) * dampening;
                float y = UnityEngine.Random.Range(-intensity, intensity) * dampening;
                target.localPosition = original + new Vector3(x, y, 0f);
                yield return null;
            }

            target.localPosition = original;
        }

        // ── Alpha/CanvasGroup Animations ────────────────────────

        /// <summary>
        /// Fades a CanvasGroup alpha from one value to another.
        /// </summary>
        public static IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
        {
            group.alpha = from;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                group.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            group.alpha = to;
        }

        // ── Composite Animations ────────────────────────────────

        /// <summary>
        /// Staggered entrance: scales and fades in children sequentially.
        /// </summary>
        public static IEnumerator StaggerChildren(Transform parent, float delayPerChild, float animDuration)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (!child.gameObject.activeSelf) continue;

                // Store and zero scale
                Vector3 targetScale = child.localScale;
                child.localScale = Vector3.zero;

                // Wait for stagger delay (except first)
                if (i > 0)
                    yield return new WaitForSecondsRealtime(delayPerChild);

                // Bounce in (fire and forget — don't wait for completion)
                var host = child.GetComponent<MonoBehaviour>();
                if (host != null)
                {
                    host.StartCoroutine(BounceIn(child, animDuration));
                }
                else
                {
                    child.localScale = targetScale;
                }
            }
        }

        // ── Easing Functions ────────────────────────────────────

        public static float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        public static float EaseInCubic(float t)
        {
            return t * t * t;
        }

        public static float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }

        public static float EaseOutBack(float t, float overshoot = 1.70158f)
        {
            float c3 = overshoot + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + overshoot * Mathf.Pow(t - 1f, 2f);
        }

        public static float EaseOutElastic(float t)
        {
            if (t <= 0f) return 0f;
            if (t >= 1f) return 1f;

            float c4 = (2f * Mathf.PI) / 3f;
            return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
        }
    }
}
