using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Knockout.Combat.States;
using Knockout.Characters.Components;

namespace Knockout.Tests.PlayMode.Combat
{
    /// <summary>
    /// PlayMode tests for SpecialKnockdownState.
    /// </summary>
    [TestFixture]
    public class SpecialKnockdownStateTests
    {
        private GameObject _characterObject;
        private CharacterCombat _combat;

        [SetUp]
        public void SetUp()
        {
            // Create character with required components
            _characterObject = new GameObject("TestCharacter");

            // Add Animator and CharacterAnimator
            _characterObject.AddComponent<Animator>();
            _characterObject.AddComponent<CharacterAnimator>();

            // Add CharacterCombat
            _combat = _characterObject.AddComponent<CharacterCombat>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_characterObject != null)
            {
                Object.DestroyImmediate(_characterObject);
            }
        }

        [UnityTest]
        public IEnumerator SpecialKnockdownState_EntersSuccessfully()
        {
            yield return null; // Wait for Start()

            // Arrange
            var specialKnockdownState = new SpecialKnockdownState();

            // Act
            specialKnockdownState.Enter(_combat);

            yield return null;

            // Assert
            Assert.IsFalse(specialKnockdownState.CanGetUp, "Should not be able to get up immediately");
            Assert.AreEqual(0f, specialKnockdownState.RecoveryTimer, 0.01f, "Timer should start at 0");
        }

        [UnityTest]
        public IEnumerator SpecialKnockdownState_FiresEventsOnEnterAndExit()
        {
            yield return null; // Wait for Start()

            // Arrange
            bool startEventFired = false;
            bool endEventFired = false;

            SpecialKnockdownState.OnSpecialKnockdownStart += (combat) => startEventFired = true;
            SpecialKnockdownState.OnSpecialKnockdownEnd += (combat) => endEventFired = true;

            var specialKnockdownState = new SpecialKnockdownState();

            // Act
            specialKnockdownState.Enter(_combat);
            yield return null;

            specialKnockdownState.Exit(_combat);
            yield return null;

            // Assert
            Assert.IsTrue(startEventFired, "OnSpecialKnockdownStart should fire on Enter");
            Assert.IsTrue(endEventFired, "OnSpecialKnockdownEnd should fire on Exit");

            // Cleanup - unsubscribe
            SpecialKnockdownState.OnSpecialKnockdownStart -= (combat) => startEventFired = true;
            SpecialKnockdownState.OnSpecialKnockdownEnd -= (combat) => endEventFired = true;
        }

        [UnityTest]
        public IEnumerator SpecialKnockdownState_RecoveryTimerIncrements()
        {
            yield return null; // Wait for Start()

            // Arrange
            var specialKnockdownState = new SpecialKnockdownState();
            specialKnockdownState.Enter(_combat);

            // Act - simulate Update() calls
            specialKnockdownState.Update(_combat);
            yield return new WaitForSeconds(0.5f);
            specialKnockdownState.Update(_combat);

            // Assert
            Assert.Greater(specialKnockdownState.RecoveryTimer, 0f, "Timer should increment during Update");
        }

        [UnityTest]
        public IEnumerator SpecialKnockdownState_CanGetUpAfterRecoveryDuration()
        {
            yield return null; // Wait for Start()

            // Arrange - set short recovery duration for testing
            var specialKnockdownState = new SpecialKnockdownState();
            specialKnockdownState.SetRecoveryDuration(0.2f);
            specialKnockdownState.Enter(_combat);

            // Act - wait for recovery duration
            yield return new WaitForSeconds(0.25f);
            specialKnockdownState.Update(_combat);

            // Assert
            Assert.IsTrue(specialKnockdownState.CanGetUp, "Should be able to get up after recovery duration");
        }

        [UnityTest]
        public IEnumerator SpecialKnockdownState_CanTransitionToIdleAfterRecovery()
        {
            yield return null; // Wait for Start()

            // Arrange
            var specialKnockdownState = new SpecialKnockdownState();
            specialKnockdownState.SetRecoveryDuration(0.1f);
            specialKnockdownState.Enter(_combat);

            // Wait for recovery
            yield return new WaitForSeconds(0.15f);
            specialKnockdownState.Update(_combat);

            var idleState = new IdleState();

            // Act
            bool canTransition = specialKnockdownState.CanTransitionTo(idleState);

            // Assert
            Assert.IsTrue(canTransition, "Should be able to transition to Idle after recovery");

            // Cleanup
            specialKnockdownState.Exit(_combat);
        }

        [UnityTest]
        public IEnumerator SpecialKnockdownState_CannotTransitionToAttackingState()
        {
            yield return null; // Wait for Start()

            // Arrange
            var specialKnockdownState = new SpecialKnockdownState();
            specialKnockdownState.Enter(_combat);

            var attackingState = new AttackingState();

            // Act
            bool canTransition = specialKnockdownState.CanTransitionTo(attackingState);

            // Assert
            Assert.IsFalse(canTransition, "Cannot attack while in special knockdown");

            // Cleanup
            specialKnockdownState.Exit(_combat);
        }

        [UnityTest]
        public IEnumerator SpecialKnockdownState_CanTransitionToKnockedOutState()
        {
            yield return null; // Wait for Start()

            // Arrange
            var specialKnockdownState = new SpecialKnockdownState();
            specialKnockdownState.Enter(_combat);

            var knockedOutState = new KnockedOutState();

            // Act
            bool canTransition = specialKnockdownState.CanTransitionTo(knockedOutState);

            // Assert
            Assert.IsTrue(canTransition, "Can be knocked out from special knockdown");

            // Cleanup
            specialKnockdownState.Exit(_combat);
        }

        [UnityTest]
        public IEnumerator SpecialKnockdownState_LongerRecoveryThanNormalKnockdown()
        {
            yield return null; // Wait for Start()

            // Arrange
            var specialKnockdownState = new SpecialKnockdownState();

            // Act - use default recovery duration
            specialKnockdownState.Enter(_combat);

            // Assert
            // Special knockdown should have longer recovery than normal knockdown (~2-3s)
            // Default is 4s
            Assert.GreaterOrEqual(SpecialKnockdownState.DEFAULT_RECOVERY_DURATION, 3f,
                "Special knockdown should have longer recovery than normal knockdown");

            // Cleanup
            specialKnockdownState.Exit(_combat);
        }
    }
}
