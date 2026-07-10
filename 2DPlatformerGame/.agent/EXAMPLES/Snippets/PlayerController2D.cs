// ============================================================================
// PlayerController2D.cs
// Purpose: Complete 2D platformer player controller with movement, jump, dash
// Dependencies: Rigidbody2D, Input System, Animator (optional)
// Unity Version: 6000.3.18f1
// ============================================================================

using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameName.Gameplay.Player
{
    /// <summary>
    /// Full-featured 2D platformer player controller supporting movement, jumping
    /// (with coyote time and jump buffering), variable jump height, wall sliding,
    /// wall jumping, and dashing.
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> Main player movement controller for a 2D platformer.</para>
    /// <para><b>Dependencies:</b> Rigidbody2D (required), Animator (optional),
    /// Input System InputActionReferences (assigned via Inspector).</para>
    /// <para><b>Inspector Setup:</b></para>
    /// <list type="bullet">
    ///   <item>Assign Input Action references for Move, Jump, and Dash.</item>
    ///   <item>Create child GameObjects for GroundCheck and WallCheck positions.</item>
    ///   <item>Set Ground and Wall layer masks.</item>
    ///   <item>Tune movement values for desired game feel.</item>
    /// </list>
    /// <para><b>Performance:</b> All component refs cached. Animation hashes static.
    /// No GC allocations in update loop. Physics uses NonAlloc queries.</para>
    /// </remarks>
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public class PlayerController2D : MonoBehaviour
    {
        #region Constants

        private const float GroundCheckRadius = 0.2f;
        private const float WallCheckDistance = 0.5f;
        private const float CoyoteTimeDuration = 0.12f;
        private const float JumpBufferDuration = 0.15f;

        private static readonly int AnimHash_Speed = Animator.StringToHash("Speed");
        private static readonly int AnimHash_VelocityY = Animator.StringToHash("VelocityY");
        private static readonly int AnimHash_IsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int AnimHash_IsWallSliding = Animator.StringToHash("IsWallSliding");
        private static readonly int AnimHash_Jump = Animator.StringToHash("Jump");
        private static readonly int AnimHash_Dash = Animator.StringToHash("Dash");

        #endregion

        #region Serialized Fields

        [Header("Movement")]
        [Tooltip("Maximum horizontal speed in units per second.")]
        [SerializeField, Min(0f)] private float _moveSpeed = 8f;

        [Tooltip("How quickly the player reaches max speed.")]
        [SerializeField, Min(0f)] private float _acceleration = 50f;

        [Tooltip("How quickly the player decelerates.")]
        [SerializeField, Min(0f)] private float _deceleration = 50f;

        [Space(10)]
        [Header("Jump")]
        [Tooltip("Vertical force applied when jumping.")]
        [SerializeField, Min(0f)] private float _jumpForce = 16f;

        [Tooltip("Gravity multiplier when falling (snappier falls).")]
        [SerializeField, Range(1f, 10f)] private float _fallMultiplier = 2.5f;

        [Tooltip("Gravity multiplier for short jumps (button released early).")]
        [SerializeField, Range(1f, 10f)] private float _lowJumpMultiplier = 2f;

        [Tooltip("Maximum number of air jumps (0 = no double jump).")]
        [SerializeField, Min(0)] private int _maxAirJumps;

        [Space(10)]
        [Header("Wall Mechanics")]
        [Tooltip("Enable wall sliding and wall jumping.")]
        [SerializeField] private bool _enableWallMechanics;

        [Tooltip("Downward speed while wall sliding.")]
        [SerializeField, Min(0f)] private float _wallSlideSpeed = 2f;

        [Tooltip("Force applied when wall jumping (X = horizontal, Y = vertical).")]
        [SerializeField] private Vector2 _wallJumpForce = new(12f, 16f);

        [Tooltip("Duration of input lock after wall jump.")]
        [SerializeField, Range(0f, 0.5f)] private float _wallJumpLockDuration = 0.15f;

        [Space(10)]
        [Header("Dash")]
        [Tooltip("Enable dashing.")]
        [SerializeField] private bool _enableDash;

        [Tooltip("Dash speed in units per second.")]
        [SerializeField, Min(0f)] private float _dashSpeed = 20f;

        [Tooltip("Dash duration in seconds.")]
        [SerializeField, Range(0.05f, 0.5f)] private float _dashDuration = 0.15f;

        [Tooltip("Cooldown between dashes.")]
        [SerializeField, Min(0f)] private float _dashCooldown = 0.5f;

        [Space(10)]
        [Header("Ground Detection")]
        [Tooltip("Ground check position (child transform).")]
        [SerializeField] private Transform _groundCheck;

        [Tooltip("Layers considered as ground.")]
        [SerializeField] private LayerMask _groundLayer;

        [Space(10)]
        [Header("Wall Detection")]
        [Tooltip("Wall check position (child transform).")]
        [SerializeField] private Transform _wallCheck;

        [Tooltip("Layers considered as walls.")]
        [SerializeField] private LayerMask _wallLayer;

        [Space(10)]
        [Header("Input")]
        [Tooltip("Move input action reference.")]
        [SerializeField] private InputActionReference _moveAction;

        [Tooltip("Jump input action reference.")]
        [SerializeField] private InputActionReference _jumpAction;

        [Tooltip("Dash input action reference.")]
        [SerializeField] private InputActionReference _dashAction;

        [Space(10)]
        [Header("Debug")]
        [SerializeField] private bool _showGizmos = true;

        #endregion

        #region Private Fields

        private Rigidbody2D _rb;
        private Animator _animator;
        private Vector2 _moveInput;
        private bool _isGrounded;
        private bool _wasGrounded;
        private bool _isTouchingWall;
        private bool _isWallSliding;
        private bool _isFacingRight = true;
        private bool _isDashing;
        private bool _jumpHeld;
        private float _coyoteTimeCounter;
        private float _jumpBufferCounter;
        private float _wallJumpLockCounter;
        private float _dashCooldownCounter;
        private int _airJumpsRemaining;
        private CancellationTokenSource _dashCts;

        #endregion

        #region Properties

        /// <summary>Gets whether the player is on the ground.</summary>
        public bool IsGrounded => _isGrounded;

        /// <summary>Gets whether the player is wall sliding.</summary>
        public bool IsWallSliding => _isWallSliding;

        /// <summary>Gets whether the player is dashing.</summary>
        public bool IsDashing => _isDashing;

        /// <summary>Gets the facing direction. 1 = right, -1 = left.</summary>
        public int FacingDirection => _isFacingRight ? 1 : -1;

        /// <summary>Gets the current velocity.</summary>
        public Vector2 Velocity => _rb != null ? _rb.linearVelocity : Vector2.zero;

        #endregion

        #region Events

        /// <summary>Raised when the player lands.</summary>
        public event Action OnLanded;

        /// <summary>Raised when the player jumps.</summary>
        public event Action OnJumped;

        /// <summary>Raised when the player starts a dash.</summary>
        public event Action OnDashStarted;

        /// <summary>Raised when the player wall jumps.</summary>
        public event Action OnWallJumped;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            TryGetComponent(out _animator);

            Debug.Assert(_rb != null, $"[{name}] Rigidbody2D required.", this);

            _rb.freezeRotation = true;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private void OnEnable()
        {
            if (_moveAction != null) _moveAction.action.Enable();
            if (_jumpAction != null)
            {
                _jumpAction.action.Enable();
                _jumpAction.action.performed += OnJumpPerformed;
                _jumpAction.action.canceled += OnJumpCanceled;
            }
            if (_dashAction != null && _enableDash)
            {
                _dashAction.action.Enable();
                _dashAction.action.performed += OnDashPerformed;
            }
        }

        private void OnDisable()
        {
            if (_moveAction != null) _moveAction.action.Disable();
            if (_jumpAction != null)
            {
                _jumpAction.action.performed -= OnJumpPerformed;
                _jumpAction.action.canceled -= OnJumpCanceled;
                _jumpAction.action.Disable();
            }
            if (_dashAction != null)
            {
                _dashAction.action.performed -= OnDashPerformed;
                _dashAction.action.Disable();
            }

            _dashCts?.Cancel();
            _dashCts?.Dispose();
            _dashCts = null;
        }

        private void Update()
        {
            if (_isDashing) return;

            ReadInput();
            CheckGround();
            CheckWall();
            HandleCoyoteTime();
            HandleJumpBuffer();
            HandleWallSlide();
            HandleFlip();
            UpdateTimers();
            UpdateAnimator();
        }

        private void FixedUpdate()
        {
            if (_isDashing) return;

            ApplyMovement();
            ApplyBetterJumpGravity();
            ApplyWallSlideGravity();
        }

        private void OnValidate()
        {
            if (_groundCheck == null)
                Debug.LogWarning($"[{name}] Ground Check not assigned.", this);
            if (_groundLayer == 0)
                Debug.LogWarning($"[{name}] Ground Layer not set.", this);
            if (_enableWallMechanics && _wallCheck == null)
                Debug.LogWarning($"[{name}] Wall Check not assigned (wall mechanics enabled).", this);
        }

        private void OnDrawGizmosSelected()
        {
            if (!_showGizmos) return;

            if (_groundCheck != null)
            {
                Gizmos.color = _isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(_groundCheck.position, GroundCheckRadius);
            }

            if (_wallCheck != null && _enableWallMechanics)
            {
                Gizmos.color = _isTouchingWall ? Color.blue : Color.gray;
                Vector3 dir = _isFacingRight ? Vector3.right : Vector3.left;
                Gizmos.DrawRay(_wallCheck.position, dir * WallCheckDistance);
            }
        }

        #endregion

        #region Input Callbacks

        private void OnJumpPerformed(InputAction.CallbackContext ctx)
        {
            _jumpHeld = true;
            _jumpBufferCounter = JumpBufferDuration;
        }

        private void OnJumpCanceled(InputAction.CallbackContext ctx)
        {
            _jumpHeld = false;
        }

        private void OnDashPerformed(InputAction.CallbackContext ctx)
        {
            if (!_enableDash || _isDashing || _dashCooldownCounter > 0f) return;
            StartDash();
        }

        #endregion

        #region Private Methods

        private void ReadInput()
        {
            if (_moveAction != null && _wallJumpLockCounter <= 0f)
            {
                _moveInput = _moveAction.action.ReadValue<Vector2>();
            }
        }

        private void CheckGround()
        {
            _wasGrounded = _isGrounded;
            _isGrounded = _groundCheck != null &&
                Physics2D.OverlapCircle(_groundCheck.position, GroundCheckRadius, _groundLayer);

            if (!_wasGrounded && _isGrounded)
            {
                _airJumpsRemaining = _maxAirJumps;
                OnLanded?.Invoke();
            }
        }

        private void CheckWall()
        {
            if (!_enableWallMechanics || _wallCheck == null)
            {
                _isTouchingWall = false;
                return;
            }

            Vector2 dir = _isFacingRight ? Vector2.right : Vector2.left;
            _isTouchingWall = Physics2D.Raycast(_wallCheck.position, dir, WallCheckDistance, _wallLayer);
        }

        private void HandleCoyoteTime()
        {
            _coyoteTimeCounter = _isGrounded ? CoyoteTimeDuration : _coyoteTimeCounter - Time.deltaTime;
        }

        private void HandleJumpBuffer()
        {
            if (_jumpBufferCounter <= 0f) return;
            _jumpBufferCounter -= Time.deltaTime;

            // Ground jump or coyote jump
            if (_coyoteTimeCounter > 0f)
            {
                ExecuteJump();
                return;
            }

            // Wall jump
            if (_enableWallMechanics && _isWallSliding)
            {
                ExecuteWallJump();
                return;
            }

            // Air jump
            if (_airJumpsRemaining > 0)
            {
                _airJumpsRemaining--;
                ExecuteJump();
            }
        }

        private void HandleWallSlide()
        {
            _isWallSliding = _enableWallMechanics &&
                             _isTouchingWall &&
                             !_isGrounded &&
                             _rb.linearVelocity.y < 0f &&
                             Mathf.Abs(_moveInput.x) > 0.1f;
        }

        private void HandleFlip()
        {
            if ((_moveInput.x > 0.1f && !_isFacingRight) || (_moveInput.x < -0.1f && _isFacingRight))
            {
                _isFacingRight = !_isFacingRight;
                Vector3 scale = transform.localScale;
                scale.x *= -1f;
                transform.localScale = scale;
            }
        }

        private void UpdateTimers()
        {
            if (_wallJumpLockCounter > 0f) _wallJumpLockCounter -= Time.deltaTime;
            if (_dashCooldownCounter > 0f) _dashCooldownCounter -= Time.deltaTime;
        }

        private void ApplyMovement()
        {
            if (_wallJumpLockCounter > 0f) return;

            float targetSpeed = _moveInput.x * _moveSpeed;
            float currentSpeed = _rb.linearVelocity.x;
            float rate = Mathf.Abs(targetSpeed) > 0.01f ? _acceleration : _deceleration;
            float newSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.fixedDeltaTime);

            _rb.linearVelocity = new Vector2(newSpeed, _rb.linearVelocity.y);
        }

        private void ApplyBetterJumpGravity()
        {
            if (_isWallSliding) return;

            if (_rb.linearVelocity.y < 0f)
            {
                _rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * (_fallMultiplier - 1f) * Time.fixedDeltaTime);
            }
            else if (_rb.linearVelocity.y > 0f && !_jumpHeld)
            {
                _rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * (_lowJumpMultiplier - 1f) * Time.fixedDeltaTime);
            }
        }

        private void ApplyWallSlideGravity()
        {
            if (!_isWallSliding) return;

            if (_rb.linearVelocity.y < -_wallSlideSpeed)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, -_wallSlideSpeed);
            }
        }

        private void ExecuteJump()
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
            _coyoteTimeCounter = 0f;
            _jumpBufferCounter = 0f;
            _isGrounded = false;
            _animator?.SetTrigger(AnimHash_Jump);
            OnJumped?.Invoke();
        }

        private void ExecuteWallJump()
        {
            float wallDir = _isFacingRight ? -1f : 1f;
            _rb.linearVelocity = new Vector2(wallDir * _wallJumpForce.x, _wallJumpForce.y);
            _wallJumpLockCounter = _wallJumpLockDuration;
            _jumpBufferCounter = 0f;

            // Flip away from wall
            _isFacingRight = !_isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;

            _animator?.SetTrigger(AnimHash_Jump);
            OnWallJumped?.Invoke();
        }

        private async void StartDash()
        {
            _dashCts?.Cancel();
            _dashCts?.Dispose();
            _dashCts = new CancellationTokenSource();

            _isDashing = true;
            _dashCooldownCounter = _dashCooldown;

            float dashDir = _isFacingRight ? 1f : -1f;
            float originalGravity = _rb.gravityScale;
            _rb.gravityScale = 0f;
            _rb.linearVelocity = new Vector2(dashDir * _dashSpeed, 0f);

            _animator?.SetTrigger(AnimHash_Dash);
            OnDashStarted?.Invoke();

            try
            {
                await Awaitable.WaitForSecondsAsync(_dashDuration, _dashCts.Token);
            }
            catch (OperationCanceledException)
            {
                // Dash was cancelled (OnDisable)
            }
            finally
            {
                _rb.gravityScale = originalGravity;
                _isDashing = false;
            }
        }

        private void UpdateAnimator()
        {
            if (_animator == null) return;

            _animator.SetFloat(AnimHash_Speed, Mathf.Abs(_moveInput.x));
            _animator.SetFloat(AnimHash_VelocityY, _rb.linearVelocity.y);
            _animator.SetBool(AnimHash_IsGrounded, _isGrounded);
            _animator.SetBool(AnimHash_IsWallSliding, _isWallSliding);
        }

        #endregion
    }
}
