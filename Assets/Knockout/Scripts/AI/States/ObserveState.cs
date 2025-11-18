using UnityEngine;

namespace Knockout.AI.States
{
    /// <summary>
    /// Default AI state - maintains optimal distance and observes player.
    /// Decides whether to approach, retreat, attack, or defend based on context.
    /// </summary>
    public class ObserveState : AIState
    {
        // Distance thresholds
        private const float OPTIMAL_DISTANCE_MIN = 2.0f;
        private const float OPTIMAL_DISTANCE_MAX = 3.5f;
        private const float TOO_FAR_THRESHOLD = 4.0f;
        private const float TOO_CLOSE_THRESHOLD = 1.5f;

        public override void Enter(AIContext context)
        {
            // Initialize observe state
            Debug.Log("[AI] Entering ObserveState - Watching player");
        }

        public override AIState Update(AIContext context)
        {
            // Highest priority: Defend if player is attacking and close
            if (context.PlayerIsAttacking && context.DistanceToPlayer < 3.0f)
            {
                return new DefendState();
            }

            // Check if should retreat (too close or low health)
            if (context.DistanceToPlayer < TOO_CLOSE_THRESHOLD)
            {
                return new RetreatState();
            }

            // Retreat if health is critical
            if (context.OwnHealthPercentage < 30f)
            {
                return new RetreatState();
            }

            // Check if should approach (too far)
            if (context.DistanceToPlayer > TOO_FAR_THRESHOLD)
            {
                return new ApproachState();
            }

            // In optimal range - consider attacking
            if (context.DistanceToPlayer >= OPTIMAL_DISTANCE_MIN &&
                context.DistanceToPlayer <= OPTIMAL_DISTANCE_MAX &&
                !context.PlayerIsAttacking)
            {
                // Attack if in good range and player not blocking
                // 50% chance to attack when in range
                if (Random.value > 0.5f)
                {
                    return new AttackState();
                }
            }

            // Stay in observe state
            return null;
        }

        public override void Exit(AIContext context)
        {
            // Cleanup when leaving observe state
        }

        public override bool CanTransitionTo(AIState newState)
        {
            // Can transition to any state from observe
            return newState != null && !(newState is ObserveState);
        }
    }
}
