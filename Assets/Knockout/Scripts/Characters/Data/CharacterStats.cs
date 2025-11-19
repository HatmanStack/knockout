using UnityEngine;

namespace Knockout.Characters.Data
{
    /// <summary>
    /// ScriptableObject containing character statistics and properties.
    /// Used to define character-specific values like health, speed, and damage modifiers.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterStats", menuName = "Knockout/Character Stats", order = 1)]
    public class CharacterStats : ScriptableObject
    {
        [Header("Health Settings")]
        [SerializeField]
        [Tooltip("Maximum health points for this character")]
        private float maxHealth = 100f;

        [Header("Movement Settings")]
        [SerializeField]
        [Range(1f, 10f)]
        [Tooltip("Movement speed multiplier")]
        private float moveSpeed = 5f;

        [SerializeField]
        [Range(1f, 20f)]
        [Tooltip("Rotation speed when turning")]
        private float rotationSpeed = 10f;

        [Header("Combat Settings")]
        [SerializeField]
        [Range(0.5f, 2f)]
        [Tooltip("Multiplier for damage dealt by this character")]
        private float damageMultiplier = 1f;

        [SerializeField]
        [Range(0.5f, 2f)]
        [Tooltip("Multiplier for damage taken by this character")]
        private float damageTakenMultiplier = 1f;

        // Read-only public properties
        public float MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public float RotationSpeed => rotationSpeed;
        public float DamageMultiplier => damageMultiplier;
        public float DamageTakenMultiplier => damageTakenMultiplier;

        #region Test Helpers

        /// <summary>
        /// Sets max health (for testing purposes).
        /// </summary>
        public void SetMaxHealth(float value)
        {
            maxHealth = value;
        }

        /// <summary>
        /// Sets damage taken multiplier (for testing purposes).
        /// </summary>
        public void SetDamageTakenMultiplier(float value)
        {
            damageTakenMultiplier = value;
        }

        #endregion
    }
}
