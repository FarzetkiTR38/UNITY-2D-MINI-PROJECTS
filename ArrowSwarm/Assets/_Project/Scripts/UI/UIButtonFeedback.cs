namespace ArrowSwarm.UI
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary>
    /// Adds tactile squeeze and bounce micro-animations to UI buttons on pointer press and release.
    /// Supports optional idle breathing animation for prominent call-to-action buttons (e.g. Play).
    /// </summary>
    [DisallowMultipleComponent]
    public class UIButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [Header("Target & Scaling")]
        [Tooltip("Transform to scale (defaults to this transform if null).")]
        [SerializeField] private Transform _targetTransform;
        [SerializeField] private float _pressedScale = 0.92f;
        [SerializeField] private float _punchScale = 1.06f;
        [SerializeField] private float _pressDuration = 0.08f;
        [SerializeField] private float _releaseDuration = 0.14f;

        [Header("Idle Breathing (Call To Action)")]
        [SerializeField] private bool _enableIdleBreath;
        [SerializeField] private float _breathScale = 1.04f;
        [SerializeField] private float _breathSpeed = 2.2f;

        private Vector3 _baseScale = Vector3.one;
        private bool _baseScaleRecorded;
        private Coroutine _activeAnimationRoutine;
        private Button _button;
        private bool _isPressed;
        private float _breathTimer;

        /// <summary>Gets or sets whether idle breathing is enabled.</summary>
        public bool EnableIdleBreath { get => _enableIdleBreath; set => _enableIdleBreath = value; }

        private void Awake() => EnsureTarget();

        private void EnsureTarget()
        {
            if (_targetTransform == null) _targetTransform = transform;
            if (!_baseScaleRecorded)
            {
                _baseScale = _targetTransform.localScale != Vector3.zero ? _targetTransform.localScale : Vector3.one;
                _baseScaleRecorded = true;
            }
            if (_button == null) _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            EnsureTarget();
            _isPressed = false;
            _targetTransform.localScale = _baseScale;
            _breathTimer = Random.Range(0f, Mathf.PI * 2f);
        }

        private void OnDisable()
        {
            if (_activeAnimationRoutine != null) { StopCoroutine(_activeAnimationRoutine); _activeAnimationRoutine = null; }
            _isPressed = false;
            if (_targetTransform != null) _targetTransform.localScale = _baseScale;
        }

        private void Update()
        {
            if (!_enableIdleBreath || _isPressed || _activeAnimationRoutine != null) return;
            if (_button != null && !_button.interactable) return;

            _breathTimer += Time.unscaledDeltaTime * _breathSpeed;
            float wave = (Mathf.Sin(_breathTimer) + 1f) * 0.5f;
            _targetTransform.localScale = _baseScale * Mathf.Lerp(1f, _breathScale, wave);
        }

        /// <summary>Invoked when user presses down on the button.</summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            EnsureTarget();
            if (_button != null && !_button.interactable) return;
            _isPressed = true;
            if (_activeAnimationRoutine != null) StopCoroutine(_activeAnimationRoutine);
            if (isActiveAndEnabled && Application.isPlaying)
                _activeAnimationRoutine = StartCoroutine(AnimateScaleRoutine(_targetTransform.localScale, _baseScale * _pressedScale, _pressDuration));
            else
                _targetTransform.localScale = _baseScale * _pressedScale;
        }

        /// <summary>Invoked when user releases the button.</summary>
        public void OnPointerUp(PointerEventData eventData)
        {
            EnsureTarget();
            if (!_isPressed) return;
            _isPressed = false;
            if (_activeAnimationRoutine != null) StopCoroutine(_activeAnimationRoutine);
            if (isActiveAndEnabled && Application.isPlaying)
                _activeAnimationRoutine = StartCoroutine(ReleaseBounceRoutine());
            else
                _targetTransform.localScale = _baseScale;
        }

        /// <summary>Invoked when cursor/touch exits button boundary while pressed.</summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            EnsureTarget();
            if (!_isPressed) return;
            _isPressed = false;
            if (_activeAnimationRoutine != null) StopCoroutine(_activeAnimationRoutine);
            if (isActiveAndEnabled && Application.isPlaying)
                _activeAnimationRoutine = StartCoroutine(AnimateScaleRoutine(_targetTransform.localScale, _baseScale, _pressDuration));
            else
                _targetTransform.localScale = _baseScale;
        }

        private IEnumerator ReleaseBounceRoutine()
        {
            Vector3 startScale = _targetTransform.localScale;
            Vector3 punchTarget = _baseScale * _punchScale;
            float halfDur = _releaseDuration * 0.45f;
            float elapsed = 0f;

            while (elapsed < halfDur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / halfDur);
                _targetTransform.localScale = Vector3.LerpUnclamped(startScale, punchTarget, Mathf.Sin(t * Mathf.PI * 0.5f));
                yield return null;
            }

            float returnDur = _releaseDuration * 0.55f;
            elapsed = 0f;
            while (elapsed < returnDur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / returnDur);
                _targetTransform.localScale = Vector3.Lerp(punchTarget, _baseScale, t * t * (3f - 2f * t));
                yield return null;
            }

            _targetTransform.localScale = _baseScale;
            _activeAnimationRoutine = null;
        }

        private IEnumerator AnimateScaleRoutine(Vector3 from, Vector3 to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _targetTransform.localScale = Vector3.Lerp(from, to, t * t * (3f - 2f * t));
                yield return null;
            }

            _targetTransform.localScale = to;
            _activeAnimationRoutine = null;
        }
    }
}
