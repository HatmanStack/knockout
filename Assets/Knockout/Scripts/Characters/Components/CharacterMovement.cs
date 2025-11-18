using UnityEngine;
using Knockout.Characters.Data;
using Knockout.Combat.States;

namespace Knockout.Characters.Components
{
    /// <summary>
    /// Manages character locomotion, rotation, and root motion application.
    /// Handles movement input processing and selective root motion during attacks.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CharacterAnimator))]
    public class CharacterMovement : MonoBehaviour
    {
        [Header("Character Stats")]
        [SerializeField]
        [Tooltip("Character stats ScriptableObject")]
        private CharacterStats characterStats;

        [Header("Movement Settings")]
        [SerializeField]
        [Tooltip("Speed multiplier when blocking")]
        [Range(0.1f, 1f)]
        private float blockingSpeedMultiplier = 0.5f;

        // Component references
        private CharacterAnimator _characterAnimator;
        private CharacterCombat _characterCombat;
        private Rigidbody _rigidbody;
        private Animator _animator;

        // Movement state
        private Vector2 _movementInput;
        private Vector3 _facingDirection;

        // Root motion tracking
        private Vector3 _rootMotionDelta;

        #region Public Properties

        /// <summary>
        /// Gets the current movement input.
        /// </summary>
        public Vector2 MovementInput => _movementInput;

        /// <summary>
        /// Gets the current facing direction.
        /// </summary>
        public Vector3 FacingDirection => _facingDirection;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Cache component references
            _characterAnimator = GetComponent<CharacterAnimator>();
            _characterCombat = GetComponent<CharacterCombat>();
            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();

            // Initialize facing direction
            _facingDirection = transform.forward;
        }

        private void FixedUpdate()
        {
            // Apply movement
            ApplyMovement();
        }

        private void OnAnimatorMove()
        {
            // Capture root motion delta
            if (_animator != null)
            {
                _rootMotionDelta = _animator.deltaPosition;
            }

            // Selectively apply root motion based on combat state
            ApplyRootMotion();
        }

        private void OnValidate()
        {
            // Provide editor-time warnings
            if (characterStats == null)
            {
                Debug.LogWarning($"[{gameObject.name}] CharacterMovement: CharacterStats not assigned!", this);
            }
        }

        #endregion

        #region Movement Input

        /// <summary>
        /// Sets the movement input vector.
        /// Called by CharacterInput component.
        /// </summary>
        /// <param name="input">Movement input (-1 to 1 on X and Y axes)</param>
        public void SetMovementInput(Vector2 input)
        {
            _movementInput = input;

            // Update animator with movement direction and speed
            float speed = input.magnitude;
            Vector2 direction = input.normalized;

            if (_characterAnimator != null)
            {
                _characterAnimator.SetMovement(direction, speed);
            }
        }

        #endregion

        #region Movement Application

        /// <summary>
        /// Applies movement based on input and current state.
        /// </summary>
        private void ApplyMovement()
        {
            // Check if movement is allowed
            if (!CanMove())
            {
                return;
            }

            // Get move speed from stats
            float moveSpeed = characterStats != null ? characterStats.MoveSpeed : 5f;

            // Apply blocking speed reduction
            if (_characterCombat != null && _characterCombat.IsBlocking)
            {
                moveSpeed *= blockingSpeedMultiplier;
            }

            // Calculate world-space movement direction
            Vector3 moveDirection = new Vector3(_movementInput.x, 0f, _movementInput.y);

            // Calculate target velocity
            Vector3 targetVelocity = moveDirection * moveSpeed;

            // Apply to rigidbody (preserve Y velocity for gravity)
            if (_rigidbody != null)
            {
                _rigidbody.velocity = new Vector3(targetVelocity.x, _rigidbody.velocity.y, targetVelocity.z);
            }
        }

        /// <summary>
        /// Checks if character is allowed to move based on current state.
        /// </summary>
        private bool CanMove()
        {
            if (_characterCombat == null)
            {
                return true;
            }

            // Can move in idle and blocking states
            CombatState currentState = _characterCombat.CurrentState;

            return currentState is IdleState || currentState is BlockingState;
        }

        #endregion

        #region Root Motion

        /// <summary>
        /// Applies root motion selectively based on combat state.
        /// Root motion is used during attacks for momentum, but not during locomotion.
        /// </summary>
        private void ApplyRootMotion()
        {
            // Only apply root motion during attacks
            if (_characterCombat != null && _characterCombat.IsAttacking)
            {
                // Apply horizontal root motion (XZ), ignore vertical (Y)
                Vector3 rootMotionXZ = new Vector3(_rootMotionDelta.x, 0f, _rootMotionDelta.z);

                if (_rigidbody != null)
                {
                    _rigidbody.MovePosition(_rigidbody.position + rootMotionXZ);
                }
            }
            // During other states, root motion is ignored (transform-based movement used instead)
        }

        #endregion

        #region Rotation

        /// <summary>
        /// Rotates character to face the specified target position.
        /// </summary>
        /// <param name="targetPosition">World position to face</param>
        public void RotateToward(Vector3 targetPosition)
        {
            // Calculate direction to target (on XZ plane)
            Vector3 directionToTarget = targetPosition - transform.position;
            directionToTarget.y = 0f; // Ignore height difference

            if (directionToTarget.sqrMagnitude < 0.01f)
            {
                return; // Target too close or same position
            }

            directionToTarget.Normalize();

            // Get rotation speed from stats
            float rotationSpeed = characterStats != null ? characterStats.RotationSpeed : 10f;

            // Smoothly rotate toward target
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            // Update facing direction
            _facingDirection = directionToTarget;
        }

        /// <summary>
        /// Rotates character to face the specified direction.
        /// </summary>
        /// <param name="direction">Direction to face (normalized)</param>
        public void RotateTowardDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.01f)
            {
                return;
            }

            direction.y = 0f;
            direction.Normalize();

            // Get rotation speed from stats
            float rotationSpeed = characterStats != null ? characterStats.RotationSpeed : 10f;

            // Smoothly rotate toward direction
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            // Update facing direction
            _facingDirection = direction;
        }

        /// <summary>
        /// Instantly sets the character's facing direction.
        /// </summary>
        /// <param name="direction">Direction to face (normalized)</param>
        public void SetFacingDirection(Vector3 direction)
        {
            direction.y = 0f;
            direction.Normalize();

            transform.rotation = Quaternion.LookRotation(direction);
            _facingDirection = direction;
        }

        #endregion
    }
}
