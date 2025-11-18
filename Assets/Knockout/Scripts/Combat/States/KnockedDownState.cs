using Knockout.Characters.Components;

namespace Knockout.Combat.States
{
    /// <summary>
    /// Knocked down combat state - character is on the ground and getting up.
    /// Can transition to: Idle (after get-up animation), KnockedOut.
    /// </summary>
    public class KnockedDownState : CombatState
    {
        public override void Enter(CharacterCombat combat)
        {
            // Knockdown animation triggered by CharacterHealth
            // Followed by get-up animation
            // State is exited by animation event (OnGetUpComplete)
        }

        public override void Update(CharacterCombat combat)
        {
            // Knocked down - waiting for get-up animation to complete
        }

        public override void Exit(CharacterCombat combat)
        {
            // Back on feet, can act again
        }

        public override bool CanTransitionTo(CombatState newState)
        {
            // Cannot act while knocked down (must get up first)
            if (newState is AttackingState
                || newState is BlockingState
                || newState is HitStunnedState
                || newState is KnockedDownState)
            {
                return false;
            }

            // Valid transitions
            return newState is IdleState
                || newState is KnockedOutState;
        }
    }
}
