using System;
using UnityEngine;
using Knockout.Characters.Data;

namespace Knockout.Characters.Components
{
    /// <summary>
    /// Failure reasons for special move execution.
    /// </summary>
    public enum SpecialMoveFailureReason
    {
        OnCooldown,
        InsufficientStamina,
        InvalidState
    }

    /// <summary>
    /// Manages special move execution, cooldown tracking, and stamina gating.
    /// Special moves are powerful signature techniques with dual-resource requirements (cooldown + stamina).
    /// </summary>
    public class CharacterSpecialMoves : MonoBehaviour
    {
        [Header("Special Move Configuration")]
        [SerializeField]
        [Tooltip("Special move data for this character")]
        private SpecialMoveData specialMoveData;

        // Component references
        private CharacterInput _characterInput;
        private CharacterStamina _characterStamina;
        private CharacterCombat _characterCombat;

        // Cooldown state
        private float _cooldownTimeRemaining = 0f;
        private bool _isInitialized = false;

        #region Events

        /// <summary>
        /// Fired when special move is successfully used.
        /// Parameter: SpecialMoveData
        /// </summary>
        public event Action<SpecialMoveData> OnSpecialMoveUsed;

        /// <summary>
        /// Fired when special move fails to execute.
        /// Parameter: SpecialMoveFailureReason
        /// </summary>
        public event Action<SpecialMoveFailureReason> OnSpecialMoveFailed;

        /// <summary>
        /// Fired when special move cooldown finishes and move is ready to use.
        /// </summary>
        public event Action OnSpecialMoveReady;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets whether special move is currently on cooldown.
        /// </summary>
        public bool IsOnCooldown => _cooldownTimeRemaining > 0f;

        /// <summary>
        /// Gets the cooldown progress as a value from 0 to 1.
        /// 0 = ready to use, 1 = just used.
        /// </summary>
        public float CooldownProgress
        {
            get
            {
                if (specialMoveData == null) return 0f;
                float totalCooldown = specialMoveData.CooldownSeconds;
                if (totalCooldown <= 0f) return 0f;
                return Mathf.Clamp01(_cooldownTimeRemaining / totalCooldown);
            }
        }

        /// <summary>
        /// Gets the remaining cooldown time in seconds.
        /// </summary>
        public float CooldownTimeRemaining => _cooldownTimeRemaining;

        /// <summary>
        /// Gets the special move data.
        /// </summary>
        public SpecialMoveData SpecialMoveData => specialMoveData;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Cache component references
            _characterInput = GetComponent<CharacterInput>();
            _characterStamina = GetComponent<CharacterStamina>();
            _characterCombat = GetComponent<CharacterCombat>();

            if (_characterInput == null)
            {
                Debug.LogError($"[{gameObject.name}] CharacterSpecialMoves requires CharacterInput component!", this);
            }

            if (_characterStamina == null)
            {
                Debug.LogError($"[{gameObject.name}] CharacterSpecialMoves requires CharacterStamina component!", this);
            }

            if (_characterCombat == null)
            {
                Debug.LogError($"[{gameObject.name}] CharacterSpecialMoves requires CharacterCombat component!", this);
            }
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (!_isInitialized) return;

            // Countdown cooldown timer
            if (_cooldownTimeRemaining > 0f)
            {
                bool wasOnCooldown = true;
                _cooldownTimeRemaining -= Time.deltaTime;

                if (_cooldownTimeRemaining <= 0f)
                {
                    _cooldownTimeRemaining = 0f;
                    // Fire ready event when cooldown expires
                    OnSpecialMoveReady?.Invoke();
                }
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from input events
            if (_characterInput != null)
            {
                _characterInput.OnSpecialMovePressed -= HandleSpecialMoveInput;
            }
        }

        private void OnValidate()
        {
            // Provide editor-time warnings
            if (specialMoveData == null)
            {
                Debug.LogWarning($"[{gameObject.name}] CharacterSpecialMoves: SpecialMoveData not assigned!", this);
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes special move system and subscribes to events.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            // Validate required data
            if (specialMoveData == null)
            {
                Debug.LogError($"[{gameObject.name}] CharacterSpecialMoves: SpecialMoveData is required!", this);
                return;
            }

            // Initialize cooldown state (ready to use at start)
            _cooldownTimeRemaining = 0f;

            // Subscribe to input events
            if (_characterInput != null)
            {
                _characterInput.OnSpecialMovePressed += HandleSpecialMoveInput;
            }

            _isInitialized = true;
        }

        #endregion

        #region Input Handling

        private void HandleSpecialMoveInput()
        {
            // Attempt to use special move on input
            TryUseSpecialMove();
        }

        #endregion

        #region Special Move Execution

        /// <summary>
        /// Attempts to execute the special move.
        /// Returns true if successful, false if failed.
        /// </summary>
        public bool TryUseSpecialMove()
        {
            // Check if special move can be used
            if (!CanUseSpecialMove(out SpecialMoveFailureReason failureReason))
            {
                // Fire failure event
                OnSpecialMoveFailed?.Invoke(failureReason);
                return false;
            }

            // Execute special move
            ExecuteSpecialMove();
            return true;
        }

        private bool CanUseSpecialMove(out SpecialMoveFailureReason failureReason)
        {
            failureReason = SpecialMoveFailureReason.InvalidState;

            // Check if on cooldown
            if (IsOnCooldown)
            {
                failureReason = SpecialMoveFailureReason.OnCooldown;
                return false;
            }

            // Check if sufficient stamina
            if (_characterStamina != null && specialMoveData != null)
            {
                if (_characterStamina.CurrentStamina < specialMoveData.StaminaCost)
                {
                    failureReason = SpecialMoveFailureReason.InsufficientStamina;
                    return false;
                }
            }

            // Check if in valid state (can attack)
            // For now, we assume if CharacterCombat exists and is valid, we can attack
            // This will be more sophisticated when integrated with combat system
            if (_characterCombat == null)
            {
                failureReason = SpecialMoveFailureReason.InvalidState;
                return false;
            }

            return true;
        }

        private void ExecuteSpecialMove()
        {
            if (specialMoveData == null || _characterCombat == null)
            {
                return;
            }

            // Execute special move attack through combat system
            // This will handle stamina consumption, damage, knockback, and hitbox activation
            bool success = _characterCombat.ExecuteSpecialMove(specialMoveData);

            if (!success)
            {
                // Attack execution failed (shouldn't happen since we checked in CanUseSpecialMove)
                Debug.LogWarning($"[{gameObject.name}] Special move execution failed unexpectedly!", this);
                return;
            }

            // Start cooldown
            _cooldownTimeRemaining = specialMoveData.CooldownSeconds;

            // Fire success event
            OnSpecialMoveUsed?.Invoke(specialMoveData);

            Debug.Log($"[{gameObject.name}] Special Move '{specialMoveData.SpecialMoveName}' executed! " +
                     $"Cooldown: {specialMoveData.CooldownSeconds}s, Stamina Cost: {specialMoveData.StaminaCost}");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets the cooldown timer (for testing or special circumstances).
        /// </summary>
        public void ResetCooldown()
        {
            bool wasOnCooldown = IsOnCooldown;
            _cooldownTimeRemaining = 0f;

            if (wasOnCooldown)
            {
                OnSpecialMoveReady?.Invoke();
            }
        }

        /// <summary>
        /// Starts the cooldown timer without executing the special move (for testing).
        /// </summary>
        public void StartCooldown()
        {
            if (specialMoveData != null)
            {
                _cooldownTimeRemaining = specialMoveData.CooldownSeconds;
            }
        }

        #endregion
    }
}
