using UnityEngine;

namespace Knockout.Characters.Data
{
    /// <summary>
    /// ScriptableObject containing special move configuration including damage, costs, and cooldown.
    /// Special moves are powerful, signature techniques with dual-resource requirements (cooldown + stamina).
    /// </summary>
    [CreateAssetMenu(fileName = "SpecialMoveData", menuName = "Knockout/Special Move Data", order = 8)]
    public class SpecialMoveData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        [Tooltip("Name of this special move (e.g., 'Haymaker', 'Liver Shot')")]
        private string specialMoveName = "Special Move";

        [Header("Base Attack Reference")]
        [SerializeField]
        [Tooltip("Base attack data used as template for animation and timing")]
        private AttackData baseAttackData;

        [Header("Damage Modifiers")]
        [SerializeField]
        [Tooltip("Damage multiplier applied to base attack damage (e.g., 2.0 = double damage)")]
        private float damageMultiplier = 2.0f;

        [SerializeField]
        [Tooltip("Knockback multiplier applied to base attack knockback")]
        private float knockbackMultiplier = 2.5f;

        [Header("Resource Costs")]
        [SerializeField]
        [Tooltip("Cooldown duration in seconds before special move can be used again")]
        private float cooldownSeconds = 45f;

        [SerializeField]
        [Tooltip("Stamina cost to execute this special move")]
        private float staminaCost = 40f;

        [Header("Special Knockdown")]
        [SerializeField]
        [Tooltip("Whether this special move triggers enhanced knockdown state")]
        private bool triggersSpecialKnockdown = true;

        [SerializeField]
        [Tooltip("Duration of special knockdown in seconds (if triggered)")]
        private float specialKnockdownDuration = 3.0f;

        [Header("Animation")]
        [SerializeField]
        [Tooltip("Animation trigger name (or uses base attack trigger if empty)")]
        private string animationTrigger = "";

        [Header("Visual/Audio (Optional)")]
        [SerializeField]
        [Tooltip("Unique visual effect identifier for this special move")]
        private string visualEffectId = "";

        [SerializeField]
        [Tooltip("Unique audio effect identifier for this special move")]
        private string audioEffectId = "";

        // Read-only public properties
        /// <summary>
        /// Name of this special move.
        /// </summary>
        public string SpecialMoveName => specialMoveName;

        /// <summary>
        /// Base attack data used as template for this special move.
        /// </summary>
        public AttackData BaseAttackData => baseAttackData;

        /// <summary>
        /// Damage multiplier applied to base attack damage.
        /// </summary>
        public float DamageMultiplier => damageMultiplier;

        /// <summary>
        /// Knockback multiplier applied to base attack knockback.
        /// </summary>
        public float KnockbackMultiplier => knockbackMultiplier;

        /// <summary>
        /// Cooldown duration in seconds.
        /// </summary>
        public float CooldownSeconds => cooldownSeconds;

        /// <summary>
        /// Stamina cost to execute this special move.
        /// </summary>
        public float StaminaCost => staminaCost;

        /// <summary>
        /// Whether this special move triggers enhanced knockdown state.
        /// </summary>
        public bool TriggersSpecialKnockdown => triggersSpecialKnockdown;

        /// <summary>
        /// Duration of special knockdown in seconds (if triggered).
        /// </summary>
        public float SpecialKnockdownDuration => specialKnockdownDuration;

        /// <summary>
        /// Animation trigger name (uses base attack trigger if empty).
        /// </summary>
        public string AnimationTrigger => string.IsNullOrEmpty(animationTrigger) && baseAttackData != null
            ? baseAttackData.AnimationTrigger
            : animationTrigger;

        /// <summary>
        /// Visual effect identifier for this special move.
        /// </summary>
        public string VisualEffectId => visualEffectId;

        /// <summary>
        /// Audio effect identifier for this special move.
        /// </summary>
        public string AudioEffectId => audioEffectId;

        private void OnValidate()
        {
            // Ensure cooldown is positive
            cooldownSeconds = Mathf.Max(0.1f, cooldownSeconds);

            // Ensure stamina cost is positive
            staminaCost = Mathf.Max(0.1f, staminaCost);

            // Clamp multipliers to reasonable ranges [1.0, 3.0]
            damageMultiplier = Mathf.Clamp(damageMultiplier, 1.0f, 3.0f);
            knockbackMultiplier = Mathf.Clamp(knockbackMultiplier, 1.0f, 3.0f);

            // Ensure special knockdown duration is non-negative
            specialKnockdownDuration = Mathf.Max(0f, specialKnockdownDuration);
        }
    }
}
