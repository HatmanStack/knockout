using Knockout.Characters.Components;

namespace Knockout.Combat.States
{
    /// <summary>
    /// Abstract base class for combat states.
    /// Implements the State pattern for managing character combat behavior.
    /// </summary>
    public abstract class CombatState
    {
        /// <summary>
        /// Called when entering this state.
        /// </summary>
        /// <param name="combat">Reference to the CharacterCombat component</param>
        public abstract void Enter(CharacterCombat combat);

        /// <summary>
        /// Called every frame while in this state.
        /// </summary>
        /// <param name="combat">Reference to the CharacterCombat component</param>
        public abstract void Update(CharacterCombat combat);

        /// <summary>
        /// Called when exiting this state.
        /// </summary>
        /// <param name="combat">Reference to the CharacterCombat component</param>
        public abstract void Exit(CharacterCombat combat);

        /// <summary>
        /// Checks if this state can transition to the specified new state.
        /// </summary>
        /// <param name="newState">The state to transition to</param>
        /// <returns>True if transition is valid, false otherwise</returns>
        public abstract bool CanTransitionTo(CombatState newState);

        /// <summary>
        /// Gets the name of this state for debugging.
        /// </summary>
        public virtual string StateName => GetType().Name;
    }
}
