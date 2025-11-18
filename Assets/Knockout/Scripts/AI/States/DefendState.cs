using UnityEngine;

namespace Knockout.AI.States
{
    /// <summary>
    /// AI state for defensive blocking.
    /// Activates blocking and holds for a duration before transitioning.
    /// </summary>
    public class DefendState : AIState
    {
        // Block duration settings
        private const float MIN_BLOCK_DURATION = 0.5f;
        private const float MAX_BLOCK_DURATION = 1.5f;

        // Health threshold for defensive behavior
        private const float CRITICAL_HEALTH = 20f;

        private float _blockDuration;

        public override void Enter(AIContext context)
        {
            Debug.Log("[AI] Entering DefendState - Blocking");

            // Randomize block duration to be unpredictable
            _blockDuration = Random.Range(MIN_BLOCK_DURATION, MAX_BLOCK_DURATION);
        }

        public override AIState Update(AIContext context)
        {
            // Check if block duration has elapsed
            if (context.TimeSinceLastStateChange >= _blockDuration)
            {
                // If health is critical, retreat after blocking
                if (context.OwnHealthPercentage < CRITICAL_HEALTH)
                {
                    return new RetreatState();
                }

                // If player is vulnerable (not attacking), counter-attack
                if (!context.PlayerIsAttacking && context.DistanceToPlayer < 2.5f)
                {
                    return new AttackState();
                }

                // Otherwise return to observing
                return new ObserveState();
            }

            // Continue blocking
            return null;
        }

        public override void Exit(AIContext context)
        {
            Debug.Log("[AI] Exiting DefendState - Lowering guard");
        }

        public override bool CanTransitionTo(AIState newState)
        {
            // Can transition to any state except itself
            return newState != null && !(newState is DefendState);
        }
    }
}
