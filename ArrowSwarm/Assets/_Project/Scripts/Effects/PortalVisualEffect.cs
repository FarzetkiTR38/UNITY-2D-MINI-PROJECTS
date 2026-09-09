namespace ArrowSwarm.Effects
{
    using System.Collections;
    using ArrowSwarm.Core;
    using ArrowSwarm.Mob;
    using UnityEngine;

    /// <summary>
    /// Handles visual swirl rotation, breathing pulse, and spawn/finish burst animations
    /// for the level path portals. Zero GC allocations in Update.
    /// </summary>
    public class PortalVisualEffect : MonoBehaviour
    {
        [Header("Swirl Settings")]
        [SerializeField] private float _rotationSpeed = 45f;
        [SerializeField] private float _pulseFrequency = 2.5f;
        [SerializeField] private float _pulseAmplitude = 0.05f;

        [Header("Burst Settings")]
        [SerializeField] private float _burstScaleMultiplier = 1.3f;
        [SerializeField] private float _burstDuration = 0.28f;

        private SpriteRenderer _spriteRenderer;
        private Vector3 _baseScale;
        private bool _isSpawnPortal = true;
        private bool _isFinishPortal = true;
        private Coroutine _burstCoroutine;
        private Color _originalColor = Color.white;

        /// <summary>
        /// Initializes the portal effect configuration.
        /// </summary>
        public void Initialize(bool isSpawn, bool isFinish)
        {
            _isSpawnPortal = isSpawn;
            _isFinishPortal = isFinish;
            _baseScale = transform.localScale;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null)
            {
                _originalColor = _spriteRenderer.color;
            }

            // Reverse rotation direction if exclusively finish portal
            if (!_isSpawnPortal && _isFinishPortal)
            {
                _rotationSpeed = -55f;
            }
        }

        private void Awake()
        {
            _baseScale = transform.localScale;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null)
            {
                _originalColor = _spriteRenderer.color;
            }
        }

        private void OnEnable()
        {
            MobSpawner.OnMobSpawned += HandleMobSpawned;
            GameManager.OnMobReachedFinish += HandleMobReachedFinish;
        }

        private void OnDisable()
        {
            MobSpawner.OnMobSpawned -= HandleMobSpawned;
            GameManager.OnMobReachedFinish -= HandleMobReachedFinish;

            if (_burstCoroutine != null)
            {
                StopCoroutine(_burstCoroutine);
                _burstCoroutine = null;
            }

            transform.localScale = _baseScale;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _originalColor;
            }
        }

        private void Update()
        {
            // Continuous swirl rotation
            transform.Rotate(0f, 0f, _rotationSpeed * Time.deltaTime);

            // Subtle breathing pulse if not actively playing an explosive burst
            if (_burstCoroutine == null)
            {
                float pulse = 1f + Mathf.Sin(Time.time * _pulseFrequency) * _pulseAmplitude;
                transform.localScale = _baseScale * pulse;
            }
        }

        private void HandleMobSpawned()
        {
            if (!_isSpawnPortal || !gameObject.activeInHierarchy) return;
            TriggerBurst(new Color(1.3f, 1.3f, 1.3f, 1f));
        }

        private void HandleMobReachedFinish()
        {
            if (!_isFinishPortal || !gameObject.activeInHierarchy) return;
            TriggerBurst(new Color(1f, 0.4f, 0.4f, 1f));
        }

        /// <summary>
        /// Triggers a sudden elastic warp punch and color tint.
        /// </summary>
        public void TriggerBurst(Color flashColor)
        {
            if (_burstCoroutine != null)
            {
                StopCoroutine(_burstCoroutine);
            }
            _burstCoroutine = StartCoroutine(BurstRoutine(flashColor));
        }

        private IEnumerator BurstRoutine(Color flashColor)
        {
            float elapsed = 0f;
            Vector3 peakScale = _baseScale * _burstScaleMultiplier;

            while (elapsed < _burstDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _burstDuration);

                // Elastic ease out
                float ease = 1f - Mathf.Pow(1f - t, 3f);
                transform.localScale = Vector3.Lerp(peakScale, _baseScale, ease);

                if (_spriteRenderer != null)
                {
                    _spriteRenderer.color = Color.Lerp(flashColor, _originalColor, ease);
                }

                yield return null;
            }

            transform.localScale = _baseScale;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _originalColor;
            }
            _burstCoroutine = null;
        }
    }
}
