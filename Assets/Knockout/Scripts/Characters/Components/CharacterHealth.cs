using System;
using UnityEngine;
using Knockout.Combat.HitDetection;
using Knockout.Characters.Data;

namespace Knockout.Characters.Components
{
    /// <summary>
    /// Manages character health, damage application, and death.
    /// Calculates final damage with modifiers and triggers appropriate hit reactions.
    /// </summary>
    public class CharacterHealth : MonoBehaviour
    {
        [Header("Character Stats")]
        [SerializeField]
        [Tooltip("Character stats ScriptableObject")]
        private CharacterStats characterStats;

        // Component references
        private CharacterAnimator _characterAnimator;
        private CharacterCombat _characterCombat;

        // Current health state
        private float _currentHealth;
        private bool _isDead;

        #region Events

        /// <summary>
        /// Fired when health changes.
        /// Parameters: (currentHealth, maxHealth)
        /// </summary>
        public event Action<float, float> OnHealthChanged;

        /// <summary>
        /// Fired when character dies (health reaches zero).
        /// </summary>
        public event Action OnDeath;

        /// <summary>
        /// Fired when hit is taken.
        /// </summary>
        public event Action<HitData> OnHitTaken;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the current health value.
        /// </summary>
        public float CurrentHealth => _currentHealth;

        /// <summary>
        /// Gets the maximum health from character stats.
        /// </summary>
        public float MaxHealth => characterStats != null ? characterStats.MaxHealth : 100f;

        /// <summary>
        /// Gets the current health as a percentage (0 to 1).
        /// </summary>
        public float HealthPercentage => MaxHealth > 0 ? _currentHealth / MaxHealth : 0f;

        /// <summary>
        /// Gets whether the character is dead.
        /// </summary>
        public bool IsDead => _isDead;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Cache component references
            _characterAnimator = GetComponent<CharacterAnimator>();
            _characterCombat = GetComponent<CharacterCombat>();
        }

        private void Start()
        {
            // Initialize health to max
            _currentHealth = MaxHealth;
            _isDead = false;

            // Fire initial health changed event
            OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
        }

        private void OnValidate()
        {
            // Provide editor-time warnings
            if (characterStats == null)
            {
                Debug.LogWarning($"[{gameObject.name}] CharacterHealth: CharacterStats not assigned!", this);
            }
        }

        #endregion

        #region Damage Application

        /// <summary>
        /// Takes damage from a hit.
        /// Calculates final damage with all modifiers and triggers appropriate reactions.
        /// </summary>
        /// <param name="hitData">Data about the hit</param>
        public void TakeDamage(HitData hitData)
        {
            if (_isDead)
            {
                return; // Already dead, ignore further damage
            }

            // Calculate final damage with modifiers
            float finalDamage = CalculateFinalDamage(hitData);

            // Apply damage
            _currentHealth -= finalDamage;
            _currentHealth = Mathf.Max(0f, _currentHealth); // Clamp to 0

            // Fire events
            OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
            OnHitTaken?.Invoke(hitData);

            // Determine reaction based on damage and health
            if (_currentHealth <= 0f)
            {
                Die();
            }
            else if (ShouldKnockdown(finalDamage))
            {
                TriggerKnockdown();
            }
            else
            {
                TriggerHitReaction(finalDamage, hitData.HitType);
            }
        }

        /// <summary>
        /// Calculates final damage amount with all modifiers applied.
        /// </summary>
        private float CalculateFinalDamage(HitData hitData)
        {
            float damage = hitData.Damage;

            // Apply character damage taken multiplier from stats
            if (characterStats != null)
            {
                damage *= characterStats.DamageTakenMultiplier;
            }

            // Apply blocking reduction (75% damage reduction)
            if (_characterCombat != null && _characterCombat.IsBlocking)
            {
                damage *= 0.25f; // 75% reduction = take 25% damage
            }

            return damage;
        }

        #endregion

        #region Hit Reactions

        /// <summary>
        /// Determines if damage should trigger a knockdown.
        /// </summary>
        private bool ShouldKnockdown(float damage)
        {
            // Knockdown if heavy hit and low health
            return damage > 30f && _currentHealth < 30f;
        }

        /// <summary>
        /// Triggers appropriate hit reaction based on damage amount.
        /// </summary>
        private void TriggerHitReaction(float damage, int hitType)
        {
            // Determine hit reaction type based on damage
            int reactionType;

            if (damage < 15f)
            {
                reactionType = 0; // Light
            }
            else if (damage < 30f)
            {
                reactionType = 1; // Medium
            }
            else
            {
                reactionType = 2; // Heavy
            }

            // Use provided hit type if it's more severe
            reactionType = Mathf.Max(reactionType, hitType);

            // Trigger reaction via combat component
            if (_characterCombat != null)
            {
                _characterCombat.TriggerHitReaction(reactionType);
            }
        }

        /// <summary>
        /// Triggers knockdown reaction.
        /// </summary>
        private void TriggerKnockdown()
        {
            if (_characterCombat != null)
            {
                _characterCombat.TriggerKnockdown();
            }
        }

        /// <summary>
        /// Handles character death.
        /// </summary>
        private void Die()
        {
            if (_isDead)
            {
                return; // Already dead
            }

            _isDead = true;

            // Trigger knockout via combat component
            if (_characterCombat != null)
            {
                _characterCombat.TriggerKnockout();
            }

            // Fire death event
            OnDeath?.Invoke();

            // Disable character controls (if input component exists)
            var characterInput = GetComponent<CharacterInput>();
            if (characterInput != null)
            {
                characterInput.DisableInput();
            }
        }

        #endregion

        #region Healing

        /// <summary>
        /// Heals the character by the specified amount.
        /// </summary>
        /// <param name="amount">Amount to heal</param>
        public void Heal(float amount)
        {
            if (_isDead)
            {
                return; // Cannot heal when dead
            }

            _currentHealth += amount;
            _currentHealth = Mathf.Min(_currentHealth, MaxHealth); // Clamp to max

            // Fire health changed event
            OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
        }

        /// <summary>
        /// Resets health to maximum.
        /// Useful for round resets.
        /// </summary>
        public void ResetHealth()
        {
            _currentHealth = MaxHealth;
            _isDead = false;

            // Fire health changed event
            OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
        }

        #endregion
    }
}
