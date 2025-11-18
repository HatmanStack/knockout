using UnityEngine;
using Knockout.Characters.Components;
using Knockout.Combat.States;

namespace Knockout.Combat
{
    /// <summary>
    /// Manages combat state transitions and delegates updates to the current state.
    /// Enforces valid state transitions and provides state change events.
    /// </summary>
    public class CombatStateMachine
    {
        private CombatState _currentState;
        private CharacterCombat _combat;

        /// <summary>
        /// Event fired when state changes.
        /// </summary>
        public event System.Action<CombatState, CombatState> OnStateChanged;

        /// <summary>
        /// Gets the current combat state.
        /// </summary>
        public CombatState CurrentState => _currentState;

        /// <summary>
        /// Initializes the state machine with a starting state.
        /// </summary>
        /// <param name="combat">Reference to CharacterCombat component</param>
        /// <param name="initialState">The initial state to start in</param>
        public void Initialize(CharacterCombat combat, CombatState initialState)
        {
            _combat = combat;
            _currentState = initialState;
            _currentState?.Enter(_combat);
        }

        /// <summary>
        /// Updates the current state.
        /// Call this from CharacterCombat.Update().
        /// </summary>
        public void Update()
        {
            _currentState?.Update(_combat);
        }

        /// <summary>
        /// Attempts to change to a new state.
        /// Validates the transition before changing.
        /// </summary>
        /// <param name="newState">The state to transition to</param>
        /// <returns>True if transition succeeded, false if invalid</returns>
        public bool ChangeState(CombatState newState)
        {
            if (newState == null)
            {
                Debug.LogWarning("[CombatStateMachine] Attempted to change to null state!");
                return false;
            }

            // Check if transition is valid
            if (_currentState != null && !_currentState.CanTransitionTo(newState))
            {
                Debug.LogWarning($"[CombatStateMachine] Invalid state transition: {_currentState.StateName} -> {newState.StateName}");
                return false;
            }

            // Perform transition
            CombatState oldState = _currentState;

            _currentState?.Exit(_combat);
            _currentState = newState;
            _currentState.Enter(_combat);

            // Fire event
            OnStateChanged?.Invoke(oldState, newState);

            return true;
        }

        /// <summary>
        /// Checks if a transition to the specified state is valid.
        /// </summary>
        public bool CanTransitionTo(CombatState newState)
        {
            if (newState == null) return false;
            if (_currentState == null) return true;
            return _currentState.CanTransitionTo(newState);
        }

        /// <summary>
        /// Gets the current state name for debugging.
        /// </summary>
        public string GetCurrentStateName()
        {
            return _currentState?.StateName ?? "None";
        }
    }
}
