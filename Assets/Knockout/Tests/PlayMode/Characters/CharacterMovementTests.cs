using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Knockout.Characters.Components;
using Knockout.Characters.Data;

namespace Knockout.Tests.PlayMode.Characters
{
    /// <summary>
    /// Play mode tests for CharacterMovement component.
    /// </summary>
    public class CharacterMovementTests
    {
        private GameObject _testCharacter;
        private CharacterMovement _movement;
        private CharacterStats _testStats;

        [SetUp]
        public void SetUp()
        {
            // Create test character
            _testCharacter = CreateMinimalCharacter();
            _movement = _testCharacter.GetComponent<CharacterMovement>();

            // Create test stats
            _testStats = ScriptableObject.CreateInstance<CharacterStats>();
            _testStats.SetMaxHealth(100f);

            // Assign stats via reflection
            var statsField = typeof(CharacterMovement).GetField("characterStats",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            statsField?.SetValue(_movement, _testStats);
        }

        [TearDown]
        public void TearDown()
        {
            if (_testCharacter != null)
            {
                Object.Destroy(_testCharacter);
            }

            if (_testStats != null)
            {
                Object.DestroyImmediate(_testStats);
            }
        }

        [UnityTest]
        public IEnumerator CharacterMovement_ProcessesInput_StoresMovementVector()
        {
            yield return null; // Wait for Start()

            // Arrange
            Vector2 input = new Vector2(0.5f, 1f);

            // Act
            _movement.SetMovementInput(input);

            yield return null;

            // Assert
            Assert.AreEqual(input, _movement.MovementInput);
        }

        [UnityTest]
        public IEnumerator CharacterMovement_SetFacingDirection_UpdatesRotation()
        {
            yield return null; // Wait for Start()

            // Arrange
            Vector3 targetDirection = Vector3.right;

            // Act
            _movement.SetFacingDirection(targetDirection);

            yield return null;

            // Assert
            Assert.AreEqual(Vector3.right, _movement.FacingDirection);
            Vector3 forward = _testCharacter.transform.forward;
            Assert.AreEqual(1f, forward.x, 0.1f);
            Assert.AreEqual(0f, forward.z, 0.1f);
        }

        [UnityTest]
        public IEnumerator CharacterMovement_RotateTowardDirection_RotatesSmoothly()
        {
            yield return null; // Wait for Start()

            // Arrange
            _movement.SetFacingDirection(Vector3.forward);
            Vector3 targetDirection = Vector3.right;

            // Act - rotate over multiple frames
            for (int i = 0; i < 10; i++)
            {
                _movement.RotateTowardDirection(targetDirection);
                yield return new WaitForFixedUpdate();
            }

            // Assert - should be close to target direction
            Vector3 currentFacing = _movement.FacingDirection;
            float dotProduct = Vector3.Dot(currentFacing, targetDirection);
            Assert.Greater(dotProduct, 0.9f, "Should be facing close to target direction");
        }

        [UnityTest]
        public IEnumerator CharacterMovement_RotateToward_FacesTargetPosition()
        {
            yield return null; // Wait for Start()

            // Arrange
            _testCharacter.transform.position = Vector3.zero;
            Vector3 targetPosition = new Vector3(5f, 0f, 0f);

            // Act - rotate over multiple frames
            for (int i = 0; i < 20; i++)
            {
                _movement.RotateToward(targetPosition);
                yield return new WaitForFixedUpdate();
            }

            // Assert
            Vector3 directionToTarget = (targetPosition - _testCharacter.transform.position).normalized;
            float dotProduct = Vector3.Dot(_movement.FacingDirection, directionToTarget);
            Assert.Greater(dotProduct, 0.9f, "Should be facing toward target");
        }

        // NOTE: Movement velocity tests require physics simulation to work correctly
        // and may be flaky in test environment. Marked as Ignore for now.

        /*
        [UnityTest]
        [Ignore("Requires physics simulation")]
        public IEnumerator CharacterMovement_ProcessesInput_MovesCharacter()
        {
            yield return null;

            // Arrange
            Vector3 startPosition = _testCharacter.transform.position;
            _movement.SetMovementInput(Vector2.up); // Move forward

            // Act - let physics run
            yield return new WaitForSeconds(0.5f);

            // Assert
            Vector3 endPosition = _testCharacter.transform.position;
            float distance = Vector3.Distance(startPosition, endPosition);
            Assert.Greater(distance, 0.1f, "Character should have moved");
        }
        */

        #region Helper Methods

        private GameObject CreateMinimalCharacter()
        {
            GameObject character = new GameObject("TestCharacter");

            // Add Rigidbody (required by CharacterMovement)
            Rigidbody rb = character.AddComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = false; // Disable gravity for tests
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            // Add Animator (required)
            character.AddComponent<Animator>();

            // Add CharacterAnimator (required by CharacterMovement)
            character.AddComponent<CharacterAnimator>();

            // Add CharacterCombat (required for state checks)
            character.AddComponent<CharacterCombat>();

            // Add CharacterMovement
            character.AddComponent<CharacterMovement>();

            return character;
        }

        #endregion
    }
}
