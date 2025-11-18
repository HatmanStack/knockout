using UnityEngine;

namespace Knockout.Combat.HitDetection
{
    /// <summary>
    /// Data structure representing a hit event between an attacker and a target.
    /// Contains all information needed to calculate damage and trigger appropriate reactions.
    /// </summary>
    public struct HitData
    {
        /// <summary>
        /// The GameObject that initiated the attack.
        /// </summary>
        public GameObject Attacker;

        /// <summary>
        /// Base damage amount before modifiers.
        /// </summary>
        public float Damage;

        /// <summary>
        /// Knockback force applied to the target.
        /// </summary>
        public float Knockback;

        /// <summary>
        /// World position where the hit occurred.
        /// </summary>
        public Vector3 HitPoint;

        /// <summary>
        /// Direction of the hit (normalized vector from attacker to target).
        /// </summary>
        public Vector3 HitDirection;

        /// <summary>
        /// Type of hit: 0 = light, 1 = medium, 2 = heavy.
        /// </summary>
        public int HitType;

        /// <summary>
        /// Name of the attack that caused this hit (for debugging and effects).
        /// </summary>
        public string AttackName;

        /// <summary>
        /// Constructor to initialize all fields.
        /// </summary>
        public HitData(GameObject attacker, float damage, float knockback,
                       Vector3 hitPoint, Vector3 hitDirection, int hitType, string attackName)
        {
            Attacker = attacker;
            Damage = damage;
            Knockback = knockback;
            HitPoint = hitPoint;
            HitDirection = hitDirection;
            HitType = hitType;
            AttackName = attackName;
        }
    }
}
