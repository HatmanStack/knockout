namespace Knockout.AI.States
{
    /// <summary>
    /// Abstract base class for AI behavioral states.
    /// Implements the State pattern for AI decision-making.
    /// Unlike combat states, AI states return the next state to transition to.
    /// </summary>
    public abstract class AIState
    {
        /// <summary>
        /// Called when entering this state.
        /// </summary>
        /// <param name="context">Current AI context data</param>
        public abstract void Enter(AIContext context);

        /// <summary>
        /// Called each AI update tick while in this state.
        /// Returns the next state to transition to, or null to stay in current state.
        /// </summary>
        /// <param name="context">Current AI context data</param>
        /// <returns>Next state to transition to, or null to remain in current state</returns>
        public abstract AIState Update(AIContext context);

        /// <summary>
        /// Called when exiting this state.
        /// </summary>
        /// <param name="context">Current AI context data</param>
        public abstract void Exit(AIContext context);

        /// <summary>
        /// Checks if this state can transition to the specified new state.
        /// </summary>
        /// <param name="newState">The state to transition to</param>
        /// <returns>True if transition is valid, false otherwise</returns>
        public abstract bool CanTransitionTo(AIState newState);

        /// <summary>
        /// Gets the name of this state for debugging.
        /// </summary>
        public virtual string StateName => GetType().Name;
    }
}
