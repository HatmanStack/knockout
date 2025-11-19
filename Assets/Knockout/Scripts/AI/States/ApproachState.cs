using UnityEngine;

namespace Knockout.AI.States
{
    /// <summary>
    /// AI state for closing distance to the player.
    /// Moves toward player until reaching optimal attack range.
    /// </summary>
    public class ApproachState : AIState
    {
        // Distance thresholds
        private const float OPTIMAL_DISTANCE_MIN = 2.0f;
        private const float OPTIMAL_DISTANCE_MAX = 3.0f;
        private const float ATTACK_RANGE = 2.5f;

        // State duration randomization
        private const float MIN_APPROACH_DURATION = 0.3f;
        private const float MAX_APPROACH_DURATION = 1.5f;

        private float _approachDuration;

        public override void Enter(AIContext context)
        {
            _approachDuration = Random.Range(MIN_APPROACH_DURATION, MAX_APPROACH_DURATION);
            #if UNITY_EDITOR
            Debug.Log($"[AI] Entering ApproachState - Moving toward player for up to {_approachDuration:F2}s");
            #endif
        }

        public override AIState Update(AIContext context)
        {
            // Defend if player attacks during approach
            if (context.PlayerIsAttacking && context.DistanceToPlayer < 3.0f)
            {
                return new DefendState();
            }

            // Check if reached optimal range
            if (context.DistanceToPlayer >= OPTIMAL_DISTANCE_MIN &&
                context.DistanceToPlayer <= OPTIMAL_DISTANCE_MAX)
            {
                return new ObserveState();
            }

            // Close enough to attack
            if (context.DistanceToPlayer < ATTACK_RANGE)
            {
                return new AttackState();
            }

            // Continue approaching
            return null;
        }

        public override void Exit(AIContext context)
        {
            // Cleanup when leaving approach state
        }

        public override bool CanTransitionTo(AIState newState)
        {
            // Can transition to Observe, Attack, or Defend
            return newState is ObserveState ||
                   newState is AttackState ||
                   newState is DefendState;
        }
    }
}
