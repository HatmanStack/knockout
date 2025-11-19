using NUnit.Framework;
using UnityEngine;
using Knockout.AI;

namespace Knockout.Tests.EditMode.AI
{
    /// <summary>
    /// Edit mode tests for AI target detection.
    /// </summary>
    public class AITargetDetectorTests
    {
        [SetUp]
        public void SetUp()
        {
            // Clear cache before each test
            AITargetDetector.ClearCache();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up any created objects
            var allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj.name == "Player" || obj.name == "TestObject")
                {
                    Object.DestroyImmediate(obj);
                }
            }
        }

        [Test]
        public void FindPlayerCharacter_ReturnsPlayer_WhenPlayerTagged()
        {
            // Arrange
            var player = new GameObject("Player");
            player.tag = "Player";

            // Act
            GameObject found = AITargetDetector.FindPlayerCharacter();

            // Assert
            Assert.IsNotNull(found);
            Assert.AreEqual(player, found);

            // Cleanup
            Object.DestroyImmediate(player);
        }

        [Test]
        public void FindPlayerCharacter_ReturnsNull_WhenNoPlayerExists()
        {
            // Arrange - no player created

            // Act
            GameObject found = AITargetDetector.FindPlayerCharacter();

            // Assert
            Assert.IsNull(found);
        }

        [Test]
        public void IsTargetValid_ReturnsTrue_ForValidTarget()
        {
            // Arrange
            var target = new GameObject("TestObject");

            // Act
            bool isValid = AITargetDetector.IsTargetValid(target);

            // Assert
            Assert.IsTrue(isValid);

            // Cleanup
            Object.DestroyImmediate(target);
        }

        [Test]
        public void IsTargetValid_ReturnsFalse_ForNullTarget()
        {
            // Act
            bool isValid = AITargetDetector.IsTargetValid(null);

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void ClearCache_ClearsPlayerCache()
        {
            // Arrange
            var player = new GameObject("Player");
            player.tag = "Player";

            // Find player (caches it)
            AITargetDetector.FindPlayerCharacter();

            // Act
            AITargetDetector.ClearCache();

            // Destroy the player
            Object.DestroyImmediate(player);

            // Try to find again (should return null since cache is cleared)
            GameObject found = AITargetDetector.FindPlayerCharacter();

            // Assert
            Assert.IsNull(found);
        }
    }
}
