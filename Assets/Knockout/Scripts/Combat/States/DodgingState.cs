using System;
using UnityEngine;
using Knockout.Characters.Components;
using Knockout.Characters.Data;

namespace Knockout.Combat.States
{
    /// <summary>
    /// Dodging combat state - character performs directional dodge with i-frame invincibility.
    /// During i-frame window, character is invulnerable to hits.
    /// Can transition to: Idle (on completion), HitStunned (outside i-frames), KnockedDown, KnockedOut.
    /// Cannot transition to: Attacking, Blocking.
    /// </summary>
    public class DodgingState : CombatState
    {
        private const int TARGET_FRAME_RATE = 60;
        private const float FIXED_DELTA_TIME = 1f / TARGET_FRAME_RATE;

        private int _currentFrame;
        private DodgeDirection _dodgeDirection;
        private DodgeData _dodgeData;
        private Vector3 _dodgeVelocity;
        private Rigidbody _rigidbody;
        private Transform _transform;
        private bool _movementApplied;

        /// <summary>
        /// Event fired when dodge begins.
        /// </summary>
        public static event Action<CharacterCombat, DodgeDirection> OnDodgeStarted;

        /// <summary>
        /// Event fired when dodge completes.
        /// </summary>
        public static event Action<CharacterCombat> OnDodgeEnded;

        /// <summary>
        /// Gets whether character is currently invulnerable (during i-frames).
        /// </summary>
        public bool IsInvulnerable
        {
            get
            {
                if (_dodgeData == null) return false;

                int iFrameEnd = _dodgeData.IFrameStartFrame + _dodgeData.IFrameDurationFrames;
                return _currentFrame >= _dodgeData.IFrameStartFrame && _currentFrame < iFrameEnd;
            }
        }

        /// <summary>
        /// Gets the current dodge direction.
        /// </summary>
        public DodgeDirection CurrentDirection => _dodgeDirection;

        public override void Enter(CharacterCombat combat)
        {
            // This method will be called with direction parameter
            // Direction will be set via SetDirection before Enter is called
            Debug.LogError($"[{combat.gameObject.name}] DodgingState.Enter called without direction! Use SetDirection first.");
        }

        /// <summary>
        /// Enters the dodging state with the specified direction.
        /// </summary>
        /// <param name="combat">Reference to CharacterCombat</param>
        /// <param name="direction">Direction to dodge</param>
        /// <param name="dodgeData">Dodge configuration data</param>
        public void Enter(CharacterCombat combat, DodgeDirection direction, DodgeData dodgeData)
        {
            if (dodgeData == null)
            {
                Debug.LogError($"[{combat.gameObject.name}] DodgingState: DodgeData is required!", combat);
                return;
            }

            // Store dodge parameters
            _dodgeDirection = direction;
            _dodgeData = dodgeData;
            _currentFrame = 0;
            _movementApplied = false;

            // Cache component references
            _rigidbody = combat.GetComponent<Rigidbody>();
            _transform = combat.transform;

            // Calculate dodge velocity based on direction
            CalculateDodgeVelocity(combat);

            // Trigger dodge animation
            TriggerDodgeAnimation(combat);

            // Fire event
            OnDodgeStarted?.Invoke(combat, direction);
        }

        public override void Update(CharacterCombat combat)
        {
            // Track frame count for timing
            _currentFrame++;

            // Apply dodge movement (distribute over dodge duration)
            ApplyDodgeMovement();

            // Check for dodge completion
            if (_dodgeData != null && _currentFrame >= _dodgeData.DodgeDurationFrames)
            {
                // Dodge complete - transition handled by state machine
                // CombatStateMachine should check this and auto-transition to Idle
            }
        }

        public override void Exit(CharacterCombat combat)
        {
            // Clear dodge velocity
            if (_rigidbody != null)
            {
                _rigidbody.velocity = Vector3.zero;
            }

            // Fire event
            OnDodgeEnded?.Invoke(combat);
        }

        public override bool CanTransitionTo(CombatState newState)
        {
            // Cannot attack while dodging
            if (newState is AttackingState)
            {
                return false;
            }

            // Cannot block while dodging
            if (newState is BlockingState)
            {
                return false;
            }

            // Cannot dodge while already dodging
            if (newState is DodgingState)
            {
                return false;
            }

            // Can transition to idle when dodge completes
            if (newState is IdleState)
            {
                return _dodgeData != null && _currentFrame >= _dodgeData.DodgeDurationFrames;
            }

            // Can be hit outside i-frame window
            if (newState is HitStunnedState)
            {
                return !IsInvulnerable;
            }

            // Always allow knockdown/knockout
            return newState is KnockedDownState
                || newState is SpecialKnockdownState
                || newState is KnockedOutState;
        }

        /// <summary>
        /// Checks if dodge is complete.
        /// </summary>
        public bool IsDodgeComplete()
        {
            return _dodgeData != null && _currentFrame >= _dodgeData.DodgeDurationFrames;
        }

        /// <summary>
        /// Gets the current frame count.
        /// </summary>
        public int CurrentFrame => _currentFrame;

        private void CalculateDodgeVelocity(CharacterCombat combat)
        {
            if (_transform == null || _dodgeData == null)
            {
                _dodgeVelocity = Vector3.zero;
                return;
            }

            // Get movement direction based on character facing and dodge direction
            Vector3 moveDirection = Vector3.zero;

            switch (_dodgeDirection)
            {
                case DodgeDirection.Left:
                    // Left relative to facing direction
                    moveDirection = -_transform.right;
                    break;

                case DodgeDirection.Right:
                    // Right relative to facing direction
                    moveDirection = _transform.right;
                    break;

                case DodgeDirection.Back:
                    // Backward from facing direction
                    moveDirection = -_transform.forward;
                    break;
            }

            // Calculate velocity to cover dodge distance over dodge duration
            float dodgeDuration = _dodgeData.DodgeDuration;
            float speed = _dodgeData.DodgeDistance / dodgeDuration;

            _dodgeVelocity = moveDirection * speed * _dodgeData.DodgeSpeedMultiplier;
        }

        private void ApplyDodgeMovement()
        {
            if (_rigidbody == null || _dodgeData == null)
            {
                return;
            }

            // Apply movement during dodge duration
            if (_currentFrame < _dodgeData.DodgeDurationFrames)
            {
                // Use velocity-based movement for smooth dodge
                _rigidbody.velocity = new Vector3(_dodgeVelocity.x, _rigidbody.velocity.y, _dodgeVelocity.z);
                _movementApplied = true;
            }
            else if (_movementApplied)
            {
                // Stop movement when dodge completes
                _rigidbody.velocity = Vector3.zero;
                _movementApplied = false;
            }
        }

        private void TriggerDodgeAnimation(CharacterCombat combat)
        {
            CharacterAnimator animator = combat.GetComponent<CharacterAnimator>();
            if (animator == null)
            {
                return;
            }

            // Trigger appropriate dodge animation based on direction
            switch (_dodgeDirection)
            {
                case DodgeDirection.Left:
                    animator.TriggerDodgeLeft();
                    break;

                case DodgeDirection.Right:
                    animator.TriggerDodgeRight();
                    break;

                case DodgeDirection.Back:
                    animator.TriggerDodgeBack();
                    break;
            }
        }
    }
}
