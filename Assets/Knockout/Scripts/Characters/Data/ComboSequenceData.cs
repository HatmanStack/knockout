using UnityEngine;

namespace Knockout.Characters.Data
{
    /// <summary>
    /// ScriptableObject defining a predefined combo sequence with attack chain and bonuses.
    /// Sequences grant damage bonuses and special effects when completed successfully.
    /// </summary>
    [CreateAssetMenu(fileName = "ComboSequenceData", menuName = "Knockout/Combo Sequence Data", order = 6)]
    public class ComboSequenceData : ScriptableObject
    {
        [Header("Sequence Identity")]
        [SerializeField]
        [Tooltip("Name of this combo sequence (e.g., '1-2 Combo', 'Hook Finisher')")]
        private string sequenceName = "Combo Sequence";

        [Header("Attack Sequence")]
        [SerializeField]
        [Tooltip("Required attack sequence (0=Jab, 1=Hook, 2=Uppercut). Example: [0, 0, 1] = Jab-Jab-Hook")]
        private int[] attackSequence = new int[] { 0, 0 };

        [SerializeField]
        [Tooltip("Timing window in frames (at 60fps) for completing the sequence. Tighter than natural chain windows.")]
        private int timingWindowFrames = 15;

        [Header("Bonuses")]
        [SerializeField]
        [Tooltip("Damage multiplier bonus applied to final hit AFTER combo scaling (1.0 = no bonus, 2.0 = double damage)")]
        private float damageBonusMultiplier = 1.25f;

        [Header("Special Properties")]
        [SerializeField]
        [Tooltip("Enhanced knockback applied on sequence completion")]
        private bool enhancedKnockback = false;

        [SerializeField]
        [Tooltip("Knockback multiplier if enhanced knockback is enabled")]
        private float knockbackMultiplier = 1.5f;

        [SerializeField]
        [Tooltip("Guaranteed stagger on sequence completion (interrupts defender)")]
        private bool guaranteedStagger = false;

        [Header("Visual/Audio (Optional)")]
        [SerializeField]
        [Tooltip("VFX prefab to spawn on sequence completion (optional)")]
        private GameObject vfxPrefab = null;

        [SerializeField]
        [Tooltip("Audio clip to play on sequence completion (optional)")]
        private AudioClip soundClip = null;

        // Read-only public properties
        public string SequenceName => sequenceName;
        public int[] AttackSequence => attackSequence;
        public int TimingWindowFrames => timingWindowFrames;
        public float DamageBonusMultiplier => damageBonusMultiplier;
        public bool EnhancedKnockback => enhancedKnockback;
        public float KnockbackMultiplier => knockbackMultiplier;
        public bool GuaranteedStagger => guaranteedStagger;
        public GameObject VfxPrefab => vfxPrefab;
        public AudioClip SoundClip => soundClip;

        /// <summary>
        /// Number of attacks in this sequence.
        /// </summary>
        public int SequenceLength => attackSequence != null ? attackSequence.Length : 0;

        /// <summary>
        /// Timing window in seconds (at 60fps).
        /// </summary>
        public float TimingWindowSeconds => timingWindowFrames / 60f;

        private void OnValidate()
        {
            // Ensure sequence has at least 2 attacks
            if (attackSequence == null || attackSequence.Length < 2)
            {
                attackSequence = new int[] { 0, 0 }; // Default to Jab-Jab
            }

            // Validate attack type indices (0-2 for Jab, Hook, Uppercut)
            for (int i = 0; i < attackSequence.Length; i++)
            {
                attackSequence[i] = Mathf.Clamp(attackSequence[i], 0, 2);
            }

            // Clamp damage bonus to reasonable range (1.0-3.0x)
            damageBonusMultiplier = Mathf.Clamp(damageBonusMultiplier, 1.0f, 3.0f);

            // Clamp knockback multiplier to reasonable range
            knockbackMultiplier = Mathf.Clamp(knockbackMultiplier, 1.0f, 3.0f);

            // Ensure timing window is positive
            timingWindowFrames = Mathf.Max(1, timingWindowFrames);
        }
    }
}
