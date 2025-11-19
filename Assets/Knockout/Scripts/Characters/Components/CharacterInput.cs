using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Knockout.Characters.Components
{
    /// <summary>
    /// Connects Unity Input System to character components via event-driven architecture.
    /// Subscribes to Input Actions and raises events for other components to consume.
    /// NOTE: Requires KnockoutInputActions.inputactions asset to be generated.
    /// </summary>
    public class CharacterInput : MonoBehaviour
    {
        // Input actions reference - will be assigned manually or auto-generated
        private InputActionAsset _inputActions;
        private InputActionMap _gameplayActionMap;

        // Individual actions
        private InputAction _movementAction;
        private InputAction _jabAction;
        private InputAction _hookAction;
        private InputAction _uppercutAction;
        private InputAction _blockAction;
        private InputAction _dodgeLeftAction;
        private InputAction _dodgeRightAction;
        private InputAction _dodgeBackAction;

        // Input enabled state
        private bool _inputEnabled = true;

        #region Events

        /// <summary>
        /// Fired when movement input changes.
        /// </summary>
        public event Action<Vector2> OnMoveInput;

        /// <summary>
        /// Fired when jab button is pressed.
        /// </summary>
        public event Action OnJabPressed;

        /// <summary>
        /// Fired when hook button is pressed.
        /// </summary>
        public event Action OnHookPressed;

        /// <summary>
        /// Fired when uppercut button is pressed.
        /// </summary>
        public event Action OnUppercutPressed;

        /// <summary>
        /// Fired when block button is pressed.
        /// </summary>
        public event Action OnBlockPressed;

        /// <summary>
        /// Fired when block button is released.
        /// </summary>
        public event Action OnBlockReleased;

        /// <summary>
        /// Fired when dodge left button is pressed.
        /// </summary>
        public event Action OnDodgeLeftPressed;

        /// <summary>
        /// Fired when dodge right button is pressed.
        /// </summary>
        public event Action OnDodgeRightPressed;

        /// <summary>
        /// Fired when dodge back button is pressed.
        /// </summary>
        public event Action OnDodgeBackPressed;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Try to load input actions asset
            LoadInputActions();

            if (_inputActions != null)
            {
                // Get gameplay action map
                _gameplayActionMap = _inputActions.FindActionMap("Gameplay");

                if (_gameplayActionMap != null)
                {
                    // Cache individual actions
                    _movementAction = _gameplayActionMap.FindAction("Movement");
                    _jabAction = _gameplayActionMap.FindAction("Jab");
                    _hookAction = _gameplayActionMap.FindAction("Hook");
                    _uppercutAction = _gameplayActionMap.FindAction("Uppercut");
                    _blockAction = _gameplayActionMap.FindAction("Block");
                    _dodgeLeftAction = _gameplayActionMap.FindAction("DodgeLeft");
                    _dodgeRightAction = _gameplayActionMap.FindAction("DodgeRight");
                    _dodgeBackAction = _gameplayActionMap.FindAction("DodgeBack");
                }
                else
                {
                    Debug.LogWarning($"[{gameObject.name}] CharacterInput: Gameplay action map not found in Input Actions!", this);
                }
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] CharacterInput: Input Actions asset not found! " +
                    "Generate it via Tools > Knockout > Generate Input Actions", this);
            }
        }

        private void OnEnable()
        {
            // Subscribe to input actions
            SubscribeToInputs();

            // Enable input actions
            _gameplayActionMap?.Enable();
        }

        private void OnDisable()
        {
            // Unsubscribe from input actions
            UnsubscribeFromInputs();

            // Disable input actions
            _gameplayActionMap?.Disable();
        }

        private void Update()
        {
            // Poll movement input (it's a continuous value)
            if (_inputEnabled && _movementAction != null)
            {
                Vector2 movement = _movementAction.ReadValue<Vector2>();
                OnMoveInput?.Invoke(movement);
            }
        }

        #endregion

        #region Input System Setup

        private void LoadInputActions()
        {
            // Try to load KnockoutInputActions asset from Resources
            _inputActions = Resources.Load<InputActionAsset>("Input/KnockoutInputActions");

            // Note: If using the auto-generated C# class instead, this method can be simplified
            // to instantiate the generated class directly (e.g., new KnockoutInputActions())
        }

        private void SubscribeToInputs()
        {
            if (_jabAction != null)
            {
                _jabAction.performed += OnJabInput;
            }

            if (_hookAction != null)
            {
                _hookAction.performed += OnHookInput;
            }

            if (_uppercutAction != null)
            {
                _uppercutAction.performed += OnUppercutInput;
            }

            if (_blockAction != null)
            {
                _blockAction.started += OnBlockInput;
                _blockAction.canceled += OnBlockCanceled;
            }

            if (_dodgeLeftAction != null)
            {
                _dodgeLeftAction.performed += OnDodgeLeftInput;
            }

            if (_dodgeRightAction != null)
            {
                _dodgeRightAction.performed += OnDodgeRightInput;
            }

            if (_dodgeBackAction != null)
            {
                _dodgeBackAction.performed += OnDodgeBackInput;
            }
        }

        private void UnsubscribeFromInputs()
        {
            if (_jabAction != null)
            {
                _jabAction.performed -= OnJabInput;
            }

            if (_hookAction != null)
            {
                _hookAction.performed -= OnHookInput;
            }

            if (_uppercutAction != null)
            {
                _uppercutAction.performed -= OnUppercutInput;
            }

            if (_blockAction != null)
            {
                _blockAction.started -= OnBlockInput;
                _blockAction.canceled -= OnBlockCanceled;
            }

            if (_dodgeLeftAction != null)
            {
                _dodgeLeftAction.performed -= OnDodgeLeftInput;
            }

            if (_dodgeRightAction != null)
            {
                _dodgeRightAction.performed -= OnDodgeRightInput;
            }

            if (_dodgeBackAction != null)
            {
                _dodgeBackAction.performed -= OnDodgeBackInput;
            }
        }

        #endregion

        #region Input Callbacks

        private void OnJabInput(InputAction.CallbackContext context)
        {
            if (_inputEnabled)
            {
                OnJabPressed?.Invoke();
            }
        }

        private void OnHookInput(InputAction.CallbackContext context)
        {
            if (_inputEnabled)
            {
                OnHookPressed?.Invoke();
            }
        }

        private void OnUppercutInput(InputAction.CallbackContext context)
        {
            if (_inputEnabled)
            {
                OnUppercutPressed?.Invoke();
            }
        }

        private void OnBlockInput(InputAction.CallbackContext context)
        {
            if (_inputEnabled)
            {
                OnBlockPressed?.Invoke();
            }
        }

        private void OnBlockCanceled(InputAction.CallbackContext context)
        {
            if (_inputEnabled)
            {
                OnBlockReleased?.Invoke();
            }
        }

        private void OnDodgeLeftInput(InputAction.CallbackContext context)
        {
            if (_inputEnabled)
            {
                OnDodgeLeftPressed?.Invoke();
            }
        }

        private void OnDodgeRightInput(InputAction.CallbackContext context)
        {
            if (_inputEnabled)
            {
                OnDodgeRightPressed?.Invoke();
            }
        }

        private void OnDodgeBackInput(InputAction.CallbackContext context)
        {
            if (_inputEnabled)
            {
                OnDodgeBackPressed?.Invoke();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Enables input processing.
        /// </summary>
        public void EnableInput()
        {
            _inputEnabled = true;
        }

        /// <summary>
        /// Disables input processing.
        /// Input events will not fire while disabled.
        /// </summary>
        public void DisableInput()
        {
            _inputEnabled = false;

            // Reset movement input to zero
            OnMoveInput?.Invoke(Vector2.zero);
        }

        /// <summary>
        /// Gets whether input is currently enabled.
        /// </summary>
        public bool IsInputEnabled => _inputEnabled;

        #endregion
    }
}
