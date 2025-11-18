using UnityEngine;

namespace Knockout.AI
{
    /// <summary>
    /// Data structure containing all information needed for AI decision-making.
    /// Updated each frame and passed to AI states for evaluation.
    /// </summary>
    public struct AIContext
    {
        /// <summary>
        /// Reference to the player character GameObject.
        /// </summary>
        public GameObject PlayerTarget { get; set; }

        /// <summary>
        /// Current distance from AI to player.
        /// </summary>
        public float DistanceToPlayer { get; set; }

        /// <summary>
        /// AI character's current health as percentage (0-100).
        /// </summary>
        public float OwnHealthPercentage { get; set; }

        /// <summary>
        /// Player character's current health as percentage (0-100).
        /// </summary>
        public float PlayerHealthPercentage { get; set; }

        /// <summary>
        /// Whether the player is currently executing an attack.
        /// </summary>
        public bool PlayerIsAttacking { get; set; }

        /// <summary>
        /// Time in seconds since last state transition.
        /// </summary>
        public float TimeSinceLastStateChange { get; set; }

        /// <summary>
        /// AI position in world space.
        /// </summary>
        public Vector3 AIPosition { get; set; }

        /// <summary>
        /// Player position in world space.
        /// </summary>
        public Vector3 PlayerPosition { get; set; }

        /// <summary>
        /// Updates context with current game state information.
        /// </summary>
        /// <param name="aiPosition">Current AI position</param>
        /// <param name="playerPosition">Current player position</param>
        /// <param name="ownHealth">AI health percentage (0-100)</param>
        /// <param name="playerHealth">Player health percentage (0-100)</param>
        /// <param name="playerAttacking">Is player currently attacking</param>
        public void UpdateFrom(Vector3 aiPosition, Vector3 playerPosition,
            float ownHealth, float playerHealth, bool playerAttacking)
        {
            AIPosition = aiPosition;
            PlayerPosition = playerPosition;
            DistanceToPlayer = Vector3.Distance(aiPosition, playerPosition);
            OwnHealthPercentage = ownHealth;
            PlayerHealthPercentage = playerHealth;
            PlayerIsAttacking = playerAttacking;
        }
    }
}
