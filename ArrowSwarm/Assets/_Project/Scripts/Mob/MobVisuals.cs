namespace ArrowSwarm.Mob
{
    using TMPro;
    using UnityEngine;

    /// <summary>Manages mob sprites, HP, damage feedback, freeze tint, and facing direction.</summary>
    public partial class MobVisuals : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private TextMeshPro _hpText;
        [SerializeField] private Sprite[] _mobVariants;
        [SerializeField] private float _flashDuration = 0.4f;

        private Vector3 _originalLocalPosition;
        private Vector3 _originalLocalScale;
        private bool _originalFlipX;
        private MobHealth _health;
        private Mob _mob;
        private MobAnimation _animation;
        private bool _isShaking;
        private bool _isFrozen;
        private Coroutine _flashCoroutine;

        private Color RestingTint => _isFrozen ? new Color(0.45f, 0.85f, 1f, 1f) : Color.white;

        private void Awake()
        {
            _health = GetComponent<MobHealth>();
            _mob = GetComponent<Mob>();
            _animation = GetComponent<MobAnimation>();
            if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (_spriteRenderer == null) return;
            _originalLocalPosition = _spriteRenderer.transform.localPosition;
            _originalLocalScale = _spriteRenderer.transform.localScale;
            _originalFlipX = _spriteRenderer.flipX;
        }

        /// <summary>Sets up the authored animation or legacy sprite variant and initial HP.</summary>
        public void Initialize(int hp)
        {
            StopDamageFlash();
            _isFrozen = false;
            if (_spriteRenderer != null)
            {
                if ((_animation == null || !_animation.IsConfigured) &&
                    _mobVariants != null && _mobVariants.Length > 0)
                {
                    _spriteRenderer.sprite = _mobVariants[Random.Range(0, _mobVariants.Length)];
                }
                _spriteRenderer.sortingOrder = 20;
                _spriteRenderer.color = RestingTint;
                _spriteRenderer.transform.localPosition = _originalLocalPosition;
                _spriteRenderer.transform.localScale = _originalLocalScale;
                _spriteRenderer.flipX = _originalFlipX;
            }
            _animation?.Initialize(_mob != null ? _mob.MobId : GetInstanceID());

            if (_hpText != null)
            {
                MeshRenderer hpRenderer = _hpText.GetComponent<MeshRenderer>();
                if (hpRenderer != null) hpRenderer.sortingOrder = 25;
            }
            UpdateHPDisplay(hp);
            if (_health != null)
            {
                _health.OnDamageTaken -= HandleDamageTaken;
                _health.OnDamageTaken += HandleDamageTaken;
            }
        }

        /// <summary>Updates the HP text display.</summary>
        public void UpdateHPDisplay(int currentHP)
        {
            if (_hpText != null) _hpText.text = currentHP.ToString();
        }

        /// <summary>Applies freeze tint and pauses the authored sprite animation.</summary>
        public void SetFrozen(bool frozen)
        {
            _isFrozen = frozen;
            _animation?.SetFrozen(frozen);
            if (_spriteRenderer != null && !_isShaking) _spriteRenderer.color = RestingTint;
        }

        /// <summary>Plays the death visual effect.</summary>
        public void PlayDeathEffect()
        {
            if (ArrowSwarm.Effects.ParticleManager.HasInstance)
            {
                ArrowSwarm.Effects.ParticleManager.Instance.SpawnMobDeathEffect(transform.position);
            }
        }

        /// <summary>Resets animation and damage feedback before this mob returns to its pool.</summary>
        public void ResetVisuals()
        {
            StopDamageFlash();
            _isFrozen = false;
            _animation?.ResetAnimation();
            if (_spriteRenderer != null)
            {
                _spriteRenderer.transform.localPosition = _originalLocalPosition;
                _spriteRenderer.transform.localScale = _originalLocalScale;
                _spriteRenderer.flipX = _originalFlipX;
                _spriteRenderer.color = RestingTint;
            }
            if (_health != null) _health.OnDamageTaken -= HandleDamageTaken;
        }

        /// <summary>Updates the facing direction of the mob sprite.</summary>
        public void UpdateFacingDirection(Vector2 direction)
        {
            if (_spriteRenderer == null || direction == Vector2.zero) return;
            if (direction.x < -0.1f) _spriteRenderer.flipX = true;
            else if (direction.x > 0.1f) _spriteRenderer.flipX = false;
        }
    }
}
