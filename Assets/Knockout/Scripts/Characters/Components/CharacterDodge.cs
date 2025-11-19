using System;
using UnityEngine;
using Knockout.Characters.Data;
using Knockout.Combat;
using Knockout.Combat.States;

namespace Knockout.Characters.Components
{
    /// <summary>
    /// Manages dodge execution, cooldown, and orchestration with combat state machine.
    /// Subscribes to dodge input events and triggers dodge state transitions.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class CharacterDodge : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField]
        [Tooltip("Dodge configuration data")]
        private DodgeData dodgeData;

        [Header("Dependencies")]
        [SerializeField]
        private CharacterInput characterInput;

        [SerializeField]
        private CombatStateMachine combatStateMachine;

        // Component references
        private CharacterCombat _characterCombat;

        // Cooldown tracking (frame-based for precision)
        private int _cooldownFramesRemaining;
        private bool _isInitialized = false;

        #region Events

        /// <summary>
        /// Fired when dodge is ready (cooldown complete).
        /// </summary>
        public event Action OnDodgeReady;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets whether character can currently dodge.
        /// </summary>
        public bool CanDodge
        {
            get
            {
                if (!_isInitialized || dodgeData == null || combatStateMachine == null)
                {
                    return false;
                }

                // Check cooldown
                if (_cooldownFramesRemaining > 0)
                {
                    return false;
                }

                // Check if current state allows dodge
                CombatState currentState = combatStateMachine.CurrentState;

                // Can dodge from Idle, Blocking, or Exhausted states
                return currentState is IdleState
                    || currentState is BlockingState
                    || currentState is ExhaustedState;
            }
        }

        /// <summary>
        /// Gets whether character is currently dodging.
        /// </summary>
        public bool IsDodging
        {
            get
            {
                return _isInitialized
                    && combatStateMachine != null
                    && combatStateMachine.CurrentState is DodgingState;
            }
        }

        /// <summary>
        /// Gets cooldown progress (0-1, where 0 = ready, 1 = just used).
        /// </summary>
        public float CooldownProgress
        {
            get
            {
                if (dodgeData == null || dodgeData.CooldownFrames == 0)
                {
                    return 0f;
                }

                return Mathf.Clamp01((float)_cooldownFramesRemaining / dodgeData.CooldownFrames);
            }
        }

        /// <summary>
        /// Gets cooldown frames remaining.
        /// </summary>
        public int CooldownFramesRemaining => _cooldownFramesRemaining;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Auto-find dependencies if not assigned
            if (characterInput == null)
            {
                characterInput = GetComponent<CharacterInput>();
            }

            if (combatStateMachine == null)
            {
                combatStateMachine = GetComponent<CombatStateMachine>();
            }

            _characterCombat = GetComponent<CharacterCombat>();
        }

        private void FixedUpdate()
        {
            if (!_isInitialized)
            {
                return;
            }

            // Update cooldown timer (frame-based)
            if (_cooldownFramesRemaining > 0)
            {
                _cooldownFramesRemaining--;

                // Fire ready event when cooldown completes
                if (_cooldownFramesRemaining == 0)
                {
                    OnDodgeReady?.Invoke();
                }
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from input events
            if (characterInput != null)
            {
                characterInput.OnDodgeLeftPressed -= OnDodgeLeftInput;
                characterInput.OnDodgeRightPressed -= OnDodgeRightInput;
                characterInput.OnDodgeBackPressed -= OnDodgeBackInput;
            }
        }

        private void OnValidate()
        {
            // Validate dependencies in editor
            if (dodgeData == null)
            {
                Debug.LogWarning($"[{gameObject.name}] CharacterDodge: DodgeData not assigned!", this);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the dodge system. Called by CharacterController.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            // Validate dependencies
            if (dodgeData == null)
            {
                Debug.LogError($"[{gameObject.name}] CharacterDodge: DodgeData is required!", this);
                return;
            }

            if (combatStateMachine == null)
            {
                Debug.LogError($"[{gameObject.name}] CharacterDodge: CombatStateMachine is required!", this);
                return;
            }

            if (_characterCombat == null)
            {
                Debug.LogError($"[{gameObject.name}] CharacterDodge: CharacterCombat is required!", this);
                return;
            }

            // Subscribe to input events
            if (characterInput != null)
            {
                characterInput.OnDodgeLeftPressed += OnDodgeLeftInput;
                characterInput.OnDodgeRightPressed += OnDodgeRightInput;
                characterInput.OnDodgeBackPressed += OnDodgeBackInput;
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] CharacterDodge: CharacterInput not found. Dodge can only be triggered manually.", this);
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Attempts to trigger a dodge in the specified direction.
        /// </summary>
        /// <param name="direction">Direction to dodge</param>
        /// <returns>True if dodge was triggered successfully</returns>
        public bool TryDodge(DodgeDirection direction)
        {
            // Check if dodge is available
            if (!CanDodge)
            {
                return false;
            }

            // Trigger dodge state transition
            bool success = combatStateMachine.TriggerDodge(direction, dodgeData);

            if (success)
            {
                // Start cooldown
                _cooldownFramesRemaining = dodgeData.CooldownFrames;
            }

            return success;
        }

        #endregion

        #region Input Handlers

        private void OnDodgeLeftInput()
        {
            TryDodge(DodgeDirection.Left);
        }

        private void OnDodgeRightInput()
        {
            TryDodge(DodgeDirection.Right);
        }

        private void OnDodgeBackInput()
        {
            TryDodge(DodgeDirection.Back);
        }

        #endregion
    }
}
