using UnityEngine;
using Knockout.AI;
using Knockout.AI.States;

namespace Knockout.Characters.Components
{
    /// <summary>
    /// AI component that drives AI-controlled characters using state machine.
    /// Replaces CharacterInput for AI opponents.
    /// </summary>
    [RequireComponent(typeof(CharacterCombat))]
    [RequireComponent(typeof(CharacterMovement))]
    [RequireComponent(typeof(CharacterHealth))]
    public class CharacterAI : MonoBehaviour
    {
        [Header("AI Target")]
        [SerializeField]
        [Tooltip("Player character to target. Leave null for automatic detection.")]
        private GameObject targetPlayer;

        [Header("AI Settings")]
        [SerializeField]
        [Tooltip("How often AI makes decisions (seconds)")]
        [Range(0.05f, 0.5f)]
        private float reactionTime = 0.1f;

        [SerializeField]
        [Tooltip("Chance to choose optimal attack (0-1)")]
        [Range(0.0f, 1.0f)]
        private float attackAccuracy = 0.7f;

        [SerializeField]
        [Tooltip("Aggression level affects distance thresholds")]
        [Range(0.0f, 1.0f)]
        private float aggressionLevel = 0.5f;

        // Component references
        private CharacterCombat _characterCombat;
        private CharacterMovement _characterMovement;
        private CharacterHealth _characterHealth;

        // AI state machine
        private AIStateMachine _stateMachine;

        // Decision-making timer
        private float _decisionTimer;

        // Current AI context
        private AIContext _currentContext;

        #region Public Properties

        /// <summary>
        /// Gets the current player target.
        /// </summary>
        public GameObject Target => targetPlayer;

        /// <summary>
        /// Gets the AI state machine.
        /// </summary>
        public AIStateMachine StateMachine => _stateMachine;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Cache component references
            _characterCombat = GetComponent<CharacterCombat>();
            _characterMovement = GetComponent<CharacterMovement>();
            _characterHealth = GetComponent<CharacterHealth>();
        }

        private void Start()
        {
            // Find player target if not assigned
            if (targetPlayer == null)
            {
                targetPlayer = AITargetDetector.FindPlayerCharacter();

                if (targetPlayer == null)
                {
                    Debug.LogError($"[{gameObject.name}] CharacterAI could not find player target!", this);
                    enabled = false;
                    return;
                }
            }

            // Initialize state machine
            _stateMachine = new AIStateMachine();
            _stateMachine.Initialize(new ObserveState());

            // Initialize decision timer
            _decisionTimer = 0f;

            Debug.Log($"[{gameObject.name}] CharacterAI initialized, targeting {targetPlayer.name}");
        }

        private void Update()
        {
            if (targetPlayer == null || _stateMachine == null)
                return;

            // Update state time
            _stateMachine.UpdateStateTime(Time.deltaTime);

            // Update decision timer
            _decisionTimer += Time.deltaTime;

            // Make decisions at configured interval (default 10Hz)
            if (_decisionTimer >= reactionTime)
            {
                _decisionTimer = 0f;
                UpdateAI();
            }

            // Execute actions based on current state (every frame)
            ExecuteCurrentStateActions();
        }

        #endregion

        #region AI Update

        /// <summary>
        /// Updates AI context and state machine.
        /// </summary>
        private void UpdateAI()
        {
            // Update context with current game state
            _currentContext = BuildContext();

            // Update state machine (may cause state transitions)
            _stateMachine.Update(_currentContext);
        }

        /// <summary>
        /// Builds AI context from current game state.
        /// </summary>
        private AIContext BuildContext()
        {
            var context = new AIContext();

            // Get positions
            Vector3 aiPosition = transform.position;
            Vector3 playerPosition = targetPlayer.transform.position;

            // Get health values
            float ownHealth = _characterHealth.CurrentHealthPercentage;
            float playerHealth = 100f; // Default if no health component

            var playerHealthComponent = targetPlayer.GetComponent<CharacterHealth>();
            if (playerHealthComponent != null)
            {
                playerHealth = playerHealthComponent.CurrentHealthPercentage;
            }

            // Check if player is attacking
            bool playerAttacking = false;
            var playerCombat = targetPlayer.GetComponent<CharacterCombat>();
            if (playerCombat != null)
            {
                playerAttacking = playerCombat.CurrentState is Knockout.Combat.States.AttackingState;
            }

            // Update context
            context.UpdateFrom(aiPosition, playerPosition, ownHealth, playerHealth, playerAttacking);
            context.PlayerTarget = targetPlayer;
            context.TimeSinceLastStateChange = _stateMachine.Context.TimeSinceLastStateChange;

            return context;
        }

        /// <summary>
        /// Executes actions based on current AI state.
        /// </summary>
        private void ExecuteCurrentStateActions()
        {
            if (_stateMachine == null || _stateMachine.CurrentState == null)
                return;

            var currentState = _stateMachine.CurrentState;

            // Execute actions based on state type
            if (currentState is ObserveState)
            {
                ExecuteObserveActions();
            }
            else if (currentState is ApproachState)
            {
                ExecuteApproachActions();
            }
            else if (currentState is RetreatState)
            {
                ExecuteRetreatActions();
            }
            else if (currentState is AttackState attackState)
            {
                ExecuteAttackActions(attackState);
            }
            else if (currentState is DefendState)
            {
                ExecuteDefendActions();
            }
        }

        #endregion

        #region State Action Execution

        /// <summary>
        /// Executes actions for ObserveState (maintain distance, strafe).
        /// </summary>
        private void ExecuteObserveActions()
        {
            // Always face player
            RotateTowardPlayer();

            // Maintain optimal distance by moving or strafing
            float distance = Vector3.Distance(transform.position, targetPlayer.transform.position);

            if (distance > 3.5f)
            {
                // Too far, move closer
                MoveTowardPlayer();
            }
            else if (distance < 2.5f)
            {
                // Too close, back away
                MoveAwayFromPlayer();
            }
            else
            {
                // Optimal range, strafe randomly
                StrafeRandomly();
            }
        }

        /// <summary>
        /// Executes actions for ApproachState (move toward player).
        /// </summary>
        private void ExecuteApproachActions()
        {
            RotateTowardPlayer();
            MoveTowardPlayer();
        }

        /// <summary>
        /// Executes actions for RetreatState (move away from player).
        /// </summary>
        private void ExecuteRetreatActions()
        {
            RotateTowardPlayer();
            MoveAwayFromPlayer();
        }

        /// <summary>
        /// Executes actions for AttackState (execute chosen attack).
        /// </summary>
        private void ExecuteAttackActions(AttackState attackState)
        {
            // Stop moving during attack
            _characterMovement.SetMovementInput(Vector2.zero);

            // Face player
            RotateTowardPlayer();

            // Execute attack once when entering state
            // Check if attack hasn't been executed yet
            int chosenAttack = attackState.GetChosenAttack();

            if (chosenAttack >= 0)
            {
                bool attackExecuted = false;

                switch (chosenAttack)
                {
                    case AttackState.ATTACK_JAB:
                        attackExecuted = _characterCombat.ExecuteJab();
                        break;
                    case AttackState.ATTACK_HOOK:
                        attackExecuted = _characterCombat.ExecuteHook();
                        break;
                    case AttackState.ATTACK_UPPERCUT:
                        attackExecuted = _characterCombat.ExecuteUppercut();
                        break;
                }

                if (attackExecuted)
                {
                    Debug.Log($"[AI] Executed attack type {chosenAttack}");
                }
            }

            // Check if attack animation is complete, transition to ObserveState
            // This is handled by the state machine, but we could add additional logic here
        }

        /// <summary>
        /// Executes actions for DefendState (activate blocking).
        /// </summary>
        private void ExecuteDefendActions()
        {
            // Stop moving while blocking
            _characterMovement.SetMovementInput(Vector2.zero);

            // Face player
            RotateTowardPlayer();

            // Start blocking
            if (!_characterCombat.IsBlocking)
            {
                _characterCombat.StartBlocking();
            }
        }

        #endregion

        #region Movement Helpers

        /// <summary>
        /// Moves AI toward the player.
        /// </summary>
        private void MoveTowardPlayer()
        {
            if (targetPlayer == null) return;

            Vector3 direction = (targetPlayer.transform.position - transform.position).normalized;
            Vector2 input = new Vector2(direction.x, direction.z);

            _characterMovement.SetMovementInput(input);
        }

        /// <summary>
        /// Moves AI away from the player.
        /// </summary>
        private void MoveAwayFromPlayer()
        {
            if (targetPlayer == null) return;

            Vector3 direction = (transform.position - targetPlayer.transform.position).normalized;
            Vector2 input = new Vector2(direction.x, direction.z);

            _characterMovement.SetMovementInput(input);
        }

        /// <summary>
        /// Rotates AI to face the player.
        /// </summary>
        private void RotateTowardPlayer()
        {
            if (targetPlayer == null) return;

            _characterMovement.RotateToward(targetPlayer.transform.position);
        }

        /// <summary>
        /// Makes AI strafe randomly (circling behavior).
        /// </summary>
        private void StrafeRandomly()
        {
            // Pick random strafe direction
            float strafeDirection = Random.value > 0.5f ? 1f : -1f;

            // Small forward/backward component for natural movement
            float forwardComponent = Random.Range(-0.3f, 0.3f);

            Vector2 input = new Vector2(strafeDirection, forwardComponent);
            _characterMovement.SetMovementInput(input);
        }

        #endregion
    }
}
