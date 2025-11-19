using Knockout.Characters.Components;

namespace Knockout.Combat.States
{
    /// <summary>
    /// Blocking combat state - character is defending and reducing incoming damage.
    /// Can transition to: Idle (release block), HitStunned (block broken), KnockedDown, KnockedOut.
    /// </summary>
    public class BlockingState : CombatState
    {
        public override void Enter(CharacterCombat combat)
        {
            // Set blocking animation via CharacterAnimator
            // Damage reduction is handled by CharacterHealth when it checks blocking state
        }

        public override void Update(CharacterCombat combat)
        {
            // Blocking state - waiting for player to release block or be hit
        }

        public override void Exit(CharacterCombat combat)
        {
            // Stop blocking animation
        }

        public override bool CanTransitionTo(CombatState newState)
        {
            // Cannot attack while blocking
            if (newState is AttackingState)
            {
                return false;
            }

            // Cannot transition to blocking from blocking
            if (newState is BlockingState)
            {
                return false;
            }

            // Valid transitions
            return newState is IdleState
                || newState is HitStunnedState
                || newState is KnockedDownState
                || newState is SpecialKnockdownState
                || newState is KnockedOutState;
        }
    }
}
