using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Knockout.Characters.Components;
using Knockout.Characters.Data;
using Knockout.Combat;
using Knockout.Combat.States;

namespace Knockout.Tests.PlayMode.Defense
{
    /// <summary>
    /// PlayMode tests for CharacterDodge component.
    /// </summary>
    [TestFixture]
    public class CharacterDodgeTests
    {
        private GameObject _testCharacter;
        private CharacterDodge _characterDodge;
        private DodgeData _dodgeData;
        private CombatStateMachine _stateMachine;
        private CharacterCombat _combat;

        [SetUp]
        public void Setup()
        {
            // Create test character
            _testCharacter = new GameObject("TestCharacter");

            // Add required components
            _combat = _testCharacter.AddComponent<CharacterCombat>();
            _characterDodge = _testCharacter.AddComponent<CharacterDodge>();
            _testCharacter.AddComponent<CharacterAnimator>();
            _testCharacter.AddComponent<CharacterInput>();
            _testCharacter.AddComponent<Rigidbody>();

            // Create test dodge data
            _dodgeData = ScriptableObject.CreateInstance<DodgeData>();

            // Set up dodge component
            var dodgeDataField = typeof(CharacterDodge).GetField("dodgeData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            dodgeDataField.SetValue(_characterDodge, _dodgeData);

            // Create and initialize state machine
            _stateMachine = new CombatStateMachine();
            _stateMachine.Initialize(_combat, new IdleState());

            var stateMachineField = typeof(CharacterDodge).GetField("combatStateMachine",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            stateMachineField.SetValue(_characterDodge, _stateMachine);

            // Initialize dodge
            _characterDodge.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_testCharacter);
            Object.Destroy(_dodgeData);
        }

        [Test]
        public void CharacterDodge_TryDodge_SucceedsWhenAvailable()
        {
            // Act
            bool result = _characterDodge.TryDodge(DodgeDirection.Left);

            // Assert
            Assert.IsTrue(result, "TryDodge should succeed when available");
            Assert.IsTrue(_characterDodge.IsDodging, "Should be in dodging state");
        }

        [UnityTest]
        public IEnumerator CharacterDodge_TryDodge_FailsWhenOnCooldown()
        {
            // Arrange - perform first dodge
            _characterDodge.TryDodge(DodgeDirection.Left);
            yield return null;

            // Act - try to dodge again immediately
            bool result = _characterDodge.TryDodge(DodgeDirection.Right);

            // Assert
            Assert.IsFalse(result, "TryDodge should fail when on cooldown");
        }

        [UnityTest]
        public IEnumerator CharacterDodge_TryDodge_FailsWhenInInvalidState()
        {
            // Arrange - transition to attacking state
            _stateMachine.ChangeState(new AttackingState());
            yield return null;

            // Act
            bool result = _characterDodge.TryDodge(DodgeDirection.Left);

            // Assert
            Assert.IsFalse(result, "TryDodge should fail when in invalid state (attacking)");
        }

        [UnityTest]
        public IEnumerator CharacterDodge_Cooldown_ExpiresAfterFrames()
        {
            // Arrange - perform first dodge
            _characterDodge.TryDodge(DodgeDirection.Left);

            // Wait for dodge to complete and return to idle
            yield return new WaitForSeconds(0.5f);
            _stateMachine.ChangeState(new IdleState());

            // Wait for cooldown (simulate FixedUpdate frames)
            for (int i = 0; i < 15; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            // Act - try to dodge again
            bool result = _characterDodge.TryDodge(DodgeDirection.Right);

            // Assert
            Assert.IsTrue(result, "TryDodge should succeed after cooldown expires");
        }

        [UnityTest]
        public IEnumerator CharacterDodge_OnDodgeReady_EventFiresWhenCooldownCompletes()
        {
            // Arrange
            bool eventFired = false;
            _characterDodge.OnDodgeReady += () => { eventFired = true; };

            // Perform dodge to start cooldown
            _characterDodge.TryDodge(DodgeDirection.Left);

            // Wait for cooldown (simulate FixedUpdate frames)
            for (int i = 0; i < 15; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            // Assert
            Assert.IsTrue(eventFired, "OnDodgeReady event should fire when cooldown completes");
        }

        [Test]
        public void CharacterDodge_CanDodge_TrueWhenInIdleState()
        {
            // Arrange - ensure in idle state
            _stateMachine.ChangeState(new IdleState());

            // Assert
            Assert.IsTrue(_characterDodge.CanDodge, "CanDodge should be true in idle state");
        }

        [Test]
        public void CharacterDodge_CanDodge_TrueWhenInBlockingState()
        {
            // Arrange - transition to blocking state
            _stateMachine.ChangeState(new BlockingState());

            // Assert
            Assert.IsTrue(_characterDodge.CanDodge, "CanDodge should be true in blocking state");
        }

        [Test]
        public void CharacterDodge_CanDodge_TrueWhenInExhaustedState()
        {
            // Arrange - transition to exhausted state
            _stateMachine.ChangeState(new ExhaustedState());

            // Assert
            Assert.IsTrue(_characterDodge.CanDodge, "CanDodge should be true in exhausted state (defensive action)");
        }

        [Test]
        public void CharacterDodge_CooldownProgress_ReturnsCorrectValue()
        {
            // Arrange - perform dodge
            _characterDodge.TryDodge(DodgeDirection.Left);

            // Assert
            float progress = _characterDodge.CooldownProgress;
            Assert.Greater(progress, 0f, "Cooldown progress should be > 0 immediately after dodge");
            Assert.LessOrEqual(progress, 1f, "Cooldown progress should be <= 1");
        }

        [Test]
        public void CharacterDodge_IsDodging_TrueWhenInDodgingState()
        {
            // Act
            _characterDodge.TryDodge(DodgeDirection.Left);

            // Assert
            Assert.IsTrue(_characterDodge.IsDodging, "IsDodging should be true when in dodging state");
        }

        [Test]
        public void CharacterDodge_IsDodging_FalseWhenInIdleState()
        {
            // Arrange - ensure in idle state
            _stateMachine.ChangeState(new IdleState());

            // Assert
            Assert.IsFalse(_characterDodge.IsDodging, "IsDodging should be false when in idle state");
        }
    }
}
