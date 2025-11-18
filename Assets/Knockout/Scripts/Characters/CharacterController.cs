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

        // Phase 3 components
        private Knockout.Characters.Components.CharacterInput _characterInput;
        private Knockout.Characters.Components.CharacterMovement _characterMovement;
        private Knockout.Characters.Components.CharacterCombat _characterCombat;
        private Knockout.Characters.Components.CharacterHealth _characterHealth;

        // TODO: Phase 4 component
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

        /// <summary>
        /// Character input component (Phase 3).
        /// </summary>
        public Knockout.Characters.Components.CharacterInput CharacterInput => _characterInput;

        /// <summary>
        /// Character movement component (Phase 3).
        /// </summary>
        public Knockout.Characters.Components.CharacterMovement CharacterMovement => _characterMovement;

        /// <summary>
        /// Character combat component (Phase 3).
        /// </summary>
        public Knockout.Characters.Components.CharacterCombat CharacterCombat => _characterCombat;

        /// <summary>
        /// Character health component (Phase 3).
        /// </summary>
        public Knockout.Characters.Components.CharacterHealth CharacterHealth => _characterHealth;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            CacheComponents();
            ValidateSetup();
        }

        private void Start()
        {
            // Wire up component event connections
            WireComponentEvents();
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

            // Phase 3 components
            _characterInput = GetComponent<Knockout.Characters.Components.CharacterInput>();
            _characterMovement = GetComponent<Knockout.Characters.Components.CharacterMovement>();
            _characterCombat = GetComponent<Knockout.Characters.Components.CharacterCombat>();
            _characterHealth = GetComponent<Knockout.Characters.Components.CharacterHealth>();

            // TODO: Phase 4 component
            // _characterAI = GetComponent<CharacterAI>();
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

        /// <summary>
        /// Wires up event connections between components.
        /// </summary>
        private void WireComponentEvents()
        {
            // CharacterInput → CharacterMovement
            if (_characterInput != null && _characterMovement != null)
            {
                _characterInput.OnMoveInput += _characterMovement.SetMovementInput;
            }

            // CharacterInput → CharacterCombat
            if (_characterInput != null && _characterCombat != null)
            {
                _characterInput.OnJabPressed += _characterCombat.ExecuteJab;
                _characterInput.OnHookPressed += _characterCombat.ExecuteHook;
                _characterInput.OnUppercutPressed += _characterCombat.ExecuteUppercut;
                _characterInput.OnBlockPressed += _characterCombat.StartBlocking;
                _characterInput.OnBlockReleased += _characterCombat.StopBlocking;
            }

            // CharacterHealth → CharacterInput (disable input on death)
            if (_characterHealth != null && _characterInput != null)
            {
                _characterHealth.OnDeath += _characterInput.DisableInput;
            }
        }

        #endregion
    }
}
