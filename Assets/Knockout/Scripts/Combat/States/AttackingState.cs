using Knockout.Characters.Components;

namespace Knockout.Combat.States
{
    /// <summary>
    /// Attacking combat state - character is executing an attack.
    /// Cannot be interrupted by other attacks or blocking.
    /// Can transition to: Idle (after completion), HitStunned, KnockedDown, KnockedOut.
    /// </summary>
    public class AttackingState : CombatState
    {
        public override void Enter(CharacterCombat combat)
        {
            // Attack animation is triggered by CharacterCombat before entering this state
            // State is exited by animation event (OnAttackEnd)
        }

        public override void Update(CharacterCombat combat)
        {
            // Attacking state - waiting for animation to complete
            // Animation events will trigger state transition back to idle
        }

        public override void Exit(CharacterCombat combat)
        {
            // Attack complete, clean up
        }

        public override bool CanTransitionTo(CombatState newState)
        {
            // Cannot transition to other attacks or blocking (committed to attack)
            if (newState is AttackingState || newState is BlockingState)
            {
                return false;
            }

            // Valid transitions
            return newState is IdleState
                || newState is ExhaustedState
                || newState is HitStunnedState
                || newState is KnockedDownState
                || newState is SpecialKnockdownState
                || newState is KnockedOutState;
        }
    }
}
