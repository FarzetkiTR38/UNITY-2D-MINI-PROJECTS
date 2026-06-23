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
    }
}
