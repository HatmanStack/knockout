using UnityEngine;
using Knockout.Characters.Components;
using Knockout.Combat.States;

namespace Knockout.AI
{
    /// <summary>
    /// Dummy AI for training mode with controllable behaviors.
    /// Provides predictable, reactive behaviors for practice: Passive, Blocking, Dodging, Counter.
    /// </summary>
    public class DummyAI : MonoBehaviour
    {
        [Header("Character References")]
        [SerializeField] [Tooltip("Character combat component")]
        private CharacterCombat characterCombat;

        [SerializeField] [Tooltip("Character input component (for simulating inputs)")]
        private CharacterInput characterInput;

        [SerializeField] [Tooltip("Combat state machine")]
        private CombatStateMachine combatStateMachine;

        [SerializeField] [Tooltip("Character dodge component")]
        private CharacterDodge characterDodge;

        [Header("Behavior Settings")]
        [SerializeField] [Tooltip("Current dummy behavior")]
        private DummyBehavior currentBehavior = DummyBehavior.Passive;

        [SerializeField] [Tooltip("Reaction time delay (seconds) for blocking/dodging")]
        [Range(0f, 0.5f)]
        private float reactionTime = 0.1f;

        [SerializeField] [Tooltip("Counter attack delay after successful block/parry (seconds)")]
        [Range(0.1f, 1f)]
        private float counterAttackDelay = 0.3f;

        // Internal state
        private CharacterHealth _opponentHealth;
        private float _lastReactionTime;
        private bool _isCountering;

        /// <summary>
        /// Dummy behavior enum.
        /// </summary>
        public enum DummyBehavior
        {
            Passive,   // Stand idle, doesn't attack or defend
            Blocking,  // Always blocks incoming attacks
            Dodging,   // Attempts to dodge incoming attacks
            Counter    // Blocks then counter-attacks
        }

        private void Awake()
        {
            ValidateReferences();
        }

        private void Start()
        {
            FindOpponent();

            if (_opponentHealth != null)
            {
                // Subscribe to opponent's attack events
                var opponentCombat = _opponentHealth.GetComponent<CharacterCombat>();
                if (opponentCombat != null)
                {
                    opponentCombat.OnAttackStarted += OnOpponentAttackStarted;
                }
            }

            // Disable aggressive AI if present
            var standardAI = GetComponent<CharacterAI>();
            if (standardAI != null)
            {
                standardAI.enabled = false;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (_opponentHealth != null)
            {
                var opponentCombat = _opponentHealth.GetComponent<CharacterCombat>();
                if (opponentCombat != null)
                {
                    opponentCombat.OnAttackStarted -= OnOpponentAttackStarted;
                }
            }
        }

        private void Update()
        {
            // Execute behavior
            switch (currentBehavior)
            {
                case DummyBehavior.Passive:
                    ExecutePassiveBehavior();
                    break;

                case DummyBehavior.Blocking:
                    ExecuteBlockingBehavior();
                    break;

                case DummyBehavior.Dodging:
                    ExecuteDodgingBehavior();
                    break;

                case DummyBehavior.Counter:
                    ExecuteCounterBehavior();
                    break;
            }
        }

        private void ValidateReferences()
        {
            if (characterCombat == null)
            {
                characterCombat = GetComponent<CharacterCombat>();
            }

            if (characterInput == null)
            {
                characterInput = GetComponent<CharacterInput>();
            }

            if (combatStateMachine == null)
            {
                combatStateMachine = GetComponent<CombatStateMachine>();
            }

            if (characterDodge == null)
            {
                characterDodge = GetComponent<CharacterDodge>();
            }
        }

        private void FindOpponent()
        {
            // Find player character (assumes training mode has player vs dummy)
            CharacterHealth[] allHealthComponents = FindObjectsOfType<CharacterHealth>();
            foreach (var health in allHealthComponents)
            {
                if (health.gameObject != gameObject)
                {
                    _opponentHealth = health;
                    break;
                }
            }
        }

        /// <summary>
        /// Passive behavior: Stand idle, no actions.
        /// </summary>
        private void ExecutePassiveBehavior()
        {
            // Do nothing - just stand there
            // Ensure not blocking
            if (characterInput != null && combatStateMachine != null)
            {
                if (combatStateMachine.CurrentState is BlockingState)
                {
                    characterInput.ReleaseBlock();
                }
            }
        }

        /// <summary>
        /// Blocking behavior: Always blocks.
        /// </summary>
        private void ExecuteBlockingBehavior()
        {
            if (characterInput == null || combatStateMachine == null)
            {
                return;
            }

            // Always hold block
            if (!(combatStateMachine.CurrentState is BlockingState))
            {
                characterInput.PressBlock();
            }
        }

        /// <summary>
        /// Dodging behavior: Attempts to dodge incoming attacks.
        /// </summary>
        private void ExecuteDodgingBehavior()
        {
            // Dodging is reactive - handled in OnOpponentAttackStarted
            // Here we just ensure not blocking
            if (characterInput != null && combatStateMachine != null)
            {
                if (combatStateMachine.CurrentState is BlockingState)
                {
                    characterInput.ReleaseBlock();
                }
            }
        }

        /// <summary>
        /// Counter behavior: Blocks then counter-attacks.
        /// </summary>
        private void ExecuteCounterBehavior()
        {
            if (characterInput == null || combatStateMachine == null)
            {
                return;
            }

            // Block when not countering
            if (!_isCountering && !(combatStateMachine.CurrentState is BlockingState))
            {
                characterInput.PressBlock();
            }
        }

        /// <summary>
        /// Called when opponent starts an attack.
        /// </summary>
        private void OnOpponentAttackStarted(AttackData attackData)
        {
            if (Time.time - _lastReactionTime < reactionTime)
            {
                return; // Still in reaction delay
            }

            _lastReactionTime = Time.time;

            switch (currentBehavior)
            {
                case DummyBehavior.Blocking:
                    // Already blocking continuously
                    break;

                case DummyBehavior.Dodging:
                    AttemptDodge();
                    break;

                case DummyBehavior.Counter:
                    // Already blocking, trigger counter attack after delay
                    if (!_isCountering)
                    {
                        StartCoroutine(CounterAttackRoutine());
                    }
                    break;
            }
        }

        /// <summary>
        /// Attempts to dodge in a random direction.
        /// </summary>
        private void AttemptDodge()
        {
            if (characterDodge == null || characterInput == null)
            {
                return;
            }

            if (!characterDodge.CanDodge)
            {
                return;
            }

            // Random dodge direction
            int direction = Random.Range(0, 3);
            switch (direction)
            {
                case 0:
                    characterInput.PressDodgeLeft();
                    break;
                case 1:
                    characterInput.PressDodgeRight();
                    break;
                case 2:
                    characterInput.PressDodgeBack();
                    break;
            }
        }

        /// <summary>
        /// Counter attack routine (block then attack).
        /// </summary>
        private System.Collections.IEnumerator CounterAttackRoutine()
        {
            _isCountering = true;

            // Wait for counter delay
            yield return new WaitForSeconds(counterAttackDelay);

            // Release block
            if (characterInput != null)
            {
                characterInput.ReleaseBlock();
            }

            // Short delay
            yield return new WaitForSeconds(0.1f);

            // Execute jab (simple attack)
            if (characterCombat != null && combatStateMachine != null)
            {
                if (combatStateMachine.CurrentState is IdleState)
                {
                    var jabMethod = typeof(CharacterCombat).GetMethod("PerformJab",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    jabMethod?.Invoke(characterCombat, null);
                }
            }

            // Return to blocking
            yield return new WaitForSeconds(0.5f);
            _isCountering = false;
        }

        /// <summary>
        /// Sets the dummy behavior mode.
        /// </summary>
        public void SetBehavior(DummyBehavior behavior)
        {
            // Reset state when changing behavior
            if (currentBehavior != behavior)
            {
                _isCountering = false;

                // Release block if switching away from blocking behaviors
                if (characterInput != null && combatStateMachine != null)
                {
                    if (combatStateMachine.CurrentState is BlockingState)
                    {
                        characterInput.ReleaseBlock();
                    }
                }
            }

            currentBehavior = behavior;
            Debug.Log($"DummyAI: Behavior set to {behavior}");
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            ValidateReferences();
        }
        #endif
    }
}
