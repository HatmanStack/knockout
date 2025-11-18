using UnityEngine;

namespace Knockout.AI.States
{
    /// <summary>
    /// AI state for executing attacks.
    /// Chooses attack type based on distance and executes it.
    /// </summary>
    public class AttackState : AIState
    {
        // Attack type constants (matching CharacterCombat attack methods)
        public const int ATTACK_JAB = 0;
        public const int ATTACK_HOOK = 1;
        public const int ATTACK_UPPERCUT = 2;

        // Distance thresholds for attack selection
        private const float CLOSE_RANGE = 1.5f;
        private const float MEDIUM_RANGE = 2.5f;

        // Attack selection randomization
        private const float OPTIMAL_CHOICE_CHANCE = 0.7f; // 70% optimal, 30% random

        // Chosen attack for this state instance
        private int _chosenAttack = -1;
        private bool _attackExecuted = false;

        public override void Enter(AIContext context)
        {
            Debug.Log("[AI] Entering AttackState - Selecting attack");

            // Choose attack based on distance
            _chosenAttack = ChooseAttack(context);
            _attackExecuted = false;
        }

        public override AIState Update(AIContext context)
        {
            // Attack has been executed by CharacterAI component
            // This state will be active during attack animation
            // Transition back to Observe after attack completes
            // (CharacterAI will handle timing based on animation)

            // If we get hit during attack, defend
            if (context.PlayerIsAttacking && !_attackExecuted)
            {
                return new DefendState();
            }

            // Mark attack as executed after first frame
            if (!_attackExecuted)
            {
                _attackExecuted = true;
            }

            // Stay in attack state - CharacterAI will transition us out
            // after attack animation completes
            return null;
        }

        public override void Exit(AIContext context)
        {
            Debug.Log("[AI] Exiting AttackState - Attack complete");
        }

        public override bool CanTransitionTo(AIState newState)
        {
            // Can transition to Observe, Defend, or Retreat after attack
            return newState is ObserveState ||
                   newState is DefendState ||
                   newState is RetreatState;
        }

        /// <summary>
        /// Chooses the best attack type based on distance and randomization.
        /// </summary>
        /// <param name="context">Current AI context</param>
        /// <returns>Attack type constant (JAB, HOOK, or UPPERCUT)</returns>
        public int ChooseAttack(AIContext context)
        {
            float distance = context.DistanceToPlayer;
            int optimalAttack;

            // Determine optimal attack for distance
            if (distance < CLOSE_RANGE)
            {
                // Close range: Uppercut (high damage, slow)
                optimalAttack = ATTACK_UPPERCUT;
            }
            else if (distance < MEDIUM_RANGE)
            {
                // Medium range: Hook or Jab (randomized 50/50)
                optimalAttack = Random.value > 0.5f ? ATTACK_HOOK : ATTACK_JAB;
            }
            else
            {
                // Far range: Jab (fastest, longest reach)
                optimalAttack = ATTACK_JAB;
            }

            // Add randomization (70% optimal, 30% random)
            if (Random.value > OPTIMAL_CHOICE_CHANCE)
            {
                // Choose random attack instead
                return Random.Range(0, 3); // 0=Jab, 1=Hook, 2=Uppercut
            }

            return optimalAttack;
        }

        /// <summary>
        /// Gets the chosen attack type for this state instance.
        /// CharacterAI will call this to execute the attack.
        /// </summary>
        public int GetChosenAttack()
        {
            return _chosenAttack;
        }
    }
}
