using UnityEngine;

namespace Knockout.Characters.Data
{
    /// <summary>
    /// ScriptableObject containing attack properties and frame data.
    /// Defines damage, knockback, timing, and animation parameters for a specific attack.
    /// </summary>
    [CreateAssetMenu(fileName = "AttackData", menuName = "Knockout/Attack Data", order = 2)]
    public class AttackData : ScriptableObject
    {
        [Header("Attack Identity")]
        [SerializeField]
        [Tooltip("Name of this attack (e.g., 'Jab', 'Hook', 'Uppercut')")]
        private string attackName = "Attack";

        [Header("Damage Properties")]
        [SerializeField]
        [Tooltip("Base damage dealt by this attack")]
        private float damage = 10f;

        [SerializeField]
        [Tooltip("Knockback force applied to the target")]
        private float knockback = 0.5f;

        [Header("Frame Data (at 60fps)")]
        [SerializeField]
        [Tooltip("Number of frames before hitbox activates")]
        private int startupFrames = 6;

        [SerializeField]
        [Tooltip("Number of frames hitbox is active")]
        private int activeFrames = 3;

        [SerializeField]
        [Tooltip("Number of frames after hitbox deactivates until character can act again")]
        private int recoveryFrames = 6;

        [Header("Animation Properties")]
        [SerializeField]
        [Tooltip("Animator trigger parameter name for this attack")]
        private string animationTrigger = "AttackTrigger";

        [SerializeField]
        [Tooltip("Attack type index: 0=jab, 1=hook, 2=uppercut")]
        private int attackTypeIndex = 0;

        // Read-only public properties
        public string AttackName => attackName;
        public float Damage => damage;
        public float Knockback => knockback;
        public int StartupFrames => startupFrames;
        public int ActiveFrames => activeFrames;
        public int RecoveryFrames => recoveryFrames;
        public string AnimationTrigger => animationTrigger;
        public int AttackTypeIndex => attackTypeIndex;

        /// <summary>
        /// Total duration of the attack in frames (startup + active + recovery).
        /// </summary>
        public int TotalFrames => startupFrames + activeFrames + recoveryFrames;

        /// <summary>
        /// Total duration of the attack in seconds (at 60fps).
        /// </summary>
        public float TotalDuration => TotalFrames / 60f;
    }
}
