using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Knockout.Characters.Components;
using Knockout.Characters.Data;
using Knockout.Combat;
using Knockout.Combat.States;
using Knockout.Combat.HitDetection;

namespace Knockout.Tests.PlayMode.Defense
{
    /// <summary>
    /// PlayMode tests for CharacterParry component.
    /// </summary>
    [TestFixture]
    public class CharacterParryTests
    {
        private GameObject _testCharacter;
        private GameObject _attackerCharacter;
        private CharacterParry _characterParry;
        private ParryData _parryData;
        private CombatStateMachine _stateMachine;
        private CharacterCombat _combat;
        private CharacterCombat _attackerCombat;

        [SetUp]
        public void Setup()
        {
            // Create test character (defender)
            _testCharacter = new GameObject("TestCharacter");
            _combat = _testCharacter.AddComponent<CharacterCombat>();
            _characterParry = _testCharacter.AddComponent<CharacterParry>();
            _testCharacter.AddComponent<CharacterAnimator>();
            _testCharacter.AddComponent<CharacterInput>();

            // Create attacker character
            _attackerCharacter = new GameObject("Attacker");
            _attackerCombat = _attackerCharacter.AddComponent<CharacterCombat>();
            _attackerCharacter.AddComponent<CharacterAnimator>();

            // Create test parry data
            _parryData = ScriptableObject.CreateInstance<ParryData>();

            // Set up parry component
            var parryDataField = typeof(CharacterParry).GetField("parryData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            parryDataField.SetValue(_characterParry, _parryData);

            // Create and initialize state machine
            _stateMachine = new CombatStateMachine();
            _stateMachine.Initialize(_combat, new IdleState());

            var stateMachineField = typeof(CharacterParry).GetField("combatStateMachine",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            stateMachineField.SetValue(_characterParry, _stateMachine);

            // Initialize parry
            _characterParry.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_testCharacter);
            Object.Destroy(_attackerCharacter);
            Object.Destroy(_parryData);
        }

        [UnityTest]
        public IEnumerator CharacterParry_TryParry_SucceedsWithinParryWindow()
        {
            // Arrange - simulate block press
            SimulateBlockPress();

            // Wait a few frames (within parry window)
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            var hitData = CreateTestHitData();

            // Act
            bool result = _characterParry.TryParry(hitData, _attackerCombat);

            // Assert
            Assert.IsTrue(result, "Parry should succeed when block pressed within window");
        }

        [UnityTest]
        public IEnumerator CharacterParry_TryParry_FailsOutsideParryWindow()
        {
            // Arrange - simulate block press and wait too long
            SimulateBlockPress();

            // Wait past parry window (default 6 frames)
            for (int i = 0; i < 10; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            var hitData = CreateTestHitData();

            // Act
            bool result = _characterParry.TryParry(hitData, _attackerCombat);

            // Assert
            Assert.IsFalse(result, "Parry should fail when block pressed outside window");
        }

        [Test]
        public void CharacterParry_TryParry_FailsWhenOnCooldown()
        {
            // Arrange - simulate successful parry to trigger cooldown
            SimulateBlockPress();
            var hitData = CreateTestHitData();
            _characterParry.TryParry(hitData, _attackerCombat);

            // Act - try to parry again immediately
            bool result = _characterParry.TryParry(hitData, _attackerCombat);

            // Assert
            Assert.IsFalse(result, "Parry should fail when on cooldown");
        }

        [UnityTest]
        public IEnumerator CharacterParry_OnParrySuccess_EventFires()
        {
            // Arrange
            bool eventFired = false;
            CharacterCombat capturedAttacker = null;

            _characterParry.OnParrySuccess += (attacker) =>
            {
                eventFired = true;
                capturedAttacker = attacker;
            };

            SimulateBlockPress();
            yield return new WaitForFixedUpdate();

            var hitData = CreateTestHitData();

            // Act
            _characterParry.TryParry(hitData, _attackerCombat);
            yield return null;

            // Assert
            Assert.IsTrue(eventFired, "OnParrySuccess event should fire");
            Assert.AreEqual(_attackerCombat, capturedAttacker, "Event should pass attacker");
        }

        [UnityTest]
        public IEnumerator CharacterParry_CounterWindow_OpensOnParrySuccess()
        {
            // Arrange
            SimulateBlockPress();
            yield return new WaitForFixedUpdate();

            var hitData = CreateTestHitData();

            // Act
            _characterParry.TryParry(hitData, _attackerCombat);
            yield return null;

            // Assert
            Assert.IsTrue(_characterParry.IsInCounterWindow, "Counter window should open after parry");
        }

        [UnityTest]
        public IEnumerator CharacterParry_CounterWindow_ClosesAfterDuration()
        {
            // Arrange
            SimulateBlockPress();
            yield return new WaitForFixedUpdate();

            var hitData = CreateTestHitData();
            _characterParry.TryParry(hitData, _attackerCombat);

            // Act - wait for counter window to expire
            yield return new WaitForSeconds(0.6f); // Default counter window is 0.5s

            // Assert
            Assert.IsFalse(_characterParry.IsInCounterWindow,
                "Counter window should close after duration");
        }

        [Test]
        public void CharacterParry_CanParry_TrueWhenInIdleState()
        {
            // Arrange - ensure in idle state
            _stateMachine.ChangeState(new IdleState());

            // Assert
            Assert.IsTrue(_characterParry.CanParry, "CanParry should be true in idle state");
        }

        [Test]
        public void CharacterParry_CanParry_TrueWhenInBlockingState()
        {
            // Arrange
            _stateMachine.ChangeState(new BlockingState());

            // Assert
            Assert.IsTrue(_characterParry.CanParry, "CanParry should be true in blocking state");
        }

        [Test]
        public void CharacterParry_CanParry_TrueWhenInExhaustedState()
        {
            // Arrange
            _stateMachine.ChangeState(new ExhaustedState());

            // Assert
            Assert.IsTrue(_characterParry.CanParry,
                "CanParry should be true in exhausted state (defensive action)");
        }

        [Test]
        public void CharacterParry_CooldownProgress_ReturnsCorrectValue()
        {
            // Arrange - trigger parry to start cooldown
            SimulateBlockPress();
            var hitData = CreateTestHitData();
            _characterParry.TryParry(hitData, _attackerCombat);

            // Assert
            float progress = _characterParry.CooldownProgress;
            Assert.Greater(progress, 0f, "Cooldown progress should be > 0 after parry");
            Assert.LessOrEqual(progress, 1f, "Cooldown progress should be <= 1");
        }

        [UnityTest]
        public IEnumerator CharacterParry_OnParryReady_EventFiresWhenCooldownCompletes()
        {
            // Arrange
            bool eventFired = false;
            _characterParry.OnParryReady += () => { eventFired = true; };

            // Trigger parry to start cooldown
            SimulateBlockPress();
            var hitData = CreateTestHitData();
            _characterParry.TryParry(hitData, _attackerCombat);

            // Wait for cooldown (simulate FixedUpdate frames)
            for (int i = 0; i < 25; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            // Assert
            Assert.IsTrue(eventFired, "OnParryReady event should fire when cooldown completes");
        }

        private void SimulateBlockPress()
        {
            // Simulate block input by invoking the event
            var inputField = typeof(CharacterParry).GetField("characterInput",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var characterInput = inputField.GetValue(_characterParry) as CharacterInput;

            if (characterInput != null)
            {
                // Trigger OnBlockPressed event
                var eventInfo = typeof(CharacterInput).GetEvent("OnBlockPressed");
                var eventField = typeof(CharacterInput).GetField("OnBlockPressed",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                if (eventField != null)
                {
                    var eventDelegate = eventField.GetValue(characterInput) as System.Action;
                    eventDelegate?.Invoke();
                }
            }

            // Also manually call the private method
            var method = typeof(CharacterParry).GetMethod("OnBlockPressedInput",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(_characterParry, null);
        }

        private HitData CreateTestHitData()
        {
            return new HitData(
                attacker: _attackerCharacter,
                damage: 20f,
                knockback: 0f,
                hitPoint: Vector3.zero,
                hitDirection: Vector3.forward,
                hitType: 0,
                attackName: "TestAttack"
            );
        }
    }
}
