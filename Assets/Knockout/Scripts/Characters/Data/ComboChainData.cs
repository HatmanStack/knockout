using UnityEngine;

namespace Knockout.Characters.Data
{
    /// <summary>
    /// ScriptableObject defining natural combo chain timing windows and damage scaling.
    /// Controls how attacks chain together and how combo damage scales.
    /// </summary>
    [CreateAssetMenu(fileName = "ComboChainData", menuName = "Knockout/Combo Chain Data", order = 7)]
    public class ComboChainData : ScriptableObject
    {
        [Header("Chain Timing Windows (at 60fps)")]
        [SerializeField]
        [Tooltip("Chain window for Jab attacks (frames). Default: 18 frames (~0.3s) - forgiving")]
        private int jabChainWindowFrames = 18;

        [SerializeField]
        [Tooltip("Chain window for Hook attacks (frames). Default: 12 frames (~0.2s) - medium")]
        private int hookChainWindowFrames = 12;

        [SerializeField]
        [Tooltip("Chain window for Uppercut attacks (frames). Default: 6 frames (~0.1s) - tight")]
        private int uppercutChainWindowFrames = 6;

        [SerializeField]
        [Tooltip("Maximum time between any hits before combo resets (frames). Default: 90 frames (~1.5s)")]
        private int globalComboTimeoutFrames = 90;

        [Header("Damage Scaling")]
        [SerializeField]
        [Tooltip("Damage multipliers by combo position (1st hit, 2nd hit, 3rd hit, etc.). Must be descending.")]
        private float[] damageScaling = new float[] { 1.0f, 0.75f, 0.5f, 0.5f };

        [Header("Combo Interruption")]
        [SerializeField]
        [Tooltip("Window in frames for defender to block and break combo. Default: 8 frames (~0.133s)")]
        private int comboBreakWindowFrames = 8;

        [SerializeField]
        [Tooltip("Damage bonus multiplier for first hit during parry counter window. Default: 1.25x")]
        private float counterWindowDamageBonus = 1.25f;

        // Read-only public properties
        public int JabChainWindowFrames => jabChainWindowFrames;
        public int HookChainWindowFrames => hookChainWindowFrames;
        public int UppercutChainWindowFrames => uppercutChainWindowFrames;
        public int GlobalComboTimeoutFrames => globalComboTimeoutFrames;
        public int ComboBreakWindowFrames => comboBreakWindowFrames;
        public float CounterWindowDamageBonus => counterWindowDamageBonus;

        /// <summary>
        /// Gets the chain window in frames for the specified attack type.
        /// </summary>
        /// <param name="attackTypeIndex">Attack type: 0=Jab, 1=Hook, 2=Uppercut</param>
        /// <returns>Chain window in frames</returns>
        public int GetChainWindow(int attackTypeIndex)
        {
            switch (attackTypeIndex)
            {
                case 0: return jabChainWindowFrames;
                case 1: return hookChainWindowFrames;
                case 2: return uppercutChainWindowFrames;
                default:
                    Debug.LogWarning($"Invalid attack type index: {attackTypeIndex}. Returning Jab window.");
                    return jabChainWindowFrames;
            }
        }

        /// <summary>
        /// Gets the damage scale multiplier for the specified combo hit number.
        /// </summary>
        /// <param name="comboHitNumber">Combo hit number (1-indexed: 1st hit, 2nd hit, etc.)</param>
        /// <returns>Damage multiplier (1.0 = full damage)</returns>
        public float GetDamageScale(int comboHitNumber)
        {
            if (comboHitNumber < 1)
            {
                Debug.LogWarning($"Invalid combo hit number: {comboHitNumber}. Must be >= 1. Returning 1.0.");
                return 1.0f;
            }

            // Convert to 0-indexed array position
            int index = comboHitNumber - 1;

            // If beyond array bounds, use last value (floor)
            if (index >= damageScaling.Length)
            {
                return damageScaling[damageScaling.Length - 1];
            }

            return damageScaling[index];
        }

        private void OnValidate()
        {
            // Ensure positive chain windows
            jabChainWindowFrames = Mathf.Max(1, jabChainWindowFrames);
            hookChainWindowFrames = Mathf.Max(1, hookChainWindowFrames);
            uppercutChainWindowFrames = Mathf.Max(1, uppercutChainWindowFrames);
            globalComboTimeoutFrames = Mathf.Max(1, globalComboTimeoutFrames);
            comboBreakWindowFrames = Mathf.Max(1, comboBreakWindowFrames);

            // Ensure damage scaling array exists and has at least 2 values
            if (damageScaling == null || damageScaling.Length < 2)
            {
                damageScaling = new float[] { 1.0f, 0.75f, 0.5f, 0.5f };
            }

            // Ensure damage scaling is descending and in valid range
            for (int i = 0; i < damageScaling.Length; i++)
            {
                // Clamp to valid range (0.1 to 1.0)
                damageScaling[i] = Mathf.Clamp(damageScaling[i], 0.1f, 1.0f);

                // Ensure descending order (each value <= previous)
                if (i > 0 && damageScaling[i] > damageScaling[i - 1])
                {
                    damageScaling[i] = damageScaling[i - 1];
                }
            }

            // Clamp counter window damage bonus to reasonable range
            counterWindowDamageBonus = Mathf.Clamp(counterWindowDamageBonus, 1.0f, 2.0f);
        }
    }
}
