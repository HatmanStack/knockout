using UnityEngine;
using Cinemachine;

namespace Knockout.Utilities
{
    /// <summary>
    /// Controls the camera to follow both fighters, dynamically adjusting position and zoom
    /// to keep both characters in frame during combat.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Camera References")]
        [SerializeField] [Tooltip("Cinemachine virtual camera to control")]
        private CinemachineVirtualCamera virtualCamera;

        [SerializeField] [Tooltip("Empty transform that will track the midpoint between fighters")]
        private Transform cameraTarget;

        [Header("Character References")]
        [SerializeField] [Tooltip("Player character transform")]
        private Transform playerCharacter;

        [SerializeField] [Tooltip("AI character transform")]
        private Transform aiCharacter;

        [Header("Camera Settings")]
        [SerializeField] [Range(5f, 20f)] [Tooltip("Minimum camera distance from midpoint")]
        private float minDistance = 8f;

        [SerializeField] [Range(10f, 30f)] [Tooltip("Maximum camera distance from midpoint")]
        private float maxDistance = 15f;

        [SerializeField] [Range(0.5f, 3f)] [Tooltip("Distance multiplier for zoom calculation")]
        private float distanceMultiplier = 1.5f;

        [SerializeField] [Range(0.1f, 5f)] [Tooltip("Smooth camera movement speed")]
        private float smoothSpeed = 2f;

        [Header("Camera Bounds")]
        [SerializeField] [Tooltip("Minimum Y position for camera target")]
        private float minHeight = 0f;

        [SerializeField] [Tooltip("Maximum Y position for camera target")]
        private float maxHeight = 5f;

        private CinemachineTransposer _transposer;
        private Vector3 _targetPosition;

        private void Awake()
        {
            ValidateReferences();
            InitializeCamera();
        }

        private void ValidateReferences()
        {
            if (virtualCamera == null)
            {
                Debug.LogError("CameraController: Virtual camera reference is missing!");
            }

            if (cameraTarget == null)
            {
                Debug.LogWarning("CameraController: Camera target not assigned, creating one automatically.");
                GameObject targetObj = new GameObject("CameraTarget");
                cameraTarget = targetObj.transform;
            }

            if (playerCharacter == null || aiCharacter == null)
            {
                Debug.LogWarning("CameraController: Character references missing. Will attempt to find them at Start.");
            }
        }

        private void Start()
        {
            // Attempt to find characters if not assigned
            if (playerCharacter == null || aiCharacter == null)
            {
                FindCharacters();
            }
        }

        private void FindCharacters()
        {
            var characters = FindObjectsOfType<Characters.CharacterController>();

            if (characters.Length < 2)
            {
                Debug.LogError($"CameraController: Found {characters.Length} characters, need at least 2!");
                return;
            }

            // Assign based on tags or components
            foreach (var character in characters)
            {
                if (character.GetComponent<Characters.Components.CharacterInput>() != null)
                {
                    playerCharacter = character.transform;
                }
                else if (character.GetComponent<Characters.Components.CharacterAI>() != null)
                {
                    aiCharacter = character.transform;
                }
            }

            if (playerCharacter == null || aiCharacter == null)
            {
                Debug.LogError("CameraController: Could not identify player and AI characters!");
            }
        }

        private void InitializeCamera()
        {
            if (virtualCamera != null)
            {
                // Get the transposer component for distance adjustment
                _transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();

                if (_transposer == null)
                {
                    Debug.LogError("CameraController: Virtual camera missing CinemachineTransposer component!");
                }

                // Set camera to follow the target
                virtualCamera.Follow = cameraTarget;
                virtualCamera.LookAt = cameraTarget;
            }
        }

        private void LateUpdate()
        {
            if (playerCharacter == null || aiCharacter == null || cameraTarget == null)
            {
                return;
            }

            UpdateCameraTarget();
            UpdateCameraDistance();
        }

        private void UpdateCameraTarget()
        {
            // Calculate midpoint between fighters
            Vector3 midpoint = (playerCharacter.position + aiCharacter.position) * 0.5f;

            // Apply height bounds
            midpoint.y = Mathf.Clamp(midpoint.y, minHeight, maxHeight);

            // Smooth movement toward target position
            _targetPosition = Vector3.Lerp(cameraTarget.position, midpoint, Time.deltaTime * smoothSpeed);

            cameraTarget.position = _targetPosition;
        }

        private void UpdateCameraDistance()
        {
            if (_transposer == null)
            {
                return;
            }

            // Calculate distance between fighters
            float fighterDistance = Vector3.Distance(playerCharacter.position, aiCharacter.position);

            // Calculate desired camera distance based on fighter separation
            float desiredDistance = Mathf.Clamp(
                fighterDistance * distanceMultiplier,
                minDistance,
                maxDistance
            );

            // Smoothly adjust camera distance
            Vector3 currentOffset = _transposer.m_FollowOffset;
            float currentDistance = Mathf.Abs(currentOffset.z);
            float newDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * smoothSpeed);

            // Update the follow offset (assuming camera is behind the action on Z axis)
            currentOffset.z = -newDistance;
            _transposer.m_FollowOffset = currentOffset;
        }

        // Public method to manually set character references (useful for runtime setup)
        public void SetCharacters(Transform player, Transform ai)
        {
            playerCharacter = player;
            aiCharacter = ai;

            if (playerCharacter != null && aiCharacter != null)
            {
                // Immediately update camera to prevent jarring initial position
                UpdateCameraTarget();
            }
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (playerCharacter == null || aiCharacter == null)
            {
                return;
            }

            // Draw line between characters
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(playerCharacter.position, aiCharacter.position);

            // Draw midpoint sphere
            Vector3 midpoint = (playerCharacter.position + aiCharacter.position) * 0.5f;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(midpoint, 0.5f);

            // Draw camera bounds
            Gizmos.color = Color.cyan;
            Vector3 boundsCenter = midpoint;
            boundsCenter.y = (minHeight + maxHeight) * 0.5f;
            Vector3 boundsSize = new Vector3(0.1f, maxHeight - minHeight, 0.1f);
            Gizmos.DrawWireCube(boundsCenter, boundsSize);
        }
        #endif
    }
}
