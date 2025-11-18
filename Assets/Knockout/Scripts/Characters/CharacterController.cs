using UnityEngine;
using Knockout.Characters.Data;

namespace Knockout.Characters
{
    /// <summary>
    /// Main character controller component that coordinates all character subsystems.
    /// This is a minimal foundation that will be extended in later phases with:
    /// - CharacterAnimator (Phase 2)
    /// - CharacterInput (Phase 2)
    /// - CharacterMovement (Phase 2)
    /// - CharacterCombat (Phase 3)
    /// - CharacterHealth (Phase 3)
    /// - CharacterAI (Phase 4)
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody))]
    public class CharacterController : MonoBehaviour
    {
        [Header("Character Data")]
        [SerializeField]
        [Tooltip("Character statistics and properties")]
        private CharacterStats characterStats;

        // Cached Unity components
        private Animator _animator;
        private Rigidbody _rigidbody;

        // Phase 2 components
        private Knockout.Characters.Components.CharacterAnimator _characterAnimator;

        // TODO: Add component references in future phases
        // private CharacterInput _characterInput;
        // private CharacterMovement _characterMovement;
        // private CharacterCombat _characterCombat;
        // private CharacterHealth _characterHealth;
        // private CharacterAI _characterAI;

        #region Public Properties

        /// <summary>
        /// Character statistics and properties.
        /// </summary>
        public CharacterStats Stats => characterStats;

        /// <summary>
        /// Cached Animator component.
        /// </summary>
        public Animator Animator => _animator;

        /// <summary>
        /// Cached Rigidbody component.
        /// </summary>
        public new Rigidbody rigidbody => _rigidbody;

        /// <summary>
        /// Character animator component (Phase 2).
        /// </summary>
        public Knockout.Characters.Components.CharacterAnimator CharacterAnimator => _characterAnimator;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            CacheComponents();
            ValidateSetup();
        }

        private void Start()
        {
            // TODO: Initialize subsystems in future phases
            // Example:
            // _characterAnimator?.Initialize(this);
            // _characterInput?.Initialize(this);
            // _characterMovement?.Initialize(this);
        }

        private void OnValidate()
        {
            // Provide editor-time warnings if required components are missing
            if (characterStats == null)
            {
                Debug.LogWarning($"[{gameObject.name}] CharacterStats is not assigned. " +
                    "Please assign a CharacterStats ScriptableObject.", this);
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Cache references to required Unity components.
        /// </summary>
        private void CacheComponents()
        {
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody>();

            // Phase 2 components
            _characterAnimator = GetComponent<Knockout.Characters.Components.CharacterAnimator>();

            // TODO: Cache custom components in future phases
            // _characterInput = GetComponent<CharacterInput>();
            // _characterMovement = GetComponent<CharacterMovement>();
            // etc.
        }

        /// <summary>
        /// Validate that all required components and data are present.
        /// </summary>
        private void ValidateSetup()
        {
            bool isValid = true;

            // Validate CharacterStats
            if (characterStats == null)
            {
                Debug.LogError($"[{gameObject.name}] CharacterStats is not assigned!", this);
                isValid = false;
            }

            // Validate Animator
            if (_animator == null)
            {
                Debug.LogError($"[{gameObject.name}] Animator component is missing!", this);
                isValid = false;
            }
            else if (_animator.avatar == null)
            {
                Debug.LogError($"[{gameObject.name}] Animator has no Avatar assigned!", this);
                isValid = false;
            }
            else if (!_animator.avatar.isHuman)
            {
                Debug.LogError($"[{gameObject.name}] Animator Avatar must be Humanoid!", this);
                isValid = false;
            }
            else if (!_animator.avatar.isValid)
            {
                Debug.LogError($"[{gameObject.name}] Animator Avatar is not valid!", this);
                isValid = false;
            }

            // Validate Rigidbody
            if (_rigidbody == null)
            {
                Debug.LogError($"[{gameObject.name}] Rigidbody component is missing!", this);
                isValid = false;
            }

            if (!isValid)
            {
                Debug.LogError($"[{gameObject.name}] CharacterController setup is invalid! " +
                    "Character may not function correctly.", this);
            }
        }

        #endregion
    }
}
