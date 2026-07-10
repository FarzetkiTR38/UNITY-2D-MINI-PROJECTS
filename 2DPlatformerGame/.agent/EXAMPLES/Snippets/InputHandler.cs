// ============================================================================
// InputHandler.cs
// Purpose: Centralized Input System handler for multi-platform input
// Dependencies: Input System Package
// Unity Version: 6000.3.18f1
// ============================================================================

using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameName.Systems.Input
{
    /// <summary>
    /// Centralized input handler wrapping the Input System for multi-platform support.
    /// Manages action maps, device detection, and input context switching.
    /// </summary>
    [DisallowMultipleComponent]
    public class InputHandler : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Input Actions")]
        [Tooltip("The Input Action Asset containing all action maps.")]
        [SerializeField] private InputActionAsset _inputActionAsset;

        [Header("Action Map Names")]
        [Tooltip("Name of the gameplay action map.")]
        [SerializeField] private string _playerActionMap = "Player";

        [Tooltip("Name of the UI action map.")]
        [SerializeField] private string _uiActionMap = "UI";

        #endregion

        #region Private Fields

        private InputActionMap _playerActions;
        private InputActionMap _uiActions;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _attackAction;
        private InputAction _dashAction;
        private InputAction _interactAction;
        private InputAction _pauseAction;
        private string _lastDeviceLayout;

        #endregion

        #region Properties

        /// <summary>Gets the current movement input as a Vector2.</summary>
        public Vector2 MoveInput => _moveAction?.ReadValue<Vector2>() ?? Vector2.zero;

        /// <summary>Gets the current input device type.</summary>
        public InputDeviceType CurrentDeviceType { get; private set; } = InputDeviceType.KeyboardMouse;

        #endregion

        #region Events

        /// <summary>Raised when jump is performed.</summary>
        public event Action OnJumpPressed;

        /// <summary>Raised when jump is released.</summary>
        public event Action OnJumpReleased;

        /// <summary>Raised when attack is performed.</summary>
        public event Action OnAttackPressed;

        /// <summary>Raised when dash is performed.</summary>
        public event Action OnDashPressed;

        /// <summary>Raised when interact is performed.</summary>
        public event Action OnInteractPressed;

        /// <summary>Raised when pause is performed.</summary>
        public event Action OnPausePressed;

        /// <summary>Raised when input device type changes.</summary>
        public event Action<InputDeviceType> OnDeviceChanged;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_inputActionAsset == null)
            {
                Debug.LogError($"[{name}] Input Action Asset not assigned!", this);
                return;
            }

            CacheActionMaps();
            CacheActions();
        }

        private void OnEnable()
        {
            SubscribeToActions();
            EnablePlayerInput();
            InputSystem.onActionChange += OnActionChange;
        }

        private void OnDisable()
        {
            UnsubscribeFromActions();
            _playerActions?.Disable();
            _uiActions?.Disable();
            InputSystem.onActionChange -= OnActionChange;
        }

        #endregion

        #region Public Methods

        /// <summary>Switches to the Player action map for gameplay.</summary>
        public void EnablePlayerInput()
        {
            _uiActions?.Disable();
            _playerActions?.Enable();
        }

        /// <summary>Switches to the UI action map for menu navigation.</summary>
        public void EnableUIInput()
        {
            _playerActions?.Disable();
            _uiActions?.Enable();
        }

        /// <summary>Disables all input action maps.</summary>
        public void DisableAllInput()
        {
            _playerActions?.Disable();
            _uiActions?.Disable();
        }

        #endregion

        #region Private Methods

        private void CacheActionMaps()
        {
            _playerActions = _inputActionAsset.FindActionMap(_playerActionMap);
            _uiActions = _inputActionAsset.FindActionMap(_uiActionMap);

            if (_playerActions == null)
                Debug.LogError($"[{name}] Action map '{_playerActionMap}' not found!", this);
            if (_uiActions == null)
                Debug.LogWarning($"[{name}] Action map '{_uiActionMap}' not found.", this);
        }

        private void CacheActions()
        {
            if (_playerActions == null) return;

            _moveAction = _playerActions.FindAction("Move");
            _jumpAction = _playerActions.FindAction("Jump");
            _attackAction = _playerActions.FindAction("Attack");
            _dashAction = _playerActions.FindAction("Dash");
            _interactAction = _playerActions.FindAction("Interact");
            _pauseAction = _playerActions.FindAction("Pause");
        }

        private void SubscribeToActions()
        {
            if (_jumpAction != null)
            {
                _jumpAction.performed += ctx => OnJumpPressed?.Invoke();
                _jumpAction.canceled += ctx => OnJumpReleased?.Invoke();
            }
            if (_attackAction != null)
                _attackAction.performed += ctx => OnAttackPressed?.Invoke();
            if (_dashAction != null)
                _dashAction.performed += ctx => OnDashPressed?.Invoke();
            if (_interactAction != null)
                _interactAction.performed += ctx => OnInteractPressed?.Invoke();
            if (_pauseAction != null)
                _pauseAction.performed += ctx => OnPausePressed?.Invoke();
        }

        private void UnsubscribeFromActions()
        {
            _jumpAction?.Reset();
            _attackAction?.Reset();
            _dashAction?.Reset();
            _interactAction?.Reset();
            _pauseAction?.Reset();
        }

        private void OnActionChange(object obj, InputActionChange change)
        {
            if (change != InputActionChange.ActionPerformed) return;
            if (obj is not InputAction action) return;

            InputDevice device = action.activeControl?.device;
            if (device == null) return;

            string layout = device.layout;
            if (layout == _lastDeviceLayout) return;
            _lastDeviceLayout = layout;

            InputDeviceType newType = layout switch
            {
                "Keyboard" or "Mouse" => InputDeviceType.KeyboardMouse,
                "Touchscreen" => InputDeviceType.Touch,
                _ => InputDeviceType.Gamepad
            };

            if (newType != CurrentDeviceType)
            {
                CurrentDeviceType = newType;
                OnDeviceChanged?.Invoke(CurrentDeviceType);
            }
        }

        #endregion
    }

    /// <summary>Defines the active input device type.</summary>
    public enum InputDeviceType
    {
        /// <summary>Keyboard and mouse input.</summary>
        KeyboardMouse = 0,

        /// <summary>Gamepad/controller input.</summary>
        Gamepad = 1,

        /// <summary>Touchscreen input.</summary>
        Touch = 2
    }
}
