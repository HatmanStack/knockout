using UnityEngine;

namespace Knockout.Characters.Data
{
    /// <summary>
    /// ScriptableObject containing dodge configuration including i-frame timing and movement properties.
    /// Defines dodge duration, invincibility frames, movement distance, and cooldown.
    /// All timing values are frame-based for precision (60fps target).
    /// </summary>
    [CreateAssetMenu(fileName = "DodgeData", menuName = "Knockout/Dodge Data", order = 4)]
    public class DodgeData : ScriptableObject
    {
        private const int TARGET_FRAME_RATE = 60;
        private const float FIXED_DELTA_TIME = 1f / TARGET_FRAME_RATE;

        [Header("Dodge Timing (Frame-Based)")]
        [SerializeField]
        [Tooltip("Total dodge animation duration in frames (e.g., 18 frames = ~0.3s at 60fps)")]
        private int dodgeDurationFrames = 18;

        [SerializeField]
        [Tooltip("Frame when invincibility starts (e.g., 2 frames = almost immediate)")]
        private int iFrameStartFrame = 2;

        [SerializeField]
        [Tooltip("Duration of invincibility frames (e.g., 8 frames = ~0.133s, 40% of total)")]
        private int iFrameDurationFrames = 8;

        [SerializeField]
        [Tooltip("Cooldown before next dodge in frames (e.g., 12 frames = ~0.2s to prevent spam)")]
        private int cooldownFrames = 12;

        [Header("Dodge Movement")]
        [SerializeField]
        [Tooltip("Distance character moves during dodge in world units")]
        private float dodgeDistance = 1.5f;

        [SerializeField]
        [Tooltip("Movement speed multiplier during dodge (applied to base movement speed)")]
        private float dodgeSpeedMultiplier = 1.0f;

        #region Public Properties

        /// <summary>
        /// Total dodge animation duration in frames.
        /// </summary>
        public int DodgeDurationFrames => dodgeDurationFrames;

        /// <summary>
        /// Frame when invincibility starts.
        /// </summary>
        public int IFrameStartFrame => iFrameStartFrame;

        /// <summary>
        /// Duration of invincibility frames.
        /// </summary>
        public int IFrameDurationFrames => iFrameDurationFrames;

        /// <summary>
        /// Cooldown before next dodge in frames.
        /// </summary>
        public int CooldownFrames => cooldownFrames;

        /// <summary>
        /// Distance character moves during dodge in world units.
        /// </summary>
        public float DodgeDistance => dodgeDistance;

        /// <summary>
        /// Movement speed multiplier during dodge.
        /// </summary>
        public float DodgeSpeedMultiplier => dodgeSpeedMultiplier;

        /// <summary>
        /// Total dodge duration in seconds (frame count converted to time).
        /// </summary>
        public float DodgeDuration => dodgeDurationFrames * FIXED_DELTA_TIME;

        /// <summary>
        /// Time when invincibility starts in seconds.
        /// </summary>
        public float IFrameStartTime => iFrameStartFrame * FIXED_DELTA_TIME;

        /// <summary>
        /// Duration of invincibility in seconds.
        /// </summary>
        public float IFrameDuration => iFrameDurationFrames * FIXED_DELTA_TIME;

        /// <summary>
        /// Cooldown duration in seconds.
        /// </summary>
        public float CooldownDuration => cooldownFrames * FIXED_DELTA_TIME;

        /// <summary>
        /// Frame when invincibility ends (start + duration).
        /// </summary>
        public int IFrameEndFrame => iFrameStartFrame + iFrameDurationFrames;

        /// <summary>
        /// Time when invincibility ends in seconds.
        /// </summary>
        public float IFrameEndTime => IFrameEndFrame * FIXED_DELTA_TIME;

        #endregion

        #region Validation

        private void OnValidate()
        {
            // Ensure dodge duration is positive
            dodgeDurationFrames = Mathf.Max(1, dodgeDurationFrames);

            // Ensure i-frame start is within dodge duration
            iFrameStartFrame = Mathf.Clamp(iFrameStartFrame, 0, dodgeDurationFrames - 1);

            // Ensure i-frame duration fits within dodge duration
            int maxIFrameDuration = dodgeDurationFrames - iFrameStartFrame;
            iFrameDurationFrames = Mathf.Clamp(iFrameDurationFrames, 0, maxIFrameDuration);

            // Ensure cooldown is non-negative
            cooldownFrames = Mathf.Max(0, cooldownFrames);

            // Ensure dodge distance is positive
            dodgeDistance = Mathf.Max(0.1f, dodgeDistance);

            // Ensure speed multiplier is positive
            dodgeSpeedMultiplier = Mathf.Max(0.1f, dodgeSpeedMultiplier);
        }

        #endregion
    }
}
