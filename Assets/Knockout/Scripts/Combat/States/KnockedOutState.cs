using Knockout.Characters.Components;

namespace Knockout.Combat.States
{
    /// <summary>
    /// Knocked out combat state - character is defeated and cannot act.
    /// This is a terminal state with no valid transitions.
    /// </summary>
    public class KnockedOutState : CombatState
    {
        public override void Enter(CharacterCombat combat)
        {
            // Knockout animation triggered by CharacterHealth
            // Character is disabled and match ends
        }

        public override void Update(CharacterCombat combat)
        {
            // Knocked out - terminal state, no actions possible
        }

        public override void Exit(CharacterCombat combat)
        {
            // This state should never be exited during normal gameplay
            // Only exit if match is reset/restarted
        }

        public override bool CanTransitionTo(CombatState newState)
        {
            // Terminal state - no valid transitions
            return false;
        }
    }
}
