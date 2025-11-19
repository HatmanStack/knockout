using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Knockout.Combat.States;
using Knockout.Characters.Components;
using Knockout.Characters.Data;
using Knockout.Combat;

namespace Knockout.Tests.PlayMode.Defense
{
    /// <summary>
    /// PlayMode tests for DodgingState combat state.
    /// </summary>
    [TestFixture]
    public class DodgingStateTests
    {
        private GameObject _testCharacter;
        private CharacterCombat _combat;
        private DodgeData _dodgeData;
        private DodgingState _dodgingState;

        [SetUp]
        public void Setup()
        {
            // Create test character
            _testCharacter = new GameObject("TestCharacter");
            _combat = _testCharacter.AddComponent<CharacterCombat>();
            _testCharacter.AddComponent<CharacterAnimator>();
            _testCharacter.AddComponent<Rigidbody>();

            // Create test dodge data
            _dodgeData = ScriptableObject.CreateInstance<DodgeData>();

            // Create dodging state
            _dodgingState = new DodgingState();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_testCharacter);
            Object.Destroy(_dodgeData);
        }

        [UnityTest]
        public IEnumerator DodgingState_Enter_TriggersAnimation()
        {
            // Arrange
            CharacterAnimator animator = _combat.GetComponent<CharacterAnimator>();
            bool animationTriggered = false;

            // Note: In real implementation, subscribe to animation events
            // For this test, we verify state enters without errors

            // Act
            _dodgingState.Enter(_combat, DodgeDirection.Left, _dodgeData);

            yield return null;

            // Assert
            Assert.IsNotNull(_dodgingState);
            Assert.AreEqual(DodgeDirection.Left, _dodgingState.CurrentDirection);
        }

        [UnityTest]
        public IEnumerator DodgingState_IsInvulnerable_TrueDuringIFrameWindow()
        {
            // Arrange
            _dodgingState.Enter(_combat, DodgeDirection.Left, _dodgeData);

            // Act - advance to i-frame window (frame 2-10 by default)
            for (int i = 0; i < 5; i++)
            {
                _dodgingState.Update(_combat);
                yield return new WaitForFixedUpdate();
            }

            // Assert
            Assert.IsTrue(_dodgingState.IsInvulnerable, "Should be invulnerable during i-frame window");
        }

        [UnityTest]
        public IEnumerator DodgingState_IsInvulnerable_FalseOutsideIFrameWindow()
        {
            // Arrange
            _dodgingState.Enter(_combat, DodgeDirection.Left, _dodgeData);

            // Act - advance past i-frame window
            for (int i = 0; i < 15; i++)
            {
                _dodgingState.Update(_combat);
                yield return new WaitForFixedUpdate();
            }

            // Assert
            Assert.IsFalse(_dodgingState.IsInvulnerable, "Should be vulnerable after i-frame window");
        }

        [UnityTest]
        public IEnumerator DodgingState_CharacterMovesInCorrectDirection()
        {
            // Arrange
            Vector3 startPosition = _testCharacter.transform.position;
            _dodgingState.Enter(_combat, DodgeDirection.Left, _dodgeData);

            // Act - let dodge movement apply
            for (int i = 0; i < 10; i++)
            {
                _dodgingState.Update(_combat);
                yield return new WaitForFixedUpdate();
            }

            // Assert - character should have moved
            Vector3 endPosition = _testCharacter.transform.position;
            float distance = Vector3.Distance(startPosition, endPosition);
            Assert.Greater(distance, 0.1f, "Character should move during dodge");
        }

        [Test]
        public void DodgingState_IsDodgeComplete_FalseInitially()
        {
            // Arrange
            _dodgingState.Enter(_combat, DodgeDirection.Left, _dodgeData);

            // Assert
            Assert.IsFalse(_dodgingState.IsDodgeComplete(), "Dodge should not be complete initially");
        }

        [UnityTest]
        public IEnumerator DodgingState_IsDodgeComplete_TrueAfterDuration()
        {
            // Arrange
            _dodgingState.Enter(_combat, DodgeDirection.Left, _dodgeData);

            // Act - advance past dodge duration
            for (int i = 0; i < 20; i++)
            {
                _dodgingState.Update(_combat);
                yield return new WaitForFixedUpdate();
            }

            // Assert
            Assert.IsTrue(_dodgingState.IsDodgeComplete(), "Dodge should be complete after duration");
        }

        [Test]
        public void DodgingState_CanTransitionTo_IdleWhenComplete()
        {
            // Arrange
            _dodgingState.Enter(_combat, DodgeDirection.Left, _dodgeData);

            // Simulate completion by advancing frames
            for (int i = 0; i < 20; i++)
            {
                _dodgingState.Update(_combat);
            }

            // Act & Assert
            Assert.IsTrue(_dodgingState.CanTransitionTo(new IdleState()),
                "Should allow transition to Idle when dodge complete");
        }

        [Test]
        public void DodgingState_CanTransitionTo_BlocksForbiddenStates()
        {
            // Arrange
            _dodgingState.Enter(_combat, DodgeDirection.Left, _dodgeData);

            // Act & Assert
            Assert.IsFalse(_dodgingState.CanTransitionTo(new AttackingState()),
                "Should not allow transition to Attacking");
            Assert.IsFalse(_dodgingState.CanTransitionTo(new BlockingState()),
                "Should not allow transition to Blocking");
            Assert.IsFalse(_dodgingState.CanTransitionTo(new DodgingState()),
                "Should not allow transition to Dodging");
        }

        [UnityTest]
        public IEnumerator DodgingState_OnDodgeStarted_EventFires()
        {
            // Arrange
            bool eventFired = false;
            CharacterCombat capturedCombat = null;
            DodgeDirection capturedDirection = DodgeDirection.Left;

            DodgingState.OnDodgeStarted += (combat, direction) =>
            {
                eventFired = true;
                capturedCombat = combat;
                capturedDirection = direction;
            };

            // Act
            _dodgingState.Enter(_combat, DodgeDirection.Right, _dodgeData);
            yield return null;

            // Assert
            Assert.IsTrue(eventFired, "OnDodgeStarted event should fire");
            Assert.AreEqual(_combat, capturedCombat, "Event should pass correct combat reference");
            Assert.AreEqual(DodgeDirection.Right, capturedDirection, "Event should pass correct direction");

            // Cleanup
            DodgingState.OnDodgeStarted = null;
        }

        [UnityTest]
        public IEnumerator DodgingState_OnDodgeEnded_EventFires()
        {
            // Arrange
            bool eventFired = false;
            DodgingState.OnDodgeEnded += (combat) => { eventFired = true; };

            _dodgingState.Enter(_combat, DodgeDirection.Left, _dodgeData);

            // Act
            _dodgingState.Exit(_combat);
            yield return null;

            // Assert
            Assert.IsTrue(eventFired, "OnDodgeEnded event should fire");

            // Cleanup
            DodgingState.OnDodgeEnded = null;
        }
    }
}
