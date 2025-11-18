using UnityEngine;

namespace Knockout.AI.States
{
    /// <summary>
    /// AI state for backing away from the player.
    /// Used when too close to player or when health is low.
    /// </summary>
    public class RetreatState : AIState
    {
        // Distance thresholds
        private const float SAFE_DISTANCE = 3.5f;
        private const float CRITICAL_HEALTH = 20f;

        public override void Enter(AIContext context)
        {
            Debug.Log("[AI] Entering RetreatState - Backing away");
        }

        public override AIState Update(AIContext context)
        {
            // Defend if player pursues and attacks
            if (context.PlayerIsAttacking && context.DistanceToPlayer < 2.5f)
            {
                return new DefendState();
            }

            // Check if reached safe distance
            if (context.DistanceToPlayer > SAFE_DISTANCE)
            {
                // If health still critical, stay defensive
                if (context.OwnHealthPercentage < CRITICAL_HEALTH)
                {
                    return new DefendState();
                }

                return new ObserveState();
            }

            // Continue retreating
            return null;
        }

        public override void Exit(AIContext context)
        {
            // Cleanup when leaving retreat state
        }

        public override bool CanTransitionTo(AIState newState)
        {
            // Can transition to Observe or Defend
            return newState is ObserveState ||
                   newState is DefendState;
        }
    }
}
