using Knockout.Characters.Components;

namespace Knockout.Combat.States
{
    /// <summary>
    /// Hit stunned combat state - character has been hit and is temporarily disabled.
    /// Can transition to: Idle (after recovery), KnockedDown, KnockedOut.
    /// </summary>
    public class HitStunnedState : CombatState
    {
        public override void Enter(CharacterCombat combat)
        {
            // Hit reaction animation triggered by CharacterHealth
            // State is exited by animation event (OnHitReactionEnd)
        }

        public override void Update(CharacterCombat combat)
        {
            // Hit stunned - waiting for animation to complete
        }

        public override void Exit(CharacterCombat combat)
        {
            // Stun complete, can act again
        }

        public override bool CanTransitionTo(CombatState newState)
        {
            // Cannot act while stunned
            if (newState is AttackingState || newState is BlockingState)
            {
                return false;
            }

            // Cannot transition to stunned from stunned
            if (newState is HitStunnedState)
            {
                return false;
            }

            // Valid transitions
            return newState is IdleState
                || newState is KnockedDownState
                || newState is KnockedOutState;
        }
    }
}
