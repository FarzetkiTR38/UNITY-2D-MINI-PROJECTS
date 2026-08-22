namespace ArrowSwarm.Mob
{
    using TMPro;
    using UnityEngine;

    /// <summary>
    /// Manages mob visual representation: sprite selection,
    /// HP text display, damage shake animation, and death effect.
    /// </summary>
    public class MobVisuals : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private TextMeshPro _hpText;
        [SerializeField] private Sprite[] _mobVariants; // 5 visual variants
        [SerializeField] private float _flashDuration = 0.5f;

        private Vector3 _originalLocalPosition;
        private MobHealth _health;
        private bool _isShaking;

        private void Awake()
        {
            _health = GetComponent<MobHealth>();
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        /// <summary>
        /// Sets up mob visuals with a random variant and initial HP display.
        /// Sets sorting orders: sprite=20 (above portals), HP text=25 (frontmost).
        /// </summary>
        public void Initialize(int hp)
        {
            // Random visual variant
            if (_mobVariants != null && _mobVariants.Length > 0)
            {
                _spriteRenderer.sprite = _mobVariants[Random.Range(0, _mobVariants.Length)];
            }

            // Sorting order: mobs render above portals (15) and arrows (5-6)
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sortingOrder = 20;
            }

            // HP text renders above everything
            if (_hpText != null)
            {
                MeshRenderer hpRenderer = _hpText.GetComponent<MeshRenderer>();
                if (hpRenderer != null)
                {
                    hpRenderer.sortingOrder = 25;
                }
            }

            UpdateHPDisplay(hp);
            _originalLocalPosition = _spriteRenderer.transform.localPosition;

            // Subscribe to health events
            if (_health != null)
            {
                _health.OnDamageTaken += HandleDamageTaken;
            }
        }

        /// <summary>
        /// Updates the HP text display.
        /// </summary>
        public void UpdateHPDisplay(int currentHP)
        {
            if (_hpText != null)
            {
                _hpText.text = currentHP.ToString();
            }
        }

        /// <summary>
        /// Plays the death visual effect.
        /// </summary>
        public void PlayDeathEffect()
        {
            // Particle effects added in Phase 8
            // For now, just disable the sprite
        }

        /// <summary>
        /// Resets visuals for pool reuse.
        /// </summary>
        public void ResetVisuals()
        {
            _isShaking = false;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.transform.localPosition = _originalLocalPosition;
                _spriteRenderer.color = Color.white;
            }
            if (_health != null)
            {
                _health.OnDamageTaken -= HandleDamageTaken;
            }
        }

        /// <summary>
        /// Updates the facing direction of the mob sprite.
        /// </summary>
        public void UpdateFacingDirection(Vector2 direction)
        {
            if (_spriteRenderer == null || direction == Vector2.zero) return;

            // Flip sprite based on horizontal direction
            if (direction.x < -0.1f)
            {
                _spriteRenderer.flipX = true;
            }
            else if (direction.x > 0.1f)
            {
                _spriteRenderer.flipX = false;
            }
        }

        private void HandleDamageTaken(int damage, int remainingHP)
        {
            UpdateHPDisplay(remainingHP);
            if (!_isShaking)
            {
                StartCoroutine(FlashCoroutine());
            }
        }

        private System.Collections.IEnumerator FlashCoroutine()
        {
            _isShaking = true;
            float elapsed = 0f;
            WaitForEndOfFrame waitFrame = new WaitForEndOfFrame();
            
            // Set color to red
            Color originalColor = Color.white; // Or whatever default is
            _spriteRenderer.color = Color.red;

            while (elapsed < _flashDuration)
            {
                // Fade color back to original during flash
                _spriteRenderer.color = Color.Lerp(Color.red, originalColor, elapsed / _flashDuration);
                
                elapsed += Time.deltaTime;
                yield return waitFrame;
            }

            _spriteRenderer.color = originalColor;
            _isShaking = false;
        }
    }
}
