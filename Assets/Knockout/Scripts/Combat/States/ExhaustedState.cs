using System;
using UnityEngine;
using Knockout.Characters.Components;

namespace Knockout.Combat.States
{
    /// <summary>
    /// Exhausted combat state - character has depleted stamina and cannot attack.
    /// Defensive actions (blocking, dodging) are still allowed.
    /// Can transition to: Idle (after recovery), Blocking, HitStunned, KnockedDown, KnockedOut.
    /// Cannot transition to: Attacking (stamina depleted).
    /// </summary>
    public class ExhaustedState : CombatState
    {
        private float _exhaustionTimer;
        private CharacterStamina _stamina;
        private float _minimumExhaustionDuration;

        /// <summary>
        /// Event fired when exhaustion state begins.
        /// </summary>
        public static event Action<CharacterCombat> OnExhaustedStart;

        /// <summary>
        /// Event fired when exhaustion state ends.
        /// </summary>
        public static event Action<CharacterCombat> OnExhaustedEnd;

        public override void Enter(CharacterCombat combat)
        {
            // Get stamina component
            _stamina = combat.GetComponent<CharacterStamina>();

            if (_stamina == null)
            {
                Debug.LogError($"[{combat.gameObject.name}] ExhaustedState: CharacterStamina component required!", combat);
                return;
            }

            // Get exhaustion duration from stamina data
            var staminaDataField = typeof(CharacterStamina).GetField("staminaData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var staminaData = staminaDataField?.GetValue(_stamina) as Characters.Data.StaminaData;

            _minimumExhaustionDuration = staminaData != null ? staminaData.ExhaustionDuration : 2f;

            // Reset timer
            _exhaustionTimer = 0f;

            // Apply slower stamina regeneration
            if (staminaData != null)
            {
                _stamina.SetRegenMultiplier(staminaData.ExhaustionRegenMultiplier);
            }

            // Trigger exhaustion animation (use tired idle or standard idle)
            // Animation trigger would go here if we had a dedicated exhaustion animation
            CharacterAnimator animator = combat.GetComponent<CharacterAnimator>();
            if (animator != null)
            {
                // For now, exhaustion uses standard idle animation
                // Could add: animator.TriggerExhaustion() if animation exists
            }

            // Fire event
            OnExhaustedStart?.Invoke(combat);
        }

        public override void Update(CharacterCombat combat)
        {
            // Track exhaustion duration
            _exhaustionTimer += Time.deltaTime;

            // Check for recovery condition
            // Note: Actual auto-recovery transition is handled by CombatStateMachine
            // This state just tracks whether recovery is possible
        }

        public override void Exit(CharacterCombat combat)
        {
            // Reset stamina regeneration to normal
            if (_stamina != null)
            {
                _stamina.ResetRegenMultiplier();
            }

            // Fire event
            OnExhaustedEnd?.Invoke(combat);
        }

        public override bool CanTransitionTo(CombatState newState)
        {
            // Cannot attack while exhausted
            if (newState is AttackingState)
            {
                return false;
            }

            // Cannot stay in exhausted state
            if (newState is ExhaustedState)
            {
                return false;
            }

            // Can transition to idle if recovered
            if (newState is IdleState)
            {
                return CanRecover();
            }

            // Defensive actions allowed
            return newState is BlockingState
                || newState is HitStunnedState
                || newState is KnockedDownState
                || newState is KnockedOutState;
        }

        /// <summary>
        /// Checks if the character can recover from exhaustion.
        /// Recovery requires: minimum duration passed AND stamina above threshold.
        /// </summary>
        public bool CanRecover()
        {
            if (_stamina == null)
            {
                return true; // Safety fallback
            }

            // Check minimum duration
            bool minDurationPassed = _exhaustionTimer >= _minimumExhaustionDuration;

            // Check stamina threshold
            var staminaDataField = typeof(CharacterStamina).GetField("staminaData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var staminaData = staminaDataField?.GetValue(_stamina) as Characters.Data.StaminaData;

            float recoveryThreshold = staminaData != null ? staminaData.ExhaustionRecoveryThreshold : 25f;
            bool staminaAboveThreshold = (_stamina.StaminaPercentage * 100f) >= recoveryThreshold;

            return minDurationPassed && staminaAboveThreshold;
        }

        /// <summary>
        /// Gets the exhaustion timer value (for debugging/testing).
        /// </summary>
        public float ExhaustionTimer => _exhaustionTimer;
    }
}
