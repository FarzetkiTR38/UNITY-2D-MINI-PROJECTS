// ============================================================================
// MonoBehaviourTemplate.cs
// Purpose: Production-ready MonoBehaviour template with all required elements
// Dependencies: None (self-contained template)
// Unity Version: 6000.3.18f1
// ============================================================================

using System;
using UnityEngine;

namespace GameName.Gameplay
{
    /// <summary>
    /// Template MonoBehaviour demonstrating the required structure,
    /// documentation, and patterns for all Unity 6 scripts.
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> Serves as a starting template for new MonoBehaviour scripts.
    /// Copy this file and replace all template content with actual implementation.</para>
    /// <para><b>Dependencies:</b>
    /// <list type="bullet">
    ///   <item><see cref="Rigidbody2D"/> — Required for physics-based behavior.</item>
    /// </list>
    /// </para>
    /// <para><b>Inspector Setup:</b>
    /// <list type="bullet">
    ///   <item>Set <c>_exampleSpeed</c> to desired movement speed.</item>
    ///   <item>Assign <c>_targetTransform</c> to the target object.</item>
    /// </list>
    /// </para>
    /// <para><b>Usage:</b> Attach to a GameObject with a Rigidbody2D component.
    /// Call <see cref="SetActive"/> to enable/disable behavior.</para>
    /// <para><b>Performance:</b> All component references cached in Awake.
    /// No GC allocations in Update loop. Uses cached animation hashes.</para>
    /// <para><b>Extension:</b> Implement custom behavior by overriding
    /// the virtual methods or subscribing to the public events.</para>
    /// </remarks>
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public class MonoBehaviourTemplate : MonoBehaviour
    {
        #region Constants

        /// <summary>Default movement speed when not configured.</summary>
        private const float DefaultSpeed = 5f;

        /// <summary>Radius used for proximity checks.</summary>
        private const float ProximityCheckRadius = 0.3f;

        /// <summary>Maximum allowed health value.</summary>
        private const int MaxAllowedHealth = 999;

        // Cached animator parameter hashes
        private static readonly int AnimHash_Speed = Animator.StringToHash("Speed");
        private static readonly int AnimHash_IsActive = Animator.StringToHash("IsActive");

        #endregion

        #region Serialized Fields

        [Header("Movement Settings")]
        [Tooltip("Movement speed in units per second.")]
        [SerializeField, Min(0f)]
        private float _exampleSpeed = DefaultSpeed;

        [Tooltip("Acceleration rate for smooth movement.")]
        [SerializeField, Range(0.1f, 50f)]
        private float _acceleration = 10f;

        [Space(10)]

        [Header("References")]
        [Tooltip("Target transform to track or follow.")]
        [SerializeField]
        private Transform _targetTransform;

        [Tooltip("Layer mask for interaction detection.")]
        [SerializeField]
        private LayerMask _interactionLayer;

        [Space(10)]

        [Header("Configuration")]
        [Tooltip("Configuration data asset for this component.")]
        [SerializeField]
        private ScriptableObject _configData;

        [Space(10)]

        [Header("Debug")]
        [Tooltip("Enable visual debug gizmos in Scene view.")]
        [SerializeField]
        private bool _showDebugGizmos = true;

        #endregion

        #region Private Fields

        private Rigidbody2D _rigidbody2D;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private bool _isActive;
        private Vector2 _currentVelocity;

        #endregion

        #region Properties

        /// <summary>Gets a value indicating whether this component is actively processing.</summary>
        public bool IsActive => _isActive;

        /// <summary>Gets the current movement speed.</summary>
        public float CurrentSpeed => _currentVelocity.magnitude;

        /// <summary>Gets the configured maximum speed.</summary>
        public float MaxSpeed => _exampleSpeed;

        #endregion

        #region Events

        /// <summary>Raised when the active state changes. Parameter: new active state.</summary>
        public event Action<bool> OnActiveStateChanged;

        /// <summary>Raised when the component reaches its target.</summary>
        public event Action OnTargetReached;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Cache required components
            _rigidbody2D = GetComponent<Rigidbody2D>();
            Debug.Assert(_rigidbody2D != null, $"[{name}] Rigidbody2D is required.", this);

            // Cache optional components
            TryGetComponent(out _animator);
            TryGetComponent(out _spriteRenderer);
        }

        private void OnEnable()
        {
            // Subscribe to events
            // Example: _eventChannel.OnEventRaised += HandleEvent;
        }

        private void Start()
        {
            // Cross-object initialization
            // Safe to reference other objects here
            _isActive = true;
        }

        private void Update()
        {
            if (!_isActive) return;

            // Frame-rate dependent logic
            UpdateAnimator();
        }

        private void FixedUpdate()
        {
            if (!_isActive) return;

            // Physics-dependent logic
            ApplyMovement();
        }

        private void LateUpdate()
        {
            // Post-update logic (camera follow, etc.)
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            // Example: _eventChannel.OnEventRaised -= HandleEvent;
        }

        private void OnDestroy()
        {
            // Final cleanup
            // Release Addressable handles
            // Dispose native collections
        }

        private void OnValidate()
        {
            // Validate Inspector assignments
            if (_targetTransform == null)
            {
                Debug.LogWarning($"[{name}] Target Transform is not assigned.", this);
            }

            if (_interactionLayer == 0)
            {
                Debug.LogWarning($"[{name}] Interaction Layer is not set.", this);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!_showDebugGizmos) return;

            // Draw proximity check radius
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, ProximityCheckRadius);

            // Draw line to target
            if (_targetTransform != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, _targetTransform.position);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Enables or disables the active processing of this component.
        /// </summary>
        /// <param name="active">Whether to activate or deactivate.</param>
        public void SetActive(bool active)
        {
            if (_isActive == active) return;

            _isActive = active;
            OnActiveStateChanged?.Invoke(_isActive);

            if (!_isActive)
            {
                _rigidbody2D.linearVelocity = Vector2.zero;
            }
        }

        /// <summary>
        /// Sets a new target transform to track.
        /// </summary>
        /// <param name="target">The new target. Can be null to clear the target.</param>
        public void SetTarget(Transform target)
        {
            _targetTransform = target;
        }

        #endregion

        #region Private Methods

        private void ApplyMovement()
        {
            if (_targetTransform == null) return;

            Vector2 direction = ((Vector2)_targetTransform.position - (Vector2)transform.position).normalized;
            Vector2 targetVelocity = direction * _exampleSpeed;

            _currentVelocity = Vector2.MoveTowards(
                _rigidbody2D.linearVelocity,
                targetVelocity,
                _acceleration * Time.fixedDeltaTime
            );

            _rigidbody2D.linearVelocity = _currentVelocity;

            // Check if reached target
            float distanceToTarget = Vector2.Distance(transform.position, _targetTransform.position);
            if (distanceToTarget <= ProximityCheckRadius)
            {
                OnTargetReached?.Invoke();
            }
        }

        private void UpdateAnimator()
        {
            if (_animator == null) return;

            _animator.SetFloat(AnimHash_Speed, CurrentSpeed);
            _animator.SetBool(AnimHash_IsActive, _isActive);
        }

        #endregion

        #region Context Menu

        [ContextMenu("Debug/Log State")]
        private void DebugLogState()
        {
            Debug.Log($"[{name}] Active: {_isActive}, Speed: {CurrentSpeed:F2}, " +
                      $"Target: {(_targetTransform != null ? _targetTransform.name : "None")}", this);
        }

        [ContextMenu("Debug/Toggle Active")]
        private void DebugToggleActive()
        {
            SetActive(!_isActive);
        }

        #endregion
    }
}
