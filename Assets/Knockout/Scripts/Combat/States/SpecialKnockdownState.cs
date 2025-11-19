using System;
using UnityEngine;
using Knockout.Characters.Components;

namespace Knockout.Combat.States
{
    /// <summary>
    /// Special knockdown combat state - enhanced knockdown from special moves.
    /// Similar to KnockedDownState but with longer recovery time and enhanced presentation.
    /// Can transition to: Idle (after recovery), KnockedOut.
    /// </summary>
    public class SpecialKnockdownState : CombatState
    {
        private float _recoveryTimer;
        private float _recoveryDuration;
        private bool _canGetUp;

        /// <summary>
        /// Event fired when special knockdown begins.
        /// </summary>
        public static event Action<CharacterCombat> OnSpecialKnockdownStart;

        /// <summary>
        /// Event fired when special knockdown ends (recovery complete).
        /// </summary>
        public static event Action<CharacterCombat> OnSpecialKnockdownEnd;

        /// <summary>
        /// Default recovery duration for special knockdowns (longer than normal knockdown).
        /// </summary>
        public const float DEFAULT_RECOVERY_DURATION = 4f; // Normal knockdown ~2-3s

        public override void Enter(CharacterCombat combat)
        {
            // Initialize recovery timer
            _recoveryTimer = 0f;
            _recoveryDuration = DEFAULT_RECOVERY_DURATION;
            _canGetUp = false;

            // Trigger special knockdown animation
            // Reuse knockout animation with longer duration
            CharacterAnimator animator = combat.GetComponent<CharacterAnimator>();
            if (animator != null)
            {
                animator.TriggerKnockdown();
                // Note: In future, could add animator.TriggerSpecialKnockdown() for unique animation
            }

            // Fire event for VFX/audio integration
            OnSpecialKnockdownStart?.Invoke(combat);
        }

        public override void Update(CharacterCombat combat)
        {
            // Track recovery time
            _recoveryTimer += Time.deltaTime;

            // Check if recovery duration elapsed
            if (_recoveryTimer >= _recoveryDuration && !_canGetUp)
            {
                _canGetUp = true;

                // Trigger get-up animation
                CharacterAnimator animator = combat.GetComponent<CharacterAnimator>();
                if (animator != null)
                {
                    // Get-up animation will fire OnGetUpComplete event
                    // which will trigger transition to IdleState
                    // Note: If no animation event, we could auto-transition here
                }
            }
        }

        public override void Exit(CharacterCombat combat)
        {
            // Recovery complete
            OnSpecialKnockdownEnd?.Invoke(combat);
        }

        public override bool CanTransitionTo(CombatState newState)
        {
            // Cannot act while knocked down (must recover first)
            if (newState is AttackingState
                || newState is BlockingState
                || newState is HitStunnedState
                || newState is ExhaustedState
                || newState is KnockedDownState
                || newState is SpecialKnockdownState)
            {
                return false;
            }

            // Can transition to idle after recovery
            if (newState is IdleState)
            {
                return _canGetUp;
            }

            // Valid transitions
            return newState is KnockedOutState;
        }

        /// <summary>
        /// Gets whether the character can get up from special knockdown.
        /// </summary>
        public bool CanGetUp => _canGetUp;

        /// <summary>
        /// Gets the recovery timer value (for debugging/testing).
        /// </summary>
        public float RecoveryTimer => _recoveryTimer;

        /// <summary>
        /// Sets a custom recovery duration for this special knockdown.
        /// Must be called before or immediately after Enter().
        /// </summary>
        /// <param name="duration">Custom recovery duration in seconds</param>
        public void SetRecoveryDuration(float duration)
        {
            _recoveryDuration = Mathf.Max(0f, duration);
        }
    }
}
