using NUnit.Framework;
using UnityEngine;
using Knockout.AI;

namespace Knockout.Tests.EditMode.AI
{
    /// <summary>
    /// Edit mode tests for AI state machine and context.
    /// </summary>
    public class AIStateMachineTests
    {
        [Test]
        public void AIContext_UpdateFrom_CalculatesDistanceToPlayer()
        {
            // Arrange
            var context = new AIContext();
            var aiPos = Vector3.zero;
            var playerPos = new Vector3(5f, 0f, 0f);

            // Act
            context.UpdateFrom(aiPos, playerPos, 100f, 100f, false);

            // Assert
            Assert.AreEqual(5f, context.DistanceToPlayer, 0.1f);
        }

        [Test]
        public void AIContext_UpdateFrom_CalculatesHealthPercentages()
        {
            // Arrange
            var context = new AIContext();
            var aiPos = Vector3.zero;
            var playerPos = Vector3.zero;

            // Act
            context.UpdateFrom(aiPos, playerPos, 50f, 100f, false);

            // Assert
            Assert.AreEqual(50f, context.OwnHealthPercentage, 0.1f);
            Assert.AreEqual(100f, context.PlayerHealthPercentage, 0.1f);
        }

        [Test]
        public void AIContext_UpdateFrom_TracksPlayerAttacking()
        {
            // Arrange
            var context = new AIContext();
            var aiPos = Vector3.zero;
            var playerPos = Vector3.zero;

            // Act - player attacking
            context.UpdateFrom(aiPos, playerPos, 100f, 100f, true);

            // Assert
            Assert.IsTrue(context.PlayerIsAttacking);
        }

        [Test]
        public void AIContext_UpdateFrom_TracksTimeSinceLastStateChange()
        {
            // Arrange
            var context = new AIContext();
            var aiPos = Vector3.zero;
            var playerPos = Vector3.zero;

            // Act - first update
            context.UpdateFrom(aiPos, playerPos, 100f, 100f, false);
            float firstTime = context.TimeSinceLastStateChange;

            // Simulate time passing
            context.TimeSinceLastStateChange = 1.5f;

            // Assert
            Assert.AreEqual(1.5f, context.TimeSinceLastStateChange, 0.01f);
        }
    }
}
