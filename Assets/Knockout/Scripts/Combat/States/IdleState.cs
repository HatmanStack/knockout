using Knockout.Characters.Components;

namespace Knockout.Combat.States
{
    /// <summary>
    /// Idle combat state - character is ready to act.
    /// Can transition to: Attacking, Blocking, HitStunned, KnockedDown, KnockedOut.
    /// </summary>
    public class IdleState : CombatState
    {
        public override void Enter(CharacterCombat combat)
        {
            // Character is idle and ready to act
            // No specific animation needed (handled by locomotion layer)
        }

        public override void Update(CharacterCombat combat)
        {
            // Idle state - waiting for input or events
            // Input handling is done by CharacterInput component
        }

        public override void Exit(CharacterCombat combat)
        {
            // Clean up when leaving idle state
        }

        public override bool CanTransitionTo(CombatState newState)
        {
            // Can transition to any state except staying in idle
            if (newState is IdleState)
            {
                return false; // Already idle
            }

            // Valid transitions
            return newState is AttackingState
                || newState is BlockingState
                || newState is DodgingState
                || newState is ExhaustedState
                || newState is HitStunnedState
                || newState is KnockedDownState
                || newState is SpecialKnockdownState
                || newState is KnockedOutState;
        }
    }
}
