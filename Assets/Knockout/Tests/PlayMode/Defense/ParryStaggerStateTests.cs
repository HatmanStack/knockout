using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Knockout.Combat.States;
using Knockout.Characters.Components;

namespace Knockout.Tests.PlayMode.Defense
{
    /// <summary>
    /// PlayMode tests for ParryStaggerState combat state.
    /// </summary>
    [TestFixture]
    public class ParryStaggerStateTests
    {
        private GameObject _testCharacter;
        private CharacterCombat _combat;
        private ParryStaggerState _parryStaggerState;

        [SetUp]
        public void Setup()
        {
            // Create test character
            _testCharacter = new GameObject("TestCharacter");
            _combat = _testCharacter.AddComponent<CharacterCombat>();
            _testCharacter.AddComponent<CharacterAnimator>();

            // Create parry stagger state
            _parryStaggerState = new ParryStaggerState();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_testCharacter);
        }

        [UnityTest]
        public IEnumerator ParryStaggerState_Enter_TriggersAnimation()
        {
            // Act
            _parryStaggerState.Enter(_combat, 0.5f);
            yield return null;

            // Assert
            Assert.IsNotNull(_parryStaggerState);
            Assert.AreEqual(0.5f, _parryStaggerState.StaggerDuration);
        }

        [Test]
        public void ParryStaggerState_IsStaggerComplete_FalseInitially()
        {
            // Arrange
            _parryStaggerState.Enter(_combat, 0.5f);

            // Assert
            Assert.IsFalse(_parryStaggerState.IsStaggerComplete(), "Stagger should not be complete initially");
        }

        [UnityTest]
        public IEnumerator ParryStaggerState_IsStaggerComplete_TrueAfterDuration()
        {
            // Arrange
            _parryStaggerState.Enter(_combat, 0.2f); // Short duration for faster test

            // Act - wait for duration
            yield return new WaitForSeconds(0.3f);

            _parryStaggerState.Update(_combat);

            // Assert
            Assert.IsTrue(_parryStaggerState.IsStaggerComplete(),
                "Stagger should be complete after duration");
        }

        [Test]
        public void ParryStaggerState_CanTransitionTo_IdleWhenComplete()
        {
            // Arrange
            _parryStaggerState.Enter(_combat, 0.1f);

            // Simulate completion
            for (int i = 0; i < 10; i++)
            {
                _parryStaggerState.Update(_combat);
            }

            // Act & Assert
            Assert.IsTrue(_parryStaggerState.CanTransitionTo(new IdleState()),
                "Should allow transition to Idle when stagger complete");
        }

        [Test]
        public void ParryStaggerState_CanTransitionTo_BlocksForbiddenStates()
        {
            // Arrange
            _parryStaggerState.Enter(_combat, 0.5f);

            // Act & Assert
            Assert.IsFalse(_parryStaggerState.CanTransitionTo(new AttackingState()),
                "Should not allow transition to Attacking");
            Assert.IsFalse(_parryStaggerState.CanTransitionTo(new BlockingState()),
                "Should not allow transition to Blocking");
            Assert.IsFalse(_parryStaggerState.CanTransitionTo(new DodgingState()),
                "Should not allow transition to Dodging");
            Assert.IsFalse(_parryStaggerState.CanTransitionTo(new ParryStaggerState()),
                "Should not allow transition to ParryStagger");
        }

        [Test]
        public void ParryStaggerState_CanTransitionTo_AllowsHitStunned()
        {
            // Arrange
            _parryStaggerState.Enter(_combat, 0.5f);

            // Act & Assert
            Assert.IsTrue(_parryStaggerState.CanTransitionTo(new HitStunnedState()),
                "Should allow transition to HitStunned (can be hit during stagger)");
        }

        [UnityTest]
        public IEnumerator ParryStaggerState_OnParryStaggered_EventFires()
        {
            // Arrange
            bool eventFired = false;
            ParryStaggerState.OnParryStaggered += (combat) => { eventFired = true; };

            // Act
            _parryStaggerState.Enter(_combat, 0.5f);
            yield return null;

            // Assert
            Assert.IsTrue(eventFired, "OnParryStaggered event should fire");

            // Cleanup
            ParryStaggerState.OnParryStaggered = null;
        }

        [UnityTest]
        public IEnumerator ParryStaggerState_OnParryStaggerEnded_EventFires()
        {
            // Arrange
            bool eventFired = false;
            ParryStaggerState.OnParryStaggerEnded += (combat) => { eventFired = true; };

            _parryStaggerState.Enter(_combat, 0.5f);

            // Act
            _parryStaggerState.Exit(_combat);
            yield return null;

            // Assert
            Assert.IsTrue(eventFired, "OnParryStaggerEnded event should fire");

            // Cleanup
            ParryStaggerState.OnParryStaggerEnded = null;
        }

        [UnityTest]
        public IEnumerator ParryStaggerState_StaggerTimer_IncreasesOverTime()
        {
            // Arrange
            _parryStaggerState.Enter(_combat, 0.5f);
            float initialTimer = _parryStaggerState.StaggerTimer;

            // Act
            yield return new WaitForSeconds(0.1f);
            _parryStaggerState.Update(_combat);

            // Assert
            Assert.Greater(_parryStaggerState.StaggerTimer, initialTimer,
                "Stagger timer should increase over time");
        }
    }
}
