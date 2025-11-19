using System;
using UnityEngine;
using Knockout.Characters.Data;
using Knockout.Combat;
using Knockout.Combat.States;
using Knockout.Combat.HitDetection;

namespace Knockout.Characters.Components
{
    /// <summary>
    /// Manages parry detection, timing, cooldown, and counter window.
    /// Integrates with hit detection to intercept hits at precise timing.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class CharacterParry : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField]
        [Tooltip("Parry configuration data")]
        private ParryData parryData;

        [Header("Dependencies")]
        [SerializeField]
        private CharacterInput characterInput;

        [SerializeField]
        private CombatStateMachine combatStateMachine;

        // Component references
        private CharacterCombat _characterCombat;

        // Parry timing tracking (frame-based for precision)
        private int _lastBlockPressFrame;
        private int _currentFrame;
        private const int TARGET_FRAME_RATE = 60;

        // Cooldown tracking
        private int _cooldownFramesRemaining;

        // Counter window tracking
        private float _counterWindowTimer;
        private bool _inCounterWindow;

        private bool _isInitialized = false;

        #region Events

        /// <summary>
        /// Fired when parry succeeds.
        /// Parameter: attacker CharacterCombat
        /// </summary>
        public event Action<CharacterCombat> OnParrySuccess;

        /// <summary>
        /// Fired when parry is ready (cooldown complete).
        /// </summary>
        public event Action OnParryReady;

        /// <summary>
        /// Fired when counter window opens after successful parry.
        /// </summary>
        public event Action OnCounterWindowOpened;

        /// <summary>
        /// Fired when counter window closes.
        /// </summary>
        public event Action OnCounterWindowClosed;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets whether character can currently parry.
        /// </summary>
        public bool CanParry
        {
            get
            {
                if (!_isInitialized || parryData == null)
                {
                    return false;
                }

                // Check cooldown
                if (_cooldownFramesRemaining > 0)
                {
                    return false;
                }

                // Can parry from states where blocking is allowed
                CombatState currentState = combatStateMachine?.CurrentState;
                return currentState is IdleState
                    || currentState is BlockingState
                    || currentState is ExhaustedState;
            }
        }

        /// <summary>
        /// Gets whether character is in counter window.
        /// </summary>
        public bool IsInCounterWindow => _inCounterWindow;

        /// <summary>
        /// Gets cooldown progress (0-1, where 0 = ready, 1 = just used).
        /// </summary>
        public float CooldownProgress
        {
            get
            {
                if (parryData == null || parryData.ParryCooldownFrames == 0)
                {
                    return 0f;
                }

                return Mathf.Clamp01((float)_cooldownFramesRemaining / parryData.ParryCooldownFrames);
            }
        }

        /// <summary>
        /// Gets counter window progress (0-1, where 0 = start, 1 = expired).
        /// </summary>
        public float CounterWindowProgress
        {
            get
            {
                if (!_inCounterWindow || parryData == null)
                {
                    return 0f;
                }

                return Mathf.Clamp01(_counterWindowTimer / parryData.CounterWindowDuration);
            }
        }

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

            // Increment frame counter
            _currentFrame++;

            // Update cooldown timer (frame-based)
            if (_cooldownFramesRemaining > 0)
            {
                _cooldownFramesRemaining--;

                if (_cooldownFramesRemaining == 0)
                {
                    OnParryReady?.Invoke();
                }
            }
        }

        private void Update()
        {
            if (!_isInitialized)
            {
                return;
            }

            // Update counter window timer
            if (_inCounterWindow)
            {
                _counterWindowTimer += Time.deltaTime;

                if (_counterWindowTimer >= parryData.CounterWindowDuration)
                {
                    // Counter window expired
                    _inCounterWindow = false;
                    OnCounterWindowClosed?.Invoke();
                }
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from input events
            if (characterInput != null)
            {
                characterInput.OnBlockPressed -= OnBlockPressedInput;
            }
        }

        private void OnValidate()
        {
            // Validate dependencies in editor
            if (parryData == null)
            {
                Debug.LogWarning($"[{gameObject.name}] CharacterParry: ParryData not assigned!", this);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the parry system. Called by CharacterController.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            // Validate dependencies
            if (parryData == null)
            {
                Debug.LogError($"[{gameObject.name}] CharacterParry: ParryData is required!", this);
                return;
            }

            if (combatStateMachine == null)
            {
                Debug.LogError($"[{gameObject.name}] CharacterParry: CombatStateMachine is required!", this);
                return;
            }

            if (_characterCombat == null)
            {
                Debug.LogError($"[{gameObject.name}] CharacterParry: CharacterCombat is required!", this);
                return;
            }

            // Subscribe to input events
            if (characterInput != null)
            {
                characterInput.OnBlockPressed += OnBlockPressedInput;
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] CharacterParry: CharacterInput not found. Parry can only be triggered manually.", this);
            }

            _isInitialized = true;
            _currentFrame = 0;
            _lastBlockPressFrame = -1000; // Far in past
        }

        /// <summary>
        /// Attempts to parry an incoming hit.
        /// Called by hit detection system before damage is applied.
        /// </summary>
        /// <param name="hitData">Data about the incoming hit</param>
        /// <param name="attacker">The attacker CharacterCombat</param>
        /// <returns>True if parry successful, false otherwise</returns>
        public bool TryParry(HitData hitData, CharacterCombat attacker)
        {
            // Check if parry is available
            if (!CanParry)
            {
                return false;
            }

            // Check if block was pressed within parry window
            int framesSinceBlockPress = _currentFrame - _lastBlockPressFrame;

            if (framesSinceBlockPress > parryData.ParryWindowFrames)
            {
                // Outside parry window - too long ago
                return false;
            }

            // Parry successful!
            ExecuteParry(attacker);
            return true;
        }

        #endregion

        #region Input Handlers

        private void OnBlockPressedInput()
        {
            // Record when block was pressed for parry timing check
            _lastBlockPressFrame = _currentFrame;
        }

        #endregion

        #region Private Methods

        private void ExecuteParry(CharacterCombat attacker)
        {
            // Start cooldown
            _cooldownFramesRemaining = parryData.ParryCooldownFrames;

            // Stagger the attacker
            if (attacker != null)
            {
                CombatStateMachine attackerStateMachine = attacker.GetComponent<CombatStateMachine>();
                if (attackerStateMachine != null)
                {
                    // Create parry stagger state
                    ParryStaggerState staggerState = new ParryStaggerState();

                    // Exit current state manually
                    CombatState oldState = attackerStateMachine.CurrentState;
                    oldState?.Exit(attacker);

                    // Enter stagger state with duration
                    staggerState.Enter(attacker, parryData.AttackerStaggerDuration);

                    // Manually transition state machine
                    // Use reflection to set current state since ChangeState validates transitions
                    var stateMachineType = typeof(CombatStateMachine);
                    var currentStateField = stateMachineType.GetField("_currentState",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    currentStateField?.SetValue(attackerStateMachine, staggerState);

                    // Fire state change event
                    var onStateChangedEvent = stateMachineType.GetField("OnStateChanged",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    var eventDelegate = onStateChangedEvent?.GetValue(attackerStateMachine) as System.Action<CombatState, CombatState>;
                    eventDelegate?.Invoke(oldState, staggerState);
                }
            }

            // Open counter window
            _inCounterWindow = true;
            _counterWindowTimer = 0f;
            OnCounterWindowOpened?.Invoke();

            // Fire parry success event
            OnParrySuccess?.Invoke(attacker);
        }

        #endregion
    }
}
