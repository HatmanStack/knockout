using System.Collections.Generic;
using UnityEngine;
using Knockout.Characters.Data;
using Knockout.Characters.Components;

namespace Knockout.Combat.HitDetection
{
    /// <summary>
    /// Component that marks a collider as a hitbox (attacking area).
    /// Attached to hands/limbs during attack animations.
    /// Handles collision detection with hurtboxes and damage delivery.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class HitboxData : MonoBehaviour
    {
        [Header("Hitbox Properties")]
        [SerializeField]
        [Tooltip("Reference to the root character GameObject that owns this hitbox")]
        private GameObject ownerCharacter;

        // Current attack data while hitbox is active
        private AttackData _currentAttack;

        // Special move data (if this hitbox is for a special move)
        private SpecialMoveData _currentSpecialMove;
        private bool _isSpecialMove = false;

        // Tracks which targets have been hit in current attack (prevents multi-hit)
        private HashSet<GameObject> _hitTargets = new HashSet<GameObject>();

        // Cached collider reference
        private Collider _collider;

        #region Public Properties

        /// <summary>
        /// The character GameObject that owns this hitbox.
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
                Debug.LogWarning($"[{gameObject.name}] Hitbox collider should be a trigger. Setting isTrigger = true.", this);
                _collider.isTrigger = true;
            }

            // Disable hitbox by default (activated during attacks)
            if (_collider != null)
            {
                _collider.enabled = false;
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

        #region Hitbox Management

        /// <summary>
        /// Activates the hitbox for a specific attack.
        /// Called by animation events during attack animations.
        /// </summary>
        /// <param name="attackData">Data for the current attack</param>
        public void ActivateHitbox(AttackData attackData)
        {
            if (attackData == null)
            {
                Debug.LogWarning($"[{gameObject.name}] ActivateHitbox called with null AttackData!", this);
                return;
            }

            _currentAttack = attackData;
            _currentSpecialMove = null;
            _isSpecialMove = false;
            _hitTargets.Clear();

            if (_collider != null)
            {
                _collider.enabled = true;
            }
        }

        /// <summary>
        /// Activates the hitbox for a special move attack with enhanced properties.
        /// </summary>
        /// <param name="attackData">Base attack data</param>
        /// <param name="specialMoveData">Special move modifiers</param>
        public void ActivateHitbox(AttackData attackData, SpecialMoveData specialMoveData)
        {
            if (attackData == null || specialMoveData == null)
            {
                Debug.LogWarning($"[{gameObject.name}] ActivateHitbox (special move) called with null data!", this);
                return;
            }

            _currentAttack = attackData;
            _currentSpecialMove = specialMoveData;
            _isSpecialMove = true;
            _hitTargets.Clear();

            if (_collider != null)
            {
                _collider.enabled = true;
            }
        }

        /// <summary>
        /// Deactivates the hitbox after attack completes.
        /// Called by animation events.
        /// </summary>
        public void DeactivateHitbox()
        {
            if (_collider != null)
            {
                _collider.enabled = false;
            }

            _currentAttack = null;
            _currentSpecialMove = null;
            _isSpecialMove = false;
            _hitTargets.Clear();
        }

        #endregion

        #region Collision Detection

        private void OnTriggerEnter(Collider other)
        {
            // Check if other has HurtboxData component
            HurtboxData hurtbox = other.GetComponent<HurtboxData>();
            if (hurtbox == null)
            {
                return;
            }

            // Prevent self-damage
            if (hurtbox.OwnerCharacter == ownerCharacter)
            {
                return;
            }

            // Prevent hitting same target multiple times per attack
            if (_hitTargets.Contains(hurtbox.OwnerCharacter))
            {
                return;
            }

            // Mark target as hit
            _hitTargets.Add(hurtbox.OwnerCharacter);

            // Calculate hit data
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            HitData hitData = CalculateHitData(hurtbox, hitPoint);

            // Send hit to target
            SendHitToTarget(hurtbox.OwnerCharacter, hitData);
        }

        #endregion

        #region Damage Calculation

        /// <summary>
        /// Calculates hit data based on attack and hurtbox properties.
        /// </summary>
        private HitData CalculateHitData(HurtboxData hurtbox, Vector3 hitPoint)
        {
            // Get base damage from attack
            float baseDamage = _currentAttack != null ? _currentAttack.Damage : 0f;

            // Apply special move multiplier if this is a special move
            if (_isSpecialMove && _currentSpecialMove != null)
            {
                baseDamage *= _currentSpecialMove.DamageMultiplier;
            }

            // Apply hurtbox damage multiplier
            float damageAfterHurtbox = baseDamage * hurtbox.DamageMultiplier;

            // Apply combo damage scaling if owner has combo tracker
            float comboMultiplier = 1.0f;
            CharacterComboTracker comboTracker = ownerCharacter?.GetComponent<CharacterComboTracker>();
            if (comboTracker != null && _currentAttack != null)
            {
                // Register hit landed and get damage multiplier (includes combo scaling + sequence bonuses)
                comboMultiplier = comboTracker.RegisterHitLanded(_currentAttack.AttackTypeIndex, damageAfterHurtbox);
            }

            // Apply combo scaling
            float finalDamage = damageAfterHurtbox * comboMultiplier;

            // Calculate hit direction
            Vector3 hitDirection = (hurtbox.transform.position - transform.position).normalized;

            // Determine hit type
            int hitType = DetermineHitType(finalDamage, hurtbox);

            // Get knockback (with special move multiplier if applicable)
            float knockback = _currentAttack != null ? _currentAttack.Knockback : 0f;
            if (_isSpecialMove && _currentSpecialMove != null)
            {
                knockback *= _currentSpecialMove.KnockbackMultiplier;
            }

            // Get attack name
            string attackName = _isSpecialMove && _currentSpecialMove != null
                ? _currentSpecialMove.SpecialMoveName
                : (_currentAttack != null ? _currentAttack.AttackName : "Unknown");

            return new HitData(
                attacker: ownerCharacter,
                damage: finalDamage,
                knockback: knockback,
                hitPoint: hitPoint,
                hitDirection: hitDirection,
                hitType: hitType,
                attackName: attackName,
                isSpecialMove: _isSpecialMove,
                specialMoveData: _currentSpecialMove
            );
        }

        /// <summary>
        /// Determines hit type based on damage amount and hurtbox override.
        /// </summary>
        private int DetermineHitType(float damage, HurtboxData hurtbox)
        {
            // Use hurtbox override if specified
            if (hurtbox.HitTypeOverride >= 0)
            {
                return hurtbox.HitTypeOverride;
            }

            // Categorize by damage thresholds
            if (damage < 15f)
            {
                return 0; // Light
            }
            else if (damage < 30f)
            {
                return 1; // Medium
            }
            else
            {
                return 2; // Heavy
            }
        }

        #endregion

        #region Hit Delivery

        /// <summary>
        /// Sends hit data to the target character's health component.
        /// </summary>
        private void SendHitToTarget(GameObject target, HitData hitData)
        {
            CharacterHealth health = target.GetComponent<CharacterHealth>();
            if (health != null)
            {
                // Check if target is blocking before hit
                CharacterCombat targetCombat = target.GetComponent<CharacterCombat>();
                bool wasBlocking = targetCombat != null && targetCombat.IsBlocking;

                // Apply damage
                health.TakeDamage(hitData);

                // If hit was blocked, break attacker's combo
                if (wasBlocking)
                {
                    CharacterComboTracker ownerComboTracker = ownerCharacter?.GetComponent<CharacterComboTracker>();
                    if (ownerComboTracker != null)
                    {
                        // Break combo due to block
                        ownerComboTracker.BreakCombo();
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] Hit target {target.name} but no CharacterHealth component found!", this);
            }
        }

        #endregion
    }
}
