using UnityEngine;

namespace Knockout.Combat.HitDetection
{
    /// <summary>
    /// Component that marks a collider as a hurtbox (area that can receive damage).
    /// Attached to body parts like head, torso, etc.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class HurtboxData : MonoBehaviour
    {
        [Header("Hurtbox Properties")]
        [SerializeField]
        [Tooltip("Damage multiplier for hits on this body part (e.g., head = 1.5x, body = 1.0x)")]
        private float damageMultiplier = 1.0f;

        [SerializeField]
        [Tooltip("Override hit type: -1 = use attack's type, 0 = light, 1 = medium, 2 = heavy")]
        private int hitTypeOverride = -1;

        [SerializeField]
        [Tooltip("Reference to the root character GameObject that owns this hurtbox")]
        private GameObject ownerCharacter;

        private Collider _collider;

        #region Public Properties

        /// <summary>
        /// Damage multiplier for this body part.
        /// </summary>
        public float DamageMultiplier => damageMultiplier;

        /// <summary>
        /// Hit type override (-1 means use attack's type).
        /// </summary>
        public int HitTypeOverride => hitTypeOverride;

        /// <summary>
        /// The character GameObject that owns this hurtbox.
        /// </summary>
        public GameObject OwnerCharacter => ownerCharacter;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _collider = GetComponent<Collider>();

            // Verify collider is set as trigger
            if (_collider != null && !_collider.isTrigger)
            {
                Debug.LogWarning($"[{gameObject.name}] Hurtbox collider should be a trigger. Setting isTrigger = true.", this);
                _collider.isTrigger = true;
            }
        }

        private void OnValidate()
        {
            // Auto-assign owner character by searching up the hierarchy
            if (ownerCharacter == null)
            {
                var controller = GetComponentInParent<Characters.CharacterController>();
                if (controller != null)
                {
                    ownerCharacter = controller.gameObject;
                }
            }
        }

        #endregion
    }
}
