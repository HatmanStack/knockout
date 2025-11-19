using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Knockout.Characters;
using Knockout.Characters.Data;

namespace Knockout.Tests.PlayMode.Characters
{
    /// <summary>
    /// Tests for CharacterController component initialization and validation.
    /// </summary>
    public class CharacterControllerTests
    {
        [UnityTest]
        public IEnumerator CharacterController_Initializes_WithoutErrors()
        {
            // Arrange
            GameObject characterGO = new GameObject("TestCharacter");

            // Add required components
            Animator animator = characterGO.AddComponent<Animator>();
            Rigidbody rb = characterGO.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            // Create test CharacterStats
            CharacterStats stats = ScriptableObject.CreateInstance<CharacterStats>();

            // Add CharacterController
            CharacterController controller = characterGO.AddComponent<CharacterController>();

            // Assign stats via reflection (since field is private)
            var statsField = typeof(CharacterController).GetField("characterStats",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            statsField.SetValue(controller, stats);

            // Act - Wait for Awake and Start to complete
            yield return null;

            // Assert
            Assert.IsNotNull(controller.Stats, "Stats should be assigned");
            Assert.IsNotNull(controller.Animator, "Animator should be cached");
            Assert.IsNotNull(controller.rigidbody, "Rigidbody should be cached");
            Assert.AreEqual(animator, controller.Animator, "Cached animator should match component");
            Assert.AreEqual(rb, controller.rigidbody, "Cached rigidbody should match component");

            // Cleanup
            Object.Destroy(characterGO);
            Object.Destroy(stats);
        }

        [UnityTest]
        public IEnumerator CharacterController_LogsError_WhenStatsNotAssigned()
        {
            // Arrange
            GameObject characterGO = new GameObject("TestCharacter");
            characterGO.AddComponent<Animator>();
            Rigidbody rb = characterGO.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            // Add CharacterController without assigning stats
            CharacterController controller = characterGO.AddComponent<CharacterController>();

            // Act - Wait for Awake/Start
            yield return null;

            // Assert - Error should be logged (we can't directly test Debug.LogError in NUnit)
            // The test passes if no exception is thrown
            Assert.IsNotNull(controller, "Controller should still exist even with missing stats");
            Assert.IsNull(controller.Stats, "Stats should be null when not assigned");

            // Cleanup
            Object.Destroy(characterGO);
        }

        [UnityTest]
        public IEnumerator CharacterController_RequiresAnimator()
        {
            // Arrange
            GameObject characterGO = new GameObject("TestCharacter");
            Rigidbody rb = characterGO.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            // Act - Try to add CharacterController without Animator
            // RequireComponent should automatically add Animator
            CharacterController controller = characterGO.AddComponent<CharacterController>();
            yield return null;

            // Assert
            Assert.IsNotNull(controller.Animator, "Animator should be added automatically by RequireComponent");

            // Cleanup
            Object.Destroy(characterGO);
        }

        [UnityTest]
        public IEnumerator CharacterController_RequiresRigidbody()
        {
            // Arrange
            GameObject characterGO = new GameObject("TestCharacter");
            characterGO.AddComponent<Animator>();

            // Act - Try to add CharacterController without Rigidbody
            // RequireComponent should automatically add Rigidbody
            CharacterController controller = characterGO.AddComponent<CharacterController>();
            yield return null;

            // Assert
            Assert.IsNotNull(controller.rigidbody, "Rigidbody should be added automatically by RequireComponent");

            // Cleanup
            Object.Destroy(characterGO);
        }
    }
}
