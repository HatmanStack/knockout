using System;
using UnityEngine;
using Knockout.Characters.Components;

namespace Knockout.Combat.States
{
    /// <summary>
    /// Parry stagger combat state - attacker is vulnerable after being parried.
    /// Cannot perform any actions during stagger duration.
    /// Defender has counter window to attack.
    /// Can transition to: Idle (on stagger end), HitStunned, KnockedDown, KnockedOut.
    /// </summary>
    public class ParryStaggerState : CombatState
    {
        private float _staggerTimer;
        private float _staggerDuration;

        /// <summary>
        /// Event fired when parry stagger begins.
        /// </summary>
        public static event Action<CharacterCombat> OnParryStaggered;

        /// <summary>
        /// Event fired when parry stagger ends.
        /// </summary>
        public static event Action<CharacterCombat> OnParryStaggerEnded;

        public override void Enter(CharacterCombat combat)
        {
            // Default stagger duration if not set
            _staggerDuration = 0.5f;
            _staggerTimer = 0f;

            // Trigger stagger animation (reuse hit reaction or create specific)
            CharacterAnimator animator = combat.GetComponent<CharacterAnimator>();
            if (animator != null)
            {
                // Use light hit reaction for stagger animation
                // Could be replaced with dedicated stagger animation later
                animator.TriggerHitReaction(0);
            }

            // Fire event
            OnParryStaggered?.Invoke(combat);
        }

        /// <summary>
        /// Enters the parry stagger state with a specific duration.
        /// </summary>
        /// <param name="combat">Reference to CharacterCombat</param>
        /// <param name="duration">Stagger duration in seconds</param>
        public void Enter(CharacterCombat combat, float duration)
        {
            _staggerDuration = Mathf.Max(0.1f, duration);
            _staggerTimer = 0f;

            // Trigger stagger animation
            CharacterAnimator animator = combat.GetComponent<CharacterAnimator>();
            if (animator != null)
            {
                // Use light hit reaction for stagger animation
                animator.TriggerHitReaction(0);
            }

            // Fire event
            OnParryStaggered?.Invoke(combat);
        }

        public override void Update(CharacterCombat combat)
        {
            // Track stagger duration
            _staggerTimer += Time.deltaTime;

            // Stagger complete - transition handled by auto-transition in CharacterCombat
        }

        public override void Exit(CharacterCombat combat)
        {
            // Fire event
            OnParryStaggerEnded?.Invoke(combat);
        }

        public override bool CanTransitionTo(CombatState newState)
        {
            // Cannot perform any voluntary actions while staggered
            if (newState is AttackingState)
            {
                return false;
            }

            if (newState is BlockingState)
            {
                return false;
            }

            if (newState is DodgingState)
            {
                return false;
            }

            // Cannot stay in stagger
            if (newState is ParryStaggerState)
            {
                return false;
            }

            // Can transition to idle when stagger completes
            if (newState is IdleState)
            {
                return IsStaggerComplete();
            }

            // Can be hit during stagger (interrupts stagger)
            return newState is HitStunnedState
                || newState is KnockedDownState
                || newState is SpecialKnockdownState
                || newState is KnockedOutState;
        }

        /// <summary>
        /// Checks if stagger duration has completed.
        /// </summary>
        public bool IsStaggerComplete()
        {
            return _staggerTimer >= _staggerDuration;
        }

        /// <summary>
        /// Gets the current stagger timer value (for debugging/testing).
        /// </summary>
        public float StaggerTimer => _staggerTimer;

        /// <summary>
        /// Gets the stagger duration.
        /// </summary>
        public float StaggerDuration => _staggerDuration;
    }
}
