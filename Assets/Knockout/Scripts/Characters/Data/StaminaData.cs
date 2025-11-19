using UnityEngine;

namespace Knockout.Characters.Data
{
    /// <summary>
    /// ScriptableObject containing stamina configuration and exhaustion parameters.
    /// Defines stamina pool size, regeneration rate, consumption costs, and exhaustion penalties.
    /// </summary>
    [CreateAssetMenu(fileName = "StaminaData", menuName = "Knockout/Stamina Data", order = 3)]
    public class StaminaData : ScriptableObject
    {
        [Header("Stamina Pool")]
        [SerializeField]
        [Tooltip("Maximum stamina pool size")]
        private float maxStamina = 100f;

        [SerializeField]
        [Tooltip("Stamina regeneration rate per second when not attacking")]
        private float regenPerSecond = 25f;

        [Header("Attack Costs")]
        [SerializeField]
        [Tooltip("Stamina cost for each attack type (indexed: 0=Jab, 1=Hook, 2=Uppercut)")]
        private float[] attackCosts = new float[] { 10f, 15f, 20f };

        [SerializeField]
        [Tooltip("Base stamina cost for special moves")]
        private float specialMoveCost = 40f;

        [Header("Exhaustion Settings")]
        [SerializeField]
        [Tooltip("Minimum duration of exhaustion state in seconds")]
        private float exhaustionDuration = 2f;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Stamina regeneration multiplier during exhaustion (0.5 = half speed)")]
        private float exhaustionRegenMultiplier = 0.5f;

        [SerializeField]
        [Tooltip("Stamina threshold percentage to exit exhaustion (e.g., 25 = 25%)")]
        private float exhaustionRecoveryThreshold = 25f;

        // Read-only public properties
        /// <summary>
        /// Maximum stamina pool size.
        /// </summary>
        public float MaxStamina => maxStamina;

        /// <summary>
        /// Stamina regeneration rate per second when not attacking.
        /// </summary>
        public float RegenPerSecond => regenPerSecond;

        /// <summary>
        /// Array of stamina costs indexed by attack type (0=Jab, 1=Hook, 2=Uppercut).
        /// </summary>
        public float[] AttackCosts => attackCosts;

        /// <summary>
        /// Base stamina cost for special moves.
        /// </summary>
        public float SpecialMoveCost => specialMoveCost;

        /// <summary>
        /// Minimum duration of exhaustion state in seconds.
        /// </summary>
        public float ExhaustionDuration => exhaustionDuration;

        /// <summary>
        /// Stamina regeneration multiplier during exhaustion.
        /// </summary>
        public float ExhaustionRegenMultiplier => exhaustionRegenMultiplier;

        /// <summary>
        /// Stamina threshold percentage to exit exhaustion state.
        /// </summary>
        public float ExhaustionRecoveryThreshold => exhaustionRecoveryThreshold;

        /// <summary>
        /// Gets the stamina cost for a specific attack type.
        /// </summary>
        /// <param name="attackTypeIndex">Attack type (0=Jab, 1=Hook, 2=Uppercut)</param>
        /// <returns>Stamina cost, or 0 if index is invalid</returns>
        public float GetAttackCost(int attackTypeIndex)
        {
            if (attackTypeIndex < 0 || attackTypeIndex >= attackCosts.Length)
            {
                return 0f;
            }
            return attackCosts[attackTypeIndex];
        }

        private void OnValidate()
        {
            // Clamp max stamina to positive values
            maxStamina = Mathf.Max(0f, maxStamina);

            // Clamp regeneration rate to non-negative
            regenPerSecond = Mathf.Max(0f, regenPerSecond);

            // Ensure attack costs array has correct size
            if (attackCosts == null || attackCosts.Length != 3)
            {
                attackCosts = new float[] { 10f, 15f, 20f };
            }

            // Clamp attack costs to valid range [0, maxStamina]
            for (int i = 0; i < attackCosts.Length; i++)
            {
                attackCosts[i] = Mathf.Clamp(attackCosts[i], 0f, maxStamina);
            }

            // Clamp special move cost
            specialMoveCost = Mathf.Clamp(specialMoveCost, 0f, maxStamina);

            // Clamp exhaustion duration to non-negative
            exhaustionDuration = Mathf.Max(0f, exhaustionDuration);

            // Exhaustion regen multiplier already clamped by [Range] attribute

            // Clamp recovery threshold to valid percentage range
            exhaustionRecoveryThreshold = Mathf.Clamp(exhaustionRecoveryThreshold, 0f, 100f);
        }
    }
}
