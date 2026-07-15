using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NeonGalaxy.VFX
{
    /// <summary>
    /// Overlay-based screen flash effect for premium moments.
    /// Attached to a full-screen UI Image on a Canvas.
    /// Rapidly fades in then out a color overlay for visual punctuation.
    /// Used for Nova Cross, line clears, and special events.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ScreenFlash : MonoBehaviour
    {
        private Image _flashImage;
        private Coroutine _activeFlash;

        private void Awake()
        {
            _flashImage = GetComponent<Image>();
            _flashImage.raycastTarget = false;
            _flashImage.color = Color.clear;
        }

        /// <summary>
        /// Triggers a screen flash with the given color and duration.
        /// </summary>
        public void Flash(Color color, float duration)
        {
            if (_activeFlash != null)
            {
                StopCoroutine(_activeFlash);
            }
            _activeFlash = StartCoroutine(FlashRoutine(color, duration));
        }

        private IEnumerator FlashRoutine(Color color, float duration)
        {
            float halfDuration = duration * 0.5f;

            // Phase 1: Flash in
            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                _flashImage.color = Color.Lerp(Color.clear, color, t);
                yield return null;
            }

            // Phase 2: Flash out
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                _flashImage.color = Color.Lerp(color, Color.clear, t);
                yield return null;
            }

            _flashImage.color = Color.clear;
            _activeFlash = null;
        }

        /// <summary>
        /// Triggers a mega flash for board clear and premium celebration moments.
        /// More intense and longer-lasting than a standard flash.
        /// Peaks at near-white before slowly fading to the target color and then out.
        /// </summary>
        public void MegaFlash(Color color, float duration)
        {
            if (_activeFlash != null)
            {
                StopCoroutine(_activeFlash);
            }
            _activeFlash = StartCoroutine(MegaFlashRoutine(color, duration));
        }

        private IEnumerator MegaFlashRoutine(Color color, float duration)
        {
            // Phase 1: Rapid flash to near-white (20% of duration)
            float phase1 = duration * 0.2f;
            Color peakColor = Color.Lerp(color, Color.white, 0.7f);
            peakColor.a = Mathf.Min(color.a * 1.5f, 0.9f);

            float elapsed = 0f;
            while (elapsed < phase1)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / phase1);
                // Use ease-out for snappy flash-in
                t = 1f - (1f - t) * (1f - t);
                _flashImage.color = Color.Lerp(Color.clear, peakColor, t);
                yield return null;
            }

            // Phase 2: Hold at peak briefly (10% of duration)
            float phase2 = duration * 0.1f;
            elapsed = 0f;
            while (elapsed < phase2)
            {
                elapsed += Time.unscaledDeltaTime;
                _flashImage.color = peakColor;
                yield return null;
            }

            // Phase 3: Slow fade through color, then to clear (70% of duration)
            float phase3 = duration * 0.7f;
            elapsed = 0f;
            while (elapsed < phase3)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / phase3);
                // Ease-in-out for smooth fade
                t = t * t * (3f - 2f * t);
                if (t < 0.3f)
                {
                    // Transition from peak to color
                    _flashImage.color = Color.Lerp(peakColor, color, t / 0.3f);
                }
                else
                {
                    // Transition from color to clear
                    _flashImage.color = Color.Lerp(color, Color.clear, (t - 0.3f) / 0.7f);
                }
                yield return null;
            }

            _flashImage.color = Color.clear;
            _activeFlash = null;
        }
    }
}
