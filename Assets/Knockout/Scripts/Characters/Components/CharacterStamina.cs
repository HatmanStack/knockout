using System;
using UnityEngine;
using Knockout.Characters.Data;

namespace Knockout.Characters.Components
{
    /// <summary>
    /// Manages character stamina, consumption, and regeneration.
    /// Stamina is consumed by offensive actions (attacks, special moves) and regenerates passively when not attacking.
    /// Defensive actions (blocking, dodging, parrying) are free.
    /// </summary>
    public class CharacterStamina : MonoBehaviour
    {
        [Header("Stamina Configuration")]
        [SerializeField]
        [Tooltip("Stamina configuration data")]
        private StaminaData staminaData;

        // Component references
        private CharacterCombat _characterCombat;

        // Current stamina state
        private float _currentStamina;
        private bool _isInitialized = false;
        private float _regenRateMultiplier = 1f;

        #region Events

        /// <summary>
        /// Fired when stamina changes.
        /// Parameters: (currentStamina, maxStamina)
        /// </summary>
        public event Action<float, float> OnStaminaChanged;

        /// <summary>
        /// Fired when stamina is depleted to zero.
        /// </summary>
        public event Action OnStaminaDepleted;

        /// <summary>
        /// Fired when attempting to attack without sufficient stamina.
        /// </summary>
        public event Action OnAttackFailedNoStamina;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the current stamina value.
        /// </summary>
        public float CurrentStamina => _currentStamina;

        /// <summary>
        /// Gets the maximum stamina from configuration.
        /// </summary>
        public float MaxStamina => staminaData != null ? staminaData.MaxStamina : 100f;

        /// <summary>
        /// Gets the current stamina as a percentage (0 to 1).
        /// </summary>
        public float StaminaPercentage => MaxStamina > 0 ? _currentStamina / MaxStamina : 0f;

        /// <summary>
        /// Gets whether stamina is currently depleted.
        /// </summary>
        public bool IsDepleted => _currentStamina <= 0f;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Cache component references
            _characterCombat = GetComponent<CharacterCombat>();

            if (_characterCombat == null)
            {
                Debug.LogError($"[{gameObject.name}] CharacterStamina requires CharacterCombat component!", this);
            }
        }

        private void Start()
        {
            Initialize();
        }

        private void FixedUpdate()
        {
            if (!_isInitialized) return;

            // Passive stamina regeneration
            RegenerateStamina();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (_characterCombat != null)
            {
                _characterCombat.OnAttackExecuted -= HandleAttackExecuted;
            }
        }

        private void OnValidate()
        {
            // Provide editor-time warnings
            if (staminaData == null)
            {
                Debug.LogWarning($"[{gameObject.name}] CharacterStamina: StaminaData not assigned!", this);
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes stamina system and subscribes to events.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            // Validate required data
            if (staminaData == null)
            {
                Debug.LogError($"[{gameObject.name}] CharacterStamina: StaminaData is required!", this);
                return;
            }

            // Initialize stamina to max
            _currentStamina = MaxStamina;
            _regenRateMultiplier = 1f;

            // Subscribe to combat events
            if (_characterCombat != null)
            {
                _characterCombat.OnAttackExecuted += HandleAttackExecuted;
            }

            _isInitialized = true;

            // Fire initial stamina changed event
            OnStaminaChanged?.Invoke(_currentStamina, MaxStamina);
        }

        #endregion

        #region Stamina Management

        /// <summary>
        /// Checks if sufficient stamina is available for an action.
        /// </summary>
        /// <param name="cost">Stamina cost to check</param>
        /// <returns>True if stamina is sufficient, false otherwise</returns>
        public bool HasStamina(float cost)
        {
            return _currentStamina >= cost;
        }

        /// <summary>
        /// Consumes stamina for an action.
        /// </summary>
        /// <param name="cost">Amount of stamina to consume</param>
        /// <returns>True if consumption succeeded, false if insufficient stamina</returns>
        public bool ConsumeStamina(float cost)
        {
            if (!HasStamina(cost))
            {
                return false;
            }

            _currentStamina -= cost;
            _currentStamina = Mathf.Max(0f, _currentStamina);

            // Fire events
            OnStaminaChanged?.Invoke(_currentStamina, MaxStamina);

            if (_currentStamina <= 0f)
            {
                OnStaminaDepleted?.Invoke();
            }

            return true;
        }

        /// <summary>
        /// Sets the stamina regeneration rate multiplier.
        /// </summary>
        /// <param name="multiplier">Regeneration multiplier (1.0 = normal, 0.5 = half speed)</param>
        public void SetRegenMultiplier(float multiplier)
        {
            _regenRateMultiplier = Mathf.Clamp(multiplier, 0f, 2f);
        }

        /// <summary>
        /// Resets regeneration multiplier to normal (1.0).
        /// </summary>
        public void ResetRegenMultiplier()
        {
            _regenRateMultiplier = 1f;
        }

        #endregion

        #region Regeneration

        private void RegenerateStamina()
        {
            // Don't regenerate if already at max
            if (_currentStamina >= MaxStamina)
            {
                return;
            }

            // Pause regeneration during attacks
            if (_characterCombat != null && _characterCombat.IsAttacking)
            {
                return;
            }

            // Calculate regeneration amount
            float regenAmount = staminaData.RegenPerSecond * _regenRateMultiplier * Time.fixedDeltaTime;
            float previousStamina = _currentStamina;

            // Apply regeneration
            _currentStamina += regenAmount;
            _currentStamina = Mathf.Min(_currentStamina, MaxStamina);

            // Fire event if value changed
            if (!Mathf.Approximately(previousStamina, _currentStamina))
            {
                OnStaminaChanged?.Invoke(_currentStamina, MaxStamina);
            }
        }

        #endregion

        #region Event Handlers

        private void HandleAttackExecuted(int attackTypeIndex)
        {
            // Determine stamina cost
            float staminaCost = GetStaminaCostForAttack(attackTypeIndex);

            // Consume stamina (already happened before attack execution, but this is for tracking)
            // Note: Stamina consumption is now handled by CharacterCombat integration in next task
            // This handler is here for future event listening (e.g., VFX, audio)
        }

        /// <summary>
        /// Gets the stamina cost for a specific attack type.
        /// Checks AttackData first, then falls back to StaminaData defaults.
        /// </summary>
        /// <param name="attackTypeIndex">Attack type (0=Jab, 1=Hook, 2=Uppercut)</param>
        /// <returns>Stamina cost for the attack</returns>
        private float GetStaminaCostForAttack(int attackTypeIndex)
        {
            if (staminaData == null)
            {
                return 0f;
            }

            // Get cost from StaminaData based on attack type
            return staminaData.GetAttackCost(attackTypeIndex);
        }

        /// <summary>
        /// Gets the stamina cost for a specific AttackData.
        /// Uses AttackData.StaminaCost if set (> 0), otherwise uses StaminaData default.
        /// </summary>
        /// <param name="attackData">Attack data to check</param>
        /// <returns>Stamina cost for the attack</returns>
        public float GetStaminaCostForAttackData(AttackData attackData)
        {
            if (attackData == null)
            {
                return 0f;
            }

            // If AttackData has custom stamina cost (> 0), use it
            if (attackData.StaminaCost > 0f)
            {
                return attackData.StaminaCost;
            }

            // Otherwise use default from StaminaData based on attack type
            return GetStaminaCostForAttack(attackData.AttackTypeIndex);
        }

        #endregion

        #region Test Helpers

        /// <summary>
        /// Sets current stamina (for testing purposes).
        /// </summary>
        public void SetCurrentStamina(float value)
        {
            _currentStamina = Mathf.Clamp(value, 0f, MaxStamina);
            OnStaminaChanged?.Invoke(_currentStamina, MaxStamina);

            if (_currentStamina <= 0f)
            {
                OnStaminaDepleted?.Invoke();
            }
        }

        #endregion
    }
}
