using System;
using System.Collections.Generic;
using UnityEngine;
using Knockout.Combat;
using Knockout.Combat.States;
using Knockout.Combat.HitDetection;
using Knockout.Characters.Data;

namespace Knockout.Characters.Components
{
    /// <summary>
    /// Manages character combat actions, state machine, and hitbox activation.
    /// Coordinates attacks, defense, and combat state transitions.
    /// </summary>
    [RequireComponent(typeof(CharacterAnimator))]
    public class CharacterCombat : MonoBehaviour
    {
        [Header("Combat Data")]
        [SerializeField]
        [Tooltip("Attack data for jab attack")]
        private AttackData jabAttackData;

        [SerializeField]
        [Tooltip("Attack data for hook attack")]
        private AttackData hookAttackData;

        [SerializeField]
        [Tooltip("Attack data for uppercut attack")]
        private AttackData uppercutAttackData;

        [Header("Hitbox References")]
        [SerializeField]
        [Tooltip("Left hand hitbox GameObject")]
        private GameObject leftHandHitbox;

        [SerializeField]
        [Tooltip("Right hand hitbox GameObject")]
        private GameObject rightHandHitbox;

        // Component references
        private CharacterAnimator _characterAnimator;
        private CharacterStamina _characterStamina;

        // Combat state machine
        private CombatStateMachine _stateMachine;

        // Current attack being executed
        private AttackData _currentAttack;

        // Hitbox components
        private HitboxData _leftHitboxData;
        private HitboxData _rightHitboxData;

        #region Events

        /// <summary>
        /// Fired when an attack is executed.
        /// Parameter: attack type (0 = jab, 1 = hook, 2 = uppercut)
        /// </summary>
        public event Action<int> OnAttackExecuted;

        /// <summary>
        /// Fired when blocking starts.
        /// </summary>
        public event Action OnBlockStarted;

        /// <summary>
        /// Fired when blocking ends.
        /// </summary>
        public event Action OnBlockEnded;

        /// <summary>
        /// Fired when attempting to attack without sufficient stamina.
        /// </summary>
        public event Action OnAttackFailedNoStamina;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the current combat state.
        /// </summary>
        public CombatState CurrentState => _stateMachine?.CurrentState;

        /// <summary>
        /// Gets whether the character is currently blocking.
        /// </summary>
        public bool IsBlocking => CurrentState is BlockingState;

        /// <summary>
        /// Gets whether the character is currently attacking.
        /// </summary>
        public bool IsAttacking => CurrentState is AttackingState;

        /// <summary>
        /// Gets whether the character can currently act.
        /// </summary>
        public bool CanAct => CurrentState is IdleState || CurrentState is BlockingState;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Cache component references
            _characterAnimator = GetComponent<CharacterAnimator>();
            _characterStamina = GetComponent<CharacterStamina>();

            // Find hitboxes in children if not assigned
            if (leftHandHitbox == null || rightHandHitbox == null)
            {
                FindHitboxes();
            }

            // Get HitboxData components
            if (leftHandHitbox != null)
            {
                _leftHitboxData = leftHandHitbox.GetComponent<HitboxData>();
            }

            if (rightHandHitbox != null)
            {
                _rightHitboxData = rightHandHitbox.GetComponent<HitboxData>();
            }

            // Initialize state machine
            _stateMachine = new CombatStateMachine();
        }

        private void Start()
        {
            // Initialize state machine with idle state
            _stateMachine.Initialize(this, new IdleState());

            // Subscribe to animator events
            if (_characterAnimator != null)
            {
                _characterAnimator.OnHitboxActivate += HandleHitboxActivate;
                _characterAnimator.OnHitboxDeactivate += HandleHitboxDeactivate;
                _characterAnimator.OnAttackEnd += HandleAttackEnd;
                _characterAnimator.OnHitReactionEnd += HandleHitReactionEnd;
                _characterAnimator.OnGetUpComplete += HandleGetUpComplete;
            }
        }

        private void Update()
        {
            // Update state machine
            _stateMachine?.Update();
        }

        private void OnDestroy()
        {
            // Unsubscribe from animator events
            if (_characterAnimator != null)
            {
                _characterAnimator.OnHitboxActivate -= HandleHitboxActivate;
                _characterAnimator.OnHitboxDeactivate -= HandleHitboxDeactivate;
                _characterAnimator.OnAttackEnd -= HandleAttackEnd;
                _characterAnimator.OnHitReactionEnd -= HandleHitReactionEnd;
                _characterAnimator.OnGetUpComplete -= HandleGetUpComplete;
            }
        }

        private void OnValidate()
        {
            // Auto-find hitboxes in editor
            if (leftHandHitbox == null || rightHandHitbox == null)
            {
                FindHitboxes();
            }
        }

        #endregion

        #region Attack Methods

        /// <summary>
        /// Executes an attack with the specified attack data.
        /// </summary>
        /// <param name="attackData">The attack data to execute</param>
        /// <returns>True if attack started, false if unable to attack</returns>
        public bool ExecuteAttack(AttackData attackData)
        {
            if (attackData == null)
            {
                Debug.LogWarning($"[{gameObject.name}] ExecuteAttack called with null AttackData!", this);
                return false;
            }

            // Check stamina availability FIRST (before state transition)
            if (_characterStamina != null)
            {
                float staminaCost = _characterStamina.GetStaminaCostForAttackData(attackData);

                if (!_characterStamina.HasStamina(staminaCost))
                {
                    // Insufficient stamina - attack fails
                    OnAttackFailedNoStamina?.Invoke();
                    return false;
                }

                // Consume stamina before attack execution
                if (!_characterStamina.ConsumeStamina(staminaCost))
                {
                    // Consumption failed (shouldn't happen since we checked, but safety)
                    OnAttackFailedNoStamina?.Invoke();
                    return false;
                }
            }

            // Check if can attack
            if (!_stateMachine.CanTransitionTo(new AttackingState()))
            {
                return false;
            }

            // Store current attack
            _currentAttack = attackData;

            // Transition to attacking state
            _stateMachine.ChangeState(new AttackingState());

            // Trigger attack animation
            _characterAnimator.TriggerAttack(attackData.AttackTypeIndex);

            // Fire event with attack type
            OnAttackExecuted?.Invoke(attackData.AttackTypeIndex);

            return true;
        }

        /// <summary>
        /// Executes a jab attack.
        /// </summary>
        public bool ExecuteJab()
        {
            return ExecuteAttack(jabAttackData);
        }

        /// <summary>
        /// Executes a hook attack.
        /// </summary>
        public bool ExecuteHook()
        {
            return ExecuteAttack(hookAttackData);
        }

        /// <summary>
        /// Executes an uppercut attack.
        /// </summary>
        public bool ExecuteUppercut()
        {
            return ExecuteAttack(uppercutAttackData);
        }

        #endregion

        #region Defense Methods

        /// <summary>
        /// Starts blocking.
        /// </summary>
        public bool StartBlocking()
        {
            // Check if can block
            if (!_stateMachine.CanTransitionTo(new BlockingState()))
            {
                return false;
            }

            // Transition to blocking state
            _stateMachine.ChangeState(new BlockingState());

            // Trigger block animation
            _characterAnimator.SetBlocking(true);

            // Fire event
            OnBlockStarted?.Invoke();

            return true;
        }

        /// <summary>
        /// Stops blocking.
        /// </summary>
        public void StopBlocking()
        {
            // Only stop blocking if currently blocking
            if (!IsBlocking)
            {
                return;
            }

            // Transition back to idle
            _stateMachine.ChangeState(new IdleState());

            // Stop block animation
            _characterAnimator.SetBlocking(false);

            // Fire event
            OnBlockEnded?.Invoke();
        }

        #endregion

        #region Hit Reaction Methods

        /// <summary>
        /// Triggers a hit reaction based on hit type.
        /// Called by CharacterHealth when damage is taken.
        /// </summary>
        /// <param name="hitType">0=light, 1=medium, 2=heavy</param>
        public void TriggerHitReaction(int hitType)
        {
            // Transition to hit stunned state
            _stateMachine.ChangeState(new HitStunnedState());

            // Trigger hit reaction animation
            _characterAnimator.TriggerHitReaction(hitType);
        }

        /// <summary>
        /// Triggers a knockdown.
        /// Called by CharacterHealth.
        /// </summary>
        public void TriggerKnockdown()
        {
            // Transition to knocked down state
            _stateMachine.ChangeState(new KnockedDownState());

            // Trigger knockdown animation
            _characterAnimator.TriggerKnockdown();
        }

        /// <summary>
        /// Triggers a knockout.
        /// Called by CharacterHealth when health reaches zero.
        /// </summary>
        public void TriggerKnockout()
        {
            // Transition to knocked out state
            _stateMachine.ChangeState(new KnockedOutState());

            // Trigger knockout animation
            _characterAnimator.TriggerKnockout();
        }

        #endregion

        #region Hitbox Management

        private void HandleHitboxActivate()
        {
            if (_currentAttack == null)
            {
                Debug.LogWarning($"[{gameObject.name}] Hitbox activation event but no current attack!", this);
                return;
            }

            // Determine which hitbox to activate based on attack
            // For now, activate both (later can be refined per attack)
            HitboxData hitboxToActivate = DetermineActiveHitbox(_currentAttack);

            if (hitboxToActivate != null)
            {
                hitboxToActivate.ActivateHitbox(_currentAttack);
            }
        }

        private void HandleHitboxDeactivate()
        {
            // Deactivate all hitboxes
            if (_leftHitboxData != null)
            {
                _leftHitboxData.DeactivateHitbox();
            }

            if (_rightHitboxData != null)
            {
                _rightHitboxData.DeactivateHitbox();
            }
        }

        private HitboxData DetermineActiveHitbox(AttackData attack)
        {
            // Simple logic: alternate between left and right
            // Can be enhanced later based on attack type or animation
            // For now, use right hand for most attacks
            return _rightHitboxData != null ? _rightHitboxData : _leftHitboxData;
        }

        #endregion

        #region Animation Event Handlers

        private void HandleAttackEnd()
        {
            // Attack complete, return to idle
            _stateMachine.ChangeState(new IdleState());

            // Clear current attack
            _currentAttack = null;
        }

        private void HandleHitReactionEnd()
        {
            // Hit stun over, return to idle
            _stateMachine.ChangeState(new IdleState());
        }

        private void HandleGetUpComplete()
        {
            // Get up complete, return to idle
            _stateMachine.ChangeState(new IdleState());
        }

        #endregion

        #region Helper Methods

        private void FindHitboxes()
        {
            // Find hitbox container
            Transform hitboxesContainer = transform.Find("Hitboxes");
            if (hitboxesContainer == null)
            {
                return;
            }

            // Find left and right hand hitboxes
            if (leftHandHitbox == null)
            {
                Transform leftHand = hitboxesContainer.Find("Hitbox_LeftHand");
                if (leftHand != null)
                {
                    leftHandHitbox = leftHand.gameObject;
                }
            }

            if (rightHandHitbox == null)
            {
                Transform rightHand = hitboxesContainer.Find("Hitbox_RightHand");
                if (rightHand != null)
                {
                    rightHandHitbox = rightHand.gameObject;
                }
            }
        }

        #endregion
    }
}
