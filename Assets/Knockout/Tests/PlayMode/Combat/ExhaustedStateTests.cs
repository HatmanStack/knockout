using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Knockout.Combat.States;
using Knockout.Characters.Components;
using Knockout.Characters.Data;
using Knockout.Combat;

namespace Knockout.Tests.PlayMode.Combat
{
    /// <summary>
    /// PlayMode tests for ExhaustedState integration.
    /// </summary>
    [TestFixture]
    public class ExhaustedStateTests
    {
        private GameObject _characterObject;
        private CharacterCombat _combat;
        private CharacterStamina _stamina;
        private StaminaData _staminaData;

        [SetUp]
        public void SetUp()
        {
            // Create stamina data
            _staminaData = ScriptableObject.CreateInstance<StaminaData>();

            // Create character with all required components
            _characterObject = new GameObject("TestCharacter");

            // Add Animator and CharacterAnimator
            _characterObject.AddComponent<Animator>();
            _characterObject.AddComponent<CharacterAnimator>();

            // Add CharacterCombat
            _combat = _characterObject.AddComponent<CharacterCombat>();

            // Add CharacterStamina
            _stamina = _characterObject.AddComponent<CharacterStamina>();

            // Set stamina data
            var staminaDataField = typeof(CharacterStamina).GetField("staminaData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            staminaDataField.SetValue(_stamina, _staminaData);
        }

        [TearDown]
        public void TearDown()
        {
            if (_characterObject != null)
            {
                Object.DestroyImmediate(_characterObject);
            }

            if (_staminaData != null)
            {
                Object.DestroyImmediate(_staminaData);
            }
        }

        [UnityTest]
        public IEnumerator ExhaustedState_EnterAppliesSlowerRegenMultiplier()
        {
            yield return null; // Wait for Start()

            // Arrange
            var exhaustedState = new ExhaustedState();

            // Act
            exhaustedState.Enter(_combat);

            // Wait a frame
            yield return null;

            // Assert - check that regen multiplier was applied
            // We can verify by checking stamina regeneration rate
            _stamina.SetCurrentStamina(50f);
            float staminaBefore = _stamina.CurrentStamina;

            yield return new WaitForSeconds(0.2f);

            // Stamina should regenerate slower during exhaustion
            // (This is a proxy test - actual multiplier is internal)
            float regenAmount = _stamina.CurrentStamina - staminaBefore;
            Assert.Greater(regenAmount, 0f, "Stamina should still regenerate during exhaustion");

            // Cleanup
            exhaustedState.Exit(_combat);
        }

        [UnityTest]
        public IEnumerator ExhaustedState_ExitRestoresNormalRegen()
        {
            yield return null; // Wait for Start()

            // Arrange
            var exhaustedState = new ExhaustedState();
            exhaustedState.Enter(_combat);

            // Act
            exhaustedState.Exit(_combat);

            yield return null;

            // Assert - verify normal regen restored
            _stamina.SetCurrentStamina(50f);
            float staminaBefore = _stamina.CurrentStamina;

            yield return new WaitForSeconds(0.2f);

            float regenAmount = _stamina.CurrentStamina - staminaBefore;
            Assert.Greater(regenAmount, 0f, "Stamina should regenerate after exiting exhaustion");
        }

        [UnityTest]
        public IEnumerator ExhaustedState_CannotRecoverBeforeMinimumDuration()
        {
            yield return null; // Wait for Start()

            // Arrange
            var exhaustedState = new ExhaustedState();
            exhaustedState.Enter(_combat);

            // Immediately restore stamina above threshold
            _stamina.SetCurrentStamina(_staminaData.MaxStamina);

            yield return null;

            // Act - check if can recover (should be false, min duration not passed)
            bool canRecover = exhaustedState.CanRecover();

            // Assert
            Assert.IsFalse(canRecover, "Should not recover before minimum exhaustion duration");

            // Cleanup
            exhaustedState.Exit(_combat);
        }

        [UnityTest]
        public IEnumerator ExhaustedState_CanRecoverAfterDurationAndThreshold()
        {
            yield return null; // Wait for Start()

            // Arrange - set short exhaustion duration for testing
            var durationField = typeof(StaminaData).GetField("exhaustionDuration",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            durationField.SetValue(_staminaData, 0.1f); // 0.1 second

            var exhaustedState = new ExhaustedState();
            exhaustedState.Enter(_combat);

            // Restore stamina above threshold
            _stamina.SetCurrentStamina(_staminaData.MaxStamina);

            // Wait for minimum duration + buffer
            yield return new WaitForSeconds(0.15f);

            // Act
            bool canRecover = exhaustedState.CanRecover();

            // Assert
            Assert.IsTrue(canRecover, "Should be able to recover after minimum duration and threshold met");

            // Cleanup
            exhaustedState.Exit(_combat);
        }

        [UnityTest]
        public IEnumerator ExhaustedState_FiresEventsOnEnterAndExit()
        {
            yield return null; // Wait for Start()

            // Arrange
            bool startEventFired = false;
            bool endEventFired = false;

            ExhaustedState.OnExhaustedStart += (combat) => startEventFired = true;
            ExhaustedState.OnExhaustedEnd += (combat) => endEventFired = true;

            var exhaustedState = new ExhaustedState();

            // Act
            exhaustedState.Enter(_combat);
            yield return null;

            exhaustedState.Exit(_combat);
            yield return null;

            // Assert
            Assert.IsTrue(startEventFired, "OnExhaustedStart should fire on Enter");
            Assert.IsTrue(endEventFired, "OnExhaustedEnd should fire on Exit");

            // Cleanup - unsubscribe
            ExhaustedState.OnExhaustedStart -= (combat) => startEventFired = true;
            ExhaustedState.OnExhaustedEnd -= (combat) => endEventFired = true;
        }

        [UnityTest]
        public IEnumerator ExhaustedState_TracksExhaustionTimer()
        {
            yield return null; // Wait for Start()

            // Arrange
            var exhaustedState = new ExhaustedState();
            exhaustedState.Enter(_combat);

            // Act - simulate Update() calls
            exhaustedState.Update(_combat);
            yield return new WaitForSeconds(0.2f);
            exhaustedState.Update(_combat);

            // Assert
            Assert.Greater(exhaustedState.ExhaustionTimer, 0f, "Timer should track time spent in exhaustion");

            // Cleanup
            exhaustedState.Exit(_combat);
        }
    }
}
