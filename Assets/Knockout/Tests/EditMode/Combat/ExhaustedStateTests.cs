using NUnit.Framework;
using UnityEngine;
using Knockout.Combat.States;
using Knockout.Characters.Components;
using Knockout.Characters.Data;

namespace Knockout.Tests.EditMode.Combat
{
    /// <summary>
    /// EditMode tests for ExhaustedState logic.
    /// </summary>
    [TestFixture]
    public class ExhaustedStateTests
    {
        [Test]
        public void ExhaustedState_CannotTransitionToAttackingState()
        {
            // Arrange
            var exhaustedState = new ExhaustedState();
            var attackingState = new AttackingState();

            // Act
            bool canTransition = exhaustedState.CanTransitionTo(attackingState);

            // Assert
            Assert.IsFalse(canTransition, "Should not allow transition to AttackingState while exhausted");
        }

        [Test]
        public void ExhaustedState_AllowsTransitionToBlockingState()
        {
            // Arrange
            var exhaustedState = new ExhaustedState();
            var blockingState = new BlockingState();

            // Act
            bool canTransition = exhaustedState.CanTransitionTo(blockingState);

            // Assert
            Assert.IsTrue(canTransition, "Should allow transition to BlockingState (defensive action)");
        }

        [Test]
        public void ExhaustedState_AllowsTransitionToHitStunnedState()
        {
            // Arrange
            var exhaustedState = new ExhaustedState();
            var hitStunnedState = new HitStunnedState();

            // Act
            bool canTransition = exhaustedState.CanTransitionTo(hitStunnedState);

            // Assert
            Assert.IsTrue(canTransition, "Should allow transition to HitStunnedState (hit reaction)");
        }

        [Test]
        public void ExhaustedState_CannotStayInExhaustedState()
        {
            // Arrange
            var exhaustedState = new ExhaustedState();
            var anotherExhaustedState = new ExhaustedState();

            // Act
            bool canTransition = exhaustedState.CanTransitionTo(anotherExhaustedState);

            // Assert
            Assert.IsFalse(canTransition, "Should not allow staying in exhausted state");
        }

        [Test]
        public void ExhaustedState_HasCorrectStateName()
        {
            // Arrange
            var exhaustedState = new ExhaustedState();

            // Act
            string stateName = exhaustedState.StateName;

            // Assert
            Assert.AreEqual("ExhaustedState", stateName);
        }
    }
}
