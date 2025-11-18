using UnityEngine;

namespace Knockout.AI
{
    /// <summary>
    /// Utility class for AI target detection and tracking.
    /// Provides methods to find player character and other potential targets.
    /// </summary>
    public static class AITargetDetector
    {
        // Cache for player character to avoid repeated searches
        private static GameObject _cachedPlayer;

        /// <summary>
        /// Finds the player character in the scene by tag.
        /// Uses caching for performance.
        /// </summary>
        /// <returns>Player character GameObject, or null if not found</returns>
        public static GameObject FindPlayerCharacter()
        {
            // Return cached player if still valid
            if (_cachedPlayer != null)
            {
                return _cachedPlayer;
            }

            // Search for player by tag
            _cachedPlayer = GameObject.FindWithTag("Player");

            if (_cachedPlayer == null)
            {
                Debug.LogWarning("[AITargetDetector] Could not find player character with tag 'Player'");
            }
            else
            {
                #if UNITY_EDITOR
                Debug.Log($"[AITargetDetector] Found player character: {_cachedPlayer.name}");
                #endif
            }

            return _cachedPlayer;
        }

        /// <summary>
        /// Finds the nearest character within a radius.
        /// Useful for future multi-opponent scenarios.
        /// </summary>
        /// <param name="position">Position to search from</param>
        /// <param name="radius">Search radius</param>
        /// <returns>Nearest character GameObject, or null if none found</returns>
        public static GameObject FindNearestCharacter(Vector3 position, float radius)
        {
            Collider[] colliders = Physics.OverlapSphere(position, radius);

            GameObject nearestCharacter = null;
            float nearestDistance = float.MaxValue;

            foreach (var collider in colliders)
            {
                // Check if this is a character (has CharacterController component or similar)
                var characterController = collider.GetComponent<Characters.CharacterController>();
                if (characterController != null)
                {
                    float distance = Vector3.Distance(position, collider.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestCharacter = collider.gameObject;
                    }
                }
            }

            return nearestCharacter;
        }

        /// <summary>
        /// Clears the cached player reference.
        /// Call this when scene changes or player is destroyed.
        /// </summary>
        public static void ClearCache()
        {
            _cachedPlayer = null;
        }

        /// <summary>
        /// Validates that a target is still valid (not destroyed).
        /// </summary>
        /// <param name="target">Target to validate</param>
        /// <returns>True if target is valid, false otherwise</returns>
        public static bool IsTargetValid(GameObject target)
        {
            return target != null;
        }
    }
}
