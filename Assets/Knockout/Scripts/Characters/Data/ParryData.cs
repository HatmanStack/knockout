using UnityEngine;

namespace Knockout.Characters.Data
{
    /// <summary>
    /// ScriptableObject containing parry configuration for timed perfect block.
    /// Defines parry timing window, stagger duration, and counter window.
    /// All timing values are frame-based for precision (60fps target).
    /// </summary>
    [CreateAssetMenu(fileName = "ParryData", menuName = "Knockout/Parry Data", order = 5)]
    public class ParryData : ScriptableObject
    {
        private const int TARGET_FRAME_RATE = 60;
        private const float FIXED_DELTA_TIME = 1f / TARGET_FRAME_RATE;

        [Header("Parry Timing (Frame-Based)")]
        [SerializeField]
        [Tooltip("Parry window in frames before hit connects (e.g., 6 frames = ~0.1s)")]
        private int parryWindowFrames = 6;

        [SerializeField]
        [Tooltip("Duration parry state lasts after successful parry in frames")]
        private int parrySuccessDurationFrames = 12;

        [SerializeField]
        [Tooltip("Cooldown before next parry attempt in frames (prevent spam)")]
        private int parryCooldownFrames = 18;

        [Header("Stagger Settings")]
        [SerializeField]
        [Tooltip("Duration attacker is staggered after being parried (seconds)")]
        private float attackerStaggerDuration = 0.5f;

        [SerializeField]
        [Tooltip("Duration defender has to counter attack (seconds, matches stagger)")]
        private float counterWindowDuration = 0.5f;

        #region Public Properties

        /// <summary>
        /// Parry window in frames before hit connects.
        /// </summary>
        public int ParryWindowFrames => parryWindowFrames;

        /// <summary>
        /// Duration parry state lasts after successful parry in frames.
        /// </summary>
        public int ParrySuccessDurationFrames => parrySuccessDurationFrames;

        /// <summary>
        /// Cooldown before next parry attempt in frames.
        /// </summary>
        public int ParryCooldownFrames => parryCooldownFrames;

        /// <summary>
        /// Duration attacker is staggered after being parried (seconds).
        /// </summary>
        public float AttackerStaggerDuration => attackerStaggerDuration;

        /// <summary>
        /// Duration defender has to counter attack (seconds).
        /// </summary>
        public float CounterWindowDuration => counterWindowDuration;

        /// <summary>
        /// Parry window duration in seconds.
        /// </summary>
        public float ParryWindow => parryWindowFrames * FIXED_DELTA_TIME;

        /// <summary>
        /// Parry success duration in seconds.
        /// </summary>
        public float ParrySuccessDuration => parrySuccessDurationFrames * FIXED_DELTA_TIME;

        /// <summary>
        /// Cooldown duration in seconds.
        /// </summary>
        public float CooldownDuration => parryCooldownFrames * FIXED_DELTA_TIME;

        #endregion

        #region Validation

        private void OnValidate()
        {
            // Ensure parry window is positive
            parryWindowFrames = Mathf.Max(1, parryWindowFrames);

            // Ensure success duration is positive
            parrySuccessDurationFrames = Mathf.Max(1, parrySuccessDurationFrames);

            // Ensure cooldown is non-negative
            parryCooldownFrames = Mathf.Max(0, parryCooldownFrames);

            // Ensure stagger duration is positive
            attackerStaggerDuration = Mathf.Max(0.1f, attackerStaggerDuration);

            // Ensure counter window is positive
            counterWindowDuration = Mathf.Max(0.1f, counterWindowDuration);

            // Counter window should not exceed stagger duration (makes sense gameplay-wise)
            if (counterWindowDuration > attackerStaggerDuration)
            {
                counterWindowDuration = attackerStaggerDuration;
            }
        }

        #endregion
    }
}
