namespace ArrowSwarm.UI
{
    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// In-scene studio splash shown before the boot loading UI. The logo pops in,
    /// holds briefly and the whole overlay hands over to the loading screen while
    /// the logo is still fully visible, so the logo never fades out on its own. Night Board
    /// arrows draw their gameplay-style paths while the logo appears.
    /// All visuals are authored in the BootScene; this only animates them.
    /// </summary>
    public class BootSplashOverlay : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _overlayGroup;
        [SerializeField] private RectTransform _logoTransform;
        [SerializeField] private CanvasGroup _logoGroup;
        [SerializeField] private float _logoInDuration = 0.35f;
        [SerializeField] private float _holdDuration = 0.55f;
        [SerializeField] private float _handoverDuration = 0.25f;
        [SerializeField] private float _logoStartScale = 0.86f;

        [Header("Night Board Arrows")]
        [SerializeField] private SplashArrowPath[] _arrows = new SplashArrowPath[0];
        [SerializeField] private float _arrowDrawDuration = 0.5f;
        [SerializeField] private float _arrowStagger = 0.07f;

        private Vector3 _logoBaseScale = Vector3.one;

        private void Awake()
        {
            if (_logoTransform != null) _logoBaseScale = _logoTransform.localScale;
            if (_overlayGroup != null) _overlayGroup.alpha = 1f;
            ApplyLogo(0f);
            ApplyArrows(0f);
        }

        /// <summary>Plays the splash and invokes <paramref name="onFinished"/> once the overlay is gone.</summary>
        public void Play(Action onFinished)
        {
            StartCoroutine(PlayRoutine(onFinished));
        }

        private IEnumerator PlayRoutine(Action onFinished)
        {
            // Let the scene activation frame pass so the pop-in is not swallowed by the load hitch.
            yield return null;

            // Logo pop-in and arrow drawing share one timeline; the hold ends once both have settled.
            float arrowsEnd = _arrowDrawDuration + _arrowStagger * Mathf.Max(0, _arrows.Length - 1);
            float total = Mathf.Max(_logoInDuration + _holdDuration, arrowsEnd + 0.12f);
            float elapsed = 0f;
            while (elapsed < total)
            {
                elapsed += Mathf.Min(Time.unscaledDeltaTime, 1f / 30f);
                ApplyLogo(elapsed / Mathf.Max(0.01f, _logoInDuration));
                ApplyArrows(elapsed);
                yield return null;
            }
            ApplyLogo(1f);
            ApplyArrows(float.MaxValue);

            yield return Animate(_handoverDuration, t => { if (_overlayGroup != null) _overlayGroup.alpha = 1f - t; });
            if (_overlayGroup != null)
            {
                _overlayGroup.alpha = 0f;
                _overlayGroup.blocksRaycasts = false;
            }

            gameObject.SetActive(false);
            onFinished?.Invoke();
        }

        private void ApplyArrows(float elapsed)
        {
            for (int i = 0; i < _arrows.Length; i++)
            {
                if (_arrows[i] == null) continue;
                float t = Mathf.Clamp01((elapsed - i * _arrowStagger) / Mathf.Max(0.01f, _arrowDrawDuration));
                _arrows[i].Progress = 1f - Mathf.Pow(1f - t, 3f);
            }
        }

        private void ApplyLogo(float t)
        {
            float eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
            if (_logoGroup != null) _logoGroup.alpha = eased;
            if (_logoTransform != null)
            {
                _logoTransform.localScale = _logoBaseScale * Mathf.LerpUnclamped(_logoStartScale, 1f, eased);
            }
        }

        private static IEnumerator Animate(float duration, Action<float> apply)
        {
            float elapsed = 0f;
            float safeDuration = Mathf.Max(0.01f, duration);
            while (elapsed < safeDuration)
            {
                elapsed += Mathf.Min(Time.unscaledDeltaTime, 1f / 30f);
                apply(Mathf.Clamp01(elapsed / safeDuration));
                yield return null;
            }

            apply(1f);
        }
    }
}
