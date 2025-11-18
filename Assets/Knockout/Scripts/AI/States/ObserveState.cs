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

        // State duration randomization
        private const float MIN_OBSERVE_DURATION = 0.5f;
        private const float MAX_OBSERVE_DURATION = 2.0f;

        // Mistake probability
        private const float MISTAKE_CHANCE = 0.2f; // 20% chance to make suboptimal choice

        private float _observeDuration;

        public override void Enter(AIContext context)
        {
            // Initialize observe state with random duration
            _observeDuration = Random.Range(MIN_OBSERVE_DURATION, MAX_OBSERVE_DURATION);
            #if UNITY_EDITOR
            Debug.Log($"[AI] Entering ObserveState - Watching player for {_observeDuration:F2}s");
            #endif
        }

        public override AIState Update(AIContext context)
        {
            // Don't make decisions too quickly - enforce minimum observe duration
            if (context.TimeSinceLastStateChange < _observeDuration * 0.5f)
            {
                return null; // Stay in observe state
            }

            // Occasionally make a mistake (20% chance)
            if (Random.value < MISTAKE_CHANCE)
            {
                return MakeSuboptimalChoice(context);
            }

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
                !context.PlayerIsAttacking &&
                context.TimeSinceLastStateChange >= _observeDuration)
            {
                // Attack if in good range and observed long enough
                // 50% chance to attack when in range
                if (Random.value > 0.5f)
                {
                    return new AttackState();
                }
            }

            // Stay in observe state
            return null;
        }

        /// <summary>
        /// Makes a suboptimal choice to add unpredictability.
        /// </summary>
        private AIState MakeSuboptimalChoice(AIContext context)
        {
            // Make a random "mistake"
            float choice = Random.value;

            if (choice < 0.33f && context.OwnHealthPercentage > 50f)
            {
                // Attack when should be cautious
                #if UNITY_EDITOR
                Debug.Log("[AI] Making mistake: attacking when should be cautious");
                #endif
                return new AttackState();
            }
            else if (choice < 0.66f)
            {
                // Approach when should maintain distance
                #if UNITY_EDITOR
                Debug.Log("[AI] Making mistake: approaching unnecessarily");
                #endif
                return new ApproachState();
            }

            // Otherwise, stay in current state (mistake is inaction)
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
