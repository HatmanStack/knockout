using UnityEngine;
using Knockout.AI.States;

namespace Knockout.AI
{
    /// <summary>
    /// Manages AI state transitions and delegates updates to the current state.
    /// Unlike combat state machine, AI states determine their own transitions.
    /// </summary>
    public class AIStateMachine
    {
        private AIState _currentState;
        private AIContext _context;

        /// <summary>
        /// Event fired when state changes.
        /// </summary>
        public event System.Action<AIState, AIState> OnStateChanged;

        /// <summary>
        /// Gets the current AI state.
        /// </summary>
        public AIState CurrentState => _currentState;

        /// <summary>
        /// Gets the current context.
        /// </summary>
        public AIContext Context => _context;

        /// <summary>
        /// Initializes the state machine with a starting state.
        /// </summary>
        /// <param name="initialState">The initial state to start in</param>
        public void Initialize(AIState initialState)
        {
            _currentState = initialState;
            _context = new AIContext();
            _currentState?.Enter(_context);
        }

        /// <summary>
        /// Updates the context and current state.
        /// State determines if a transition should occur.
        /// </summary>
        /// <param name="context">Updated context with current game state</param>
        public void Update(AIContext context)
        {
            _context = context;

            if (_currentState == null)
            {
                Debug.LogWarning("[AIStateMachine] No current state!");
                return;
            }

            // Update current state and get potential next state
            AIState nextState = _currentState.Update(_context);

            // If state wants to transition, attempt it
            if (nextState != null && nextState != _currentState)
            {
                ChangeState(nextState);
            }
        }

        /// <summary>
        /// Attempts to change to a new state.
        /// Validates the transition before changing.
        /// </summary>
        /// <param name="newState">The state to transition to</param>
        /// <returns>True if transition succeeded, false if invalid</returns>
        public bool ChangeState(AIState newState)
        {
            if (newState == null)
            {
                Debug.LogWarning("[AIStateMachine] Attempted to change to null state!");
                return false;
            }

            // Check if transition is valid
            if (_currentState != null && !_currentState.CanTransitionTo(newState))
            {
                Debug.LogWarning($"[AIStateMachine] Invalid state transition: {_currentState.StateName} -> {newState.StateName}");
                return false;
            }

            // Perform transition
            AIState oldState = _currentState;

            _currentState?.Exit(_context);
            _currentState = newState;
            _currentState.Enter(_context);

            // Reset state change timer
            _context.TimeSinceLastStateChange = 0f;

            // Fire event
            OnStateChanged?.Invoke(oldState, newState);

            return true;
        }

        /// <summary>
        /// Updates the time since last state change.
        /// Should be called with deltaTime each frame.
        /// </summary>
        public void UpdateStateTime(float deltaTime)
        {
            var ctx = _context;
            ctx.TimeSinceLastStateChange += deltaTime;
            _context = ctx;
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
