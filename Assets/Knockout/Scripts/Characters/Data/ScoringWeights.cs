using UnityEngine;

namespace Knockout.Characters.Data
{
    /// <summary>
    /// ScriptableObject containing configurable weights for judge scoring metrics.
    /// Defines point values for different combat actions to calculate round winner.
    /// </summary>
    [CreateAssetMenu(fileName = "ScoringWeights", menuName = "Knockout/Scoring Weights", order = 9)]
    public class ScoringWeights : ScriptableObject
    {
        [Header("Offensive Scoring")]
        [SerializeField]
        [Tooltip("Points awarded for landing a clean hit")]
        private float cleanHitPoints = 1f;

        [SerializeField]
        [Tooltip("Bonus points per hit in a combo (additive)")]
        private float comboHitBonus = 0.5f;

        [SerializeField]
        [Tooltip("Points for completing a predefined combo sequence")]
        private float comboSequencePoints = 5f;

        [SerializeField]
        [Tooltip("Points for landing a special move")]
        private float specialMovePoints = 8f;

        [SerializeField]
        [Tooltip("Points for causing a knockdown")]
        private float knockdownPoints = 10f;

        [SerializeField]
        [Tooltip("Multiplier for total damage dealt (damage * multiplier = points)")]
        private float damageDealtWeight = 0.1f;

        [Header("Defensive Scoring")]
        [SerializeField]
        [Tooltip("Points per successful block")]
        private float blockPoints = 0.5f;

        [SerializeField]
        [Tooltip("Points per successful parry")]
        private float parryPoints = 2f;

        [SerializeField]
        [Tooltip("Points per successful dodge (avoided hit)")]
        private float dodgePoints = 1f;

        [Header("Control Scoring")]
        [SerializeField]
        [Tooltip("Points awarded per second spent in offensive range")]
        private float aggressionPointsPerSecond = 0.1f;

        [SerializeField]
        [Tooltip("Bonus points for controlling the center of the ring")]
        private float ringControlBonus = 0f;

        [Header("Penalties")]
        [SerializeField]
        [Tooltip("Points deducted per exhaustion occurrence")]
        private float exhaustionPenalty = 3f;

        [SerializeField]
        [Tooltip("Small penalty for whiffed attacks")]
        private float missedAttackPenalty = 0f;

        // Read-only public properties
        /// <summary>
        /// Points awarded for landing a clean hit.
        /// </summary>
        public float CleanHitPoints => cleanHitPoints;

        /// <summary>
        /// Bonus points per hit in a combo (additive).
        /// </summary>
        public float ComboHitBonus => comboHitBonus;

        /// <summary>
        /// Points for completing a predefined combo sequence.
        /// </summary>
        public float ComboSequencePoints => comboSequencePoints;

        /// <summary>
        /// Points for landing a special move.
        /// </summary>
        public float SpecialMovePoints => specialMovePoints;

        /// <summary>
        /// Points for causing a knockdown.
        /// </summary>
        public float KnockdownPoints => knockdownPoints;

        /// <summary>
        /// Multiplier for total damage dealt.
        /// </summary>
        public float DamageDealtWeight => damageDealtWeight;

        /// <summary>
        /// Points per successful block.
        /// </summary>
        public float BlockPoints => blockPoints;

        /// <summary>
        /// Points per successful parry.
        /// </summary>
        public float ParryPoints => parryPoints;

        /// <summary>
        /// Points per successful dodge.
        /// </summary>
        public float DodgePoints => dodgePoints;

        /// <summary>
        /// Points awarded per second spent in offensive range.
        /// </summary>
        public float AggressionPointsPerSecond => aggressionPointsPerSecond;

        /// <summary>
        /// Bonus points for controlling the center of the ring.
        /// </summary>
        public float RingControlBonus => ringControlBonus;

        /// <summary>
        /// Points deducted per exhaustion occurrence.
        /// </summary>
        public float ExhaustionPenalty => exhaustionPenalty;

        /// <summary>
        /// Small penalty for whiffed attacks.
        /// </summary>
        public float MissedAttackPenalty => missedAttackPenalty;

        private void OnValidate()
        {
            // Clamp offensive weights to non-negative values
            cleanHitPoints = Mathf.Max(0f, cleanHitPoints);
            comboHitBonus = Mathf.Max(0f, comboHitBonus);
            comboSequencePoints = Mathf.Max(0f, comboSequencePoints);
            specialMovePoints = Mathf.Max(0f, specialMovePoints);
            knockdownPoints = Mathf.Max(0f, knockdownPoints);
            damageDealtWeight = Mathf.Max(0f, damageDealtWeight);

            // Clamp defensive weights to non-negative values
            blockPoints = Mathf.Max(0f, blockPoints);
            parryPoints = Mathf.Max(0f, parryPoints);
            dodgePoints = Mathf.Max(0f, dodgePoints);

            // Clamp control weights to non-negative values
            aggressionPointsPerSecond = Mathf.Max(0f, aggressionPointsPerSecond);
            ringControlBonus = Mathf.Max(0f, ringControlBonus);

            // Clamp penalties to non-negative values
            exhaustionPenalty = Mathf.Max(0f, exhaustionPenalty);
            missedAttackPenalty = Mathf.Max(0f, missedAttackPenalty);
        }
    }
}
