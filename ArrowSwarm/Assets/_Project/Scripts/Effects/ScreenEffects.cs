namespace ArrowSwarm.Effects
{
    using ArrowSwarm.Core;
    using ArrowSwarm.Utils;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Handles full-screen visual effects: red flash on wrong click,
    /// screen shake on impact, and vignette effects.
    /// </summary>
    public class ScreenEffects : Singleton<ScreenEffects>
    {
        [Header("Flash")]
        [SerializeField] private Image _flashOverlay;
        [SerializeField] private Color _wrongClickColor = new Color(1f, 0f, 0f, 0.3f);
        [SerializeField] private Color _mobFinishColor = new Color(1f, 0.5f, 0f, 0.3f);
        [SerializeField] private float _flashDuration = 0.3f;

        [Header("Shake")]
        [SerializeField] private float _shakeIntensity = 0.15f;
        [SerializeField] private float _shakeDuration = 0.2f;

        private UnityEngine.Camera _mainCamera;
        private Vector3 _originalCameraPos;
        private bool _isShaking;

        private UnityEngine.Camera MainCamera
        {
            get
            {
                if (_mainCamera == null)
                {
                    _mainCamera = UnityEngine.Camera.main;
                }
                return _mainCamera;
            }
        }

        protected override void OnSingletonAwake()
        {
        }

        private void OnEnable()
        {
            GameManager.OnWrongClick += HandleWrongClick;
            GameManager.OnMobReachedFinish += HandleMobFinish;
        }

        private void OnDisable()
        {
            GameManager.OnWrongClick -= HandleWrongClick;
            GameManager.OnMobReachedFinish -= HandleMobFinish;
        }

        /// <summary>
        /// Flashes the screen with a color overlay.
        /// </summary>
        public void Flash(Color color, float duration = 0.3f)
        {
            if (_flashOverlay == null) return;
            StopAllCoroutines();
            StartCoroutine(FlashRoutine(color, duration));
        }

        /// <summary>
        /// Shakes the camera briefly.
        /// </summary>
        public void Shake(float intensity = 0.15f, float duration = 0.2f)
        {
            if (MainCamera == null || _isShaking) return;
            StartCoroutine(ShakeRoutine(intensity, duration));
        }

        private void HandleWrongClick()
        {
            Flash(_wrongClickColor, _flashDuration);
            Shake(_shakeIntensity, _shakeDuration);
        }

        private void HandleMobFinish()
        {
            Flash(_mobFinishColor, _flashDuration);
            Shake(_shakeIntensity * 0.7f, _shakeDuration);
        }

        private System.Collections.IEnumerator FlashRoutine(Color color, float duration)
        {
            _flashOverlay.color = color;
            _flashOverlay.gameObject.SetActive(true);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(color.a, 0f, elapsed / duration);
                _flashOverlay.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }

            _flashOverlay.gameObject.SetActive(false);
        }

        private System.Collections.IEnumerator ShakeRoutine(float intensity, float duration)
        {
            if (MainCamera == null) yield break;
            _isShaking = true;
            _originalCameraPos = MainCamera.transform.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (MainCamera == null) yield break;
                float x = Random.Range(-intensity, intensity);
                float y = Random.Range(-intensity, intensity);
                MainCamera.transform.position = _originalCameraPos + new Vector3(x, y, 0);
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (MainCamera != null) MainCamera.transform.position = _originalCameraPos;
            _isShaking = false;
        }
    }
}
