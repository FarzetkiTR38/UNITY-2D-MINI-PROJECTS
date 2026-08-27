namespace ArrowSwarm.Tutorial
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Controls the animated hand cursor for tutorial guidance.
    /// Handles smooth gliding to targets, finger tap micro-animations,
    /// and expanding pulse ripple effects.
    /// </summary>
    public class TutorialHandUI : MonoBehaviour
    {
        [Header("--- UI References ---")]
        [SerializeField] private RectTransform _handContainer;
        [SerializeField] private Image _handImage;
        [SerializeField] private RectTransform _rippleCircle;
        [SerializeField] private CanvasGroup _rippleGroup;
        [SerializeField] private CanvasGroup _handCanvasGroup;

        [Header("--- Animation Tuning ---")]
        [SerializeField] private float _tapScaleDown = 0.84f;
        [SerializeField] private float _tapCycleDuration = 0.85f;

        private Vector2 _targetCanvasPos;
        private bool _isPointing;
        private Coroutine _tapRoutine;
        private Coroutine _rippleRoutine;
        private Coroutine _moveRoutine;
        private Camera _mainCamera;

        private void Awake()
        {
            if (_handContainer == null) _handContainer = GetComponent<RectTransform>();
            if (_handCanvasGroup == null)
            {
                _handCanvasGroup = GetComponent<CanvasGroup>();
                if (_handCanvasGroup == null) _handCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // The hand cursor is purely visual - NEVER block clicks/raycasts to arrows underneath
            _handCanvasGroup.blocksRaycasts = false;
            _handCanvasGroup.interactable = false;

            var allImages = GetComponentsInChildren<Image>(true);
            foreach (var img in allImages)
            {
                img.raycastTarget = false;
            }

            _mainCamera = Camera.main;
            HideImmediately();
        }

        /// <summary>
        /// Points the hand cursor to a target world position (e.g. arrow position).
        /// </summary>
        public void PointToWorldPosition(Vector3 worldPos, Canvas parentCanvas, bool animateMove = true)
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) _mainCamera = FindFirstObjectByType<Camera>();
            if (_mainCamera == null || parentCanvas == null) return;

            Vector2 screenPos = _mainCamera.WorldToScreenPoint(worldPos);
            RectTransform canvasRect = parentCanvas.transform as RectTransform;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPos, parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _mainCamera, out Vector2 localPoint))
            {
                // Pivot (0.2, 0.9) is index finger tip. Place right on arrow center.
                PointToLocalPosition(localPoint, animateMove);
            }
        }

        /// <summary>
        /// Points the hand cursor to a local canvas position.
        /// </summary>
        public void PointToLocalPosition(Vector2 localPos, bool animateMove = true)
        {
            if (_handContainer == null) _handContainer = GetComponent<RectTransform>();
            if (_handCanvasGroup == null) _handCanvasGroup = GetComponent<CanvasGroup>();

            _targetCanvasPos = localPos;
            gameObject.SetActive(true);
            _isPointing = true;

            if (_handContainer != null)
            {
                _handContainer.localScale = Vector3.one;
                _handContainer.anchoredPosition = _targetCanvasPos;
            }
            if (_handCanvasGroup != null) _handCanvasGroup.alpha = 1f;

            if (_moveRoutine != null) StopCoroutine(_moveRoutine);

            if (animateMove && _handContainer != null && gameObject.activeInHierarchy)
            {
                _moveRoutine = StartCoroutine(MoveToTargetRoutine(_targetCanvasPos));
            }

            StartTapAnimation();
            StartRippleAnimation();
        }

        /// <summary>
        /// Hides the hand cursor with smooth fade out.
        /// </summary>
        public void Hide()
        {
            _isPointing = false;
            StopTapAnimation();
            StopRippleAnimation();

            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(FadeOutRoutine());
            }
            else
            {
                HideImmediately();
            }
        }

        /// <summary>
        /// Immediately hides the cursor without animation.
        /// </summary>
        public void HideImmediately()
        {
            _isPointing = false;
            StopTapAnimation();
            StopRippleAnimation();

            if (_handContainer != null) _handContainer.localScale = Vector3.one;
            if (_handCanvasGroup != null) _handCanvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        private IEnumerator MoveToTargetRoutine(Vector2 target)
        {
            Vector2 start = _handContainer.anchoredPosition;
            float elapsed = 0f;
            float duration = 0.3f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                _handContainer.anchoredPosition = Vector2.Lerp(start, target, t);
                yield return null;
            }

            _handContainer.anchoredPosition = target;
            _moveRoutine = null;
        }

        private void StartTapAnimation()
        {
            if (_tapRoutine != null) StopCoroutine(_tapRoutine);
            _tapRoutine = StartCoroutine(TapLoopRoutine());
        }

        private void StopTapAnimation()
        {
            if (_tapRoutine != null)
            {
                StopCoroutine(_tapRoutine);
                _tapRoutine = null;
            }
            if (_handContainer != null) _handContainer.localScale = Vector3.one;
        }

        private IEnumerator TapLoopRoutine()
        {
            Vector3 normalScale = Vector3.one;
            Vector3 tapScale = new Vector3(_tapScaleDown, _tapScaleDown, 1f);

            while (_isPointing)
            {
                // Press down
                float elapsed = 0f;
                float duration = _tapCycleDuration * 0.35f;
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = elapsed / duration;
                    if (_handContainer != null) _handContainer.localScale = Vector3.Lerp(normalScale, tapScale, t);
                    yield return null;
                }

                // Release up
                elapsed = 0f;
                duration = _tapCycleDuration * 0.45f;
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = elapsed / duration;
                    if (_handContainer != null) _handContainer.localScale = Vector3.Lerp(tapScale, normalScale, t);
                    yield return null;
                }

                if (_handContainer != null) _handContainer.localScale = normalScale;
                yield return new WaitForSecondsRealtime(0.18f);
            }
        }

        private void StartRippleAnimation()
        {
            if (_rippleCircle == null || _rippleGroup == null) return;
            if (_rippleRoutine != null) StopCoroutine(_rippleRoutine);
            _rippleRoutine = StartCoroutine(RippleLoopRoutine());
        }

        private void StopRippleAnimation()
        {
            if (_rippleRoutine != null)
            {
                StopCoroutine(_rippleRoutine);
                _rippleRoutine = null;
            }
            if (_rippleGroup != null) _rippleGroup.alpha = 0f;
        }

        private IEnumerator RippleLoopRoutine()
        {
            while (_isPointing && _rippleCircle != null && _rippleGroup != null)
            {
                _rippleCircle.localScale = Vector3.one * 0.25f;
                _rippleGroup.alpha = 0.95f;

                float elapsed = 0f;
                float duration = 0.75f;

                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = elapsed / duration;
                    _rippleCircle.localScale = Vector3.Lerp(Vector3.one * 0.25f, Vector3.one * 1.6f, t);
                    _rippleGroup.alpha = Mathf.Lerp(0.95f, 0f, t);
                    yield return null;
                }

                _rippleGroup.alpha = 0f;
                yield return new WaitForSecondsRealtime(0.15f);
            }
        }

        private IEnumerator FadeOutRoutine()
        {
            float elapsed = 0f;
            float duration = 0.18f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                if (_handCanvasGroup != null) _handCanvasGroup.alpha = 1f - t;
                yield return null;
            }

            HideImmediately();
        }
    }
}
