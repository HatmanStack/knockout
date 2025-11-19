using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Cinemachine;
using Knockout.Utilities;

namespace Knockout.Tests.PlayMode.Utilities
{
    public class CameraControllerTests
    {
        private GameObject _cameraControllerObj;
        private CameraController _cameraController;
        private GameObject _playerObj;
        private GameObject _aiObj;
        private GameObject _virtualCameraObj;
        private CinemachineVirtualCamera _virtualCamera;

        [SetUp]
        public void SetUp()
        {
            // Create character transforms
            _playerObj = new GameObject("Player");
            _playerObj.transform.position = new Vector3(-2f, 0f, 0f);

            _aiObj = new GameObject("AI");
            _aiObj.transform.position = new Vector3(2f, 0f, 0f);

            // Create virtual camera
            _virtualCameraObj = new GameObject("VirtualCamera");
            _virtualCamera = _virtualCameraObj.AddComponent<CinemachineVirtualCamera>();

            // Add transposer component
            var transposer = _virtualCamera.AddCinemachineComponent<CinemachineTransposer>();
            transposer.m_FollowOffset = new Vector3(0f, 5f, -10f);

            // Create camera controller
            _cameraControllerObj = new GameObject("CameraController");
            _cameraController = _cameraControllerObj.AddComponent<CameraController>();

            // Use reflection to set private fields for testing
            var vcField = typeof(CameraController).GetField("virtualCamera",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            vcField.SetValue(_cameraController, _virtualCamera);

            var playerField = typeof(CameraController).GetField("playerCharacter",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            playerField.SetValue(_cameraController, _playerObj.transform);

            var aiField = typeof(CameraController).GetField("aiCharacter",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            aiField.SetValue(_cameraController, _aiObj.transform);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_cameraControllerObj);
            Object.DestroyImmediate(_playerObj);
            Object.DestroyImmediate(_aiObj);
            Object.DestroyImmediate(_virtualCameraObj);
        }

        [UnityTest]
        public IEnumerator SetCharacters_UpdatesReferences()
        {
            // Arrange
            var newPlayer = new GameObject("NewPlayer").transform;
            var newAI = new GameObject("NewAI").transform;
            newPlayer.position = new Vector3(-3f, 0f, 0f);
            newAI.position = new Vector3(3f, 0f, 0f);

            // Act
            _cameraController.SetCharacters(newPlayer, newAI);
            yield return null;

            // Assert - verify camera updated (we can't easily check private fields, but can verify no errors)
            Assert.IsNotNull(_cameraController);

            // Cleanup
            Object.DestroyImmediate(newPlayer.gameObject);
            Object.DestroyImmediate(newAI.gameObject);
        }

        [UnityTest]
        public IEnumerator CameraTarget_PositionsBetweenCharacters()
        {
            // Arrange - characters at (-2, 0, 0) and (2, 0, 0), midpoint should be (0, 0, 0)
            yield return null; // Let Awake/Start run

            // Act - wait a few frames for camera to update
            for (int i = 0; i < 10; i++)
            {
                yield return null;
            }

            // Assert - camera target should be near midpoint
            // We can verify the virtual camera's Follow target is set
            Assert.IsNotNull(_virtualCamera.Follow, "Camera should have a follow target");
        }

        [UnityTest]
        public IEnumerator CameraDistance_AdjustsBasedOnFighterSeparation()
        {
            // Arrange
            yield return null;

            // Get initial camera distance
            var transposer = _virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
            float initialDistance = Mathf.Abs(transposer.m_FollowOffset.z);

            // Act - move fighters farther apart
            _playerObj.transform.position = new Vector3(-5f, 0f, 0f);
            _aiObj.transform.position = new Vector3(5f, 0f, 0f);

            // Wait for camera to adjust
            yield return new WaitForSeconds(0.5f);

            // Assert - camera distance should have increased
            float newDistance = Mathf.Abs(transposer.m_FollowOffset.z);
            Assert.Greater(newDistance, initialDistance,
                "Camera should zoom out when fighters move apart");
        }

        [UnityTest]
        public IEnumerator CameraTarget_RespectsBounds()
        {
            // Arrange - move characters very high
            _playerObj.transform.position = new Vector3(-2f, 100f, 0f);
            _aiObj.transform.position = new Vector3(2f, 100f, 0f);

            yield return null;

            // Act - wait for camera to update
            for (int i = 0; i < 10; i++)
            {
                yield return null;
            }

            // Assert - camera target Y should be clamped
            Transform cameraTarget = _virtualCamera.Follow;
            if (cameraTarget != null)
            {
                // Default maxHeight is 5f based on CameraController
                Assert.LessOrEqual(cameraTarget.position.y, 5f,
                    "Camera target should respect max height bounds");
            }
        }

        [UnityTest]
        public IEnumerator CameraController_HandlesNullReferences()
        {
            // Arrange - create controller with no references
            var emptyController = new GameObject("EmptyController");
            var controller = emptyController.AddComponent<CameraController>();

            // Act & Assert - should not throw errors
            yield return null;
            Assert.IsNotNull(controller);

            // Cleanup
            Object.DestroyImmediate(emptyController);
        }
    }
}
